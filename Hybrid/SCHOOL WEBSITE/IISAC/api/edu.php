<?php

function edu_settings($config)
{
  $api = rtrim((string) ($config['sikkhaloy_api'] ?? getenv('SIKKHALOY_API') ?: 'https://hybridapi.sikkhaloy.com'), '/');
  $local = rtrim((string) ($config['sikkhaloy_api_local'] ?? getenv('SIKKHALOY_API_LOCAL') ?: ''), '/');
  $bases = [];
  foreach ([$api, $local] as $base) {
    if ($base !== '' && !in_array($base, $bases, true)) {
      $bases[] = $base;
    }
  }
  return [
    'api' => $api,
    'bases' => $bases,
    'school_id' => (int) ($config['school_id'] ?? getenv('SCHOOL_ID') ?: 1),
    'website_key' => (string) ($config['sikkhaloy_website_key'] ?? getenv('SIKKHALOY_WEBSITE_KEY') ?: ''),
  ];
}

function edu_local_url($path)
{
  $path = '/' . ltrim((string) $path, '/');
  return public_url($path);
}

function edu_rewrite_person($person, $kind)
{
  if (!is_array($person)) {
    return $person;
  }
  $id = (int) ($person['id'] ?? $person['Id'] ?? 0);
  $photo = $person['photoUrl'] ?? $person['PhotoUrl'] ?? '';
  $role = strtolower((string) ($person['kind'] ?? $person['Kind'] ?? $kind ?: 'student'));
  return [
    'id' => $id,
    'name' => $person['name'] ?? $person['Name'] ?? '',
    'title' => $person['title'] ?? $person['Title'] ?? '',
    'meta' => $person['meta'] ?? $person['Meta'] ?? '',
    'kind' => $role,
    'photoUrl' => $photo ? edu_local_url('/api/edu/' . $role . '-photo/' . $id) : '',
  ];
}

function edu_http_get($url, $websiteKey = '')
{
  $headers = ["Accept: */*"];
  if ($websiteKey !== '') {
    $headers[] = 'X-Website-Key: ' . $websiteKey;
  }
  if (function_exists('curl_init')) {
    $ch = curl_init($url);
    curl_setopt_array($ch, [
      CURLOPT_RETURNTRANSFER => true,
      CURLOPT_FOLLOWLOCATION => true,
      CURLOPT_TIMEOUT => 12,
      CURLOPT_SSL_VERIFYPEER => true,
      CURLOPT_HTTPHEADER => $headers,
    ]);
    $raw = curl_exec($ch);
    $code = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    if ($raw === false || $code >= 400) {
      return null;
    }
    return $raw;
  }
  $ctx = stream_context_create([
    'http' => [
      'method' => 'GET',
      'timeout' => 12,
      'ignore_errors' => true,
      'header' => implode("\r\n", $headers) . "\r\n",
    ],
  ]);
  $raw = @file_get_contents($url, false, $ctx);
  return $raw === false ? null : $raw;
}


function edu_fetch_json($url, $websiteKey = '')
{
  $raw = edu_http_get($url, $websiteKey);
  if ($raw === null) {
    return [null, 'Sikkhaloy API is not reachable'];
  }
  $data = json_decode($raw, true);
  return [is_array($data) ? $data : null, null];
}

function edu_stream_image($url, $websiteKey = '')
{
  $raw = edu_http_get($url, $websiteKey);
  if ($raw === null || $raw === '') {
    send(['error' => 'Photo not found'], 404);
  }
  $mime = 'image/jpeg';
  if (strncmp($raw, "\x89PNG", 4) === 0) {
    $mime = 'image/png';
  } elseif (strncmp($raw, 'GIF', 3) === 0) {
    $mime = 'image/gif';
  }
  header('Content-Type: ' . $mime);
  header('Cache-Control: public, max-age=3600');
  echo $raw;
  exit;
}

