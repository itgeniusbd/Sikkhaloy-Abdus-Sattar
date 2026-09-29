# IISAC / school website security

## What this site does NOT do
- It does **not** connect to the Sikkhaloy database directly.
- It calls `https://hybridapi.sikkhaloy.com` over HTTPS using `school_id` and optional `SIKKHALOY_WEBSITE_KEY`.

## Configure on Hostinger / any PHP host

1. **Website API key**  
   Set `sikkhaloy_website_key` in config (installer field) or env `SIKKHALOY_WEBSITE_KEY`.  
   The site sends it as header `X-Website-Key` on Sikkhaloy API calls.  
   Enforce the same key on the Sikkhaloy API side for real protection.

2. **Keep secrets off the web**  
   Prefer moving config outside `public_html`, e.g. `/home/USER/iisac-config.json`, then set env:
   `IISAC_CONFIG_PATH=/home/USER/iisac-config.json`  
   Root and `api/.htaccess` also deny download of `config.json`, `.env`, `install.lock`.

3. **HTTPS**  
   `.htaccess` redirects HTTP → HTTPS (skips localhost).

4. **Admin passwords**  
   New/changed passwords must be ≥10 chars with upper, lower, number, and symbol.  
   Optional TOTP 2FA is not enabled yet — use a password manager and unique admin passwords.

5. **Rate limit / logs**  
   Edu/admission proxy routes are rate-limited per IP.  
   PHP/Node write edu call logs under the OS temp dir (`iisac-logs`), not under the public web root.

6. **Optional allowlist**  
   Installer field `sikkhaloy_allow_hosts` stores comma-separated hosts for documentation / future API enforcement. Enforce allowlists on the Sikkhaloy API for domain/IP binding.

## Verify after deploy
- `https://YOURDOMAIN/api/config.json` → denied / not found  
- Weak admin password rejected  
- Burst of `/api/edu/home` eventually returns 429  
- Sikkhaloy requests include `X-Website-Key` when the key is set
