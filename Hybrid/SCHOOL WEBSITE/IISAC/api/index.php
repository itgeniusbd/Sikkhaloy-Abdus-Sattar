<?php
require_once __DIR__ . '/cloudinary.php';
require_once __DIR__ . '/password_policy.php';
require_once __DIR__ . '/rate_limit.php';
require_once __DIR__ . '/edu.php';
require __DIR__ . '/path.php';

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Headers: Authorization, Content-Type, X-Auth-Token');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS');
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
  http_response_code(204);
  exit;
}

if (!is_file(dirname(__DIR__) . '/install.lock') || !is_file(__DIR__ . '/config.json')) {
  http_response_code(503);
  echo json_encode(['error' => 'Site is not installed. Open /install/']);
  exit;
}

$config = require __DIR__ . '/config.php';
$port = $config['db_port'] ?? '3306';

try {
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
  echo json_encode(['error' => 'MySQL connection failed. Check api/config.json or run /install/']);
  exit;
}

try {
  $pdo->exec('ALTER TABLE settings MODIFY setting_value MEDIUMTEXT NOT NULL');
} catch (Throwable $e) {
  /* already TEXT */
}
try {
  $pdo->exec('ALTER TABLE posts ADD COLUMN created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP');
} catch (Throwable $e) {
  /* column exists */
}
users_ensure($pdo);

try {
  $pdo->exec('CREATE TABLE IF NOT EXISTS subscribers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(160) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci');
} catch (Throwable $e) {
  /* table exists */
}

if (isset($_GET['route']) && is_string($_GET['route'])) {
  $path = trim($_GET['route'], '/');
} else {
  $uri = parse_url($_SERVER['REQUEST_URI'] ?? '/', PHP_URL_PATH) ?: '/';
  if (preg_match('#/api(?:/(.*))?$#', $uri, $m)) {
    $path = trim($m[1] ?? '', '/');
    if ($path === 'index.php') {
      $path = '';
    }
  } else {
    $base = safa_web_base();
    if ($base !== '') {
      $uri = preg_replace('#^' . preg_quote($base, '#') . '#', '', $uri);
    }
    $path = trim(preg_replace('#^/api#', '', $uri), '/');
  }
}
$method = $_SERVER['REQUEST_METHOD'];
$parts = $path === '' ? [] : explode('/', $path);

function json_body() {
  return json_decode(file_get_contents('php://input'), true) ?: [];
}

function send($data, $code = 200) {
  http_response_code($code);
  echo json_encode($data);
  exit;
}

function bearer() {
  $header = $_SERVER['HTTP_AUTHORIZATION']
    ?? $_SERVER['REDIRECT_HTTP_AUTHORIZATION']
    ?? $_SERVER['REDIRECT_REDIRECT_HTTP_AUTHORIZATION']
    ?? '';
  if ($header === '' && function_exists('apache_request_headers')) {
    foreach (apache_request_headers() as $key => $value) {
      if (strcasecmp($key, 'Authorization') === 0) {
        $header = $value;
        break;
      }
    }
  }
  if (preg_match('/Bearer\s+(\S+)/', $header, $m)) {
    return $m[1];
  }
  $alt = $_SERVER['HTTP_X_AUTH_TOKEN'] ?? $_SERVER['REDIRECT_HTTP_X_AUTH_TOKEN'] ?? '';
  return is_string($alt) ? trim($alt) : '';
}

function require_user(PDO $pdo) {
  $token = bearer();
  if (!$token) send(['error' => 'Login required'], 401);
  $stmt = $pdo->prepare('SELECT user_id FROM tokens WHERE token = ? AND expires_at > NOW()');
  $stmt->execute([$token]);
  $row = $stmt->fetch(PDO::FETCH_ASSOC);
  if (!$row) send(['error' => 'Session expired'], 401);
  return (int) $row['user_id'];
}

function users_ensure(PDO $pdo) {
  foreach ([
    "ALTER TABLE users ADD COLUMN role VARCHAR(20) NOT NULL DEFAULT 'admin'",
    "ALTER TABLE users ADD COLUMN display_name VARCHAR(120) NOT NULL DEFAULT ''",
    'ALTER TABLE users ADD COLUMN permissions TEXT',
    'ALTER TABLE users ADD COLUMN active TINYINT NOT NULL DEFAULT 1',
    "ALTER TABLE users ADD COLUMN photo VARCHAR(255) NOT NULL DEFAULT ''",
    "ALTER TABLE users ADD COLUMN designation VARCHAR(160) NOT NULL DEFAULT ''",
  ] as $sql) {
    try { $pdo->exec($sql); } catch (Throwable $e) { /* exists */ }
  }
  $id = $pdo->query('SELECT id FROM users ORDER BY id ASC LIMIT 1')->fetchColumn();
  if ($id) {
    $pdo->prepare("UPDATE users SET role = 'admin', display_name = IF(display_name = '', username, display_name) WHERE id = ?")
      ->execute([$id]);
    $sitePhoto = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'admin_photo'")->fetchColumn();
    if ($sitePhoto) {
      $pdo->prepare("UPDATE users SET photo = ? WHERE id = ? AND (photo IS NULL OR photo = '')")
        ->execute([$sitePhoto, $id]);
    }
  }
}

