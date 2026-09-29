<?php
require __DIR__ . '/api/path.php';

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
  $inject = '<script>window.SAFA_BASE=' . json_encode($base) . ';</script>';
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