function edu_handle(array $parts, $method, array $config)
{
  if (($parts[0] ?? '') !== 'edu' || $method !== 'GET') {
    return false;
  }

  $edu = edu_settings($config);
  $schoolId = $edu['school_id'];
  $api = $edu['api'];
  $websiteKey = (string) ($edu['website_key'] ?? '');
  if ($schoolId <= 0 || $api === '') {
    send(['error' => 'Set school_id and sikkhaloy_api in api/config.json'], 503);
  }
  if (function_exists('iisac_rate_limit')) {
    iisac_rate_limit('edu', 90, 60);
  }

  if (($parts[1] ?? '') === 'home') {
    $data = null;
    foreach ($edu['bases'] as $base) {
      foreach (['/api/public/website/', '/api/public/institutes/'] as $path) {
        [$try] = edu_fetch_json($base . $path . $schoolId, $websiteKey);
        if ($try && !empty($try['school'] ?? $try['School'] ?? $try['stats'] ?? $try['Stats'] ?? null)) {
          $data = $try;
          break 2;
        }
      }
    }
    if (!$data || empty($data['school'] ?? $data['School'] ?? null)) {
      [$list] = edu_fetch_json($api . '/api/public/institutes', $websiteKey);
      $found = null;
      foreach (is_array($list) ? $list : [] as $row) {
        if ((int) ($row['schoolID'] ?? $row['SchoolID'] ?? 0) === $schoolId) {
          $found = $row;
          break;
        }
      }
      if (!$found) {
        send(['error' => 'Institution not found on Sikkhaloy API'], 502);
      }
      $data = [
        'school' => [
          'schoolID' => (int) ($found['schoolID'] ?? $found['SchoolID'] ?? $schoolId),
          'schoolName' => $found['schoolName'] ?? $found['SchoolName'] ?? '',
          'hasLogo' => !empty($found['hasLogo'] ?? $found['HasLogo']),
        ],
      ];
    }
    $school = $data['school'] ?? $data['School'] ?? [];
    $stats = $data['stats'] ?? $data['Stats'] ?? [];
    $classes = $stats['classes'] ?? $stats['Classes'] ?? [];
    $mappedClasses = [];
    foreach (is_array($classes) ? $classes : [] as $row) {
      $mappedClasses[] = [
        'classID' => (int) ($row['classID'] ?? $row['ClassID'] ?? 0),
        'name' => $row['name'] ?? $row['Name'] ?? '',
        'count' => (int) ($row['count'] ?? $row['Count'] ?? 0),
      ];
    }
    $hasLogo = !empty($school['logoUrl'] ?? $school['LogoUrl'] ?? $school['hasLogo'] ?? $school['HasLogo']);
    send([
      'school' => [
        'schoolID' => (int) ($school['schoolID'] ?? $school['SchoolID'] ?? $schoolId),
        'schoolName' => $school['schoolName'] ?? $school['SchoolName'] ?? '',
        'dialog' => $school['dialog'] ?? $school['Dialog'] ?? '',
        'established' => $school['established'] ?? $school['Established'] ?? '',
        'principal' => $school['principal'] ?? $school['Principal'] ?? '',
        'address' => $school['address'] ?? $school['Address'] ?? '',
        'phone' => $school['phone'] ?? $school['Phone'] ?? '',
        'email' => $school['email'] ?? $school['Email'] ?? '',
        'website' => $school['website'] ?? $school['Website'] ?? '',
        // Always point at this install's school_id logo endpoint (not a hardcoded school).
        'logoUrl' => edu_local_url('/api/edu/logo'),
      ],
      'stats' => [
        'students' => (int) ($stats['students'] ?? $stats['Students'] ?? 0),
        'teachers' => (int) ($stats['teachers'] ?? $stats['Teachers'] ?? 0),
        'staff' => (int) ($stats['staff'] ?? $stats['Staff'] ?? 0),
        'educationYear' => $stats['educationYear'] ?? $stats['EducationYear'] ?? '',
        'classes' => $mappedClasses,
      ],
      'teachers' => array_map(function ($row) {
        return edu_rewrite_person($row, 'teacher');
      }, $data['teachers'] ?? $data['Teachers'] ?? []),
      'staff' => array_map(function ($row) {
        return edu_rewrite_person($row, 'staff');
      }, $data['staff'] ?? $data['Staff'] ?? []),
      'students' => array_map(function ($row) {
        return edu_rewrite_person($row, 'student');
      }, $data['students'] ?? $data['Students'] ?? []),
      'birthdays' => array_map(function ($row) {
        return edu_rewrite_person($row, $row['kind'] ?? $row['Kind'] ?? 'student');
      }, $data['birthdays'] ?? $data['Birthdays'] ?? []),
      'notices' => array_values(array_filter(array_map(function ($row) {
        $title = trim((string) ($row['title'] ?? $row['Title'] ?? ''));
        if ($title === '') {
          return null;
        }
        return [
          'id' => (int) ($row['id'] ?? $row['Id'] ?? 0),
          'title' => $title,
          'body' => trim((string) ($row['body'] ?? $row['Body'] ?? '')),
          'date' => $row['date'] ?? $row['Date'] ?? '',
        ];
      }, $data['notices'] ?? $data['Notices'] ?? []))),
    ]);
  }

  if (($parts[1] ?? '') === 'logo') {
    edu_stream_image($api . '/api/public/institutes/' . $schoolId . '/logo', $websiteKey);
  }

  if (($parts[1] ?? '') === 'student-photo' && isset($parts[2])) {
    edu_stream_image($api . '/api/public/website/' . $schoolId . '/students/' . (int) $parts[2] . '/photo', $websiteKey);
  }

  if (($parts[1] ?? '') === 'teacher-photo' && isset($parts[2])) {
    edu_stream_image($api . '/api/public/website/' . $schoolId . '/teachers/' . (int) $parts[2] . '/photo', $websiteKey);
  }

  if (($parts[1] ?? '') === 'staff-photo' && isset($parts[2])) {
    edu_stream_image($api . '/api/public/website/' . $schoolId . '/staff/' . (int) $parts[2] . '/photo', $websiteKey);
  }

  return false;
}
