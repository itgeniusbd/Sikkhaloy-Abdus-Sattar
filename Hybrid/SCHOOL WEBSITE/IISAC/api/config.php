<?php
$defaults = [
  'db_host' => getenv('DB_HOST') ?: 'localhost',
  'db_port' => getenv('DB_PORT') ?: '3306',
  'db_name' => getenv('DB_NAME') ?: 'iisac_website',
  'db_user' => getenv('DB_USER') ?: 'root',
  'db_pass' => getenv('DB_PASS') ?: '',
  'school_id' => (int) (getenv('SCHOOL_ID') ?: 0),
  'sikkhaloy_api' => getenv('SIKKHALOY_API') ?: 'https://hybridapi.sikkhaloy.com',
  'sikkhaloy_website_key' => getenv('SIKKHALOY_WEBSITE_KEY') ?: '',
  'sikkhaloy_allow_hosts' => getenv('SIKKHALOY_ALLOW_HOSTS') ?: '',
  'cloudinary_cloud_name' => getenv('CLOUDINARY_CLOUD_NAME') ?: '',
  'cloudinary_api_key' => getenv('CLOUDINARY_API_KEY') ?: '',
  'cloudinary_api_secret' => getenv('CLOUDINARY_API_SECRET') ?: '',
  'cloudinary_folder' => getenv('CLOUDINARY_FOLDER') ?: 'iisac',
  'upload_dir' => dirname(__DIR__) . '/uploads/projects',
  'upload_url' => '/uploads/projects',
];

function iisac_load_config_file($path, $defaults)
{
  if (!$path || !is_file($path)) {
    return null;
  }
  $json = json_decode((string) file_get_contents($path), true);
  if (!is_array($json) || empty($json['db_name'])) {
    return null;
  }
  $merged = array_merge($defaults, $json, [
    'upload_dir' => $defaults['upload_dir'],
    'upload_url' => $defaults['upload_url'],
  ]);
  $merged['school_id'] = (int) ($merged['school_id'] ?? 1);
  $merged['sikkhaloy_api'] = rtrim((string) ($merged['sikkhaloy_api'] ?? $defaults['sikkhaloy_api']), '/');
  $merged['sikkhaloy_website_key'] = (string) ($merged['sikkhaloy_website_key'] ?? $defaults['sikkhaloy_website_key']);
  $merged['sikkhaloy_allow_hosts'] = (string) ($merged['sikkhaloy_allow_hosts'] ?? $defaults['sikkhaloy_allow_hosts']);
  return $merged;
}

$candidates = [];
$envPath = getenv('IISAC_CONFIG_PATH') ?: '';
if ($envPath !== '') {
  $candidates[] = $envPath;
}
// Preferred: one level above the web root (outside public_html)
$candidates[] = dirname(__DIR__) . '/../iisac-config.json';
$candidates[] = dirname(__DIR__) . '/iisac-config.json';
$candidates[] = __DIR__ . '/config.json';

foreach ($candidates as $candidate) {
  $loaded = iisac_load_config_file($candidate, $defaults);
  if ($loaded) {
    return $loaded;
  }
}

$local = __DIR__ . '/config.local.php';
if (is_file($local)) {
  $loaded = require $local;
  if (is_array($loaded)) {
    return array_merge($defaults, $loaded);
  }
}

return $defaults;
