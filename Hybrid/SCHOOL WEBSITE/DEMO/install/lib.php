<?php

function safa_root()
{
  return dirname(__DIR__);
}

function safa_lock_file()
{
  return safa_root() . '/install.lock';
}

function safa_config_file()
{
  return safa_root() . '/api/config.json';
}

function safa_is_installed()
{
  return is_file(safa_lock_file()) && is_file(safa_config_file());
}

function safa_json_input()
{
  $raw = file_get_contents('php://input');
  $data = json_decode($raw, true);
  return is_array($data) ? $data : [];
}

function safa_send($data, $code = 200)
{
  http_response_code($code);
  header('Content-Type: application/json; charset=utf-8');
  echo json_encode($data, JSON_UNESCAPED_UNICODE);
  exit;
}

function safa_safe_name($value)
{
  $value = trim((string) $value);
  if (!preg_match('/^[A-Za-z0-9_\-]{1,64}$/', $value)) {
    return '';
  }
  return $value;
}

function safa_is_built_site()
{
  $html = safa_root() . '/index.html';
  if (!is_file($html)) {
    $html = safa_root() . '/app.html';
  }
  if (!is_file($html)) {
    return false;
  }
  $contents = file_get_contents($html);
  return $contents !== false && strpos($contents, '/src/main.jsx') === false;
}

function safa_requirements()
{
  $root = safa_root();
  $phpOk = version_compare(PHP_VERSION, '7.4.0', '>=');
  $checks = [
    [
      'id' => 'php',
      'label' => 'PHP 7.4+',
      'ok' => $phpOk,
      'detail' => 'PHP ' . PHP_VERSION,
    ],
    [
      'id' => 'pdo',
      'label' => 'PDO MySQL',
      'ok' => class_exists('PDO') && in_array('mysql', PDO::getAvailableDrivers(), true),
      'detail' => class_exists('PDO') ? 'Available' : 'Missing',
    ],
    [
      'id' => 'json',
      'label' => 'JSON',
      'ok' => function_exists('json_encode'),
      'detail' => function_exists('json_encode') ? 'Available' : 'Missing',
    ],
    [
      'id' => 'api',
      'label' => 'api folder writable',
      'ok' => is_dir($root . '/api') && is_writable($root . '/api'),
      'detail' => $root . '/api',
    ],
    [
      'id' => 'root',
      'label' => 'Site folder writable',
      'ok' => is_writable($root),
      'detail' => $root,
    ],
    [
      'id' => 'uploads',
      'label' => 'uploads folder',
      'ok' => true,
      'detail' => $root . '/uploads',
    ],
    [
      'id' => 'build',
      'label' => 'Website build (index.html)',
      'ok' => safa_is_built_site(),
      'detail' => safa_is_built_site()
        ? 'Built files found'
        : 'Run npm run build, then upload the dist folder',
    ],
  ];

  $uploads = $root . '/uploads';
  if (!is_dir($uploads)) {
    @mkdir($uploads, 0775, true);
  }
  $checks[5]['ok'] = is_dir($uploads) && is_writable($uploads);

  $ready = true;
  foreach ($checks as $check) {
    if (!$check['ok'] && $check['id'] !== 'build') {
      $ready = false;
    }
  }

  return [
    'ready' => $ready,
    'php' => PHP_VERSION,
    'checks' => $checks,
    'built' => safa_is_built_site(),
  ];
}

function safa_pdo($host, $user, $pass, $name = '', $port = '3306')
{
  $port = $port !== '' ? $port : '3306';
  $dsn = "mysql:host={$host};port={$port};charset=utf8mb4";
  if ($name !== '') {
    $dsn .= ';dbname=' . $name;
  }
  return new PDO($dsn, $user, $pass, [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
  ]);
}

