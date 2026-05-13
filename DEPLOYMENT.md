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

From the repository root:

```bash
dotnet restore Cw.Branding.sln
dotnet publish Cw.Branding.Web -c Release -o ./publish
```

Copy to the server (example):
```bash
scp -r ./publish/* deploy@your-server:/opt/cw-branding/current/
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
