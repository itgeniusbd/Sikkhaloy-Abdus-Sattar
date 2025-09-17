using System.Data.Entity;

namespace Attendance_API.DB_Model
{
    public class EduContext : DbContext
    {
        public EduContext() : base("name=EduConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //throw new UnintentionalCodeFirstException();
        }
        public virtual DbSet<VW_Attendance_Users> VW_Attendance_Users { get; set; }
        public virtual DbSet<SchoolInfo> SchoolInfos { get; set; }
        public virtual DbSet<Attendance_Device_Setting> Attendance_Device_Settings { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<Attendance_Schedule_Day> Attendance_Schedule_Days { get; set; }
        public virtual DbSet<VW_Attendance_User_Leave> Attendance_User_Leaves { get; set; }
        public virtual DbSet<VW_Attendance_Stu> VW_Attendance_Stus { get; set; }
        public virtual DbSet<Attendance_Record> Attendance_Records { get; set; }
        public virtual DbSet<VW_Attendance_Stu_Setting> VW_Attendance_Stu_Settings { get; set; }
        public virtual DbSet<VW_Attendance_Emp_Setting> VW_Attendance_Emp_Settings { get; set; }
        public virtual DbSet<Attendance_SMS> Attendance_sms { get; set; }
        public virtual DbSet<Employee_Attendance_Record> Employee_Attendance_Records { get; set; }
        public virtual DbSet<VW_Emp_Info> VW_Emp_Infos { get; set; }
        public virtual DbSet<Attendance_Device_DataUpdateList> Attendance_Device_DataUpdateLists { get; set; }
        public virtual DbSet<Device_Finger_Print_Record> Device_Finger_Print_Records { get; set; }
        public virtual DbSet<SMS_Send_Record> SMS_Send_Record { get; set; }
        public virtual DbSet<SMS_OtherInfo> SMS_OtherInfo { get; set; }
    }
}