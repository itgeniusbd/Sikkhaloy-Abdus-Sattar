using Education;
using EDUCATION.COM.API.Attendance_DataSetTableAdapters;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;

namespace EDUCATION.COM.API
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    [System.Web.Script.Services.ScriptService]
    public class Device_Attendance : System.Web.Services.WebService
    {
        [WebMethod]
        public DataTable Get_Users(int SchoolID, string API_PIN)
        {
            All_UserTableAdapter Users = new All_UserTableAdapter();
            if (Valid_API(SchoolID, API_PIN))
            {
                return Users.GetData(SchoolID);
            }
            else
            {
                DataTable Null_Table = new DataTable("No_Value");
                return Null_Table;
            }
        }

        [WebMethod]
        public DataTable API_Validity(string UserName, string Password)
        {
            API_ConfigTableAdapter API = new API_ConfigTableAdapter();

            return API.GetData(UserName, Password);
        }

        [WebMethod]
        public void Set_Attendance_Record(string Category, int UserID, int SchoolID, DateTime Attendance_Date, string API_PIN)
        {
            if (Valid_API(SchoolID, API_PIN))
            {
                if (Is_Device_Attendance_Enable(SchoolID, API_PIN))
                {
                    if (Category == "Student")
                    {
                        Attendance_Students_APITableAdapter Stu = new Attendance_Students_APITableAdapter();
                        Stu.GetData(SchoolID, Attendance_Date, UserID);

                        #region SMS_Send

                        SMS_Class SMS = new SMS_Class(SchoolID.ToString());
                        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());

                        con.Open();
                        SqlCommand SMS_Sending_Status_Command = new SqlCommand("SELECT Attendance_Schedule.StartTime, Attendance_Schedule.EndTime, Attendance_Schedule_AssignStudent.Is_Abs_SMS, Attendance_Schedule_AssignStudent.Is_Late_SMS, Attendance_Schedule_AssignStudent.Entry_Confirmation, Attendance_Schedule_AssignStudent.Exit_Confirmation FROM Attendance_Schedule_AssignStudent INNER JOIN Education_Year ON Attendance_Schedule_AssignStudent.EducationYearID = Education_Year.EducationYearID INNER JOIN Attendance_Schedule ON Attendance_Schedule_AssignStudent.ScheduleID = Attendance_Schedule.ScheduleID INNER JOIN  Student ON Attendance_Schedule_AssignStudent.StudentID = Student.StudentID WHERE (Education_Year.Status = N'True') AND (Attendance_Schedule_AssignStudent.StudentID = @StudentID) AND (Attendance_Schedule_AssignStudent.SchoolID = @SchoolID) AND (Student.Status = N'Active')", con);
                        SMS_Sending_Status_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                        SMS_Sending_Status_Command.Parameters.AddWithValue("@StudentID", UserID);

                        SqlDataReader SMS_Status_DR;
                        SMS_Status_DR = SMS_Sending_Status_Command.ExecuteReader();

                        DateTime StartTime = DateTime.Now;
                        DateTime EndTime = DateTime.Now;
                        bool Entry_Confirmation = false;
                        bool Exit_Confirmation = false;
                        bool Is_Abs_SMS = false;
                        bool Is_Late_SMS = false;

                        while (SMS_Status_DR.Read())
                        {
                            Entry_Confirmation = Convert.ToBoolean(SMS_Status_DR["Entry_Confirmation"]);
                            Exit_Confirmation = Convert.ToBoolean(SMS_Status_DR["Exit_Confirmation"]);
                            Is_Abs_SMS = Convert.ToBoolean(SMS_Status_DR["Is_Abs_SMS"]);
                            Is_Late_SMS = Convert.ToBoolean(SMS_Status_DR["Is_Late_SMS"]);
                            StartTime = Convert.ToDateTime(SMS_Status_DR["StartTime"].ToString());
                            EndTime = Convert.ToDateTime(SMS_Status_DR["EndTime"].ToString());
                        }
                        con.Close();


                        con.Open();
                        SqlCommand Stu_Attendence_Command = new SqlCommand("SELECT Attendance_Record.AttendanceRecordID, SchoolInfo.SchoolName, Student.ID, Student.StudentsName, Student.SMSPhoneNo, Attendance_Record.Attendance, Attendance_Record.AttendanceDate,  Attendance_Record.EntryTime, Attendance_Record.ExitTime, Attendance_Record.Is_Entry_SMS_Sent, Attendance_Record.Is_ExitTime_SMS_Sent, Attendance_Record.ExitConfirmed_Status FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN SchoolInfo ON Attendance_Record.SchoolID = SchoolInfo.SchoolID WHERE (Attendance_Record.StudentID = @StudentID) AND (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.AttendanceDate = @AttendanceDate)", con);
                        Stu_Attendence_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                        Stu_Attendence_Command.Parameters.AddWithValue("@StudentID", UserID);
                        Stu_Attendence_Command.Parameters.AddWithValue("@AttendanceDate", Attendance_Date.ToShortDateString());

                        SqlDataReader Stu_Attendence_DR;
                        Stu_Attendence_DR = Stu_Attendence_Command.ExecuteReader();

                        string AttendanceRecordID = "";
                        string SchoolName = "";
                        string ID = "";
                        string StudentsName = "";
                        string SMSPhoneNo = "";
                        string Attendance = "";
                        DateTime AttendanceDate = DateTime.Now;
                        DateTime EntryTime = DateTime.Now;
                        DateTime ExitTime = DateTime.Now;
                        bool Is_Entry_SMS_Sent = false;
                        bool Is_ExitTime_SMS_Sent = false;
                        string ExitConfirmed_Status = "";

                        while (Stu_Attendence_DR.Read())
                        {
                            AttendanceRecordID = Stu_Attendence_DR["AttendanceRecordID"].ToString();
                            SchoolName = Stu_Attendence_DR["SchoolName"].ToString();
                            ID = Stu_Attendence_DR["ID"].ToString();
                            StudentsName = Stu_Attendence_DR["StudentsName"].ToString();
                            SMSPhoneNo = Stu_Attendence_DR["SMSPhoneNo"].ToString();
                            Attendance = Stu_Attendence_DR["Attendance"].ToString();
                            AttendanceDate = Convert.ToDateTime(Stu_Attendence_DR["AttendanceDate"].ToString());

                            if (Stu_Attendence_DR["EntryTime"].ToString() != "")
                            {
                                EntryTime = Convert.ToDateTime(Stu_Attendence_DR["EntryTime"].ToString());
                            }

                            if (Stu_Attendence_DR["ExitTime"].ToString() != "")
                            {
                                ExitTime = Convert.ToDateTime(Stu_Attendence_DR["ExitTime"].ToString());
                            }

                            Is_Entry_SMS_Sent = Convert.ToBoolean(Stu_Attendence_DR["Is_Entry_SMS_Sent"]);
                            Is_ExitTime_SMS_Sent = Convert.ToBoolean(Stu_Attendence_DR["Is_ExitTime_SMS_Sent"]);
                            ExitConfirmed_Status = Stu_Attendence_DR["ExitConfirmed_Status"].ToString();
                        }
                        con.Close();

                        #region Pre
                        if (Attendance == "Pre" && Entry_Confirmation && !Is_Entry_SMS_Sent)
                        {
                            string Entry = "Respected Guardian, " + StudentsName + "  has Safely Entered in " + SchoolName + "  at " + EntryTime.ToString("h:mm tt");
                            string Msg = Entry;
                            int TotalSMS = SMS.SMS_Conut(Msg);
                            int SMSBalance = SMS.SMSBalance;

                            if (SMSBalance >= TotalSMS)
                            {
                                if (SMS.SMS_GetBalance() >= TotalSMS)
                                {
                                    Get_Validation IsValid = SMS.SMS_Validation(SMSPhoneNo, Msg);

                                    if (IsValid.Validation)
                                    {
                                        Guid SMS_Send_ID = SMS.SMS_Send(SMSPhoneNo, Msg, "Device Attendence");
                                        if (SMS_Send_ID != Guid.Empty)
                                        {
                                            SqlCommand Insert_SMS_Command = new SqlCommand("INSERT INTO SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID) SELECT @SMS_Send_ID, @SchoolID, @StudentID, EducationYearID FROM Education_Year WHERE (Status = N'True') AND (SchoolID = @SchoolID)", con);
                                            Insert_SMS_Command.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID.ToString());
                                            Insert_SMS_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                                            Insert_SMS_Command.Parameters.AddWithValue("@StudentID", UserID);

                                            SqlCommand Update_Att_Command = new SqlCommand("UPDATE Attendance_Record SET  Is_Entry_SMS_Sent =1 WHERE (AttendanceRecordID = @AttendanceRecordID)", con);
                                            Update_Att_Command.Parameters.AddWithValue("@AttendanceRecordID", AttendanceRecordID);

                                            con.Open();
                                            Insert_SMS_Command.ExecuteNonQuery();
                                            Update_Att_Command.ExecuteNonQuery();
                                            con.Close();
                                        }
                                    }
                                }
                            }
                        }

                        #endregion Pre

                        #region Late
                        if (Attendance == "Late" && Is_Late_SMS && !Is_Entry_SMS_Sent)
                        {
                            TimeSpan span = EntryTime.Subtract(StartTime);
                            string Late = "Respected Guardian, " + StudentsName + " Today( " + AttendanceDate.ToString("d MMM yyyy") + " ) Late " + span.Minutes + " min. In " + SchoolName + "  Entered at " + EntryTime.ToString("h:mm tt");
                            string Msg = Late;
                            int TotalSMS = SMS.SMS_Conut(Msg);
                            int SMSBalance = SMS.SMSBalance;

                            if (SMSBalance >= TotalSMS)
                            {
                                if (SMS.SMS_GetBalance() >= TotalSMS)
                                {
                                    Get_Validation IsValid = SMS.SMS_Validation(SMSPhoneNo, Msg);

                                    if (IsValid.Validation)
                                    {
                                        Guid SMS_Send_ID = SMS.SMS_Send(SMSPhoneNo, Msg, "Device Attendence");
                                        if (SMS_Send_ID != Guid.Empty)
                                        {
                                            SqlCommand Insert_SMS_Command = new SqlCommand("INSERT INTO SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID) SELECT @SMS_Send_ID, @SchoolID, @StudentID, EducationYearID FROM Education_Year WHERE (Status = N'True') AND (SchoolID = @SchoolID)", con);

                                            Insert_SMS_Command.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID.ToString());
                                            Insert_SMS_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                                            Insert_SMS_Command.Parameters.AddWithValue("@StudentID", UserID);

                                            SqlCommand Update_Att_Command = new SqlCommand("UPDATE Attendance_Record SET Is_Entry_SMS_Sent = 1 WHERE (AttendanceRecordID = @AttendanceRecordID)", con);

                                            Update_Att_Command.Parameters.AddWithValue("@AttendanceRecordID", AttendanceRecordID);

                                            con.Open();
                                            Insert_SMS_Command.ExecuteNonQuery();
                                            Update_Att_Command.ExecuteNonQuery();
                                            con.Close();
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Late

                        #region Late_Abs
                        if (Attendance == "Late Abs" && !Is_Entry_SMS_Sent)
                        {
                            if (Is_Late_SMS || Entry_Confirmation)
                            {
                                TimeSpan span = EntryTime.Subtract(StartTime);
                                string Late_abs = "Respected Guardian, " + StudentsName + " Today(" + AttendanceDate.ToString("d MMM yyyy") + ") Late " + span.Minutes + " min. (Count as Absent) In " + SchoolName + " Entered at " + EntryTime.ToString("h:mm tt");
                                string Msg = Late_abs;
                                int TotalSMS = SMS.SMS_Conut(Msg);
                                int SMSBalance = SMS.SMSBalance;

                                if (SMSBalance >= TotalSMS)
                                {
                                    if (SMS.SMS_GetBalance() >= TotalSMS)
                                    {
                                        Get_Validation IsValid = SMS.SMS_Validation(SMSPhoneNo, Msg);

                                        if (IsValid.Validation)
                                        {
                                            Guid SMS_Send_ID = SMS.SMS_Send(SMSPhoneNo, Msg, "Device Attendence");
                                            if (SMS_Send_ID != Guid.Empty)
                                            {
                                                SqlCommand Insert_SMS_Command = new SqlCommand("INSERT INTO SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID) SELECT @SMS_Send_ID, @SchoolID, @StudentID, EducationYearID FROM Education_Year WHERE (Status = N'True') AND (SchoolID = @SchoolID)", con);
                                                Insert_SMS_Command.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID.ToString());
                                                Insert_SMS_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                                                Insert_SMS_Command.Parameters.AddWithValue("@StudentID", UserID);

                                                SqlCommand Update_Att_Command = new SqlCommand("UPDATE Attendance_Record SET Is_Entry_SMS_Sent = 1 WHERE (AttendanceRecordID = @AttendanceRecordID)", con);
                                                Update_Att_Command.Parameters.AddWithValue("@AttendanceRecordID", AttendanceRecordID);

                                                con.Open();
                                                Insert_SMS_Command.ExecuteNonQuery();
                                                Update_Att_Command.ExecuteNonQuery();
                                                con.Close();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Late_Abs

                        #region Exit
                        if (ExitConfirmed_Status == "Yes" && Exit_Confirmation && !Is_ExitTime_SMS_Sent)
                        {
                            string Exit = "Respected Guardian, " + StudentsName + " has Left " + SchoolName + " at " + ExitTime.ToString("h:mm tt");
                            string Msg = Exit;
                            int TotalSMS = SMS.SMS_Conut(Msg);
                            int SMSBalance = SMS.SMSBalance;

                            if (SMSBalance >= TotalSMS)
                            {
                                if (SMS.SMS_GetBalance() >= TotalSMS)
                                {
                                    Get_Validation IsValid = SMS.SMS_Validation(SMSPhoneNo, Msg);

                                    if (IsValid.Validation)
                                    {
                                        Guid SMS_Send_ID = SMS.SMS_Send(SMSPhoneNo, Msg, "Device Attendence");
                                        if (SMS_Send_ID != Guid.Empty)
                                        {
                                            SqlCommand Insert_SMS_Command = new SqlCommand("INSERT INTO SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID) SELECT @SMS_Send_ID, @SchoolID, @StudentID, EducationYearID FROM Education_Year WHERE (Status = N'True') AND (SchoolID = @SchoolID)", con);
                                            Insert_SMS_Command.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID.ToString());
                                            Insert_SMS_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                                            Insert_SMS_Command.Parameters.AddWithValue("@StudentID", UserID);

                                            SqlCommand Update_Att_Command = new SqlCommand("UPDATE Attendance_Record SET  Is_ExitTime_SMS_Sent =1 WHERE (AttendanceRecordID = @AttendanceRecordID)", con);
                                            Update_Att_Command.Parameters.AddWithValue("@AttendanceRecordID", AttendanceRecordID);

                                            con.Open();
                                            Insert_SMS_Command.ExecuteNonQuery();
                                            Update_Att_Command.ExecuteNonQuery();
                                            con.Close();
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Exit

                        #region Abs SMS

                        if (Attendance_Date.ToShortDateString() == DateTime.Today.ToShortDateString())
                        {
                            SqlCommand Stu_Abs_Command = new SqlCommand("SELECT Attendance_Record.AttendanceRecordID, Attendance_Record.StudentID, SchoolInfo.SchoolName, Student.ID, Student.StudentsName, Student.SMSPhoneNo, Attendance_Record.Attendance,  Attendance_Record.AttendanceDate, Attendance_Record.EntryTime, Attendance_Record.ExitTime, Attendance_Record.Is_Entry_SMS_Sent, Attendance_Record.Is_ExitTime_SMS_Sent, Attendance_Record.ExitConfirmed_Status, Attendance_Schedule.StartTime, Attendance_Schedule.EndTime,Attendance_Schedule_AssignStudent.Is_Abs_SMS FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN SchoolInfo ON Attendance_Record.SchoolID = SchoolInfo.SchoolID INNER JOIN Attendance_Schedule_AssignStudent ON Student.StudentID = Attendance_Schedule_AssignStudent.StudentID INNER JOIN Attendance_Schedule ON Attendance_Schedule_AssignStudent.ScheduleID = Attendance_Schedule.ScheduleID INNER JOIN Education_Year ON Attendance_Schedule.EducationYearID = Education_Year.EducationYearID WHERE (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.AttendanceDate = @AttendanceDate) AND (Attendance_Record.Attendance = N'Abs') AND (Attendance_Record.Is_Entry_SMS_Sent = 0) AND (Education_Year.Status = N'True')", con);
                            Stu_Abs_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                            Stu_Abs_Command.Parameters.AddWithValue("@AttendanceDate", Attendance_Date.ToShortDateString());

                            con.Open();
                            SqlDataReader Stu_Abs_DR;
                            Stu_Abs_DR = Stu_Abs_Command.ExecuteReader();
                            string StudentID = "";

                            while (Stu_Abs_DR.Read())
                            {
                                AttendanceRecordID = Stu_Abs_DR["AttendanceRecordID"].ToString();
                                StudentID = Stu_Abs_DR["StudentID"].ToString();
                                SchoolName = Stu_Abs_DR["SchoolName"].ToString();
                                ID = Stu_Abs_DR["ID"].ToString();
                                StudentsName = Stu_Abs_DR["StudentsName"].ToString();
                                SMSPhoneNo = Stu_Abs_DR["SMSPhoneNo"].ToString();
                                AttendanceDate = Convert.ToDateTime(Stu_Abs_DR["AttendanceDate"].ToString());
                                Is_Entry_SMS_Sent = Convert.ToBoolean(Stu_Abs_DR["Is_Entry_SMS_Sent"]);
                                Is_ExitTime_SMS_Sent = Convert.ToBoolean(Stu_Abs_DR["Is_ExitTime_SMS_Sent"]);
                                ExitConfirmed_Status = Stu_Abs_DR["ExitConfirmed_Status"].ToString();
                                Is_Abs_SMS = Convert.ToBoolean(Stu_Abs_DR["Is_Abs_SMS"]);

                                if (Is_Abs_SMS)
                                {
                                    string Abs = "Respected Guardian, " + StudentsName + " Today(" + AttendanceDate.ToString("d MMM yyyy") + ") absent from " + SchoolName + ". Please send to class regularly";
                                    string Msg = Abs;
                                    int TotalSMS = SMS.SMS_Conut(Msg);
                                    int SMSBalance = SMS.SMSBalance;

                                    if (SMSBalance >= TotalSMS)
                                    {
                                        if (SMS.SMS_GetBalance() >= TotalSMS)
                                        {
                                            Get_Validation IsValid = SMS.SMS_Validation(SMSPhoneNo, Msg);

                                            if (IsValid.Validation)
                                            {
                                                Guid SMS_Send_ID = SMS.SMS_Send(SMSPhoneNo, Msg, "Device Attendence");
                                                if (SMS_Send_ID != Guid.Empty)
                                                {
                                                    SqlCommand Insert_SMS_Command = new SqlCommand("INSERT INTO SMS_OtherInfo (SMS_Send_ID, SchoolID, StudentID, EducationYearID) SELECT @SMS_Send_ID, @SchoolID, @StudentID, EducationYearID FROM Education_Year WHERE (Status = N'True') AND (SchoolID = @SchoolID)", con);
                                                    Insert_SMS_Command.Parameters.AddWithValue("@SMS_Send_ID", SMS_Send_ID.ToString());
                                                    Insert_SMS_Command.Parameters.AddWithValue("@SchoolID", SchoolID);
                                                    Insert_SMS_Command.Parameters.AddWithValue("@StudentID", StudentID);
                                                    Insert_SMS_Command.ExecuteNonQuery();

                                                    SqlCommand Update_Att_Command = new SqlCommand("UPDATE Attendance_Record SET Is_Entry_SMS_Sent = 1 WHERE (AttendanceRecordID = @AttendanceRecordID)", con);
                                                    Update_Att_Command.Parameters.AddWithValue("@AttendanceRecordID", AttendanceRecordID);
                                                    Update_Att_Command.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            con.Close();
                        }
                        #endregion Abs SMS

                        #endregion SMS_Send
                    }
                    else
                    {
                        Attendance_Employee_APITableAdapter Emp = new Attendance_Employee_APITableAdapter();
                        Emp.GetData(SchoolID, Attendance_Date, UserID);
                    }
                }
            }
        }

        [WebMethod]
        public bool Is_Device_Attendance_Enable(int SchoolID, string API_PIN)
        {
            API_ConfigTableAdapter API = new API_ConfigTableAdapter();
            bool ret;

            if (API.Is_Attendance_Enable(SchoolID, API_PIN).ToString() == "")
            {
                ret = false;
            }
            else
            {
                ret = (bool)API.Is_Attendance_Enable(SchoolID, API_PIN);
            }
            return ret;
        }

        [WebMethod]
        public bool Valid_UserID(string UserName, string Password, int SchoolID, string API_PIN)
        {
            API_ConfigTableAdapter API = new API_ConfigTableAdapter();

            bool ret;

            try
            {
                ret = (bool)API.Validation_UserID(UserName, Password, SchoolID, API_PIN);
            }
            catch
            {
                ret = false;
            }
            return ret;

        }

        [WebMethod]
        public bool Valid_API(int SchoolID, string API_PIN)
        {
            API_ConfigTableAdapter API = new API_ConfigTableAdapter();

            bool ret;

            if (API.Validation_API(SchoolID, API_PIN).ToString() == "")
            {
                ret = false;
            }
            else
            {
                ret = (bool)API.Validation_API(SchoolID, API_PIN);
            }
            return ret;
        }

        [WebMethod]
        public Attendance_DataSet.Device_Finger_Print_RecordDataTable Finger_Print_Record(int SchoolID, string API_PIN)
        {
            Device_Finger_Print_RecordTableAdapter Device_Finger_Print = new Device_Finger_Print_RecordTableAdapter();
            return Device_Finger_Print.GetData(SchoolID, API_PIN);
        }

        [WebMethod]
        public void Finger_Print_insert(int SchoolID, int DeviceID, int Finger_Index, string Temp_Data, int Flag, string API_PIN)
        {
            if (Valid_API(SchoolID, API_PIN))
            {
                Device_Finger_Print_RecordTableAdapter Device_Finger_Print = new Device_Finger_Print_RecordTableAdapter();
                Device_Finger_Print.InsertQuery(SchoolID, DeviceID, Finger_Index, Temp_Data, Flag);
            }
        }

        [WebMethod]
        public void Attendance_Record_Device(string ID, DateTime EntryDateTime, string EntryDate, int Stu_Emp_ID, int DeviceID, string Category, int SchoolID, string API_PIN)
        {
            if (Valid_API(SchoolID, API_PIN))
            {
                All_UserTableAdapter Users = new All_UserTableAdapter();
                Users.Insert_Attendance_Record_Device(ID, EntryDateTime, EntryDate, Stu_Emp_ID, DeviceID, Category, SchoolID);
            }
        }
    }
}
