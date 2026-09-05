using System.Data;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;

namespace Sikkhaloy.LocalData;

public sealed class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options)
        : base(options)
    {
    }

    public DbSet<LocalStudent> Students => Set<LocalStudent>();
    public DbSet<LocalSchoolClass> Classes => Set<LocalSchoolClass>();
    public DbSet<LocalClassGroup> ClassGroups => Set<LocalClassGroup>();
    public DbSet<LocalClassSection> ClassSections => Set<LocalClassSection>();
    public DbSet<LocalClassShift> ClassShifts => Set<LocalClassShift>();
    public DbSet<LocalClassJoin> ClassJoins => Set<LocalClassJoin>();
    public DbSet<LocalEducationYear> EducationYears => Set<LocalEducationYear>();
    public DbSet<OutboxEntry> Outbox => Set<OutboxEntry>();
    public DbSet<SyncWatermark> Watermarks => Set<SyncWatermark>();
    public DbSet<YearWatermark> YearWatermarks => Set<YearWatermark>();
    public DbSet<CachedMenu> Menus => Set<CachedMenu>();
    public DbSet<CachedSession> Sessions => Set<CachedSession>();
    public DbSet<CachedApiSnapshot> ApiSnapshots => Set<CachedApiSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalStudent>(e =>
        {
            e.HasKey(x => x.LocalId);
            e.HasIndex(x => new { x.SchoolID, x.StudentCode });
            e.Property(x => x.StudentCode).HasMaxLength(50);
            e.Property(x => x.StudentsName).HasMaxLength(200);
            e.Property(x => x.SMSPhoneNo).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<LocalSchoolClass>(e =>
        {
            e.ToTable("Classes");
            e.HasKey(x => x.LocalId);
            e.HasIndex(x => x.ClassID);
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LocalClassGroup>(e =>
        {
            e.ToTable("ClassGroups");
            e.HasKey(x => x.LocalId);
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LocalClassSection>(e =>
        {
            e.ToTable("ClassSections");
            e.HasKey(x => x.LocalId);
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LocalClassShift>(e =>
        {
            e.ToTable("ClassShifts");
            e.HasKey(x => x.LocalId);
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LocalClassJoin>(e =>
        {
            e.ToTable("ClassJoins");
            e.HasKey(x => x.LocalId);
        });

        modelBuilder.Entity<LocalEducationYear>(e =>
        {
            e.ToTable("EducationYears");
            e.HasKey(x => x.EducationYearID);
            e.Property(x => x.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<YearWatermark>(e =>
        {
            e.ToTable("YearWatermarks");
            e.HasKey(x => new { x.SchoolID, x.EducationYearID });
        });

        modelBuilder.Entity<OutboxEntry>(e =>
        {
            e.HasKey(x => x.OutboxId);
            e.Property(x => x.OutboxId).ValueGeneratedOnAdd();
            e.HasIndex(x => x.CreatedUtc);
        });

        modelBuilder.Entity<SyncWatermark>(e =>
        {
            e.HasKey(x => x.SchoolID);
        });

        modelBuilder.Entity<CachedMenu>(e =>
        {
            e.ToTable("Menus");
            e.HasKey(x => x.UserName);
        });

        modelBuilder.Entity<CachedSession>(e =>
        {
            e.HasKey(x => x.UserName);
            e.Property(x => x.DisplayName).IsRequired(false);
        });

        modelBuilder.Entity<CachedApiSnapshot>(e =>
        {
            e.ToTable("ApiSnapshots");
            e.HasKey(x => x.CacheKey);
            e.Property(x => x.CacheKey).HasMaxLength(500);
        });
    }

    public static string DefaultDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SIKKHALOY",
            "Hybrid");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "sikkhaloy.db");
    }

    public static async Task EnsureSchemaAsync(LocalDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Classes (
                LocalId TEXT NOT NULL CONSTRAINT PK_Classes PRIMARY KEY,
                ClassID INTEGER NOT NULL,
                SchoolID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                SortOrder INTEGER NOT NULL,
                SyncStatus INTEGER NOT NULL DEFAULT 0
            );
            """, cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_Classes_ClassID ON Classes(ClassID);",
            cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ClassGroups (
                LocalId TEXT NOT NULL CONSTRAINT PK_ClassGroups PRIMARY KEY,
                SubjectGroupID INTEGER NOT NULL,
                SchoolID INTEGER NOT NULL,
                ClassID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                SyncStatus INTEGER NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ClassSections (
                LocalId TEXT NOT NULL CONSTRAINT PK_ClassSections PRIMARY KEY,
                SectionID INTEGER NOT NULL,
                SchoolID INTEGER NOT NULL,
                ClassID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                SyncStatus INTEGER NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ClassShifts (
                LocalId TEXT NOT NULL CONSTRAINT PK_ClassShifts PRIMARY KEY,
                ShiftID INTEGER NOT NULL,
                SchoolID INTEGER NOT NULL,
                ClassID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                SyncStatus INTEGER NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ClassJoins (
                LocalId TEXT NOT NULL CONSTRAINT PK_ClassJoins PRIMARY KEY,
                JoinID INTEGER NOT NULL,
                SchoolID INTEGER NOT NULL,
                ClassID INTEGER NOT NULL,
                SubjectGroupID INTEGER NOT NULL,
                SectionID INTEGER NOT NULL,
                ShiftID INTEGER NOT NULL,
                GroupName TEXT NOT NULL,
                SectionName TEXT NOT NULL,
                ShiftName TEXT NOT NULL,
                SyncStatus INTEGER NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS EducationYears (
                EducationYearID INTEGER NOT NULL CONSTRAINT PK_EducationYears PRIMARY KEY,
                SchoolID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                SortOrder INTEGER NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS YearWatermarks (
                SchoolID INTEGER NOT NULL,
                EducationYearID INTEGER NOT NULL,
                LastChangeId INTEGER NOT NULL,
                PulledUtc TEXT NOT NULL,
                CONSTRAINT PK_YearWatermarks PRIMARY KEY (SchoolID, EducationYearID)
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Menus (
                UserName TEXT NOT NULL CONSTRAINT PK_Menus PRIMARY KEY,
                PayloadJson TEXT NOT NULL,
                PulledUtc TEXT NOT NULL
            );
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS ApiSnapshots (
                CacheKey TEXT NOT NULL CONSTRAINT PK_ApiSnapshots PRIMARY KEY,
                PayloadJson TEXT NOT NULL,
                PulledUtc TEXT NOT NULL
            );
            """, cancellationToken);

        var added = false;
        added |= await EnsureColumnAsync(db, "Students", "BloodGroup", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "Religion", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "AdmissionDate", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "IsNew", "INTEGER", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "ClassName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "SectionName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "ShiftName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "GroupName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "StudentEmailAddress", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "LegalIdentity", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "StudentsLocalAddress", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "StudentPermanentAddress", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "OtherDetails", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "PrevSchoolName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "PrevClass", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "PrevExamYear", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "PrevExamGrade", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "FatherOccupation", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "FatherPhoneNumber", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "MotherOccupation", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "MotherPhoneNumber", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "GuardianName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "GuardianRelationshipwithStudent", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Students", "GuardianPhoneNumber", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "DisplayName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "StudentID", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "StudentClassID", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "ClassID", "INTEGER NOT NULL DEFAULT 0", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "StudentCode", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "ClassName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Sessions", "SectionName", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Classes", "LocalId", "TEXT", cancellationToken);
        added |= await EnsureColumnAsync(db, "Classes", "SyncStatus", "INTEGER", cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE Sessions SET DisplayName = COALESCE(DisplayName, UserName, '') WHERE DisplayName IS NULL;
            UPDATE Sessions SET StudentID = COALESCE(StudentID, 0);
            UPDATE Sessions SET StudentClassID = COALESCE(StudentClassID, 0);
            UPDATE Sessions SET ClassID = COALESCE(ClassID, 0);
            """, cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE Classes SET SyncStatus = COALESCE(SyncStatus, 0);
            UPDATE Classes SET LocalId = '' WHERE LocalId IS NULL;
            """, cancellationToken);

        await BackfillClassLocalIdsAsync(db, cancellationToken);
        await RebuildClassesPrimaryKeyAsync(db, cancellationToken);

        if (added)
        {
            await db.Database.ExecuteSqlRawAsync("UPDATE Watermarks SET LastChangeId = 0;", cancellationToken);
            await db.Database.ExecuteSqlRawAsync("UPDATE YearWatermarks SET LastChangeId = 0;", cancellationToken);
        }
    }

    private static async Task<bool> EnsureColumnAsync(
        LocalDbContext db,
        string table,
        string column,
        string sqlType,
        CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info(\"{table}\")";
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    names.Add(reader.GetString(1));
            }

            if (names.Contains(column))
                return false;

            await using var alter = connection.CreateCommand();
            alter.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {sqlType};";
            await alter.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static async Task BackfillClassLocalIdsAsync(LocalDbContext db, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            var ids = new List<long>();
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT ClassID FROM Classes WHERE LocalId IS NULL OR LocalId = ''";
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                    ids.Add(reader.GetInt64(0));
            }

            foreach (var id in ids)
            {
                await using var update = connection.CreateCommand();
                update.CommandText = "UPDATE Classes SET LocalId = $id WHERE ClassID = $classId";
                var p1 = update.CreateParameter();
                p1.ParameterName = "$id";
                p1.Value = Guid.NewGuid().ToString();
                update.Parameters.Add(p1);
                var p2 = update.CreateParameter();
                p2.ParameterName = "$classId";
                p2.Value = id;
                update.Parameters.Add(p2);
                await update.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static async Task RebuildClassesPrimaryKeyAsync(LocalDbContext db, CancellationToken cancellationToken)
    {
        var connection = db.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            await connection.OpenAsync(cancellationToken);

        try
        {
            string? pkColumn = null;
            await using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA table_info('Classes')";
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    if (reader.GetInt32(5) == 1)
                        pkColumn = reader.GetString(1);
                }
            }

            if (pkColumn is null || string.Equals(pkColumn, "LocalId", StringComparison.OrdinalIgnoreCase))
                return;

            await using var tx = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var sql in new[]
                {
                    "DROP TABLE IF EXISTS Classes__rebuild;",
                    """
                    CREATE TABLE Classes__rebuild (
                        LocalId TEXT NOT NULL CONSTRAINT PK_Classes PRIMARY KEY,
                        ClassID INTEGER NOT NULL,
                        SchoolID INTEGER NOT NULL,
                        Name TEXT NOT NULL,
                        SortOrder INTEGER NOT NULL,
                        SyncStatus INTEGER NOT NULL DEFAULT 0
                    );
                    """,
                    """
                    INSERT INTO Classes__rebuild (LocalId, ClassID, SchoolID, Name, SortOrder, SyncStatus)
                    SELECT LocalId, ClassID, SchoolID, Name, SortOrder, COALESCE(SyncStatus, 0)
                    FROM Classes;
                    """,
                    "DROP TABLE Classes;",
                    "ALTER TABLE Classes__rebuild RENAME TO Classes;",
                    "CREATE INDEX IF NOT EXISTS IX_Classes_ClassID ON Classes(ClassID);"
                })
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = tx;
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }
}
