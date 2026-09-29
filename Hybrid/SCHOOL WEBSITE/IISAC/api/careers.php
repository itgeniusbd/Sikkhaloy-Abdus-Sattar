<?php
require_once __DIR__ . '/cloudinary.php';

function careers_ensure(PDO $pdo)
{
  $pdo->exec('CREATE TABLE IF NOT EXISTS jobs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    title_bn VARCHAR(200) DEFAULT \'\',
    department VARCHAR(120) DEFAULT \'\',
    location VARCHAR(160) DEFAULT \'\',
    job_type VARCHAR(40) DEFAULT \'Full-time\',
    deadline VARCHAR(20) DEFAULT \'\',
    description TEXT,
    description_bn TEXT,
    published TINYINT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci');
  try {
    $pdo->exec("ALTER TABLE jobs ADD COLUMN image VARCHAR(255) DEFAULT ''");
  } catch (Throwable $e) {
    /* already present */
  }
  $pdo->exec('CREATE TABLE IF NOT EXISTS job_applications (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL,
    name VARCHAR(160) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(160) DEFAULT \'\',
    address TEXT,
    education TEXT,
    experience TEXT,
    cover_letter TEXT,
    cv VARCHAR(255) DEFAULT \'\',
    status VARCHAR(20) NOT NULL DEFAULT \'pending\',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci');
}

function careers_map_job($row)
{
  return [
    'id' => (int) $row['id'],
    'title' => $row['title'],
    'titleBn' => $row['title_bn'] ?? '',
    'department' => $row['department'] ?? '',
    'location' => $row['location'] ?? '',
    'jobType' => $row['job_type'] ?? 'Full-time',
    'deadline' => $row['deadline'] ?? '',
    'description' => $row['description'] ?? '',
    'descriptionBn' => $row['description_bn'] ?? '',
    'image' => function_exists('public_url') ? public_url($row['image'] ?? '') : ($row['image'] ?? ''),
    'published' => ((int) ($row['published'] ?? 1)) !== 0,
    'createdAt' => $row['created_at'] ?? '',
  ];
}

function careers_map_app($row)
{
  return [
    'id' => (int) $row['id'],
    'jobId' => (int) $row['job_id'],
    'jobTitle' => $row['job_title'] ?? '',
    'name' => $row['name'],
    'phone' => $row['phone'],
    'email' => $row['email'] ?? '',
    'address' => $row['address'] ?? '',
    'education' => $row['education'] ?? '',
    'experience' => $row['experience'] ?? '',
    'coverLetter' => $row['cover_letter'] ?? '',
    'cv' => $row['cv'] ?? '',
    'status' => $row['status'] ?? 'pending',
    'createdAt' => $row['created_at'] ?? '',
  ];
}

function careers_save_cv($file, $dir, $urlBase, array $config = [])
{
  $ext = strtolower(pathinfo($file['name'], PATHINFO_EXTENSION));
  if (!in_array($ext, ['pdf', 'doc', 'docx'], true)) {
    send(['error' => 'CV must be PDF or Word file.'], 400);
  }

  $cfg = $config ?: ($GLOBALS['config'] ?? []);
  if (is_array($cfg) && cloudinary_configured($cfg)) {
    $url = cloudinary_upload_file($file['tmp_name'], $file['name'], 'careers', $cfg);
    if ($url) {
      return $url;
    }
  }

  if (!is_dir($dir)) mkdir($dir, 0775, true);
  $name = uniqid('cv_', true) . '.' . $ext;
  move_uploaded_file($file['tmp_name'], $dir . '/' . $name);
  return $urlBase . '/' . $name;
}

function careers_handle($parts, $method, PDO $pdo, $config)
{
  if (($parts[0] ?? '') !== 'jobs' && ($parts[0] ?? '') !== 'job-applications') {
    return false;
  }
  careers_ensure($pdo);

  if (($parts[0] ?? '') === 'jobs' && $method === 'GET' && !isset($parts[1])) {
    $rows = $pdo->query('SELECT * FROM jobs WHERE published = 1 ORDER BY id DESC')->fetchAll(PDO::FETCH_ASSOC);
    send(['jobs' => array_map('careers_map_job', $rows)]);
  }

  if (($parts[0] ?? '') === 'jobs' && ($parts[1] ?? '') === 'manage' && $method === 'GET') {
    require_perm($pdo, 'career');
    $rows = $pdo->query('SELECT * FROM jobs ORDER BY id DESC')->fetchAll(PDO::FETCH_ASSOC);
    send(['jobs' => array_map('careers_map_job', $rows)]);
  }

  if (($parts[0] ?? '') === 'jobs' && $method === 'GET' && isset($parts[1]) && !isset($parts[2])) {
    $stmt = $pdo->prepare('SELECT * FROM jobs WHERE id = ?');
    $stmt->execute([(int) $parts[1]]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    if (!$row || (!(int) $row['published'] && !bearer())) send(['error' => 'Job not found'], 404);
    send(['job' => careers_map_job($row)]);
  }

  if (($parts[0] ?? '') === 'jobs' && $method === 'POST' && !isset($parts[1])) {
    require_perm($pdo, 'career');
    $body = (!empty($_POST) || !empty($_FILES)) ? $_POST : json_body();
    $id = (int) ($body['id'] ?? 0);
    $published = (isset($body['published']) && ($body['published'] === false || $body['published'] === 'false' || $body['published'] === '0' || $body['published'] === 0)) ? 0 : 1;
    $fields = [
      trim((string) ($body['title'] ?? '')),
      trim((string) ($body['titleBn'] ?? $body['title_bn'] ?? '')),
      trim((string) ($body['department'] ?? '')),
      trim((string) ($body['location'] ?? '')),
      trim((string) ($body['jobType'] ?? $body['job_type'] ?? 'Full-time')),
      trim((string) ($body['deadline'] ?? '')),
      (string) ($body['description'] ?? ''),
      (string) ($body['descriptionBn'] ?? $body['description_bn'] ?? ''),
      $published,
    ];
    if ($fields[0] === '') send(['error' => 'Job title is required.'], 400);
    $image = trim((string) ($body['image'] ?? ''));
    if (!empty($_FILES['jobCover']['tmp_name'])) {
      $image = save_upload($_FILES['jobCover'], dirname($config['upload_dir']) . '/jobs', '/uploads/jobs');
    } elseif ($id) {
      $prev = $pdo->prepare('SELECT image FROM jobs WHERE id = ?');
      $prev->execute([$id]);
      $image = $prev->fetchColumn() ?: $image;
    }
    if ($id) {
      $pdo->prepare('UPDATE jobs SET title=?, title_bn=?, department=?, location=?, job_type=?, deadline=?, description=?, description_bn=?, published=?, image=? WHERE id=?')
        ->execute([...$fields, $image, $id]);
    } else {
      $pdo->prepare('INSERT INTO jobs (title, title_bn, department, location, job_type, deadline, description, description_bn, published, image) VALUES (?,?,?,?,?,?,?,?,?,?)')
        ->execute([...$fields, $image]);
      $id = (int) $pdo->lastInsertId();
    }
    $row = $pdo->prepare('SELECT * FROM jobs WHERE id = ?');
    $row->execute([$id]);
    send(['ok' => true, 'job' => careers_map_job($row->fetch(PDO::FETCH_ASSOC))]);
  }

  if (($parts[0] ?? '') === 'jobs' && $method === 'DELETE' && isset($parts[1])) {
    require_perm($pdo, 'career');
    $pdo->prepare('DELETE FROM job_applications WHERE job_id = ?')->execute([(int) $parts[1]]);
    $pdo->prepare('DELETE FROM jobs WHERE id = ?')->execute([(int) $parts[1]]);
    send(['ok' => true]);
  }

  if (($parts[0] ?? '') === 'jobs' && ($parts[2] ?? '') === 'apply' && $method === 'POST') {
    $jobId = (int) $parts[1];
    $job = $pdo->prepare('SELECT id FROM jobs WHERE id = ? AND published = 1');
    $job->execute([$jobId]);
    if (!$job->fetch()) send(['error' => 'Job not found'], 404);
    $name = trim((string) ($_POST['name'] ?? ''));
    $phone = trim((string) ($_POST['phone'] ?? ''));
    if ($name === '' || $phone === '') send(['error' => 'Name and phone are required.'], 400);
    $cv = '';
    if (!empty($_FILES['jobCv']['tmp_name'])) {
      $cv = careers_save_cv($_FILES['jobCv'], dirname($config['upload_dir']) . '/careers', '/uploads/careers', $config);
    }
    if ($cv === '') send(['error' => 'CV file is required.'], 400);
    $pdo->prepare('INSERT INTO job_applications (job_id, name, phone, email, address, education, experience, cover_letter, cv) VALUES (?,?,?,?,?,?,?,?,?)')
      ->execute([
        $jobId, $name, $phone,
        trim((string) ($_POST['email'] ?? '')),
        trim((string) ($_POST['address'] ?? '')),
        trim((string) ($_POST['education'] ?? '')),
        trim((string) ($_POST['experience'] ?? '')),
        trim((string) ($_POST['coverLetter'] ?? $_POST['cover_letter'] ?? '')),
        $cv,
      ]);
    send(['ok' => true]);
  }

  if (($parts[0] ?? '') === 'job-applications' && $method === 'GET' && !isset($parts[1])) {
    require_perm($pdo, 'career');
    $sql = 'SELECT a.*, j.title AS job_title FROM job_applications a LEFT JOIN jobs j ON j.id = a.job_id WHERE 1=1';
    $args = [];
    if (!empty($_GET['jobId'])) {
      $sql .= ' AND a.job_id = ?';
      $args[] = (int) $_GET['jobId'];
    }
    if (!empty($_GET['status'])) {
      $sql .= ' AND a.status = ?';
      $args[] = $_GET['status'];
    }
    $sql .= ' ORDER BY a.id DESC';
    $stmt = $pdo->prepare($sql);
    $stmt->execute($args);
    send(['applications' => array_map('careers_map_app', $stmt->fetchAll(PDO::FETCH_ASSOC))]);
  }

  if (($parts[0] ?? '') === 'job-applications' && ($parts[2] ?? '') === 'status' && $method === 'POST') {
    require_perm($pdo, 'career');
    $body = json_body();
    $status = trim((string) ($body['status'] ?? ''));
    if (!in_array($status, ['pending', 'shortlisted', 'rejected', 'confirmed'], true)) {
      send(['error' => 'Invalid status.'], 400);
    }
    $pdo->prepare('UPDATE job_applications SET status = ? WHERE id = ?')->execute([$status, (int) $parts[1]]);
    send(['ok' => true]);
  }

  if (($parts[0] ?? '') === 'job-applications' && ($parts[2] ?? '') === 'cv' && $method === 'GET') {
    require_perm($pdo, 'career');
    $stmt = $pdo->prepare('SELECT cv, name FROM job_applications WHERE id = ?');
    $stmt->execute([(int) $parts[1]]);
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    if (!$row || !$row['cv']) send(['error' => 'CV not found'], 404);
    $rel = ltrim($row['cv'], '/');
    $path = dirname(dirname(__DIR__)) . '/' . $rel;
    if (!is_file($path)) {
      $path = dirname($config['upload_dir']) . '/careers/' . basename($row['cv']);
    }
    if (!is_file($path)) send(['error' => 'CV file missing'], 404);
    header('Content-Type: application/octet-stream');
    header('Content-Disposition: attachment; filename="' . basename($row['cv']) . '"');
    readfile($path);
    exit;
  }

  return false;
}
