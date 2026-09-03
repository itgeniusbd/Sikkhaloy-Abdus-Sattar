-- Inventory module tables (also auto-created by Hybrid InventoryService on first use).
-- Safe to run more than once.

IF OBJECT_ID(N'dbo.Inv_ItemCategory', N'U') IS NULL
CREATE TABLE dbo.Inv_ItemCategory (
    ItemCategoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_ItemCategory_Insert DEFAULT (GETDATE())
);

IF OBJECT_ID(N'dbo.Inv_Item', N'U') IS NULL
CREATE TABLE dbo.Inv_Item (
    ItemID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    ItemCategoryID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Unit NVARCHAR(40) NOT NULL CONSTRAINT DF_Inv_Item_Unit DEFAULT (N'pcs'),
    Sku NVARCHAR(80) NULL,
    MinStock DECIMAL(18,3) NOT NULL CONSTRAINT DF_Inv_Item_Min DEFAULT (0),
    PurchasePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Item_PPrice DEFAULT (0),
    SalePrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Item_SPrice DEFAULT (0),
    IsActive BIT NOT NULL CONSTRAINT DF_Inv_Item_Active DEFAULT (1),
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Item_Insert DEFAULT (GETDATE())
);

IF OBJECT_ID(N'dbo.Inv_Purchase', N'U') IS NULL
CREATE TABLE dbo.Inv_Purchase (
    PurchaseID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    EducationYearID INT NOT NULL,
    RegistrationID INT NOT NULL,
    AccountID INT NULL,
    DocDate DATE NOT NULL,
    InvoiceNo NVARCHAR(80) NULL,
    Supplier NVARCHAR(200) NULL,
    SupplierID INT NULL,
    Note NVARCHAR(500) NULL,
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Total DEFAULT (0),
    PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Paid DEFAULT (0),
    ExpenseID INT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Purchase_Insert DEFAULT (GETDATE())
);

IF OBJECT_ID(N'dbo.Inv_PurchaseLine', N'U') IS NULL
CREATE TABLE dbo.Inv_PurchaseLine (
    PurchaseLineID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PurchaseID INT NOT NULL,
    ItemID INT NOT NULL,
    Qty DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);

IF OBJECT_ID(N'dbo.Inv_Sale', N'U') IS NULL
CREATE TABLE dbo.Inv_Sale (
    SaleID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    EducationYearID INT NOT NULL,
    RegistrationID INT NOT NULL,
    AccountID INT NOT NULL,
    DocDate DATE NOT NULL,
    InvoiceNo NVARCHAR(80) NULL,
    Customer NVARCHAR(200) NULL,
    Note NVARCHAR(500) NULL,
    Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Sale_Total DEFAULT (0),
    ExtraIncomeID INT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Sale_Insert DEFAULT (GETDATE())
);

IF OBJECT_ID(N'dbo.Inv_SaleLine', N'U') IS NULL
CREATE TABLE dbo.Inv_SaleLine (
    SaleLineID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SaleID INT NOT NULL,
    ItemID INT NOT NULL,
    Qty DECIMAL(18,3) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL
);

