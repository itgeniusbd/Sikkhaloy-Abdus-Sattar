-- SQLite migration for AttendanceDevice multi-schedule support.
-- NOTE: AttendanceDevice now runs this automatically on startup (SqliteMultiScheduleMigration).
-- Manual run is only needed if you must upgrade the DB while the app is closed.

-- 1. Add ScheduleID column to Attendance_Record if it doesn't exist
ALTER TABLE Attendance_Record ADD COLUMN ScheduleID INTEGER DEFAULT 0;

-- 2. Back-fill ScheduleID from the legacy single ScheduleID stored on User_Info.
--    Default any existing attendance record whose ScheduleID is 0 to the user's ScheduleID.
UPDATE Attendance_Record
SET ScheduleID = (
    SELECT ScheduleID FROM User_Info WHERE User_Info.DeviceID = Attendance_Record.DeviceID
)
WHERE ScheduleID = 0 OR ScheduleID IS NULL;

-- 3. Create the User_Schedule table which holds every schedule-device mapping.
CREATE TABLE IF NOT EXISTS User_Schedule (
    UserScheduleID INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceID INTEGER NOT NULL,
    ScheduleID INTEGER NOT NULL,
    Is_Student INTEGER NOT NULL DEFAULT 1
);

-- 4. Seed User_Schedule from existing single-schedule User_Info rows.
INSERT INTO User_Schedule (DeviceID, ScheduleID, Is_Student)
SELECT DeviceID, ScheduleID, Is_Student
FROM User_Info
WHERE ScheduleID IS NOT NULL AND ScheduleID > 0;

-- 5. Ensure every existing attendance record has a real ScheduleID value.
UPDATE Attendance_Record
SET ScheduleID = 0
WHERE ScheduleID IS NULL;
