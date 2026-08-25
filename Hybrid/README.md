# SIKKHALOY Hybrid (Phase 0)

Windows office app that keeps working without internet, then auto-syncs to SQL Server database **`EduHybrid`** (separate from the live Web Forms database `Edu`). The Web Forms app in `SIKKHALOY V2/` is unchanged.

Open [`SIKKHALOY.Hybrid.sln`](SIKKHALOY.Hybrid.sln) in Visual Studio 2022 (with the .NET 9 / ASP.NET workload). Do not open the main `SIKKHALOY.sln` Web Forms solution for this app. `dotnet build SIKKHALOY.Hybrid.sln` also works.

## Projects

| Project | Role |
|---------|------|
| `Sikkhaloy.Shared` | DTOs, sync contracts |
| `Sikkhaloy.LocalData` | SQLite, outbox, offline login, sync engine |
| `Sikkhaloy.SyncApi` | JWT login + `POST /api/sync/push` + `GET /api/sync/pull` |
| `Sikkhaloy.App` | Blazor UI (shared by Windows and web) |
| `Sikkhaloy.Client` | WPF + BlazorWebView host |
| `Sikkhaloy.Web` | Browser host (Blazor Server) |

## Run locally

1. Create **EduHybrid** as schema-only (empty tables + SPs + triggers, no row data): run [`Database/EduHybrid_Schema_Only.sql`](Database/EduHybrid_Schema_Only.sql) in **SQLCMD mode** (see [`Database/Create_EduHybrid.sql`](Database/Create_EduHybrid.sql)). Do not point this app at live `Edu`.
2. Optional if the schema script was not used — run [`Database/Hybrid_Sync_Tables.sql`](Database/Hybrid_Sync_Tables.sql) on `EduHybrid`.
3. Optional — run [`Database/Schema_Inventory.sql`](Database/Schema_Inventory.sql) and save the result for later module mapping.
4. Start the API:

```
dotnet run --project Hybrid/src/Sikkhaloy.SyncApi
```

5. Start the desktop app:

```
dotnet run --project Hybrid/src/Sikkhaloy.Client
```

Or start the **web** app (browser) instead of the Windows client:

```
dotnet run --project Hybrid/src/Sikkhaloy.Web
```

Then open `http://localhost:5288/`. The Sync API must already be running on `http://localhost:5135/`. In Visual Studio you can set **multiple startup projects**: `Sikkhaloy.SyncApi` + `Sikkhaloy.Web`.

6. Log in with an existing **Admin**, **Sub-Admin**, **Authority**, or **Sub-Authority** Membership user (first login needs internet/API). Office users can add or edit a student while the API is stopped; start the API again and wait ~30 seconds or click **এখন সিঙ্ক**. Authority stays online-only on the authority dashboard.

Local SQLite path (Windows app): `%LocalAppData%\SIKKHALOY\Hybrid\sikkhaloy.db`  
Web sessions use a temporary file under `%LocalAppData%\SIKKHALOY\Hybrid\web\`.

## Notes

- JWT `Jwt:Key` in `appsettings.json` is **dev-only**. Override with user secrets or environment for any shared machine.
- Phase 0 pull pages existing `Student` rows by `StudentID`. Web Forms edits to already-synced rows will flow after a later changelog/trigger pass.
- SMS and payment gateways stay online-only. Authority users sign in from the same login panel and land on `/authority`.
