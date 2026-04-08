-- Quick delete April 2026 student counts only

DELETE FROM AAP_Student_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026
DELETE FROM AAP_StudentClass_Count_Monthly WHERE MONTH(Month) = 4 AND YEAR(Month) = 2026

PRINT 'April 2026 student count data deleted!'
PRINT 'Now use Generate Student Count button for April 2026'