function access_pages() {
  return ['profile', 'menus', 'slider', 'admissions', 'career', 'footer', 'blog', 'about', 'messages', 'subscribers', 'socials', 'contact'];
}

function parse_permissions($raw, $role) {
  if ($role === 'admin') return access_pages();
  $list = json_decode($raw ?: '[]', true);
  if (!is_array($list)) return [];
  return array_values(array_intersect($list, access_pages()));
}

function public_user($row) {
  if (!$row) return null;
  $role = (($row['role'] ?? '') === 'admin') ? 'admin' : 'subadmin';
  return [
    'id' => (int) $row['id'],
    'username' => $row['username'],
    'displayName' => $row['display_name'] ?: $row['username'],
    'designation' => $row['designation'] ?? '',
    'photo' => public_url($row['photo'] ?? ''),
    'role' => $role,
    'permissions' => parse_permissions($row['permissions'] ?? '[]', $role),
    'active' => ((int) ($row['active'] ?? 1)) !== 0,
  ];
}

function account_of(PDO $pdo, $id) {
  $stmt = $pdo->prepare('SELECT * FROM users WHERE id = ?');
  $stmt->execute([$id]);
  return $stmt->fetch(PDO::FETCH_ASSOC) ?: null;
}

function require_perm(PDO $pdo, ...$perms) {
  $id = require_user($pdo);
  $row = account_of($pdo, $id);
  if (!$row || (int) ($row['active'] ?? 1) === 0) send(['error' => 'Session expired'], 401);
  if (($row['role'] ?? '') === 'admin') return $row;
  $have = parse_permissions($row['permissions'] ?? '[]', 'subadmin');
  foreach ($perms as $perm) {
    if ($perm === 'profile' || in_array($perm, $have, true)) return $row;
  }
  send(['error' => 'No access.'], 403);
}

function require_admin_user(PDO $pdo) {
  $row = require_perm($pdo, 'profile');
  if (($row['role'] ?? '') !== 'admin') send(['error' => 'Only the main admin can manage users.'], 403);
  return $row;
}

function public_url($path)
{
  if (!$path || preg_match('#^https?://#i', $path) || strpos($path, 'data:') === 0) {
    return $path;
  }
  $base = safa_web_base();
  if ($base !== '' && strpos($path, $base . '/') === 0) {
    return $path;
  }
  if ($path[0] !== '/') {
    $path = '/' . $path;
  }
  return $base . $path;
}

function map_project($row) {
  $gallery = json_decode($row['gallery_json'] ?: '[]', true) ?: [];
  return [
    'id' => $row['id'],
    'slug' => $row['slug'],
    'title' => $row['title'],
    'status' => $row['status'],
    'deal' => $row['deal'],
    'badge' => $row['badge'],
    'category' => $row['category'],
    'type' => $row['type'],
    'location' => $row['location'],
    'city' => $row['city'],
    'price' => (float) $row['price'],
    'beds' => (int) $row['beds'],
    'baths' => (int) $row['baths'],
    'rooms' => (int) $row['rooms'],
    'parking' => (int) $row['parking'],
    'size' => (int) $row['size'],
    'image' => public_url($row['image']),
    'gallery' => array_map('public_url', $gallery),
    'description' => $row['description'],
    'features' => json_decode($row['features_json'] ?: '[]', true) ?: [],
    'phone' => $row['phone'],
    'whatsapp' => $row['whatsapp'],
  ];
}

function save_upload($file, $dir, $urlBase) {
  $ext = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
  if (!in_array($ext, ['jpg', 'jpeg', 'png', 'webp', 'gif'], true)) {
    send(['error' => 'Only image files are allowed'], 400);
  }

  $cfg = $GLOBALS['config'] ?? [];
  if (is_array($cfg) && cloudinary_configured($cfg)) {
    $folder = cloudinary_folder_from_url_base($urlBase);
    $url = cloudinary_upload_file($file['tmp_name'], $file['name'], $folder, $cfg);
    if ($url) {
      return $url;
    }
  }

  if (!is_dir($dir)) mkdir($dir, 0775, true);
  $name = uniqid('p_', true) . '.' . $ext;
  move_uploaded_file($file['tmp_name'], $dir . '/' . $name);
  return $urlBase . '/' . $name;
}

function map_post($row) {
  return [
    'id' => $row['id'],
    'title' => $row['title'],
    'category' => $row['category'],
    'image' => public_url($row['image']),
    'excerpt' => $row['excerpt'],
    'body' => $row['body'],
  ];
}

if (($parts[0] ?? '') === 'posts' && $method === 'GET' && !isset($parts[1])) {
  try {
    $rows = $pdo->query('SELECT * FROM posts ORDER BY created_at DESC, title ASC')->fetchAll(PDO::FETCH_ASSOC);
  } catch (Throwable $e) {
    $rows = $pdo->query('SELECT * FROM posts ORDER BY title ASC')->fetchAll(PDO::FETCH_ASSOC);
  }
  send(['posts' => array_map('map_post', $rows)]);
}

