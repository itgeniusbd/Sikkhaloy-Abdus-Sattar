-- Repair Pre/Late/Late Abs rows that have midnight (12:00 AM) EntryTime — not a real punch.
-- Example school: Luxmipur Khatune Zannat Mohila Madrasha (SchoolID 1233).
-- After running: re-download today's punches from the device and Send to Website.

DECLARE @SchoolID INT = 1233; -- change if needed
DECLARE @AttendanceDate DATE = CAST(GETDATE() AS DATE);

UPDATE Attendance_Record
SET Attendance = N'Abs',
    EntryTime = NULL
WHERE SchoolID = @SchoolID
  AND CAST(AttendanceDate AS DATE) = @AttendanceDate
  AND Attendance IN (N'Pre', N'Late', N'Late Abs')
  AND (
        EntryTime IS NULL
        OR CAST(EntryTime AS time) = CAST('00:00:00' AS time)
      );

SELECT @@ROWCOUNT AS StudentRowsFixed;
