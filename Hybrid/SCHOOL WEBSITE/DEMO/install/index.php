<?php
require __DIR__ . '/lib.php';
require dirname(__DIR__) . '/api/path.php';

if (safa_is_installed() && ($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'GET' && !isset($_GET['done'])) {
  $base = safa_web_base();
  header('Location: ' . ($base === '' ? '/' : $base . '/'));
  exit;
}

if (($_SERVER['REQUEST_METHOD'] ?? 'GET') === 'POST') {
  $input = safa_json_input();
  if (!$input) {
    $input = $_POST;
  }
  $action = $input['action'] ?? '';

  if ($action === 'requirements') {
    safa_send(safa_requirements());
  }

  if (safa_is_installed() && $action !== 'status') {
    safa_send(['ok' => false, 'error' => 'Already installed. Delete install.lock and api/config.json to reinstall.'], 403);
  }

  if ($action === 'test_db') {
    safa_send(safa_test_db($input));
  }

  if ($action === 'install') {
    safa_send(safa_install($input));
  }

  safa_send(['ok' => false, 'error' => 'Unknown action'], 400);
}

$installed = safa_is_installed();
?>
<!doctype html>
<html lang="bn">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>SAFA PROPERTIS Installer</title>
  <link rel="icon" type="image/svg+xml" href="../logo.svg" />
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Noto+Sans+Bengali:wght@400;500;600;700&family=Playfair+Display:wght@600;700&display=swap" rel="stylesheet" />
  <style>
    :root { --orange:#f58634; --ink:#161616; --muted:#666; --line:#e8e8e8; --bg:#f6f4f1; }
    * { box-sizing: border-box; }
    body { margin:0; font-family:"Noto Sans Bengali", Inter, sans-serif; background:var(--bg); color:var(--ink); }
    .top { background:#111; color:#fff; }
    .top-inner { max-width:880px; margin:0 auto; padding:18px 20px; display:flex; align-items:center; justify-content:space-between; gap:12px; }
    .brand { display:flex; align-items:center; gap:10px; color:#fff; text-decoration:none; }
    .brand img { width:42px; height:42px; background:#fff; border-radius:50%; padding:3px; }
    .brand strong { display:block; letter-spacing:.04em; }
    .brand small { display:block; color:var(--orange); letter-spacing:.16em; font-size:10px; }
    .lang { display:flex; border:1px solid #fff; border-radius:999px; overflow:hidden; }
    .lang button { border:0; background:transparent; color:#fff; padding:6px 12px; font-weight:800; cursor:pointer; }
    .lang button.on { background:var(--orange); }
    .wrap { max-width:880px; margin:28px auto 60px; padding:0 16px; }
    .card { background:#fff; border:1px solid var(--line); border-radius:18px; padding:28px; box-shadow:0 10px 30px rgba(0,0,0,.04); }
    h1 { font-family:"Playfair Display", serif; font-size:34px; margin:0 0 8px; }
    .lead { color:var(--muted); margin:0 0 22px; line-height:1.6; }
    .steps { display:flex; gap:8px; margin:0 0 24px; flex-wrap:wrap; }
    .steps span { padding:6px 12px; border-radius:999px; background:#f3f3f3; font-size:13px; font-weight:600; }
    .steps span.on { background:var(--orange); color:#fff; }
    .grid { display:grid; grid-template-columns:1fr 1fr; gap:14px; }
    label { display:block; font-size:13px; font-weight:700; margin:0 0 6px; }
    input[type=text], input[type=password], input[type=email] { width:100%; border:1px solid #d8d8d8; border-radius:10px; padding:11px 12px; font:inherit; }
    .full { grid-column:1 / -1; }
    .row { display:flex; align-items:center; gap:10px; margin-top:8px; }
    .checks { display:grid; gap:10px; }
    .check { display:flex; justify-content:space-between; gap:12px; padding:12px 14px; border:1px solid var(--line); border-radius:12px; }
    .check b { display:block; }
    .ok { color:#1a7f37; font-weight:800; }
    .bad { color:#b42318; font-weight:800; }
    .actions { display:flex; justify-content:space-between; gap:10px; margin-top:22px; flex-wrap:wrap; }
    button.btn, a.btn { border:0; background:#111; color:#fff; padding:12px 18px; border-radius:999px; font-weight:700; cursor:pointer; text-decoration:none; display:inline-block; }
    button.btn.orange, a.btn.orange { background:var(--orange); }
    button.btn:disabled { opacity:.5; cursor:not-allowed; }
    .err { background:#fff1f0; color:#b42318; padding:10px 12px; border-radius:10px; margin:12px 0; display:none; }
    .okbox { background:#eefbf1; color:#1a7f37; padding:10px 12px; border-radius:10px; margin:12px 0; display:none; }
    .note { background:#fff7ed; border:1px solid #ffd7b5; padding:12px 14px; border-radius:12px; color:#7a4b12; font-size:14px; }
    .done { text-align:center; padding:20px 0; }
    .done code { background:#f3f3f3; padding:2px 8px; border-radius:6px; }
    @media (max-width:720px) { .grid { grid-template-columns:1fr; } h1 { font-size:28px; } }
  </style>
</head>
<body>
  <header class="top">
    <div class="top-inner">
      <a class="brand" href="../">
        <img src="../logo.svg" alt="SAFA PROPERTIS" />
        <span><strong>SAFA</strong><small>PROPERTIS</small></span>
      </a>
      <div class="lang" role="group">
        <button type="button" id="langBn" class="on">BN</button>
        <button type="button" id="langEn">EN</button>
      </div>
    </div>
  </header>

  <main class="wrap">
    <section class="card">
      <div class="steps" id="steps"></div>
      <h1 id="title"></h1>
      <p class="lead" id="lead"></p>
      <div class="err" id="error"></div>
      <div class="okbox" id="success"></div>
      <div id="panel"></div>
      <div class="actions" id="actions"></div>
    </section>
  </main>

  <script>
    const installed = <?php echo $installed ? 'true' : 'false'; ?>;
    const homeUrl = <?php echo json_encode(safa_web_base() === '' ? '../' : safa_web_base() . '/'); ?>;
    const loginUrl = <?php echo json_encode((safa_web_base() === '' ? '..' : safa_web_base()) . '/login'); ?>;
    const i18n = {
      bn: {
        s1: '১. প্রয়োজনীয়তা', s2: '২. ডাটাবেজ', s3: '৩. সাইট ও অ্যাডমিন', s4: '৪. সম্পন্ন',
        t1: 'ওয়েবসাইট ইনস্টলার',
        l1: 'হোস্টিং প্যানেলে ফাইল আপলোডের পর প্রথমবার এই পাতা আসবে। নিচের চেক পাস হলে পরের ধাপে যান।',
        t2: 'ডাটাবেজ তথ্য',
        l2: 'cPanel / hPanel → MySQL Databases থেকে হোস্ট, ডাটাবেজ নাম, ইউজার ও পাসওয়ার্ড নিন।',
        t3: 'সাইট ও অ্যাডমিন',
        l3: 'যোগাযোগের তথ্য এবং অ্যাডমিন লগইন সেট করুন। চাইলে ডিফল্ট প্রজেক্ট ও ব্লগও ইনস্টল হবে।',
        t4: 'ইনস্টল সম্পন্ন',
        l4: 'সাইট চালু। নিচের লগইন দিয়ে অ্যাডমিন প্যানেলে যান।',
        next: 'পরবর্তী', back: 'পেছনে', test: 'কানেকশন টেস্ট', install: 'ইনস্টল করুন',
        open: 'ওয়েবসাইট খুলুন', login: 'অ্যাডমিন লগইন',
        host: 'ডাটাবেজ হোস্ট', port: 'পোর্ট', db: 'ডাটাবেজ নাম', user: 'ডাটাবেজ ইউজার', pass: 'ডাটাবেজ পাসওয়ার্ড',
        create: 'ডাটাবেজ না থাকলে তৈরি করুন (লোকাল/XAMPP)',
        site: 'সাইটের নাম', admin: 'অ্যাডমিন ইউজারনেম', apass: 'অ্যাডমিন পাসওয়ার্ড', aconfirm: 'পাসওয়ার্ড নিশ্চিত',
        phone: 'ফোন', whatsapp: 'WhatsApp', email: 'ইমেইল', address: 'ঠিকানা',
        seed: 'ডিফল্ট প্রজেক্ট, ব্লগ ও স্লাইডার ইনস্টল করুন',
        wait: 'ইনস্টল হচ্ছে...',
        doneUser: 'ইউজারনেম',
        already: 'সাইট আগেই ইনস্টল করা আছে।',
      },
      en: {
        s1: '1. Requirements', s2: '2. Database', s3: '3. Site & admin', s4: '4. Done',
        t1: 'Website installer',
        l1: 'This page appears on first visit after you upload the files. Continue when the checks pass.',
        t2: 'Database details',
        l2: 'Copy host, database name, user, and password from cPanel / hPanel → MySQL Databases.',
        t3: 'Site and admin',
        l3: 'Set contact details and the admin login. Default projects and blog posts can be installed too.',
        t4: 'Installation complete',
        l4: 'The site is ready. Use the login below to open the admin panel.',
        next: 'Next', back: 'Back', test: 'Test connection', install: 'Install now',
        open: 'Open website', login: 'Admin login',
        host: 'Database host', port: 'Port', db: 'Database name', user: 'Database user', pass: 'Database password',
        create: 'Create database if missing (local / XAMPP)',
        site: 'Site name', admin: 'Admin username', apass: 'Admin password', aconfirm: 'Confirm password',
        phone: 'Phone', whatsapp: 'WhatsApp', email: 'Email', address: 'Address',
        seed: 'Install default projects, blog, and slider',
        wait: 'Installing...',
        doneUser: 'Username',
        already: 'The site is already installed.',
      }
    };

    let lang = 'bn';
    let step = installed ? 4 : 1;
    let reqs = null;
    let adminUser = 'admin';
    const form = {
      db_host: 'localhost',
      db_port: '3306',
      db_name: 'safa_propertis',
      db_user: '',
      db_pass: '',
      create_db: false,
      site_name: 'SAFA PROPERTIS',
      admin_user: 'admin',
      admin_pass: '',
      admin_pass_confirm: '',
      phone: '+880 1712-345678',
      whatsapp: '8801712345678',
      email: 'info@safapropertis.com',
      address: 'House #42, Road #7, Gulshan-1, Dhaka - 1212',
      seed_demo: true,
    };

    const $ = (id) => document.getElementById(id);
    const t = () => i18n[lang];

    function showMsg(id, text) {
      const el = $(id);
      el.style.display = text ? 'block' : 'none';
      el.textContent = text || '';
    }

    async function api(action, extra = {}) {
      const res = await fetch('index.php', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ action, ...form, ...extra }),
      });
      return res.json();
    }

    function field(key, label, type = 'text') {
      return `<div class="${key === 'address' ? 'full' : ''}">
        <label>${label}</label>
        <input type="${type}" value="${String(form[key] ?? '').replace(/"/g, '&quot;')}" data-key="${key}" />
      </div>`;
    }

    function render() {
      const copy = t();
      $('steps').innerHTML = [copy.s1, copy.s2, copy.s3, copy.s4]
        .map((label, i) => `<span class="${step === i + 1 ? 'on' : ''}">${label}</span>`).join('');
      $('title').textContent = [copy.t1, copy.t2, copy.t3, copy.t4][step - 1];
      $('lead').textContent = [copy.l1, copy.l2, copy.l3, copy.l4][step - 1];
      $('langBn').classList.toggle('on', lang === 'bn');
      $('langEn').classList.toggle('on', lang === 'en');

      if (step === 1) {
        const rows = (reqs?.checks || []).map((c) => `
          <div class="check">
            <div><b>${c.label}</b><small>${c.detail}</small></div>
            <div class="${c.ok ? 'ok' : 'bad'}">${c.ok ? 'OK' : 'X'}</div>
          </div>`).join('');
        $('panel').innerHTML = `<div class="checks">${rows || '...'}</div>`;
        $('actions').innerHTML = `<span></span><button class="btn orange" id="next" ${reqs && !reqs.ready ? 'disabled' : ''}>${copy.next}</button>`;
        $('next')?.addEventListener('click', () => { step = 2; showMsg('error'); render(); });
      }

      if (step === 2) {
        $('panel').innerHTML = `<div class="grid">
          ${field('db_host', copy.host)}
          ${field('db_port', copy.port)}
          ${field('db_name', copy.db)}
          ${field('db_user', copy.user)}
          ${field('db_pass', copy.pass, 'password')}
          <div class="full row"><input type="checkbox" data-key="create_db" ${form.create_db ? 'checked' : ''} /> ${copy.create}</div>
        </div>`;
        $('actions').innerHTML = `
          <button class="btn" id="back">${copy.back}</button>
          <div>
            <button class="btn" id="test">${copy.test}</button>
            <button class="btn orange" id="next">${copy.next}</button>
          </div>`;
        bindInputs();
        $('back').onclick = () => { step = 1; showMsg('error'); showMsg('success'); render(); };
        $('test').onclick = async () => {
          showMsg('error'); showMsg('success');
          const data = await api('test_db');
          if (data.ok) showMsg('success', data.message);
          else showMsg('error', data.error || 'Failed');
        };
        $('next').onclick = async () => {
          showMsg('error'); showMsg('success');
          const data = await api('test_db');
          if (!data.ok) { showMsg('error', data.error || 'Failed'); return; }
          step = 3; render();
        };
      }

      if (step === 3) {
        $('panel').innerHTML = `<div class="grid">
          ${field('site_name', copy.site)}
          ${field('admin_user', copy.admin)}
          ${field('admin_pass', copy.apass, 'password')}
          ${field('admin_pass_confirm', copy.aconfirm, 'password')}
          ${field('phone', copy.phone)}
          ${field('whatsapp', copy.whatsapp)}
          ${field('email', copy.email, 'email')}
          ${field('address', copy.address)}
          <div class="full row"><input type="checkbox" data-key="seed_demo" ${form.seed_demo ? 'checked' : ''} /> ${copy.seed}</div>
        </div>`;
        $('actions').innerHTML = `
          <button class="btn" id="back">${copy.back}</button>
          <button class="btn orange" id="go">${copy.install}</button>`;
        bindInputs();
        $('back').onclick = () => { step = 2; showMsg('error'); render(); };
        $('go').onclick = async () => {
          showMsg('error'); showMsg('success');
          $('go').disabled = true;
          $('go').textContent = copy.wait;
          const data = await api('install');
          if (!data.ok) {
            showMsg('error', data.error || 'Failed');
            $('go').disabled = false;
            $('go').textContent = copy.install;
            return;
          }
          adminUser = data.admin_user || form.admin_user;
          step = 4;
          render();
        };
      }

      if (step === 4) {
        $('panel').innerHTML = `<div class="done">
          <p>${copy.doneUser}: <code>${adminUser || form.admin_user}</code></p>
        </div>`;
        $('actions').innerHTML = `
          <a class="btn" href="${homeUrl}">${copy.open}</a>
          <a class="btn orange" href="${loginUrl}">${copy.login}</a>`;
      }
    }

    function bindInputs() {
      document.querySelectorAll('[data-key]').forEach((el) => {
        el.addEventListener('input', () => {
          form[el.dataset.key] = el.type === 'checkbox' ? el.checked : el.value;
        });
        el.addEventListener('change', () => {
          form[el.dataset.key] = el.type === 'checkbox' ? el.checked : el.value;
        });
      });
    }

    $('langBn').onclick = () => { lang = 'bn'; document.documentElement.lang = 'bn'; render(); };
    $('langEn').onclick = () => { lang = 'en'; document.documentElement.lang = 'en'; render(); };

    async function boot() {
      if (!installed) {
        reqs = await api('requirements');
      }
      render();
    }
    boot();
  </script>
</body>
</html>