IF OBJECT_ID(N'dbo.Inv_Log', N'U') IS NOT NULL
    DROP TABLE dbo.Inv_Log;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Item_School' AND object_id = OBJECT_ID(N'dbo.Inv_Item'))
    CREATE INDEX IX_Inv_Item_School ON dbo.Inv_Item (SchoolID, ItemCategoryID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Purchase_School' AND object_id = OBJECT_ID(N'dbo.Inv_Purchase'))
    CREATE INDEX IX_Inv_Purchase_School ON dbo.Inv_Purchase (SchoolID, EducationYearID, DocDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Sale_School' AND object_id = OBJECT_ID(N'dbo.Inv_Sale'))
    CREATE INDEX IX_Inv_Sale_School ON dbo.Inv_Sale (SchoolID, EducationYearID, DocDate);

IF OBJECT_ID(N'dbo.Inv_Supplier', N'U') IS NULL
CREATE TABLE dbo.Inv_Supplier (
    SupplierID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    Address NVARCHAR(400) NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Supplier_Insert DEFAULT (GETDATE())
);

IF OBJECT_ID(N'dbo.Inv_SupplierPayment', N'U') IS NULL
CREATE TABLE dbo.Inv_SupplierPayment (
    PaymentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    SupplierID INT NOT NULL,
    PurchaseID INT NULL,
    AccountID INT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    DocDate DATE NOT NULL,
    Note NVARCHAR(500) NULL,
    ExpenseID INT NULL,
    RegistrationID INT NOT NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_SupplierPayment_Insert DEFAULT (GETDATE())
);

IF COL_LENGTH(N'dbo.Inv_Item', N'MinStock') IS NULL
    ALTER TABLE dbo.Inv_Item ADD MinStock DECIMAL(18,3) NOT NULL CONSTRAINT DF_Inv_Item_Min DEFAULT (0);
IF COL_LENGTH(N'dbo.Inv_Item', N'Sku') IS NULL
    ALTER TABLE dbo.Inv_Item ADD Sku NVARCHAR(80) NULL;
IF COL_LENGTH(N'dbo.Inv_Purchase', N'SupplierID') IS NULL
    ALTER TABLE dbo.Inv_Purchase ADD SupplierID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Purchase', N'PaidAmount') IS NULL
    ALTER TABLE dbo.Inv_Purchase ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Purchase_Paid DEFAULT (0);
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inv_Purchase') AND name = N'AccountID' AND is_nullable = 0)
    ALTER TABLE dbo.Inv_Purchase ALTER COLUMN AccountID INT NULL;
GO

-- UPDATE must be a new batch (or dynamic SQL). SQL Server compiles the whole batch first,
-- so PaidAmount is "invalid" if it was only added above in the same batch.
IF COL_LENGTH(N'dbo.Inv_Purchase', N'PaidAmount') IS NOT NULL
    EXEC(N'UPDATE dbo.Inv_Purchase SET PaidAmount = Total WHERE ExpenseID IS NOT NULL AND ISNULL(PaidAmount, 0) = 0 AND Total > 0');

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Supplier_School' AND object_id = OBJECT_ID(N'dbo.Inv_Supplier'))
    CREATE INDEX IX_Inv_Supplier_School ON dbo.Inv_Supplier (SchoolID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Purchase_Supplier' AND object_id = OBJECT_ID(N'dbo.Inv_Purchase'))
    CREATE INDEX IX_Inv_Purchase_Supplier ON dbo.Inv_Purchase (SchoolID, SupplierID, DocDate);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_SupplierPayment_School' AND object_id = OBJECT_ID(N'dbo.Inv_SupplierPayment'))
    CREATE INDEX IX_Inv_SupplierPayment_School ON dbo.Inv_SupplierPayment (SchoolID, SupplierID, DocDate);

IF OBJECT_ID(N'dbo.Inv_Customer', N'U') IS NULL
CREATE TABLE dbo.Inv_Customer (
    CustomerID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SchoolID INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    StudentID INT NULL,
    StudentCode NVARCHAR(50) NULL,
    ClassName NVARCHAR(80) NULL,
    InsertDate DATETIME NOT NULL CONSTRAINT DF_Inv_Customer_Insert DEFAULT (GETDATE())
);

IF COL_LENGTH(N'dbo.Inv_Sale', N'CustomerID') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD CustomerID INT NULL;
IF COL_LENGTH(N'dbo.Inv_Sale', N'PaidAmount') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD PaidAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Inv_Sale_Paid DEFAULT (0);
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Inv_Sale') AND name = N'AccountID' AND is_nullable = 0)
    ALTER TABLE dbo.Inv_Sale ALTER COLUMN AccountID INT NULL;
GO
IF COL_LENGTH(N'dbo.Inv_Sale', N'PaidAmount') IS NOT NULL
    EXEC(N'UPDATE dbo.Inv_Sale SET PaidAmount = Total WHERE ExtraIncomeID IS NOT NULL AND ISNULL(PaidAmount, 0) = 0 AND Total > 0');
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Customer_School' AND object_id = OBJECT_ID(N'dbo.Inv_Customer'))
    CREATE INDEX IX_Inv_Customer_School ON dbo.Inv_Customer (SchoolID, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inv_Sale_Customer' AND object_id = OBJECT_ID(N'dbo.Inv_Sale'))
    CREATE INDEX IX_Inv_Sale_Customer ON dbo.Inv_Sale (SchoolID, CustomerID, DocDate);
IF COL_LENGTH(N'dbo.Inv_Sale', N'FeePayOrderID') IS NULL
    ALTER TABLE dbo.Inv_Sale ADD FeePayOrderID INT NULL;
