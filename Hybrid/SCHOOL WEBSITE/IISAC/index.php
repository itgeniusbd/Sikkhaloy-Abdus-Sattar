<?php
$pathHelper = __DIR__ . '/api/path.php';
if (!is_file($pathHelper)) {
  http_response_code(500);
  header('Content-Type: text/html; charset=utf-8');
  echo '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Upload incomplete</title></head><body style="font-family:sans-serif;max-width:640px;margin:40px auto;line-height:1.5">';
  echo '<h1>Upload incomplete</h1>';
  echo '<p>Missing file: <code>api/path.php</code>.</p>';
  echo '<p>Upload the <strong>entire</strong> contents of the <code>dist</code> folder (or the hostinger zip) into <code>public_html</code>, including the <code>api</code> folder — not only <code>index.php</code>.</p>';
  echo '</body></html>';
  exit;
}
require $pathHelper;

$root = __DIR__;
$lock = $root . '/install.lock';
$config = $root . '/api/config.json';

if (!is_file($lock) || !is_file($config)) {
  $base = safa_web_base();
  header('Location: ' . ($base === '' ? '/install/' : $base . '/install/'));
  exit;
}

$htmlFile = is_file($root . '/app.html') ? $root . '/app.html' : $root . '/index.html';
if (is_file($htmlFile)) {
  $html = file_get_contents($htmlFile);
  $base = safa_web_base();
  $inject = '<script>window.SITE_BASE=' . json_encode($base) . ';window.SAFA_BASE=' . json_encode($base) . ';</script>';
  $html = preg_replace('#<head[^>]*>#i', '$0' . $inject, $html, 1);
  if ($base !== '') {
    $html = str_replace('="/assets/', '="' . $base . '/assets/', $html);
    $html = str_replace("='/assets/", "='" . $base . '/assets/', $html);
    $html = str_replace('="/logo.svg"', '="' . $base . '/logo.svg"', $html);
    $html = str_replace('href="/logo.svg"', 'href="' . $base . '/logo.svg"', $html);
    $html = str_replace('src="./assets/', 'src="' . $base . '/assets/', $html);
    $html = str_replace('href="./assets/', 'href="' . $base . '/assets/', $html);
  }
  header('Content-Type: text/html; charset=utf-8');
  echo $html;
  exit;
}

http_response_code(500);
header('Content-Type: text/plain; charset=utf-8');
echo "Website files are missing. On your computer run npm run build, then upload everything inside the dist folder.";
