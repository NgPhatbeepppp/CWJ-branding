# Deployment Guide

This document covers secure configuration and deployment of `Cw.Branding.Web`.

---

## 0) Where each step runs
- **Build machine / CI runner**: has repo source code, runs `dotnet restore/publish`, and usually runs migrations.
- **Production server**: runs only the deployed app artifact (`publish` output) plus runtime dependencies.
- **DB server**: the selected database host (can be the same machine as the production server).

## 0.1) .NET requirement matrix
- Run published app with `dotnet Cw.Branding.Web.dll`: requires **.NET runtime** on production server.
- Run `dotnet ef ...`: requires **.NET SDK** + project source (`.csproj`) on the machine where the command runs.
- Current service example uses `ExecStart=/usr/bin/dotnet ...dll`, so the production server needs at minimum the .NET runtime.
- For self-contained publish, the .NET runtime is bundled — update `ExecStart` to run the app binary directly.

---

## 1) Supported database providers

Set `DatabaseProvider` to one of the values below — in environment variable or `appsettings.json`:

| Value       | EF Core provider                         | Connection string key                       |
|-------------|------------------------------------------|---------------------------------------------|
| `SqlServer` | `Microsoft.EntityFrameworkCore.SqlServer`| `ConnectionStrings__SqlServerConnection`    |
| `Postgres`  | `Npgsql.EntityFrameworkCore.PostgreSQL`  | `ConnectionStrings__PostgresConnection`     |
| `MariaDb`   | `Pomelo.EntityFrameworkCore.MySql`       | `ConnectionStrings__MariaDbConnection`      |

The app will throw a clear startup error if the matching connection string is missing or empty.

---

## 2) Secure secrets — do NOT store passwords in `appsettings.json`

Passwords and sensitive values must come from **environment variables** (production/CI) or **.NET User Secrets** (local development). `appsettings.json` contains only non-sensitive defaults (provider name, host, database name).

### 2a) Local development — User Secrets

Initialize User Secrets for the project (run once):

```bash
dotnet user-secrets init --project Cw.Branding.Web
```

Set the secret for your chosen provider:

**SQL Server**
```bash
dotnet user-secrets set "ConnectionStrings:SqlServerConnection" "Server=localhost;Database=CwBrandingDb;User Id=your-user;Password=your-password;TrustServerCertificate=True" --project Cw.Branding.Web
```

**PostgreSQL**
```bash
dotnet user-secrets set "ConnectionStrings:PostgresConnection" "Host=localhost;Port=5432;Database=CwBrandingDb;Username=your-user;Password=your-password" --project Cw.Branding.Web
```

**MariaDB**
```bash
dotnet user-secrets set "ConnectionStrings:MariaDbConnection" "Server=localhost;Port=3306;Database=CwBrandingDb;User=your-user;Password=your-password" --project Cw.Branding.Web
```

Optionally override the provider for local dev:
```bash
dotnet user-secrets set "DatabaseProvider" "MariaDb" --project Cw.Branding.Web
```

List or clear secrets:
```bash
dotnet user-secrets list --project Cw.Branding.Web
dotnet user-secrets clear --project Cw.Branding.Web
```

### 2b) Production / CI — Environment variables

Use double underscores (`__`) as the configuration path separator.

**SQL Server**
```
DatabaseProvider=SqlServer
ConnectionStrings__SqlServerConnection=Server=your-host;Database=CwBrandingDb;User Id=your-user;Password=your-password;TrustServerCertificate=True
```

**PostgreSQL**
```
DatabaseProvider=Postgres
ConnectionStrings__PostgresConnection=Host=your-host;Port=5432;Database=CwBrandingDb;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true
```

**MariaDB**
```
DatabaseProvider=MariaDb
ConnectionStrings__MariaDbConnection=Server=your-host;Port=3306;Database=CwBrandingDb;User=your-user;Password=your-password;SslMode=Required
```

PowerShell (for quick local testing only):
```powershell
$env:DatabaseProvider = "Postgres"
$env:ConnectionStrings__PostgresConnection = "Host=localhost;Port=5432;Database=CwBrandingDb;Username=your-user;Password=your-password"
```

