# Deployment Guideline

## 1) Database provider selection
Set `DatabaseProvider` to one of:
- `SqlServer`
- `Postgres`
- `MariaDb`

You can set it via environment variable:

- PowerShell: `$env:DatabaseProvider="Postgres"`

## 2) Do not store DB passwords in `appsettings.json`
Use secure configuration sources:
- Environment variables (recommended for deployment)
- User Secrets (local development)

### Environment variable examples

#### PostgreSQL
- `ConnectionStrings__PostgresConnection="Host=your-host;Port=5432;Database=CwBrandingDb;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"`

#### MariaDB
- `ConnectionStrings__MariaDbConnection="Server=your-host;Port=3306;Database=CwBrandingDb;User=your-user;Password=your-password;SslMode=Required"`

#### SQL Server
- `ConnectionStrings__SqlServerConnection="Server=your-host;Database=CwBrandingDb;User Id=your-user;Password=your-password;TrustServerCertificate=True"`

## 3) Apply migrations
Run migration update after deployment configuration is set:

- `dotnet ef database update --project Cw.Branding.Web`

## 4) Publish and run
- `dotnet publish Cw.Branding.Web -c Release -o ./publish`
- Run the published app with required environment variables set.

## 5) Recommended production settings
- Enforce HTTPS and valid TLS certificates.
- Restrict DB network access to application hosts only.
- Rotate DB credentials regularly.
- Use least-privilege DB accounts.
