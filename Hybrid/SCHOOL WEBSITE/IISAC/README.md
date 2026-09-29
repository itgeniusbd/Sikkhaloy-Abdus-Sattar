# IISAC Institution Website

Same stack and design as the DEMO site: React + Vite, MySQL on hosting, PHP API for local content, and live education data from the Sikkhaloy API.

## Data split

- **Sikkhaloy / Education database:** institution name, logo, student totals by class, teachers, current students.
- **MySQL on hosting:** news, photo gallery, hero slider, about/contact pages, messages, newsletter, admin login.

Set `SCHOOL_ID` to this institution’s SchoolID in Sikkhaloy. Change it for the next school website.

## Local run

1. Start MariaDB / XAMPP MySQL.
2. Start Sikkhaloy SyncApi (`http://127.0.0.1:5135`) so name, logo, and lists can load.
3. Copy `.env.example` to `.env` and set `DB_USER`, `DB_PASS`, `SCHOOL_ID`.
4. Install and start:

```bash
npm install
npm run dev
```

Open http://localhost:5174

Admin: `admin` / `iisac123`

## Hostinger / cPanel

1. On this PC run `npm run build`.
2. Upload everything inside `dist/` to `public_html`.
3. Create a MySQL database and user.
4. Open the domain. The installer appears on first visit.
5. Enter MySQL details, **Sikkhaloy School ID**, and the public Sikkhaloy API URL.
6. Log in at `/login`.

To reinstall, delete `install.lock` and `api/config.json`, then visit the site again.
