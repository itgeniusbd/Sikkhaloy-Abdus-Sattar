<?php
$defaults = [
  'db_host' => getenv('DB_HOST') ?: 'localhost',
  'db_port' => getenv('DB_PORT') ?: '3306',
  'db_name' => getenv('DB_NAME') ?: 'safa_propertis',
  'db_user' => getenv('DB_USER') ?: 'root',
  'db_pass' => getenv('DB_PASS') ?: '',
  'upload_dir' => dirname(__DIR__) . '/uploads/projects',
  'upload_url' => '/uploads/projects',
];

$jsonFile = __DIR__ . '/config.json';
if (is_file($jsonFile)) {
  $json = json_decode(file_get_contents($jsonFile), true);
  if (is_array($json) && !empty($json['db_name'])) {
    return array_merge($defaults, $json, [
      'upload_dir' => $defaults['upload_dir'],
      'upload_url' => $defaults['upload_url'],
    ]);
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
