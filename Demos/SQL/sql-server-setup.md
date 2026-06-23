# SQL Server in Docker — Quick Setup

Spin up a fresh SQL Server container. Use this if your machine fails, you need to wipe the
database, or you want a clean start.

## 1. Start Docker Desktop

Open Docker Desktop if it isn't already running (whale icon steady in the tray).

## 2. Pull the image

```powershell
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

## 3. Run the container

```powershell
docker run `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=YOUR_PASS_WORD" `
  -p 1433:1433 `
  --name librarymssql `
  -d `
  mcr.microsoft.com/mssql/server:2022-latest
```

The container now shows in Docker Desktop. Verify it works by connecting through the **mssql**
extension in VS Code:

- Server: `localhost,1433`
- Authentication: SQL Login
- User: `sa`
- Password: `YOUR_PASS_WORD`
- Trust server certificate: yes
