<?php

function cloudinary_configured(array $config = [])
{
  $name = $config['cloudinary_cloud_name'] ?? getenv('CLOUDINARY_CLOUD_NAME') ?: '';
  $key = $config['cloudinary_api_key'] ?? getenv('CLOUDINARY_API_KEY') ?: '';
  $secret = $config['cloudinary_api_secret'] ?? getenv('CLOUDINARY_API_SECRET') ?: '';
  return $name !== '' && $key !== '' && $secret !== '';
}

function cloudinary_settings(array $config = [])
{
  return [
    'cloud_name' => (string) ($config['cloudinary_cloud_name'] ?? getenv('CLOUDINARY_CLOUD_NAME') ?: ''),
    'api_key' => (string) ($config['cloudinary_api_key'] ?? getenv('CLOUDINARY_API_KEY') ?: ''),
    'api_secret' => (string) ($config['cloudinary_api_secret'] ?? getenv('CLOUDINARY_API_SECRET') ?: ''),
    'folder' => rtrim((string) ($config['cloudinary_folder'] ?? getenv('CLOUDINARY_FOLDER') ?: 'iisac'), '/'),
  ];
}

/**
 * Upload a local temp file to Cloudinary. Returns secure_url or null on failure.
 */
function cloudinary_upload_file($tmpPath, $originalName, $folderSuffix, array $config = [])
{
  if (!cloudinary_configured($config) || !is_file($tmpPath)) {
    return null;
  }

  $settings = cloudinary_settings($config);
  $folder = $settings['folder'] . '/' . trim((string) $folderSuffix, '/');
  $timestamp = time();
  $publicId = pathinfo($originalName, PATHINFO_FILENAME);
  $publicId = preg_replace('/[^a-zA-Z0-9_\-]/', '_', (string) $publicId);
  if ($publicId === '') {
    $publicId = 'file_' . $timestamp;
  }
  $publicId = $publicId . '_' . substr(uniqid('', true), -8);

  $ext = strtolower(pathinfo($originalName, PATHINFO_EXTENSION));
  $resourceType = in_array($ext, ['pdf', 'doc', 'docx'], true) ? 'raw' : 'image';

  $paramsToSign = [
    'folder' => $folder,
    'public_id' => $publicId,
    'timestamp' => $timestamp,
  ];
  ksort($paramsToSign);
  $toSign = [];
  foreach ($paramsToSign as $k => $v) {
    $toSign[] = $k . '=' . $v;
  }
  $signature = sha1(implode('&', $toSign) . $settings['api_secret']);

  $endpoint = 'https://api.cloudinary.com/v1_1/' . rawurlencode($settings['cloud_name']) . '/' . $resourceType . '/upload';

  $post = [
    'file' => new CURLFile($tmpPath, mime_content_type($tmpPath) ?: null, $originalName),
    'api_key' => $settings['api_key'],
    'timestamp' => $timestamp,
    'folder' => $folder,
    'public_id' => $publicId,
    'signature' => $signature,
  ];

  $ch = curl_init($endpoint);
  curl_setopt_array($ch, [
    CURLOPT_POST => true,
    CURLOPT_POSTFIELDS => $post,
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_TIMEOUT => 60,
  ]);
  $raw = curl_exec($ch);
  $err = curl_error($ch);
  $code = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
  curl_close($ch);

  if ($raw === false || $code >= 400) {
    error_log('Cloudinary upload failed: HTTP ' . $code . ' ' . $err . ' ' . substr((string) $raw, 0, 300));
    return null;
  }

  $json = json_decode($raw, true);
  if (!is_array($json) || empty($json['secure_url'])) {
    error_log('Cloudinary upload bad response: ' . substr((string) $raw, 0, 300));
    return null;
  }

  return (string) $json['secure_url'];
}

function cloudinary_folder_from_url_base($urlBase)
{
  $urlBase = trim((string) $urlBase, '/');
  if (str_starts_with($urlBase, 'uploads/')) {
    $urlBase = substr($urlBase, strlen('uploads/'));
  }
  return $urlBase !== '' ? $urlBase : 'projects';
}