if (($parts[0] ?? '') === 'posts' && $method === 'GET' && isset($parts[1])) {
  $stmt = $pdo->prepare('SELECT * FROM posts WHERE id = ?');
  $stmt->execute([$parts[1]]);
  $row = $stmt->fetch(PDO::FETCH_ASSOC);
  if (!$row) send(['error' => 'Post not found'], 404);
  send(['post' => map_post($row)]);
}

if (($parts[0] ?? '') === 'posts' && $method === 'POST') {
  require_perm($pdo, 'blog');
  $id = $_POST['id'] ?: preg_replace('/[^a-z0-9\-]/', '', strtolower($_POST['title'] ?? 'post'));
  $image = $_POST['image'] ?? '';
  if (!empty($_FILES['postImage']['tmp_name'])) {
    $image = save_upload($_FILES['postImage'], dirname($config['upload_dir']) . '/posts', '/uploads/posts');
  }
  if ($image === '') {
    $exist = $pdo->prepare('SELECT image FROM posts WHERE id = ?');
    $exist->execute([$id]);
    $row = $exist->fetch(PDO::FETCH_ASSOC);
    if (!empty($row['image'])) $image = $row['image'];
  }
  $stmt = $pdo->prepare('INSERT INTO posts (id, title, category, image, excerpt, body) VALUES (?,?,?,?,?,?)
    ON DUPLICATE KEY UPDATE title=VALUES(title), category=VALUES(category), image=VALUES(image), excerpt=VALUES(excerpt), body=VALUES(body)');
  $stmt->execute([$id, $_POST['title'] ?? '', $_POST['category'] ?? '', $image, $_POST['excerpt'] ?? '', $_POST['body'] ?? '']);
  send(['ok' => true, 'id' => $id]);
}

if (($parts[0] ?? '') === 'posts' && $method === 'DELETE' && isset($parts[1])) {
  require_perm($pdo, 'blog');
  $pdo->prepare('DELETE FROM posts WHERE id = ?')->execute([$parts[1]]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'projects' && $method === 'GET' && !isset($parts[1])) {
  $rows = $pdo->query('SELECT * FROM projects ORDER BY created_at DESC')->fetchAll(PDO::FETCH_ASSOC);
  send(['projects' => array_map('map_project', $rows)]);
}

if (($parts[0] ?? '') === 'projects' && $method === 'GET' && isset($parts[1])) {
  $stmt = $pdo->prepare('SELECT * FROM projects WHERE slug = ?');
  $stmt->execute([$parts[1]]);
  $row = $stmt->fetch(PDO::FETCH_ASSOC);
  if (!$row) send(['error' => 'Project not found'], 404);
  send(['project' => map_project($row)]);
}

if (($parts[0] ?? '') === 'projects' && $method === 'POST') {
  require_user($pdo);
  $id = $_POST['id'] ?: preg_replace('/[^a-z0-9\-]/', '', strtolower($_POST['title'] ?? 'project'));
  $slug = $_POST['slug'] ?: $id;
  $image = $_POST['image'] ?? '';
  if (!empty($_FILES['image']['tmp_name'])) {
    $image = save_upload($_FILES['image'], $config['upload_dir'], $config['upload_url']);
  }
  if ($image === '') {
    $exist = $pdo->prepare('SELECT image FROM projects WHERE id = ? OR slug = ?');
    $exist->execute([$id, $slug]);
    $row = $exist->fetch(PDO::FETCH_ASSOC);
    if (!empty($row['image'])) $image = $row['image'];
  }
  $gallery = [];
  if (!empty($_POST['gallery'])) {
    $decoded = json_decode($_POST['gallery'], true);
    if (is_array($decoded)) $gallery = $decoded;
  }
  if (!empty($_FILES['gallery'])) {
    $files = $_FILES['gallery'];
    if (is_array($files['name'])) {
      for ($i = 0; $i < count($files['name']); $i++) {
        if (!$files['tmp_name'][$i]) continue;
        $gallery[] = save_upload([
          'name' => $files['name'][$i],
          'tmp_name' => $files['tmp_name'][$i],
        ], $config['upload_dir'], $config['upload_url']);
      }
    }
  }
  if (!$image && $gallery) $image = $gallery[0];
  $features = json_decode($_POST['features'] ?? '[]', true) ?: [];
  $stmt = $pdo->prepare('INSERT INTO projects
    (id, slug, title, status, deal, badge, category, type, location, city, price, beds, baths, rooms, parking, size, image, gallery_json, description, features_json, phone, whatsapp)
    VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)
    ON DUPLICATE KEY UPDATE title=VALUES(title), status=VALUES(status), deal=VALUES(deal), badge=VALUES(badge), category=VALUES(category), type=VALUES(type), location=VALUES(location), city=VALUES(city), price=VALUES(price), beds=VALUES(beds), baths=VALUES(baths), rooms=VALUES(rooms), parking=VALUES(parking), size=VALUES(size), image=VALUES(image), gallery_json=VALUES(gallery_json), description=VALUES(description), features_json=VALUES(features_json), phone=VALUES(phone), whatsapp=VALUES(whatsapp)');
  $stmt->execute([
    $id, $slug, $_POST['title'] ?? '', $_POST['status'] ?? 'ongoing', $_POST['deal'] ?? 'buy',
    $_POST['badge'] ?? '', $_POST['category'] ?? '', $_POST['type'] ?? '', $_POST['location'] ?? '',
    $_POST['city'] ?? '', $_POST['price'] ?? 0, $_POST['beds'] ?? 0, $_POST['baths'] ?? 0,
    $_POST['rooms'] ?? 0, $_POST['parking'] ?? 0, $_POST['size'] ?? 0, $image,
    json_encode($gallery), $_POST['description'] ?? '', json_encode($features),
    $_POST['phone'] ?? '', $_POST['whatsapp'] ?? '',
  ]);
  send(['ok' => true, 'id' => $id]);
}

if (($parts[0] ?? '') === 'projects' && $method === 'DELETE' && isset($parts[1])) {
  require_user($pdo);
  $stmt = $pdo->prepare('DELETE FROM projects WHERE id = ? OR slug = ?');
  $stmt->execute([$parts[1], $parts[1]]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'auth' && ($parts[1] ?? '') === 'login' && $method === 'POST') {
  $body = json_body();
  $stmt = $pdo->prepare('SELECT * FROM users WHERE username = ?');
  $stmt->execute([$body['username'] ?? '']);
  $user = $stmt->fetch(PDO::FETCH_ASSOC);
  if (!$user || !password_verify($body['password'] ?? '', $user['password_hash'])) {
    send(['error' => 'Wrong username or password.'], 401);
  }
  if ((int) ($user['active'] ?? 1) === 0) {
    send(['error' => 'This account is disabled.'], 403);
  }
  $token = bin2hex(random_bytes(24));
  $pdo->prepare('INSERT INTO tokens (token, user_id, expires_at) VALUES (?, ?, DATE_ADD(NOW(), INTERVAL 7 DAY))')
    ->execute([$token, $user['id']]);
  send(['token' => $token, 'user' => public_user($user)]);
}

if (($parts[0] ?? '') === 'auth' && ($parts[1] ?? '') === 'me' && $method === 'GET') {
  $row = require_perm($pdo, 'profile');
  send(['ok' => true, 'user' => public_user($row)]);
}

if (($parts[0] ?? '') === 'auth' && ($parts[1] ?? '') === 'logout' && $method === 'POST') {
  $token = bearer();
  if ($token) {
    $pdo->prepare('DELETE FROM tokens WHERE token = ?')->execute([$token]);
  }
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'users' && $method === 'GET' && !isset($parts[1])) {
  require_admin_user($pdo);
  $rows = $pdo->query('SELECT id, username, role, display_name, designation, photo, permissions, active, created_at FROM users ORDER BY id ASC')->fetchAll(PDO::FETCH_ASSOC);
  send(['users' => array_map('public_user', $rows)]);
}

if (($parts[0] ?? '') === 'users' && $method === 'POST' && !isset($parts[1])) {
  require_admin_user($pdo);
  $body = json_body();
  $username = strtolower(trim((string) ($body['username'] ?? '')));
  $displayName = trim((string) ($body['displayName'] ?? $body['display_name'] ?? $username));
  $designation = trim((string) ($body['designation'] ?? ''));
  $password = (string) ($body['password'] ?? '');
  $permissions = parse_permissions(json_encode($body['permissions'] ?? []), 'subadmin');
  $active = (isset($body['active']) && ($body['active'] === false || $body['active'] === 0)) ? 0 : 1;
  if (!preg_match('/^[a-z0-9._-]{3,40}$/', $username)) {
    send(['error' => 'Username must be 3-40 letters, numbers, or . _ -'], 400);
  }
  $pwErr = password_policy_error($password); if ($pwErr !== '') send(['error' => $pwErr], 400);
  try {
    $pdo->prepare('INSERT INTO users (username, password_hash, role, display_name, designation, permissions, active) VALUES (?, ?, ?, ?, ?, ?, ?)')
      ->execute([$username, password_hash($password, PASSWORD_BCRYPT), 'subadmin', $displayName ?: $username, $designation, json_encode($permissions), $active]);
    $row = account_of($pdo, (int) $pdo->lastInsertId());
    send(['ok' => true, 'user' => public_user($row)]);
  } catch (Throwable $e) {
    send(['error' => 'Username already exists.'], 400);
  }
}

if (($parts[0] ?? '') === 'users' && isset($parts[1]) && ($method === 'PUT' || $method === 'POST')) {
  require_admin_user($pdo);
  $id = (int) $parts[1];
  $user = account_of($pdo, $id);
  if (!$user) send(['error' => 'User not found.'], 404);
  if (($user['role'] ?? '') === 'admin') send(['error' => 'The main admin account cannot be edited here.'], 400);
  $body = json_body();
  $displayName = trim((string) ($body['displayName'] ?? $body['display_name'] ?? $user['display_name'] ?? $user['username']));
  $designation = trim((string) ($body['designation'] ?? $user['designation'] ?? ''));
  $permissions = parse_permissions(json_encode($body['permissions'] ?? []), 'subadmin');
  $active = (isset($body['active']) && ($body['active'] === false || $body['active'] === 0)) ? 0 : 1;
  $password = (string) ($body['password'] ?? '');
  if ($password !== '') { $pwErr = password_policy_error($password); if ($pwErr !== '') send(['error' => $pwErr], 400); }
  if ($password !== '') {
    $pdo->prepare('UPDATE users SET display_name = ?, designation = ?, permissions = ?, active = ?, password_hash = ? WHERE id = ?')
      ->execute([$displayName ?: $user['username'], $designation, json_encode($permissions), $active, password_hash($password, PASSWORD_BCRYPT), $id]);
  } else {
    $pdo->prepare('UPDATE users SET display_name = ?, designation = ?, permissions = ?, active = ? WHERE id = ?')
      ->execute([$displayName ?: $user['username'], $designation, json_encode($permissions), $active, $id]);
  }
  if (!$active) {
    $pdo->prepare('DELETE FROM tokens WHERE user_id = ?')->execute([$id]);
  }
  send(['ok' => true, 'user' => public_user(account_of($pdo, $id))]);
}

if (($parts[0] ?? '') === 'users' && isset($parts[1]) && $method === 'DELETE') {
  require_admin_user($pdo);
  $id = (int) $parts[1];
  $user = account_of($pdo, $id);
  if (!$user) send(['error' => 'User not found.'], 404);
  if (($user['role'] ?? '') === 'admin') send(['error' => 'The main admin account cannot be deleted.'], 400);
  $pdo->prepare('DELETE FROM users WHERE id = ?')->execute([$id]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'messages' && $method === 'POST') {
  $body = json_body();
  if (empty($body['name']) || empty($body['message'])) send(['error' => 'Name and message required'], 400);
  $pdo->prepare('INSERT INTO messages (name, phone, email, address, message) VALUES (?,?,?,?,?)')
    ->execute([$body['name'], $body['phone'] ?? '', $body['email'] ?? '', $body['address'] ?? '', $body['message']]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'messages' && $method === 'GET') {
  require_perm($pdo, 'messages');
  $rows = $pdo->query('SELECT id, name, phone, email, address, message, created_at FROM messages ORDER BY created_at DESC, id DESC')->fetchAll(PDO::FETCH_ASSOC);
  send(['messages' => $rows]);
}

if (($parts[0] ?? '') === 'messages' && $method === 'DELETE' && isset($parts[1])) {
  require_perm($pdo, 'messages');
  $pdo->prepare('DELETE FROM messages WHERE id = ?')->execute([(int) $parts[1]]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'subscribers' && $method === 'POST') {
  $body = json_body();
  $email = strtolower(trim($body['email'] ?? ''));
  if (!filter_var($email, FILTER_VALIDATE_EMAIL)) send(['error' => 'Valid email required'], 400);
  try {
    $pdo->prepare('INSERT INTO subscribers (email) VALUES (?)')->execute([$email]);
  } catch (Throwable $e) {
    if ((int) $e->getCode() !== 23000) {
      send(['error' => 'Could not save subscriber'], 500);
    }
  }
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'subscribers' && $method === 'GET') {
  require_perm($pdo, 'subscribers');
  $rows = $pdo->query('SELECT id, email, created_at FROM subscribers ORDER BY created_at DESC, id DESC')->fetchAll(PDO::FETCH_ASSOC);
  send(['subscribers' => $rows]);
}

if (($parts[0] ?? '') === 'subscribers' && $method === 'DELETE' && isset($parts[1])) {
  require_perm($pdo, 'subscribers');
  $pdo->prepare('DELETE FROM subscribers WHERE id = ?')->execute([(int) $parts[1]]);
  send(['ok' => true]);
}

if (($parts[0] ?? '') === 'socials' && $method === 'POST') {
  require_perm($pdo, 'socials');
  $body = json_body();
  $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  foreach (['facebook', 'x', 'linkedin', 'instagram', 'whatsapp'] as $key) {
    $up->execute(['social_' . $key, trim((string) ($body[$key] ?? ''))]);
  }
  $up->execute(['pay_online', trim((string) ($body['payOnline'] ?? $body['pay_online'] ?? ''))]);
  $saved = $pdo->query('SELECT setting_key, setting_value FROM settings')->fetchAll(PDO::FETCH_KEY_PAIR);
  if (!empty($saved['admin_photo'])) {
    $saved['admin_photo'] = public_url($saved['admin_photo']);
  }
  send(['ok' => true, 'settings' => $saved]);
}

if (($parts[0] ?? '') === 'theme' && $method === 'POST') {
  require_perm($pdo, 'profile');
  $body = json_body();
  $hex = function ($value, $fallback) {
    $raw = strtolower(trim((string) $value));
    return preg_match('/^#[0-9a-f]{6}$/', $raw) ? $raw : $fallback;
  };
  $primary = $hex($body['primary'] ?? $body['theme_primary'] ?? '', '#f58634');
  $button = $hex($body['button'] ?? $body['theme_button'] ?? '', $primary);
  $menuHover = $hex($body['menuHover'] ?? $body['theme_menu_hover'] ?? '', $primary);
  $sidebar = $hex($body['sidebar'] ?? $body['theme_sidebar'] ?? '', '#141414');
  $menuBg = $hex($body['menuBg'] ?? $body['theme_menu_bg'] ?? '', '#252525');
  $navBg = $hex($body['navBg'] ?? $body['theme_nav_bg'] ?? '', '#fff4ea');
  $brand = $hex($body['brand'] ?? $body['theme_brand'] ?? '', '#161616');
  $headerBg = $hex($body['headerBg'] ?? $body['theme_header_bg'] ?? '', '#ffffff');
  $headerMode = strtolower(trim((string) ($body['headerMode'] ?? $body['theme_header_mode'] ?? 'light'))) === 'dark' ? 'dark' : 'light';
  $siteName = trim((string) ($body['siteName'] ?? $body['site_name'] ?? ''));
  $siteDialog = trim((string) ($body['siteDialog'] ?? $body['site_dialog'] ?? ''));
  $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $up->execute(['theme_primary', $primary]);
  $up->execute(['theme_button', $button]);
  $up->execute(['theme_menu_hover', $menuHover]);
  $up->execute(['theme_sidebar', $sidebar]);
  $up->execute(['theme_menu_bg', $menuBg]);
  $up->execute(['theme_nav_bg', $navBg]);
  $up->execute(['theme_brand', $brand]);
  $up->execute(['theme_header_bg', $headerBg]);
  $up->execute(['theme_header_mode', $headerMode]);
  $up->execute(['site_name', $siteName]);
  $up->execute(['site_dialog', $siteDialog]);
  $saved = $pdo->query('SELECT setting_key, setting_value FROM settings')->fetchAll(PDO::FETCH_KEY_PAIR);
  if (!empty($saved['admin_photo'])) {
    $saved['admin_photo'] = public_url($saved['admin_photo']);
  }
  send(['ok' => true, 'settings' => $saved]);
}

if (($parts[0] ?? '') === 'settings' && $method === 'GET') {
  $rows = $pdo->query('SELECT setting_key, setting_value FROM settings')->fetchAll(PDO::FETCH_KEY_PAIR);
  if (!empty($rows['admin_photo'])) {
    $rows['admin_photo'] = public_url($rows['admin_photo']);
  }
  send(['settings' => $rows]);
}

if (($parts[0] ?? '') === 'profile' && $method === 'POST') {
  $account = require_perm($pdo, 'profile');
  $userId = (int) $account['id'];
  $name = trim($_POST['admin_name'] ?? '');
  $designation = trim((string) ($_POST['designation'] ?? $_POST['admin_designation'] ?? ''));
  $address = trim($_POST['admin_address'] ?? '');
  $photo = trim($_POST['admin_photo'] ?? '');
  $currentPass = (string) ($_POST['current_password'] ?? '');
  $newPass = (string) ($_POST['new_password'] ?? '');
  $confirmPass = (string) ($_POST['confirm_password'] ?? '');
  if (!empty($_FILES['adminPhoto']['tmp_name'])) {
    $photo = save_upload($_FILES['adminPhoto'], dirname($config['upload_dir']) . '/pages', '/uploads/pages');
  }
  if ($currentPass !== '' || $newPass !== '' || $confirmPass !== '') {
    if (strlen($newPass) < 6) {
      $pwErr = password_policy_error($newPass); if ($pwErr !== '') send(['error' => $pwErr], 400);
    }
    if ($newPass !== $confirmPass) {
      send(['error' => 'New passwords do not match.'], 400);
    }
    $stmt = $pdo->prepare('SELECT password_hash FROM users WHERE id = ?');
    $stmt->execute([$userId]);
    $user = $stmt->fetch(PDO::FETCH_ASSOC);
    if (!$user || !password_verify($currentPass, $user['password_hash'])) {
      send(['error' => 'Current password is wrong.'], 400);
    }
    $pdo->prepare('UPDATE users SET password_hash = ? WHERE id = ?')
      ->execute([password_hash($newPass, PASSWORD_BCRYPT), $userId]);
  }
  if ($photo !== '') {
    $pdo->prepare('UPDATE users SET display_name = ?, designation = ?, photo = ? WHERE id = ?')
      ->execute([$name ?: $account['username'], $designation, $photo, $userId]);
  } else {
    $pdo->prepare('UPDATE users SET display_name = ?, designation = ? WHERE id = ?')
      ->execute([$name ?: $account['username'], $designation, $userId]);
  }
  if (($account['role'] ?? '') === 'admin') {
    $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
    $up->execute(['admin_name', $name]);
    $up->execute(['admin_address', $address]);
    if ($photo !== '') {
      $up->execute(['admin_photo', $photo]);
    }
  }
  $saved = $pdo->query('SELECT setting_key, setting_value FROM settings')->fetchAll(PDO::FETCH_KEY_PAIR);
  if (!empty($saved['admin_photo'])) {
    $saved['admin_photo'] = public_url($saved['admin_photo']);
  }
  send(['ok' => true, 'settings' => $saved, 'user' => public_user(account_of($pdo, $userId))]);
}

if (($parts[0] ?? '') === 'hero' && $method === 'GET') {
  $stmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'hero_slides'");
  $raw = $stmt ? $stmt->fetchColumn() : '';
  $slides = $raw ? json_decode($raw, true) : [];
  if (!is_array($slides)) $slides = [];
  foreach ($slides as &$slide) {
    if (!empty($slide['image'])) $slide['image'] = public_url($slide['image']);
  }
  unset($slide);
  send(['slides' => $slides]);
}

if (($parts[0] ?? '') === 'hero' && $method === 'POST') {
  require_perm($pdo, 'slider');
  $slides = json_decode($_POST['slides'] ?? '[]', true);
  if (!is_array($slides) || !$slides) send(['error' => 'At least one slide is required'], 400);
  $heroDir = dirname($config['upload_dir']) . '/hero';
  $heroUrl = '/uploads/hero';
  foreach ($_FILES as $key => $file) {
    if (!preg_match('/^hero(\d+)$/', $key, $m) || empty($file['tmp_name'])) continue;
    $i = (int) $m[1];
    if (!isset($slides[$i])) continue;
    $slides[$i]['image'] = save_upload($file, $heroDir, $heroUrl);
  }
  $clean = [];
  foreach (array_slice($slides, 0, 8) as $i => $slide) {
    $clean[] = [
      'id' => $slide['id'] ?? ('slide-' . ($i + 1)),
      'image' => $slide['image'] ?? '',
      'eyebrow' => $slide['eyebrow'] ?? '',
      'title' => $slide['title'] ?? '',
      'text' => $slide['text'] ?? '',
    ];
  }
  $stmt = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES ('hero_slides', ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $stmt->execute([json_encode($clean, JSON_UNESCAPED_UNICODE)]);
  send(['ok' => true, 'slides' => $clean]);
}

if (($parts[0] ?? '') === 'footer-gallery' && $method === 'GET') {
  $stmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'footer_gallery'");
  $raw = $stmt ? $stmt->fetchColumn() : '';
  $items = $raw ? json_decode($raw, true) : [];
  if (!is_array($items)) $items = [];
  foreach ($items as &$item) {
    if (!empty($item['image'])) $item['image'] = public_url($item['image']);
  }
  unset($item);
  send(['items' => $items]);
}

if (($parts[0] ?? '') === 'footer-gallery' && $method === 'POST') {
  require_perm($pdo, 'footer');
  $items = json_decode($_POST['items'] ?? '[]', true);
  if (!is_array($items)) $items = [];
  $dir = dirname($config['upload_dir']) . '/gallery';
  $url = '/uploads/gallery';
  foreach ($_FILES as $key => $file) {
    if (!preg_match('/^foot(\d+)$/', $key, $m) || empty($file['tmp_name'])) continue;
    $i = (int) $m[1];
    if (!isset($items[$i])) $items[$i] = ['image' => '', 'title' => '', 'link' => '/project'];
    $items[$i]['image'] = save_upload($file, $dir, $url);
  }
  $clean = [];
  foreach (array_slice($items, 0, 48) as $i => $item) {
    if (empty($item['image'])) continue;
    $clean[] = [
      'image' => $item['image'] ?? '',
      'title' => $item['title'] ?? ('Gallery ' . ($i + 1)),
      'link' => $item['link'] ?? '/gallery',
    ];
  }
  $stmt = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES ('footer_gallery', ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $stmt->execute([json_encode($clean, JSON_UNESCAPED_UNICODE)]);
  send(['ok' => true, 'items' => $clean]);
}

if (($parts[0] ?? '') === 'videos' && $method === 'GET') {
  $stmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'video_gallery'");
  $raw = $stmt ? $stmt->fetchColumn() : '';
  $videos = $raw ? json_decode($raw, true) : [];
  if (!is_array($videos)) $videos = [];
  send(['videos' => $videos]);
}

if (($parts[0] ?? '') === 'videos' && $method === 'POST') {
  require_perm($pdo, 'footer');
  $payload = json_body();
  $items = $payload['videos'] ?? $payload ?? [];
  if (!empty($_POST['videos'])) {
    $posted = json_decode($_POST['videos'], true);
    if (is_array($posted)) $items = $posted;
  }
  if (!is_array($items)) $items = [];
  $clean = [];
  foreach (array_slice($items, 0, 24) as $i => $item) {
    $url = trim((string) ($item['url'] ?? ''));
    if ($url === '') continue;
    $clean[] = [
      'id' => $item['id'] ?? ('v-' . ($i + 1)),
      'url' => $url,
      'title' => $item['title'] ?? '',
      'titleBn' => $item['titleBn'] ?? '',
    ];
  }
  $stmt = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES ('video_gallery', ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $stmt->execute([json_encode($clean, JSON_UNESCAPED_UNICODE)]);
  send(['ok' => true, 'videos' => $clean]);
}

if (($parts[0] ?? '') === 'pages' && $method === 'GET') {
  $stmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'site_pages'");
  $raw = $stmt ? $stmt->fetchColumn() : '';
  $pages = $raw ? json_decode($raw, true) : [];
  if (!is_array($pages)) $pages = [];
  if (!empty($pages['about']['image'])) {
    $pages['about']['image'] = public_url($pages['about']['image']);
  }
  send(['pages' => $pages]);
}

if (($parts[0] ?? '') === 'pages' && $method === 'POST') {
  require_perm($pdo, 'about', 'contact');
  $pages = json_decode($_POST['pages'] ?? '{}', true);
  if (!is_array($pages)) $pages = [];
  if (!empty($_FILES['aboutImage']['tmp_name'])) {
    if (!isset($pages['about']) || !is_array($pages['about'])) $pages['about'] = [];
    $pages['about']['image'] = save_upload($_FILES['aboutImage'], dirname($config['upload_dir']) . '/pages', '/uploads/pages');
  }
  $stmt = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES ('site_pages', ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $stmt->execute([json_encode($pages, JSON_UNESCAPED_UNICODE)]);
  foreach (['phone', 'whatsapp', 'email', 'address'] as $key) {
    if (empty($pages['contact'][$key])) continue;
    $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
    $up->execute([$key, $pages['contact'][$key]]);
  }
  $siteName = trim((string) ($pages['contact']['siteName'] ?? $pages['contact']['site_name'] ?? ''));
  if ($siteName !== '') {
    $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES ('site_name', ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
    $up->execute([$siteName]);
  }
  send(['ok' => true, 'pages' => $pages]);
}

if (($parts[0] ?? '') === 'menus' && $method === 'GET') {
  $menuStmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'site_menus'");
  $pageStmt = $pdo->query("SELECT setting_value FROM settings WHERE setting_key = 'cms_pages'");
  $menus = json_decode($menuStmt ? ($menuStmt->fetchColumn() ?: '[]') : '[]', true);
  $pages = json_decode($pageStmt ? ($pageStmt->fetchColumn() ?: '{}') : '{}', true);
  if (!is_array($menus)) $menus = [];
  if (!is_array($pages)) $pages = [];
  foreach ($pages as $slug => $page) {
    if (!empty($page['image'])) {
      $pages[$slug]['image'] = public_url($page['image']);
    }
    if (!empty($page['members']) && is_array($page['members'])) {
      foreach ($page['members'] as $i => $member) {
        if (!empty($member['image'])) {
          $pages[$slug]['members'][$i]['image'] = public_url($member['image']);
        }
      }
    }
  }
  send(['menus' => $menus, 'pages' => $pages]);
}

if (($parts[0] ?? '') === 'menus' && $method === 'POST') {
  require_perm($pdo, 'menus');
  $menus = json_decode($_POST['menus'] ?? '[]', true);
  $pages = json_decode($_POST['pages'] ?? '{}', true);
  if (!is_array($menus)) $menus = [];
  if (!is_array($pages)) $pages = [];
  $slug = trim($_POST['pageSlug'] ?? '');
  if ($slug !== '' && !empty($_FILES['pageImage']['tmp_name'])) {
    if (!isset($pages[$slug]) || !is_array($pages[$slug])) $pages[$slug] = [];
    $pages[$slug]['image'] = save_upload($_FILES['pageImage'], dirname($config['upload_dir']) . '/pages', '/uploads/pages');
  }
  foreach ($_FILES as $key => $file) {
    if (!preg_match('/^member_(.+)$/', $key, $match) || empty($file['tmp_name'])) continue;
    $memberId = $match[1];
    $target = $slug;
    if ($target === '') {
      foreach ($pages as $pageSlug => $page) {
        foreach ($page['members'] ?? [] as $member) {
          if (($member['id'] ?? '') === $memberId) {
            $target = $pageSlug;
            break 2;
          }
        }
      }
    }
    if ($target === '' || !isset($pages[$target]) || !is_array($pages[$target]['members'] ?? null)) continue;
    foreach ($pages[$target]['members'] as $i => $member) {
      if (($member['id'] ?? '') === $memberId) {
        $pages[$target]['members'][$i]['image'] = save_upload($file, dirname($config['upload_dir']) . '/pages', '/uploads/pages');
      }
    }
  }
  $up = $pdo->prepare("INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)");
  $up->execute(['site_menus', json_encode($menus, JSON_UNESCAPED_UNICODE)]);
  $up->execute(['cms_pages', json_encode($pages, JSON_UNESCAPED_UNICODE)]);
  send(['ok' => true, 'menus' => $menus, 'pages' => $pages]);
}

require_once __DIR__ . '/edu.php';
require __DIR__ . '/admissions.php';
if (admissions_handle($parts, $method, $pdo, $config)) {
  exit;
}
require __DIR__ . '/careers.php';
if (careers_handle($parts, $method, $pdo, $config)) {
  exit;
}
if (edu_handle($parts, $method, $config)) {
  exit;
}

if (($parts[0] ?? '') === 'seed' && $method === 'POST') {
  $count = (int) $pdo->query('SELECT COUNT(*) FROM users')->fetchColumn();
  if ($count === 0) {
    $hash = password_hash('iisac123', PASSWORD_BCRYPT);
    $pdo->prepare('INSERT INTO users (username, password_hash, role, display_name, active) VALUES (?, ?, ?, ?, 1)')->execute(['admin', $hash, 'admin', 'Admin']);
  }
  send(['ok' => true]);
}

send(['error' => 'Not found'], 404);
