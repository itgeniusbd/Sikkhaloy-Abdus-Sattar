using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ZKTeco.PushAPI.Models;

namespace ZKTeco.PushAPI.DataAccess
{
    /// <summary>
    /// Attendance data repository for database operations
    /// </summary>
    public class AttendanceRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public AttendanceRepository()
        {
            _dbConnection = new DatabaseConnection();
        }

        /// <summary>
        /// Get device user info by DeviceID
        /// </summary>
        public DeviceUserInfo GetDeviceUserInfo(int deviceID, int schoolID)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                // First try to find student
                var studentQuery = @"
                    SELECT TOP 1 
                        s.DeviceID,
                        s.SchoolID,
                        1 as IsStudent,
                        s.StudentID as UserID,
                        sc.ClassID,
                        sc.StudentClassID,
                        sc.EducationYearID,
                        sc.ScheduleID,
                        s.StudentsName as UserName,
                        s.SMSPhoneNo as PhoneNumber
                    FROM Student s
                    INNER JOIN Student_Class sc ON s.StudentID = sc.StudentID
                    INNER JOIN Education_Year ey ON sc.EducationYearID = ey.EducationYearID
                    WHERE s.DeviceID = @DeviceID 
                        AND s.SchoolID = @SchoolID 
                        AND s.Status = 'Active'
                        AND ey.Status = 'True'";