function safa_test_db($input)
{
  $host = trim($input['db_host'] ?? 'localhost');
  $port = trim($input['db_port'] ?? '3306');
  $name = safa_safe_name($input['db_name'] ?? '');
  $user = trim($input['db_user'] ?? '');
  $pass = (string) ($input['db_pass'] ?? '');
  $create = !empty($input['create_db']);

  if ($host === '' || $user === '' || $name === '') {
    return ['ok' => false, 'error' => 'Host, database name, and username are required.'];
  }

  try {
    $pdo = safa_pdo($host, $user, $pass, '', $port);
  } catch (Throwable $e) {
    return ['ok' => false, 'error' => 'Could not connect to MySQL: ' . $e->getMessage()];
  }

  try {
    if ($create) {
      $pdo->exec('CREATE DATABASE IF NOT EXISTS `' . str_replace('`', '', $name) . '` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci');
    }
    $pdo->exec('USE `' . str_replace('`', '', $name) . '`');
  } catch (Throwable $e) {
    return ['ok' => false, 'error' => 'Database not found. Create it in cPanel or tick “Create database”. ' . $e->getMessage()];
  }

  return ['ok' => true, 'message' => 'Database connection successful.'];
}

function safa_create_tables(PDO $pdo)
{
  $sql = [
    'CREATE TABLE IF NOT EXISTS users (
      id INT AUTO_INCREMENT PRIMARY KEY,
      username VARCHAR(80) NOT NULL UNIQUE,
      password_hash VARCHAR(255) NOT NULL,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS tokens (
      token VARCHAR(80) PRIMARY KEY,
      user_id INT NOT NULL,
      expires_at DATETIME NOT NULL,
      FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS settings (
      setting_key VARCHAR(80) PRIMARY KEY,
      setting_value TEXT NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS projects (
      id VARCHAR(120) PRIMARY KEY,
      slug VARCHAR(160) NOT NULL UNIQUE,
      title VARCHAR(255) NOT NULL,
      status VARCHAR(30) NOT NULL DEFAULT "ongoing",
      deal VARCHAR(20) NOT NULL DEFAULT "buy",
      badge VARCHAR(40) DEFAULT "",
      category VARCHAR(80) DEFAULT "",
      type VARCHAR(40) DEFAULT "Apartment",
      location VARCHAR(255) DEFAULT "",
      city VARCHAR(80) DEFAULT "",
      price DECIMAL(14,2) DEFAULT 0,
      beds INT DEFAULT 0,
      baths INT DEFAULT 0,
      rooms INT DEFAULT 0,
      parking INT DEFAULT 0,
      size INT DEFAULT 0,
      image TEXT,
      gallery_json TEXT,
      description TEXT,
      features_json TEXT,
      phone VARCHAR(40) DEFAULT "",
      whatsapp VARCHAR(40) DEFAULT "",
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS messages (
      id INT AUTO_INCREMENT PRIMARY KEY,
      name VARCHAR(120) NOT NULL,
      phone VARCHAR(40) DEFAULT "",
      email VARCHAR(160) DEFAULT "",
      address VARCHAR(255) DEFAULT "",
      message TEXT,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS subscribers (
      id INT AUTO_INCREMENT PRIMARY KEY,
      email VARCHAR(160) NOT NULL UNIQUE,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'CREATE TABLE IF NOT EXISTS posts (
      id VARCHAR(120) PRIMARY KEY,
      title VARCHAR(255) NOT NULL,
      category VARCHAR(80) DEFAULT "",
      image TEXT,
      excerpt TEXT,
      body TEXT,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
  ];

  foreach ($sql as $statement) {
    $pdo->exec($statement);
  }

  try {
    $pdo->exec('ALTER TABLE settings MODIFY setting_value TEXT NOT NULL');
  } catch (Throwable $e) {
    /* already TEXT */
  }
  try {
    $pdo->exec('ALTER TABLE posts ADD COLUMN created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP');
  } catch (Throwable $e) {
    /* column exists */
  }
}

function safa_setting(PDO $pdo, $key, $value)
{
  $stmt = $pdo->prepare('INSERT INTO settings (setting_key, setting_value) VALUES (?, ?) ON DUPLICATE KEY UPDATE setting_value = VALUES(setting_value)');
  $stmt->execute([$key, $value]);
}

function safa_mkdirs()
{
  $root = safa_root();
  foreach (['uploads', 'uploads/projects', 'uploads/hero', 'uploads/gallery', 'uploads/posts', 'uploads/pages'] as $dir) {
    $path = $root . '/' . $dir;
    if (!is_dir($path)) {
      @mkdir($path, 0775, true);
    }
  }
}

function safa_write_config($input)
{
  $config = [
    'db_host' => trim($input['db_host'] ?? 'localhost'),
    'db_port' => trim($input['db_port'] ?? '3306') ?: '3306',
    'db_name' => safa_safe_name($input['db_name'] ?? ''),
    'db_user' => trim($input['db_user'] ?? ''),
    'db_pass' => (string) ($input['db_pass'] ?? ''),
  ];
  $json = json_encode($config, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
  $file = safa_config_file();
  if (file_put_contents($file, $json) === false) {
    throw new RuntimeException('Could not write api/config.json. Make the api folder writable (755 or 775).');
  }
  @chmod($file, 0644);
}

function safa_write_lock()
{
  $ok = file_put_contents(safa_lock_file(), date('c') . "\nSAFA PROPERTIS installed\n");
  if ($ok === false) {
    throw new RuntimeException('Could not write install.lock. Make the site folder writable.');
  }
}

function safa_install($input)
{
  $test = safa_test_db($input);
  if (empty($test['ok'])) {
    return $test;
  }

  $adminUser = trim($input['admin_user'] ?? 'admin');
  $adminPass = (string) ($input['admin_pass'] ?? '');
  $adminConfirm = (string) ($input['admin_pass_confirm'] ?? '');
  $siteName = trim($input['site_name'] ?? 'SAFA PROPERTIS');
  $phone = trim($input['phone'] ?? '+880 1712-345678');
  $whatsapp = trim($input['whatsapp'] ?? '8801712345678');
  $email = trim($input['email'] ?? 'info@safapropertis.com');
  $address = trim($input['address'] ?? 'House #42, Road #7, Gulshan-1, Dhaka - 1212');
  $seedDemo = !isset($input['seed_demo']) || !empty($input['seed_demo']);

  if ($adminUser === '' || !preg_match('/^[A-Za-z0-9_\-.]{3,40}$/', $adminUser)) {
    return ['ok' => false, 'error' => 'Admin username must be 3–40 letters, numbers, or _ - .'];
  }
  if (strlen($adminPass) < 6) {
    return ['ok' => false, 'error' => 'Admin password must be at least 6 characters.'];
  }
  if ($adminPass !== $adminConfirm) {
    return ['ok' => false, 'error' => 'Admin passwords do not match.'];
  }
  if ($siteName === '') {
    $siteName = 'SAFA PROPERTIS';
  }

  $host = trim($input['db_host'] ?? 'localhost');
  $port = trim($input['db_port'] ?? '3306');
  $name = safa_safe_name($input['db_name'] ?? '');
  $user = trim($input['db_user'] ?? '');
  $pass = (string) ($input['db_pass'] ?? '');

  try {
    $pdo = safa_pdo($host, $user, $pass, '', $port);
    if (!empty($input['create_db'])) {
      $pdo->exec('CREATE DATABASE IF NOT EXISTS `' . str_replace('`', '', $name) . '` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci');
    }
    $pdo->exec('USE `' . str_replace('`', '', $name) . '`');

    safa_create_tables($pdo);
    safa_mkdirs();

    safa_setting($pdo, 'site_name', $siteName);
    safa_setting($pdo, 'phone', $phone);
    safa_setting($pdo, 'whatsapp', $whatsapp);
    safa_setting($pdo, 'email', $email);
    safa_setting($pdo, 'address', $address);

    $users = (int) $pdo->query('SELECT COUNT(*) FROM users')->fetchColumn();
    $hash = password_hash($adminPass, PASSWORD_BCRYPT);
    if ($users === 0) {
      $pdo->prepare('INSERT INTO users (username, password_hash) VALUES (?, ?)')
        ->execute([$adminUser, $hash]);
    } else {
      $userId = $pdo->query('SELECT id FROM users ORDER BY id ASC LIMIT 1')->fetchColumn();
      $pdo->prepare('UPDATE users SET username = ?, password_hash = ? WHERE id = ?')
        ->execute([$adminUser, $hash, $userId]);
    }

    $seed = require __DIR__ . '/seed-data.php';
    $pages = $seed['pages'];
    $pages['about']['lead'] = $pages['about']['lead'] ?? '';
    $pages['contact']['phone'] = $phone;
    $pages['contact']['whatsapp'] = $whatsapp;
    $pages['contact']['email'] = $email;
    $pages['contact']['address'] = $address;

    $heroCount = (int) $pdo->query("SELECT COUNT(*) FROM settings WHERE setting_key = 'hero_slides'")->fetchColumn();
    if ($heroCount === 0) {
      safa_setting($pdo, 'hero_slides', json_encode($seed['hero'], JSON_UNESCAPED_UNICODE));
    }

    $pageCount = (int) $pdo->query("SELECT COUNT(*) FROM settings WHERE setting_key = 'site_pages'")->fetchColumn();
    if ($pageCount === 0) {
      safa_setting($pdo, 'site_pages', json_encode($pages, JSON_UNESCAPED_UNICODE));
    }

    if ($seedDemo) {
      $projectCount = (int) $pdo->query('SELECT COUNT(*) FROM projects')->fetchColumn();
      if ($projectCount === 0) {
        $stmt = $pdo->prepare('INSERT INTO projects
          (id, slug, title, status, deal, badge, category, type, location, city, price, beds, baths, rooms, parking, size, image, gallery_json, description, features_json, phone, whatsapp)
          VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)');
        foreach ($seed['projects'] as $p) {
          $stmt->execute([
            $p['id'],
            $p['slug'],
            $p['title'],
            $p['status'],
            $p['deal'],
            $p['badge'],
            $p['category'],
            $p['type'],
            $p['location'],
            $p['city'],
            $p['price'],
            $p['beds'],
            $p['baths'],
            $p['rooms'],
            $p['parking'],
            $p['size'],
            $p['image'],
            json_encode($p['gallery'], JSON_UNESCAPED_UNICODE),
            $p['description'],
            json_encode($p['features'], JSON_UNESCAPED_UNICODE),
            $p['phone'] ?? $phone,
            $p['whatsapp'] ?? $whatsapp,
          ]);
        }
      }

      $postCount = (int) $pdo->query('SELECT COUNT(*) FROM posts')->fetchColumn();
      if ($postCount === 0) {
        $stmt = $pdo->prepare('INSERT INTO posts (id, title, category, image, excerpt, body) VALUES (?,?,?,?,?,?)');
        foreach ($seed['posts'] as $post) {
          $stmt->execute([$post['id'], $post['title'], $post['category'], $post['image'], $post['excerpt'], $post['body']]);
        }
      }

      $footerCount = (int) $pdo->query("SELECT COUNT(*) FROM settings WHERE setting_key = 'footer_gallery'")->fetchColumn();
      if ($footerCount === 0) {
        $gallery = [];
        foreach (array_slice($seed['projects'], 0, 6) as $p) {
          $gallery[] = [
            'image' => $p['image'],
            'title' => $p['title'],
            'link' => '/project/' . $p['slug'],
          ];
        }
        safa_setting($pdo, 'footer_gallery', json_encode($gallery, JSON_UNESCAPED_UNICODE));
      }
    }

    safa_write_config($input);
    safa_write_lock();
  } catch (Throwable $e) {
    return ['ok' => false, 'error' => $e->getMessage()];
  }

  return [
    'ok' => true,
    'message' => 'Installation complete.',
    'admin_user' => $adminUser,
    'site_name' => $siteName,
  ];
}
