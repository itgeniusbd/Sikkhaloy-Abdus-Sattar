<?php
header('Content-Type: application/json; charset=utf-8');

if (!is_file(dirname(__DIR__) . '/install.lock')) {
  http_response_code(503);
  echo json_encode(['error' => 'Open /install/ first.']);
  exit;
}

$config = require __DIR__ . '/config.php';

try {
  $port = $config['db_port'] ?? '3306';
  $pdo = new PDO(
    "mysql:host={$config['db_host']};port={$port};dbname={$config['db_name']};charset=utf8mb4",
    $config['db_user'],
    $config['db_pass'],
    [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
    PDO::MYSQL_ATTR_INIT_COMMAND => 'SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci',
  ]
  );
} catch (Throwable $e) {
  http_response_code(500);
  echo json_encode(['error' => 'MySQL connection failed.']);
  exit;
}

$users = (int) $pdo->query('SELECT COUNT(*) FROM users')->fetchColumn();
echo json_encode([
  'ok' => true,
  'message' => $users > 0
    ? 'Admin already exists. Use the username and password you set in the installer.'
    : 'No admin user found. Delete install.lock and api/config.json, then open /install/.',
]);
