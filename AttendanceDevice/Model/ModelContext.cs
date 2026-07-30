using System.Data.Entity;
using System.Data.SQLite;
using AttendanceDevice.Config_Class;


namespace AttendanceDevice.Model
{
    class ModelContext : DbContext
    {
        static ModelContext()
        {
            SqliteDatabaseBootstrap.EnsureDatabase();
            SqliteMultiScheduleMigration.EnsureApplied();
        }

        public ModelContext() : base(new SQLiteConnection(@"Data Source=" + Config_Class.AppPaths.DatabasePath), true) { }

        public DbSet<User> Users { get; set; }
        public DbSet<User_Schedule> User_Schedules { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<AttendanceLog_Backup> attendanceLog_Backups { get; set; }
        public DbSet<Attendance_Record> attendance_Records { get; set; }
        public DbSet<Attendance_Schedule_Day> attendance_Schedule_Days { get; set; }
        public DbSet<User_Leave_Record> user_Leave_Records { get; set; }
        public DbSet<DataUpdateList> dataUpdateLists { get; set; }
        public DbSet<User_FingerPrint> user_FingerPrints { get; set; }
    }


    public static class EntityExtensions
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}
