# SAFA PROPERTIS

Developer company website with project listing, filters, details, WhatsApp contact, and MySQL storage.

## How it works

1. Visitors open the React website (Home, Project, Blog, About, Contact).
2. Project cards use **See Details**.
3. The details page shows photos, specs, **phone**, and **WhatsApp**.
4. Admin login saves projects into **MySQL** through `/api`.
5. On this computer, if MySQL is off, the site still opens with demo projects (orange notice at the top). After MySQL is connected, new projects stay in the database and work on hosting.

## Local run

1. Install [XAMPP](https://www.apachefriends.org/) and start **MySQL**.
2. Copy `.env.example` to `.env` and set `DB_USER` / `DB_PASS` (XAMPP root is often empty).
3. Install and start:

```bash
npm install
npm run dev
```

Open http://localhost:5173

Admin: `admin` / `safa123`

## Hostinger / cPanel hosting

1. On this PC run `npm run build`.
2. Upload **everything inside `dist/`** to `public_html` (installer, api, website, uploads).
3. In hPanel / cPanel create a MySQL database and user, and add the user to the database.
4. Open your domain. The **installer** appears on first visit.
5. Enter database details, site contact info, and an admin password. Keep “default projects” ticked if you want demo data.
6. After install, open the website and log in at `/login`.

To reinstall, delete `install.lock` and `api/config.json` on the server, then visit the site again.

Contact messages are stored in the `messages` table.
