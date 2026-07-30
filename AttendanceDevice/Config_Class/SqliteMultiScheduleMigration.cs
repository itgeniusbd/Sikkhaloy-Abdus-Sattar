using System;
using System.Data.SQLite;
using System.IO;

namespace AttendanceDevice.Config_Class
{
    /// <summary>
    /// Upgrades an existing SikkhaloyAppDB.db for multi-schedule support on app startup.
    /// Safe to run repeatedly; no-op when schema is already current.
    /// </summary>
    internal static class SqliteMultiScheduleMigration
    {
        public static void EnsureApplied()
        {
            string dbPath = AppPaths.DatabasePath;

            if (!File.Exists(dbPath))
                return;

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        Apply(conn, tx);
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void Apply(SQLiteConnection conn, SQLiteTransaction tx)
        {
            Execute(conn, tx, @"
CREATE TABLE IF NOT EXISTS User_Schedule (
    UserScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID INTEGER NOT NULL,
    ScheduleID INTEGER NOT NULL,
    Is_Student INTEGER NOT NULL DEFAULT 1
)");

            if (TableExists(conn, tx, "User_Info") && TableIsEmpty(conn, tx, "User_Schedule"))
            {
                Execute(conn, tx, @"
INSERT INTO User_Schedule (DeviceID, ScheduleID, Is_Student)
SELECT DeviceID, ScheduleID, Is_Student
FROM User_Info
WHERE ScheduleID IS NOT NULL AND ScheduleID > 0");
            }

            if (!TableExists(conn, tx, "Attendance_Record"))
                return;

            if (!ColumnExists(conn, tx, "Attendance_Record", "ScheduleID"))
            {
                TryAddColumn(conn, tx, "Attendance_Record", "ScheduleID INTEGER DEFAULT 0");
            }

            if (TableExists(conn, tx, "User_Info"))
            {
                Execute(conn, tx, @"
UPDATE Attendance_Record
SET ScheduleID = (
    SELECT ScheduleID FROM User_Info WHERE User_Info.DeviceID = Attendance_Record.DeviceID
)
WHERE (ScheduleID = 0 OR ScheduleID IS NULL)
  AND EXISTS (
      SELECT 1 FROM User_Info WHERE User_Info.DeviceID = Attendance_Record.DeviceID
  )");
            }

            Execute(conn, tx, "UPDATE Attendance_Record SET ScheduleID = 0 WHERE ScheduleID IS NULL");

            if (TableExists(conn, tx, "Schedule_Day") && !ColumnExists(conn, tx, "Schedule_Day", "ScheduleName"))
            {
                TryAddColumn(conn, tx, "Schedule_Day", "ScheduleName TEXT");
            }

            if (TableExists(conn, tx, "Institution_Info") && !ColumnExists(conn, tx, "Institution_Info", "ServerTodayDate"))
            {
                TryAddColumn(conn, tx, "Institution_Info", "ServerTodayDate TEXT");
            }
        }

        private static bool TableExists(SQLiteConnection conn, SQLiteTransaction tx, string tableName)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = @name";
                cmd.Parameters.AddWithValue("@name", tableName);
                return cmd.ExecuteScalar() != null;
            }
        }

        private static bool TableIsEmpty(SQLiteConnection conn, SQLiteTransaction tx, string tableName)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = $"SELECT COUNT(*) FROM {tableName}";
                return Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
        }

        private static bool ColumnExists(SQLiteConnection conn, SQLiteTransaction tx, string tableName, string columnName)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = $"PRAGMA table_info({tableName})";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader["name"].ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
            }

            return false;
        }

        private static void TryAddColumn(SQLiteConnection conn, SQLiteTransaction tx, string tableName, string columnDefinition)
        {
            try
            {
                Execute(conn, tx, $"ALTER TABLE {tableName} ADD COLUMN {columnDefinition}");
            }
            catch (SQLiteException)
            {
                // Column may already exist on some DB builds.
            }
        }

        private static void Execute(SQLiteConnection conn, SQLiteTransaction tx, string sql)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