                using (var cmd = new SqlCommand(studentQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DeviceUserInfo
                            {
                                DeviceID = Convert.ToInt32(reader["DeviceID"]),
                                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                IsStudent = true,
                                UserID = Convert.ToInt32(reader["UserID"]),
                                ClassID = reader["ClassID"] != DBNull.Value ? Convert.ToInt32(reader["ClassID"]) : (int?)null,
                                StudentClassID = reader["StudentClassID"] != DBNull.Value ? Convert.ToInt32(reader["StudentClassID"]) : (int?)null,
                                EducationYearID = Convert.ToInt32(reader["EducationYearID"]),
                                ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                                UserName = reader["UserName"].ToString(),
                                PhoneNumber = reader["PhoneNumber"]?.ToString()
                            };
                        }
                    }
                }

                // If not student, try to find employee
                var employeeQuery = @"
                    SELECT TOP 1 
                        vw.DeviceID,
                        vw.SchoolID,
                        0 as IsStudent,
                        vw.EmployeeID as UserID,
                        NULL as ClassID,
                        NULL as StudentClassID,
                        (SELECT TOP 1 EducationYearID FROM Education_Year WHERE SchoolID = vw.SchoolID AND Status = 'True') as EducationYearID,
                        ISNULL(eas.ScheduleID, 0) as ScheduleID,
                        vw.FirstName + ' ' + vw.LastName as UserName,
                        vw.Phone as PhoneNumber
                    FROM VW_Emp_Info vw
                    LEFT JOIN Employee_Attendance_Schedule_Assign eas ON vw.EmployeeID = eas.EmployeeID
                    WHERE vw.DeviceID = @DeviceID 
                        AND vw.SchoolID = @SchoolID 
                        AND vw.Job_Status = 'Active'";

                using (var cmd = new SqlCommand(employeeQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceID", deviceID);
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DeviceUserInfo
                            {
                                DeviceID = Convert.ToInt32(reader["DeviceID"]),
                                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                IsStudent = false,
                                UserID = Convert.ToInt32(reader["UserID"]),
                                ClassID = null,
                                StudentClassID = null,
                                EducationYearID = Convert.ToInt32(reader["EducationYearID"]),
                                ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                                UserName = reader["UserName"].ToString(),
                                PhoneNumber = reader["PhoneNumber"]?.ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get schedule information
        /// </summary>
        public ScheduleInfo GetScheduleInfo(int scheduleID, string dayName)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                var query = @"
                    SELECT 
                        ScheduleID,
                        SchoolID,
                        Day,
                        StartTime,
                        LateEntryTime,
                        EndTime,
                        Is_OnDay
                    FROM Attendance_Schedule_Day
                    WHERE ScheduleID = @ScheduleID AND Day = @Day";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleID);
                    cmd.Parameters.AddWithValue("@Day", dayName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new ScheduleInfo
                            {
                                ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                Day = reader["Day"].ToString(),
                                StartTime = (TimeSpan)reader["StartTime"],
                                LateEntryTime = (TimeSpan)reader["LateEntryTime"],
                                EndTime = (TimeSpan)reader["EndTime"],
                                Is_OnDay = Convert.ToBoolean(reader["Is_OnDay"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get attendance device settings
        /// </summary>
        public AttendanceDeviceSettings GetDeviceSettings(int schoolID)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                var query = @"
                    SELECT 
                        SchoolID,
                        Is_Device_Attendance_Enable,
                        Is_Student_Attendance_Enable,
                        Is_Employee_Attendance_Enable,
                        Is_Holiday_As_Offday
                    FROM Attendance_Device_Settings
                    WHERE SchoolID = @SchoolID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AttendanceDeviceSettings
                            {
                                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                Is_Device_Attendance_Enable = Convert.ToBoolean(reader["Is_Device_Attendance_Enable"]),
                                Is_Student_Attendance_Enable = Convert.ToBoolean(reader["Is_Student_Attendance_Enable"]),
                                Is_Employee_Attendance_Enable = Convert.ToBoolean(reader["Is_Employee_Attendance_Enable"]),
                                Is_Holiday_As_Offday = Convert.ToBoolean(reader["Is_Holiday_As_Offday"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if date is holiday
        /// </summary>
        public bool IsHoliday(int schoolID, DateTime date)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                var query = "SELECT COUNT(*) FROM Holiday WHERE SchoolID = @SchoolID AND HolidayDate = @Date";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SchoolID", schoolID);
                    cmd.Parameters.AddWithValue("@Date", date.Date);

                    var count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Save student attendance record
        /// </summary>
        public async Task<bool> SaveStudentAttendance(StudentAttendanceRecord record)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                await conn.OpenAsync();

                // Check if record exists
                var checkQuery = @"
                    SELECT AttendanceRecordID 
                    FROM Attendance_Record 
                    WHERE SchoolID = @SchoolID 
                        AND StudentID = @StudentID 
                        AND AttendanceDate = @AttendanceDate";

                int? existingRecordID = null;

                using (var checkCmd = new SqlCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@SchoolID", record.SchoolID);
                    checkCmd.Parameters.AddWithValue("@StudentID", record.StudentID);
                    checkCmd.Parameters.AddWithValue("@AttendanceDate", record.AttendanceDate.Date);

                    var result = await checkCmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        existingRecordID = Convert.ToInt32(result);
                    }
                }

                if (existingRecordID.HasValue)
                {
                    // Update existing record
                    var updateQuery = @"
                        UPDATE Attendance_Record 
                        SET 
                            Attendance = @Attendance,
                            EntryTime = @EntryTime,
                            ExitStatus = @ExitStatus,
                            ExitTime = @ExitTime,
                            Is_OUT = @Is_OUT
                        WHERE AttendanceRecordID = @AttendanceRecordID";

                    using (var updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@Attendance", record.Attendance ?? "");
                        updateCmd.Parameters.AddWithValue("@EntryTime", (object)record.EntryTime ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@ExitStatus", record.ExitStatus ?? "");
                        updateCmd.Parameters.AddWithValue("@ExitTime", (object)record.ExitTime ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("@Is_OUT", record.Is_OUT);
                        updateCmd.Parameters.AddWithValue("@AttendanceRecordID", existingRecordID.Value);

                        await updateCmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    // Insert new record
                    var insertQuery = @"
                        INSERT INTO Attendance_Record 
                        (SchoolID, ClassID, EducationYearID, StudentID, StudentClassID, 
                         Attendance, AttendanceDate, EntryTime, ExitStatus, ExitTime, Is_OUT)
                        VALUES 
                        (@SchoolID, @ClassID, @EducationYearID, @StudentID, @StudentClassID,
                         @Attendance, @AttendanceDate, @EntryTime, @ExitStatus, @ExitTime, @Is_OUT)";

                    using (var insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@SchoolID", record.SchoolID);
                        insertCmd.Parameters.AddWithValue("@ClassID", record.ClassID);
                        insertCmd.Parameters.AddWithValue("@EducationYearID", record.EducationYearID);
                        insertCmd.Parameters.AddWithValue("@StudentID", record.StudentID);
                        insertCmd.Parameters.AddWithValue("@StudentClassID", record.StudentClassID);
                        insertCmd.Parameters.AddWithValue("@Attendance", record.Attendance ?? "");
                        insertCmd.Parameters.AddWithValue("@AttendanceDate", record.AttendanceDate.Date);
                        insertCmd.Parameters.AddWithValue("@EntryTime", (object)record.EntryTime ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@ExitStatus", record.ExitStatus ?? "");
                        insertCmd.Parameters.AddWithValue("@ExitTime", (object)record.ExitTime ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@Is_OUT", record.Is_OUT);

                        await insertCmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Save employee attendance record
        /// </summary>
        public async Task<bool> SaveEmployeeAttendance(EmployeeAttendanceRecord record)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    await conn.OpenAsync();

                    // Format date as dd-MM-yyyy for consistency
                    var dateString = record.AttendanceDate.ToString("dd-MM-yyyy");

                    var query = @"
                        IF NOT EXISTS (
                            SELECT 1 FROM Employee_Attendance_Record 
                            WHERE EmployeeID = @EmployeeID 
                            AND CAST(AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)
                            AND SchoolID = @SchoolID
                        )
                        BEGIN
                            INSERT INTO Employee_Attendance_Record (
                                SchoolID, RegistrationID, EmployeeID, AttendanceStatus, 
                                AttendanceDate, EntryTime, ExitTime, ExitStatus, Is_OUT
                            ) VALUES (
                                @SchoolID, @RegistrationID, @EmployeeID, @AttendanceStatus,
                                CONVERT(DATE, @AttendanceDate, 105), @EntryTime, @ExitTime, @ExitStatus, @Is_OUT
                            )
                        END
                        ELSE
                        BEGIN
                            UPDATE Employee_Attendance_Record 
                            SET EntryTime = @EntryTime,
                                ExitTime = @ExitTime,
                                AttendanceStatus = @AttendanceStatus,
                                ExitStatus = @ExitStatus,
                                Is_OUT = @Is_OUT
                            WHERE EmployeeID = @EmployeeID 
                            AND CAST(AttendanceDate AS DATE) = CAST(@AttendanceDate AS DATE)
                            AND SchoolID = @SchoolID
                        END";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SchoolID", record.SchoolID);
                        cmd.Parameters.AddWithValue("@RegistrationID", record.RegistrationID);
                        cmd.Parameters.AddWithValue("@EmployeeID", record.EmployeeID);
                        cmd.Parameters.AddWithValue("@AttendanceStatus", record.AttendanceStatus ?? "Pre");
                        cmd.Parameters.AddWithValue("@AttendanceDate", dateString);
                        cmd.Parameters.AddWithValue("@EntryTime", record.EntryTime);
                        cmd.Parameters.AddWithValue("@ExitTime", record.ExitTime ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ExitStatus", record.ExitStatus ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Is_OUT", record.Is_OUT);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    conn.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get SchoolID from Device Serial Number
        /// Device_Institution_Mapping table থেকে SchoolID খুঁজে বের করে
        /// </summary>
        public int? GetSchoolIDFromDeviceSerial(string deviceSerial)
        {
            if (string.IsNullOrEmpty(deviceSerial)) return null;

            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                // First try new mapping table
                var mappingQuery = @"
                    SELECT TOP 1 SchoolID 
                    FROM Device_Institution_Mapping 
                    WHERE DeviceSerialNumber = @DeviceSerial 
                        AND IsActive = 1";

                using (var cmd = new SqlCommand(mappingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        // Update last push time
                        UpdateDeviceLastPushTime(deviceSerial);
                        return Convert.ToInt32(result);
                    }
                }

                // Fallback: Try old Devices table (for backward compatibility)
                var deviceQuery = "SELECT TOP 1 SchoolID FROM Devices WHERE DeviceSN = @DeviceSerial";

                using (var cmd = new SqlCommand(deviceQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Update device last push time
        /// </summary>
        public void UpdateDeviceLastPushTime(string deviceSerial)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();
                    var query = @"
                        UPDATE Device_Institution_Mapping 
                        SET LastPushTime = GETDATE() 
                        WHERE DeviceSerialNumber = @DeviceSerial";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Log error but don't throw
            }
        }

        /// <summary>
        /// Get device details by serial number
        /// </summary>
        public DeviceInfo GetDeviceInfo(string deviceSerial)
        {
            using (var conn = _dbConnection.GetConnection())
            {
                conn.Open();

                var query = @"
                    SELECT 
                        m.MappingID,
                        m.SchoolID,
                        m.DeviceSerialNumber,
                        m.DeviceName,
                        m.DeviceLocation,
                        m.IsActive,
                        m.LastPushTime,
                        s.SchoolName
                    FROM Device_Institution_Mapping m
                    INNER JOIN SchoolInfo s ON m.SchoolID = s.SchoolID
                    WHERE m.DeviceSerialNumber = @DeviceSerial AND m.IsActive = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DeviceInfo
                            {
                                MappingID = Convert.ToInt32(reader["MappingID"]),
                                SchoolID = Convert.ToInt32(reader["SchoolID"]),
                                DeviceSerialNumber = reader["DeviceSerialNumber"].ToString(),
                                DeviceName = reader["DeviceName"]?.ToString(),
                                DeviceLocation = reader["DeviceLocation"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                LastPushTime = reader["LastPushTime"] != DBNull.Value 
                                    ? (DateTime?)reader["LastPushTime"] 
                                    : null,
                                SchoolName = reader["SchoolName"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all devices from database
        /// </summary>
        public List<dynamic> GetAllDevices()
        {
            var devices = new List<dynamic>();

            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();
                    var query = @"
                        SELECT 
                            m.MappingID,
                            m.SchoolID,
                            m.DeviceSerialNumber,
                            m.DeviceName,
                            m.DeviceLocation,
                            m.IsActive,
                            m.LastPushTime,
                            m.CreatedDate,
                            s.SchoolName
                        FROM Device_Institution_Mapping m
                        INNER JOIN SchoolInfo s ON m.SchoolID = s.SchoolID
                        ORDER BY m.LastPushTime DESC, m.CreatedDate DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                devices.Add(new
                                {
                                    mappingID = Convert.ToInt32(reader["MappingID"]),
                                    schoolID = Convert.ToInt32(reader["SchoolID"]),
                                    schoolName = reader["SchoolName"].ToString(),
                                    deviceSerial = reader["DeviceSerialNumber"].ToString(),
                                    deviceName = reader["DeviceName"] != DBNull.Value ? reader["DeviceName"].ToString() : null,
                                    deviceLocation = reader["DeviceLocation"] != DBNull.Value ? reader["DeviceLocation"].ToString() : null,
                                    isActive = Convert.ToBoolean(reader["IsActive"]),
                                    lastPushTime = reader["LastPushTime"] != DBNull.Value
                                        ? (DateTime?)Convert.ToDateTime(reader["LastPushTime"])
                                        : null,
                                    createdDate = reader["CreatedDate"] != DBNull.Value
                                        ? (DateTime?)Convert.ToDateTime(reader["CreatedDate"])
                                        : null
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return devices;
        }

        /// <summary>
        /// Store device command for device to fetch on next getrequest
        /// ডিভাইসকে command পাঠানোর জন্য queue এ রাখে
        /// </summary>
        public void StoreDeviceCommand(string deviceSerial, string command)
        {
            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    // Note: This requires a Device_Commands table
                    // You may need to create this table if it doesn't exist
                    var query = @"
                        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Device_Commands')
                        BEGIN
                            CREATE TABLE Device_Commands (
                                CommandID INT IDENTITY(1,1) PRIMARY KEY,
                                DeviceSerialNumber NVARCHAR(100) NOT NULL,
                                Command NVARCHAR(MAX) NOT NULL,
                                CommandStatus NVARCHAR(50) DEFAULT 'Pending',
                                CreatedDate DATETIME DEFAULT GETDATE(),
                                ProcessedDate DATETIME NULL
                            )
                        END

                        INSERT INTO Device_Commands (DeviceSerialNumber, Command, CommandStatus)
                        VALUES (@DeviceSerial, @Command, 'Pending')";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);
                        cmd.Parameters.AddWithValue("@Command", command);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error - command storage failed
                System.Diagnostics.Debug.WriteLine($"StoreDeviceCommand Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get pending commands for device
        /// ডিভাইসের জন্য pending commands fetch করে
        /// </summary>
        public List<string> GetPendingDeviceCommands(string deviceSerial)
        {
            var commands = new List<string>();

            try
            {
                using (var conn = _dbConnection.GetConnection())
                {
                    conn.Open();

                    var query = @"
                        SELECT CommandID, Command 
                        FROM Device_Commands 
                        WHERE DeviceSerialNumber = @DeviceSerial 
                            AND CommandStatus = 'Pending'
                        ORDER BY CreatedDate ASC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                commands.Add(reader["Command"].ToString());
                            }
                        }
                    }

                    // Mark commands as processed
                    if (commands.Count > 0)
                    {
                        var updateQuery = @"
                            UPDATE Device_Commands 
                            SET CommandStatus = 'Sent', ProcessedDate = GETDATE()
                            WHERE DeviceSerialNumber = @DeviceSerial 
                                AND CommandStatus = 'Pending'";

                        using (var updateCmd = new SqlCommand(updateQuery, conn))
                        {
                        updateCmd.Parameters.AddWithValue("@DeviceSerial", deviceSerial);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"GetPendingDeviceCommands Error: {ex.Message}");
            }

            return commands;
        }
    }
}
