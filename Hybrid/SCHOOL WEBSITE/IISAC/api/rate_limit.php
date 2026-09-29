<?php

function iisac_client_ip()
{
  $xff = $_SERVER['HTTP_X_FORWARDED_FOR'] ?? '';
  if ($xff) {
    $parts = explode(',', $xff);
    return trim($parts[0]);
  }
  return $_SERVER['REMOTE_ADDR'] ?? '0.0.0.0';
}

function iisac_rate_limit($bucket, $limit = 60, $windowSec = 60)
{
  $dir = sys_get_temp_dir() . '/iisac-rate';
  if (!is_dir($dir)) {
    @mkdir($dir, 0700, true);
  }
  $ip = preg_replace('/[^a-zA-Z0-9_.:-]/', '_', iisac_client_ip());
  $file = $dir . '/' . preg_replace('/[^a-zA-Z0-9_-]/', '_', $bucket) . '_' . $ip . '.json';
  $now = time();
  $data = ['start' => $now, 'count' => 0];
  if (is_file($file)) {
    $raw = json_decode((string) @file_get_contents($file), true);
    if (is_array($raw) && isset($raw['start'], $raw['count']) && ($now - (int) $raw['start']) < $windowSec) {
      $data = $raw;
    } else {
      $data = ['start' => $now, 'count' => 0];
    }
  }
  $data['count'] = (int) $data['count'] + 1;
  @file_put_contents($file, json_encode($data), LOCK_EX);
  if ($data['count'] > $limit) {
    http_response_code(429);
    header('Content-Type: application/json; charset=utf-8');
    header('Retry-After: ' . $windowSec);
    echo json_encode(['error' => 'Too many requests. Please try again later.']);
    exit;
  }
}

function iisac_edu_log($schoolId, $path, $status)
{
  $dir = sys_get_temp_dir() . '/iisac-logs';
  if (!is_dir($dir)) {
    @mkdir($dir, 0700, true);
  }
  $line = json_encode([
    'ts' => gmdate('c'),
    'ip' => iisac_client_ip(),
    'school_id' => (int) $schoolId,
    'path' => (string) $path,
    'status' => (int) $status,
  ], JSON_UNESCAPED_SLASHES);
  @file_put_contents($dir . '/edu-' . gmdate('Y-m-d') . '.log', $line . "
", FILE_APPEND | LOCK_EX);
}
