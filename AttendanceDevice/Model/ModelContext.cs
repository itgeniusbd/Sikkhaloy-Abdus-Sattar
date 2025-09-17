
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Reflection;


namespace AttendanceDevice.Model
{
    class ModelContext : DbContext
    {
        public ModelContext() : base(new SQLiteConnection(@"Data Source=" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SikkhaloyAppDB.db"), true) { }

        public DbSet<User> Users { get; set; }
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
