<?php

function admissions_ensure_table(PDO $pdo)
{
  $pdo->exec('CREATE TABLE IF NOT EXISTS admissions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    status VARCHAR(20) NOT NULL DEFAULT \'pending\',
    students_name VARCHAR(160) NOT NULL,
    sms_phone VARCHAR(20) NOT NULL,
    gender VARCHAR(20) DEFAULT \'Male\',
    fathers_name VARCHAR(160) NOT NULL,
    education_year_id INT NOT NULL,
    education_year VARCHAR(80) DEFAULT \'\',
    class_id INT NOT NULL,
    class_name VARCHAR(80) DEFAULT \'\',
    group_id INT DEFAULT 0,
    group_name VARCHAR(80) DEFAULT \'\',
    section_id INT DEFAULT 0,
    section_name VARCHAR(80) DEFAULT \'\',
    shift_id INT DEFAULT 0,
    shift_name VARCHAR(80) DEFAULT \'\',
    roll_no VARCHAR(40) DEFAULT \'\',
    student_email VARCHAR(160) DEFAULT \'\',
    date_of_birth VARCHAR(20) DEFAULT \'\',
    legal_identity VARCHAR(80) DEFAULT \'\',
    blood_group VARCHAR(20) DEFAULT \'\',
    religion VARCHAR(40) DEFAULT \'\',
    permanent_address TEXT,
    local_address TEXT,
    photo VARCHAR(255) DEFAULT \'\',
    mothers_name VARCHAR(160) DEFAULT \'\',
    mother_phone VARCHAR(20) DEFAULT \'\',
    mother_occupation VARCHAR(120) DEFAULT \'\',
    father_phone VARCHAR(20) DEFAULT \'\',
    father_occupation VARCHAR(120) DEFAULT \'\',
    guardian_name VARCHAR(160) DEFAULT \'\',
    guardian_relation VARCHAR(80) DEFAULT \'\',
    guardian_phone VARCHAR(20) DEFAULT \'\',
    prev_school VARCHAR(200) DEFAULT \'\',
    prev_class VARCHAR(80) DEFAULT \'\',
    prev_exam_year VARCHAR(20) DEFAULT \'\',
    prev_grade VARCHAR(40) DEFAULT \'\',
    subjects_json TEXT,
    assigned_id VARCHAR(40) DEFAULT \'\',
    sikkhaloy_student_id INT DEFAULT NULL,
    confirm_error TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    confirmed_at DATETIME NULL
  ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci');
  try {
    $pdo->exec('ALTER TABLE admissions ADD COLUMN payments_json TEXT');
  } catch (Throwable $e) {
    /* already present */
  }
}

function admissions_pick($source, array $keys, $default = '')
{
  foreach ($keys as $key) {
    if (isset($source[$key]) && trim((string) $source[$key]) !== '') {
      return trim((string) $source[$key]);
    }
  }
  return $default;
}

function admissions_int($source, array $keys)
{
  $value = admissions_pick($source, $keys, '0');
  $n = (int) $value;
  return $n > 0 ? $n : 0;
}

function admissions_subjects($raw)
{
  if (is_array($raw)) {
    return $raw;
  }
  if (!is_string($raw) || trim($raw) === '') {
    return [];
  }
  $parsed = json_decode($raw, true);
  return is_array($parsed) ? $parsed : [];
}

function admissions_map_row(array $row)
{
  return [
    'id' => (int) $row['id'],
    'status' => $row['status'],
    'studentsName' => $row['students_name'],
    'smsPhone' => $row['sms_phone'],
    'gender' => $row['gender'],
    'fathersName' => $row['fathers_name'],
    'educationYearId' => (int) $row['education_year_id'],
    'educationYear' => $row['education_year'],
    'classId' => (int) $row['class_id'],
    'className' => $row['class_name'],
    'groupId' => (int) $row['group_id'],
    'groupName' => $row['group_name'],
    'sectionId' => (int) $row['section_id'],
    'sectionName' => $row['section_name'],
    'shiftId' => (int) $row['shift_id'],
    'shiftName' => $row['shift_name'],
    'rollNo' => $row['roll_no'],
    'studentEmail' => $row['student_email'],
    'dateOfBirth' => $row['date_of_birth'],
    'legalIdentity' => $row['legal_identity'],
    'bloodGroup' => $row['blood_group'],
    'religion' => $row['religion'],
    'permanentAddress' => $row['permanent_address'],
    'localAddress' => $row['local_address'],
    'photo' => public_url($row['photo']),
    'mothersName' => $row['mothers_name'],
    'motherPhone' => $row['mother_phone'],
    'motherOccupation' => $row['mother_occupation'],
    'fatherPhone' => $row['father_phone'],
    'fatherOccupation' => $row['father_occupation'],
    'guardianName' => $row['guardian_name'],
    'guardianRelation' => $row['guardian_relation'],
    'guardianPhone' => $row['guardian_phone'],
    'prevSchool' => $row['prev_school'],
    'prevClass' => $row['prev_class'],
    'prevExamYear' => $row['prev_exam_year'],
    'prevGrade' => $row['prev_grade'],
    'subjects' => admissions_subjects($row['subjects_json'] ?? ''),
    'payments' => admissions_subjects($row['payments_json'] ?? ''),
    'assignedId' => $row['assigned_id'],
    'sikkhaloyStudentId' => $row['sikkhaloy_student_id'] ? (int) $row['sikkhaloy_student_id'] : null,
    'confirmError' => $row['confirm_error'],
    'createdAt' => $row['created_at'],
    'confirmedAt' => $row['confirmed_at'],
  ];
}

function admissions_input(array $body)
{
  return [
    'studentsName' => admissions_pick($body, ['studentsName', 'StudentsName']),
    'smsPhone' => preg_replace('/\D+/', '', admissions_pick($body, ['smsPhone', 'SMSPhoneNo', 'smsPhoneNo'])),
    'gender' => admissions_pick($body, ['gender', 'Gender'], 'Male'),
    'fathersName' => admissions_pick($body, ['fathersName', 'FathersName']),
    'educationYearId' => admissions_int($body, ['educationYearId', 'EducationYearID']),
    'educationYear' => admissions_pick($body, ['educationYear', 'EducationYear']),
    'classId' => admissions_int($body, ['classId', 'ClassID']),
    'className' => admissions_pick($body, ['className', 'ClassName']),
    'groupId' => admissions_int($body, ['groupId', 'SubjectGroupID']),
    'groupName' => admissions_pick($body, ['groupName', 'GroupName']),
    'sectionId' => admissions_int($body, ['sectionId', 'SectionID']),
    'sectionName' => admissions_pick($body, ['sectionName', 'SectionName']),
    'shiftId' => admissions_int($body, ['shiftId', 'ShiftID']),
    'shiftName' => admissions_pick($body, ['shiftName', 'ShiftName']),
    'rollNo' => admissions_pick($body, ['rollNo', 'RollNo']),
    'studentEmail' => admissions_pick($body, ['studentEmail', 'StudentEmailAddress']),
    'dateOfBirth' => admissions_pick($body, ['dateOfBirth', 'DateofBirth']),
    'legalIdentity' => admissions_pick($body, ['legalIdentity', 'LegalIdentity']),
    'bloodGroup' => admissions_pick($body, ['bloodGroup', 'BloodGroup']),
    'religion' => admissions_pick($body, ['religion', 'Religion']),
    'permanentAddress' => admissions_pick($body, ['permanentAddress', 'StudentPermanentAddress']),
    'localAddress' => admissions_pick($body, ['localAddress', 'StudentsLocalAddress']),
    'mothersName' => admissions_pick($body, ['mothersName', 'MothersName']),
    'motherPhone' => admissions_pick($body, ['motherPhone', 'MotherPhoneNumber']),
    'motherOccupation' => admissions_pick($body, ['motherOccupation', 'MotherOccupation']),
    'fatherPhone' => admissions_pick($body, ['fatherPhone', 'FatherPhoneNumber']),
    'fatherOccupation' => admissions_pick($body, ['fatherOccupation', 'FatherOccupation']),
    'guardianName' => admissions_pick($body, ['guardianName', 'GuardianName']),
    'guardianRelation' => admissions_pick($body, ['guardianRelation', 'GuardianRelationshipwithStudent']),
    'guardianPhone' => admissions_pick($body, ['guardianPhone', 'GuardianPhoneNumber']),
    'prevSchool' => admissions_pick($body, ['prevSchool', 'PrevSchoolName']),
    'prevClass' => admissions_pick($body, ['prevClass', 'PrevClass']),
    'prevExamYear' => admissions_pick($body, ['prevExamYear', 'PrevExamYear']),
    'prevGrade' => admissions_pick($body, ['prevGrade', 'PrevExamGrade']),
    'subjects' => admissions_subjects($body['subjects'] ?? $body['Subjects'] ?? []),
  ];
}

function admissions_photo_data($photo)
{
  if (!$photo) {
    return '';
  }
  if (strpos($photo, 'data:') === 0) {
    return $photo;
  }
  $rel = ltrim((string) $photo, '/');
  $abs = dirname(__DIR__) . '/' . $rel;
  if (!is_file($abs)) {
    return '';
  }
  $ext = strtolower(pathinfo($abs, PATHINFO_EXTENSION));
  $mime = $ext === 'png' ? 'image/png' : ($ext === 'webp' ? 'image/webp' : 'image/jpeg');
  return 'data:' . $mime . ';base64,' . base64_encode((string) file_get_contents($abs));
}

function admissions_admit_payload(array $row)
{
  $subjects = [];
  foreach ($row['subjects'] ?? [] as $item) {
    $subjects[] = [
      'subjectID' => (int) ($item['subjectID'] ?? $item['SubjectID'] ?? 0),
      'subjectType' => $item['subjectType'] ?? $item['SubjectType'] ?? 'Compulsory',
    ];
  }
  return [
    'studentsName' => $row['studentsName'],
    'smsPhoneNo' => $row['smsPhone'],
    'gender' => $row['gender'],
    'fathersName' => $row['fathersName'],
    'educationYearID' => $row['educationYearId'],
    'classID' => $row['classId'],
    'sectionID' => $row['sectionId'] ?: 0,
    'shiftID' => $row['shiftId'] ?: 0,
    'subjectGroupID' => $row['groupId'] ?: 0,
    'rollNo' => $row['rollNo'],
    'studentEmailAddress' => $row['studentEmail'],
    'dateofBirth' => $row['dateOfBirth'],
    'legalIdentity' => $row['legalIdentity'],
    'bloodGroup' => $row['bloodGroup'],
    'religion' => $row['religion'],
    'studentsLocalAddress' => $row['localAddress'],
    'studentPermanentAddress' => $row['permanentAddress'],
    'mothersName' => $row['mothersName'],
    'motherOccupation' => $row['motherOccupation'],
    'motherPhoneNumber' => $row['motherPhone'],
    'fatherOccupation' => $row['fatherOccupation'],
    'fatherPhoneNumber' => $row['fatherPhone'],
    'guardianName' => $row['guardianName'],
    'guardianRelationshipwithStudent' => $row['guardianRelation'],
    'guardianPhoneNumber' => $row['guardianPhone'],
    'prevSchoolName' => $row['prevSchool'],
    'prevClass' => $row['prevClass'],
    'prevExamYear' => $row['prevExamYear'],
    'prevExamGrade' => $row['prevGrade'],
    'photoDataUrl' => admissions_photo_data($row['photo'] ?? ''),
    'subjects' => $subjects,
    'sendSms' => !empty($row['sendSms']) || !isset($row['sendSms']),
    'payments' => admissions_map_payments($row['payments'] ?? []),
  ];
}

function admissions_map_payments($raw)
{
  $out = [];
  foreach (admissions_subjects($raw) as $item) {
    $out[] = [
      'roleID' => (int) ($item['roleID'] ?? $item['RoleID'] ?? 0),
      'role' => $item['role'] ?? $item['Role'] ?? '',
      'payFor' => $item['payFor'] ?? $item['PayFor'] ?? '',
      'amount' => (float) ($item['amount'] ?? $item['Amount'] ?? 0),
      'discount' => (float) ($item['discount'] ?? $item['Discount'] ?? 0),
      'assignRoleID' => (int) ($item['assignRoleID'] ?? $item['AssignRoleID'] ?? 0),
      'lateFee' => (float) ($item['lateFee'] ?? $item['LateFee'] ?? 0),
      'startDate' => $item['startDate'] ?? $item['StartDate'] ?? null,
      'endDate' => $item['endDate'] ?? $item['EndDate'] ?? null,
    ];
  }
  return $out;
}

function admissions_http_json($url, $method = 'GET', $payload = null, $headers = [])
{
  $body = $payload === null ? null : json_encode($payload, JSON_UNESCAPED_UNICODE);
  if (function_exists('curl_init')) {
    $ch = curl_init($url);
    $hdrs = ['Accept: application/json'];
    if ($body !== null) {
      $hdrs[] = 'Content-Type: application/json';
    }
    foreach ($headers as $header) {
      $hdrs[] = $header;
    }
    curl_setopt_array($ch, [
      CURLOPT_RETURNTRANSFER => true,
      CURLOPT_FOLLOWLOCATION => true,
      CURLOPT_TIMEOUT => 30,
      CURLOPT_CUSTOMREQUEST => $method,
      CURLOPT_HTTPHEADER => $hdrs,
    ]);
    if ($body !== null) {
      curl_setopt($ch, CURLOPT_POSTFIELDS, $body);
    }
    $raw = curl_exec($ch);
    $code = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    if ($raw === false) {
      return [null, 0];
    }
    return [json_decode($raw, true), $code];
  }
  $opts = [
    'http' => [
      'method' => $method,
      'timeout' => 30,
      'ignore_errors' => true,
      'header' => implode("\r\n", array_merge(['Accept: application/json'], $headers)) . "\r\n",
    ],
  ];
  if ($body !== null) {
    $opts['http']['header'] .= "Content-Type: application/json\r\n";
    $opts['http']['content'] = $body;
  }
  $raw = @file_get_contents($url, false, stream_context_create($opts));
  return [$raw === false ? null : json_decode($raw, true), 200];
}

function admissions_fetch_first(array $bases, $path)
{
  foreach ($bases as $base) {
    [$data, $code] = admissions_http_json(rtrim($base, '/') . $path);
    if (is_array($data) && $code < 400) {
      return $data;
    }
  }
  return null;
}

function admissions_map_form(array $data)
{
  $sessions = [];
  foreach ($data['sessions'] ?? $data['Sessions'] ?? [] as $row) {
    $sessions[] = [
      'id' => (int) ($row['id'] ?? $row['Id'] ?? 0),
      'name' => $row['name'] ?? $row['Name'] ?? '',
    ];
  }
  $classes = [];
  foreach ($data['classes'] ?? $data['Classes'] ?? [] as $row) {
    $classes[] = [
      'id' => (int) ($row['id'] ?? $row['Id'] ?? 0),
      'name' => $row['name'] ?? $row['Name'] ?? '',
    ];
  }
  return [
    'lastStudentId' => $data['lastStudentId'] ?? $data['LastStudentId'] ?? '',
    'activeEducationYearId' => (int) ($data['activeEducationYearId'] ?? $data['ActiveEducationYearId'] ?? 0),
    'sessions' => $sessions,
    'classes' => $classes,
    'feeRoles' => array_map(function ($row) {
      return [
        'roleID' => (int) ($row['roleID'] ?? $row['RoleID'] ?? 0),
        'role' => $row['role'] ?? $row['Role'] ?? '',
        'numberOfPay' => (int) ($row['numberOfPay'] ?? $row['NumberOfPay'] ?? 0),
      ];
    }, $data['feeRoles'] ?? $data['FeeRoles'] ?? []),
  ];
}

function admissions_map_options(array $data)
{
  $lookup = function ($rows) {
    $out = [];
    foreach (is_array($rows) ? $rows : [] as $row) {
      $out[] = [
        'id' => (int) ($row['id'] ?? $row['Id'] ?? 0),
        'name' => $row['name'] ?? $row['Name'] ?? '',
      ];
    }
    return $out;
  };
  $subjects = [];
  foreach ($data['subjects'] ?? $data['Subjects'] ?? [] as $row) {
    $subjects[] = [
      'subjectID' => (int) ($row['subjectID'] ?? $row['SubjectID'] ?? 0),
      'subjectName' => $row['subjectName'] ?? $row['SubjectName'] ?? '',
      'subjectType' => $row['subjectType'] ?? $row['SubjectType'] ?? 'Compulsory',
      'selected' => !empty($row['selected'] ?? $row['Selected']),
    ];
  }
  return [
    'lastRollNo' => $data['lastRollNo'] ?? $data['LastRollNo'] ?? '',
    'groups' => $lookup($data['groups'] ?? $data['Groups'] ?? []),
    'sections' => $lookup($data['sections'] ?? $data['Sections'] ?? []),
    'shifts' => $lookup($data['shifts'] ?? $data['Shifts'] ?? []),
    'subjects' => $subjects,
    'fees' => admissions_map_payments($data['fees'] ?? $data['Fees'] ?? []),
  ];
}

function admissions_handle(array $parts, $method, PDO $pdo, array $config)
{
  if (($parts[0] ?? '') !== 'admissions') {
    return false;
  }

  admissions_ensure_table($pdo);
  $edu = edu_settings($config);
  $schoolId = $edu['school_id'];
  $bases = $edu['bases'];

  if (($parts[1] ?? '') === 'form' && $method === 'GET') {
    $data = admissions_fetch_first($bases, '/api/public/website/' . $schoolId . '/admission');
    if (!$data) {
      send(['error' => 'Sikkhaloy admission form is not reachable'], 502);
    }
    send(admissions_map_form($data));
  }

  if (($parts[1] ?? '') === 'options' && $method === 'GET') {
    $classId = (int) ($_GET['classId'] ?? 0);
    if ($classId <= 0) {
      send(['error' => 'Class is required'], 400);
    }
    $qs = http_build_query([
      'classId' => $classId,
      'educationYearId' => (int) ($_GET['educationYearId'] ?? 0),
      'groupId' => (int) ($_GET['groupId'] ?? 0),
      'sectionId' => (int) ($_GET['sectionId'] ?? 0),
      'shiftId' => (int) ($_GET['shiftId'] ?? 0),
    ]);
    $data = admissions_fetch_first($bases, '/api/public/website/' . $schoolId . '/admission/options?' . $qs);
    if (!$data) {
      send(['error' => 'Sikkhaloy class options are not reachable'], 502);
    }
    send(admissions_map_options($data));
  }

  if ($method === 'POST' && !isset($parts[1])) {
    $body = array_merge(json_body(), $_POST);
    $item = admissions_input($body);
    if ($item['studentsName'] === '' || $item['fathersName'] === '') {
      send(['error' => 'Student name and father\'s name are required.'], 400);
    }
    if (!preg_match('/^01[3-9]\d{8}$/', $item['smsPhone'])) {
      send(['error' => 'SMS mobile must be 11 digits (01XXXXXXXXX).'], 400);
    }
    if ($item['dateOfBirth'] === '') {
      send(['error' => 'Date of birth is required.'], 400);
    }
    if ($item['permanentAddress'] === '') {
      send(['error' => 'Permanent address is required.'], 400);
    }
    if (!preg_match('/^01[3-9]\d{8}$/', $item['fatherPhone']) || !preg_match('/^01[3-9]\d{8}$/', $item['motherPhone'])) {
      send(['error' => 'Father and mother mobile must be 11 digits (01XXXXXXXXX).'], 400);
    }
    if ($item['educationYearId'] <= 0 || $item['classId'] <= 0) {
      send(['error' => 'Session year and class are required.'], 400);
    }
    $photo = '';
    if (!empty($_FILES['studentPhoto']['tmp_name'])) {
      $photo = save_upload($_FILES['studentPhoto'], dirname($config['upload_dir']) . '/admissions', '/uploads/admissions');
    }
    $stmt = $pdo->prepare('INSERT INTO admissions (
      students_name, sms_phone, gender, fathers_name, education_year_id, education_year,
      class_id, class_name, group_id, group_name, section_id, section_name, shift_id, shift_name,
      roll_no, student_email, date_of_birth, legal_identity, blood_group, religion,
      permanent_address, local_address, photo, mothers_name, mother_phone, mother_occupation,
      father_phone, father_occupation, guardian_name, guardian_relation, guardian_phone,
      prev_school, prev_class, prev_exam_year, prev_grade, subjects_json
    ) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)');
    $stmt->execute([
      $item['studentsName'], $item['smsPhone'], $item['gender'], $item['fathersName'],
      $item['educationYearId'], $item['educationYear'], $item['classId'], $item['className'],
      $item['groupId'], $item['groupName'], $item['sectionId'], $item['sectionName'],
      $item['shiftId'], $item['shiftName'], $item['rollNo'], $item['studentEmail'],
      $item['dateOfBirth'], $item['legalIdentity'], $item['bloodGroup'], $item['religion'],
      $item['permanentAddress'], $item['localAddress'], $photo, $item['mothersName'],
      $item['motherPhone'], $item['motherOccupation'], $item['fatherPhone'], $item['fatherOccupation'],
      $item['guardianName'], $item['guardianRelation'], $item['guardianPhone'],
      $item['prevSchool'], $item['prevClass'], $item['prevExamYear'], $item['prevGrade'],
      json_encode($item['subjects'], JSON_UNESCAPED_UNICODE),
    ]);
    send(['ok' => true, 'id' => (int) $pdo->lastInsertId()]);
  }

  if ($method === 'GET' && !isset($parts[1])) {
    require_perm($pdo, 'admissions');
    $rows = $pdo->query('SELECT * FROM admissions ORDER BY created_at DESC, id DESC')->fetchAll(PDO::FETCH_ASSOC);
    send(['admissions' => array_map('admissions_map_row', $rows)]);
  }

  if ($method === 'DELETE' && isset($parts[1]) && !isset($parts[2])) {
    require_perm($pdo, 'admissions');
    $pdo->prepare('DELETE FROM admissions WHERE id = ?')->execute([(int) $parts[1]]);
    send(['ok' => true]);
  }

  if ($method === 'POST' && isset($parts[1]) && ($parts[2] ?? '') === 'confirm') {
    require_perm($pdo, 'admissions');
    $id = (int) $parts[1];
    $stmt = $pdo->prepare('SELECT * FROM admissions WHERE id = ?');
    $stmt->execute([$id]);
    $found = $stmt->fetch(PDO::FETCH_ASSOC);
    if (!$found) {
      send(['error' => 'Application not found'], 404);
    }
    $mapped = admissions_map_row($found);
    $body = array_merge(json_body(), $_POST);
    $mapped['payments'] = admissions_map_payments($body['payments'] ?? $body['Payments'] ?? []);
    $mapped['sendSms'] = !isset($body['sendSms']) && !isset($body['SendSms'])
      ? true
      : !empty($body['sendSms'] ?? $body['SendSms']);
    if (($found['status'] ?? '') === 'confirmed' && !empty($found['sikkhaloy_student_id'])) {
      if (!$mapped['payments']) {
        send(['ok' => true, 'admission' => $mapped]);
      }
      $payPayload = [
        'studentID' => (int) $found['sikkhaloy_student_id'],
        'educationYearID' => (int) $found['education_year_id'],
        'classID' => (int) $found['class_id'],
        'payments' => $mapped['payments'],
      ];
      $payResult = null;
      $payError = 'Sikkhaloy payment API is not reachable';
      $key = (string) ($config['sikkhaloy_website_key'] ?? getenv('SIKKHALOY_WEBSITE_KEY') ?: '');
      $headers = $key !== '' ? ['X-Website-Key: ' . $key] : [];
      foreach ($bases as $base) {
        [$data, $code] = admissions_http_json(rtrim($base, '/') . '/api/public/website/' . $schoolId . '/admit-payments', 'POST', $payPayload, $headers);
        if (is_array($data) && $code < 400 && !empty($data['succeeded'] ?? $data['Succeeded'])) {
          $payResult = $data;
          break;
        }
        if (is_array($data)) {
          $payError = $data['error'] ?? $data['Error'] ?? $payError;
        }
      }
      if (!$payResult) {
        $pdo->prepare('UPDATE admissions SET confirm_error = ? WHERE id = ?')->execute([$payError, $id]);
        send(['error' => $payError], 502);
      }
      $stored = admissions_map_payments($payResult['payments'] ?? $payResult['Payments'] ?? $mapped['payments']);
      $pdo->prepare('UPDATE admissions SET payments_json = ?, confirm_error = NULL WHERE id = ?')
        ->execute([json_encode($stored, JSON_UNESCAPED_UNICODE), $id]);
      $stmt->execute([$id]);
      send(['ok' => true, 'admission' => admissions_map_row($stmt->fetch(PDO::FETCH_ASSOC)), 'result' => $payResult]);
    }
    $payload = admissions_admit_payload($mapped);
    $key = (string) ($config['sikkhaloy_website_key'] ?? getenv('SIKKHALOY_WEBSITE_KEY') ?: '');
    $headers = $key !== '' ? ['X-Website-Key: ' . $key] : [];
    $result = null;
    $error = 'Sikkhaloy admit API is not reachable';
    foreach ($bases as $base) {
      [$data, $code] = admissions_http_json(rtrim($base, '/') . '/api/public/website/' . $schoolId . '/admit', 'POST', $payload, $headers);
      if (is_array($data) && $code < 400 && !empty($data['succeeded'] ?? $data['Succeeded'])) {
        $result = $data;
        break;
      }
      if (is_array($data)) {
        $error = $data['error'] ?? $data['Error'] ?? $error;
      }
    }
    if (!$result) {
      $pdo->prepare('UPDATE admissions SET confirm_error = ? WHERE id = ?')->execute([$error, $id]);
      send(['error' => $error], 502);
    }
    $assigned = $result['studentCode'] ?? $result['StudentCode'] ?? '';
    $studentId = (int) ($result['studentID'] ?? $result['StudentID'] ?? 0);
    $stored = admissions_map_payments($result['payments'] ?? $result['Payments'] ?? $mapped['payments']);
    $pdo->prepare('UPDATE admissions SET status = \'confirmed\', assigned_id = ?, sikkhaloy_student_id = ?, payments_json = ?, confirm_error = NULL, confirmed_at = NOW() WHERE id = ?')
      ->execute([$assigned, $studentId ?: null, json_encode($stored, JSON_UNESCAPED_UNICODE), $id]);
    $stmt->execute([$id]);
    send(['ok' => true, 'admission' => admissions_map_row($stmt->fetch(PDO::FETCH_ASSOC)), 'result' => $result]);
  }

  return false;
}
