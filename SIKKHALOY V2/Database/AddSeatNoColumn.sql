-- =============================================
-- Add SeatNo Column to StudentsClass Table
-- Date: 2024
-- Description: Adds SeatNo field next to RollNo for student seat number management
-- =============================================

USE [Edu]
GO

PRINT '========================================='
PRINT 'Adding SeatNo Column to StudentsClass'
PRINT 'Starting: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='

-- Check if SeatNo column already exists
IF NOT EXISTS (SELECT 1 FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.StudentsClass') 
               AND name = 'SeatNo')
BEGIN
    -- Add SeatNo column to StudentsClass table
    ALTER TABLE dbo.StudentsClass
    ADD SeatNo NVARCHAR(50) NULL
    
    PRINT '? SeatNo column added to StudentsClass table'
END
ELSE
BEGIN
    PRINT '- SeatNo column already exists in StudentsClass table'
END
GO

-- Create index for better performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes 
               WHERE name = 'IX_StudentsClass_SeatNo')
BEGIN
    CREATE NONCLUSTERED INDEX IX_StudentsClass_SeatNo
    ON dbo.StudentsClass(SeatNo)
    INCLUDE (StudentClassID, StudentID, ClassID)
    
    PRINT '? Index IX_StudentsClass_SeatNo created'
END
ELSE
BEGIN
    PRINT '- Index IX_StudentsClass_SeatNo already exists'
END
GO

PRINT ''
PRINT '========================================='
PRINT 'SeatNo Column Addition Complete'
PRINT 'Completed: ' + CONVERT(VARCHAR, GETDATE(), 120)
PRINT '========================================='
GO