---

## 3) Server prerequisites (production server — Ubuntu/Debian example)

Install infrastructure components:

```bash
sudo apt-get update
sudo apt-get install -y nginx mariadb-server   # or postgresql, or leave DB off-host
```

Install the .NET runtime (SDK only needed if running EF CLI on the production server):

```bash
# Example for .NET 8 runtime
sudo apt-get install -y dotnet-runtime-8.0
```

---

## 4) Create database and user (DB server)

### MariaDB / MySQL
```bash
sudo mysql -u root -p
```
```sql
CREATE DATABASE CwBrandingDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cw_user'@'%' IDENTIFIED BY 'REPLACE_WITH_STRONG_PASSWORD';
GRANT ALL PRIVILEGES ON CwBrandingDb.* TO 'cw_user'@'%';
FLUSH PRIVILEGES;
```

### PostgreSQL
```bash
sudo -u postgres psql
```
```sql
CREATE DATABASE "CwBrandingDb";
CREATE USER cw_user WITH PASSWORD 'REPLACE_WITH_STRONG_PASSWORD';
GRANT ALL PRIVILEGES ON DATABASE "CwBrandingDb" TO cw_user;
```

### SQL Server
Create the database and a login via SSMS, Azure Data Studio, or `sqlcmd`. Grant `db_owner` on `CwBrandingDb`.

---

## 5) Publish app artifact (build machine or CI)

> ⚠️ `wwwroot/css/site.css` is **generated by Tailwind CSS** at build time — it is not committed to source control.
> You must run `npm` before `dotnet publish`, otherwise the CSS file will be missing from the publish output and **all styles will be 404 in production**.

### 5.1) Prerequisites on the build machine

- **Node.js ≥ 18** and **npm** must be installed.

```bash
node --version   # must print v18.x or higher
npm --version
```

Install Node.js if missing (Ubuntu/Debian):
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs
```

### 5.2) Full publish sequence

Run these commands **in order** from the repository root:

```bash
# 1. Restore .NET packages
dotnet restore Cw.Branding.sln

# 2. Install Node packages (needed to build Tailwind CSS)
cd Cw.Branding.Web
npm ci
cd ..

