-- =============================================
-- Add Expense Sub-Category Support
-- Run this script once on the database
-- =============================================

-- 1. Create Expense_SubCategory table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Expense_SubCategory')
BEGIN
    CREATE TABLE Expense_SubCategory (
        ExpenseSubCategoryID INT IDENTITY(1,1) PRIMARY KEY,
        ExpenseCategoryID    INT NOT NULL,
        SubCategoryName      NVARCHAR(200) NOT NULL,
        SchoolID             INT NOT NULL,
        RegistrationID       INT NULL,
        CONSTRAINT FK_ExpenseSubCategory_Category FOREIGN KEY (ExpenseCategoryID)
            REFERENCES Expense_CategoryName(ExpenseCategoryID)
    );
    PRINT 'Expense_SubCategory table created.';
END
ELSE
    PRINT 'Expense_SubCategory table already exists.';

-- 2. Add ExpenseSubCategoryID column to Expenditure table (nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('Expenditure') AND name = 'ExpenseSubCategoryID'
)
BEGIN
    ALTER TABLE Expenditure ADD ExpenseSubCategoryID INT NULL;
    PRINT 'ExpenseSubCategoryID column added to Expenditure.';
END
ELSE
    PRINT 'ExpenseSubCategoryID column already exists in Expenditure.';
