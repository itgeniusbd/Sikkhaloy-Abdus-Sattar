using System;

namespace ZKTeco.PushAPI.Models
{
    /// <summary>
    /// Student Attendance Record Model
    /// Attendance_Record ??????? ????
    /// </summary>
    public class StudentAttendanceRecord
    {
        public int AttendanceRecordID { get; set; }
        public int SchoolID { get; set; }
        public int ClassID { get; set; }
        public int EducationYearID { get; set; }
        public int StudentID { get; set; }
        public int StudentClassID { get; set; }
        public string Attendance { get; set; } // Pre, Late, Late Abs, Abs
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public string ExitStatus { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public bool Is_OUT { get; set; }
    }

    /// <summary>
    /// Employee Attendance Record Model
    /// Employee_Attendance_Record ??????? ????
    /// </summary>
    public class EmployeeAttendanceRecord
    {
        public int EmployeeAttendanceRecordID { get; set; }
        public int SchoolID { get; set; }
        public int EmployeeID { get; set; }
        public int RegistrationID { get; set; }
        public string AttendanceStatus { get; set; } // Pre, Late, Late Abs, Abs
        public DateTime AttendanceDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public string ExitStatus { get; set; }
        public TimeSpan? ExitTime { get; set; }
        public bool Is_OUT { get; set; }
    }

    /// <summary>
    /// Device User Info
    /// </summary>
    public class DeviceUserInfo
    {
        public int DeviceID { get; set; }
        public int SchoolID { get; set; }
        public bool IsStudent { get; set; }
        public int UserID { get; set; } // StudentID or EmployeeID
        public int? ClassID { get; set; }
        public int? StudentClassID { get; set; }
        public int EducationYearID { get; set; }
        public int ScheduleID { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Schedule Info
    /// </summary>
    public class ScheduleInfo
    {
        public int ScheduleID { get; set; }
        public int SchoolID { get; set; }
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan LateEntryTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool Is_OnDay { get; set; }
    }

    /// <summary>
    /// Device Settings
    /// </summary>
    public class AttendanceDeviceSettings
    {
        public int SchoolID { get; set; }
        public bool Is_Device_Attendance_Enable { get; set; }
        public bool Is_Student_Attendance_Enable { get; set; }
        public bool Is_Employee_Attendance_Enable { get; set; }
        public bool Is_Holiday_As_Offday { get; set; }
    }

    /// <summary>
    /// Parsed Attendance Log from Device
    /// </summary>
    public class DeviceAttendanceLog
    {
        public string DeviceSerial { get; set; }
        public int DeviceID { get; set; }
        public string UserID { get; set; }
        public DateTime AttendanceTime { get; set; }
        public int Status { get; set; } // 0=Check-In, 1=Check-Out
        public int VerifyType { get; set; } // 0=Password, 1=Fingerprint, 2=Card, 15=Face
        public DateTime ReceivedTime { get; set; }
    }

    /// <summary>
    /// Device Information Model
    /// Device-Institution mapping ?? ????
    /// </summary>
    public class DeviceInfo
    {
        public int MappingID { get; set; }
        public int SchoolID { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string DeviceLocation { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastPushTime { get; set; }
        public string SchoolName { get; set; }
        public string Remarks { get; set; }
        
        // Optional properties (for backward compatibility)
        public int DeviceID { get; set; }
        public string DeviceSerial { get; set; }
        public string DeviceIP { get; set; }
        public int Port { get; set; }
        public string DeviceStatus { get; set; }
        public DateTime? LastSyncTime { get; set; }
    }

    /// <summary>
    /// Device Registration Model
    /// </summary>
    public class DeviceRegistrationModel
    {
        public string DeviceSerial { get; set; }
        public string DeviceName { get; set; }
        public string DeviceIP { get; set; }
        public int Port { get; set; }
        public int SchoolID { get; set; }
        public int CommKey { get; set; }
    }

    /// <summary>
    /// Device User Data Model (for sync to device)
    /// </summary>
    public class DeviceUserData
    {
        public int UserDeviceID { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        public string UserType { get; set; } // Student or Employee
        public string Status { get; set; }
    }

    /// <summary>
    /// Device Fingerprint Data Model (for sync to device)
    /// </summary>
    public class DeviceFingerprintData
    {
        public int UserDeviceID { get; set; }
        public int FingerIndex { get; set; }
        public string TemplateData { get; set; }
        public int Flag { get; set; }
        public string UserType { get; set; }
    }
}
