# Database notes

This project uses **Microsoft SQL Server** through Entity Framework Core.

For the easiest local setup, run SQL Server in Docker:

```bash
docker run --name healthflow-sql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

The API currently uses `Database.EnsureCreated()` so the schema is created automatically for a demo/dev database.
For a long-lived production system, switch to EF Core migrations before your first production launch.
