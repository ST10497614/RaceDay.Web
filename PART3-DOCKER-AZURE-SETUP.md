# RaceDay Part 3 — Docker and Azure Blob Storage

## What changed
- The API now owns Azure Blob Storage access. The MVC project uploads and retrieves banners only through API HTTP endpoints.
- Organisers can upload an optional JPG/JPEG, PNG or WEBP event banner (maximum 5 MB) when creating or editing an event.
- Banner files are stored in the private `event-banners` Blob container. The API streams them back through `GET /api/events/{id}/banner`; the MVC app proxies them through `/event-banners/{id}` so browser clients do not need Docker-internal hostnames.
- Dockerfiles build and run both .NET 10 applications. Compose also starts Azurite as a local Azure Blob Storage emulator.
- Existing RaceDayDB schema and tables are not changed. SQL Server remains an external dependency and is configured through `.env`.

## Before running
1. Install and start Docker Desktop.
2. Copy `.env.example` to `.env` in the solution root.
3. Set `RACEDAY_DB_CONNECTION` to a SQL Server connection string reachable from the containers. For a SQL Server instance on the Windows host, enable TCP/IP and configure a static TCP port (commonly 1433), then use `Server=host.docker.internal,1433;...`. Use the actual SQL login/password or other supported SQL authentication details for your environment.
4. Generate a NEW JWT signing key (do not reuse the key previously stored in the uploaded appsettings file):
   ```powershell
   [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
   ```
   Put the result in `RACEDAY_JWT_KEY` in `.env`.
5. Ensure `.env` is not committed or shared.

## Running from Visual Studio (without Docker)
The old JWT signing key was present in the uploaded project and should be treated as exposed. Set a new key with .NET User Secrets rather than putting it back into `appsettings.json`:
```powershell
dotnet user-secrets init --project .\RaceDay.API
dotnet user-secrets set "Jwt:Key" "YOUR_NEW_RANDOM_32_BYTE_BASE64_SECRET" --project .\RaceDay.API
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true" --project .\RaceDay.API
```
Run Azurite locally (for example with Docker Desktop) and keep the existing local SQL Server connection string configured for your machine.

## Run from the solution root
```powershell
docker compose up --build
```
- MVC website: http://localhost:8081
- API: http://localhost:8080
- Swagger (Development API): http://localhost:8080/swagger
- Azurite Blob endpoint: http://localhost:10000

Stop containers with `Ctrl+C`, then optionally run `docker compose down`. To remove local Azurite data too, use `docker compose down -v` (this deletes the emulator's stored blobs).

## Important notes
- This Compose file uses Azurite for local development. For Azure deployment, set `BlobStorage__ConnectionString` to your Azure Storage connection string via a secret store/environment variable; do not commit it.
- Do not publish `.env`, connection strings, JWT keys, or real user data.
- If SQL Server runs in Docker or on another host, replace the example server address with that reachable host and port.
- The container build has not been executed in this environment; run the commands above on the machine with Docker Desktop to validate the local SQL connectivity and image build.
