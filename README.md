# giaoanpro-backend

Backend API for the GiaoAnPro project.

## Tech stack
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core (Code First)
- SQL Server (or any EF Core compatible provider)

## Prerequisites
- .NET 8 SDK installed
- (Optional) dotnet-ef tool for EF Core CLI: dotnet tool install --global dotnet-ef

- A running database (e.g., SQL Server). Update the connection string in `.env` (follow `.env.example`) or in `giaoanpro-backend.API/appsettings.json`.

## Quick start (local)
1. Clone the repository: git clone https://github.com/FPTUniversity-Projects/giaoanpro-backend.git cd giaoanpro-backend
2. Restore and build: dotnet restore dotnet build
3. Run the API: 
- From command line:
  ```
  cd giaoanpro-backend.API
  dotnet run
  ```
- Or open the solution in Visual Studio and start the API project (use __Solution Explorer__ then launch the API project with __Debug > Start Debugging__ or press __F5__).

## Database migrations (EF Core)
If automatic migrations do not apply, run these commands from the repository root:

Open a terminal of solution root and run:
- Add a migration: dotnet ef migrations add Init_Schema -p ./giaoanpro-backend.Persistence -s ./giaoanpro-backend.API --context GiaoanproDBContext
- Apply the migration to the database: dotnet ef database update -p ./giaoanpro-backend.Persistence -s ./giaoanpro-backend.API --context GiaoanproDBContext

Notes:
- `-p` (or `--project`) points to the project that contains the `DbContext`.
- `-s` (or `--startup-project`) points to the project that provides the runtime configuration (usually the API).
- Ensure the connection string available to the startup project points to the intended database.

## Common troubleshooting
- "dotnet ef" not found: install the __dotnet-ef__ global tool (see Prerequisites).
- Migration fails with provider error: confirm EF Core provider packages are referenced in `giaoanpro-backend.Persistence` and the correct connection string is used by the startup project.
- Ensure the `GiaoanproDBContext` class is available and correctly registered in the API's `Program.cs`.

## Contributing
- Create feature branches from `main`.
- Open pull requests with a clear description and migration steps if schema changes are included.

## License
Add a license file (e.g., `LICENSE`) to this repository if needed.