# 3. Publish (the MSBuild target BuildTailwindCss runs npm run build:css automatically
#    and outputs wwwroot/css/site.css before the publish completes)
dotnet publish Cw.Branding.Web -c Release -o ./publish
```

Verify that the CSS file was generated before copying:
```bash
ls ./publish/wwwroot/css/site.css   # must exist
ls ./publish/wwwroot/images/        # must contain your image files
ls ./publish/wwwroot/js/            # must contain site.js
ls ./publish/wwwroot/lib/           # must contain jquery etc.
```

If `site.css` is missing, run `npm run build:css` manually inside `Cw.Branding.Web/` and re-run `dotnet publish`.

### 5.3) Copy to the server

```bash
scp -r ./publish/* deploy@your-server:/opt/cw-branding/current/
```

After copying, **always fix ownership and permissions** so the `cwbranding` service user can read the files:
```bash
sudo chown -R cwbranding:cwbranding /opt/cw-branding/current
sudo find /opt/cw-branding/current -type d -exec chmod 755 {} \;
sudo find /opt/cw-branding/current -type f -exec chmod 644 {} \;
```

> If you skip this step and the files were copied as `root` or `deploy`, all static files (`/css/`, `/images/`, `/js/`, `/lib/`) will return **404** in the browser even though they exist on disk.

Restart the service and verify:
```bash
sudo systemctl restart cw-branding
ls /opt/cw-branding/current/wwwroot/css/site.css
ls /opt/cw-branding/current/wwwroot/images/
curl -I https://your-domain.com/css/site.css   # expect 200
```

---

## 6) Configure app as a `systemd` service (production server — Linux)

Create service account and directories:
```bash
sudo useradd --system --create-home --shell /usr/sbin/nologin cwbranding
sudo mkdir -p /opt/cw-branding/current
sudo mkdir -p /opt/cw-branding/shared/logs
sudo mkdir -p /opt/cw-branding/shared/wwwroot/uploads
sudo chown -R cwbranding:cwbranding /opt/cw-branding
```

Create `/etc/systemd/system/cw-branding.service`:

```ini
[Unit]
Description=CW Branding Web
After=network.target

[Service]
User=cwbranding
Group=cwbranding
WorkingDirectory=/opt/cw-branding/current
ExecStart=/usr/bin/dotnet /opt/cw-branding/current/Cw.Branding.Web.dll
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DatabaseProvider=MariaDb
Environment=ConnectionStrings__MariaDbConnection=Server=127.0.0.1;Port=3306;Database=CwBrandingDb;User=cw_user;Password=REPLACE_WITH_STRONG_PASSWORD;SslMode=Required

[Install]
WantedBy=multi-user.target
```

> ⚠️ Do not commit this file with real passwords to source control.
> For better secret management, consider using `systemd` credential files or a secret manager (Vault, AWS Secrets Manager, etc.).

Start and enable the service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable cw-branding
sudo systemctl start cw-branding
sudo systemctl status cw-branding
```

---

## 7) Apply EF Core migrations

> `localhost` in the connection string resolves on the machine where `dotnet ef` runs.
> The DB is usually bound to `127.0.0.1` on the production server and is not exposed to the internet.
> An SSH tunnel lets the build machine reach that local port securely without opening the DB port publicly.

---

### Option A (recommended): SSH tunnel from build machine to production DB

Use this when:
- The production DB listens on `127.0.0.1` (MariaDB/PostgreSQL default — not publicly reachable).
- You want to run migrations from the build machine or CI runner where source code lives.
- You do not want to install the .NET SDK on the production server.

#### How SSH tunnelling works

```
Build machine          Production server
  localhost:13306  -->  SSH  -->  127.0.0.1:3306 (MariaDB)
```

`dotnet ef` on the build machine connects to `localhost:13306`.
SSH forwards that traffic through an encrypted tunnel to `127.0.0.1:3306` on the production server.
The DB never needs to be publicly exposed.

---

#### Step 1 — Ensure SSH access to the production server

You need an SSH user on the production server that has permission to forward ports.
Check `/etc/ssh/sshd_config` on the server and ensure:

```
AllowTcpForwarding yes
```

Reload SSH if you change this:
```bash
sudo systemctl reload ssh
```

---

#### Step 2 — Open the SSH tunnel (build machine)

**Linux / macOS / WSL / PowerShell with OpenSSH:**

```bash
ssh -N -L 13306:127.0.0.1:3306 deploy@your-server-ip
```

| Flag | Meaning |
|------|---------|
| `-N` | Do not execute a remote command — tunnel only. |
| `-L 13306:127.0.0.1:3306` | Forward local port `13306` → `127.0.0.1:3306` on the remote server. |
| `deploy@your-server-ip` | SSH user and server address. |

Run this in a **separate terminal** and keep it open while running migrations.

To run it in the background instead:
```bash
ssh -f -N -L 13306:127.0.0.1:3306 deploy@your-server-ip
# Kill it later with:
kill $(lsof -ti:13306)
```

**Windows PowerShell (built-in OpenSSH):**
```powershell
ssh -N -L 13306:127.0.0.1:3306 deploy@your-server-ip
```

> Use port `15432` instead of `13306` for PostgreSQL (remote port `5432`).

---

#### Step 3 — Set the connection string to target the tunnel (build machine)

The connection string must point to `localhost` with the **local tunnel port** (e.g., `13306`).

**MariaDB — PowerShell:**
```powershell
$env:ConnectionStrings__MariaDbConnection = "Server=127.0.0.1;Port=13306;Database=CwBrandingDb;User=cw_user;Password=your-password;SslMode=None"
$env:DatabaseProvider = "MariaDb"
```

**MariaDB — Bash:**
```bash
export ConnectionStrings__MariaDbConnection="Server=127.0.0.1;Port=13306;Database=CwBrandingDb;User=cw_user;Password=your-password;SslMode=None"
export DatabaseProvider="MariaDb"
```

> `SslMode=None` is safe here because traffic is already encrypted inside the SSH tunnel.

**PostgreSQL — PowerShell:**
```powershell
$env:ConnectionStrings__PostgresConnection = "Host=127.0.0.1;Port=15432;Database=CwBrandingDb;Username=cw_user;Password=your-password;SSL Mode=Disable"
$env:DatabaseProvider = "Postgres"
```

**PostgreSQL — Bash:**
```bash
export ConnectionStrings__PostgresConnection="Host=127.0.0.1;Port=15432;Database=CwBrandingDb;Username=cw_user;Password=your-password;SSL Mode=Disable"
export DatabaseProvider="Postgres"
```

---

#### Step 4 — Run migrations (build machine, same terminal)

```bash
dotnet ef database update --project Cw.Branding.Web
```

Expected output ends with:
```
Done.
```

If you see a connection refused error, verify the SSH tunnel is open and the port matches.

---

#### Step 5 — Close the tunnel

If running in foreground: press `Ctrl+C` in the tunnel terminal.

If running in background:
```bash
# Linux/macOS/WSL
kill $(lsof -ti:13306)

# PowerShell
Get-Process ssh | Where-Object { $_.CommandLine -like "*13306*" } | Stop-Process
```

---

### Option B: run migrations directly on the production server

Use this when:
- The DB is only reachable as `localhost` on the production server.
- Setting up an SSH tunnel is not practical.
- You are willing to install the .NET SDK on the production server.

Requirements on the production server:
- .NET SDK (not just runtime): `sudo apt-get install -y dotnet-sdk-8.0`
- Full source tree (not just the publish artifact).

```bash
# On the production server, inside the repository root:
export ConnectionStrings__MariaDbConnection="Server=127.0.0.1;Port=3306;Database=CwBrandingDb;User=cw_user;Password=your-password;SslMode=Required"
export DatabaseProvider="MariaDb"
dotnet ef database update --project Cw.Branding.Web
```

---

### Checking which migrations have been applied

```bash
dotnet ef migrations list --project Cw.Branding.Web
```

Pending migrations are shown without a `[applied]` label.

---

## 8) Configure Nginx as a public web server (reverse proxy)

Nginx sits in front of Kestrel, handles HTTPS/SSL termination, and forwards traffic to the app on `127.0.0.1:5000`.

```
Internet (port 80/443)  →  Nginx  →  Kestrel (127.0.0.1:5000)
```

---

### Step 1 — Install Nginx

**AlmaLinux / RHEL / Fedora:**
```bash
sudo dnf install -y nginx
sudo systemctl enable --now nginx
```

**Ubuntu / Debian:**
```bash
sudo apt-get install -y nginx
sudo systemctl enable --now nginx
```

---

### Step 2 — Create site configuration

Create `/etc/nginx/conf.d/cw-branding.conf`:

```nginx
server {
    listen 80;
    server_name your-domain.com www.your-domain.com;

    # Increase upload limit to match app setting (100MB)
    client_max_body_size 100M;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_set_header   X-Real-IP $remote_addr;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;

        proxy_read_timeout  300s;
        proxy_send_timeout  300s;
        proxy_connect_timeout 10s;
    }

    # Serve static files directly through Nginx (optional performance boost)
    location /static/ {
        alias /opt/cw-branding/current/wwwroot/;
        expires 30d;
        add_header Cache-Control "public, immutable";
    }
}
```

> Replace `your-domain.com` with your actual domain or server IP.

Test and reload:
```bash
sudo nginx -t
sudo systemctl reload nginx
```

---

### Step 3 — Open firewall ports

**AlmaLinux / RHEL (firewalld):**
```bash
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

**Ubuntu (ufw):**
```bash
sudo ufw allow 'Nginx Full'
```

---

### Step 4 — Enable HTTPS with Let's Encrypt (Certbot)

**AlmaLinux / RHEL:**
```bash
sudo dnf install -y certbot python3-certbot-nginx
```

**Ubuntu / Debian:**
```bash
sudo apt-get install -y certbot python3-certbot-nginx
```

Obtain and install certificate (auto-updates Nginx config):
```bash
sudo certbot --nginx -d your-domain.com -d www.your-domain.com
```

Certbot will:
- Obtain a certificate from Let's Encrypt
- Update `/etc/nginx/conf.d/cw-branding.conf` to add `listen 443 ssl`
- Add HTTP → HTTPS redirect automatically

Test auto-renewal:
```bash
sudo certbot renew --dry-run
```

Certbot installs a systemd timer for automatic renewal — no manual cron needed.

---

### Step 5 — Update app service to trust forwarded headers

Because Nginx proxies the request, the app must read `X-Forwarded-Proto` to know the original scheme (https).

Add this to `Program.cs` **before** `app.UseHttpsRedirection()`:

```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});
```

Rebuild and redeploy the app after this change.

---

### Step 6 — Verify end-to-end

```bash
# HTTP (should redirect to HTTPS after certbot)
curl -i http://your-domain.com/

# HTTPS
curl -i https://your-domain.com/en

# Check Nginx logs if anything is wrong
sudo tail -n 100 /var/log/nginx/access.log
sudo tail -n 100 /var/log/nginx/error.log
```

---

## 9) Fixing CSS not rendering correctly in production

ASP.NET Core's static file fingerprinting and the Razor Pages tag helpers generate versioned URLs (e.g. `site.css?v=abc123`). If styles look broken in production, work through the checklist below.

### 9.1) Root causes and fixes

#### A — Tailwind CSS was never built (most common cause of `site.css` 404)

`wwwroot/css/site.css` is **not committed to Git** — it is generated by Tailwind CSS during the build.
The `.csproj` MSBuild target `BuildTailwindCss` runs `npm run build:css` automatically, but **only if Node.js and npm are installed on the build machine**.

If Node/npm is absent or `npm ci` was never run, the MSBuild target is skipped silently, `site.css` is never created, and the publish output will not contain it.

**Fix:** ensure `npm ci` runs before `dotnet publish` (see Section 5.2).

Check on the server:
```bash
ls /opt/cw-branding/current/wwwroot/css/site.css   # must exist
```

If missing, re-run the full publish sequence from Section 5.2 on the build machine and re-copy.

---

#### B — Static files not published / wrong wwwroot path

After `dotnet publish`, the `wwwroot` folder (including all CSS/JS) must be present in the publish output.

Verify on the server:
```bash
ls /opt/cw-branding/current/wwwroot/css/
```

If it is empty or missing, re-publish and re-copy:
```bash
dotnet publish Cw.Branding.Web -c Release -o ./publish
scp -r ./publish/* deploy@your-server:/opt/cw-branding/current/
```

---

#### B — `UseStaticFiles` middleware not called

`Program.cs` must call `app.UseStaticFiles()` **before** `app.UseRouting()` / `app.MapRazorPages()`.

```csharp
app.UseStaticFiles();   // ← must be present
app.UseRouting();
app.MapRazorPages();
```

---

#### C — Nginx intercepts `/wwwroot` or strips the query-string version tag

The Nginx `location /static/` block in step 8 uses an `alias` to `/opt/cw-branding/current/wwwroot/`. CSS files referenced as `/css/site.css` (the default Razor path) do **not** match `/static/` and are correctly passed through to Kestrel.

If you added a custom Nginx `location` that accidentally captures `/css/`, `/js/`, or `/lib/`, remove it so those requests fall through to the `proxy_pass` block.

To let Nginx serve static files directly (recommended for performance), add explicit locations **with correct MIME types**:

```nginx
location ~* \.(css|js|woff2?|ttf|eot|svg|png|jpg|jpeg|gif|ico|webp)$ {
    root /opt/cw-branding/current/wwwroot;
    expires 30d;
    add_header Cache-Control "public, immutable";
    access_log off;
}
```

Place this block **before** the `location /` block inside the `server {}` block. Test and reload:
```bash
sudo nginx -t && sudo systemctl reload nginx
```

---

#### D — Wrong file permissions (files exist but app can't read them)

This is the **most common reason** static files return 404 even though they exist on disk.

When you copy files to the server as `root` or `deploy`, the `cwbranding` service user has no read permission on them.
The app finds nothing to serve and returns 404 — the same symptom as a missing file.

**Diagnose:**
```bash
# Check who owns the files
ls -la /opt/cw-branding/current/wwwroot/css/
ls -la /opt/cw-branding/current/wwwroot/images/

# Check what user the app runs as
sudo systemctl cat cw-branding | grep User
```

Expected output: files owned by `cwbranding`, mode `644` (files) / `755` (directories).

**Fix — correct ownership and permissions in one step:**
```bash
sudo chown -R cwbranding:cwbranding /opt/cw-branding/current
sudo find /opt/cw-branding/current -type d -exec chmod 755 {} \;
sudo find /opt/cw-branding/current -type f -exec chmod 644 {} \;
```

Restart the service after fixing permissions:
```bash
sudo systemctl restart cw-branding
```

Then verify the file is now served:
```bash
curl -I https://your-domain.com/css/site.css   # expect HTTP/2 200
```

> **Prevention:** always run the `chown` command immediately after every `scp` deploy (see Section 5.3).

---

#### E — Browser serving stale cached CSS

After a new deployment the browser may still serve old CSS from its cache.

- Hard-reload the page: **Ctrl+Shift+R** (Windows/Linux) or **Cmd+Shift+R** (macOS).
- Confirm the `<link>` tag in the page source contains a fresh fingerprint query string, e.g. `?v=xYz`.
- If the fingerprint never changes, ensure `asp-append-version="true"` is set on `<link>` tags in your layout:

```html
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

---

#### F — `ASPNETCORE_ENVIRONMENT` is not set to `Production`

When the environment is not `Production`, the app may serve unminified or differently-bundled assets. Confirm the systemd service has:

```ini
Environment=ASPNETCORE_ENVIRONMENT=Production
```

Restart after any change:
```bash
sudo systemctl restart cw-branding
```

---

#### G — Content Security Policy (CSP) blocking stylesheets

If a CSP header is configured (either in Nginx or in the app), it may block inline styles or external fonts used by the CSS. Check the browser console for CSP errors and adjust the policy accordingly.

---

### 9.2) Quick diagnostic checklist

Work through these **in order** — the first four cover the most common production failures.

| # | Check | Command / action |
|---|-------|-----------------|
| 1 | Files exist on server | `ls /opt/cw-branding/current/wwwroot/css/site.css` |
| 2 | **File ownership is `cwbranding`** | `ls -la /opt/cw-branding/current/wwwroot/css/` |
| 3 | **File permissions allow read (`644`/`755`)** | `stat /opt/cw-branding/current/wwwroot/css/site.css` |
| 4 | Fix ownership + permissions if wrong | `sudo chown -R cwbranding:cwbranding /opt/cw-branding/current` then `sudo find … -type f -exec chmod 644 {} \;` |
| 5 | App returns 200 for CSS URL | `curl -I https://your-domain.com/css/site.css` |
| 6 | `node` and `npm` present on build machine | `node --version && npm --version` |
| 7 | `npm ci` was run before `dotnet publish` | Review publish sequence (Section 5.2) |
| 8 | CSS file exists in publish output | `ls ./publish/wwwroot/css/site.css` |
| 9 | `UseStaticFiles()` present in `Program.cs` | Review source |
| 10 | No Nginx `location` capturing CSS path | Review `/etc/nginx/conf.d/cw-branding.conf` |
| 11 | Browser cache cleared | Hard-reload (Ctrl+Shift+R) |
| 12 | `asp-append-version="true"` on `<link>` tags | Review `_Layout.cshtml` |
| 13 | `ASPNETCORE_ENVIRONMENT=Production` set | `sudo systemctl cat cw-branding` |
| 14 | No CSP errors in browser console | Open DevTools → Console |

---

