/*
  EduHybrid schema only — empty tables, views, stored procedures, functions, triggers.
  No school/student/invoice row data.

  How to run (normal SSMS, SQLCMD Mode NOT required):
    1. Connect to the server (any database is fine; script switches to master).
    2. File > Open > File...  Hybrid\Database\EduHybrid_Schema_Only.sql
    3. Click Execute (F5).
    4. Do not run against live Edu.

  If EduHybrid already exists this script stops so data is not dropped.
  To recreate an empty/failed copy: right-click EduHybrid > Delete, then run again.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


GO
USE [master];


GO

IF (DB_ID(N'EduHybrid') IS NOT NULL)
BEGIN
    PRINT N'Database EduHybrid already exists. Aborting so existing data is not dropped.';
    RAISERROR(N'Database EduHybrid already exists. Delete it first if you want an empty schema rebuild.', 16, 1);
    SET NOEXEC ON;
END

GO
PRINT N'Creating database EduHybrid...'
GO
CREATE DATABASE [EduHybrid];
GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET AUTO_CLOSE OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
USE [EduHybrid];


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET ANSI_NULLS OFF,
                ANSI_PADDING OFF,
                ANSI_WARNINGS OFF,
                ARITHABORT OFF,
                CONCAT_NULL_YIELDS_NULL OFF,
                NUMERIC_ROUNDABORT OFF,
                QUOTED_IDENTIFIER OFF,
                ANSI_NULL_DEFAULT OFF,
                CURSOR_DEFAULT GLOBAL,
                RECOVERY SIMPLE,
                CURSOR_CLOSE_ON_COMMIT OFF,
                AUTO_CREATE_STATISTICS ON,
                AUTO_SHRINK OFF,
                AUTO_UPDATE_STATISTICS ON,
                RECURSIVE_TRIGGERS OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET ALLOW_SNAPSHOT_ISOLATION OFF;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET READ_COMMITTED_SNAPSHOT OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET AUTO_UPDATE_STATISTICS_ASYNC OFF,
                PAGE_VERIFY CHECKSUM,
                DATE_CORRELATION_OPTIMIZATION OFF,
                DISABLE_BROKER,
                PARAMETERIZATION SIMPLE,
                SUPPLEMENTAL_LOGGING OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'EduHybrid')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [EduHybrid]
    SET TRUSTWORTHY OFF,
        DB_CHAINING OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO
IF IS_SRVROLEMEMBER(N'sysadmin') = 1
    BEGIN
        IF EXISTS (SELECT 1
                   FROM   [master].[dbo].[sysdatabases]
                   WHERE  [name] = N'EduHybrid')
            BEGIN
                EXECUTE sp_executesql N'ALTER DATABASE [EduHybrid]
    SET HONOR_BROKER_PRIORITY OFF 
    WITH ROLLBACK IMMEDIATE';
            END
    END
ELSE
    BEGIN
        PRINT N'The database settings cannot be modified. You must be a SysAdmin to apply these settings.';
    END


GO
ALTER DATABASE [EduHybrid]
    SET TARGET_RECOVERY_TIME = 0 SECONDS 
    WITH ROLLBACK IMMEDIATE;


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET FILESTREAM(NON_TRANSACTED_ACCESS = OFF),
                CONTAINMENT = NONE 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET AUTO_CREATE_STATISTICS ON(INCREMENTAL = OFF),
                MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = OFF,
                DELAYED_DURABILITY = DISABLED 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET QUERY_STORE (QUERY_CAPTURE_MODE = AUTO, DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_PLANS_PER_QUERY = 200, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 367), MAX_STORAGE_SIZE_MB = 100) 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET QUERY_STORE = OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
        ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
        ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
        ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
        ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
        ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
        ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
        ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
    END


GO
IF EXISTS (SELECT 1
           FROM   [master].[dbo].[sysdatabases]
           WHERE  [name] = N'EduHybrid')
    BEGIN
        ALTER DATABASE [EduHybrid]
            SET TEMPORAL_HISTORY_RETENTION OFF 
            WITH ROLLBACK IMMEDIATE;
    END


GO
IF fulltextserviceproperty(N'IsFulltextInstalled') = 1
    EXECUTE sp_fulltext_database 'enable';


GO
PRINT N'Creating Role [aspnet_Membership_BasicAccess]...';


GO
CREATE ROLE [aspnet_Membership_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Membership_FullAccess]...';


GO
CREATE ROLE [aspnet_Membership_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Membership_ReportingAccess]...';


GO
CREATE ROLE [aspnet_Membership_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Personalization_BasicAccess]...';


GO
CREATE ROLE [aspnet_Personalization_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Personalization_FullAccess]...';


GO
CREATE ROLE [aspnet_Personalization_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Personalization_ReportingAccess]...';


GO
CREATE ROLE [aspnet_Personalization_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Profile_BasicAccess]...';


GO
CREATE ROLE [aspnet_Profile_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Profile_FullAccess]...';


GO
CREATE ROLE [aspnet_Profile_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Profile_ReportingAccess]...';


GO
CREATE ROLE [aspnet_Profile_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Roles_BasicAccess]...';


GO
CREATE ROLE [aspnet_Roles_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Roles_FullAccess]...';


GO
CREATE ROLE [aspnet_Roles_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_Roles_ReportingAccess]...';


GO
CREATE ROLE [aspnet_Roles_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Role [aspnet_WebEvent_FullAccess]...';


GO
CREATE ROLE [aspnet_WebEvent_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Membership_BasicAccess]...';


GO
CREATE SCHEMA [aspnet_Membership_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Membership_FullAccess]...';


GO
CREATE SCHEMA [aspnet_Membership_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Membership_ReportingAccess]...';


GO
CREATE SCHEMA [aspnet_Membership_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Personalization_BasicAccess]...';


GO
CREATE SCHEMA [aspnet_Personalization_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Personalization_FullAccess]...';


GO
CREATE SCHEMA [aspnet_Personalization_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Personalization_ReportingAccess]...';


GO
CREATE SCHEMA [aspnet_Personalization_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Profile_BasicAccess]...';


GO
CREATE SCHEMA [aspnet_Profile_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Profile_FullAccess]...';


GO
CREATE SCHEMA [aspnet_Profile_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Profile_ReportingAccess]...';


GO
CREATE SCHEMA [aspnet_Profile_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Roles_BasicAccess]...';


GO
CREATE SCHEMA [aspnet_Roles_BasicAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Roles_FullAccess]...';


GO
CREATE SCHEMA [aspnet_Roles_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_Roles_ReportingAccess]...';


GO
CREATE SCHEMA [aspnet_Roles_ReportingAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Schema [aspnet_WebEvent_FullAccess]...';


GO
CREATE SCHEMA [aspnet_WebEvent_FullAccess]
    AUTHORIZATION [dbo];


GO
PRINT N'Creating Table [dbo].[AAP_Auto_Process_Log]...';


GO
CREATE TABLE [dbo].[AAP_Auto_Process_Log] (
    [LogID]        INT            IDENTITY (1, 1) NOT NULL,
    [ProcessDate]  DATETIME       NULL,
    [ProcessMonth] DATE           NULL,
    [LogMessage]   NVARCHAR (MAX) NULL,
    [ProcessType]  NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Invoice]...';


GO
CREATE TABLE [dbo].[AAP_Invoice] (
    [InvoiceID]         INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID]    INT            NULL,
    [InvoiceCategoryID] INT            NULL,
    [SchoolID]          INT            NULL,
    [Invoice_SN]        INT            NULL,
    [IssuDate]          DATE           NULL,
    [EndDate]           DATE           NULL,
    [Invoice_For]       NVARCHAR (500) NULL,
    [Unit]              INT            NULL,
    [UnitPrice]         FLOAT (53)     NULL,
    [TotalAmount]       FLOAT (53)     NULL,
    [Discount]          FLOAT (53)     NOT NULL,
    [CreateDate]        DATE           NULL,
    [PaidAmount]        FLOAT (53)     NULL,
    [Due]               AS             ([TotalAmount] - ([Discount] + [PaidAmount])) PERSISTED,
    [NumberOfPayment]   INT            NULL,
    [IsPaid]            AS             (CASE WHEN ([TotalAmount] - ([Discount] + [PaidAmount])) = (0) THEN (1) ELSE (0) END) PERSISTED NOT NULL,
    [LastPaidDate]      DATE           NULL,
    [MonthName]         DATE           NULL,
    CONSTRAINT [PK_Invoice] PRIMARY KEY CLUSTERED ([InvoiceID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Invoice_Category]...';


GO
CREATE TABLE [dbo].[AAP_Invoice_Category] (
    [InvoiceCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID]    INT            NULL,
    [InvoiceCategory]   NVARCHAR (256) NULL,
    [Insert_Date]       DATE           NULL,
    CONSTRAINT [PK_AAP_Invoice_Category] PRIMARY KEY CLUSTERED ([InvoiceCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Invoice_OnlinePayment]...';


GO
CREATE TABLE [dbo].[AAP_Invoice_OnlinePayment] (
    [PaymentID]   INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]    INT             NOT NULL,
    [SP_OrderID]  NVARCHAR (100)  NOT NULL,
    [SP_TrxID]    NVARCHAR (200)  NULL,
    [SP_Method]   NVARCHAR (100)  NULL,
    [Amount]      DECIMAL (18, 2) NOT NULL,
    [SP_Code]     NVARCHAR (20)   NULL,
    [SP_Message]  NVARCHAR (500)  NULL,
    [PaymentDate] DATETIME        NULL,
    [CreatedDate] DATETIME        NOT NULL,
    PRIMARY KEY CLUSTERED ([PaymentID] ASC),
    CONSTRAINT [UQ_SP_OrderID] UNIQUE NONCLUSTERED ([SP_OrderID] ASC)
);


GO
PRINT N'Creating Index [dbo].[AAP_Invoice_OnlinePayment].[IX_OnlinePayment_OrderID]...';


GO
CREATE NONCLUSTERED INDEX [IX_OnlinePayment_OrderID]
    ON [dbo].[AAP_Invoice_OnlinePayment]([SP_OrderID] ASC);


GO
PRINT N'Creating Index [dbo].[AAP_Invoice_OnlinePayment].[IX_OnlinePayment_SchoolID]...';


GO
CREATE NONCLUSTERED INDEX [IX_OnlinePayment_SchoolID]
    ON [dbo].[AAP_Invoice_OnlinePayment]([SchoolID] ASC);


GO
PRINT N'Creating Table [dbo].[AAP_Invoice_Payment_Record]...';


GO
CREATE TABLE [dbo].[AAP_Invoice_Payment_Record] (
    [InvoicePaymentRecordID] INT        IDENTITY (1, 1) NOT NULL,
    [InvoiceReceiptID]       INT        NOT NULL,
    [InvoiceID]              INT        NOT NULL,
    [RegistrationID]         INT        NULL,
    [SchoolID]               INT        NULL,
    [Amount]                 FLOAT (53) NULL,
    [PaidDate]               DATE       NULL,
    CONSTRAINT [PK_Invoice_Payment_Record] PRIMARY KEY CLUSTERED ([InvoicePaymentRecordID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Invoice_Receipt]...';


GO
CREATE TABLE [dbo].[AAP_Invoice_Receipt] (
    [InvoiceReceiptID]  INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NULL,
    [RegistrationID]    INT            NULL,
    [TotalAmount]       FLOAT (53)     NULL,
    [PaidDate]          DATETIME       NULL,
    [PaymentBy]         NVARCHAR (128) NULL,
    [Collected_By]      NVARCHAR (128) NULL,
    [Payment_Method]    NVARCHAR (50)  NULL,
    [InvoiceReceipt_SN] INT            NULL,
    [PaidByUser]        NVARCHAR (256) NULL,
    CONSTRAINT [PK_AAP_Invoice_Receipt] PRIMARY KEY CLUSTERED ([InvoiceReceiptID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Reference]...';


GO
CREATE TABLE [dbo].[AAP_Reference] (
    [ReferenceID]         INT            IDENTITY (1, 1) NOT NULL,
    [Reference_SN]        INT            NULL,
    [Reference_Name]      NVARCHAR (128) NULL,
    [Reference_Phone]     NVARCHAR (50)  NULL,
    [Address]             NVARCHAR (500) NULL,
    [Marketing_StartDate] DATE           NULL,
    [Marketing_EndDate]   DATE           NULL,
    [TotalAmount]         FLOAT (53)     NULL,
    [PaidAmount]          FLOAT (53)     NULL,
    [Due]                 AS             ([TotalAmount] - [PaidAmount]),
    [PaymentStatus]       AS             (CASE WHEN ([TotalAmount] - [PaidAmount]) = (0) THEN 'Paid' ELSE 'Due' END),
    [Insert_Date]         DATE           NULL,
    CONSTRAINT [PK_AAP_Reference] PRIMARY KEY CLUSTERED ([ReferenceID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Reference_Commission]...';


GO
CREATE TABLE [dbo].[AAP_Reference_Commission] (
    [CommissionID]          INT             IDENTITY (1, 1) NOT NULL,
    [ReferenceID]           INT             NOT NULL,
    [Reference_School_ID]   INT             NOT NULL,
    [InvoiceID]             INT             NOT NULL,
    [SchoolID]              INT             NOT NULL,
    [Commission_Amount]     DECIMAL (18, 2) NOT NULL,
    [Commission_Percentage] DECIMAL (5, 2)  NOT NULL,
    [ServiceCharge_Amount]  DECIMAL (18, 2) NOT NULL,
    [Commission_Date]       DATE            NOT NULL,
    [Created_At]            DATETIME        NOT NULL,
    PRIMARY KEY CLUSTERED ([CommissionID] ASC)
);


GO
PRINT N'Creating Index [dbo].[AAP_Reference_Commission].[IX_RefComm_RefID]...';


GO
CREATE NONCLUSTERED INDEX [IX_RefComm_RefID]
    ON [dbo].[AAP_Reference_Commission]([ReferenceID] ASC);


GO
PRINT N'Creating Index [dbo].[AAP_Reference_Commission].[IX_RefComm_SchoolID]...';


GO
CREATE NONCLUSTERED INDEX [IX_RefComm_SchoolID]
    ON [dbo].[AAP_Reference_Commission]([SchoolID] ASC);


GO
PRINT N'Creating Table [dbo].[AAP_Reference_PaymentRecord]...';


GO
CREATE TABLE [dbo].[AAP_Reference_PaymentRecord] (
    [ReferencePaymentRecordID] INT            IDENTITY (1, 1) NOT NULL,
    [Reference_PayOrderID]     INT            NOT NULL,
    [ReferenceID]              INT            NULL,
    [SchoolID]                 INT            NULL,
    [InvoiceID]                INT            NOT NULL,
    [Amount]                   FLOAT (53)     NULL,
    [PaidDate]                 DATE           NULL,
    [Paid_By]                  NVARCHAR (128) NULL,
    [Payment_Method]           NVARCHAR (50)  NULL,
    [Reference_School_ID]      INT            NULL,
    [Note]                     NVARCHAR (500) NULL,
    CONSTRAINT [PK_AAP_Reference_PaymentRecord] PRIMARY KEY CLUSTERED ([ReferencePaymentRecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[AAP_Reference_PaymentRecord].[IX_RefPayRec_RefID]...';


GO
CREATE NONCLUSTERED INDEX [IX_RefPayRec_RefID]
    ON [dbo].[AAP_Reference_PaymentRecord]([ReferenceID] ASC);


GO
PRINT N'Creating Table [dbo].[AAP_Reference_PayOrder]...';


GO
CREATE TABLE [dbo].[AAP_Reference_PayOrder] (
    [Reference_PayOrderID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT        NULL,
    [Reference_School_ID]  INT        NULL,
    [ReferenceID]          INT        NULL,
    [InvoiceID]            INT        NULL,
    [Amount]               FLOAT (53) NULL,
    [PayOrderDate]         DATE       NULL,
    CONSTRAINT [PK_AAP_Reference_PayOrder] PRIMARY KEY CLUSTERED ([Reference_PayOrderID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Reference_School]...';


GO
CREATE TABLE [dbo].[AAP_Reference_School] (
    [Reference_School_ID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT        NULL,
    [ReferenceID]         INT        NULL,
    [Percentage]          FLOAT (53) NULL,
    [School_SignUp_Date]  DATE       NULL,
    [End_Reference_Date]  DATE       NULL,
    [InserDate]           DATE       NULL,
    CONSTRAINT [PK_AAP_Reference_School] PRIMARY KEY CLUSTERED ([Reference_School_ID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Reference_Target]...';


GO
CREATE TABLE [dbo].[AAP_Reference_Target] (
    [Reference_TargetID] INT           IDENTITY (1, 1) NOT NULL,
    [ReferenceID]        INT           NULL,
    [Target]             INT           NULL,
    [FulFill_Target]     INT           NULL,
    [TargetStatus]       NVARCHAR (50) NULL,
    [Target_SN]          INT           NULL,
    [StartDate]          DATE          NULL,
    [EndDate]            DATE          NULL,
    [InsertDate]         DATE          NULL,
    CONSTRAINT [PK_AAP_Reference_Target] PRIMARY KEY CLUSTERED ([Reference_TargetID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_Student_Count_Monthly]...';


GO
CREATE TABLE [dbo].[AAP_Student_Count_Monthly] (
    [PayableStudentRecordID] INT      IDENTITY (1, 1) NOT NULL,
    [SchoolID]               INT      NULL,
    [StudentCount]           AS       ([Active_Student] + [Reject_Countable]) PERSISTED,
    [Active_Student]         INT      NULL,
    [Reject_Countable]       INT      NULL,
    [Reject_Uncountable]     INT      NULL,
    [Month]                  DATE     NULL,
    [InsertDate]             DATETIME NULL,
    CONSTRAINT [PK_AAP_Student_Count_Monthly] PRIMARY KEY CLUSTERED ([PayableStudentRecordID] ASC)
);


GO
PRINT N'Creating Table [dbo].[AAP_StudentClass_Count_Monthly]...';


GO
CREATE TABLE [dbo].[AAP_StudentClass_Count_Monthly] (
    [PayableStudentRecordID] INT      IDENTITY (1, 1) NOT NULL,
    [SchoolID]               INT      NULL,
    [EducationYearID]        INT      NULL,
    [ClassID]                INT      NULL,
    [StudentCount]           AS       ([Active_Student] + [Reject_Countable]) PERSISTED,
    [Active_Student]         INT      NULL,
    [Reject_Countable]       INT      NULL,
    [Reject_Uncountable]     INT      NULL,
    [Month]                  DATE     NULL,
    [InsertDate]             DATETIME NULL,
    CONSTRAINT [PK_AAP_StudentClass_Count_Monthly] PRIMARY KEY CLUSTERED ([PayableStudentRecordID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Account]...';


GO
CREATE TABLE [dbo].[Account] (
    [AccountID]                      INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                       INT            NOT NULL,
    [RegistrationID]                 INT            NOT NULL,
    [AccountName]                    NVARCHAR (128) NOT NULL,
    [AccountBalance]                 AS             ((([Total_IN] + [Total_Income]) + [Deleted_Expense]) - (([Total_OUT] + [Total_Expense]) + [Deleted_Income])) PERSISTED NOT NULL,
    [Total_IN]                       FLOAT (53)     NOT NULL,
    [Total_OUT]                      FLOAT (53)     NOT NULL,
    [Total_Income]                   FLOAT (53)     NOT NULL,
    [Total_Expense]                  FLOAT (53)     NOT NULL,
    [Deleted_Income]                 FLOAT (53)     NOT NULL,
    [Deleted_Expense]                FLOAT (53)     NOT NULL,
    [Default_Status]                 BIT            NOT NULL,
    [AccountCreateDate]              DATE           NOT NULL,
    [PAY_Buttton_SMS_Enable_Disable] BIT            NULL,
    [Teacher_BackDate_Attendance]    BIT            NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED ([AccountID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Account_Log]...';


GO
CREATE TABLE [dbo].[Account_Log] (
    [AccountLogID]         INT            IDENTITY (1, 1) NOT NULL,
    [AccountID]            INT            NULL,
    [SchoolID]             INT            NOT NULL,
    [RegistrationID]       INT            NULL,
    [EducationYearID]      INT            NOT NULL,
    [Amount]               FLOAT (53)     NOT NULL,
    [Add_Subtraction]      NVARCHAR (50)  NOT NULL,
    [Pay_For]              NVARCHAR (MAX) NULL,
    [ClassOrOtherCategory] NVARCHAR (128) NOT NULL,
    [MainCategory]         NVARCHAR (128) NULL,
    [SubCategory]          NVARCHAR (128) NOT NULL,
    [Details]              NVARCHAR (MAX) NULL,
    [Insert_Date]          DATE           NOT NULL,
    [Insert_Time]          TIME (7)       NOT NULL,
    [Log_SN]               INT            NOT NULL,
    [Balance_Before]       FLOAT (53)     NOT NULL,
    [Balance_After]        FLOAT (53)     NOT NULL,
    [Activity_Date]        DATE           NOT NULL,
    [In_Ex_type]           VARCHAR (2)    NULL,
    [Insert_Up_De]         VARCHAR (2)    NULL,
    CONSTRAINT [PK_Account_Log] PRIMARY KEY CLUSTERED ([AccountLogID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Account_Log].[IX_AccountLog]...';


GO
CREATE NONCLUSTERED INDEX [IX_AccountLog]
    ON [dbo].[Account_Log]([AccountID] ASC, [Amount] ASC, [SchoolID] ASC)
    INCLUDE([Add_Subtraction], [Pay_For], [ClassOrOtherCategory], [Insert_Date], [Log_SN], [Balance_Before], [Balance_After]);


GO
PRINT N'Creating Table [dbo].[AccountIN_Record]...';


GO
CREATE TABLE [dbo].[AccountIN_Record] (
    [AccountIN_ID]     INT            IDENTITY (1, 1) NOT NULL,
    [AccountID]        INT            NOT NULL,
    [SchoolID]         INT            NOT NULL,
    [RegistrationID]   INT            NOT NULL,
    [EducationYearID]  INT            NOT NULL,
    [AccountIN_Amount] FLOAT (53)     NOT NULL,
    [IN_Details]       NVARCHAR (500) NULL,
    [AccountIN_Date]   DATE           NOT NULL,
    [Insert_Date]      DATETIME       NOT NULL,
    CONSTRAINT [PK_AccountIN_Record] PRIMARY KEY CLUSTERED ([AccountIN_ID] ASC)
);


GO
PRINT N'Creating Index [dbo].[AccountIN_Record].[IX_AccountINRecord]...';


GO
CREATE NONCLUSTERED INDEX [IX_AccountINRecord]
    ON [dbo].[AccountIN_Record]([SchoolID] ASC, [EducationYearID] ASC, [Insert_Date] ASC)
    INCLUDE([AccountIN_Amount], [AccountIN_Date], [AccountID]);


GO
PRINT N'Creating Table [dbo].[AccountOUT_Record]...';


GO
CREATE TABLE [dbo].[AccountOUT_Record] (
    [AccountOUT_ID]     INT            IDENTITY (1, 1) NOT NULL,
    [AccountID]         INT            NOT NULL,
    [SchoolID]          INT            NOT NULL,
    [RegistrationID]    INT            NOT NULL,
    [EducationYearID]   INT            NOT NULL,
    [AccountOUT_Amount] FLOAT (53)     NOT NULL,
    [Out_Details]       NVARCHAR (500) NULL,
    [AccountOUT_Date]   DATE           NOT NULL,
    [Insert_Date]       DATE           NOT NULL,
    CONSTRAINT [PK_AccountOUT] PRIMARY KEY CLUSTERED ([AccountOUT_ID] ASC)
);


GO
PRINT N'Creating Index [dbo].[AccountOUT_Record].[IX_AccountOUTRecord]...';


GO
CREATE NONCLUSTERED INDEX [IX_AccountOUTRecord]
    ON [dbo].[AccountOUT_Record]([SchoolID] ASC, [EducationYearID] ASC, [Insert_Date] ASC)
    INCLUDE([AccountOUT_Amount], [Out_Details], [AccountID]);


GO
PRINT N'Creating Table [dbo].[Admin]...';


GO
CREATE TABLE [dbo].[Admin] (
    [AdminID]                INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID]         INT             NOT NULL,
    [SchoolID]               INT             NOT NULL,
    [FirstName]              NVARCHAR (128)  NULL,
    [LastName]               NVARCHAR (128)  NULL,
    [FatherName]             NVARCHAR (128)  NULL,
    [Gender]                 NVARCHAR (50)   NULL,
    [Age]                    NVARCHAR (50)   NULL,
    [Designation]            NVARCHAR (128)  NULL,
    [DateofBirth]            NVARCHAR (50)   NULL,
    [Nationality]            NVARCHAR (50)   NULL,
    [NationalIDorPassportNO] NVARCHAR (128)  NULL,
    [Address]                NVARCHAR (500)  NULL,
    [City]                   NVARCHAR (50)   NULL,
    [PostalCode]             NVARCHAR (50)   NULL,
    [State]                  NVARCHAR (50)   NULL,
    [Phone]                  NVARCHAR (50)   NULL,
    [Email]                  NVARCHAR (50)   NULL,
    [Date]                   DATETIME        NULL,
    [Image]                  VARBINARY (MAX) NULL,
    CONSTRAINT [PK_Admin] PRIMARY KEY CLUSTERED ([AdminID] ASC)
);


GO
PRINT N'Creating Table [dbo].[aspnet_Applications]...';


GO
CREATE TABLE [dbo].[aspnet_Applications] (
    [ApplicationName]        NVARCHAR (256)   NOT NULL,
    [LoweredApplicationName] NVARCHAR (256)   NOT NULL,
    [ApplicationId]          UNIQUEIDENTIFIER NOT NULL,
    [Description]            NVARCHAR (256)   NULL,
    PRIMARY KEY NONCLUSTERED ([ApplicationId] ASC),
    UNIQUE NONCLUSTERED ([ApplicationName] ASC),
    UNIQUE NONCLUSTERED ([LoweredApplicationName] ASC)
);


GO
PRINT N'Creating Index [dbo].[aspnet_Applications].[aspnet_Applications_Index]...';


GO
CREATE CLUSTERED INDEX [aspnet_Applications_Index]
    ON [dbo].[aspnet_Applications]([LoweredApplicationName] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_Membership]...';


GO
CREATE TABLE [dbo].[aspnet_Membership] (
    [ApplicationId]                          UNIQUEIDENTIFIER NOT NULL,
    [UserId]                                 UNIQUEIDENTIFIER NOT NULL,
    [Password]                               NVARCHAR (128)   NOT NULL,
    [PasswordFormat]                         INT              NOT NULL,
    [PasswordSalt]                           NVARCHAR (128)   NOT NULL,
    [MobilePIN]                              NVARCHAR (16)    NULL,
    [Email]                                  NVARCHAR (256)   NULL,
    [LoweredEmail]                           NVARCHAR (256)   NULL,
    [PasswordQuestion]                       NVARCHAR (256)   NULL,
    [PasswordAnswer]                         NVARCHAR (128)   NULL,
    [IsApproved]                             BIT              NOT NULL,
    [IsLockedOut]                            BIT              NOT NULL,
    [CreateDate]                             DATETIME         NOT NULL,
    [LastLoginDate]                          DATETIME         NOT NULL,
    [LastPasswordChangedDate]                DATETIME         NOT NULL,
    [LastLockoutDate]                        DATETIME         NOT NULL,
    [FailedPasswordAttemptCount]             INT              NOT NULL,
    [FailedPasswordAttemptWindowStart]       DATETIME         NOT NULL,
    [FailedPasswordAnswerAttemptCount]       INT              NOT NULL,
    [FailedPasswordAnswerAttemptWindowStart] DATETIME         NOT NULL,
    [Comment]                                NTEXT            NULL,
    PRIMARY KEY NONCLUSTERED ([UserId] ASC)
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_Membership]', @OptionName = N'text in row', @OptionValue = N'3000';


GO
PRINT N'Creating Index [dbo].[aspnet_Membership].[aspnet_Membership_index]...';


GO
CREATE CLUSTERED INDEX [aspnet_Membership_index]
    ON [dbo].[aspnet_Membership]([ApplicationId] ASC, [LoweredEmail] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_Paths]...';


GO
CREATE TABLE [dbo].[aspnet_Paths] (
    [ApplicationId] UNIQUEIDENTIFIER NOT NULL,
    [PathId]        UNIQUEIDENTIFIER NOT NULL,
    [Path]          NVARCHAR (256)   NOT NULL,
    [LoweredPath]   NVARCHAR (256)   NOT NULL,
    PRIMARY KEY NONCLUSTERED ([PathId] ASC)
);


GO
PRINT N'Creating Index [dbo].[aspnet_Paths].[aspnet_Paths_index]...';


GO
CREATE UNIQUE CLUSTERED INDEX [aspnet_Paths_index]
    ON [dbo].[aspnet_Paths]([ApplicationId] ASC, [LoweredPath] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_PersonalizationAllUsers]...';


GO
CREATE TABLE [dbo].[aspnet_PersonalizationAllUsers] (
    [PathId]          UNIQUEIDENTIFIER NOT NULL,
    [PageSettings]    IMAGE            NOT NULL,
    [LastUpdatedDate] DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([PathId] ASC)
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_PersonalizationAllUsers]', @OptionName = N'text in row', @OptionValue = N'6000';


GO
PRINT N'Creating Table [dbo].[aspnet_PersonalizationPerUser]...';


GO
CREATE TABLE [dbo].[aspnet_PersonalizationPerUser] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [PathId]          UNIQUEIDENTIFIER NULL,
    [UserId]          UNIQUEIDENTIFIER NULL,
    [PageSettings]    IMAGE            NOT NULL,
    [LastUpdatedDate] DATETIME         NOT NULL,
    PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_PersonalizationPerUser]', @OptionName = N'text in row', @OptionValue = N'6000';


GO
PRINT N'Creating Index [dbo].[aspnet_PersonalizationPerUser].[aspnet_PersonalizationPerUser_index1]...';


GO
CREATE UNIQUE CLUSTERED INDEX [aspnet_PersonalizationPerUser_index1]
    ON [dbo].[aspnet_PersonalizationPerUser]([PathId] ASC, [UserId] ASC);


GO
PRINT N'Creating Index [dbo].[aspnet_PersonalizationPerUser].[aspnet_PersonalizationPerUser_ncindex2]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [aspnet_PersonalizationPerUser_ncindex2]
    ON [dbo].[aspnet_PersonalizationPerUser]([UserId] ASC, [PathId] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_Profile]...';


GO
CREATE TABLE [dbo].[aspnet_Profile] (
    [UserId]               UNIQUEIDENTIFIER NOT NULL,
    [PropertyNames]        NTEXT            NOT NULL,
    [PropertyValuesString] NTEXT            NOT NULL,
    [PropertyValuesBinary] IMAGE            NOT NULL,
    [LastUpdatedDate]      DATETIME         NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);


GO
EXECUTE sp_tableoption @TableNamePattern = N'[dbo].[aspnet_Profile]', @OptionName = N'text in row', @OptionValue = N'6000';


GO
PRINT N'Creating Table [dbo].[aspnet_Roles]...';


GO
CREATE TABLE [dbo].[aspnet_Roles] (
    [ApplicationId]   UNIQUEIDENTIFIER NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER NOT NULL,
    [RoleName]        NVARCHAR (256)   NOT NULL,
    [LoweredRoleName] NVARCHAR (256)   NOT NULL,
    [Description]     NVARCHAR (256)   NULL,
    PRIMARY KEY NONCLUSTERED ([RoleId] ASC)
);


GO
PRINT N'Creating Index [dbo].[aspnet_Roles].[aspnet_Roles_index1]...';


GO
CREATE UNIQUE CLUSTERED INDEX [aspnet_Roles_index1]
    ON [dbo].[aspnet_Roles]([ApplicationId] ASC, [LoweredRoleName] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_SchemaVersions]...';


GO
CREATE TABLE [dbo].[aspnet_SchemaVersions] (
    [Feature]                 NVARCHAR (128) NOT NULL,
    [CompatibleSchemaVersion] NVARCHAR (128) NOT NULL,
    [IsCurrentVersion]        BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Feature] ASC, [CompatibleSchemaVersion] ASC)
);


GO
PRINT N'Creating Table [dbo].[aspnet_Users]...';


GO
CREATE TABLE [dbo].[aspnet_Users] (
    [ApplicationId]    UNIQUEIDENTIFIER NOT NULL,
    [UserId]           UNIQUEIDENTIFIER NOT NULL,
    [UserName]         NVARCHAR (256)   NOT NULL,
    [LoweredUserName]  NVARCHAR (256)   NOT NULL,
    [MobileAlias]      NVARCHAR (16)    NULL,
    [IsAnonymous]      BIT              NOT NULL,
    [LastActivityDate] DATETIME         NOT NULL,
    PRIMARY KEY NONCLUSTERED ([UserId] ASC)
);


GO
PRINT N'Creating Index [dbo].[aspnet_Users].[aspnet_Users_Index]...';


GO
CREATE UNIQUE CLUSTERED INDEX [aspnet_Users_Index]
    ON [dbo].[aspnet_Users]([ApplicationId] ASC, [LoweredUserName] ASC);


GO
PRINT N'Creating Index [dbo].[aspnet_Users].[aspnet_Users_Index2]...';


GO
CREATE NONCLUSTERED INDEX [aspnet_Users_Index2]
    ON [dbo].[aspnet_Users]([ApplicationId] ASC, [LastActivityDate] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_UsersInRoles]...';


GO
CREATE TABLE [dbo].[aspnet_UsersInRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
);


GO
PRINT N'Creating Index [dbo].[aspnet_UsersInRoles].[aspnet_UsersInRoles_index]...';


GO
CREATE NONCLUSTERED INDEX [aspnet_UsersInRoles_index]
    ON [dbo].[aspnet_UsersInRoles]([RoleId] ASC);


GO
PRINT N'Creating Table [dbo].[aspnet_WebEvent_Events]...';


GO
CREATE TABLE [dbo].[aspnet_WebEvent_Events] (
    [EventId]                CHAR (32)       NOT NULL,
    [EventTimeUtc]           DATETIME        NOT NULL,
    [EventTime]              DATETIME        NOT NULL,
    [EventType]              NVARCHAR (256)  NOT NULL,
    [EventSequence]          DECIMAL (19)    NOT NULL,
    [EventOccurrence]        DECIMAL (19)    NOT NULL,
    [EventCode]              INT             NOT NULL,
    [EventDetailCode]        INT             NOT NULL,
    [Message]                NVARCHAR (1024) NULL,
    [ApplicationPath]        NVARCHAR (256)  NULL,
    [ApplicationVirtualPath] NVARCHAR (256)  NULL,
    [MachineName]            NVARCHAR (256)  NOT NULL,
    [RequestUrl]             NVARCHAR (1024) NULL,
    [ExceptionType]          NVARCHAR (256)  NULL,
    [Details]                NTEXT           NULL,
    PRIMARY KEY CLUSTERED ([EventId] ASC)
);


GO
PRINT N'Creating Table [dbo].[AST]...';


GO
CREATE TABLE [dbo].[AST] (
    [ASTID]          INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT            NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [UserName]       NVARCHAR (128) NOT NULL,
    [Category]       NVARCHAR (128) NOT NULL,
    [Password]       NVARCHAR (128) NULL,
    [PasswordAnswer] NVARCHAR (128) NULL,
    [SmsNumber]      VARCHAR (20)   NULL,
    CONSTRAINT [PK_AST] PRIMARY KEY CLUSTERED ([ASTID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Device_DataUpdateList]...';


GO
CREATE TABLE [dbo].[Attendance_Device_DataUpdateList] (
    [DateUpdateID]      INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NOT NULL,
    [RegistrationID]    INT            NULL,
    [UpdateType]        NVARCHAR (50)  NULL,
    [UpdateDescription] NVARCHAR (500) NULL,
    [UpdateDate]        DATETIME       NULL,
    CONSTRAINT [PK_Attendance_Device_DataUpdateList] PRIMARY KEY CLUSTERED ([DateUpdateID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Device_Setting]...';


GO
CREATE TABLE [dbo].[Attendance_Device_Setting] (
    [AttendanceSettingID]           INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                      INT            NULL,
    [UserName]                      NVARCHAR (100) NULL,
    [Password]                      NVARCHAR (50)  NULL,
    [IsActive]                      BIT            NULL,
    [InsertDate]                    DATETIME       NULL,
    [SettingKey]                    NVARCHAR (50)  NULL,
    [Image_Link]                    NVARCHAR (200) NULL,
    [Is_Device_Attendance_Enable]   BIT            NULL,
    [Is_All_SMS_On]                 BIT            NULL,
    [Is_Holiday_As_Offday]          BIT            NULL,
    [Is_Student_All_SMS_Active]     BIT            NULL,
    [Is_Student_Entry_SMS_ON]       BIT            NULL,
    [Is_Student_Exit_SMS_ON]        BIT            NULL,
    [Is_Student_Abs_SMS_ON]         BIT            NULL,
    [Is_Student_Late_SMS_ON]        BIT            NULL,
    [Is_Employee_SMS_Active]        BIT            NULL,
    [Is_Employee_Abs_SMS_ON]        BIT            NULL,
    [Is_Employee_Late_SMS_ON]       BIT            NULL,
    [Is_Employee_SMS_OwnNumber]     BIT            NULL,
    [Employee_SMS_Number]           NVARCHAR (50)  NULL,
    [SMS_TimeOut_Minute]            INT            NULL,
    [Is_Student_Attendance_Enable]  BIT            NULL,
    [Is_Employee_Attendance_Enable] BIT            NULL,
    [Is_English_SMS]                BIT            NULL,
    CONSTRAINT [PK_Attendance_Device_Setting] PRIMARY KEY CLUSTERED ([AttendanceSettingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Fine]...';


GO
CREATE TABLE [dbo].[Attendance_Fine] (
    [AttendanceFineID] INT           IDENTITY (1, 1) NOT NULL,
    [FineAmount]       FLOAT (53)    NULL,
    [FineFor]          NVARCHAR (50) NULL,
    [SchoolID]         INT           NULL,
    [RegistrationID]   INT           NOT NULL,
    [EducationYearID]  INT           NULL,
    CONSTRAINT [PK_Attendance_Fine] PRIMARY KEY CLUSTERED ([AttendanceFineID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Leave]...';


GO
CREATE TABLE [dbo].[Attendance_Leave] (
    [StudentLeaveID]  INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NULL,
    [RegistrationID]  INT            NOT NULL,
    [StudentID]       INT            NULL,
    [EducationYearID] INT            NULL,
    [StartDate]       DATE           NULL,
    [EndDate]         DATE           NULL,
    [Description]     NVARCHAR (500) NULL,
    [LeaveType]       NVARCHAR (100) NULL,
    [GuardianName]    NVARCHAR (200) NULL,
    CONSTRAINT [PK_Student_Leave] PRIMARY KEY CLUSTERED ([StudentLeaveID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Leave_Type]...';


GO
CREATE TABLE [dbo].[Attendance_Leave_Type] (
    [LeaveTypeID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]      INT            NOT NULL,
    [LeaveTypeName] NVARCHAR (100) NOT NULL,
    [SortOrder]     INT            NOT NULL,
    [IsActive]      BIT            NOT NULL,
    [CreatedDate]   DATETIME       NOT NULL,
    CONSTRAINT [PK_Attendance_Leave_Type] PRIMARY KEY CLUSTERED ([LeaveTypeID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Attendance_Leave_Type].[IX_Attendance_Leave_Type_SchoolID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_Leave_Type_SchoolID]
    ON [dbo].[Attendance_Leave_Type]([SchoolID] ASC, [IsActive] ASC, [SortOrder] ASC);


GO
PRINT N'Creating Table [dbo].[Attendance_Monthly_Report]...';


GO
CREATE TABLE [dbo].[Attendance_Monthly_Report] (
    [Monthly_ReportID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT           NOT NULL,
    [RegistrationID]   INT           NOT NULL,
    [EducationYearID]  INT           NULL,
    [StudentID]        INT           NOT NULL,
    [ClassID]          INT           NOT NULL,
    [StudentClassID]   INT           NOT NULL,
    [MonthName]        NVARCHAR (50) NOT NULL,
    [MonthStartDate]   DATE          NOT NULL,
    [MonthEndDate]     DATE          NOT NULL,
    [FineAmount]       FLOAT (53)    NOT NULL,
    [WorkingDays]      INT           NOT NULL,
    [TotalPresent]     INT           NOT NULL,
    [TotalAbsent]      INT           NULL,
    [TotalLateAbs]     INT           NULL,
    [Abs_Count]        AS            ([TotalLateAbs] + [TotalAbsent]) PERSISTED,
    [TotalLate]        INT           NULL,
    [TotalLeave]       INT           NULL,
    [TotalBunk]        INT           NULL,
    [PayOrderID]       INT           NULL,
    [Insert_Date]      DATETIME      NOT NULL,
    CONSTRAINT [PK_Attendance_FinePay_Record] PRIMARY KEY CLUSTERED ([Monthly_ReportID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Record]...';


GO
CREATE TABLE [dbo].[Attendance_Record] (
    [AttendanceRecordID]    INT            IDENTITY (1, 1) NOT NULL,
    [StudentID]             INT            NOT NULL,
    [RegistrationID]        INT            NOT NULL,
    [SchoolID]              INT            NOT NULL,
    [ClassID]               INT            NULL,
    [StudentClassID]        INT            NULL,
    [EducationYearID]       INT            NULL,
    [Attendance]            NVARCHAR (128) NULL,
    [AttendanceDate]        DATE           NULL,
    [Reason]                NVARCHAR (500) NULL,
    [EntryTime]             TIME (7)       NULL,
    [ExitTime]              TIME (7)       NULL,
    [InsertDate]            DATETIME       NULL,
    [ExitStatus]            NVARCHAR (50)  NULL,
    [Is_OUT]                BIT            NOT NULL,
    [IsFromDevice]          BIT            NOT NULL,
    [Attendance_ScheduleID] INT            NULL,
    [AttendanceDateKey]     AS             (CONVERT (DATE, [AttendanceDate])) PERSISTED,
    CONSTRAINT [PK_StudentAttendance] PRIMARY KEY CLUSTERED ([AttendanceRecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Attendance_Record].[<Name of Missing Index, sysname,>]...';


GO
CREATE NONCLUSTERED INDEX [<Name of Missing Index, sysname,>]
    ON [dbo].[Attendance_Record]([SchoolID] ASC, [ClassID] ASC, [StudentClassID] ASC, [EducationYearID] ASC, [Attendance] ASC, [AttendanceDate] ASC);


GO
PRINT N'Creating Index [dbo].[Attendance_Record].[IX_Attendance_Record_Result_P]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_Record_Result_P]
    ON [dbo].[Attendance_Record]([SchoolID] ASC, [ClassID] ASC, [EducationYearID] ASC, [AttendanceDate] ASC)
    INCLUDE([StudentClassID]);


GO
PRINT N'Creating Index [dbo].[Attendance_Record].[IX_Attendance_Record_Find2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_Record_Find2]
    ON [dbo].[Attendance_Record]([SchoolID] ASC, [EducationYearID] ASC, [Attendance] ASC, [AttendanceDate] ASC)
    INCLUDE([ClassID], [StudentClassID]);


GO
PRINT N'Creating Index [dbo].[Attendance_Record].[UQ_Attendance_Record_Student_Date_Schedule]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Attendance_Record_Student_Date_Schedule]
    ON [dbo].[Attendance_Record]([SchoolID] ASC, [StudentID] ASC, [AttendanceDate] ASC, [Attendance_ScheduleID] ASC) WHERE ([Attendance_ScheduleID] IS NOT NULL);


GO
PRINT N'Creating Index [dbo].[Attendance_Record].[IX_Attendance_Record_Find]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_Record_Find]
    ON [dbo].[Attendance_Record]([SchoolID] ASC, [EducationYearID] ASC, [AttendanceDate] ASC)
    INCLUDE([ClassID], [StudentClassID]);


GO
PRINT N'Creating Table [dbo].[Attendance_Record_Device]...';


GO
CREATE TABLE [dbo].[Attendance_Record_Device] (
    [AttendanceRecordDevice_ID] INT           IDENTITY (1, 1) NOT NULL,
    [ID]                        NVARCHAR (50) NULL,
    [EntryDateTime]             DATETIME      NULL,
    [ServerUp_Status]           NVARCHAR (50) NULL,
    [EntryDate]                 DATE          NULL,
    [Stu_Emp_ID]                INT           NULL,
    [DeviceID]                  INT           NULL,
    [Category]                  NVARCHAR (50) NULL,
    [SchoolID]                  INT           NULL,
    [Insert_Date]               DATETIME      NULL,
    CONSTRAINT [PK_Attendance_Record] PRIMARY KEY CLUSTERED ([AttendanceRecordDevice_ID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Schedule]...';


GO
CREATE TABLE [dbo].[Attendance_Schedule] (
    [ScheduleID]     INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [ScheduleName]   NVARCHAR (128) NULL,
    [LateEntryTime]  TIME (7)       NULL,
    [Date]           DATE           NULL,
    [StartTime]      TIME (7)       NULL,
    [EndTime]        TIME (7)       NULL,
    CONSTRAINT [PK_Attendance_Schedule] PRIMARY KEY CLUSTERED ([ScheduleID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Schedule_AssignStudent]...';


GO
CREATE TABLE [dbo].[Attendance_Schedule_AssignStudent] (
    [Schedule_AssignStuID] INT  IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT  NULL,
    [RegistrationID]       INT  NULL,
    [ScheduleID]           INT  NULL,
    [StudentID]            INT  NULL,
    [Entry_Confirmation]   BIT  NULL,
    [Exit_Confirmation]    BIT  NULL,
    [Date]                 DATE NULL,
    [Is_Abs_SMS]           BIT  NULL,
    [Is_Late_SMS]          BIT  NULL,
    CONSTRAINT [PK_Attendance_Schedule_AssignStudent] PRIMARY KEY CLUSTERED ([Schedule_AssignStuID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Schedule_ChangeRecord]...';


GO
CREATE TABLE [dbo].[Attendance_Schedule_ChangeRecord] (
    [ScheduleChangeID]   INT      IDENTITY (1, 1) NOT NULL,
    [ScheduleID]         INT      NOT NULL,
    [SchoolID]           INT      NULL,
    [RegistrationID]     INT      NULL,
    [Prev_StartTime]     TIME (7) NULL,
    [Prev_LateEntryTime] TIME (7) NULL,
    [Prev_EndTime]       TIME (7) NULL,
    [New_StartTime]      TIME (7) NULL,
    [New_LateEntryTime]  TIME (7) NULL,
    [New_EndTime]        TIME (7) NULL,
    [InsertDate]         DATETIME NULL,
    CONSTRAINT [PK_Attendance_Schedule_ChangeRecord] PRIMARY KEY CLUSTERED ([ScheduleChangeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Schedule_Day]...';


GO
CREATE TABLE [dbo].[Attendance_Schedule_Day] (
    [ScheduleDayID]  INT           IDENTITY (1, 1) NOT NULL,
    [ScheduleID]     INT           NULL,
    [SchoolID]       INT           NULL,
    [RegistrationID] INT           NULL,
    [Day]            NVARCHAR (50) NULL,
    [LateEntryTime]  TIME (7)      NULL,
    [StartTime]      TIME (7)      NULL,
    [EndTime]        TIME (7)      NULL,
    [Insert_Date]    DATE          NOT NULL,
    [Is_OnDay]       BIT           NULL,
    CONSTRAINT [PK_Attendance_Schedule_Day] PRIMARY KEY CLUSTERED ([ScheduleDayID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_SMS]...';


GO
CREATE TABLE [dbo].[Attendance_SMS] (
    [Attendance_SMSID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT            NOT NULL,
    [ScheduleTime]     TIME (7)       NULL,
    [CreateTime]       TIME (7)       NULL,
    [SentTime]         TIME (7)       NULL,
    [AttendanceDate]   DATE           NULL,
    [SMS_Text]         NVARCHAR (500) NULL,
    [MobileNo]         NVARCHAR (50)  NULL,
    [Is_Send]          BIT            NULL,
    [InsertDate]       DATETIME       NULL,
    [AttendanceStatus] NVARCHAR (50)  NULL,
    [SMS_TimeOut]      INT            NULL,
    [EmployeeID]       INT            NULL,
    [StudentID]        INT            NULL,
    CONSTRAINT [PK_Attendance_SMS] PRIMARY KEY CLUSTERED ([Attendance_SMSID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_SMS_Failed]...';


GO
CREATE TABLE [dbo].[Attendance_SMS_Failed] (
    [AttendanceSmsFailedId] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]              INT            NOT NULL,
    [ScheduleTime]          TIME (7)       NULL,
    [CreateTime]            TIME (7)       NULL,
    [SentTime]              TIME (7)       NULL,
    [AttendanceDate]        DATE           NULL,
    [SMS_Text]              NVARCHAR (500) NULL,
    [MobileNo]              NVARCHAR (50)  NULL,
    [AttendanceStatus]      NVARCHAR (50)  NULL,
    [SMS_TimeOut]           INT            NULL,
    [EmployeeID]            INT            NULL,
    [StudentID]             INT            NULL,
    [FailedReson]           NVARCHAR (128) NULL,
    [InsertDate]            DATETIME       NULL,
    CONSTRAINT [PK_Attendance_SMS_Failed] PRIMARY KEY CLUSTERED ([AttendanceSmsFailedId] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_SMS_Sender]...';


GO
CREATE TABLE [dbo].[Attendance_SMS_Sender] (
    [AttendanceSmsSenderId] INT      IDENTITY (1, 1) NOT NULL,
    [AppStartTime]          DATETIME NOT NULL,
    [AppCloseTime]          DATETIME NULL,
    [TotalEventCall]        INT      NOT NULL,
    [TotalSmsSend]          INT      NOT NULL,
    [TotalSmsFailed]        INT      NOT NULL,
    CONSTRAINT [PK_Attendance_SMS_Sender] PRIMARY KEY CLUSTERED ([AttendanceSmsSenderId] ASC)
);


GO
PRINT N'Creating Table [dbo].[Attendance_Student]...';


GO
CREATE TABLE [dbo].[Attendance_Student] (
    [AttendanceStudentID] INT      IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT      NULL,
    [RegistrationID]      INT      NOT NULL,
    [EducationYearID]     INT      NULL,
    [StudentID]           INT      NULL,
    [ExamID]              INT      NULL,
    [ClassID]             INT      NULL,
    [StudentClassID]      INT      NULL,
    [WorkingDays]         INT      NULL,
    [TotalPresent]        INT      NULL,
    [TotalAbsent]         INT      NULL,
    [TotalLate]           INT      NULL,
    [TotalLeave]          INT      NULL,
    [TotalBunk]           INT      NULL,
    [CumulativeNameID]    INT      NULL,
    [Insert_Date]         DATETIME NULL,
    [TotalLateAbs]        INT      NULL,
    CONSTRAINT [PK_Attendance_Student] PRIMARY KEY CLUSTERED ([AttendanceStudentID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Attendance_Student].[IX_Attendance_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_Performance]
    ON [dbo].[Attendance_Student]([StudentID] ASC, [ExamID] ASC, [ClassID] ASC, [StudentClassID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([WorkingDays], [TotalPresent], [TotalAbsent], [TotalLeave], [TotalLate], [TotalLateAbs]);


GO
PRINT N'Creating Index [dbo].[Attendance_Student].[IX_Attendance_BatchQuery]...';


GO
CREATE NONCLUSTERED INDEX [IX_Attendance_BatchQuery]
    ON [dbo].[Attendance_Student]([ExamID] ASC, [SchoolID] ASC, [EducationYearID] ASC, [StudentID] ASC, [StudentClassID] ASC)
    INCLUDE([WorkingDays], [TotalPresent], [TotalAbsent], [TotalLeave], [TotalLate], [TotalLateAbs]);


GO
PRINT N'Creating Table [dbo].[Authority_Info]...';


GO
CREATE TABLE [dbo].[Authority_Info] (
    [AuthorityID]            INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID]         INT             NOT NULL,
    [Name]                   NVARCHAR (128)  NULL,
    [FatherName]             NVARCHAR (128)  NULL,
    [Gender]                 NVARCHAR (50)   NULL,
    [Age]                    NVARCHAR (50)   NULL,
    [Designation]            NVARCHAR (128)  NULL,
    [DateofBirth]            DATE            NULL,
    [Nationality]            NVARCHAR (50)   NULL,
    [NationalIDorPassportNO] NVARCHAR (128)  NULL,
    [Address]                NVARCHAR (500)  NULL,
    [City]                   NVARCHAR (50)   NULL,
    [Phone]                  NVARCHAR (50)   NULL,
    [Email]                  NVARCHAR (50)   NULL,
    [JoiningDate]            DATE            NULL,
    [Image]                  VARBINARY (MAX) NULL,
    [Insert_Date]            DATETIME        NULL,
    CONSTRAINT [PK_Authority] PRIMARY KEY CLUSTERED ([AuthorityID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Authority_Link_Category]...';


GO
CREATE TABLE [dbo].[Authority_Link_Category] (
    [LinkCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [Category]       NVARCHAR (128) NULL,
    [Ascending]      INT            NULL,
    CONSTRAINT [PK_Authority_Link_Category] PRIMARY KEY CLUSTERED ([LinkCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Authority_Link_Pages]...';


GO
CREATE TABLE [dbo].[Authority_Link_Pages] (
    [LinkID]         INT              IDENTITY (1, 1) NOT NULL,
    [LinkCategoryID] INT              NULL,
    [SubCategoryID]  INT              NULL,
    [RoleId]         UNIQUEIDENTIFIER NULL,
    [PageURL]        NVARCHAR (128)   NULL,
    [PageTitle]      NVARCHAR (128)   NULL,
    [Ascending]      INT              NULL,
    CONSTRAINT [PK_Authority_Link_Pages] PRIMARY KEY CLUSTERED ([LinkID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Authority_Link_SubCategory]...';


GO
CREATE TABLE [dbo].[Authority_Link_SubCategory] (
    [SubCategoryID]  INT            IDENTITY (1, 1) NOT NULL,
    [LinkCategoryID] INT            NULL,
    [SubCategory]    NVARCHAR (128) NULL,
    [Ascending]      INT            NULL,
    CONSTRAINT [PK_Authority_Link_SubCategory] PRIMARY KEY CLUSTERED ([SubCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Authority_Link_Users]...';


GO
CREATE TABLE [dbo].[Authority_Link_Users] (
    [LinkUserID]     INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [LinkID]         INT            NULL,
    [UserName]       NVARCHAR (500) NULL,
    CONSTRAINT [PK_Authority_Link_Users] PRIMARY KEY CLUSTERED ([LinkUserID] ASC)
);


GO
PRINT N'Creating Table [dbo].[CommitteeDonation]...';


GO
CREATE TABLE [dbo].[CommitteeDonation] (
    [CommitteeDonationId]         INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]                    INT             NULL,
    [RegistrationID]              INT             NULL,
    [CommitteeMemberId]           INT             NOT NULL,
    [CommitteeDonationCategoryId] INT             NOT NULL,
    [Amount]                      FLOAT (53)      NOT NULL,
    [PaidAmount]                  FLOAT (53)      NOT NULL,
    [Due]                         AS              ([Amount] - [PaidAmount]) PERSISTED NOT NULL,
    [IsPaid]                      AS              (CONVERT (BIT, CASE WHEN ([Amount] - [PaidAmount]) = (0) THEN (1) ELSE (0) END)) PERSISTED,
    [Description]                 NVARCHAR (1024) NULL,
    [InsertDate]                  DATETIME        NOT NULL,
    [PromiseDate]                 DATE            NULL,
    CONSTRAINT [PK_CommitteeDonation] PRIMARY KEY CLUSTERED ([CommitteeDonationId] ASC)
);


GO
PRINT N'Creating Index [dbo].[CommitteeDonation].[IX_CommitteeDonation]...';


GO
CREATE NONCLUSTERED INDEX [IX_CommitteeDonation]
    ON [dbo].[CommitteeDonation]([SchoolID] ASC, [InsertDate] ASC)
    INCLUDE([CommitteeMemberId], [Amount], [CommitteeDonationCategoryId]);


GO
PRINT N'Creating Table [dbo].[CommitteeDonationCategory]...';


GO
CREATE TABLE [dbo].[CommitteeDonationCategory] (
    [CommitteeDonationCategoryId] INT           IDENTITY (1, 1) NOT NULL,
    [RegistrationID]              INT           NOT NULL,
    [SchoolID]                    INT           NOT NULL,
    [DonationCategory]            NVARCHAR (50) NOT NULL,
    [InsertDate]                  DATE          NOT NULL,
    CONSTRAINT [PK_DonationCategory] PRIMARY KEY CLUSTERED ([CommitteeDonationCategoryId] ASC)
);


GO
PRINT N'Creating Table [dbo].[CommitteeDonationTemplate]...';


GO
CREATE TABLE [dbo].[CommitteeDonationTemplate] (
    [DonationTemplateId]          INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]                    INT             NOT NULL,
    [RegistrationID]              INT             NOT NULL,
    [CommitteeMemberTypeId]       INT             NOT NULL,
    [CommitteeDonationCategoryId] INT             NOT NULL,
    [Amount]                      DECIMAL (18, 2) NOT NULL,
    [CreatedDate]                 DATETIME        NOT NULL,
    CONSTRAINT [PK_CommitteeDonationTemplate] PRIMARY KEY CLUSTERED ([DonationTemplateId] ASC)
);


GO
PRINT N'Creating Table [dbo].[CommitteeMember]...';


GO
CREATE TABLE [dbo].[CommitteeMember] (
    [CommitteeMemberId]     INT             IDENTITY (1, 1) NOT NULL,
    [CommitteeMemberTypeId] INT             NOT NULL,
    [RegistrationID]        INT             NOT NULL,
    [SchoolID]              INT             NOT NULL,
    [MemberName]            NVARCHAR (128)  NOT NULL,
    [ReferenceBy]           NVARCHAR (50)   NULL,
    [SmsNumber]             NVARCHAR (50)   NOT NULL,
    [Address]               NVARCHAR (500)  NULL,
    [Photo]                 VARBINARY (MAX) NULL,
    [TotalDonation]         FLOAT (53)      NOT NULL,
    [PaidDonation]          FLOAT (53)      NOT NULL,
    [DueDonation]           AS              ([TotalDonation] - [PaidDonation]) PERSISTED NOT NULL,
    [InsertDate]            DATETIME        NOT NULL,
    [Email]                 NVARCHAR (100)  NULL,
    [Status]                NVARCHAR (20)   NOT NULL,
    CONSTRAINT [PK_CommitteeMember] PRIMARY KEY CLUSTERED ([CommitteeMemberId] ASC)
);


GO
PRINT N'Creating Index [dbo].[CommitteeMember].[IX_CommitteeMember_Status]...';


GO
CREATE NONCLUSTERED INDEX [IX_CommitteeMember_Status]
    ON [dbo].[CommitteeMember]([Status] ASC);


GO
PRINT N'Creating Index [dbo].[CommitteeMember].[IX_CommitteeMember_Email]...';


GO
CREATE NONCLUSTERED INDEX [IX_CommitteeMember_Email]
    ON [dbo].[CommitteeMember]([Email] ASC) WHERE ([Email] IS NOT NULL);


GO
PRINT N'Creating Table [dbo].[CommitteeMember_Billing]...';


GO
CREATE TABLE [dbo].[CommitteeMember_Billing] (
    [BillingId]             INT      IDENTITY (1, 1) NOT NULL,
    [SchoolID]              INT      NOT NULL,
    [CommitteeMemberTypeId] INT      NOT NULL,
    [IsIncluded]            BIT      NOT NULL,
    [CreatedDate]           DATETIME NOT NULL,
    [UpdatedDate]           DATETIME NOT NULL,
    [IsActive]              BIT      NOT NULL,
    PRIMARY KEY CLUSTERED ([BillingId] ASC),
    CONSTRAINT [UC_School_Category] UNIQUE NONCLUSTERED ([SchoolID] ASC, [CommitteeMemberTypeId] ASC)
);


GO
PRINT N'Creating Index [dbo].[CommitteeMember_Billing].[IX_Billing_IsActive]...';


GO
CREATE NONCLUSTERED INDEX [IX_Billing_IsActive]
    ON [dbo].[CommitteeMember_Billing]([IsActive] ASC);


GO
PRINT N'Creating Index [dbo].[CommitteeMember_Billing].[IX_Billing_SchoolID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Billing_SchoolID]
    ON [dbo].[CommitteeMember_Billing]([SchoolID] ASC);


GO
PRINT N'Creating Index [dbo].[CommitteeMember_Billing].[IX_Billing_CategoryID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Billing_CategoryID]
    ON [dbo].[CommitteeMember_Billing]([CommitteeMemberTypeId] ASC);


GO
PRINT N'Creating Index [dbo].[CommitteeMember_Billing].[IX_Billing_IsIncluded]...';


GO
CREATE NONCLUSTERED INDEX [IX_Billing_IsIncluded]
    ON [dbo].[CommitteeMember_Billing]([IsIncluded] ASC);


GO
PRINT N'Creating Table [dbo].[CommitteeMemberType]...';


GO
CREATE TABLE [dbo].[CommitteeMemberType] (
    [CommitteeMemberTypeId] INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID]        INT            NOT NULL,
    [SchoolID]              INT            NOT NULL,
    [CommitteeMemberType]   NVARCHAR (256) NOT NULL,
    [InsertDate]            DATE           NOT NULL,
    CONSTRAINT [PK_CommitteeMemberType] PRIMARY KEY CLUSTERED ([CommitteeMemberTypeId] ASC)
);


GO
PRINT N'Creating Table [dbo].[CommitteeMoneyReceipt]...';


GO
CREATE TABLE [dbo].[CommitteeMoneyReceipt] (
    [CommitteeMoneyReceiptId] INT        IDENTITY (1, 1) NOT NULL,
    [RegistrationId]          INT        NOT NULL,
    [SchoolId]                INT        NOT NULL,
    [CommitteeMemberId]       INT        NOT NULL,
    [EducationYearId]         INT        NOT NULL,
    [AccountId]               INT        NOT NULL,
    [CommitteeMoneyReceiptSn] INT        NOT NULL,
    [TotalAmount]             FLOAT (53) NOT NULL,
    [PaidDate]                DATETIME   NOT NULL,
    [InsertDate]              DATE       NOT NULL,
    CONSTRAINT [PK_CommitteeMoneyReceipt] PRIMARY KEY CLUSTERED ([CommitteeMoneyReceiptId] ASC)
);


GO
PRINT N'Creating Index [dbo].[CommitteeMoneyReceipt].[IX_CommitteeMoneyReceipt]...';


GO
CREATE NONCLUSTERED INDEX [IX_CommitteeMoneyReceipt]
    ON [dbo].[CommitteeMoneyReceipt]([CommitteeMoneyReceiptId] ASC, [SchoolId] ASC, [AccountId] ASC)
    INCLUDE([CommitteeMoneyReceiptSn], [TotalAmount]);


GO
PRINT N'Creating Table [dbo].[CommitteePaymentRecord]...';


GO
CREATE TABLE [dbo].[CommitteePaymentRecord] (
    [CommitteePaymentRecordId] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolId]                 INT        NOT NULL,
    [RegistrationId]           INT        NOT NULL,
    [CommitteeDonationId]      INT        NOT NULL,
    [CommitteeMoneyReceiptId]  INT        NOT NULL,
    [PaidAmount]               FLOAT (53) NOT NULL,
    CONSTRAINT [PK_CommitteePaymentRecord] PRIMARY KEY CLUSTERED ([CommitteePaymentRecordId] ASC)
);


GO
PRINT N'Creating Index [dbo].[CommitteePaymentRecord].[IX_CommitteePaymentRecord]...';


GO
CREATE NONCLUSTERED INDEX [IX_CommitteePaymentRecord]
    ON [dbo].[CommitteePaymentRecord]([CommitteeDonationId] ASC, [SchoolId] ASC, [CommitteeMoneyReceiptId] ASC)
    INCLUDE([PaidAmount]);


GO
PRINT N'Creating Table [dbo].[CreateClass]...';


GO
CREATE TABLE [dbo].[CreateClass] (
    [ClassID]        INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [RegistrationID] INT            NOT NULL,
    [Class]          NVARCHAR (128) NULL,
    [SN]             INT            NULL,
    CONSTRAINT [PK_Class_1] PRIMARY KEY CLUSTERED ([ClassID] ASC)
);


GO
PRINT N'Creating Table [dbo].[CreateSection]...';


GO
CREATE TABLE [dbo].[CreateSection] (
    [SectionID]      INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [RegistrationID] INT            NOT NULL,
    [ClassID]        INT            NULL,
    [Section]        NVARCHAR (100) NULL,
    CONSTRAINT [PK_CreateSection] PRIMARY KEY CLUSTERED ([SectionID] ASC)
);


GO
PRINT N'Creating Table [dbo].[CreateShift]...';


GO
CREATE TABLE [dbo].[CreateShift] (
    [ShiftID]        INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT            NOT NULL,
    [SchoolID]       INT            NULL,
    [ClassID]        INT            NULL,
    [Shift]          NVARCHAR (100) NULL,
    CONSTRAINT [PK_CreateShift] PRIMARY KEY CLUSTERED ([ShiftID] ASC)
);


GO
PRINT N'Creating Table [dbo].[CreateSubjectGroup]...';


GO
CREATE TABLE [dbo].[CreateSubjectGroup] (
    [SubjectGroupID] INT           IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT           NOT NULL,
    [SchoolID]       INT           NOT NULL,
    [ClassID]        INT           NULL,
    [SubjectGroup]   NVARCHAR (50) NULL,
    CONSTRAINT [PK_CreateSubjectGroup] PRIMARY KEY CLUSTERED ([SubjectGroupID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Device_Commands]...';


GO
CREATE TABLE [dbo].[Device_Commands] (
    [CommandID]          INT            IDENTITY (1, 1) NOT NULL,
    [DeviceSerialNumber] NVARCHAR (100) NOT NULL,
    [Command]            NVARCHAR (MAX) NOT NULL,
    [CommandType]        NVARCHAR (50)  NULL,
    [CommandStatus]      NVARCHAR (50)  NOT NULL,
    [CreatedDate]        DATETIME       NOT NULL,
    [ProcessedDate]      DATETIME       NULL,
    [ResponseData]       NVARCHAR (MAX) NULL,
    [ErrorMessage]       NVARCHAR (MAX) NULL,
    [CreatedBy]          NVARCHAR (100) NULL,
    [SchoolID]           INT            NULL,
    PRIMARY KEY CLUSTERED ([CommandID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Device_Commands].[IX_Device_Commands_Status]...';


GO
CREATE NONCLUSTERED INDEX [IX_Device_Commands_Status]
    ON [dbo].[Device_Commands]([CommandStatus] ASC);


GO
PRINT N'Creating Index [dbo].[Device_Commands].[IX_Device_Commands_Date]...';


GO
CREATE NONCLUSTERED INDEX [IX_Device_Commands_Date]
    ON [dbo].[Device_Commands]([CreatedDate] DESC);


GO
PRINT N'Creating Index [dbo].[Device_Commands].[IX_Device_Commands_Serial]...';


GO
CREATE NONCLUSTERED INDEX [IX_Device_Commands_Serial]
    ON [dbo].[Device_Commands]([DeviceSerialNumber] ASC);


GO
PRINT N'Creating Table [dbo].[Device_Finger_Print_Record]...';


GO
CREATE TABLE [dbo].[Device_Finger_Print_Record] (
    [Finger_PrintID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [DeviceID]       INT            NULL,
    [Finger_Index]   INT            NULL,
    [Temp_Data]      NVARCHAR (MAX) NULL,
    [Flag]           INT            NULL,
    CONSTRAINT [PK_Device_Finger_Print_Record] PRIMARY KEY CLUSTERED ([Finger_PrintID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Device_Institution_Mapping]...';


GO
CREATE TABLE [dbo].[Device_Institution_Mapping] (
    [MappingID]          INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]           INT            NOT NULL,
    [DeviceSerialNumber] NVARCHAR (50)  NOT NULL,
    [DeviceName]         NVARCHAR (100) NULL,
    [DeviceLocation]     NVARCHAR (200) NULL,
    [IsActive]           BIT            NOT NULL,
    [CreatedDate]        DATETIME       NOT NULL,
    [LastPushTime]       DATETIME       NULL,
    [Remarks]            NVARCHAR (500) NULL,
    CONSTRAINT [PK_Device_Institution_Mapping] PRIMARY KEY CLUSTERED ([MappingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Education_Year]...';


GO
CREATE TABLE [dbo].[Education_Year] (
    [EducationYearID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NULL,
    [RegistrationID]  INT            NULL,
    [EducationYear]   NVARCHAR (128) NULL,
    [Status]          NVARCHAR (50)  NULL,
    [StartDate]       DATE           NULL,
    [EndDate]         DATE           NULL,
    [IsActive]        BIT            NULL,
    [SN]              INT            NULL,
    CONSTRAINT [PK_Education_Year] PRIMARY KEY CLUSTERED ([EducationYearID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Education_Year].[IX_EducationYear]...';


GO
CREATE NONCLUSTERED INDEX [IX_EducationYear]
    ON [dbo].[Education_Year]([SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([EducationYear], [StartDate], [EndDate], [IsActive]);


GO
PRINT N'Creating Table [dbo].[Education_Year_User]...';


GO
CREATE TABLE [dbo].[Education_Year_User] (
    [EducationYear_UserID] INT IDENTITY (1, 1) NOT NULL,
    [RegistrationID]       INT NULL,
    [EducationYearID]      INT NOT NULL,
    [SchoolID]             INT NULL,
    CONSTRAINT [PK_Education_Year_User] PRIMARY KEY CLUSTERED ([EducationYear_UserID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Education_Year_User].[IX_EducationYearUser]...';


GO
CREATE NONCLUSTERED INDEX [IX_EducationYearUser]
    ON [dbo].[Education_Year_User]([EducationYearID] ASC, [SchoolID] ASC)
    INCLUDE([RegistrationID]);


GO
PRINT N'Creating Table [dbo].[Employee_Allowance]...';


GO
CREATE TABLE [dbo].[Employee_Allowance] (
    [AllowanceID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [AllowanceName]  NVARCHAR (100) NULL,
    [CreateDate]     DATE           NULL,
    CONSTRAINT [PK_Employee_Allowance] PRIMARY KEY CLUSTERED ([AllowanceID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Allowance_Assign]...';


GO
CREATE TABLE [dbo].[Employee_Allowance_Assign] (
    [AllowanceAssignID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT           NULL,
    [RegistrationID]    INT           NULL,
    [AllowanceID]       INT           NULL,
    [EmployeeID]        INT           NULL,
    [AllowanceAmount]   FLOAT (53)    NULL,
    [Fixed_Percetage]   NVARCHAR (50) NULL,
    [CreateDate]        DATE          NULL,
    CONSTRAINT [PK_Employee_Allowance_Assign] PRIMARY KEY CLUSTERED ([AllowanceAssignID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Allowance_Records]...';


GO
CREATE TABLE [dbo].[Employee_Allowance_Records] (
    [Allowance_RecordsID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT        NULL,
    [RegistrationID]      INT        NULL,
    [AllowanceID]         INT        NULL,
    [EmployeeID]          INT        NULL,
    [Employee_PayorderID] INT        NOT NULL,
    [AllowanceAmount]     FLOAT (53) NULL,
    [CreateDate]          DATE       NULL,
    CONSTRAINT [PK_Employee_Allowance_Records] PRIMARY KEY CLUSTERED ([Allowance_RecordsID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Attendance_Record]...';


GO
CREATE TABLE [dbo].[Employee_Attendance_Record] (
    [Employee_Attendance_RecordID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]                     INT           NOT NULL,
    [RegistrationID]               INT           NOT NULL,
    [EmployeeID]                   INT           NOT NULL,
    [AttendanceStatus]             NVARCHAR (50) NULL,
    [AttendanceDate]               DATE          NULL,
    [EntryTime]                    TIME (7)      NULL,
    [ExitTime]                     TIME (7)      NULL,
    [CreatedDate]                  DATE          NULL,
    [ExitStatus]                   NVARCHAR (50) NULL,
    [Is_OUT]                       BIT           NOT NULL,
    [IsFromDevice]                 BIT           NOT NULL,
    [Attendance_ScheduleID]        INT           NULL,
    CONSTRAINT [PK_Employee_Attendance_Record] PRIMARY KEY CLUSTERED ([Employee_Attendance_RecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Employee_Attendance_Record].[IX_EmployeeAttendanceRecord]...';


GO
CREATE NONCLUSTERED INDEX [IX_EmployeeAttendanceRecord]
    ON [dbo].[Employee_Attendance_Record]([EmployeeID] ASC, [AttendanceDate] ASC, [SchoolID] ASC)
    INCLUDE([AttendanceStatus], [EntryTime], [ExitTime]);


GO
PRINT N'Creating Index [dbo].[Employee_Attendance_Record].[UQ_Employee_Attendance_Record_Employee_Date_Schedule]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Employee_Attendance_Record_Employee_Date_Schedule]
    ON [dbo].[Employee_Attendance_Record]([SchoolID] ASC, [EmployeeID] ASC, [AttendanceDate] ASC, [Attendance_ScheduleID] ASC) WHERE ([Attendance_ScheduleID] IS NOT NULL);


GO
PRINT N'Creating Table [dbo].[Employee_Attendance_Report]...';


GO
CREATE TABLE [dbo].[Employee_Attendance_Report] (
    [Employee_Attendance_ReportID] INT           NOT NULL,
    [SchoolID]                     INT           NULL,
    [RegistrationID]               INT           NULL,
    [EducationYearID]              INT           NULL,
    [EmployeeID]                   INT           NOT NULL,
    [ReportName]                   NVARCHAR (50) NULL,
    [Total_WorkingDays]            INT           NULL,
    [Total_Present]                INT           NULL,
    [Total_Absent]                 INT           NULL,
    [Total_Late]                   INT           NULL,
    [Total_Leave]                  INT           NULL,
    [Report_StartDate]             DATE          NULL,
    [Report_EndDate]               DATE          NULL,
    [CreateDate]                   DATE          NULL,
    CONSTRAINT [PK_Employee_Attendance_Report] PRIMARY KEY CLUSTERED ([Employee_Attendance_ReportID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Employee_Attendance_Report].[IX_EmployeeAttendanceReport]...';


GO
CREATE NONCLUSTERED INDEX [IX_EmployeeAttendanceReport]
    ON [dbo].[Employee_Attendance_Report]([EmployeeID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([Total_WorkingDays], [Total_Present], [Total_Absent], [Total_Late]);


GO
PRINT N'Creating Table [dbo].[Employee_Attendance_Schedule_Assign]...';


GO
CREATE TABLE [dbo].[Employee_Attendance_Schedule_Assign] (
    [Employee_Schedule_AssignID] INT  IDENTITY (1, 1) NOT NULL,
    [SchoolID]                   INT  NULL,
    [RegistrationID]             INT  NULL,
    [EmployeeID]                 INT  NOT NULL,
    [CreateDate]                 DATE NULL,
    [ScheduleID]                 INT  NULL,
    [Is_Abs_SMS]                 BIT  NULL,
    [Is_Late_SMS]                BIT  NULL,
    CONSTRAINT [PK_Employee_Schedule_Assign] PRIMARY KEY CLUSTERED ([Employee_Schedule_AssignID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Bonus]...';


GO
CREATE TABLE [dbo].[Employee_Bonus] (
    [BonusID]        INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [BonusName]      NVARCHAR (100) NULL,
    [CreateDate]     DATE           NULL,
    CONSTRAINT [PK_Employee_Bonus] PRIMARY KEY CLUSTERED ([BonusID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Bonus_Records]...';


GO
CREATE TABLE [dbo].[Employee_Bonus_Records] (
    [Bonus_RecordsID]     INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT        NULL,
    [RegistrationID]      INT        NULL,
    [BonusID]             INT        NOT NULL,
    [EmployeeID]          INT        NULL,
    [Employee_PayorderID] INT        NOT NULL,
    [Bonus_Amount]        FLOAT (53) NULL,
    [CreateDate]          DATE       NULL,
    CONSTRAINT [PK_Employee_Bonus_Records] PRIMARY KEY CLUSTERED ([Bonus_RecordsID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Deduction]...';


GO
CREATE TABLE [dbo].[Employee_Deduction] (
    [DeductionID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [DeductionName]  NVARCHAR (100) NULL,
    [CreateDate]     DATE           NULL,
    CONSTRAINT [PK_Employee_Deduction] PRIMARY KEY CLUSTERED ([DeductionID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Deduction_Assign]...';


GO
CREATE TABLE [dbo].[Employee_Deduction_Assign] (
    [DeductionAssignID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT           NULL,
    [RegistrationID]    INT           NULL,
    [DeductionID]       INT           NULL,
    [EmployeeID]        INT           NULL,
    [DeductionAmount]   FLOAT (53)    NULL,
    [Fixed_Percetage]   NVARCHAR (50) NULL,
    [CreateDate]        DATE          NULL,
    CONSTRAINT [PK_Employee_Deduction_Assign] PRIMARY KEY CLUSTERED ([DeductionAssignID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Deduction_Records]...';


GO
CREATE TABLE [dbo].[Employee_Deduction_Records] (
    [Deduction_RecordsID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT        NULL,
    [RegistrationID]      INT        NULL,
    [DeductionID]         INT        NOT NULL,
    [EmployeeID]          INT        NULL,
    [Employee_PayorderID] INT        NOT NULL,
    [Deduction_Amount]    FLOAT (53) NULL,
    [CreateDate]          DATE       NULL,
    CONSTRAINT [PK_Employee_Deduction_Records] PRIMARY KEY CLUSTERED ([Deduction_RecordsID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Fine]...';


GO
CREATE TABLE [dbo].[Employee_Fine] (
    [FineID]         INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [FineName]       NVARCHAR (100) NULL,
    [CreateDate]     DATE           NULL,
    CONSTRAINT [PK_Employee_Fine] PRIMARY KEY CLUSTERED ([FineID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Fine_Records]...';


GO
CREATE TABLE [dbo].[Employee_Fine_Records] (
    [Fine_RecordsID]      INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT        NULL,
    [RegistrationID]      INT        NULL,
    [FineID]              INT        NOT NULL,
    [EmployeeID]          INT        NULL,
    [Employee_PayorderID] INT        NOT NULL,
    [Fine_Amount]         FLOAT (53) NULL,
    [CreateDate]          DATE       NULL,
    CONSTRAINT [PK_Employee_Fine_Records] PRIMARY KEY CLUSTERED ([Fine_RecordsID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Holiday]...';


GO
CREATE TABLE [dbo].[Employee_Holiday] (
    [HolidayID]       INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NOT NULL,
    [RegistrationID]  INT            NOT NULL,
    [EducationYearID] INT            NOT NULL,
    [HolidayName]     NVARCHAR (100) NULL,
    [HolidayDate]     DATE           NULL,
    [CreateDate]      DATE           NULL,
    CONSTRAINT [PK_Employee_Holiday] PRIMARY KEY CLUSTERED ([HolidayID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Info]...';


GO
CREATE TABLE [dbo].[Employee_Info] (
    [EmployeeID]               INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                 INT            NOT NULL,
    [RegistrationID]           INT            NOT NULL,
    [ID]                       NVARCHAR (50)  NULL,
    [RFID]                     NVARCHAR (50)  NULL,
    [DeviceID]                 INT            NULL,
    [EmployeeType]             NVARCHAR (500) NULL,
    [Employee_Payorder_NameID] INT            NULL,
    [Permanent_Temporary]      NVARCHAR (50)  NULL,
    [Work_Time_Basis]          NVARCHAR (50)  NULL,
    [Time_Basis_Type]          NVARCHAR (50)  NULL,
    [Salary]                   FLOAT (53)     NULL,
    [IS_Abs_Deducted]          BIT            NULL,
    [Abs_Deduction]            FLOAT (53)     NULL,
    [IS_Late_Count_As_Abs]     BIT            NULL,
    [Late_Days]                INT            NULL,
    [Job_Status]               NVARCHAR (50)  NULL,
    [CreateDate]               DATE           NULL,
    [Bank_AccNo]               NVARCHAR (128) NULL,
    [SubCategoryID]            INT            NULL,
    CONSTRAINT [PK_Employee_Info] PRIMARY KEY CLUSTERED ([EmployeeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Leave]...';


GO
CREATE TABLE [dbo].[Employee_Leave] (
    [Employee_LeaveID]          INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                  INT            NOT NULL,
    [RegistrationID]            INT            NOT NULL,
    [EducationYearID]           INT            NOT NULL,
    [EmployeeID]                INT            NOT NULL,
    [LeaveStartDate]            DATE           NULL,
    [LeaveEndDate]              DATE           NULL,
    [LeaveReason]               NVARCHAR (400) NULL,
    [ApproveStatus]             NVARCHAR (50)  NULL,
    [ApprovedBy_RegistrationID] INT            NULL,
    [CreateDate]                DATE           NULL,
    CONSTRAINT [PK_Employee_Leave] PRIMARY KEY CLUSTERED ([Employee_LeaveID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder]...';


GO
CREATE TABLE [dbo].[Employee_Payorder] (
    [Employee_PayorderID]      INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]                 INT        NOT NULL,
    [RegistrationID]           INT        NOT NULL,
    [EducationYearID]          INT        NOT NULL,
    [EmployeeID]               INT        NOT NULL,
    [Employee_Payorder_NameID] INT        NOT NULL,
    [PayorderAmount]           FLOAT (53) NULL,
    [Allowance]                FLOAT (53) NULL,
    [Bonus]                    FLOAT (53) NULL,
    [Diduction]                FLOAT (53) NULL,
    [Fine]                     FLOAT (53) NULL,
    [InTotalSalary]            AS         ((([PayorderAmount] + [Allowance]) + [Bonus]) - ([Diduction] + [Fine])) PERSISTED,
    [PaidAmount]               FLOAT (53) NULL,
    [Due]                      AS         (((([PayorderAmount] + [Allowance]) + [Bonus]) - ([Diduction] + [Fine])) - [PaidAmount]) PERSISTED,
    [PaidStatus]               AS         (CASE WHEN (((([PayorderAmount] + [Allowance]) + [Bonus]) - ([Diduction] + [Fine])) - [PaidAmount]) = (0) THEN 'Paid' ELSE 'Due' END) PERSISTED NOT NULL,
    [Employee_Payorder_SN]     INT        NULL,
    [PayorderDate]             DATE       NULL,
    [GrossSalary]              AS         (([PayorderAmount] + [Allowance]) + [Bonus]) PERSISTED,
    CONSTRAINT [PK_Employee_Payorder] PRIMARY KEY CLUSTERED ([Employee_PayorderID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Daily]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Daily] (
    [DailyPayorderID]     INT           IDENTITY (1, 1) NOT NULL,
    [Employee_PayorderID] INT           NOT NULL,
    [SchoolID]            INT           NOT NULL,
    [RegistrationID]      INT           NOT NULL,
    [EducationYearID]     INT           NOT NULL,
    [EmployeeID]          INT           NOT NULL,
    [DailyName]           NVARCHAR (50) NULL,
    [DailyStartDate]      DATE          NULL,
    [DailyEndDate]        DATE          NULL,
    [CreateDate]          DATE          NULL,
    [Amount]              FLOAT (53)    NULL,
    CONSTRAINT [PK_Employee_Daily_Payorder] PRIMARY KEY CLUSTERED ([DailyPayorderID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Monthly]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Monthly] (
    [MonthlyPayorderID]   INT           IDENTITY (1, 1) NOT NULL,
    [Employee_PayorderID] INT           NOT NULL,
    [SchoolID]            INT           NOT NULL,
    [RegistrationID]      INT           NOT NULL,
    [EducationYearID]     INT           NOT NULL,
    [EmployeeID]          INT           NOT NULL,
    [MonthName]           NVARCHAR (50) NULL,
    [MonthStartDate]      DATE          NULL,
    [MonthEndDate]        DATE          NULL,
    [CreateDate]          DATE          NULL,
    [Amount]              FLOAT (53)    NULL,
    [WorkingDays]         INT           NULL,
    [FineCountDays]       INT           NULL,
    [FineAmount]          FLOAT (53)    NULL,
    [AbsDays]             INT           NULL,
    [LateDays]            INT           NULL,
    [LeaveDays]           INT           NULL,
    [PerDays]             INT           NULL,
    CONSTRAINT [PK_Employee_Monthly_Payorder] PRIMARY KEY CLUSTERED ([MonthlyPayorderID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Employee_Payorder_Monthly].[IX_EmployeePayorderMonthly]...';


GO
CREATE NONCLUSTERED INDEX [IX_EmployeePayorderMonthly]
    ON [dbo].[Employee_Payorder_Monthly]([EmployeeID] ASC, [MonthName] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([Amount], [CreateDate]);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Name]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Name] (
    [Employee_Payorder_NameID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                 INT            NULL,
    [RegistrationID]           INT            NULL,
    [Payorder_Name]            NVARCHAR (100) NULL,
    [CreateDate]               DATE           NULL,
    CONSTRAINT [PK_Employee_Payorder_Name] PRIMARY KEY CLUSTERED ([Employee_Payorder_NameID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Records]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Records] (
    [Employee_Payorder_RecordID] INT            IDENTITY (1, 1) NOT NULL,
    [Employee_PayorderID]        INT            NULL,
    [SchoolID]                   INT            NOT NULL,
    [RegistrationID]             INT            NOT NULL,
    [EducationYearID]            INT            NULL,
    [EmployeeID]                 INT            NULL,
    [AccountID]                  INT            NULL,
    [Amount]                     FLOAT (53)     NULL,
    [Paid_For]                   NVARCHAR (128) NULL,
    [Paid_date]                  DATE           NULL,
    [Insert_Date]                DATETIME       NULL,
    CONSTRAINT [PK_Employee_Payorder_Records] PRIMARY KEY CLUSTERED ([Employee_Payorder_RecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Employee_Payorder_Records].[IX_EmployeePayorderRecords]...';


GO
CREATE NONCLUSTERED INDEX [IX_EmployeePayorderRecords]
    ON [dbo].[Employee_Payorder_Records]([EmployeeID] ASC, [Employee_PayorderID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([Paid_date], [Amount], [AccountID]);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Weekly]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Weekly] (
    [WeeklyPayorderID]    INT           IDENTITY (1, 1) NOT NULL,
    [Employee_PayorderID] INT           NOT NULL,
    [SchoolID]            INT           NOT NULL,
    [RegistrationID]      INT           NOT NULL,
    [EducationYearID]     INT           NOT NULL,
    [EmployeeID]          INT           NOT NULL,
    [WeekName]            NVARCHAR (50) NULL,
    [WeekStartDate]       DATE          NULL,
    [WeekEndDate]         DATE          NULL,
    [CreateDate]          DATE          NULL,
    [Amount]              FLOAT (53)    NULL,
    CONSTRAINT [PK_Employee_Weekly_Payorder] PRIMARY KEY CLUSTERED ([WeeklyPayorderID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_Payorder_Work_Basis]...';


GO
CREATE TABLE [dbo].[Employee_Payorder_Work_Basis] (
    [WorkingPayorderID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NOT NULL,
    [RegistrationID]    INT            NOT NULL,
    [EducationYearID]   INT            NOT NULL,
    [EmployeeID]        INT            NOT NULL,
    [WorkingName]       NVARCHAR (100) NULL,
    [WorkingQunatity]   FLOAT (53)     NULL,
    [CreateDate]        DATE           NULL,
    [Amount]            FLOAT (53)     NULL,
    CONSTRAINT [PK_Employee_Work_Basis_Payorder] PRIMARY KEY CLUSTERED ([WorkingPayorderID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Employee_SubCategory]...';


GO
CREATE TABLE [dbo].[Employee_SubCategory] (
    [SubCategoryID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NOT NULL,
    [SubCategoryName] NVARCHAR (100) NOT NULL,
    [EmployeeType]    NVARCHAR (50)  NOT NULL,
    PRIMARY KEY CLUSTERED ([SubCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_ExamList]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_ExamList] (
    [CumulativeExamListID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT        NOT NULL,
    [RegistrationID]       INT        NOT NULL,
    [EducationYearID]      INT        NULL,
    [ExamID]               INT        NULL,
    [ClassID]              INT        NULL,
    [CumulativeNameID]     INT        NULL,
    [Date]                 DATE       NULL,
    [ExamAdd_Percentage]   FLOAT (53) NULL,
    [Exam_EnableFail]      BIT        NULL,
    [Publish_SettingID]    INT        NULL,
    [Cumulative_SettingID] INT        NULL,
    CONSTRAINT [PK_Exam_Cumulative_ExamList_1] PRIMARY KEY CLUSTERED ([CumulativeExamListID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_FullMarks]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_FullMarks] (
    [CumulativeFullMarksID] INT        IDENTITY (1, 1) NOT NULL,
    [Cumulative_SettingID]  INT        NULL,
    [CumulativeNameID]      INT        NOT NULL,
    [SchoolID]              INT        NOT NULL,
    [RegistrationID]        INT        NOT NULL,
    [SubjectID]             INT        NOT NULL,
    [ClassID]               INT        NOT NULL,
    [EducationYearID]       INT        NOT NULL,
    [FullMarks]             FLOAT (53) NOT NULL,
    [Date]                  DATE       NOT NULL,
    CONSTRAINT [PK_Exam_Cumulative_FullMarks] PRIMARY KEY CLUSTERED ([CumulativeFullMarksID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Cumulative_FullMarks].[IX_Cumulative_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_Cumulative_Performance]
    ON [dbo].[Exam_Cumulative_FullMarks]([CumulativeFullMarksID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([FullMarks], [Cumulative_SettingID], [ClassID]);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_Name]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_Name] (
    [CumulativeNameID]     INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT            NOT NULL,
    [RegistrationID]       INT            NOT NULL,
    [EducationYearID]      INT            NULL,
    [CumulativeResultName] NVARCHAR (128) NULL,
    [Date]                 DATE           NULL,
    CONSTRAINT [PK_Exam_Cumulative_Name] PRIMARY KEY CLUSTERED ([CumulativeNameID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_Setting]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_Setting] (
    [Cumulative_SettingID]              INT           IDENTITY (1, 1) NOT NULL,
    [CumulativeNameID]                  INT           NOT NULL,
    [SchoolID]                          INT           NOT NULL,
    [RegistrationID]                    INT           NOT NULL,
    [EducationYearID]                   INT           NOT NULL,
    [ClassID]                           INT           NOT NULL,
    [IS_Fail_Enable_Optional_Subject]   BIT           NULL,
    [IS_Add_Optional_Mark_In_FullMarks] BIT           NULL,
    [IS_Enable_Grade_as_it_is_if_Fail]  BIT           NULL,
    [Optional_Percentage_Deduction]     FLOAT (53)    NULL,
    [IS_Published]                      BIT           NULL,
    [Last_Published_Date]               DATETIME      NULL,
    [Exam_Position_Format]              NVARCHAR (50) NULL,
    [IS_Hide_SubExam]                   BIT           NULL,
    [IS_Hide_Sec_Position]              BIT           NULL,
    [IS_Hide_Class_Position]            BIT           NULL,
    [Attendance_FromDate]               DATE          NULL,
    [Attendance_ToDate]                 DATE          NULL,
    [GradeNameID]                       INT           NULL,
    [IS_Grade_BasePoint]                BIT           NULL,
    [Attendance_ScheduleID]             INT           NOT NULL,
    CONSTRAINT [PK_Exam_Cumulative_Setting] PRIMARY KEY CLUSTERED ([Cumulative_SettingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_Student]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_Student] (
    [Cumulative_StudentID]          INT           IDENTITY (1, 1) NOT NULL,
    [Cumulative_SettingID]          INT           NULL,
    [CumulativeNameID]              INT           NULL,
    [SchoolID]                      INT           NOT NULL,
    [RegistrationID]                INT           NOT NULL,
    [EducationYearID]               INT           NOT NULL,
    [StudentID]                     INT           NOT NULL,
    [StudentClassID]                INT           NOT NULL,
    [ClassID]                       INT           NOT NULL,
    [TotalMark_ofStudent]           FLOAT (53)    NULL,
    [ObtainedMark_ofStudent]        FLOAT (53)    NULL,
    [ObtainedPercentage_ofStudent]  AS            (round(([ObtainedMark_ofStudent] * (100)) / [TotalMark_ofStudent], (2), (0))) PERSISTED,
    [StudentAbsenceStatus]          NVARCHAR (50) NULL,
    [PassStatus_InSubject]          NVARCHAR (50) NULL,
    [PassStatus_Student]            AS            (CASE WHEN [ObtainedMark_ofStudent] < [PassMark_Student] THEN 'F' ELSE 'P' END),
    [PassMark_Student]              FLOAT (53)    NULL,
    [PassPercentage_Student]        FLOAT (53)    NULL,
    [Date]                          DATE          NULL,
    [TotalSubjest_WithOptional]     INT           NULL,
    [TotalSubject]                  INT           NULL,
    [TotalPoint]                    FLOAT (53)    NULL,
    [Student_Point]                 FLOAT (53)    NULL,
    [Student_Grade]                 NVARCHAR (50) NULL,
    [HighestMark_InExam_Class]      FLOAT (53)    NULL,
    [HighestMark_InExam_Subsection] FLOAT (53)    NULL,
    [Position_InExam_Class]         NVARCHAR (50) NULL,
    [Position_InExam_Subsection]    NVARCHAR (50) NULL,
    [Student_Comments]              NVARCHAR (50) NULL,
    [Average]                       AS            (round([ObtainedMark_ofStudent] / [TotalSubject], (2), (0))),
    [NotGolden]                     BIT           NULL,
    [IsFailed]                      AS            (CASE WHEN [PassStatus_InSubject] = N'F'
                                                             OR [ObtainedMark_ofStudent] < [PassMark_Student] THEN (1) ELSE (0) END) PERSISTED NOT NULL,
    CONSTRAINT [PK_Exam_Cumulative_Student] PRIMARY KEY CLUSTERED ([Cumulative_StudentID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Cumulative_Subject]...';


GO
CREATE TABLE [dbo].[Exam_Cumulative_Subject] (
    [Cumulative_SubjectID]             INT           IDENTITY (1, 1) NOT NULL,
    [Cumulative_SettingID]             INT           NULL,
    [SchoolID]                         INT           NOT NULL,
    [RegistrationID]                   INT           NOT NULL,
    [EducationYearID]                  INT           NOT NULL,
    [StudentID]                        INT           NOT NULL,
    [StudentClassID]                   INT           NOT NULL,
    [ClassID]                          INT           NOT NULL,
    [SubjectID]                        INT           NOT NULL,
    [CumulativeNameID]                 INT           NULL,
    [TotalMark_ofSubject]              FLOAT (53)    NULL,
    [ObtainedMark_ofSubject]           FLOAT (53)    NULL,
    [ObtainedPercentage_ofSubject]     AS            (round(([ObtainedMark_ofSubject] * (100)) / [TotalMark_ofSubject], (2), (0))) PERSISTED,
    [SubjectAbsenceStatus]             NVARCHAR (50) NULL,
    [PassStatus_Subject]               VARCHAR (50)  NULL,
    [PassPercentage_Subject]           FLOAT (53)    NULL,
    [PassMark_Subject]                 FLOAT (53)    NULL,
    [Date]                             DATE          NULL,
    [SubjectGrades]                    NVARCHAR (50) NULL,
    [SubjectPoint]                     FLOAT (53)    NULL,
    [OMark_ofSub_ConsiderOptional]     FLOAT (53)    NULL,
    [SubjectPoint_ConsiderOptional]    FLOAT (53)    NULL,
    [HighestMark_InSubject_Class]      FLOAT (53)    NULL,
    [HighestMark_InSubject_Subsection] FLOAT (53)    NULL,
    [Position_InSubject_Class]         NVARCHAR (50) NULL,
    [Position_InSubject_Subsection]    NVARCHAR (50) NULL,
    [SubjectType]                      NVARCHAR (10) NULL,
    [IS_Add_InExam]                    BIT           NULL,
    CONSTRAINT [PK_Exam_Cumulative_Subject] PRIMARY KEY CLUSTERED ([Cumulative_SubjectID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Cumulative_Subject].[IX_Exam_Cumulative_Subject_Cu_Exam]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Cumulative_Subject_Cu_Exam]
    ON [dbo].[Exam_Cumulative_Subject]([SubjectType] ASC)
    INCLUDE([Cumulative_SettingID], [StudentID], [StudentClassID]);


GO
PRINT N'Creating Table [dbo].[Exam_Full_Marks]...';


GO
CREATE TABLE [dbo].[Exam_Full_Marks] (
    [ExamFullMarksID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT        NOT NULL,
    [RegistrationID]  INT        NOT NULL,
    [SubjectID]       INT        NULL,
    [ExamID]          INT        NULL,
    [ClassID]         INT        NULL,
    [SubExamID]       INT        NULL,
    [EducationYearID] INT        NULL,
    [FullMarks]       FLOAT (53) NULL,
    [Date]            DATE       NULL,
    [Sub_PassMarks]   FLOAT (53) NULL,
    CONSTRAINT [PK_Exam_Full_Marks] PRIMARY KEY CLUSTERED ([ExamFullMarksID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Full_Marks].[IX_Exam_Full_Marks_SP]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Full_Marks_SP]
    ON [dbo].[Exam_Full_Marks]([SchoolID] ASC, [SubjectID] ASC, [ExamID] ASC, [ClassID] ASC, [EducationYearID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Full_Marks].[IX_Exam_Full_Marks_ReSubmit]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Full_Marks_ReSubmit]
    ON [dbo].[Exam_Full_Marks]([SchoolID] ASC, [ExamID] ASC, [ClassID] ASC, [EducationYearID] ASC)
    INCLUDE([SubjectID], [SubExamID], [FullMarks], [Sub_PassMarks]);


GO
PRINT N'Creating Table [dbo].[Exam_Grade_Name]...';


GO
CREATE TABLE [dbo].[Exam_Grade_Name] (
    [GradeNameID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [RegistrationID] INT            NULL,
    [GradeName]      NVARCHAR (128) NULL,
    [Insert_Date]    DATETIME       NOT NULL,
    CONSTRAINT [PK_Exam_Grade_Name] PRIMARY KEY CLUSTERED ([GradeNameID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Grading_Assign]...';


GO
CREATE TABLE [dbo].[Exam_Grading_Assign] (
    [ExamGradeAssignID] INT      IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT      NOT NULL,
    [RegistrationID]    INT      NOT NULL,
    [EducationYearID]   INT      NOT NULL,
    [ClassID]           INT      NOT NULL,
    [ExamID]            INT      NOT NULL,
    [GradeNameID]       INT      NOT NULL,
    [Insert_Date]       DATETIME NOT NULL,
    CONSTRAINT [PK_Exam_Grading_Assign] PRIMARY KEY CLUSTERED ([ExamGradeAssignID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Grading_System]...';


GO
CREATE TABLE [dbo].[Exam_Grading_System] (
    [GradingID]       INT           IDENTITY (1, 1) NOT NULL,
    [RegistrationID]  INT           NULL,
    [SchoolID]        INT           NOT NULL,
    [EducationYearID] INT           NULL,
    [Grades]          NVARCHAR (50) NULL,
    [MaxPercentage]   INT           NULL,
    [MinPercentage]   INT           NULL,
    [Comments]        NVARCHAR (50) NULL,
    [Point]           FLOAT (53)    NULL,
    [GradeNameID]     INT           NULL,
    CONSTRAINT [PK_GradingSystem] PRIMARY KEY CLUSTERED ([GradingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Name]...';


GO
CREATE TABLE [dbo].[Exam_Name] (
    [ExamID]           INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT            NOT NULL,
    [RegistrationID]   INT            NOT NULL,
    [EducationYearID]  INT            NULL,
    [ExamName]         NVARCHAR (128) NULL,
    [Period_StartDate] DATE           NULL,
    [Period_EndDate]   DATE           NULL,
    [Date]             DATETIME       NULL,
    CONSTRAINT [PK_Exam] PRIMARY KEY CLUSTERED ([ExamID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Obtain_Marks]...';


GO
CREATE TABLE [dbo].[Exam_Obtain_Marks] (
    [ObtainMarksID]      INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]           INT           NOT NULL,
    [RegistrationID]     INT           NOT NULL,
    [StudentID]          INT           NOT NULL,
    [SubjectID]          INT           NOT NULL,
    [ClassID]            INT           NOT NULL,
    [ExamID]             INT           NOT NULL,
    [SubExamID]          INT           NULL,
    [StudentClassID]     INT           NULL,
    [EducationYearID]    INT           NULL,
    [StudentRecordID]    INT           NULL,
    [StudentResultID]    INT           NULL,
    [GradingID]          INT           NULL,
    [MarksObtained]      FLOAT (53)    NULL,
    [AddPercentage]      FLOAT (53)    NULL,
    [AbsenceStatus]      NVARCHAR (50) NULL,
    [FullMark]           FLOAT (53)    NULL,
    [PassMark]           FLOAT (53)    NULL,
    [ObtainedPercentage] FLOAT (53)    NULL,
    [PassStatus]         AS            (CASE WHEN [MarksObtained] >= [PassMark] THEN 'P' ELSE 'F' END) PERSISTED NOT NULL,
    [PassPercentage]     FLOAT (53)    NULL,
    [ObtainedGrades]     NVARCHAR (50) NULL,
    [ObtainedPoint]      FLOAT (53)    NULL,
    [Date]               DATE          NULL,
    CONSTRAINT [PK_Exam_Obtain_Marks] PRIMARY KEY CLUSTERED ([ObtainMarksID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Show]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Show]
    ON [dbo].[Exam_Obtain_Marks]([StudentResultID] ASC)
    INCLUDE([StudentID], [SubjectID], [ClassID], [ExamID], [SubExamID], [MarksObtained], [AbsenceStatus], [FullMark], [PassMark], [ObtainedGrades], [ObtainedPoint]);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Result_P3]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Result_P3]
    ON [dbo].[Exam_Obtain_Marks]([AbsenceStatus] ASC)
    INCLUDE([SubjectID], [StudentResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Result_P2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Result_P2]
    ON [dbo].[Exam_Obtain_Marks]([SchoolID] ASC, [SubjectID] ASC, [ClassID] ASC, [ExamID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentResultID], [MarksObtained], [AddPercentage], [FullMark]);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Result_P]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Result_P]
    ON [dbo].[Exam_Obtain_Marks]([SchoolID] ASC, [ClassID] ASC, [ExamID] ASC, [EducationYearID] ASC)
    INCLUDE([SubjectID], [StudentResultID], [MarksObtained], [AddPercentage], [FullMark]);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Result_P4]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Result_P4]
    ON [dbo].[Exam_Obtain_Marks]([SubjectID] ASC, [StudentResultID] ASC, [AbsenceStatus] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_ExamObtainMarks_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamObtainMarks_Performance]
    ON [dbo].[Exam_Obtain_Marks]([StudentResultID] ASC, [SubjectID] ASC, [SubExamID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([MarksObtained], [FullMark], [PassMark], [AbsenceStatus]);


GO
PRINT N'Creating Index [dbo].[Exam_Obtain_Marks].[IX_Exam_Obtain_Marks_Show2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Obtain_Marks_Show2]
    ON [dbo].[Exam_Obtain_Marks]([SchoolID] ASC, [ClassID] ASC, [ExamID] ASC, [EducationYearID] ASC, [PassStatus] ASC)
    INCLUDE([StudentID], [SubjectID], [SubExamID], [MarksObtained], [AbsenceStatus], [FullMark], [PassMark]);


GO
PRINT N'Creating Table [dbo].[Exam_Publish_Setting]...';


GO
CREATE TABLE [dbo].[Exam_Publish_Setting] (
    [Publish_SettingID]                  INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]                           INT           NOT NULL,
    [RegistrationID]                     INT           NOT NULL,
    [EducationYearID]                    INT           NOT NULL,
    [ClassID]                            INT           NOT NULL,
    [ExamID]                             INT           NOT NULL,
    [IS_Fail_Enable_Optional_Subject]    BIT           NULL,
    [IS_Add_Optional_Mark_In_FullMarks]  BIT           NULL,
    [IS_Enable_Grade_as_it_is_if_Fail]   BIT           NULL,
    [IS_Enable_Fail_if_fail_in_sub_Exam] BIT           NULL,
    [Optional_Percentage_Deduction]      FLOAT (53)    NULL,
    [IS_Published]                       BIT           NULL,
    [Last_Published_Date]                DATETIME      NULL,
    [Exam_Position_Format]               NVARCHAR (50) NULL,
    [IS_Hide_Sec_Position]               BIT           NULL,
    [IS_Hide_Class_Position]             BIT           NULL,
    [Attendance_FromDate]                DATE          NULL,
    [Attendance_ToDate]                  DATE          NULL,
    [IS_Hide_FullMark]                   BIT           NULL,
    [IS_Hide_PassMark]                   BIT           NULL,
    [IS_Grade_BasePoint]                 BIT           NULL,
    [Marks_Input_Locked]                 BIT           NULL,
    [Attendance_ScheduleID]              INT           NOT NULL,
    CONSTRAINT [PK_Exam_Publish_Setting] PRIMARY KEY CLUSTERED ([Publish_SettingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Exam_Publish_Sub_Countable_Mark]...';


GO
CREATE TABLE [dbo].[Exam_Publish_Sub_Countable_Mark] (
    [Subject_Countable_MarkID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]                 INT        NOT NULL,
    [RegistrationID]           INT        NOT NULL,
    [EducationYearID]          INT        NULL,
    [SubjectID]                INT        NULL,
    [ExamID]                   INT        NULL,
    [ClassID]                  INT        NULL,
    [Countable_Mark]           FLOAT (53) NULL
);


GO
PRINT N'Creating Table [dbo].[Exam_Result_of_Student]...';


GO
CREATE TABLE [dbo].[Exam_Result_of_Student] (
    [StudentResultID]                 INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]                        INT           NOT NULL,
    [RegistrationID]                  INT           NOT NULL,
    [EducationYearID]                 INT           NOT NULL,
    [StudentID]                       INT           NOT NULL,
    [StudentClassID]                  INT           NOT NULL,
    [ClassID]                         INT           NOT NULL,
    [ExamID]                          INT           NOT NULL,
    [TotalExamFullMark_ofStudent]     FLOAT (53)    NULL,
    [TotalExamObtainedMark_ofStudent] FLOAT (53)    NULL,
    [TotalMark_ofStudent]             FLOAT (53)    NULL,
    [ObtainedMark_ofStudent]          FLOAT (53)    NULL,
    [ObtainedPercentage_ofStudent]    FLOAT (53)    NULL,
    [StudentPublishStatus]            NVARCHAR (50) NULL,
    [StudentAbsenceStatus]            NVARCHAR (50) NULL,
    [PassStatus_InSubject]            NVARCHAR (50) NULL,
    [PassStatus_Student]              NVARCHAR (50) NULL,
    [PassMark_Student]                FLOAT (53)    NULL,
    [PassPercentage_Student]          FLOAT (53)    NULL,
    [Date]                            DATE          NULL,
    [TotalSubjest_WithOptional]       INT           NULL,
    [TotalSubject]                    INT           NULL,
    [TotalPoint]                      FLOAT (53)    NULL,
    [Student_Point]                   FLOAT (53)    NULL,
    [Student_Grade]                   NVARCHAR (50) NULL,
    [HighestMark_InExam_Class]        FLOAT (53)    NULL,
    [HighestMark_InExam_Subsection]   FLOAT (53)    NULL,
    [Position_InExam_Class]           NVARCHAR (50) NULL,
    [Position_InExam_Subsection]      NVARCHAR (50) NULL,
    [Student_Comments]                NVARCHAR (50) NULL,
    [Average]                         AS            (round([ObtainedMark_ofStudent] / [TotalSubject], (2), (0))) PERSISTED,
    [NotGolden]                       BIT           NULL,
    [Publish_SettingID]               INT           NULL,
    [IsFailed]                        AS            (CASE WHEN [PassStatus_InSubject] = N'F'
                                                               OR [PassStatus_Student] = N'F' THEN (1) ELSE (0) END) PERSISTED NOT NULL,
    CONSTRAINT [PK_Exam_Result_of_Student] PRIMARY KEY CLUSTERED ([StudentResultID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_ExamResult_StudentClassBatch]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamResult_StudentClassBatch]
    ON [dbo].[Exam_Result_of_Student]([StudentResultID] ASC, [ExamID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentClassID], [ObtainedMark_ofStudent], [TotalMark_ofStudent], [Student_Grade], [Student_Point], [Average], [Position_InExam_Class], [Position_InExam_Subsection]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_Exam_Result_of_Student_Show]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Student_Show]
    ON [dbo].[Exam_Result_of_Student]([StudentClassID] ASC)
    INCLUDE([StudentResultID], [ExamID], [TotalMark_ofStudent], [ObtainedMark_ofStudent], [ObtainedPercentage_ofStudent], [StudentAbsenceStatus], [PassStatus_InSubject], [PassStatus_Student], [PassMark_Student], [Student_Point], [Student_Grade], [HighestMark_InExam_Class], [HighestMark_InExam_Subsection], [Position_InExam_Class], [Position_InExam_Subsection], [Student_Comments], [Average], [NotGolden], [Publish_SettingID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_Exam_Result_of_Student_Result_P2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Student_Result_P2]
    ON [dbo].[Exam_Result_of_Student]([SchoolID] ASC, [EducationYearID] ASC, [StudentClassID] ASC, [ExamID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_Exam_Result_of_Student_Result_P]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Student_Result_P]
    ON [dbo].[Exam_Result_of_Student]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([StudentResultID], [TotalSubject], [TotalPoint]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_Exam_Result_of_Student_Position]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Student_Position]
    ON [dbo].[Exam_Result_of_Student]([ClassID] ASC)
    INCLUDE([ExamID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Student].[IX_ExamResult_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamResult_Performance]
    ON [dbo].[Exam_Result_of_Student]([ExamID] ASC, [ClassID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentResultID], [StudentClassID], [Student_Grade], [Student_Point], [Average], [ObtainedPercentage_ofStudent], [TotalMark_ofStudent], [ObtainedMark_ofStudent], [Position_InExam_Class], [Position_InExam_Subsection]);


GO
PRINT N'Creating Table [dbo].[Exam_Result_of_Subject]...';


GO
CREATE TABLE [dbo].[Exam_Result_of_Subject] (
    [SubjectResultID]                  INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]                         INT           NOT NULL,
    [RegistrationID]                   INT           NOT NULL,
    [EducationYearID]                  INT           NOT NULL,
    [StudentID]                        INT           NOT NULL,
    [StudentClassID]                   INT           NOT NULL,
    [ClassID]                          INT           NOT NULL,
    [ExamID]                           INT           NOT NULL,
    [StudentRecordID]                  INT           NOT NULL,
    [SubjectID]                        INT           NOT NULL,
    [StudentResultID]                  INT           NOT NULL,
    [TotalExamFullMark_ofSubject]      FLOAT (53)    NULL,
    [TotalExamObtainedMark_ofSubject]  FLOAT (53)    NULL,
    [TotalMark_ofSubject]              FLOAT (53)    NULL,
    [ObtainedMark_ofSubject]           FLOAT (53)    NULL,
    [ObtainedPercentage_ofSubject]     FLOAT (53)    NULL,
    [SubjectAbsenceStatus]             NVARCHAR (50) NULL,
    [PassStatus_InSubExam]             NVARCHAR (50) NULL,
    [PassStatus_Subject]               NVARCHAR (50) NULL,
    [PassPercentage_Subject]           FLOAT (53)    NULL,
    [PassMark_Subject]                 FLOAT (53)    NULL,
    [Date]                             DATE          NULL,
    [GradingID]                        INT           NULL,
    [SubjectGrades]                    NVARCHAR (50) NULL,
    [SubjectPoint]                     FLOAT (53)    NULL,
    [OMark_ofSub_ConsiderOptional]     FLOAT (53)    NULL,
    [SubjectPoint_ConsiderOptional]    FLOAT (53)    NULL,
    [HighestMark_InSubject_Class]      FLOAT (53)    NULL,
    [HighestMark_InSubject_Subsection] FLOAT (53)    NULL,
    [Position_InSubject_Class]         NVARCHAR (50) NULL,
    [Position_InSubject_Subsection]    NVARCHAR (50) NULL,
    [SubjectType]                      NVARCHAR (10) NULL,
    [IS_Add_InExam]                    BIT           NULL,
    CONSTRAINT [PK_Exam_Result_of_Subject] PRIMARY KEY CLUSTERED ([SubjectResultID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_ExamResultSubject_BatchQuery]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamResultSubject_BatchQuery]
    ON [dbo].[Exam_Result_of_Subject]([StudentResultID] ASC, [IS_Add_InExam] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([SubjectID], [ObtainedMark_ofSubject], [TotalMark_ofSubject], [SubjectGrades], [SubjectPoint], [PassStatus_Subject], [Position_InSubject_Class], [Position_InSubject_Subsection], [HighestMark_InSubject_Class], [HighestMark_InSubject_Subsection]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Position]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Position]
    ON [dbo].[Exam_Result_of_Subject]([StudentResultID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_ExamResultSubject_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamResultSubject_Performance]
    ON [dbo].[Exam_Result_of_Subject]([StudentResultID] ASC, [SubjectID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([ObtainedMark_ofSubject], [TotalMark_ofSubject], [SubjectGrades], [SubjectPoint], [PassStatus_Subject], [IS_Add_InExam], [Position_InSubject_Class], [Position_InSubject_Subsection], [HighestMark_InSubject_Class], [HighestMark_InSubject_Subsection]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P6]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P6]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectResultID], [TotalExamFullMark_ofSubject], [TotalExamObtainedMark_ofSubject], [TotalMark_ofSubject], [PassPercentage_Subject], [OMark_ofSub_ConsiderOptional], [SubjectPoint_ConsiderOptional]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P]
    ON [dbo].[Exam_Result_of_Subject]([SubjectID] ASC, [StudentResultID] ASC)
    INCLUDE([SubjectResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Show]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Show]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectID], [SubjectGrades], [SubjectPoint]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P5]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P5]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectResultID], [StudentClassID], [SubjectID], [TotalMark_ofSubject], [ObtainedMark_ofSubject], [ObtainedPercentage_ofSubject], [SubjectPoint]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P7]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P7]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC, [SubjectType] ASC)
    INCLUDE([StudentResultID], [TotalMark_ofSubject]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P8]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P8]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [SubjectAbsenceStatus] ASC)
    INCLUDE([StudentID], [StudentClassID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Position_Sub2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Position_Sub2]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([StudentClassID], [SubjectID], [StudentResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P3]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P3]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC, [PassStatus_InSubExam] ASC, [PassStatus_Subject] ASC)
    INCLUDE([SubjectResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P4]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P4]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectResultID], [ObtainedMark_ofSubject], [ObtainedPercentage_ofSubject]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Position_Sub]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Position_Sub]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [StudentClassID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectID], [StudentResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P2]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [ClassID] ASC, [ExamID] ASC)
    INCLUDE([SubjectResultID], [SubjectID], [StudentResultID]);


GO
PRINT N'Creating Index [dbo].[Exam_Result_of_Subject].[IX_Exam_Result_of_Subject_Result_P9]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Result_of_Subject_Result_P9]
    ON [dbo].[Exam_Result_of_Subject]([SchoolID] ASC, [EducationYearID] ASC, [StudentID] ASC, [StudentClassID] ASC, [ClassID] ASC, [SubjectAbsenceStatus] ASC);


GO
PRINT N'Creating Table [dbo].[Exam_Routine_CellData]...';


GO
CREATE TABLE [dbo].[Exam_Routine_CellData] (
    [CellID]      INT            IDENTITY (1, 1) NOT NULL,
    [RoutineID]   INT            NOT NULL,
    [RowIndex]    INT            NOT NULL,
    [ColumnIndex] INT            NOT NULL,
    [SubjectID]   INT            NULL,
    [SubjectText] NVARCHAR (200) NULL,
    [StartTime]   NVARCHAR (20)  NULL,
    [EndTime]     NVARCHAR (20)  NULL,
    [Duration]    NVARCHAR (20)  NULL,
    [TimeText]    NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Exam_Routine_CellData] PRIMARY KEY CLUSTERED ([CellID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_CellData].[IX_Exam_Routine_CellData_RoutineID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Routine_CellData_RoutineID]
    ON [dbo].[Exam_Routine_CellData]([RoutineID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_CellData].[IX_ExamRoutineCellData]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamRoutineCellData]
    ON [dbo].[Exam_Routine_CellData]([RoutineID] ASC, [RowIndex] ASC, [ColumnIndex] ASC)
    INCLUDE([SubjectID], [SubjectText], [Duration]);


GO
PRINT N'Creating Table [dbo].[Exam_Routine_ClassColumns]...';


GO
CREATE TABLE [dbo].[Exam_Routine_ClassColumns] (
    [ColumnID]    INT IDENTITY (1, 1) NOT NULL,
    [RoutineID]   INT NOT NULL,
    [ColumnIndex] INT NOT NULL,
    [ClassID]     INT NOT NULL,
    CONSTRAINT [PK_Exam_Routine_ClassColumns] PRIMARY KEY CLUSTERED ([ColumnID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_ClassColumns].[IX_Exam_Routine_ClassColumns_RoutineID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Routine_ClassColumns_RoutineID]
    ON [dbo].[Exam_Routine_ClassColumns]([RoutineID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_ClassColumns].[IX_ExamRoutineClassColumns]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamRoutineClassColumns]
    ON [dbo].[Exam_Routine_ClassColumns]([ColumnID] ASC, [ClassID] ASC, [RoutineID] ASC)
    INCLUDE([ColumnIndex]);


GO
PRINT N'Creating Table [dbo].[Exam_Routine_Rows]...';


GO
CREATE TABLE [dbo].[Exam_Routine_Rows] (
    [RowID]     INT           IDENTITY (1, 1) NOT NULL,
    [RoutineID] INT           NOT NULL,
    [RowIndex]  INT           NOT NULL,
    [ExamDate]  DATE          NULL,
    [DayName]   NVARCHAR (50) NULL,
    [StartTime] NVARCHAR (20) NULL,
    [EndTime]   NVARCHAR (20) NULL,
    [Duration]  NVARCHAR (20) NULL,
    [ExamTime]  NVARCHAR (50) NULL,
    CONSTRAINT [PK_Exam_Routine_Rows] PRIMARY KEY CLUSTERED ([RowID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_Rows].[IX_ExamRoutineRows]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamRoutineRows]
    ON [dbo].[Exam_Routine_Rows]([RowID] ASC, [RoutineID] ASC, [RowIndex] ASC)
    INCLUDE([ExamDate], [DayName]);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_Rows].[IX_Exam_Routine_Rows_RoutineID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Routine_Rows_RoutineID]
    ON [dbo].[Exam_Routine_Rows]([RoutineID] ASC);


GO
PRINT N'Creating Table [dbo].[Exam_Routine_SavedData]...';


GO
CREATE TABLE [dbo].[Exam_Routine_SavedData] (
    [RoutineID]        INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT            NOT NULL,
    [RoutineName]      NVARCHAR (200) NOT NULL,
    [TitleText]        NVARCHAR (500) NULL,
    [SubtitleText]     NVARCHAR (500) NULL,
    [ExamInfoText]     NVARCHAR (500) NULL,
    [InstructionText]  NVARCHAR (MAX) NULL,
    [NotesText]        NVARCHAR (MAX) NULL,
    [SignatureText]    NVARCHAR (200) NULL,
    [ClassColumnCount] INT            NOT NULL,
    [RowCount]         INT            NOT NULL,
    [CreatedDate]      DATETIME       NOT NULL,
    [ModifiedDate]     DATETIME       NULL,
    [CreatedBy]        INT            NULL,
    [IsActive]         BIT            NOT NULL,
    [EducationYearID]  INT            NULL,
    CONSTRAINT [PK_Exam_Routine_SavedData] PRIMARY KEY CLUSTERED ([RoutineID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_SavedData].[IX_Exam_Routine_SavedData_SchoolID]...';


GO
CREATE NONCLUSTERED INDEX [IX_Exam_Routine_SavedData_SchoolID]
    ON [dbo].[Exam_Routine_SavedData]([SchoolID] ASC);


GO
PRINT N'Creating Index [dbo].[Exam_Routine_SavedData].[IX_ExamRoutineSavedData]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExamRoutineSavedData]
    ON [dbo].[Exam_Routine_SavedData]([RoutineID] ASC, [RoutineName] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([CreatedDate], [IsActive]);


GO
PRINT N'Creating Table [dbo].[Exam_SubExam_Name]...';


GO
CREATE TABLE [dbo].[Exam_SubExam_Name] (
    [SubExamID]       INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT           NULL,
    [RegistrationID]  INT           NULL,
    [EducationYearID] INT           NULL,
    [SubExamName]     NVARCHAR (50) NULL,
    [Sub_ExamSN]      INT           NULL,
    CONSTRAINT [PK_Exam_SubExam_Name] PRIMARY KEY CLUSTERED ([SubExamID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Expenditure]...';


GO
CREATE TABLE [dbo].[Expenditure] (
    [ExpenseID]            INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT            NULL,
    [EducationYearID]      INT            NULL,
    [RegistrationID]       INT            NOT NULL,
    [ExpenseCategoryID]    INT            NOT NULL,
    [Amount]               FLOAT (53)     NULL,
    [ExpenseFor]           NVARCHAR (MAX) NULL,
    [ExpenseDate]          DATE           NULL,
    [AccountID]            INT            NULL,
    [Insert_Date]          DATE           NULL,
    [ExpenseSubCategoryID] INT            NULL,
    CONSTRAINT [PK_Expenditure] PRIMARY KEY CLUSTERED ([ExpenseID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Expenditure].[IX_Expenditure_SID_EduY]...';


GO
CREATE NONCLUSTERED INDEX [IX_Expenditure_SID_EduY]
    ON [dbo].[Expenditure]([SchoolID] ASC, [EducationYearID] ASC, [ExpenseDate] ASC)
    INCLUDE([ExpenseCategoryID], [Amount]);


GO
PRINT N'Creating Index [dbo].[Expenditure].[IX_Expenditure]...';


GO
CREATE NONCLUSTERED INDEX [IX_Expenditure]
    ON [dbo].[Expenditure]([SchoolID] ASC, [EducationYearID] ASC, [ExpenseDate] ASC)
    INCLUDE([Amount], [ExpenseCategoryID], [ExpenseFor]);


GO
PRINT N'Creating Table [dbo].[Expense_CategoryName]...';


GO
CREATE TABLE [dbo].[Expense_CategoryName] (
    [ExpenseCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID]    INT            NULL,
    [SchoolID]          INT            NULL,
    [CategoryName]      NVARCHAR (300) NULL,
    CONSTRAINT [PK_Expense_Category] PRIMARY KEY CLUSTERED ([ExpenseCategoryID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Expense_CategoryName].[IX_ExpenseCategoryName]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExpenseCategoryName]
    ON [dbo].[Expense_CategoryName]([SchoolID] ASC, [ExpenseCategoryID] ASC)
    INCLUDE([CategoryName]);


GO
PRINT N'Creating Table [dbo].[Expense_SubCategory]...';


GO
CREATE TABLE [dbo].[Expense_SubCategory] (
    [ExpenseSubCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [ExpenseCategoryID]    INT            NOT NULL,
    [SubCategoryName]      NVARCHAR (200) NOT NULL,
    [SchoolID]             INT            NOT NULL,
    [RegistrationID]       INT            NULL,
    PRIMARY KEY CLUSTERED ([ExpenseSubCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Extra_Income]...';


GO
CREATE TABLE [dbo].[Extra_Income] (
    [Extra_IncomeID]         INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]               INT             NOT NULL,
    [EducationYearID]        INT             NOT NULL,
    [RegistrationID]         INT             NOT NULL,
    [Extra_IncomeCategoryID] INT             NOT NULL,
    [AccountID]              INT             NULL,
    [Extra_IncomeAmount]     FLOAT (53)      NOT NULL,
    [Extra_IncomeFor]        NVARCHAR (1000) NULL,
    [Extra_IncomeDate]       DATE            NOT NULL,
    [Insert_Date]            DATETIME        NULL,
    CONSTRAINT [PK_Extra_Income] PRIMARY KEY CLUSTERED ([Extra_IncomeID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Extra_Income].[IX_ExtraIncome]...';


GO
CREATE NONCLUSTERED INDEX [IX_ExtraIncome]
    ON [dbo].[Extra_Income]([SchoolID] ASC, [EducationYearID] ASC, [Extra_IncomeDate] ASC)
    INCLUDE([Extra_IncomeAmount], [Extra_IncomeFor], [Extra_IncomeCategoryID]);


GO
PRINT N'Creating Table [dbo].[Extra_IncomeCategory]...';


GO
CREATE TABLE [dbo].[Extra_IncomeCategory] (
    [Extra_IncomeCategoryID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                  INT            NOT NULL,
    [RegistrationID]            INT            NOT NULL,
    [Extra_Income_CategoryName] NVARCHAR (128) NULL,
    [Insert_Date]               DATETIME       NULL,
    [Total_Extra_Income]        FLOAT (53)     NULL,
    CONSTRAINT [PK_Extra_IncomeCategory] PRIMARY KEY CLUSTERED ([Extra_IncomeCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Hybrid_ChangeLog]...';


GO
CREATE TABLE [dbo].[Hybrid_ChangeLog] (
    [ChangeId]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT              NOT NULL,
    [EducationYearID] INT              NULL,
    [EntityType]      NVARCHAR (64)    NOT NULL,
    [ServerId]        INT              NOT NULL,
    [LocalId]         UNIQUEIDENTIFIER NULL,
    [Operation]       NVARCHAR (16)    NOT NULL,
    [ChangedUtc]      DATETIME2 (7)    NOT NULL,
    [OriginDeviceId]  NVARCHAR (64)    NULL,
    CONSTRAINT [PK_Hybrid_ChangeLog] PRIMARY KEY CLUSTERED ([ChangeId] ASC)
);


GO
PRINT N'Creating Index [dbo].[Hybrid_ChangeLog].[IX_Hybrid_ChangeLog_School_Change]...';


GO
CREATE NONCLUSTERED INDEX [IX_Hybrid_ChangeLog_School_Change]
    ON [dbo].[Hybrid_ChangeLog]([SchoolID] ASC, [ChangeId] ASC);


GO
PRINT N'Creating Table [dbo].[Hybrid_EntityMap]...';


GO
CREATE TABLE [dbo].[Hybrid_EntityMap] (
    [LocalId]    UNIQUEIDENTIFIER NOT NULL,
    [EntityType] NVARCHAR (64)    NOT NULL,
    [ServerId]   INT              NOT NULL,
    [SchoolID]   INT              NOT NULL,
    [DeviceId]   NVARCHAR (64)    NOT NULL,
    [CreatedUtc] DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_Hybrid_EntityMap] PRIMARY KEY CLUSTERED ([LocalId] ASC),
    CONSTRAINT [UQ_Hybrid_EntityMap_Type_Server] UNIQUE NONCLUSTERED ([EntityType] ASC, [ServerId] ASC)
);


GO
PRINT N'Creating Index [dbo].[Hybrid_EntityMap].[IX_Hybrid_EntityMap_School_Type]...';


GO
CREATE NONCLUSTERED INDEX [IX_Hybrid_EntityMap_School_Type]
    ON [dbo].[Hybrid_EntityMap]([SchoolID] ASC, [EntityType] ASC);


GO
PRINT N'Creating Table [dbo].[Income_Assign_Role]...';


GO
CREATE TABLE [dbo].[Income_Assign_Role] (
    [AssignRoleID]    INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID]  INT            NOT NULL,
    [SchoolID]        INT            NULL,
    [EducationYearID] INT            NULL,
    [RoleID]          INT            NOT NULL,
    [ClassID]         INT            NULL,
    [PayFor]          NVARCHAR (128) NULL,
    [Amount]          FLOAT (53)     NULL,
    [LateFee]         FLOAT (53)     NULL,
    [StartDate]       DATE           NULL,
    [EndDate]         DATE           NULL,
    [Date]            DATETIME       NULL,
    CONSTRAINT [PK_Income_Assign_Role] PRIMARY KEY CLUSTERED ([AssignRoleID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Income_Discount_Record]...';


GO
CREATE TABLE [dbo].[Income_Discount_Record] (
    [DiscountFeeID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NULL,
    [EducationYearID] INT            NULL,
    [RegistrationID]  INT            NOT NULL,
    [StudentID]       INT            NOT NULL,
    [StudentClassID]  INT            NULL,
    [PayOrderID]      INT            NOT NULL,
    [Reason]          NVARCHAR (MAX) NULL,
    [PreviousAmount]  FLOAT (53)     NULL,
    [PostAmount]      FLOAT (53)     NULL,
    [Date]            DATE           NULL,
    CONSTRAINT [PK_Income_Discount-Student] PRIMARY KEY CLUSTERED ([DiscountFeeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Income_LateFee_Change_Record]...';


GO
CREATE TABLE [dbo].[Income_LateFee_Change_Record] (
    [LateFeeChangeID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT        NULL,
    [EducationYearID] INT        NULL,
    [StudentID]       INT        NOT NULL,
    [StudentClassID]  INT        NULL,
    [RegistrationID]  INT        NOT NULL,
    [PayOrderID]      INT        NOT NULL,
    [PreviousAmount]  FLOAT (53) NULL,
    [PostAmount]      FLOAT (53) NULL,
    [Date]            DATE       NULL,
    CONSTRAINT [PK_Income_LateFee_Change_Record] PRIMARY KEY CLUSTERED ([LateFeeChangeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Income_LateFee_Discount_Record]...';


GO
CREATE TABLE [dbo].[Income_LateFee_Discount_Record] (
    [LateFeeDiscountID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NULL,
    [EducationYearID]   INT            NULL,
    [StudentID]         INT            NOT NULL,
    [StudentClassID]    INT            NULL,
    [RegistrationID]    INT            NOT NULL,
    [PayOrderID]        INT            NOT NULL,
    [Reason]            NVARCHAR (MAX) NULL,
    [PreviousAmount]    FLOAT (53)     NULL,
    [PostAmount]        FLOAT (53)     NULL,
    [Date]              DATE           NULL,
    CONSTRAINT [PK_Income_LateFee-Discount] PRIMARY KEY CLUSTERED ([LateFeeDiscountID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Income_MoneyReceipt]...';


GO
CREATE TABLE [dbo].[Income_MoneyReceipt] (
    [MoneyReceiptID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT            NULL,
    [StudentID]        INT            NULL,
    [RegistrationID]   INT            NULL,
    [StudentClassID]   INT            NULL,
    [PaidDate]         DATETIME       NULL,
    [TotalAmount]      FLOAT (53)     NULL,
    [EducationYearID]  INT            NULL,
    [PaymentBy]        NVARCHAR (128) NULL,
    [BankName]         NVARCHAR (128) NULL,
    [BranchName]       NVARCHAR (128) NULL,
    [TransactionID]    INT            NULL,
    [MoneyReceipt_SN]  INT            NULL,
    [PrintedReceiptNo] NVARCHAR (50)  NULL,
    [CollectionDate]   DATETIME       NULL,
    CONSTRAINT [PK_Income_MoneyReceipt] PRIMARY KEY CLUSTERED ([MoneyReceiptID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Income_MoneyReceipt].[IX_IncomeMoneyReceipt]...';


GO
CREATE NONCLUSTERED INDEX [IX_IncomeMoneyReceipt]
    ON [dbo].[Income_MoneyReceipt]([StudentID] ASC, [SchoolID] ASC, [EducationYearID] ASC, [PaidDate] ASC)
    INCLUDE([MoneyReceipt_SN], [TotalAmount], [PaymentBy]);


GO
PRINT N'Creating Index [dbo].[Income_MoneyReceipt].[UQ_PrintedReceiptNo_SchoolID]...';


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_PrintedReceiptNo_SchoolID]
    ON [dbo].[Income_MoneyReceipt]([PrintedReceiptNo] ASC, [SchoolID] ASC) WHERE ([PrintedReceiptNo] IS NOT NULL);


GO
PRINT N'Creating Index [dbo].[Income_MoneyReceipt].[IX_Income_MoneyReceipt_PrintedReceiptNo]...';


GO
CREATE NONCLUSTERED INDEX [IX_Income_MoneyReceipt_PrintedReceiptNo]
    ON [dbo].[Income_MoneyReceipt]([PrintedReceiptNo] ASC) WHERE ([PrintedReceiptNo] IS NOT NULL);


GO
PRINT N'Creating Table [dbo].[Income_PaymentRecord]...';


GO
CREATE TABLE [dbo].[Income_PaymentRecord] (
    [PaymentRecordID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NULL,
    [EducationYearID] INT            NULL,
    [StudentID]       INT            NOT NULL,
    [StudentClassID]  INT            NULL,
    [RegistrationID]  INT            NOT NULL,
    [RoleID]          INT            NULL,
    [PayOrderID]      INT            NULL,
    [PaidAmount]      FLOAT (53)     NULL,
    [PayFor]          NVARCHAR (128) NULL,
    [PaidDate]        DATETIME       NULL,
    [MoneyReceiptID]  INT            NULL,
    [AccountID]       INT            NULL,
    CONSTRAINT [PK_Income_PaymentRecord] PRIMARY KEY CLUSTERED ([PaymentRecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Income_PaymentRecord].[IX_IncomePaymentRecord]...';


GO
CREATE NONCLUSTERED INDEX [IX_IncomePaymentRecord]
    ON [dbo].[Income_PaymentRecord]([StudentID] ASC, [MoneyReceiptID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([PaidAmount], [PaidDate], [PayFor], [AccountID]);


GO
PRINT N'Creating Table [dbo].[Income_PayOrder]...';


GO
CREATE TABLE [dbo].[Income_PayOrder] (
    [PayOrderID]        INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NULL,
    [RegistrationID]    INT            NULL,
    [StudentID]         INT            NULL,
    [ClassID]           INT            NULL,
    [StudentClassID]    INT            NULL,
    [AssignRoleID]      INT            NULL,
    [Amount]            FLOAT (53)     NULL,
    [PaidAmount]        FLOAT (53)     NOT NULL,
    [LateFee]           FLOAT (53)     NULL,
    [Discount]          FLOAT (53)     NULL,
    [LateFee_Discount]  FLOAT (53)     NULL,
    [RoleID]            INT            NULL,
    [PayFor]            NVARCHAR (128) NULL,
    [StartDate]         DATE           NULL,
    [EndDate]           DATE           NULL,
    [Status]            AS             (CASE WHEN [Is_LateFeeAdded] = (1) THEN CASE WHEN (((([Amount] + isnull([LateFee], (0))) - isnull([Discount], (0))) - isnull([PaidAmount], (0))) - isnull([LateFee_Discount], (0))) = (0) THEN 'Paid' ELSE 'Due' END ELSE CASE WHEN (([Amount] - isnull([Discount], (0))) - isnull([PaidAmount], (0))) = (0) THEN 'Paid' ELSE 'Due' END END) PERSISTED NOT NULL,
    [CreatedDate]       DATE           NULL,
    [EducationYearID]   INT            NULL,
    [LastPaidDate]      DATE           NULL,
    [NumberOfPayment]   INT            NULL,
    [Is_Active]         BIT            NULL,
    [Receivable_Amount] AS             (CASE WHEN [Is_LateFeeAdded] = (1) THEN ((([Amount] + isnull([LateFee], (0))) - isnull([Discount], (0))) - isnull([PaidAmount], (0))) - isnull([LateFee_Discount], (0)) ELSE ([Amount] - isnull([Discount], (0))) - isnull([PaidAmount], (0)) END) PERSISTED,
    [LateFeeCountable]  AS             (CASE WHEN [Is_LateFeeAdded] = (1) THEN isnull([LateFee], (0)) - isnull([LateFee_Discount], (0)) ELSE (0) END) PERSISTED NOT NULL,
    [Is_LateFeeAdded]   BIT            NULL,
    [Total_Discount]    AS             (CASE WHEN [Is_LateFeeAdded] = (1) THEN isnull([Discount], (0)) + isnull([LateFee_Discount], (0)) ELSE isnull([Discount], (0)) END) PERSISTED NOT NULL,
    CONSTRAINT [PK_Income_PayOrder] PRIMARY KEY CLUSTERED ([PayOrderID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Income_PayOrder].[IX_Income_PayOrder_DS]...';


GO
CREATE NONCLUSTERED INDEX [IX_Income_PayOrder_DS]
    ON [dbo].[Income_PayOrder]([SchoolID] ASC, [Status] ASC, [EndDate] ASC)
    INCLUDE([StudentID], [StudentClassID], [Amount], [PaidAmount], [LateFee], [Discount], [LateFee_Discount], [RoleID], [PayFor]);


GO
PRINT N'Creating Index [dbo].[Income_PayOrder].[IX_Income_PayOrder_DS2]...';


GO
CREATE NONCLUSTERED INDEX [IX_Income_PayOrder_DS2]
    ON [dbo].[Income_PayOrder]([SchoolID] ASC, [StudentID] ASC, [Status] ASC, [EndDate] ASC)
    INCLUDE([StudentClassID], [Amount], [PaidAmount], [LateFee], [Discount], [LateFee_Discount], [RoleID], [PayFor]);


GO
PRINT N'Creating Index [dbo].[Income_PayOrder].[IX_IncomePayOrder]...';


GO
CREATE NONCLUSTERED INDEX [IX_IncomePayOrder]
    ON [dbo].[Income_PayOrder]([StudentClassID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([PayOrderID], [CreatedDate], [RoleID]);


GO
PRINT N'Creating Table [dbo].[Income_Roles]...';


GO
CREATE TABLE [dbo].[Income_Roles] (
    [RoleID]         INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [Role]           NVARCHAR (500) NULL,
    [NumberOfPay]    INT            NULL,
    [Description]    NVARCHAR (500) NULL,
    [Date]           DATETIME       NULL,
    CONSTRAINT [PK_Income_CategoryName] PRIMARY KEY CLUSTERED ([RoleID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Institution_Reset_Progress]...';


GO
CREATE TABLE [dbo].[Institution_Reset_Progress] (
    [SchoolID]        INT            NOT NULL,
    [Mode]            VARCHAR (20)   NOT NULL,
    [EducationYearID] INT            NULL,
    [TotalRows]       BIGINT         NOT NULL,
    [DeletedRows]     BIGINT         NOT NULL,
    [Status]          NVARCHAR (20)  NOT NULL,
    [Message]         NVARCHAR (500) NULL,
    [UpdatedAt]       DATETIME2 (0)  NOT NULL,
    CONSTRAINT [PK_Institution_Reset_Progress] PRIMARY KEY CLUSTERED ([SchoolID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Join]...';


GO
CREATE TABLE [dbo].[Join] (
    [JoinID]         INT           IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT           NOT NULL,
    [SchoolID]       INT           NOT NULL,
    [ClassID]        INT           NULL,
    [SectionID]      NVARCHAR (50) NULL,
    [SubjectGroupID] NVARCHAR (50) NULL,
    [ShiftID]        NVARCHAR (50) NULL,
    CONSTRAINT [PK_Join] PRIMARY KEY CLUSTERED ([JoinID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Link_Category]...';


GO
CREATE TABLE [dbo].[Link_Category] (
    [LinkCategoryID] INT            IDENTITY (1, 1) NOT NULL,
    [Category]       NVARCHAR (128) NULL,
    [Ascending]      INT            NULL,
    CONSTRAINT [PK_Link_Category] PRIMARY KEY CLUSTERED ([LinkCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Link_Pages]...';


GO
CREATE TABLE [dbo].[Link_Pages] (
    [LinkID]         INT              IDENTITY (1, 1) NOT NULL,
    [LinkCategoryID] INT              NULL,
    [SubCategoryID]  INT              NULL,
    [RoleId]         UNIQUEIDENTIFIER NULL,
    [PageURL]        NVARCHAR (128)   NULL,
    [PageTitle]      NVARCHAR (128)   NULL,
    [Ascending]      INT              NULL,
    CONSTRAINT [PK_Link_Pages] PRIMARY KEY CLUSTERED ([LinkID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Link_SubCategory]...';


GO
CREATE TABLE [dbo].[Link_SubCategory] (
    [SubCategoryID]  INT            IDENTITY (1, 1) NOT NULL,
    [LinkCategoryID] INT            NULL,
    [SubCategory]    NVARCHAR (128) NULL,
    [Ascending]      INT            NULL,
    CONSTRAINT [PK_Link_SubCategory] PRIMARY KEY CLUSTERED ([SubCategoryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Link_Users]...';


GO
CREATE TABLE [dbo].[Link_Users] (
    [LinkUserID]     INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [LinkID]         INT            NULL,
    [UserName]       NVARCHAR (500) NULL,
    CONSTRAINT [PK_Link_Users] PRIMARY KEY CLUSTERED ([LinkUserID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Notice_Admin]...';


GO
CREATE TABLE [dbo].[Notice_Admin] (
    [AdminNoticeID]  INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT             NOT NULL,
    [Notice_Title]   NVARCHAR (500)  NULL,
    [Notice]         NVARCHAR (4000) NULL,
    [Notice_Image]   VARBINARY (MAX) NULL,
    [Show_Date]      DATE            NOT NULL,
    [End_Date]       DATE            NOT NULL,
    [Insert_Date]    DATETIME        NOT NULL,
    CONSTRAINT [PK_Notice_Admin] PRIMARY KEY CLUSTERED ([AdminNoticeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[NoticeBoard]...';


GO
CREATE TABLE [dbo].[NoticeBoard] (
    [NoticeBoardID]     INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID]    INT             NOT NULL,
    [SchoolID]          INT             NOT NULL,
    [NoticeType]        NVARCHAR (500)  NULL,
    [NoticeDiscription] NVARCHAR (500)  NULL,
    [Notice]            NVARCHAR (MAX)  NULL,
    [PDFNotice]         VARBINARY (MAX) NULL,
    [Date]              DATE            NULL,
    CONSTRAINT [PK_NoticeBoard] PRIMARY KEY CLUSTERED ([NoticeBoardID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Public_Contact_US]...';


GO
CREATE TABLE [dbo].[Public_Contact_US] (
    [ContactUsID] INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (128)  NULL,
    [Email]       NVARCHAR (100)  NULL,
    [MobileNo]    NVARCHAR (50)   NULL,
    [Subject]     NVARCHAR (128)  NULL,
    [Message]     NVARCHAR (4000) NULL,
    [Sent_Date]   DATETIME        NULL,
    [Is_Read]     BIT             NULL,
    CONSTRAINT [PK_Public_Contact_US] PRIMARY KEY CLUSTERED ([ContactUsID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Public_Support]...';


GO
CREATE TABLE [dbo].[Public_Support] (
    [SupportID]      INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT             NOT NULL,
    [RegistrationID] INT             NOT NULL,
    [SupportTitleID] INT             NULL,
    [Message]        NVARCHAR (4000) NULL,
    [Attach_File]    VARBINARY (MAX) NULL,
    [Is_Read]        BIT             NULL,
    [Sent_Date]      DATETIME        NULL,
    CONSTRAINT [PK_Public_Support] PRIMARY KEY CLUSTERED ([SupportID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Public_Support_Title]...';


GO
CREATE TABLE [dbo].[Public_Support_Title] (
    [SupportTitleID] INT            IDENTITY (1, 1) NOT NULL,
    [Support_Title]  NVARCHAR (256) NULL,
    [SN]             INT            NULL,
    CONSTRAINT [PK_Public_Support_Title] PRIMARY KEY CLUSTERED ([SupportTitleID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Public_Testimonial]...';


GO
CREATE TABLE [dbo].[Public_Testimonial] (
    [TestimonialID]    INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID]   INT             NOT NULL,
    [SchoolID]         INT             NOT NULL,
    [Testimonial_Text] NVARCHAR (4000) NOT NULL,
    [Is_Show]          BIT             NULL,
    [Show_SN]          INT             NULL,
    [Insert_Date]      DATE            NULL,
    CONSTRAINT [PK_Public_Testimonial] PRIMARY KEY CLUSTERED ([TestimonialID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Registration]...';


GO
CREATE TABLE [dbo].[Registration] (
    [RegistrationID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT            NOT NULL,
    [UserName]          NVARCHAR (500) NOT NULL,
    [Validation]        NVARCHAR (500) NOT NULL,
    [Category]          NVARCHAR (500) NOT NULL,
    [CreateDate]        DATETIME       NULL,
    [ExpireDate]        DATETIME       NULL,
    [CommitteeMemberId] INT            NULL,
    CONSTRAINT [PK_Registration] PRIMARY KEY CLUSTERED ([RegistrationID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Registration].[IX_Registration_CommitteeMemberId]...';


GO
CREATE NONCLUSTERED INDEX [IX_Registration_CommitteeMemberId]
    ON [dbo].[Registration]([CommitteeMemberId] ASC);


GO
PRINT N'Creating Table [dbo].[RoutineDay]...';


GO
CREATE TABLE [dbo].[RoutineDay] (
    [RoutineDayID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [RoutineInfoID]  INT            NULL,
    [RegistrationID] INT            NOT NULL,
    [Day]            NVARCHAR (500) NULL,
    CONSTRAINT [PK_RoutineDay] PRIMARY KEY CLUSTERED ([RoutineDayID] ASC)
);


GO
PRINT N'Creating Table [dbo].[RoutineForClass]...';


GO
CREATE TABLE [dbo].[RoutineForClass] (
    [RoutineForClassID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT           NOT NULL,
    [RegistrationID]    INT           NOT NULL,
    [RoutineInfoID]     INT           NULL,
    [RoutineTimeID]     INT           NULL,
    [SubjectID]         INT           NULL,
    [TeacherID]         INT           NULL,
    [ClassID]           INT           NULL,
    [SectionID]         NVARCHAR (50) NULL,
    [ShiftID]           INT           NULL,
    [SubjectGroupID]    NVARCHAR (50) NULL,
    [Day]               NVARCHAR (50) NULL,
    [EducationYearID]   INT           NULL,
    [Date]              DATE          NULL,
    CONSTRAINT [PK_ClassRoutine] PRIMARY KEY CLUSTERED ([RoutineForClassID] ASC)
);


GO
PRINT N'Creating Table [dbo].[RoutineInfo]...';


GO
CREATE TABLE [dbo].[RoutineInfo] (
    [RoutineInfoID]        INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT           NOT NULL,
    [RegistrationID]       INT           NOT NULL,
    [RoutineSpecification] NVARCHAR (50) NULL,
    [Date]                 DATE          NULL,
    CONSTRAINT [PK_RoutineInfo] PRIMARY KEY CLUSTERED ([RoutineInfoID] ASC)
);


GO
PRINT N'Creating Table [dbo].[RoutineTemporary]...';


GO
CREATE TABLE [dbo].[RoutineTemporary] (
    [RoutineTemporaryID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]           INT           NOT NULL,
    [RegistrationID]     INT           NOT NULL,
    [RoutinePeriod]      NVARCHAR (50) NULL,
    [StartTime]          TIME (7)      NULL,
    [EndTime]            TIME (7)      NULL,
    [Duration]           NVARCHAR (50) NULL,
    [DeleteCode]         NVARCHAR (50) NULL,
    CONSTRAINT [PK_RoutineTemporary] PRIMARY KEY CLUSTERED ([RoutineTemporaryID] ASC)
);


GO
PRINT N'Creating Table [dbo].[RoutineTime]...';


GO
CREATE TABLE [dbo].[RoutineTime] (
    [RoutineTimeID]  INT           IDENTITY (1, 1) NOT NULL,
    [RoutineInfoID]  INT           NULL,
    [SchoolID]       INT           NOT NULL,
    [RegistrationID] INT           NOT NULL,
    [RoutinePeriod]  NVARCHAR (50) NULL,
    [StartTime]      TIME (7)      NULL,
    [EndTime]        TIME (7)      NULL,
    [Duration]       NVARCHAR (50) NULL,
    [Is_OffTime]     BIT           NULL,
    CONSTRAINT [PK_RoutineTime] PRIMARY KEY CLUSTERED ([RoutineTimeID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SchoolInfo]...';


GO
CREATE TABLE [dbo].[SchoolInfo] (
    [SchoolID]               INT             IDENTITY (1, 1) NOT NULL,
    [SchoolName]             NVARCHAR (500)  NULL,
    [SchoolLogo]             VARBINARY (MAX) NULL,
    [Institution_Dialog]     NVARCHAR (256)  NULL,
    [Established]            NVARCHAR (50)   NULL,
    [Principal]              NVARCHAR (128)  NULL,
    [AcadamicStaff]          NVARCHAR (50)   NULL,
    [Students]               NVARCHAR (50)   NULL,
    [Address]                NVARCHAR (500)  NULL,
    [City]                   NVARCHAR (128)  NULL,
    [State]                  NVARCHAR (128)  NULL,
    [LocalArea]              NVARCHAR (128)  NULL,
    [PostalCode]             NVARCHAR (50)   NULL,
    [Phone]                  NVARCHAR (50)   NULL,
    [Email]                  NVARCHAR (50)   NULL,
    [Website]                NVARCHAR (128)  NULL,
    [UserName]               NVARCHAR (128)  NULL,
    [Validation]             NVARCHAR (50)   NULL,
    [Date]                   DATETIME        NULL,
    [School_SN]              INT             NULL,
    [Per_Student_Rate]       FLOAT (53)      NULL,
    [Device_SN]              INT             NULL,
    [IS_ServiceChargeActive] BIT             NULL,
    [Discount]               FLOAT (53)      NULL,
    [Fixed]                  FLOAT (53)      NULL,
    [Free_SMS]               INT             NULL,
    [Principal_Sign]         VARBINARY (MAX) NULL,
    [Teacher_Sign]           VARBINARY (MAX) NULL,
    [OnlinePaymentEnable]    INT             NOT NULL,
    [StoreId]                VARCHAR (100)   NULL,
    [SignatureKey]           VARCHAR (200)   NULL,
    [SchoolNameLogo]         VARBINARY (MAX) NULL,
    [AccessGraceUntil]       DATETIME        NULL,
    CONSTRAINT [PK_SchoolInfo] PRIMARY KEY CLUSTERED ([SchoolID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SchoolInfo_DueNoticeSettings]...';


GO
CREATE TABLE [dbo].[SchoolInfo_DueNoticeSettings] (
    [SettingID]     INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]      INT            NOT NULL,
    [IsEnabled]     BIT            NOT NULL,
    [HideUntilDate] DATETIME       NULL,
    [Reason]        NVARCHAR (500) NULL,
    [CreatedDate]   DATETIME       NOT NULL,
    [CreatedBy]     INT            NULL,
    PRIMARY KEY CLUSTERED ([SettingID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SikkhaloySetting]...';


GO
CREATE TABLE [dbo].[SikkhaloySetting] (
    [SikkhaloySettingId]  INT           IDENTITY (1, 1) NOT NULL,
    [SmsProvider]         NVARCHAR (50) NULL,
    [SmsProviderMultiple] NVARCHAR (50) NULL,
    [SmsSendInterval]     INT           NOT NULL,
    [SmsProcessingUnit]   INT           NOT NULL,
    CONSTRAINT [PK_SikkhaloySetting] PRIMARY KEY CLUSTERED ([SikkhaloySettingId] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS]...';


GO
CREATE TABLE [dbo].[SMS] (
    [SMSID]       INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]    INT           NULL,
    [SMS_Balance] INT           NULL,
    [Masking]     NVARCHAR (50) NULL,
    [Date]        DATE          NULL,
    CONSTRAINT [PK_SMS] PRIMARY KEY CLUSTERED ([SMSID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_Group_Name]...';


GO
CREATE TABLE [dbo].[SMS_Group_Name] (
    [SMS_GroupID]    INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationID] INT            NULL,
    [SchoolID]       INT            NULL,
    [GroupName]      NVARCHAR (256) NULL,
    CONSTRAINT [PK_SMS_Group_Name] PRIMARY KEY CLUSTERED ([SMS_GroupID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_Group_Phone_Number]...';


GO
CREATE TABLE [dbo].[SMS_Group_Phone_Number] (
    [SMS_NumberID]   INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NOT NULL,
    [SMS_GroupID]    INT            NOT NULL,
    [Name]           NVARCHAR (256) NULL,
    [MobileNo]       NVARCHAR (60)  NULL,
    [Add_Date]       DATE           NULL,
    [Address]        NVARCHAR (256) NULL,
    CONSTRAINT [PK_SMS_Group_Phone_Number] PRIMARY KEY CLUSTERED ([SMS_NumberID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_OtherInfo]...';


GO
CREATE TABLE [dbo].[SMS_OtherInfo] (
    [SMS_Send_ID]       UNIQUEIDENTIFIER NOT NULL,
    [SchoolID]          INT              NULL,
    [StudentID]         INT              NULL,
    [TeacherID]         INT              NULL,
    [EducationYearID]   INT              NULL,
    [SMS_NumberID]      INT              NULL,
    [CommitteeMemberId] INT              NULL,
    CONSTRAINT [PK_SMS_OtherInfo] PRIMARY KEY CLUSTERED ([SMS_Send_ID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_Recharge_Record]...';


GO
CREATE TABLE [dbo].[SMS_Recharge_Record] (
    [SMS_Recharge_RecordID] INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]              INT        NULL,
    [RechargeSMS]           INT        NULL,
    [PerSMS_Price]          FLOAT (53) NULL,
    [Total_Price]           AS         ([RechargeSMS] * [PerSMS_Price]),
    [Date]                  DATE       NULL,
    [Is_Paid]               BIT        NULL,
    [RegistrationID]        INT        NULL,
    CONSTRAINT [PK_SMS_Recharge_Record] PRIMARY KEY CLUSTERED ([SMS_Recharge_RecordID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_Send_Record]...';


GO
CREATE TABLE [dbo].[SMS_Send_Record] (
    [SMS_Send_ID]  UNIQUEIDENTIFIER NOT NULL,
    [PhoneNumber]  NVARCHAR (50)    NULL,
    [TextSMS]      NVARCHAR (MAX)   NULL,
    [TextCount]    FLOAT (53)       NULL,
    [SMSCount]     FLOAT (53)       NULL,
    [PurposeOfSMS] NVARCHAR (MAX)   NULL,
    [SMS_Response] NVARCHAR (50)    NULL,
    [Status]       NVARCHAR (50)    NULL,
    [Date]         DATETIME         NULL,
    CONSTRAINT [PK_SMS_Send-Record] PRIMARY KEY CLUSTERED ([SMS_Send_ID] ASC)
);


GO
PRINT N'Creating Table [dbo].[SMS_Template]...';


GO
CREATE TABLE [dbo].[SMS_Template] (
    [TemplateID]       INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT            NOT NULL,
    [TemplateName]     NVARCHAR (100) NOT NULL,
    [TemplateCategory] NVARCHAR (50)  NOT NULL,
    [TemplateType]     NVARCHAR (50)  NOT NULL,
    [MessageTemplate]  NVARCHAR (MAX) NOT NULL,
    [IsActive]         BIT            NULL,
    [CreatedDate]      DATETIME       NULL,
    [UpdatedDate]      DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([TemplateID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Staff_Info]...';


GO
CREATE TABLE [dbo].[Staff_Info] (
    [StaffID]                INT             IDENTITY (1, 1) NOT NULL,
    [EmployeeID]             INT             NULL,
    [SchoolID]               INT             NULL,
    [RegistrationID]         INT             NULL,
    [FirstName]              NVARCHAR (128)  NULL,
    [LastName]               NVARCHAR (128)  NULL,
    [Gender]                 NVARCHAR (128)  NULL,
    [FatherName]             NVARCHAR (128)  NULL,
    [Designation]            NVARCHAR (128)  NULL,
    [DateofBirth]            DATE            NULL,
    [Religion]               NVARCHAR (50)   NULL,
    [NationalIDorPassportNO] NVARCHAR (50)   NULL,
    [Address]                NVARCHAR (500)  NULL,
    [Phone]                  NVARCHAR (50)   NULL,
    [Image]                  VARBINARY (MAX) NULL,
    [Date]                   DATETIME        NULL,
    [Staff_SN]               INT             NULL,
    CONSTRAINT [PK_Staff_Info] PRIMARY KEY CLUSTERED ([StaffID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Student]...';


GO
CREATE TABLE [dbo].[Student] (
    [StudentID]                       INT             IDENTITY (1, 1) NOT NULL,
    [RegistrationID]                  INT             NOT NULL,
    [SchoolID]                        INT             NOT NULL,
    [StudentRegistrationID]           INT             NULL,
    [StudentImageID]                  INT             NULL,
    [ID]                              NVARCHAR (50)   NOT NULL,
    [RFID]                            NVARCHAR (50)   NULL,
    [SMSPhoneNo]                      NVARCHAR (50)   NOT NULL,
    [StudentsName]                    NVARCHAR (50)   NOT NULL,
    [StudentEmailAddress]             NVARCHAR (50)   NULL,
    [Gender]                          NVARCHAR (50)   NULL,
    [DateofBirth]                     DATE            NULL,
    [Legal_Identity]                  NVARCHAR (50)   NULL,
    [BloodGroup]                      NVARCHAR (50)   NULL,
    [Religion]                        NVARCHAR (50)   NULL,
    [StudentPermanentAddress]         NVARCHAR (1000) NULL,
    [StudentsLocalAddress]            NVARCHAR (1000) NULL,
    [PrevSchoolName]                  NVARCHAR (128)  NULL,
    [PrevClass]                       NVARCHAR (50)   NULL,
    [PrevExamYear]                    NVARCHAR (50)   NULL,
    [PrevExamGrade]                   NVARCHAR (50)   NULL,
    [MothersName]                     NVARCHAR (50)   NULL,
    [MotherOccupation]                NVARCHAR (50)   NULL,
    [MotherPhoneNumber]               NVARCHAR (50)   NULL,
    [FathersName]                     NVARCHAR (50)   NULL,
    [FatherOccupation]                NVARCHAR (50)   NULL,
    [FatherPhoneNumber]               NVARCHAR (50)   NULL,
    [GuardianName]                    NVARCHAR (500)  NULL,
    [GuardianRelationshipwithStudent] NVARCHAR (50)   NULL,
    [GuardianPhoneNumber]             NVARCHAR (50)   NULL,
    [Status]                          NVARCHAR (50)   NULL,
    [OtherDetails]                    NVARCHAR (2000) NULL,
    [AdmissionDate]                   DATE            NULL,
    [RejectedDate]                    DATE            NULL,
    [DeviceID]                        INT             NULL,
    [ActiveDays]                      INT             NULL,
    [ActiveDate]                      DATE            NULL,
    [ActiveTime]                      DATETIME        NULL,
    [DeactivateTime]                  DATETIME        NULL,
    CONSTRAINT [PK_Student] PRIMARY KEY CLUSTERED ([StudentID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Student].[IX_Student_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_Student_Performance]
    ON [dbo].[Student]([StudentID] ASC, [ID] ASC)
    INCLUDE([StudentsName], [StudentImageID]);


GO
PRINT N'Creating Index [dbo].[Student].[IX_Student_ID_Sta]...';


GO
CREATE NONCLUSTERED INDEX [IX_Student_ID_Sta]
    ON [dbo].[Student]([ID] ASC, [Status] ASC);


GO
PRINT N'Creating Table [dbo].[Student_Act_Deactivate_Log]...';


GO
CREATE TABLE [dbo].[Student_Act_Deactivate_Log] (
    [DeactivateLogID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT           NOT NULL,
    [RegistrationID]  INT           NOT NULL,
    [StudentClassID]  INT           NULL,
    [StudentID]       INT           NULL,
    [Status]          NVARCHAR (50) NULL,
    [Act_Deact_Time]  DATETIME      NULL,
    [InsertTime]      DATETIME      NULL,
    [InsertDate]      DATE          NULL,
    CONSTRAINT [PK_Student_Act_Deactivate_Log] PRIMARY KEY CLUSTERED ([DeactivateLogID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Student_Fault]...';


GO
CREATE TABLE [dbo].[Student_Fault] (
    [StudentFaultID]  INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT             NOT NULL,
    [RegistrationID]  INT             NOT NULL,
    [EducationYearID] INT             NULL,
    [StudentID]       INT             NULL,
    [StudentClassID]  INT             NOT NULL,
    [ClassID]         INT             NULL,
    [Fault_Title]     NVARCHAR (256)  NULL,
    [Fault]           NVARCHAR (1000) NULL,
    [Fault_Date]      DATE            NULL,
    [InsertDate]      DATE            NULL,
    CONSTRAINT [PK_Student_Fault] PRIMARY KEY CLUSTERED ([StudentFaultID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Student_Image]...';


GO
CREATE TABLE [dbo].[Student_Image] (
    [StudentImageID] INT             IDENTITY (1, 1) NOT NULL,
    [Image]          VARBINARY (MAX) NULL,
    [Guardian_Photo] VARBINARY (MAX) NULL,
    CONSTRAINT [PK_Student_Image] PRIMARY KEY CLUSTERED ([StudentImageID] ASC)
);


GO
PRINT N'Creating Table [dbo].[StudentNotice]...';


GO
CREATE TABLE [dbo].[StudentNotice] (
    [StudentNoticeId] INT            IDENTITY (1, 1) NOT NULL,
    [RegistrationId]  INT            NOT NULL,
    [SchoolId]        INT            NOT NULL,
    [EducationYearId] INT            NOT NULL,
    [NoticeTitle]     NVARCHAR (500) NOT NULL,
    [Notice]          NVARCHAR (MAX) NOT NULL,
    [InsertDate]      DATETIME       NOT NULL,
    [IsHomeWork]      BIT            NOT NULL,
    [Notice_file]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_StudentNotice] PRIMARY KEY CLUSTERED ([StudentNoticeId] ASC)
);


GO
PRINT N'Creating Table [dbo].[StudentNoticeClass]...';


GO
CREATE TABLE [dbo].[StudentNoticeClass] (
    [StudentNoticeClassId] INT IDENTITY (1, 1) NOT NULL,
    [StudentNoticeId]      INT NOT NULL,
    [ClassId]              INT NOT NULL,
    CONSTRAINT [PK_StudentNoticeClass] PRIMARY KEY CLUSTERED ([StudentNoticeClassId] ASC)
);


GO
PRINT N'Creating Table [dbo].[StudentRecord]...';


GO
CREATE TABLE [dbo].[StudentRecord] (
    [StudentRecordID] INT           IDENTITY (1, 1) NOT NULL,
    [StudentID]       INT           NULL,
    [RegistrationID]  INT           NOT NULL,
    [SchoolID]        INT           NOT NULL,
    [StudentClassID]  INT           NULL,
    [SubjectID]       INT           NULL,
    [EducationYearID] INT           NULL,
    [SubjectType]     NVARCHAR (50) NULL,
    [Date]            DATE          NOT NULL,
    CONSTRAINT [PK_StudentRecord] PRIMARY KEY CLUSTERED ([StudentRecordID] ASC)
);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Sub_Update2]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Sub_Update2]
    ON [dbo].[StudentRecord]([StudentID] ASC, [SchoolID] ASC, [SubjectID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentRecordID]);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Result_P]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Result_P]
    ON [dbo].[StudentRecord]([SchoolID] ASC, [EducationYearID] ASC, [SubjectType] ASC)
    INCLUDE([StudentClassID], [SubjectID]);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Lookup]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Lookup]
    ON [dbo].[StudentRecord]([StudentID] ASC, [SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentRecordID], [SubjectID], [SubjectType]);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Sub_Update]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Sub_Update]
    ON [dbo].[StudentRecord]([SchoolID] ASC, [EducationYearID] ASC)
    INCLUDE([StudentRecordID], [StudentID], [SubjectID]);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Sub]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Sub]
    ON [dbo].[StudentRecord]([StudentClassID] ASC)
    INCLUDE([SubjectID]);


GO
PRINT N'Creating Index [dbo].[StudentRecord].[IX_StudentRecord_Result_P2]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentRecord_Result_P2]
    ON [dbo].[StudentRecord]([SchoolID] ASC, [StudentClassID] ASC, [SubjectID] ASC, [EducationYearID] ASC, [SubjectType] ASC);


GO
PRINT N'Creating Table [dbo].[StudentsClass]...';


GO
CREATE TABLE [dbo].[StudentsClass] (
    [StudentClassID]          INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]                INT            NOT NULL,
    [RegistrationID]          INT            NOT NULL,
    [StudentID]               INT            NULL,
    [ClassID]                 INT            NULL,
    [SectionID]               NVARCHAR (50)  NULL,
    [ShiftID]                 NVARCHAR (50)  NULL,
    [SubjectGroupID]          NVARCHAR (50)  NULL,
    [RollNo]                  NVARCHAR (100) NULL,
    [EducationYearID]         INT            NULL,
    [Date]                    DATE           NULL,
    [New_StudentClassID]      INT            NULL,
    [Promotion_Demotion_Year] NVARCHAR (50)  NULL,
    [Class_Status]            NVARCHAR (50)  NULL,
    [Is_New]                  BIT            NULL,
    [SeatNo]                  NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Class] PRIMARY KEY CLUSTERED ([StudentClassID] ASC)
);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_Performance]
    ON [dbo].[StudentsClass]([ClassID] ASC, [SectionID] ASC, [ShiftID] ASC, [SubjectGroupID] ASC, [StudentID] ASC)
    INCLUDE([StudentClassID], [RollNo]);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_Show2]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_Show2]
    ON [dbo].[StudentsClass]([SchoolID] ASC, [ClassID] ASC, [EducationYearID] ASC);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_SeatNo]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_SeatNo]
    ON [dbo].[StudentsClass]([SeatNo] ASC)
    INCLUDE([StudentClassID], [StudentID], [ClassID]);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_Show]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_Show]
    ON [dbo].[StudentsClass]([SectionID] ASC, [ShiftID] ASC, [SubjectGroupID] ASC)
    INCLUDE([StudentClassID], [RollNo]);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_Position_Sub]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_Position_Sub]
    ON [dbo].[StudentsClass]([SubjectGroupID] ASC)
    INCLUDE([StudentClassID]);


GO
PRINT N'Creating Index [dbo].[StudentsClass].[IX_StudentsClass_BatchLookup]...';


GO
CREATE NONCLUSTERED INDEX [IX_StudentsClass_BatchLookup]
    ON [dbo].[StudentsClass]([StudentClassID] ASC, [ClassID] ASC, [SubjectGroupID] ASC, [SectionID] ASC, [ShiftID] ASC)
    INCLUDE([StudentID], [RollNo]);


GO
PRINT N'Creating Table [dbo].[Subject]...';


GO
CREATE TABLE [dbo].[Subject] (
    [SubjectID]      INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NOT NULL,
    [RegistrationID] INT            NOT NULL,
    [SubjectName]    NVARCHAR (128) NULL,
    [Date]           DATETIME       NULL,
    [SN]             INT            NULL,
    CONSTRAINT [PK_Subject] PRIMARY KEY CLUSTERED ([SubjectID] ASC)
);


GO
PRINT N'Creating Index [dbo].[Subject].[IX_Subject_Performance]...';


GO
CREATE NONCLUSTERED INDEX [IX_Subject_Performance]
    ON [dbo].[Subject]([SubjectID] ASC, [SN] ASC)
    INCLUDE([SubjectName]);


GO
PRINT N'Creating Index [dbo].[Subject].[IX_Subject_BatchLoad]...';


GO
CREATE NONCLUSTERED INDEX [IX_Subject_BatchLoad]
    ON [dbo].[Subject]([SubjectID] ASC, [SN] ASC)
    INCLUDE([SubjectName]);


GO
PRINT N'Creating Table [dbo].[SubjectForGroup]...';


GO
CREATE TABLE [dbo].[SubjectForGroup] (
    [SubjectForGroupID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT           NOT NULL,
    [RegistrationID]    INT           NOT NULL,
    [ClassID]           INT           NOT NULL,
    [SubjectID]         INT           NULL,
    [SubjectGroupID]    NVARCHAR (50) NULL,
    [SubjectType]       NVARCHAR (50) NULL,
    [Date]              DATETIME      NULL,
    CONSTRAINT [PK_SubjectForGroup] PRIMARY KEY CLUSTERED ([SubjectForGroupID] ASC)
);


GO
PRINT N'Creating Index [dbo].[SubjectForGroup].[IX_SubjectForGroup]...';


GO
CREATE NONCLUSTERED INDEX [IX_SubjectForGroup]
    ON [dbo].[SubjectForGroup]([ClassID] ASC, [SubjectGroupID] ASC);


GO
PRINT N'Creating Table [dbo].[Teacher]...';


GO
CREATE TABLE [dbo].[Teacher] (
    [TeacherID]              INT             IDENTITY (1, 1) NOT NULL,
    [TeacherRegistrationID]  INT             NULL,
    [RegistrationID]         INT             NOT NULL,
    [SchoolID]               INT             NOT NULL,
    [Designation]            NVARCHAR (128)  NULL,
    [FirstName]              NVARCHAR (128)  NULL,
    [LastName]               NVARCHAR (128)  NULL,
    [FatherName]             NVARCHAR (128)  NULL,
    [MothersName]            NVARCHAR (128)  NULL,
    [Gender]                 NVARCHAR (128)  NULL,
    [Age]                    NVARCHAR (128)  NULL,
    [DateofBirth]            NVARCHAR (50)   NULL,
    [Religion]               NVARCHAR (50)   NULL,
    [Nationality]            NVARCHAR (50)   NULL,
    [NationalIDorPassportNO] NVARCHAR (50)   NULL,
    [Address]                NVARCHAR (500)  NULL,
    [PermanentAddress]       NVARCHAR (256)  NULL,
    [City]                   NVARCHAR (50)   NULL,
    [PostalCode]             NVARCHAR (50)   NULL,
    [State]                  NVARCHAR (50)   NULL,
    [Phone]                  NVARCHAR (50)   NULL,
    [Email]                  NVARCHAR (50)   NULL,
    [Date]                   DATETIME        NULL,
    [Image]                  VARBINARY (MAX) NULL,
    [EmployeeID]             INT             NULL,
    [T_SN]                   INT             NULL,
    CONSTRAINT [PK_Teacher] PRIMARY KEY CLUSTERED ([TeacherID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_Achievements]...';


GO
CREATE TABLE [dbo].[Teacher_Achievements] (
    [TeacherAchievementID] INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]             INT             NULL,
    [TeacherID]            INT             NULL,
    [Achievements]         NVARCHAR (1000) NULL,
    CONSTRAINT [PK_Teacher_Achievements] PRIMARY KEY CLUSTERED ([TeacherAchievementID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_Additional]...';


GO
CREATE TABLE [dbo].[Teacher_Additional] (
    [TeacherAdditionalID] INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]            INT             NULL,
    [TeacherID]           INT             NULL,
    [AboutAdditional]     NVARCHAR (1000) NULL,
    CONSTRAINT [PK_Teacher_Additional] PRIMARY KEY CLUSTERED ([TeacherAdditionalID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_Career_Objective]...';


GO
CREATE TABLE [dbo].[Teacher_Career_Objective] (
    [CareerObjectiveID] INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT             NULL,
    [TeacherID]         INT             NULL,
    [CareerObjective]   NVARCHAR (1000) NULL,
    CONSTRAINT [PK_Teacher_Career_Objective] PRIMARY KEY CLUSTERED ([CareerObjectiveID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_EducationInfo]...';


GO
CREATE TABLE [dbo].[Teacher_EducationInfo] (
    [TeacherEducationID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]           INT           NULL,
    [TeacherID]          INT           NULL,
    [InstitutionName]    NVARCHAR (50) NULL,
    [ExamYear]           NVARCHAR (50) NULL,
    [ExamName]           NVARCHAR (50) NULL,
    [Result]             NVARCHAR (50) NULL,
    CONSTRAINT [PK_Teacher_EducationInfo] PRIMARY KEY CLUSTERED ([TeacherEducationID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_JobInfo]...';


GO
CREATE TABLE [dbo].[Teacher_JobInfo] (
    [TeacherJobID]    INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT            NULL,
    [TeacherID]       INT            NULL,
    [InstitutionName] NVARCHAR (200) NULL,
    [Position]        NVARCHAR (100) NULL,
    [Responsibilitie] NVARCHAR (500) NULL,
    [JobType]         NVARCHAR (50)  NULL,
    [JobStatus]       NVARCHAR (50)  NULL,
    [Address]         NVARCHAR (500) NULL,
    [Phone]           NVARCHAR (50)  NULL,
    [Email]           NVARCHAR (120) NULL,
    [JobYear]         NVARCHAR (100) NULL,
    CONSTRAINT [PK_Teacher_JobInfo] PRIMARY KEY CLUSTERED ([TeacherJobID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_Language]...';


GO
CREATE TABLE [dbo].[Teacher_Language] (
    [TeacherLanguageID] INT           IDENTITY (1, 1) NOT NULL,
    [SchoolID]          INT           NULL,
    [TeacherID]         INT           NULL,
    [LanguageName]      NVARCHAR (50) NULL,
    [Level]             NVARCHAR (50) NULL,
    CONSTRAINT [PK_Teacher_Language] PRIMARY KEY CLUSTERED ([TeacherLanguageID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Teacher_Skill]...';


GO
CREATE TABLE [dbo].[Teacher_Skill] (
    [TeacherSkillID] INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [TeacherID]      INT            NULL,
    [SkilName]       NVARCHAR (500) NULL,
    [Description]    NVARCHAR (800) NULL,
    CONSTRAINT [PK_Teacher_Skill] PRIMARY KEY CLUSTERED ([TeacherSkillID] ASC)
);


GO
PRINT N'Creating Table [dbo].[TecherSubject]...';


GO
CREATE TABLE [dbo].[TecherSubject] (
    [TecherSubjectID] INT  IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT  NOT NULL,
    [RegistrationID]  INT  NOT NULL,
    [TeacherID]       INT  NOT NULL,
    [ClassID]         INT  NULL,
    [SubjectID]       INT  NOT NULL,
    [date]            DATE NULL,
    CONSTRAINT [PK_TecherSubject] PRIMARY KEY CLUSTERED ([TecherSubjectID] ASC)
);


GO
PRINT N'Creating Table [dbo].[Temp_Online_DonationPaymentRecord]...';


GO
CREATE TABLE [dbo].[Temp_Online_DonationPaymentRecord] (
    [PaymentRecordID]     NVARCHAR (100)  NULL,
    [CommitteeMemberId]   INT             NULL,
    [CommitteeDonationId] INT             NULL,
    [PaidAmount]          DECIMAL (18, 2) NULL,
    [PaidDate]            DATETIME        NULL,
    [AccountID]           INT             NULL
);


GO
PRINT N'Creating Table [dbo].[Temp_Online_PaymentRecord]...';


GO
CREATE TABLE [dbo].[Temp_Online_PaymentRecord] (
    [PaymentRecordID]   NVARCHAR (128) NOT NULL,
    [StudentID]         INT            NOT NULL,
    [RoleID]            INT            NULL,
    [PayOrderID]        INT            NULL,
    [PayOrderEduYearID] INT            NULL,
    [PaidAmount]        FLOAT (53)     NULL,
    [PayFor]            NVARCHAR (128) NULL,
    [PaidDate]          DATETIME       NULL,
    [AccountID]         INT            NULL
);


GO
PRINT N'Creating Table [dbo].[Up_RFID]...';


GO
CREATE TABLE [dbo].[Up_RFID] (
    [Up_ID]   NVARCHAR (50) NULL,
    [Up_RFID] NVARCHAR (50) NULL,
    [KeyID]   INT           IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Up_RFID] PRIMARY KEY CLUSTERED ([KeyID] ASC)
);


GO
PRINT N'Creating Table [dbo].[User_Active_Sessions]...';


GO
CREATE TABLE [dbo].[User_Active_Sessions] (
    [SessionID]      INT            IDENTITY (1, 1) NOT NULL,
    [SchoolID]       INT            NULL,
    [RegistrationID] INT            NULL,
    [UserName]       NVARCHAR (255) NULL,
    [Category]       NVARCHAR (50)  NULL,
    [SessionKey]     NVARCHAR (500) NULL,
    [LastActivity]   DATETIME       NULL,
    [LoginTime]      DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([SessionID] ASC)
);


GO
PRINT N'Creating Table [dbo].[User_Balance_Submission]...';


GO
CREATE TABLE [dbo].[User_Balance_Submission] (
    [SubmissionID]     INT             IDENTITY (1, 1) NOT NULL,
    [SchoolID]         INT             NOT NULL,
    [RegistrationID]   INT             NOT NULL,
    [SubmissionAmount] DECIMAL (18, 2) NOT NULL,
    [SubmissionDate]   DATETIME        NOT NULL,
    [ReceivedBy]       NVARCHAR (100)  NULL,
    [ReceiverPhone]    NVARCHAR (15)   NULL,
    [PaymentMethod]    NVARCHAR (50)   NULL,
    [Remarks]          NVARCHAR (500)  NULL,
    [CreatedDate]      DATETIME        NOT NULL,
    [CreatedBy]        INT             NULL,
    CONSTRAINT [PK_User_Balance_Submission] PRIMARY KEY CLUSTERED ([SubmissionID] ASC)
);


GO
PRINT N'Creating Index [dbo].[User_Balance_Submission].[IX_User_Balance_Submission_School_User]...';


GO
CREATE NONCLUSTERED INDEX [IX_User_Balance_Submission_School_User]
    ON [dbo].[User_Balance_Submission]([SchoolID] ASC, [RegistrationID] ASC)
    INCLUDE([SubmissionAmount], [SubmissionDate], [CreatedDate]);


GO
PRINT N'Creating Table [dbo].[WeeklyExam]...';


GO
CREATE TABLE [dbo].[WeeklyExam] (
    [WeeklyExamID]    INT        IDENTITY (1, 1) NOT NULL,
    [SchoolID]        INT        NOT NULL,
    [RegistrationID]  INT        NOT NULL,
    [StudentID]       INT        NOT NULL,
    [ClassID]         INT        NULL,
    [ExamID]          INT        NULL,
    [StudentClassID]  INT        NULL,
    [MarksObtained]   FLOAT (53) NULL,
    [ExamDate]        DATE       NULL,
    [EducationYearID] INT        NULL,
    [Date]            DATE       NULL,
    CONSTRAINT [PK_WeeklyExam] PRIMARY KEY CLUSTERED ([WeeklyExamID] ASC)
);


GO
PRINT N'Creating Index [dbo].[WeeklyExam].[IX_WeeklyExam_Date]...';


GO
CREATE NONCLUSTERED INDEX [IX_WeeklyExam_Date]
    ON [dbo].[WeeklyExam]([SchoolID] ASC, [ExamID] ASC, [EducationYearID] ASC)
    INCLUDE([ExamDate]);


GO
PRINT N'Creating Table [dbo].[WordOfTheDay]...';


GO
CREATE TABLE [dbo].[WordOfTheDay] (
    [WordID]          INT            IDENTITY (1, 1) NOT NULL,
    [EnglishWord]     NVARCHAR (100) NOT NULL,
    [BengaliMeaning]  NVARCHAR (200) NOT NULL,
    [PartOfSpeech]    NVARCHAR (50)  NOT NULL,
    [ExampleSentence] NVARCHAR (500) NOT NULL,
    [Pronunciation]   NVARCHAR (100) NULL,
    [CreatedDate]     DATETIME       NOT NULL,
    [IsActive]        BIT            NOT NULL,
    CONSTRAINT [PK_WordOfTheDay] PRIMARY KEY CLUSTERED ([WordID] ASC)
);


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Auto_Process_Log]...';


GO
ALTER TABLE [dbo].[AAP_Auto_Process_Log]
    ADD DEFAULT (getdate()) FOR [ProcessDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_PaidAmount]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [DF_Invoice_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_TotalAmount]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [DF_Invoice_TotalAmount] DEFAULT ((0)) FOR [TotalAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Invoice_NumberOfPayment]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [DF_AAP_Invoice_NumberOfPayment] DEFAULT ((0)) FOR [NumberOfPayment];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_Discount]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [DF_Invoice_Discount] DEFAULT ((0)) FOR [Discount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_CreateDate]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [DF_Invoice_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Invoice_Category_Insert_Date]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_Category]
    ADD CONSTRAINT [DF_AAP_Invoice_Category_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Invoice_OnlinePayment]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_OnlinePayment]
    ADD DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Invoice_OnlinePayment]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_OnlinePayment]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_Payment_Record_PaidDate]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_Payment_Record]
    ADD CONSTRAINT [DF_Invoice_Payment_Record_PaidDate] DEFAULT (getdate()) FOR [PaidDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Invoice_Payment_Record_Amount]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_Payment_Record]
    ADD CONSTRAINT [DF_Invoice_Payment_Record_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Invoice_Receipt_TotalAmount]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_Receipt]
    ADD CONSTRAINT [DF_AAP_Invoice_Receipt_TotalAmount] DEFAULT ((0)) FOR [TotalAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_MoneyReceipt_SN]...';


GO
ALTER TABLE [dbo].[AAP_Invoice_Receipt]
    ADD CONSTRAINT [DF_Table_1_MoneyReceipt_SN] DEFAULT ((0)) FOR [InvoiceReceipt_SN];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_Insert_Date]...';


GO
ALTER TABLE [dbo].[AAP_Reference]
    ADD CONSTRAINT [DF_AAP_Reference_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_PaidAmount]...';


GO
ALTER TABLE [dbo].[AAP_Reference]
    ADD CONSTRAINT [DF_AAP_Reference_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_TotalAmount]...';


GO
ALTER TABLE [dbo].[AAP_Reference]
    ADD CONSTRAINT [DF_AAP_Reference_TotalAmount] DEFAULT ((0)) FOR [TotalAmount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Reference_Commission]...';


GO
ALTER TABLE [dbo].[AAP_Reference_Commission]
    ADD DEFAULT (getdate()) FOR [Created_At];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Reference_Commission]...';


GO
ALTER TABLE [dbo].[AAP_Reference_Commission]
    ADD DEFAULT (getdate()) FOR [Commission_Date];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Reference_Commission]...';


GO
ALTER TABLE [dbo].[AAP_Reference_Commission]
    ADD DEFAULT ((0)) FOR [Commission_Percentage];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Reference_Commission]...';


GO
ALTER TABLE [dbo].[AAP_Reference_Commission]
    ADD DEFAULT ((0)) FOR [Commission_Amount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[AAP_Reference_Commission]...';


GO
ALTER TABLE [dbo].[AAP_Reference_Commission]
    ADD DEFAULT ((0)) FOR [ServiceCharge_Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_PaymentRecord_PaidDate]...';


GO
ALTER TABLE [dbo].[AAP_Reference_PaymentRecord]
    ADD CONSTRAINT [DF_AAP_Reference_PaymentRecord_PaidDate] DEFAULT (getdate()) FOR [PaidDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_PaymentRecord_Amount]...';


GO
ALTER TABLE [dbo].[AAP_Reference_PaymentRecord]
    ADD CONSTRAINT [DF_AAP_Reference_PaymentRecord_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_School_InserDate]...';


GO
ALTER TABLE [dbo].[AAP_Reference_School]
    ADD CONSTRAINT [DF_AAP_Reference_School_InserDate] DEFAULT (getdate()) FOR [InserDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Student_Count_Monthly_InsertDate]...';


GO
ALTER TABLE [dbo].[AAP_Student_Count_Monthly]
    ADD CONSTRAINT [DF_AAP_Student_Count_Monthly_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Student_Count_Monthly_Month]...';


GO
ALTER TABLE [dbo].[AAP_Student_Count_Monthly]
    ADD CONSTRAINT [DF_AAP_Student_Count_Monthly_Month] DEFAULT (getdate()) FOR [Month];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_StudentClass_Count_Monthly_Month]...';


GO
ALTER TABLE [dbo].[AAP_StudentClass_Count_Monthly]
    ADD CONSTRAINT [DF_AAP_StudentClass_Count_Monthly_Month] DEFAULT (getdate()) FOR [Month];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_StudentClass_Count_Monthly_InsertDate]...';


GO
ALTER TABLE [dbo].[AAP_StudentClass_Count_Monthly]
    ADD CONSTRAINT [DF_AAP_StudentClass_Count_Monthly_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Deleted_Expense]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Deleted_Expense] DEFAULT ((0)) FOR [Deleted_Expense];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Deleted_Income]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Deleted_Income] DEFAULT ((0)) FOR [Deleted_Income];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_AccontCreateDate]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_AccontCreateDate] DEFAULT (getdate()) FOR [AccountCreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_PAY_Buttton_SMS_Enable_Disable]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_PAY_Buttton_SMS_Enable_Disable] DEFAULT ((0)) FOR [PAY_Buttton_SMS_Enable_Disable];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Total_Income]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Total_Income] DEFAULT ((0)) FOR [Total_Income];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Total_Expense]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Total_Expense] DEFAULT ((0)) FOR [Total_Expense];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Total_OUT]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Total_OUT] DEFAULT ((0)) FOR [Total_OUT];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Default_Status]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Default_Status] DEFAULT ((0)) FOR [Default_Status];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Total_IN]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [DF_Account_Total_IN] DEFAULT ((0)) FOR [Total_IN];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Account]...';


GO
ALTER TABLE [dbo].[Account]
    ADD DEFAULT ((0)) FOR [Teacher_BackDate_Attendance];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Log_Balance_Before]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [DF_Account_Log_Balance_Before] DEFAULT ((0)) FOR [Balance_Before];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Log_Insert_Date]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [DF_Account_Log_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Log_Activity_Date]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [DF_Account_Log_Activity_Date] DEFAULT (getdate()) FOR [Activity_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Log_Insert_Time]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [DF_Account_Log_Insert_Time] DEFAULT (getdate()) FOR [Insert_Time];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Account_Log_Balance_After]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [DF_Account_Log_Balance_After] DEFAULT ((0)) FOR [Balance_After];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AccountIN_Record_Insert_Date]...';


GO
ALTER TABLE [dbo].[AccountIN_Record]
    ADD CONSTRAINT [DF_AccountIN_Record_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AccountIN_Record_AccountIN_Date]...';


GO
ALTER TABLE [dbo].[AccountIN_Record]
    ADD CONSTRAINT [DF_AccountIN_Record_AccountIN_Date] DEFAULT (getdate()) FOR [AccountIN_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AccountOUT_Record_AccountOUT_Date]...';


GO
ALTER TABLE [dbo].[AccountOUT_Record]
    ADD CONSTRAINT [DF_AccountOUT_Record_AccountOUT_Date] DEFAULT (getdate()) FOR [AccountOUT_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_AccountOUT_Insert_Date]...';


GO
ALTER TABLE [dbo].[AccountOUT_Record]
    ADD CONSTRAINT [DF_AccountOUT_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Applications]...';


GO
ALTER TABLE [dbo].[aspnet_Applications]
    ADD DEFAULT (newid()) FOR [ApplicationId];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Membership]...';


GO
ALTER TABLE [dbo].[aspnet_Membership]
    ADD DEFAULT ((0)) FOR [PasswordFormat];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Paths]...';


GO
ALTER TABLE [dbo].[aspnet_Paths]
    ADD DEFAULT (newid()) FOR [PathId];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_PersonalizationPerUser]...';


GO
ALTER TABLE [dbo].[aspnet_PersonalizationPerUser]
    ADD DEFAULT (newid()) FOR [Id];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Roles]...';


GO
ALTER TABLE [dbo].[aspnet_Roles]
    ADD DEFAULT (newid()) FOR [RoleId];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Users]...';


GO
ALTER TABLE [dbo].[aspnet_Users]
    ADD DEFAULT (newid()) FOR [UserId];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Users]...';


GO
ALTER TABLE [dbo].[aspnet_Users]
    ADD DEFAULT ((0)) FOR [IsAnonymous];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[aspnet_Users]...';


GO
ALTER TABLE [dbo].[aspnet_Users]
    ADD DEFAULT (NULL) FOR [MobileAlias];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_Device_DataUpdateList]
    ADD CONSTRAINT [DF_Table_1_InsertDate] DEFAULT (getdate()) FOR [UpdateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_AttendanceEnable]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_AttendanceEnable] DEFAULT ((1)) FOR [Is_Student_Attendance_Enable];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_English_SMS]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_English_SMS] DEFAULT ((1)) FOR [Is_English_SMS];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Employee_Late_SMS_ON]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Employee_Late_SMS_ON] DEFAULT ((0)) FOR [Is_Employee_Abs_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Users_IsActive]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Users_IsActive] DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1_1]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1_1] DEFAULT ((0)) FOR [Is_Student_Late_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Employee_Late_SMS_ON1]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Employee_Late_SMS_ON1] DEFAULT ((0)) FOR [Is_Employee_Late_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1_2]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1_2] DEFAULT ((1)) FOR [Is_Employee_SMS_Active];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON] DEFAULT ((0)) FOR [Is_Student_Exit_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_All_SMS_On]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_All_SMS_On] DEFAULT ((0)) FOR [Is_All_SMS_On];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Device_Attendance_Enable]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Device_Attendance_Enable] DEFAULT ((1)) FOR [Is_Device_Attendance_Enable];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Users_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Users_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_Exit_SMS_ON1] DEFAULT ((0)) FOR [Is_Student_Abs_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_AttendanceEnable1]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_AttendanceEnable1] DEFAULT ((1)) FOR [Is_Employee_Attendance_Enable];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Holiday_As_Offday]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Holiday_As_Offday] DEFAULT ((1)) FOR [Is_Holiday_As_Offday];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_Entry]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_Entry] DEFAULT ((0)) FOR [Is_Student_Entry_SMS_ON];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Student_All_SMS_Active]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Student_All_SMS_Active] DEFAULT ((1)) FOR [Is_Student_All_SMS_Active];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_Is_Employee_SMS_OwnNumber]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_Is_Employee_SMS_OwnNumber] DEFAULT ((1)) FOR [Is_Employee_SMS_OwnNumber];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Device_Setting_SMS_TimeOut_Minute]...';


GO
ALTER TABLE [dbo].[Attendance_Device_Setting]
    ADD CONSTRAINT [DF_Attendance_Device_Setting_SMS_TimeOut_Minute] DEFAULT ((0)) FOR [SMS_TimeOut_Minute];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Leave_Type_CreatedDate]...';


GO
ALTER TABLE [dbo].[Attendance_Leave_Type]
    ADD CONSTRAINT [DF_Attendance_Leave_Type_CreatedDate] DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Leave_Type_IsActive]...';


GO
ALTER TABLE [dbo].[Attendance_Leave_Type]
    ADD CONSTRAINT [DF_Attendance_Leave_Type_IsActive] DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Leave_Type_SortOrder]...';


GO
ALTER TABLE [dbo].[Attendance_Leave_Type]
    ADD CONSTRAINT [DF_Attendance_Leave_Type_SortOrder] DEFAULT ((0)) FOR [SortOrder];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalLeave]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalLeave] DEFAULT ((0)) FOR [TotalLeave];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalBunk]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalBunk] DEFAULT ((0)) FOR [TotalBunk];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_WorkingDays]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_WorkingDays] DEFAULT ((0)) FOR [WorkingDays];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalLate]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalLate] DEFAULT ((0)) FOR [TotalLate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_Insert_Date]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalAbsent]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalAbsent] DEFAULT ((0)) FOR [TotalAbsent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalPresent]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalPresent] DEFAULT ((0)) FOR [TotalPresent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Monthly_Report_TotalLateAbs]...';


GO
ALTER TABLE [dbo].[Attendance_Monthly_Report]
    ADD CONSTRAINT [DF_Attendance_Monthly_Report_TotalLateAbs] DEFAULT ((0)) FOR [TotalLateAbs];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Record_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_Record]
    ADD CONSTRAINT [DF_Attendance_Record_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Record_IsFromDevice]...';


GO
ALTER TABLE [dbo].[Attendance_Record]
    ADD CONSTRAINT [DF_Attendance_Record_IsFromDevice] DEFAULT ((0)) FOR [IsFromDevice];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Attendance_Record]...';


GO
ALTER TABLE [dbo].[Attendance_Record]
    ADD DEFAULT ((0)) FOR [Is_OUT];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Record_Device_Insert_Date]...';


GO
ALTER TABLE [dbo].[Attendance_Record_Device]
    ADD CONSTRAINT [DF_Attendance_Record_Device_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Record_Device_ServerUp_Status]...';


GO
ALTER TABLE [dbo].[Attendance_Record_Device]
    ADD CONSTRAINT [DF_Attendance_Record_Device_ServerUp_Status] DEFAULT (N'No') FOR [ServerUp_Status];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_AssignStudent_Is_Late_SMS]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_AssignStudent]
    ADD CONSTRAINT [DF_Attendance_Schedule_AssignStudent_Is_Late_SMS] DEFAULT ((0)) FOR [Is_Late_SMS];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_AssignStudent_Entry_Confirmation]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_AssignStudent]
    ADD CONSTRAINT [DF_Attendance_Schedule_AssignStudent_Entry_Confirmation] DEFAULT ((0)) FOR [Entry_Confirmation];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_AssignStudent_Exit_Confirmation]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_AssignStudent]
    ADD CONSTRAINT [DF_Attendance_Schedule_AssignStudent_Exit_Confirmation] DEFAULT ((0)) FOR [Exit_Confirmation];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_AssignStudent_Is_Abs_SMS]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_AssignStudent]
    ADD CONSTRAINT [DF_Attendance_Schedule_AssignStudent_Is_Abs_SMS] DEFAULT ((0)) FOR [Is_Abs_SMS];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_ChangeRecord_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_ChangeRecord]
    ADD CONSTRAINT [DF_Attendance_Schedule_ChangeRecord_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_Day_Insert_Date]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_Day]
    ADD CONSTRAINT [DF_Attendance_Schedule_Day_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Schedule_Day_Is_OnDay]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_Day]
    ADD CONSTRAINT [DF_Attendance_Schedule_Day_Is_OnDay] DEFAULT ((1)) FOR [Is_OnDay];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_SMS]
    ADD CONSTRAINT [DF_Attendance_SMS_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Is_Send]...';


GO
ALTER TABLE [dbo].[Attendance_SMS]
    ADD CONSTRAINT [DF_Attendance_SMS_Is_Send] DEFAULT ((0)) FOR [Is_Send];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_CreateTime]...';


GO
ALTER TABLE [dbo].[Attendance_SMS]
    ADD CONSTRAINT [DF_Attendance_SMS_CreateTime] DEFAULT (getdate()) FOR [CreateTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Failed_CreateTime]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Failed]
    ADD CONSTRAINT [DF_Attendance_SMS_Failed_CreateTime] DEFAULT (getdate()) FOR [CreateTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Failed_InsertDate]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Failed]
    ADD CONSTRAINT [DF_Attendance_SMS_Failed_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Sender_TotalSmsFailed]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Sender]
    ADD CONSTRAINT [DF_Attendance_SMS_Sender_TotalSmsFailed] DEFAULT ((0)) FOR [TotalSmsFailed];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Sender_TotalEventCall]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Sender]
    ADD CONSTRAINT [DF_Attendance_SMS_Sender_TotalEventCall] DEFAULT ((0)) FOR [TotalEventCall];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_AppStarted]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Sender]
    ADD CONSTRAINT [DF_Table_1_AppStarted] DEFAULT (getdate()) FOR [AppStartTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_SMS_Sender_TotalSmsSend]...';


GO
ALTER TABLE [dbo].[Attendance_SMS_Sender]
    ADD CONSTRAINT [DF_Attendance_SMS_Sender_TotalSmsSend] DEFAULT ((0)) FOR [TotalSmsSend];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Attendance_Student_Insert_Date]...';


GO
ALTER TABLE [dbo].[Attendance_Student]
    ADD CONSTRAINT [DF_Attendance_Student_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Authority_Info_Insert_Date]...';


GO
ALTER TABLE [dbo].[Authority_Info]
    ADD CONSTRAINT [DF_Authority_Info_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeDonation_InsertDate]...';


GO
ALTER TABLE [dbo].[CommitteeDonation]
    ADD CONSTRAINT [DF_CommitteeDonation_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeDonation_Amount]...';


GO
ALTER TABLE [dbo].[CommitteeDonation]
    ADD CONSTRAINT [DF_CommitteeDonation_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeDonation_PaidAmount]...';


GO
ALTER TABLE [dbo].[CommitteeDonation]
    ADD CONSTRAINT [DF_CommitteeDonation_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_DonationCategory_InsertDate]...';


GO
ALTER TABLE [dbo].[CommitteeDonationCategory]
    ADD CONSTRAINT [DF_DonationCategory_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeDonationTemplate]...';


GO
ALTER TABLE [dbo].[CommitteeDonationTemplate]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMember_TotalDonation]...';


GO
ALTER TABLE [dbo].[CommitteeMember]
    ADD CONSTRAINT [DF_CommitteeMember_TotalDonation] DEFAULT ((0)) FOR [TotalDonation];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeMember]...';


GO
ALTER TABLE [dbo].[CommitteeMember]
    ADD DEFAULT ('Active') FOR [Status];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMember_InsertDate]...';


GO
ALTER TABLE [dbo].[CommitteeMember]
    ADD CONSTRAINT [DF_CommitteeMember_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMember_PaidDonation]...';


GO
ALTER TABLE [dbo].[CommitteeMember]
    ADD CONSTRAINT [DF_CommitteeMember_PaidDonation] DEFAULT ((0)) FOR [PaidDonation];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeMember_Billing]...';


GO
ALTER TABLE [dbo].[CommitteeMember_Billing]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeMember_Billing]...';


GO
ALTER TABLE [dbo].[CommitteeMember_Billing]
    ADD DEFAULT (getdate()) FOR [UpdatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeMember_Billing]...';


GO
ALTER TABLE [dbo].[CommitteeMember_Billing]
    ADD DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[CommitteeMember_Billing]...';


GO
ALTER TABLE [dbo].[CommitteeMember_Billing]
    ADD DEFAULT ((0)) FOR [IsIncluded];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMemberType_Insert_Date]...';


GO
ALTER TABLE [dbo].[CommitteeMemberType]
    ADD CONSTRAINT [DF_CommitteeMemberType_Insert_Date] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_MoneyReceipt_SN_1]...';


GO
ALTER TABLE [dbo].[CommitteeMoneyReceipt]
    ADD CONSTRAINT [DF_Table_1_MoneyReceipt_SN_1] DEFAULT ((0)) FOR [CommitteeMoneyReceiptSn];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMoneyReceipt_InsertDate]...';


GO
ALTER TABLE [dbo].[CommitteeMoneyReceipt]
    ADD CONSTRAINT [DF_CommitteeMoneyReceipt_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteeMoneyReceipt_TotalAmount]...';


GO
ALTER TABLE [dbo].[CommitteeMoneyReceipt]
    ADD CONSTRAINT [DF_CommitteeMoneyReceipt_TotalAmount] DEFAULT ((0)) FOR [TotalAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_CommitteePaymentRecord_PaidAmount]...';


GO
ALTER TABLE [dbo].[CommitteePaymentRecord]
    ADD CONSTRAINT [DF_CommitteePaymentRecord_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Device_Commands]...';


GO
ALTER TABLE [dbo].[Device_Commands]
    ADD DEFAULT ('Pending') FOR [CommandStatus];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Device_Commands]...';


GO
ALTER TABLE [dbo].[Device_Commands]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Device_Institution_Mapping]...';


GO
ALTER TABLE [dbo].[Device_Institution_Mapping]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Device_Institution_Mapping]...';


GO
ALTER TABLE [dbo].[Device_Institution_Mapping]
    ADD DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Education_Year_IsActive]...';


GO
ALTER TABLE [dbo].[Education_Year]
    ADD CONSTRAINT [DF_Education_Year_IsActive] DEFAULT ((0)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Allowance_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Allowance]
    ADD CONSTRAINT [DF_Employee_Allowance_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Allowance_Assign_AllowanceAmount]...';


GO
ALTER TABLE [dbo].[Employee_Allowance_Assign]
    ADD CONSTRAINT [DF_Employee_Allowance_Assign_AllowanceAmount] DEFAULT ((0)) FOR [AllowanceAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Allowance_Assign_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Allowance_Assign]
    ADD CONSTRAINT [DF_Employee_Allowance_Assign_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Allowance_Records_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Allowance_Records]
    ADD CONSTRAINT [DF_Employee_Allowance_Records_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Allowance_Records_AllowanceAmount]...';


GO
ALTER TABLE [dbo].[Employee_Allowance_Records]
    ADD CONSTRAINT [DF_Employee_Allowance_Records_AllowanceAmount] DEFAULT ((0)) FOR [AllowanceAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Record_IsFromDevice]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Record]
    ADD CONSTRAINT [DF_Employee_Attendance_Record_IsFromDevice] DEFAULT ((0)) FOR [IsFromDevice];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Employee_Attendance_Record]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Record]
    ADD DEFAULT ((0)) FOR [Is_OUT];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Record_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Record]
    ADD CONSTRAINT [DF_Employee_Attendance_Record_CreateDate] DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_Total_Absent]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_Total_Absent] DEFAULT ((0)) FOR [Total_Absent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_Total_Late]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_Total_Late] DEFAULT ((0)) FOR [Total_Late];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_Total_Present]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_Total_Present] DEFAULT ((0)) FOR [Total_Present];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_Total_Leave]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_Total_Leave] DEFAULT ((0)) FOR [Total_Leave];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Attendance_Report_Total_WorkingDays]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Report]
    ADD CONSTRAINT [DF_Employee_Attendance_Report_Total_WorkingDays] DEFAULT ((0)) FOR [Total_WorkingDays];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Schedule_Assign_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Attendance_Schedule_Assign]
    ADD CONSTRAINT [DF_Employee_Schedule_Assign_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Bonus_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Bonus]
    ADD CONSTRAINT [DF_Employee_Bonus_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Bonus_Records_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Bonus_Records]
    ADD CONSTRAINT [DF_Employee_Bonus_Records_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Deduction_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Deduction]
    ADD CONSTRAINT [DF_Employee_Deduction_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_AllowanceAmount]...';


GO
ALTER TABLE [dbo].[Employee_Deduction_Assign]
    ADD CONSTRAINT [DF_Table_1_AllowanceAmount] DEFAULT ((0)) FOR [DeductionAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Deduction_Assign_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Deduction_Assign]
    ADD CONSTRAINT [DF_Employee_Deduction_Assign_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Deduction_Records_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Deduction_Records]
    ADD CONSTRAINT [DF_Employee_Deduction_Records_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Fine_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Fine]
    ADD CONSTRAINT [DF_Employee_Fine_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Fine_Records_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Fine_Records]
    ADD CONSTRAINT [DF_Employee_Fine_Records_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Holiday_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Holiday]
    ADD CONSTRAINT [DF_Employee_Holiday_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Info_Salary]...';


GO
ALTER TABLE [dbo].[Employee_Info]
    ADD CONSTRAINT [DF_Employee_Info_Salary] DEFAULT ((0)) FOR [Salary];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Info_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Info]
    ADD CONSTRAINT [DF_Employee_Info_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Info_Abs_Deduction]...';


GO
ALTER TABLE [dbo].[Employee_Info]
    ADD CONSTRAINT [DF_Employee_Info_Abs_Deduction] DEFAULT ((0)) FOR [Abs_Deduction];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Info_Late_Days]...';


GO
ALTER TABLE [dbo].[Employee_Info]
    ADD CONSTRAINT [DF_Employee_Info_Late_Days] DEFAULT ((1)) FOR [Late_Days];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Leave_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Leave]
    ADD CONSTRAINT [DF_Employee_Leave_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Diduction]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_Diduction] DEFAULT ((0)) FOR [Diduction];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Bonus]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_Bonus] DEFAULT ((0)) FOR [Bonus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_PaidAmount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_PayorderDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_PayorderDate] DEFAULT (getdate()) FOR [PayorderDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Allowance]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_Allowance] DEFAULT ((0)) FOR [Allowance];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Fine]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_Fine] DEFAULT ((0)) FOR [Fine];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_PayorderAmount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder]
    ADD CONSTRAINT [DF_Employee_Payorder_PayorderAmount] DEFAULT ((0)) FOR [PayorderAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Daily_Payorder_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Daily]
    ADD CONSTRAINT [DF_Employee_Daily_Payorder_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Daily_Amount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Daily]
    ADD CONSTRAINT [DF_Employee_Payorder_Daily_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Monthly_Payorder_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Monthly]
    ADD CONSTRAINT [DF_Employee_Monthly_Payorder_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Monthly_Amount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Monthly]
    ADD CONSTRAINT [DF_Employee_Payorder_Monthly_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Name_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Name]
    ADD CONSTRAINT [DF_Employee_Payorder_Name_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Records_Paid_date]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Records]
    ADD CONSTRAINT [DF_Employee_Payorder_Records_Paid_date] DEFAULT (getdate()) FOR [Paid_date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Records_Insert_Date]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Records]
    ADD CONSTRAINT [DF_Employee_Payorder_Records_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Weekly_Payorder_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Weekly]
    ADD CONSTRAINT [DF_Employee_Weekly_Payorder_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Weekly_Amount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Weekly]
    ADD CONSTRAINT [DF_Employee_Payorder_Weekly_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Payorder_Work_Basis_Amount]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Work_Basis]
    ADD CONSTRAINT [DF_Employee_Payorder_Work_Basis_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Work_Basis_Payorder_WorkQunatity]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Work_Basis]
    ADD CONSTRAINT [DF_Employee_Work_Basis_Payorder_WorkQunatity] DEFAULT ((0)) FOR [WorkingQunatity];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Employee_Work_Basis_Payorder_CreateDate]...';


GO
ALTER TABLE [dbo].[Employee_Payorder_Work_Basis]
    ADD CONSTRAINT [DF_Employee_Work_Basis_Payorder_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_ExamList_ExamAdd_Percentage]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_ExamList]
    ADD CONSTRAINT [DF_Exam_Cumulative_ExamList_ExamAdd_Percentage] DEFAULT ((100)) FOR [ExamAdd_Percentage];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_ExamList_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_ExamList]
    ADD CONSTRAINT [DF_Exam_Cumulative_ExamList_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_ExamList_Exam_EnableFail]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_ExamList]
    ADD CONSTRAINT [DF_Exam_Cumulative_ExamList_Exam_EnableFail] DEFAULT ((0)) FOR [Exam_EnableFail];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_FullMarks_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_FullMarks]
    ADD CONSTRAINT [DF_Exam_Cumulative_FullMarks_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Name_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Name]
    ADD CONSTRAINT [DF_Exam_Cumulative_Name_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_Last_Published_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_Last_Published_Date] DEFAULT (getdate()) FOR [Last_Published_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_IS_Show_Sec_Position]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_IS_Show_Sec_Position] DEFAULT ((1)) FOR [IS_Hide_SubExam];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_IS_Hide_Sec_Position]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_IS_Hide_Sec_Position] DEFAULT ((0)) FOR [IS_Hide_Sec_Position];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_IS_Grade_BasePoint]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_IS_Grade_BasePoint] DEFAULT ((1)) FOR [IS_Grade_BasePoint];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_Attendance_ScheduleID]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_Attendance_ScheduleID] DEFAULT ((0)) FOR [Attendance_ScheduleID];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_IS_Published]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_IS_Published] DEFAULT ((1)) FOR [IS_Published];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Setting_IS_Hide_Class_Position]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Setting]
    ADD CONSTRAINT [DF_Exam_Cumulative_Setting_IS_Hide_Class_Position] DEFAULT ((0)) FOR [IS_Hide_Class_Position];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Student_StudentAbsenceStatus]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Student]
    ADD CONSTRAINT [DF_Exam_Cumulative_Student_StudentAbsenceStatus] DEFAULT (N'Absent') FOR [StudentAbsenceStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Student_NotGolden]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Student]
    ADD CONSTRAINT [DF_Exam_Cumulative_Student_NotGolden] DEFAULT ((1)) FOR [NotGolden];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Student_PassStatus_InSubject]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Student]
    ADD CONSTRAINT [DF_Exam_Cumulative_Student_PassStatus_InSubject] DEFAULT ('P') FOR [PassStatus_InSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Student_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Student]
    ADD CONSTRAINT [DF_Exam_Cumulative_Student_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Student_TotalMark_ofStudent]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Student]
    ADD CONSTRAINT [DF_Exam_Cumulative_Student_TotalMark_ofStudent] DEFAULT ((0)) FOR [TotalMark_ofStudent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Subject_SubjectType]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Subject]
    ADD CONSTRAINT [DF_Exam_Cumulative_Subject_SubjectType] DEFAULT ('Compulsory') FOR [SubjectType];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Subject_SubjectAbsenceStatus]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Subject]
    ADD CONSTRAINT [DF_Exam_Cumulative_Subject_SubjectAbsenceStatus] DEFAULT (N'Absent') FOR [SubjectAbsenceStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Subject_Date]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Subject]
    ADD CONSTRAINT [DF_Exam_Cumulative_Subject_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Subject_TotalMark_ofSubject]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Subject]
    ADD CONSTRAINT [DF_Exam_Cumulative_Subject_TotalMark_ofSubject] DEFAULT ((0)) FOR [TotalMark_ofSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Cumulative_Subject_IS_Add_InExam]...';


GO
ALTER TABLE [dbo].[Exam_Cumulative_Subject]
    ADD CONSTRAINT [DF_Exam_Cumulative_Subject_IS_Add_InExam] DEFAULT ((1)) FOR [IS_Add_InExam];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Grade_Name_Insert_Date]...';


GO
ALTER TABLE [dbo].[Exam_Grade_Name]
    ADD CONSTRAINT [DF_Exam_Grade_Name_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Grading_Assign_Insert_Date]...';


GO
ALTER TABLE [dbo].[Exam_Grading_Assign]
    ADD CONSTRAINT [DF_Exam_Grading_Assign_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Obtain_Marks_AbsenceStatus]...';


GO
ALTER TABLE [dbo].[Exam_Obtain_Marks]
    ADD CONSTRAINT [DF_Exam_Obtain_Marks_AbsenceStatus] DEFAULT (N'Absent') FOR [AbsenceStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Obtain_Marks_AddPercentage]...';


GO
ALTER TABLE [dbo].[Exam_Obtain_Marks]
    ADD CONSTRAINT [DF_Exam_Obtain_Marks_AddPercentage] DEFAULT ((100)) FOR [AddPercentage];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_IS_Show_Sec_Position]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_IS_Show_Sec_Position] DEFAULT ((1)) FOR [IS_Hide_Sec_Position];


GO
PRINT N'Creating Default Constraint [dbo].[DF__Exam_Publ__IS_Hi__70499252]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF__Exam_Publ__IS_Hi__70499252] DEFAULT ((1)) FOR [IS_Hide_PassMark];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_IS_Grade_BasePoint]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_IS_Grade_BasePoint] DEFAULT ((1)) FOR [IS_Grade_BasePoint];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_Marks_Input_Locked]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_Marks_Input_Locked] DEFAULT ((0)) FOR [Marks_Input_Locked];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_IS_Hide_Class_Position]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_IS_Hide_Class_Position] DEFAULT ((0)) FOR [IS_Hide_Class_Position];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_Attendance_ScheduleID]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_Attendance_ScheduleID] DEFAULT ((0)) FOR [Attendance_ScheduleID];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Publish_Setting_Last_Published_Date]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF_Exam_Publish_Setting_Last_Published_Date] DEFAULT (getdate()) FOR [Last_Published_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF__Exam_Publ__IS_Hi__6F556E19]...';


GO
ALTER TABLE [dbo].[Exam_Publish_Setting]
    ADD CONSTRAINT [DF__Exam_Publ__IS_Hi__6F556E19] DEFAULT ((1)) FOR [IS_Hide_FullMark];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_PassStatus_InSubject]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_PassStatus_InSubject] DEFAULT (N'P') FOR [PassStatus_InSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_StudentPublishStatus]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_StudentPublishStatus] DEFAULT (N'U') FOR [StudentPublishStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_ObtainedPercentage_ofStudent]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_ObtainedPercentage_ofStudent] DEFAULT ((0)) FOR [ObtainedPercentage_ofStudent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_TotalMark_ofStudent]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_TotalMark_ofStudent] DEFAULT ((0)) FOR [TotalMark_ofStudent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_TotalExamFullMark_ofStudent]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_TotalExamFullMark_ofStudent] DEFAULT ((0)) FOR [TotalExamFullMark_ofStudent];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_NotGolden]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_NotGolden] DEFAULT ((1)) FOR [NotGolden];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Student_StudentAbsenceStatus]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Student]
    ADD CONSTRAINT [DF_Exam_Result_of_Student_StudentAbsenceStatus] DEFAULT (N'A') FOR [StudentAbsenceStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_IS_Add_InExam]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_IS_Add_InExam] DEFAULT ((1)) FOR [IS_Add_InExam];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_TotalExamFullMark_ofSubject]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_TotalExamFullMark_ofSubject] DEFAULT ((0)) FOR [TotalExamFullMark_ofSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_TotalMark_ofSubject]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_TotalMark_ofSubject] DEFAULT ((0)) FOR [TotalMark_ofSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_PassStatus_InSubExam]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_PassStatus_InSubExam] DEFAULT (N'P') FOR [PassStatus_InSubExam];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_SubjectType]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_SubjectType] DEFAULT ('Compulsory') FOR [SubjectType];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_SubjectAbsenceStatus]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_SubjectAbsenceStatus] DEFAULT (N'Absent') FOR [SubjectAbsenceStatus];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_ObtainedPercentage_ofSubject]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_ObtainedPercentage_ofSubject] DEFAULT ((0)) FOR [ObtainedPercentage_ofSubject];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Exam_Result_of_Subject_TotalExamObtainedMark_ofSubject]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [DF_Exam_Result_of_Subject_TotalExamObtainedMark_ofSubject] DEFAULT ((0)) FOR [TotalExamObtainedMark_ofSubject];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Exam_Routine_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_SavedData]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Exam_Routine_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_SavedData]
    ADD DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Exam_Routine_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_SavedData]
    ADD DEFAULT ((1)) FOR [ClassColumnCount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[Exam_Routine_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_SavedData]
    ADD DEFAULT ((1)) FOR [RowCount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Expenditure_Amount]...';


GO
ALTER TABLE [dbo].[Expenditure]
    ADD CONSTRAINT [DF_Expenditure_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Expenditure_Insert_Date]...';


GO
ALTER TABLE [dbo].[Expenditure]
    ADD CONSTRAINT [DF_Expenditure_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Extra_Income_Extra_IncomeDate]...';


GO
ALTER TABLE [dbo].[Extra_Income]
    ADD CONSTRAINT [DF_Extra_Income_Extra_IncomeDate] DEFAULT (getdate()) FOR [Extra_IncomeDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Extra_Income_Insert_Date]...';


GO
ALTER TABLE [dbo].[Extra_Income]
    ADD CONSTRAINT [DF_Extra_Income_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Extra_IncomeCategory_Total_Extra_Income]...';


GO
ALTER TABLE [dbo].[Extra_IncomeCategory]
    ADD CONSTRAINT [DF_Extra_IncomeCategory_Total_Extra_Income] DEFAULT ((0)) FOR [Total_Extra_Income];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Extra_IncomeCategory_Insert_Date]...';


GO
ALTER TABLE [dbo].[Extra_IncomeCategory]
    ADD CONSTRAINT [DF_Extra_IncomeCategory_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Hybrid_ChangeLog_ChangedUtc]...';


GO
ALTER TABLE [dbo].[Hybrid_ChangeLog]
    ADD CONSTRAINT [DF_Hybrid_ChangeLog_ChangedUtc] DEFAULT (sysutcdatetime()) FOR [ChangedUtc];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Hybrid_EntityMap_CreatedUtc]...';


GO
ALTER TABLE [dbo].[Hybrid_EntityMap]
    ADD CONSTRAINT [DF_Hybrid_EntityMap_CreatedUtc] DEFAULT (sysutcdatetime()) FOR [CreatedUtc];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_Assign_Role_LateFee]...';


GO
ALTER TABLE [dbo].[Income_Assign_Role]
    ADD CONSTRAINT [DF_Income_Assign_Role_LateFee] DEFAULT ((0)) FOR [LateFee];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_Assign_Role_Amount]...';


GO
ALTER TABLE [dbo].[Income_Assign_Role]
    ADD CONSTRAINT [DF_Income_Assign_Role_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_Discount_Record_PreviousAmount]...';


GO
ALTER TABLE [dbo].[Income_Discount_Record]
    ADD CONSTRAINT [DF_Income_Discount_Record_PreviousAmount] DEFAULT ((0)) FOR [PreviousAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_Discount_Record_PostAmount]...';


GO
ALTER TABLE [dbo].[Income_Discount_Record]
    ADD CONSTRAINT [DF_Income_Discount_Record_PostAmount] DEFAULT ((0)) FOR [PostAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_LateFee_Change_Record_PostAmount]...';


GO
ALTER TABLE [dbo].[Income_LateFee_Change_Record]
    ADD CONSTRAINT [DF_Income_LateFee_Change_Record_PostAmount] DEFAULT ((0)) FOR [PostAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_LateFee_Change_Record_PreviousAmount]...';


GO
ALTER TABLE [dbo].[Income_LateFee_Change_Record]
    ADD CONSTRAINT [DF_Income_LateFee_Change_Record_PreviousAmount] DEFAULT ((0)) FOR [PreviousAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_LateFee_Discount_Record_PreviousAmount]...';


GO
ALTER TABLE [dbo].[Income_LateFee_Discount_Record]
    ADD CONSTRAINT [DF_Income_LateFee_Discount_Record_PreviousAmount] DEFAULT ((0)) FOR [PreviousAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_LateFee_Discount_Record_PostAmount]...';


GO
ALTER TABLE [dbo].[Income_LateFee_Discount_Record]
    ADD CONSTRAINT [DF_Income_LateFee_Discount_Record_PostAmount] DEFAULT ((0)) FOR [PostAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_MoneyReceipt_TotalAmount]...';


GO
ALTER TABLE [dbo].[Income_MoneyReceipt]
    ADD CONSTRAINT [DF_Income_MoneyReceipt_TotalAmount] DEFAULT ((0)) FOR [TotalAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_MoneyReceipt_CollectionDate]...';


GO
ALTER TABLE [dbo].[Income_MoneyReceipt]
    ADD CONSTRAINT [DF_Income_MoneyReceipt_CollectionDate] DEFAULT (getdate()) FOR [CollectionDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income__MoneyReceipt_SN]...';


GO
ALTER TABLE [dbo].[Income_MoneyReceipt]
    ADD CONSTRAINT [DF_Income__MoneyReceipt_SN] DEFAULT ((0)) FOR [MoneyReceipt_SN];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PaymentRecord_PaidAmount]...';


GO
ALTER TABLE [dbo].[Income_PaymentRecord]
    ADD CONSTRAINT [DF_Income_PaymentRecord_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_PaidAmount]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_PaidAmount] DEFAULT ((0)) FOR [PaidAmount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_LateFee]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_LateFee] DEFAULT ((0)) FOR [LateFee];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_Discount]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_Discount] DEFAULT ((0)) FOR [Discount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_Amount]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_Amount] DEFAULT ((0)) FOR [Amount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_Is_LateFeeAdded]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_Is_LateFeeAdded] DEFAULT ((0)) FOR [Is_LateFeeAdded];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_NumberOfPayment]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_NumberOfPayment] DEFAULT ((0)) FOR [NumberOfPayment];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_Is_Active]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_Is_Active] DEFAULT ((1)) FOR [Is_Active];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Income_PayOrder_LateFee_Discount]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [DF_Income_PayOrder_LateFee_Discount] DEFAULT ((0)) FOR [LateFee_Discount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_InstResetProg_Updated]...';


GO
ALTER TABLE [dbo].[Institution_Reset_Progress]
    ADD CONSTRAINT [DF_InstResetProg_Updated] DEFAULT (sysutcdatetime()) FOR [UpdatedAt];


GO
PRINT N'Creating Default Constraint [dbo].[DF_InstResetProg_Total]...';


GO
ALTER TABLE [dbo].[Institution_Reset_Progress]
    ADD CONSTRAINT [DF_InstResetProg_Total] DEFAULT ((0)) FOR [TotalRows];


GO
PRINT N'Creating Default Constraint [dbo].[DF_InstResetProg_Deleted]...';


GO
ALTER TABLE [dbo].[Institution_Reset_Progress]
    ADD CONSTRAINT [DF_InstResetProg_Deleted] DEFAULT ((0)) FOR [DeletedRows];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Notice_Admin_Insert_Date]...';


GO
ALTER TABLE [dbo].[Notice_Admin]
    ADD CONSTRAINT [DF_Notice_Admin_Insert_Date] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Contact_US_Sent_Date]...';


GO
ALTER TABLE [dbo].[Public_Contact_US]
    ADD CONSTRAINT [DF_Public_Contact_US_Sent_Date] DEFAULT (getdate()) FOR [Sent_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Contact_US_Is_Read]...';


GO
ALTER TABLE [dbo].[Public_Contact_US]
    ADD CONSTRAINT [DF_Public_Contact_US_Is_Read] DEFAULT ((0)) FOR [Is_Read];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Support_Sent_Date]...';


GO
ALTER TABLE [dbo].[Public_Support]
    ADD CONSTRAINT [DF_Public_Support_Sent_Date] DEFAULT (getdate()) FOR [Sent_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Support_Is_Read]...';


GO
ALTER TABLE [dbo].[Public_Support]
    ADD CONSTRAINT [DF_Public_Support_Is_Read] DEFAULT ((0)) FOR [Is_Read];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Testimonial_PostDate]...';


GO
ALTER TABLE [dbo].[Public_Testimonial]
    ADD CONSTRAINT [DF_Public_Testimonial_PostDate] DEFAULT (getdate()) FOR [Insert_Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Public_Testimonial_Is_Show]...';


GO
ALTER TABLE [dbo].[Public_Testimonial]
    ADD CONSTRAINT [DF_Public_Testimonial_Is_Show] DEFAULT ((1)) FOR [Is_Show];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Registration_CreateDate]...';


GO
ALTER TABLE [dbo].[Registration]
    ADD CONSTRAINT [DF_Registration_CreateDate] DEFAULT (getdate()) FOR [CreateDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_RoutineTime_Is_OffTime]...';


GO
ALTER TABLE [dbo].[RoutineTime]
    ADD CONSTRAINT [DF_RoutineTime_Is_OffTime] DEFAULT ((0)) FOR [Is_OffTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SchoolInfo_IS_ServiceChargeActive]...';


GO
ALTER TABLE [dbo].[SchoolInfo]
    ADD CONSTRAINT [DF_SchoolInfo_IS_ServiceChargeActive] DEFAULT ((1)) FOR [IS_ServiceChargeActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SchoolInfo_Discount]...';


GO
ALTER TABLE [dbo].[SchoolInfo]
    ADD CONSTRAINT [DF_SchoolInfo_Discount] DEFAULT ((0)) FOR [Discount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SchoolInfo_Free_SMS]...';


GO
ALTER TABLE [dbo].[SchoolInfo]
    ADD CONSTRAINT [DF_SchoolInfo_Free_SMS] DEFAULT ((0)) FOR [Free_SMS];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SchoolInfo]...';


GO
ALTER TABLE [dbo].[SchoolInfo]
    ADD DEFAULT ((0)) FOR [OnlinePaymentEnable];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SchoolInfo_Fixed]...';


GO
ALTER TABLE [dbo].[SchoolInfo]
    ADD CONSTRAINT [DF_SchoolInfo_Fixed] DEFAULT ((0)) FOR [Fixed];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SchoolInfo_DueNoticeSettings]...';


GO
ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SchoolInfo_DueNoticeSettings]...';


GO
ALTER TABLE [dbo].[SchoolInfo_DueNoticeSettings]
    ADD DEFAULT ((0)) FOR [IsEnabled];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SikkhaloySetting_SmsSendInterval]...';


GO
ALTER TABLE [dbo].[SikkhaloySetting]
    ADD CONSTRAINT [DF_SikkhaloySetting_SmsSendInterval] DEFAULT ((5)) FOR [SmsSendInterval];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SikkhaloySetting_SmsProcessingUnit]...';


GO
ALTER TABLE [dbo].[SikkhaloySetting]
    ADD CONSTRAINT [DF_SikkhaloySetting_SmsProcessingUnit] DEFAULT ((200)) FOR [SmsProcessingUnit];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_SMS_Balance]...';


GO
ALTER TABLE [dbo].[SMS]
    ADD CONSTRAINT [DF_SMS_SMS_Balance] DEFAULT ((0)) FOR [SMS_Balance];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Table_1_Amount]...';


GO
ALTER TABLE [dbo].[SMS_Group_Phone_Number]
    ADD CONSTRAINT [DF_Table_1_Amount] DEFAULT ((0)) FOR [MobileNo];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_Recharge_Record_Is_Paid]...';


GO
ALTER TABLE [dbo].[SMS_Recharge_Record]
    ADD CONSTRAINT [DF_SMS_Recharge_Record_Is_Paid] DEFAULT ((0)) FOR [Is_Paid];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_Recharge_Record_RechargeSMS]...';


GO
ALTER TABLE [dbo].[SMS_Recharge_Record]
    ADD CONSTRAINT [DF_SMS_Recharge_Record_RechargeSMS] DEFAULT ((0)) FOR [RechargeSMS];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_Recharge_Record_PerSMS_Price]...';


GO
ALTER TABLE [dbo].[SMS_Recharge_Record]
    ADD CONSTRAINT [DF_SMS_Recharge_Record_PerSMS_Price] DEFAULT ((0)) FOR [PerSMS_Price];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_Send_Record_TextCount]...';


GO
ALTER TABLE [dbo].[SMS_Send_Record]
    ADD CONSTRAINT [DF_SMS_Send_Record_TextCount] DEFAULT ((0)) FOR [TextCount];


GO
PRINT N'Creating Default Constraint [dbo].[DF_SMS_Send_Record_SMSCount]...';


GO
ALTER TABLE [dbo].[SMS_Send_Record]
    ADD CONSTRAINT [DF_SMS_Send_Record_SMSCount] DEFAULT ((0)) FOR [SMSCount];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SMS_Template]...';


GO
ALTER TABLE [dbo].[SMS_Template]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SMS_Template]...';


GO
ALTER TABLE [dbo].[SMS_Template]
    ADD DEFAULT (getdate()) FOR [UpdatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[SMS_Template]...';


GO
ALTER TABLE [dbo].[SMS_Template]
    ADD DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Staff_Info_Date]...';


GO
ALTER TABLE [dbo].[Staff_Info]
    ADD CONSTRAINT [DF_Staff_Info_Date] DEFAULT (getdate()) FOR [Date];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Student_ActiveTime]...';


GO
ALTER TABLE [dbo].[Student]
    ADD CONSTRAINT [DF_Student_ActiveTime] DEFAULT (getdate()) FOR [ActiveTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Student_ActiveDate]...';


GO
ALTER TABLE [dbo].[Student]
    ADD CONSTRAINT [DF_Student_ActiveDate] DEFAULT (getdate()) FOR [ActiveDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Student_Act_Deactivate_Log_InsertDate]...';


GO
ALTER TABLE [dbo].[Student_Act_Deactivate_Log]
    ADD CONSTRAINT [DF_Student_Act_Deactivate_Log_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Student_Act_Deactivate_Log_InsertTime]...';


GO
ALTER TABLE [dbo].[Student_Act_Deactivate_Log]
    ADD CONSTRAINT [DF_Student_Act_Deactivate_Log_InsertTime] DEFAULT (getdate()) FOR [InsertTime];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Student_Fault_Date]...';


GO
ALTER TABLE [dbo].[Student_Fault]
    ADD CONSTRAINT [DF_Student_Fault_Date] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[StudentNotice]...';


GO
ALTER TABLE [dbo].[StudentNotice]
    ADD DEFAULT ((0)) FOR [IsHomeWork];


GO
PRINT N'Creating Default Constraint [dbo].[DF_StudentNotice_InsertDate]...';


GO
ALTER TABLE [dbo].[StudentNotice]
    ADD CONSTRAINT [DF_StudentNotice_InsertDate] DEFAULT (getdate()) FOR [InsertDate];


GO
PRINT N'Creating Default Constraint [dbo].[DF_Teacher_Gender]...';


GO
ALTER TABLE [dbo].[Teacher]
    ADD CONSTRAINT [DF_Teacher_Gender] DEFAULT (N'Male') FOR [Gender];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[User_Active_Sessions]...';


GO
ALTER TABLE [dbo].[User_Active_Sessions]
    ADD DEFAULT (getdate()) FOR [LoginTime];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[User_Active_Sessions]...';


GO
ALTER TABLE [dbo].[User_Active_Sessions]
    ADD DEFAULT (getdate()) FOR [LastActivity];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[User_Balance_Submission]...';


GO
ALTER TABLE [dbo].[User_Balance_Submission]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[WordOfTheDay]...';


GO
ALTER TABLE [dbo].[WordOfTheDay]
    ADD DEFAULT (getdate()) FOR [CreatedDate];


GO
PRINT N'Creating Default Constraint unnamed constraint on [dbo].[WordOfTheDay]...';


GO
ALTER TABLE [dbo].[WordOfTheDay]
    ADD DEFAULT ((1)) FOR [IsActive];


GO
PRINT N'Creating Foreign Key [dbo].[FK_AAP_Invoice_AAP_Invoice_Category]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [FK_AAP_Invoice_AAP_Invoice_Category] FOREIGN KEY ([InvoiceCategoryID]) REFERENCES [dbo].[AAP_Invoice_Category] ([InvoiceCategoryID]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Membership]...';


GO
ALTER TABLE [dbo].[aspnet_Membership]
    ADD FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Membership]...';


GO
ALTER TABLE [dbo].[aspnet_Membership]
    ADD FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[aspnet_Applications] ([ApplicationId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Paths]...';


GO
ALTER TABLE [dbo].[aspnet_Paths]
    ADD FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[aspnet_Applications] ([ApplicationId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_PersonalizationAllUsers]...';


GO
ALTER TABLE [dbo].[aspnet_PersonalizationAllUsers]
    ADD FOREIGN KEY ([PathId]) REFERENCES [dbo].[aspnet_Paths] ([PathId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_PersonalizationPerUser]...';


GO
ALTER TABLE [dbo].[aspnet_PersonalizationPerUser]
    ADD FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_PersonalizationPerUser]...';


GO
ALTER TABLE [dbo].[aspnet_PersonalizationPerUser]
    ADD FOREIGN KEY ([PathId]) REFERENCES [dbo].[aspnet_Paths] ([PathId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Profile]...';


GO
ALTER TABLE [dbo].[aspnet_Profile]
    ADD FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Roles]...';


GO
ALTER TABLE [dbo].[aspnet_Roles]
    ADD FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[aspnet_Applications] ([ApplicationId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_Users]...';


GO
ALTER TABLE [dbo].[aspnet_Users]
    ADD FOREIGN KEY ([ApplicationId]) REFERENCES [dbo].[aspnet_Applications] ([ApplicationId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_UsersInRoles]...';


GO
ALTER TABLE [dbo].[aspnet_UsersInRoles]
    ADD FOREIGN KEY ([RoleId]) REFERENCES [dbo].[aspnet_Roles] ([RoleId]);


GO
PRINT N'Creating Foreign Key unnamed constraint on [dbo].[aspnet_UsersInRoles]...';


GO
ALTER TABLE [dbo].[aspnet_UsersInRoles]
    ADD FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Attendance_Schedule_AssignStudent_Attendance_Schedule]...';


GO
ALTER TABLE [dbo].[Attendance_Schedule_AssignStudent]
    ADD CONSTRAINT [FK_Attendance_Schedule_AssignStudent_Attendance_Schedule] FOREIGN KEY ([ScheduleID]) REFERENCES [dbo].[Attendance_Schedule] ([ScheduleID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Employee_Info_SubCategory]...';


GO
ALTER TABLE [dbo].[Employee_Info]
    ADD CONSTRAINT [FK_Employee_Info_SubCategory] FOREIGN KEY ([SubCategoryID]) REFERENCES [dbo].[Employee_SubCategory] ([SubCategoryID]) ON DELETE SET NULL ON UPDATE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Obtain_Marks_Exam_Name]...';


GO
ALTER TABLE [dbo].[Exam_Obtain_Marks]
    ADD CONSTRAINT [FK_Exam_Obtain_Marks_Exam_Name] FOREIGN KEY ([ExamID]) REFERENCES [dbo].[Exam_Name] ([ExamID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Obtain_Marks_Exam_Result_of_Student]...';


GO
ALTER TABLE [dbo].[Exam_Obtain_Marks]
    ADD CONSTRAINT [FK_Exam_Obtain_Marks_Exam_Result_of_Student] FOREIGN KEY ([StudentResultID]) REFERENCES [dbo].[Exam_Result_of_Student] ([StudentResultID]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Result_of_Subject_Exam_Result_of_Student]...';


GO
ALTER TABLE [dbo].[Exam_Result_of_Subject]
    ADD CONSTRAINT [FK_Exam_Result_of_Subject_Exam_Result_of_Student] FOREIGN KEY ([StudentResultID]) REFERENCES [dbo].[Exam_Result_of_Student] ([StudentResultID]) ON DELETE CASCADE ON UPDATE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Routine_CellData_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_CellData]
    ADD CONSTRAINT [FK_Exam_Routine_CellData_SavedData] FOREIGN KEY ([RoutineID]) REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Routine_ClassColumns_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_ClassColumns]
    ADD CONSTRAINT [FK_Exam_Routine_ClassColumns_SavedData] FOREIGN KEY ([RoutineID]) REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Routine_Rows_SavedData]...';


GO
ALTER TABLE [dbo].[Exam_Routine_Rows]
    ADD CONSTRAINT [FK_Exam_Routine_Rows_SavedData] FOREIGN KEY ([RoutineID]) REFERENCES [dbo].[Exam_Routine_SavedData] ([RoutineID]) ON DELETE CASCADE;


GO
PRINT N'Creating Foreign Key [dbo].[FK_Exam_Routine_SavedData_EducationYear]...';


GO
ALTER TABLE [dbo].[Exam_Routine_SavedData]
    ADD CONSTRAINT [FK_Exam_Routine_SavedData_EducationYear] FOREIGN KEY ([EducationYearID]) REFERENCES [dbo].[Education_Year] ([EducationYearID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Expenditure_Expense_CategoryName]...';


GO
ALTER TABLE [dbo].[Expenditure]
    ADD CONSTRAINT [FK_Expenditure_Expense_CategoryName] FOREIGN KEY ([ExpenseCategoryID]) REFERENCES [dbo].[Expense_CategoryName] ([ExpenseCategoryID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_ExpenseSubCategory_Category]...';


GO
ALTER TABLE [dbo].[Expense_SubCategory]
    ADD CONSTRAINT [FK_ExpenseSubCategory_Category] FOREIGN KEY ([ExpenseCategoryID]) REFERENCES [dbo].[Expense_CategoryName] ([ExpenseCategoryID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Extra_Income_Extra_IncomeCategory]...';


GO
ALTER TABLE [dbo].[Extra_Income]
    ADD CONSTRAINT [FK_Extra_Income_Extra_IncomeCategory] FOREIGN KEY ([Extra_IncomeCategoryID]) REFERENCES [dbo].[Extra_IncomeCategory] ([Extra_IncomeCategoryID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Income_Assign_Role_Income_Roles]...';


GO
ALTER TABLE [dbo].[Income_Assign_Role]
    ADD CONSTRAINT [FK_Income_Assign_Role_Income_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Income_Roles] ([RoleID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Income_PaymentRecord_Income_PayOrder]...';


GO
ALTER TABLE [dbo].[Income_PaymentRecord]
    ADD CONSTRAINT [FK_Income_PaymentRecord_Income_PayOrder] FOREIGN KEY ([PayOrderID]) REFERENCES [dbo].[Income_PayOrder] ([PayOrderID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Income_PayOrder_Income_Roles]...';


GO
ALTER TABLE [dbo].[Income_PayOrder]
    ADD CONSTRAINT [FK_Income_PayOrder_Income_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Income_Roles] ([RoleID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_SMS_Group_Phone_Number_SMS_Group_Name]...';


GO
ALTER TABLE [dbo].[SMS_Group_Phone_Number]
    ADD CONSTRAINT [FK_SMS_Group_Phone_Number_SMS_Group_Name] FOREIGN KEY ([SMS_GroupID]) REFERENCES [dbo].[SMS_Group_Name] ([SMS_GroupID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_SMS_Recharge_Record_Registration]...';


GO
ALTER TABLE [dbo].[SMS_Recharge_Record]
    ADD CONSTRAINT [FK_SMS_Recharge_Record_Registration] FOREIGN KEY ([RegistrationID]) REFERENCES [dbo].[Registration] ([RegistrationID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_SMS_Template_School]...';


GO
ALTER TABLE [dbo].[SMS_Template]
    ADD CONSTRAINT [FK_SMS_Template_School] FOREIGN KEY ([SchoolID]) REFERENCES [dbo].[SchoolInfo] ([SchoolID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_StudentsClass_CreateClass]...';


GO
ALTER TABLE [dbo].[StudentsClass]
    ADD CONSTRAINT [FK_StudentsClass_CreateClass] FOREIGN KEY ([ClassID]) REFERENCES [dbo].[CreateClass] ([ClassID]);


GO
PRINT N'Creating Foreign Key [dbo].[FK_Session_Registration]...';


GO
ALTER TABLE [dbo].[User_Active_Sessions]
    ADD CONSTRAINT [FK_Session_Registration] FOREIGN KEY ([RegistrationID]) REFERENCES [dbo].[Registration] ([RegistrationID]) ON DELETE CASCADE;


GO
PRINT N'Creating Check Constraint [dbo].[CK_AAP_Invoice_Due]...';


GO
ALTER TABLE [dbo].[AAP_Invoice]
    ADD CONSTRAINT [CK_AAP_Invoice_Due] CHECK ([Due]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_AccountBalance]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_AccountBalance] CHECK ([AccountBalance]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Deleted_Expense]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Deleted_Expense] CHECK ([Deleted_Expense]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Total_OUT]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Total_OUT] CHECK ([Total_OUT]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Total_Expense]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Total_Expense] CHECK ([Total_Expense]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Total_Income]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Total_Income] CHECK ([Total_Income]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Total_IN]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Total_IN] CHECK ([Total_IN]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Deleted_Income]...';


GO
ALTER TABLE [dbo].[Account]
    ADD CONSTRAINT [CK_Account_Deleted_Income] CHECK ([Deleted_Income]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Amount]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [CK_Account_Amount] CHECK ([Amount]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Log_SN]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [CK_Account_Log_SN] CHECK ([Log_SN]>(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Log_Balance_After]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [CK_Account_Log_Balance_After] CHECK ([Balance_After]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Account_Log_Balance_Before]...';


GO
ALTER TABLE [dbo].[Account_Log]
    ADD CONSTRAINT [CK_Account_Log_Balance_Before] CHECK ([Balance_Before]>=(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_AccountIN_Amount]...';


GO
ALTER TABLE [dbo].[AccountIN_Record]
    ADD CONSTRAINT [CK_AccountIN_Amount] CHECK ([AccountIN_Amount]>(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_AccountOUT_Amount]...';


GO
ALTER TABLE [dbo].[AccountOUT_Record]
    ADD CONSTRAINT [CK_AccountOUT_Amount] CHECK ([AccountOUT_Amount]>(0));


GO
PRINT N'Creating Check Constraint [dbo].[CK_Employee_Leave_Date]...';


GO
ALTER TABLE [dbo].[Employee_Leave]
    ADD CONSTRAINT [CK_Employee_Leave_Date] CHECK ([LeaveStartDate]<=[LeaveEndDate]);


GO
PRINT N'Creating Trigger [dbo].[Tr_AAP_Reference_PaymentRecord_Delete]...';


GO
CREATE TRIGGER [dbo].[Tr_AAP_Reference_PaymentRecord_Delete]
   ON [dbo].[AAP_Reference_PaymentRecord]
   AFTER DELETE 
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    DECLARE @ret float
	DECLARE @ReferenceID int
	
	SELECT  @ReferenceID = ReferenceID  FROM DELETED

    SELECT @ret = isnull(SUM(Amount),0) FROM AAP_Reference_PaymentRecord WHERE  (ReferenceID = @ReferenceID) 

    UPDATE [AAP_Reference] SET  PaidAmount = @ret WHERE (ReferenceID = @ReferenceID) 
END
GO
PRINT N'Creating Trigger [dbo].[Tr_AAP_Reference_PaymentRecord_InsertUpdate]...';


GO
CREATE TRIGGER [dbo].[Tr_AAP_Reference_PaymentRecord_InsertUpdate]
   ON [dbo].[AAP_Reference_PaymentRecord]
   AFTER INSERT, UPDATE 
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @ret float
	DECLARE @ReferenceID int
	
	SELECT  @ReferenceID = ReferenceID FROM INSERTED

    SELECT @ret = isnull(SUM(Amount),0) FROM AAP_Reference_PaymentRecord WHERE  (ReferenceID = @ReferenceID) 

    UPDATE [AAP_Reference] SET  PaidAmount = @ret WHERE (ReferenceID = @ReferenceID) 
END
GO
PRINT N'Creating Trigger [dbo].[Tr_AAP_Reference_PayOrder_Delete]...';


GO
CREATE TRIGGER [dbo].[Tr_AAP_Reference_PayOrder_Delete]
   ON [dbo].[AAP_Reference_PayOrder]
   AFTER Delete
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @ret float
	DECLARE @ReferenceID int
	
	SELECT @ReferenceID = ReferenceID FROM DELETED

    SELECT @ret = isnull(SUM(Amount),0) FROM AAP_Reference_PayOrder WHERE (ReferenceID = @ReferenceID)
 
    UPDATE [AAP_Reference] SET  TotalAmount = @ret WHERE  (ReferenceID = @ReferenceID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_AAP_Reference_PayOrder_InsertUpdate]...';


GO
CREATE TRIGGER [dbo].[Tr_AAP_Reference_PayOrder_InsertUpdate]
   ON [dbo].[AAP_Reference_PayOrder]
   AFTER INSERT, UPDATE 
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @ret float
	DECLARE @ReferenceID int
	
	SELECT  @ReferenceID = ReferenceID FROM INSERTED

    SELECT @ret = isnull(SUM(Amount),0) FROM AAP_Reference_PayOrder WHERE (ReferenceID = @ReferenceID)
 
    UPDATE [AAP_Reference] SET  TotalAmount = @ret WHERE  (ReferenceID = @ReferenceID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Attendance_Schedule_INSERT]...';


GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER  [dbo].[Tr_Attendance_Schedule_INSERT]
   ON [dbo].[Attendance_Schedule]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE  @ScheduleID int
DECLARE  @SchoolID int
DECLARE  @RegistrationID int 
DECLARE  @LateEntryTime time
DECLARE  @StartTime time
DECLARE  @EndTime time

SELECT *  Into #Temp_Table  FROM INSERTED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
	SELECT Top 1 @ScheduleID = ScheduleID, @SchoolID = SchoolID,@RegistrationID = RegistrationID,@LateEntryTime = LateEntryTime,@StartTime = StartTime, @EndTime = EndTime FROM #Temp_Table

	 INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Saturday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Sunday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Monday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Tuesday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Wednesday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Thursday', @LateEntryTime, @StartTime, @EndTime)
  INSERT INTO Attendance_Schedule_Day
                         (ScheduleID, SchoolID, RegistrationID,Day, LateEntryTime, StartTime, EndTime)
                  VALUES (@ScheduleID, @SchoolID, @RegistrationID, 'Friday', @LateEntryTime, @StartTime, @EndTime)
    Delete  #Temp_Table Where ScheduleID = @ScheduleID


	   Delete #Temp_Table Where ScheduleID = @ScheduleID
END
DROP TABLE #Temp_Table
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Attendance_SMS_DuplicateCheck]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Tr_Attendance_SMS_DuplicateCheck] ON [dbo].[Attendance_SMS]
INSTEAD OF INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
DECLARE @SchoolID int
DECLARE @MobileNo nvarchar(50)
DECLARE @AttendanceDate date
DECLARE @SMS_Text nvarchar(500)


DECLARE @ScheduleTime time(7)
DECLARE @CreateTime time(7)
DECLARE @SentTime time(7)
DECLARE @AttendanceStatus nvarchar(50) 
DECLARE @SMS_TimeOut int
DECLARE @EmployeeID int 
DECLARE @StudentID int

SELECT * Into #Temp_Table_SMS FROM INSERTED WHERE ScheduleTime is not null   

While EXISTS(SELECT * From #Temp_Table_SMS)
Begin

    Select Top 1 @SchoolID = SchoolID, @MobileNo = MobileNo, @AttendanceDate = AttendanceDate, @SMS_Text = SMS_Text,@ScheduleTime=ScheduleTime,@CreateTime=CreateTime, @SentTime = SentTime, @AttendanceStatus = AttendanceStatus, @SMS_TimeOut = SMS_TimeOut, @EmployeeID = EmployeeID, @StudentID = StudentID From #Temp_Table_SMS
	
    IF NOT EXISTS(SELECT Attendance_SMSID from [dbo].[Attendance_SMS] Where SchoolID = @SchoolID AND MobileNo = @MobileNo AND AttendanceDate = @AttendanceDate AND SMS_Text = @SMS_Text)
     BEGIN
	  INSERT INTO  [dbo].[Attendance_SMS] 
                         (SchoolID, ScheduleTime, CreateTime, SentTime, AttendanceDate, SMS_Text, MobileNo, AttendanceStatus, SMS_TimeOut, EmployeeID, StudentID)
VALUES        (@SchoolID, @ScheduleTime, @CreateTime, @SentTime, @AttendanceDate, @SMS_Text, @MobileNo, @AttendanceStatus, @SMS_TimeOut, @EmployeeID, @StudentID)
     END

	Delete Top(1) from #Temp_Table_SMS
END
Drop table #Temp_Table_SMS 
END
GO
PRINT N'Creating Trigger [dbo].[Tr_CommitteeDonation]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Tr_CommitteeDonation]
   ON  [dbo].[CommitteeDonation]
   AFTER  INSERT,DELETE,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

UPDATE CommitteeMember SET TotalDonation = T.Amount, PaidDonation = T.PaidAmount
FROM CommitteeMember INNER JOIN
  (SELECT SUM(PaidAmount) AS PaidAmount, SUM(Amount) AS Amount, CommitteeMemberId
   FROM CommitteeDonation GROUP BY CommitteeMemberId) AS T ON CommitteeMember.CommitteeMemberId = T.CommitteeMemberId
   WHERE
   (CommitteeMember.CommitteeMemberId in(SELECT CommitteeMemberId FROM INSERTED UNION SELECT CommitteeMemberId FROM DELETED))
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Allowance_Records]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Tr_Employee_Allowance_Records]
 ON [dbo].[Employee_Allowance_Records]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @Employee_PayorderID int
	DECLARE @SchoolID int
    DECLARE @Allowance float

	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM INSERTED
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM DELETED

	SELECT  @Allowance = SUM(AllowanceAmount) FROM Employee_Allowance_Records WHERE(Employee_PayorderID = @Employee_PayorderID)

	UPDATE Employee_Payorder SET Allowance = isnull(@Allowance,0) WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Bonus_Records]...';


GO


CREATE TRIGGER [dbo].[Tr_Employee_Bonus_Records]
 ON [dbo].[Employee_Bonus_Records]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @Employee_PayorderID int
	DECLARE @SchoolID int
    DECLARE @Bonus float

	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM INSERTED
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM DELETED

	SELECT  @Bonus = SUM(Bonus_Amount) FROM Employee_Bonus_Records WHERE(Employee_PayorderID = @Employee_PayorderID)

	UPDATE Employee_Payorder SET Bonus = isnull(@Bonus,0) WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Deduction_Records]...';


GO

CREATE TRIGGER [dbo].[Tr_Employee_Deduction_Records]
 ON [dbo].[Employee_Deduction_Records]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @Employee_PayorderID int
	DECLARE @SchoolID int
    DECLARE @Diduction float

	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM INSERTED
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM DELETED

	SELECT  @Diduction = SUM(Deduction_Amount) FROM Employee_Deduction_Records WHERE(Employee_PayorderID = @Employee_PayorderID)

	UPDATE Employee_Payorder SET Diduction = isnull(@Diduction,0) WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Fine_Records]...';


GO
CREATE TRIGGER [dbo].[Tr_Employee_Fine_Records]
 ON [dbo].[Employee_Fine_Records]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON; 
	DECLARE @Employee_PayorderID int
	DECLARE @SchoolID int
    DECLARE @Fine float
	DECLARE @Att_Fine float
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM INSERTED
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM DELETED

	SELECT  @Fine = ISNULL(SUM(Fine_Amount),0) FROM Employee_Fine_Records WHERE(Employee_PayorderID = @Employee_PayorderID)

	SELECT @Att_Fine = ISNULL(FineAmount,0) FROM  Employee_Payorder_Monthly WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)

	UPDATE Employee_Payorder SET Fine = @Fine + @Att_Fine WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_DeviceID]...';


GO
CREATE TRIGGER  [dbo].[Tr_Employee_DeviceID]
 ON dbo.Employee_Info
   AFTER INSERT
AS 
BEGIN

    SET NOCOUNT ON;
	DECLARE @EmployeeID int
	DECLARE @SchoolID int
    DECLARE @Device_SN int

	SELECT   @SchoolID = SchoolID, @EmployeeID = EmployeeID  FROM INSERTED

	SELECT  @Device_SN = isnull(Device_SN,0)+ 1 FROM SchoolInfo WHERE(SchoolID = @SchoolID)

	UPDATE Employee_Info SET DeviceID = @Device_SN  WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID)

	UPDATE SchoolInfo SET Device_SN = @Device_SN   WHERE (SchoolID = @SchoolID) 

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Payorder_Monthly]...';


GO

create TRIGGER [dbo].[Tr_Employee_Payorder_Monthly]
 ON [dbo].[Employee_Payorder_Monthly]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @Employee_PayorderID int
	DECLARE @SchoolID int
    DECLARE @Fine float
	DECLARE @Att_Fine float
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM INSERTED
	SELECT   @SchoolID = SchoolID, @Employee_PayorderID = Employee_PayorderID  FROM DELETED

	SELECT  @Fine = ISNULL(SUM(Fine_Amount),0) FROM Employee_Fine_Records WHERE(Employee_PayorderID = @Employee_PayorderID)

	SELECT @Att_Fine = ISNULL(FineAmount,0) FROM  Employee_Payorder_Monthly WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)

	UPDATE Employee_Payorder SET Fine = @Fine + @Att_Fine WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Income_PayOrder_UPDATE]...';


GO
CREATE TRIGGER [dbo].[Tr_Income_PayOrder_UPDATE]
   ON dbo.Income_PayOrder
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @PayOrderID int
  
    DECLARE @EndDate date 


	DECLARE @EndDate_I date 
	DECLARE @EndDate_D date 

	DECLARE @EndDate_Changed int 


	SELECT @PayOrderID = PayOrderID, @EndDate_I = EndDate FROM INSERTED

	SELECT @PayOrderID =PayOrderID, @EndDate_D = EndDate  FROM DELETED

	SET @EndDate_Changed =  DATEDIFF(day,@EndDate_I, @EndDate_D) 

IF(@EndDate_Changed <>0)
BEGIN
UPDATE Income_PayOrder SET Is_LateFeeAdded = (CASE WHEN EndDate < GETDATE() THEN 1 ELSE 0 END)  WHERE (Status = 'Due') AND @PayOrderID = PayOrderID
END
	
	
END
GO
PRINT N'Creating Trigger [dbo].[Tr_SMS_Count]...';


GO
CREATE TRIGGER [dbo].[Tr_SMS_Count]
   ON [dbo].[SMS_OtherInfo]
   AFTER Insert
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	--DECLARE @SchoolID int
	--DECLARE @SMS_Send_ID uniqueidentifier
	--DECLARE @SMSCount int
	
	--SELECT @SchoolID = SchoolID , @SMS_Send_ID = SMS_Send_ID FROM INSERTED

	--SELECT @SMSCount = SMSCount FROM SMS_Send_Record WHERE SMS_Send_ID = @SMS_Send_ID

	--UPDATE SMS SET SMS_Balance = SMS_Balance - @SMSCount WHERE  SchoolID = @SchoolID



UPDATE SMS SET SMS_Balance =  SMS_Balance - T.SmsCount 
FROM (SELECT INSERTED.SchoolID, SUM(SMS_Send_Record.SMSCount) AS SmsCount
       FROM INSERTED INNER JOIN SMS_Send_Record 
	   ON INSERTED.SMS_Send_ID = SMS_Send_Record.SMS_Send_ID
       GROUP BY INSERTED.SchoolID) AS T 
	   INNER JOIN SMS ON T.SchoolID = SMS.SchoolID

END
GO
PRINT N'Creating Trigger [dbo].[Tr_SMS_Recharge_InsertUpdate]...';


GO
CREATE TRIGGER [dbo].[Tr_SMS_Recharge_InsertUpdate]
   ON [dbo].[SMS_Recharge_Record]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SchoolID  int
	DECLARE @RechargeSMS int

	SELECT * Into #Temp_MemberUp FROM INSERTED 

 While EXISTS(SELECT * From #Temp_MemberUp)
  Begin
	SELECT @SchoolID  = SchoolID  ,@RechargeSMS = RechargeSMS FROM #Temp_MemberUp

	UPDATE SMS SET SMS_Balance = isnull(SMS_Balance,0) + isnull(@RechargeSMS,0)  Where SchoolID = @SchoolID 
	
	Delete #Temp_MemberUp Where SchoolID = @SchoolID 
  END
 Drop table #Temp_MemberUp 

END
GO
PRINT N'Creating Trigger [dbo].[Tr_SMS_Recharge_Delete]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Tr_SMS_Recharge_Delete]
   ON [dbo].[SMS_Recharge_Record]
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RechargeSMS int
	
	SELECT @SchoolID = SchoolID ,@RechargeSMS = RechargeSMS FROM DELETED

	UPDATE SMS SET SMS_Balance = isnull(SMS_Balance,0) -  isnull(@RechargeSMS,0) Where SchoolID = @SchoolID
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Student_DeviceID]...';


GO
CREATE TRIGGER  [dbo].[Tr_Student_DeviceID]
 ON dbo.Student
   AFTER INSERT
AS 
BEGIN

    SET NOCOUNT ON;
	DECLARE @StudentID int
	DECLARE @SchoolID int
    DECLARE @Device_SN int

	SELECT   @SchoolID = SchoolID, @StudentID = StudentID  FROM INSERTED

	SELECT  @Device_SN = isnull(Device_SN,0)+ 1 FROM SchoolInfo WHERE(SchoolID = @SchoolID)

	UPDATE Student SET DeviceID = @Device_SN   WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID)

	UPDATE SchoolInfo SET Device_SN = @Device_SN  WHERE (SchoolID = @SchoolID) 

END
GO
PRINT N'Creating Trigger [dbo].[Tr_StudentsClass_Insert]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE TRIGGER [dbo].[Tr_StudentsClass_Insert]
   ON  [dbo].[StudentsClass]
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @StudentID int
	DECLARE @StudentClassID int
	DECLARE @Is_New bit

	SELECT  @StudentClassID = StudentClassID, @StudentID = StudentID FROM INSERTED

    SELECT @Is_New = CAST(case when COUNT(StudentID) >1 then 0 else 1 end as bit) FROM  StudentsClass WHERE ((Class_Status = N'Re-Admitted') OR (Class_Status IS NULL)) AND StudentID = @StudentID 

	UPDATE StudentsClass SET Is_New = @Is_New  WHERE StudentClassID = @StudentClassID

END
GO
PRINT N'Creating View [dbo].[V_StudentResultDetails]...';


GO

-- Create View (without index - we use stored procedures instead)
CREATE VIEW dbo.V_StudentResultDetails
AS
SELECT 
    ers.StudentResultID,
    ers.StudentClassID,
    ers.ExamID,
    sc.StudentID,
    s.StudentsName,
    s.ID,
    s.StudentImageID,
    sc.RollNo,
    cc.ClassID,
    cc.Class,
    ISNULL(cs.SectionID, 0) as SectionID,
    ISNULL(cs.Section, '') as Section,
    ISNULL(csh.ShiftID, 0) as ShiftID,
    ISNULL(csh.Shift, '') as Shift,
    ISNULL(csg.SubjectGroupID, 0) as SubjectGroupID,
    ISNULL(csg.SubjectGroup, '') as SubjectGroup,
    ers.ObtainedMark_ofStudent,
    ers.TotalMark_ofStudent,
    ers.Student_Grade,
    ers.Student_Point,
    ers.Average,
    ers.ObtainedPercentage_ofStudent,
    ers.Position_InExam_Class,
    ers.Position_InExam_Subsection,
    sch.SchoolName,
    sch.Address,
    sch.Phone,
    ers.SchoolID,
    ers.EducationYearID
FROM dbo.Exam_Result_of_Student ers
INNER JOIN dbo.StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
INNER JOIN dbo.Student s ON sc.StudentID = s.StudentID
INNER JOIN dbo.CreateClass cc ON sc.ClassID = cc.ClassID
INNER JOIN dbo.SchoolInfo sch ON ers.SchoolID = sch.SchoolID
LEFT JOIN dbo.CreateSection cs ON sc.SectionID = cs.SectionID
LEFT JOIN dbo.CreateShift csh ON sc.ShiftID = csh.ShiftID
LEFT JOIN dbo.CreateSubjectGroup csg ON sc.SubjectGroupID = csg.SubjectGroupID
GO
PRINT N'Creating View [dbo].[vw_aspnet_Applications]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_Applications]
  AS SELECT [dbo].[aspnet_Applications].[ApplicationName], [dbo].[aspnet_Applications].[LoweredApplicationName], [dbo].[aspnet_Applications].[ApplicationId], [dbo].[aspnet_Applications].[Description]
  FROM [dbo].[aspnet_Applications]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_MembershipUsers]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_MembershipUsers]
  AS SELECT [dbo].[aspnet_Membership].[UserId],
            [dbo].[aspnet_Membership].[PasswordFormat],
            [dbo].[aspnet_Membership].[MobilePIN],
            [dbo].[aspnet_Membership].[Email],
            [dbo].[aspnet_Membership].[LoweredEmail],
            [dbo].[aspnet_Membership].[PasswordQuestion],
            [dbo].[aspnet_Membership].[PasswordAnswer],
            [dbo].[aspnet_Membership].[IsApproved],
            [dbo].[aspnet_Membership].[IsLockedOut],
            [dbo].[aspnet_Membership].[CreateDate],
            [dbo].[aspnet_Membership].[LastLoginDate],
            [dbo].[aspnet_Membership].[LastPasswordChangedDate],
            [dbo].[aspnet_Membership].[LastLockoutDate],
            [dbo].[aspnet_Membership].[FailedPasswordAttemptCount],
            [dbo].[aspnet_Membership].[FailedPasswordAttemptWindowStart],
            [dbo].[aspnet_Membership].[FailedPasswordAnswerAttemptCount],
            [dbo].[aspnet_Membership].[FailedPasswordAnswerAttemptWindowStart],
            [dbo].[aspnet_Membership].[Comment],
            [dbo].[aspnet_Users].[ApplicationId],
            [dbo].[aspnet_Users].[UserName],
            [dbo].[aspnet_Users].[MobileAlias],
            [dbo].[aspnet_Users].[IsAnonymous],
            [dbo].[aspnet_Users].[LastActivityDate]
  FROM [dbo].[aspnet_Membership] INNER JOIN [dbo].[aspnet_Users]
      ON [dbo].[aspnet_Membership].[UserId] = [dbo].[aspnet_Users].[UserId]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_Profiles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_Profiles]
  AS SELECT [dbo].[aspnet_Profile].[UserId], [dbo].[aspnet_Profile].[LastUpdatedDate],
      [DataSize]=  DATALENGTH([dbo].[aspnet_Profile].[PropertyNames])
                 + DATALENGTH([dbo].[aspnet_Profile].[PropertyValuesString])
                 + DATALENGTH([dbo].[aspnet_Profile].[PropertyValuesBinary])
  FROM [dbo].[aspnet_Profile]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_Roles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_Roles]
  AS SELECT [dbo].[aspnet_Roles].[ApplicationId], [dbo].[aspnet_Roles].[RoleId], [dbo].[aspnet_Roles].[RoleName], [dbo].[aspnet_Roles].[LoweredRoleName], [dbo].[aspnet_Roles].[Description]
  FROM [dbo].[aspnet_Roles]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_Users]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_Users]
  AS SELECT [dbo].[aspnet_Users].[ApplicationId], [dbo].[aspnet_Users].[UserId], [dbo].[aspnet_Users].[UserName], [dbo].[aspnet_Users].[LoweredUserName], [dbo].[aspnet_Users].[MobileAlias], [dbo].[aspnet_Users].[IsAnonymous], [dbo].[aspnet_Users].[LastActivityDate]
  FROM [dbo].[aspnet_Users]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_UsersInRoles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_UsersInRoles]
  AS SELECT [dbo].[aspnet_UsersInRoles].[UserId], [dbo].[aspnet_UsersInRoles].[RoleId]
  FROM [dbo].[aspnet_UsersInRoles]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_WebPartState_Paths]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_WebPartState_Paths]
  AS SELECT [dbo].[aspnet_Paths].[ApplicationId], [dbo].[aspnet_Paths].[PathId], [dbo].[aspnet_Paths].[Path], [dbo].[aspnet_Paths].[LoweredPath]
  FROM [dbo].[aspnet_Paths]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_WebPartState_Shared]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_WebPartState_Shared]
  AS SELECT [dbo].[aspnet_PersonalizationAllUsers].[PathId], [DataSize]=DATALENGTH([dbo].[aspnet_PersonalizationAllUsers].[PageSettings]), [dbo].[aspnet_PersonalizationAllUsers].[LastUpdatedDate]
  FROM [dbo].[aspnet_PersonalizationAllUsers]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[vw_aspnet_WebPartState_User]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

  CREATE VIEW [dbo].[vw_aspnet_WebPartState_User]
  AS SELECT [dbo].[aspnet_PersonalizationPerUser].[PathId], [dbo].[aspnet_PersonalizationPerUser].[UserId], [DataSize]=DATALENGTH([dbo].[aspnet_PersonalizationPerUser].[PageSettings]), [dbo].[aspnet_PersonalizationPerUser].[LastUpdatedDate]
  FROM [dbo].[aspnet_PersonalizationPerUser]
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating View [dbo].[VW_Attendance_Stu]...';


GO
CREATE VIEW [dbo].[VW_Attendance_Stu]
AS
SELECT        dbo.Student.SchoolID, dbo.Student.DeviceID, dbo.Student.StudentID, dbo.StudentsClass.StudentClassID, dbo.StudentsClass.ClassID, dbo.StudentsClass.EducationYearID
FROM            dbo.Student INNER JOIN
                         dbo.StudentsClass ON dbo.Student.StudentID = dbo.StudentsClass.StudentID
WHERE        (dbo.Student.Status = N'Active') AND (dbo.StudentsClass.Class_Status IS NULL)
GO
PRINT N'Creating View [dbo].[VW_Attendance_Stu_Setting]...';


GO
CREATE VIEW dbo.VW_Attendance_Stu_Setting
AS
SELECT        dbo.Attendance_Schedule_AssignStudent.SchoolID, dbo.Attendance_Schedule_AssignStudent.ScheduleID, dbo.Attendance_Schedule_AssignStudent.StudentID, dbo.Student.StudentsName, dbo.Student.SMSPhoneNo, 
                         dbo.Attendance_Schedule_AssignStudent.Entry_Confirmation, dbo.Attendance_Schedule_AssignStudent.Exit_Confirmation, dbo.Attendance_Schedule_AssignStudent.Is_Abs_SMS, 
                         dbo.Attendance_Schedule_AssignStudent.Is_Late_SMS, dbo.Attendance_Schedule_Day.LateEntryTime, dbo.Attendance_Schedule_Day.StartTime, dbo.Attendance_Schedule_Day.EndTime
FROM            dbo.Attendance_Schedule_AssignStudent INNER JOIN
                         dbo.Student ON dbo.Attendance_Schedule_AssignStudent.StudentID = dbo.Student.StudentID INNER JOIN
                         dbo.Attendance_Schedule ON dbo.Attendance_Schedule_AssignStudent.ScheduleID = dbo.Attendance_Schedule.ScheduleID INNER JOIN
                         dbo.Attendance_Schedule_Day ON dbo.Attendance_Schedule.ScheduleID = dbo.Attendance_Schedule_Day.ScheduleID
WHERE        (dbo.Student.Status = N'Active') AND (DATENAME(WEEKDAY, GETDATE()) = dbo.Attendance_Schedule_Day.Day)
GO
PRINT N'Creating View [dbo].[VW_Emp_Info]...';


GO

CREATE VIEW VW_Emp_Info AS
SELECT
    ei.EmployeeID,
    ei.SchoolID,
    ei.RegistrationID,
    COALESCE(t.FirstName,   si.FirstName)   AS FirstName,
    COALESCE(t.LastName,    si.LastName)    AS LastName,
    COALESCE(t.FatherName,  si.FatherName)  AS FatherName,
    COALESCE(t.Designation, si.Designation) AS Designation,
    ei.EmployeeType,
    ei.Permanent_Temporary,
    COALESCE(t.Phone,  si.Phone)            AS Phone,
    ei.Bank_AccNo,
    ei.Salary,
    ei.Job_Status,
    ei.DeviceID,
    ei.RFID,
    ei.Work_Time_Basis,
    ei.Time_Basis_Type,
    -- Teacher images are stored in Teacher.Image
    -- Staff   images are stored in Staff_Info.Image
    COALESCE(t.Image, si.Image)             AS Image,
    ei.ID,
    ei.Employee_Payorder_NameID,
    ei.SubCategoryID,
    sc.SubCategoryName
FROM Employee_Info ei
LEFT JOIN Teacher              t  ON ei.EmployeeID = t.EmployeeID
LEFT JOIN Staff_Info           si ON ei.EmployeeID = si.EmployeeID
LEFT JOIN Employee_SubCategory sc ON ei.SubCategoryID = sc.SubCategoryID;
GO
PRINT N'Creating View [dbo].[VW_Expense]...';


GO
CREATE VIEW dbo.VW_Expense
AS
SELECT        Expenditure.SchoolID, Expenditure.ExpenseDate AS Ex_Date, Expense_CategoryName.CategoryName , Expenditure.Amount AS Amount, RIGHT(CONVERT(VARCHAR(11), Expenditure.ExpenseDate, 
                         106), 8) AS [Month]
FROM            Expenditure INNER JOIN
                         Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
UNION ALL
SELECT        Employee_Payorder_Records.SchoolID, Employee_Payorder_Records.Paid_date AS Ex_Date, Employee_Payorder_Name.Payorder_Name AS CategoryName, Employee_Payorder_Records.Amount AS Amount, 
                         RIGHT(CONVERT(VARCHAR(11), Employee_Payorder_Records.Paid_date, 106), 8) AS [Month]
FROM            Employee_Payorder_Records INNER JOIN
                         Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
                         Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
GO
PRINT N'Creating View [dbo].[VW_Payment_Monthly_Stu]...';


GO
CREATE VIEW dbo.VW_Payment_Monthly_Stu
AS
SELECT        T_Sch.SchoolID, ISNULL(T_Active.ActiveStudent, 0) AS ActiveStudent, ISNULL(T_Re_Count.Reject_Countable, 0) AS Reject_Countable, ISNULL(T_Re_Uncount.Reject_Uncountable, 0) AS Reject_Uncountable
FROM            (SELECT        dbo.StudentsClass.SchoolID
                          FROM            dbo.StudentsClass INNER JOIN
                                                    dbo.Education_Year ON dbo.StudentsClass.EducationYearID = dbo.Education_Year.EducationYearID INNER JOIN
                                                    dbo.SchoolInfo ON dbo.StudentsClass.SchoolID = dbo.SchoolInfo.SchoolID
                          WHERE        (dbo.Education_Year.IsActive = 1) AND (dbo.SchoolInfo.Validation = N'Valid')
                          GROUP BY dbo.StudentsClass.SchoolID) AS T_Sch LEFT OUTER JOIN
                             (SELECT        Student_2.SchoolID, COUNT(DISTINCT Student_2.StudentID) AS Reject_Uncountable
                               FROM            dbo.Student AS Student_2 INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_3 ON Student_2.StudentID = StudentsClass_3.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_3 ON StudentsClass_3.EducationYearID = Education_Year_3.EducationYearID
                               WHERE        (Student_2.Status = N'Rejected') AND (Education_Year_3.IsActive = 1) AND (ISNULL(Student_2.ActiveDays, 0) <= 5) AND (FORMAT(Student_2.RejectedDate, 'MMM yyyy') = FORMAT(GETDATE(), 
                                                         'MMM yyyy'))
                               GROUP BY Student_2.SchoolID) AS T_Re_Uncount ON T_Sch.SchoolID = T_Re_Uncount.SchoolID LEFT OUTER JOIN
                             (SELECT        dbo.Student.SchoolID, COUNT(DISTINCT dbo.Student.StudentID) AS ActiveStudent
                               FROM            dbo.Student INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_1 ON dbo.Student.StudentID = StudentsClass_1.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_1 ON StudentsClass_1.EducationYearID = Education_Year_1.EducationYearID
                               WHERE        (dbo.Student.Status = 'Active') AND (Education_Year_1.IsActive = 1)
                               GROUP BY dbo.Student.SchoolID) AS T_Active ON T_Sch.SchoolID = T_Active.SchoolID LEFT OUTER JOIN
                             (SELECT        Student_1.SchoolID, COUNT(DISTINCT Student_1.StudentID) AS Reject_Countable
                               FROM            dbo.Student AS Student_1 INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_2 ON Student_1.StudentID = StudentsClass_2.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_2 ON StudentsClass_2.EducationYearID = Education_Year_2.EducationYearID
                               WHERE        (Student_1.Status = N'Rejected') AND (Education_Year_2.IsActive = 1) AND (ISNULL(Student_1.ActiveDays, 0) > 5) AND (FORMAT(Student_1.RejectedDate, 'MMM yyyy') = FORMAT(GETDATE(), 
                                                         'MMM yyyy'))
                               GROUP BY Student_1.SchoolID) AS T_Re_Count ON T_Sch.SchoolID = T_Re_Count.SchoolID
GO
PRINT N'Creating View [dbo].[VW_Payment_Monthly_StudentClass]...';


GO
CREATE VIEW dbo.VW_Payment_Monthly_StudentClass
AS
SELECT        T_Sch.SchoolID, T_Sch.EducationYearID, T_Sch.ClassID, ISNULL(T_Active.ActiveStudent, 0) AS ActiveStudent, ISNULL(T_Re_Count.Reject_Countable, 0) AS Reject_Countable, 
                         ISNULL(T_Re_Uncount.Reject_Uncountable, 0) AS Reject_Uncountable
FROM            (SELECT        dbo.StudentsClass.SchoolID, dbo.StudentsClass.EducationYearID, dbo.StudentsClass.ClassID
                          FROM            dbo.StudentsClass INNER JOIN
                                                    dbo.Education_Year ON dbo.StudentsClass.EducationYearID = dbo.Education_Year.EducationYearID INNER JOIN
                                                    dbo.SchoolInfo ON dbo.StudentsClass.SchoolID = dbo.SchoolInfo.SchoolID
                          WHERE        (dbo.Education_Year.IsActive = 1) AND (dbo.SchoolInfo.Validation = N'Valid')
                          GROUP BY dbo.StudentsClass.ClassID, dbo.StudentsClass.EducationYearID, dbo.StudentsClass.SchoolID) AS T_Sch LEFT OUTER JOIN
                             (SELECT        Student_1.SchoolID, StudentsClass_1.EducationYearID, StudentsClass_1.ClassID, COUNT(Student_1.StudentID) AS Reject_Countable
                               FROM            dbo.Student AS Student_1 INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_1 ON Student_1.StudentID = StudentsClass_1.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_1 ON StudentsClass_1.EducationYearID = Education_Year_1.EducationYearID
                               WHERE        (Student_1.Status = N'Rejected') AND (Education_Year_1.IsActive = 1) AND (ISNULL(Student_1.ActiveDays, 0) > 5) AND (FORMAT(Student_1.RejectedDate, 'MMM yyyy') = FORMAT(GETDATE(), 
                                                         'MMM yyyy'))
                               GROUP BY Student_1.SchoolID, StudentsClass_1.ClassID, StudentsClass_1.EducationYearID) AS T_Re_Count ON T_Sch.ClassID = T_Re_Count.ClassID AND 
                         T_Sch.EducationYearID = T_Re_Count.EducationYearID AND T_Sch.SchoolID = T_Re_Count.SchoolID LEFT OUTER JOIN
                             (SELECT        Student_2.SchoolID, StudentsClass_2.EducationYearID, StudentsClass_2.ClassID, COUNT(Student_2.StudentID) AS ActiveStudent
                               FROM            dbo.Student AS Student_2 INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_2 ON Student_2.StudentID = StudentsClass_2.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_2 ON StudentsClass_2.EducationYearID = Education_Year_2.EducationYearID
                               WHERE        (Student_2.Status = 'Active') AND (Education_Year_2.IsActive = 1)
                               GROUP BY Student_2.SchoolID, StudentsClass_2.ClassID, StudentsClass_2.EducationYearID) AS T_Active ON T_Sch.ClassID = T_Active.ClassID AND T_Sch.EducationYearID = T_Active.EducationYearID AND 
                         T_Sch.SchoolID = T_Active.SchoolID LEFT OUTER JOIN
                             (SELECT        dbo.Student.SchoolID, StudentsClass_3.EducationYearID, StudentsClass_3.ClassID, COUNT(dbo.Student.StudentID) AS Reject_Uncountable
                               FROM            dbo.Student INNER JOIN
                                                         dbo.StudentsClass AS StudentsClass_3 ON dbo.Student.StudentID = StudentsClass_3.StudentID INNER JOIN
                                                         dbo.Education_Year AS Education_Year_3 ON StudentsClass_3.EducationYearID = Education_Year_3.EducationYearID
                               WHERE        (dbo.Student.Status = N'Rejected') AND (Education_Year_3.IsActive = 1) AND (ISNULL(dbo.Student.ActiveDays, 0) <= 5) AND (FORMAT(dbo.Student.RejectedDate, 'MMM yyyy') = FORMAT(GETDATE(), 
                                                         'MMM yyyy'))
                               GROUP BY dbo.Student.SchoolID, StudentsClass_3.ClassID, StudentsClass_3.EducationYearID) AS T_Re_Uncount ON T_Sch.ClassID = T_Re_Uncount.ClassID AND 
                         T_Sch.EducationYearID = T_Re_Uncount.EducationYearID AND T_Sch.SchoolID = T_Re_Uncount.SchoolID
GO
PRINT N'Creating View [dbo].[VW_School_UserID]...';


GO
CREATE VIEW dbo.VW_School_UserID
AS
SELECT        dbo.AST.SchoolID, dbo.SchoolInfo.SchoolName, dbo.AST.UserName, dbo.AST.Password, dbo.AST.PasswordAnswer
FROM            dbo.AST INNER JOIN
                         dbo.SchoolInfo ON dbo.AST.SchoolID = dbo.SchoolInfo.SchoolID
WHERE        (dbo.AST.Category = N'Admin') AND (dbo.SchoolInfo.Validation = N'Valid')
GO
PRINT N'Creating View [dbo].[VW_Student_Details]...';


GO
CREATE VIEW dbo.VW_Student_Details
AS
SELECT        dbo.Student.SchoolID, dbo.Student.StudentID, dbo.SchoolInfo.SchoolName, dbo.Student.ID, dbo.Student.StudentsName, dbo.CreateClass.Class, dbo.StudentsClass.RollNo, dbo.CreateSection.Section, 
                         dbo.CreateShift.Shift, dbo.CreateSubjectGroup.SubjectGroup, dbo.Student.SMSPhoneNo, dbo.Student.StudentEmailAddress, dbo.Student.Gender, dbo.Student.DateofBirth, dbo.Student.BloodGroup, 
                         dbo.Student.Religion, dbo.Student.StudentPermanentAddress, dbo.Student.StudentsLocalAddress, dbo.Student.MothersName, dbo.Student.MotherOccupation, dbo.Student.MotherPhoneNumber, 
                         dbo.Student.FathersName, dbo.Student.FatherOccupation, dbo.Student.FatherPhoneNumber, dbo.Student.GuardianName, dbo.Student.GuardianRelationshipwithStudent, dbo.Student.GuardianPhoneNumber, 
                         dbo.Student.OtherDetails, dbo.Student.AdmissionDate
FROM            dbo.Student INNER JOIN
                         dbo.StudentsClass ON dbo.Student.StudentID = dbo.StudentsClass.StudentID INNER JOIN
                         dbo.CreateClass ON dbo.StudentsClass.ClassID = dbo.CreateClass.ClassID INNER JOIN
                         dbo.Education_Year ON dbo.StudentsClass.EducationYearID = dbo.Education_Year.EducationYearID INNER JOIN
                         dbo.SchoolInfo ON dbo.Student.SchoolID = dbo.SchoolInfo.SchoolID LEFT OUTER JOIN
                         dbo.CreateShift ON dbo.StudentsClass.ShiftID = dbo.CreateShift.ShiftID LEFT OUTER JOIN
                         dbo.CreateSection ON dbo.StudentsClass.SectionID = dbo.CreateSection.SectionID LEFT OUTER JOIN
                         dbo.CreateSubjectGroup ON dbo.StudentsClass.SubjectGroupID = dbo.CreateSubjectGroup.SubjectGroupID
WHERE        (dbo.Student.Status = N'Active') AND (dbo.Education_Year.Status = N'True')
GO
PRINT N'Creating View [dbo].[VW_TotalStudent_Amount_Report]...';


GO
CREATE VIEW dbo.VW_TotalStudent_Amount_Report
AS
SELECT        dbo.SchoolInfo.School_SN, dbo.SchoolInfo.SchoolName, COUNT(dbo.StudentsClass.StudentClassID) AS Total_Student, dbo.SchoolInfo.Per_Student_Rate, COUNT(dbo.StudentsClass.StudentClassID) 
                         * dbo.SchoolInfo.Per_Student_Rate AS Total_Taka, (CASE WHEN dbo.SchoolInfo.Fixed = (0) THEN (COUNT(dbo.StudentsClass.StudentClassID) * dbo.SchoolInfo.Per_Student_Rate) 
                         ELSE isnull(dbo.SchoolInfo.Fixed, (0)) END) AS Taka_Fixed_Ad, dbo.Education_Year.EducationYear, dbo.SchoolInfo.Address, dbo.SchoolInfo.IS_ServiceChargeActive
FROM            dbo.Student INNER JOIN
                         dbo.StudentsClass ON dbo.Student.StudentID = dbo.StudentsClass.StudentID INNER JOIN
                         dbo.SchoolInfo ON dbo.StudentsClass.SchoolID = dbo.SchoolInfo.SchoolID INNER JOIN
                         dbo.Education_Year ON dbo.StudentsClass.EducationYearID = dbo.Education_Year.EducationYearID
WHERE        (dbo.Student.Status = N'Active') AND (dbo.Education_Year.Status = N'True') AND (dbo.SchoolInfo.Validation = N'valid')
GROUP BY dbo.SchoolInfo.School_SN, dbo.SchoolInfo.SchoolName, dbo.SchoolInfo.Per_Student_Rate, dbo.Education_Year.EducationYear, dbo.SchoolInfo.Address, dbo.SchoolInfo.Fixed, 
                         dbo.SchoolInfo.IS_ServiceChargeActive
GO
PRINT N'Creating Trigger [dbo].[Tr_Attendance_Record_SMS]...';


GO

CREATE TRIGGER [dbo].[Tr_Attendance_Record_SMS] ON [dbo].[Attendance_Record]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    DECLARE @StudentID int
    DECLARE @SchoolID int
    DECLARE @RegistrationID int
    DECLARE @ClassID int
    DECLARE @StudentClassID int
    DECLARE @EducationYearID int
    DECLARE @Attendance nvarchar(50)
    DECLARE @AttendanceDate date
    DECLARE @Reason nvarchar(500)
    DECLARE @EntryTime time(7)
    DECLARE @ExitTime time(7)
    DECLARE @ExitStatus nvarchar(50)
    DECLARE @Is_OUT bit
    DECLARE @IsFromDevice bit
    DECLARE @Attendance_ScheduleID int

    DECLARE @StudentsName nvarchar(128)
    DECLARE @SMSPhoneNo nvarchar(50)
    DECLARE @Entry_Confirmation bit
    DECLARE @Exit_Confirmation bit
    DECLARE @Is_Abs_SMS bit
    DECLARE @Is_Late_SMS bit
    DECLARE @LateEntryTime time(7)
    DECLARE @StartTime time(7)
    DECLARE @EndTime time(7)

    DECLARE @ScheduleTime time(7)
    DECLARE @SMS_Text nvarchar(500)

    SELECT *
    INTO #Temp_Table_Attendance_Record
    FROM INSERTED

    SELECT TOP 1 @SchoolID = SchoolID
    FROM #Temp_Table_Attendance_Record

    DECLARE @Is_All_SMS_On bit
    DECLARE @Is_Student_All_SMS_Active bit
    DECLARE @Is_English_SMS bit
    DECLARE @SMS_TimeOut_Minute int
    DECLARE @Is_Student_Abs_SMS_ON bit
    DECLARE @Is_Student_Entry_SMS_ON bit
    DECLARE @Is_Student_Late_SMS_ON bit
    DECLARE @Is_Student_Exit_SMS_ON bit

    SELECT
        @Is_All_SMS_On = Is_All_SMS_On,
        @Is_Student_All_SMS_Active = Is_Student_All_SMS_Active,
        @Is_English_SMS = Is_English_SMS,
        @SMS_TimeOut_Minute = SMS_TimeOut_Minute,
        @Is_Student_Abs_SMS_ON = Is_Student_Abs_SMS_ON,
        @Is_Student_Entry_SMS_ON = Is_Student_Entry_SMS_ON,
        @Is_Student_Late_SMS_ON = Is_Student_Late_SMS_ON,
        @Is_Student_Exit_SMS_ON = Is_Student_Exit_SMS_ON
    FROM Attendance_Device_Setting
    WHERE SchoolID = @SchoolID

    DECLARE @SchoolName nvarchar(128)
    SELECT @SchoolName = SchoolName
    FROM SchoolInfo
    WHERE SchoolID = @SchoolID

    WHILE EXISTS (SELECT 1 FROM #Temp_Table_Attendance_Record)
    BEGIN
        SELECT TOP 1
            @StudentID = StudentID,
            @RegistrationID = RegistrationID,
            @SchoolID = SchoolID,
            @ClassID = ClassID,
            @StudentClassID = StudentClassID,
            @EducationYearID = EducationYearID,
            @Attendance = Attendance,
            @AttendanceDate = AttendanceDate,
            @Reason = Reason,
            @EntryTime = EntryTime,
            @ExitTime = ExitTime,
            @ExitStatus = ExitStatus,
            @Is_OUT = Is_OUT,
            @IsFromDevice = IsFromDevice,
            @Attendance_ScheduleID = Attendance_ScheduleID
        FROM #Temp_Table_Attendance_Record

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[Attendance_Record]
            WHERE SchoolID = @SchoolID
              AND StudentClassID = @StudentClassID
              AND AttendanceDate = @AttendanceDate
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@Attendance_ScheduleID, 0)
        )
        BEGIN
            INSERT INTO Attendance_Record
            (
                StudentID, RegistrationID, SchoolID, ClassID, StudentClassID, EducationYearID,
                Attendance, AttendanceDate, Reason, EntryTime, ExitTime, ExitStatus, Is_OUT, IsFromDevice,
                Attendance_ScheduleID
            )
            VALUES
            (
                @StudentID, @RegistrationID, @SchoolID, @ClassID, @StudentClassID, @EducationYearID,
                @Attendance, @AttendanceDate, @Reason, @EntryTime, @ExitTime, @ExitStatus, @Is_OUT, @IsFromDevice,
                @Attendance_ScheduleID
            )

            IF (@IsFromDevice = 1)
            BEGIN
                IF (@Is_All_SMS_On = 1 AND @Is_Student_All_SMS_Active = 1)
                BEGIN
                    SELECT
                        @StudentsName = StudentsName,
                        @SMSPhoneNo = SMSPhoneNo,
                        @Entry_Confirmation = Entry_Confirmation,
                        @Exit_Confirmation = Exit_Confirmation,
                        @Is_Abs_SMS = Is_Abs_SMS,
                        @Is_Late_SMS = Is_Late_SMS,
                        @LateEntryTime = LateEntryTime,
                        @StartTime = StartTime,
                        @EndTime = EndTime
                    FROM VW_Attendance_Stu_Setting
                    WHERE SchoolID = @SchoolID
                      AND StudentID = @StudentID

                    IF @Is_OUT = 0 AND @Attendance = 'Abs' AND @Is_Abs_SMS = 1 AND @Is_Student_Abs_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = @LateEntryTime
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN N'Respected guardian, ' + @StudentsName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') absent, please send to class regularly. ' + @SchoolName
                            ELSE N'à¦¸à¦®à§à¦®à¦¾à¦¨à¦¿à¦¤ à¦…à¦­à¦¿à¦­à¦¾à¦¬à¦•, ' + @StudentsName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') à¦…à¦¨à§à¦ªà¦¸à§à¦¥à¦¿à¦¤, à¦…à¦¨à§à¦—à§à¦°à¦¹ à¦•à¦°à§‡ à¦¨à¦¿à¦¯à¦¼à¦®à¦¿à¦¤ à¦•à§à¦²à¦¾à¦¸à§‡ à¦ªà¦¾à¦ à¦¾à¦¨à¥¤ ' + @SchoolName
                        END
                    END
                    ELSE IF @Is_OUT = 0 AND @Attendance = 'Late Abs' AND @Is_Abs_SMS = 1 AND @Is_Student_Abs_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = @LateEntryTime
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN N'Respected guardian, ' + @StudentsName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') late absent. entry time ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'. ' + @SchoolName
                            ELSE N'à¦¸à¦®à§à¦®à¦¾à¦¨à¦¿à¦¤ à¦…à¦­à¦¿à¦­à¦¾à¦¬à¦•, ' + @StudentsName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') à¦¬à¦¿à¦²à¦®à§à¦¬à§‡ à¦…à¦¨à§à¦ªà¦¸à§à¦¥à¦¿à¦¤ (à¦à¦¨à§à¦Ÿà§à¦°à¦¿ à¦Ÿà¦¾à¦‡à¦®), à¦à¦¨à§à¦Ÿà§à¦°à¦¿ à¦Ÿà¦¾à¦‡à¦® à¦›à¦¿à¦² ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'à¥¤ ' + @SchoolName
                        END
                    END
                    ELSE IF @Is_OUT = 0 AND @Attendance = 'Pre' AND @Entry_Confirmation = 1 AND @Is_Student_Entry_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = ISNULL(@EntryTime, @StartTime)
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN N'Respected guardian, ' + @StudentsName + N' has reached today(' + CONVERT(varchar, @AttendanceDate, 6) + N') in ' + @SchoolName + N' at ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'.'
                            ELSE N'à¦¸à¦®à§à¦®à¦¾à¦¨à¦¿à¦¤ à¦…à¦­à¦¿à¦­à¦¾à¦¬à¦•, ' + @StudentsName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') ' + @SchoolName + N' à¦ ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N' à¦ à¦ªà§Œà¦à¦›à§‡à¦›à§‡à¥¤'
                        END
                    END
                    ELSE IF @Is_OUT = 0 AND @Attendance = 'Late' AND @Is_Late_SMS = 1 AND @Is_Student_Late_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = ISNULL(@EntryTime, @LateEntryTime)
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN N'Respected guardian, ' + @StudentsName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') late ' + ISNULL(CONVERT(varchar, DATEDIFF(MINUTE, @StartTime, @EntryTime)), '') + N' min, entry time ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'. ' + @SchoolName
                            ELSE N'à¦¸à¦®à§à¦®à¦¾à¦¨à¦¿à¦¤ à¦…à¦­à¦¿à¦­à¦¾à¦¬à¦•, ' + @StudentsName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') ' + ISNULL(CONVERT(varchar, DATEDIFF(MINUTE, @StartTime, @EntryTime)), '') + N' à¦®à¦¿: à¦¬à¦¿à¦²à¦®à§à¦¬à§‡, à¦à¦¨à§à¦Ÿà§à¦°à¦¿ à¦Ÿà¦¾à¦‡à¦® à¦›à¦¿à¦² ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'à¥¤ ' + @SchoolName
                        END
                    END
                    ELSE IF @Is_OUT = 1 AND @Exit_Confirmation = 1 AND @Is_Student_Exit_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = ISNULL(@ExitTime, @EndTime)
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN N'Respected guardian, ' + @StudentsName + N' has exited today(' + CONVERT(varchar, @AttendanceDate, 6) + N') from ' + @SchoolName + N' at ' + ISNULL(CONVERT(varchar(15), @ExitTime, 100), '') + N'.'
                            ELSE N'à¦¸à¦®à§à¦®à¦¾à¦¨à¦¿à¦¤ à¦…à¦­à¦¿à¦­à¦¾à¦¬à¦•, ' + @StudentsName + N', ' + @SchoolName + N' à¦¥à§‡à¦•à§‡ à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') ' + ISNULL(CONVERT(varchar(15), @ExitTime, 100), '') + N' à¦ à¦¬à§‡à¦° à¦¹à¦¯à¦¼à§‡à¦›à§‡à¥¤'
                        END
                    END

                    IF @SMS_Text IS NOT NULL AND LEN(LTRIM(RTRIM(@SMS_Text))) > 0
                    BEGIN
                        INSERT INTO Attendance_SMS
                        (
                            SchoolID, ScheduleTime, AttendanceDate, SMS_Text, MobileNo,
                            AttendanceStatus, SMS_TimeOut, StudentID, EmployeeID
                        )
                        VALUES
                        (
                            @SchoolID, @ScheduleTime, @AttendanceDate, @SMS_Text, @SMSPhoneNo,
                            @Attendance, @SMS_TimeOut_Minute, @StudentID, 0
                        )
                    END
                END
            END
        END

        DELETE TOP (1) FROM #Temp_Table_Attendance_Record
    END

    DROP TABLE #Temp_Table_Attendance_Record

    SELECT SCOPE_IDENTITY() AS AttendanceRecordID
END
GO
PRINT N'Creating View [dbo].[VW_Attendance_Emp_Setting]...';


GO
CREATE VIEW dbo.VW_Attendance_Emp_Setting
AS
SELECT        dbo.Employee_Attendance_Schedule_Assign.SchoolID, dbo.Employee_Attendance_Schedule_Assign.EmployeeID, dbo.Employee_Attendance_Schedule_Assign.ScheduleID, 
                         dbo.VW_Emp_Info.FirstName + ' ' + dbo.VW_Emp_Info.LastName AS Name, dbo.VW_Emp_Info.Phone, dbo.Employee_Attendance_Schedule_Assign.Is_Abs_SMS, dbo.Employee_Attendance_Schedule_Assign.Is_Late_SMS, 
                         dbo.Attendance_Schedule_Day.LateEntryTime, dbo.Attendance_Schedule_Day.StartTime, dbo.Attendance_Schedule_Day.EndTime
FROM            dbo.Employee_Attendance_Schedule_Assign INNER JOIN
                         dbo.Attendance_Schedule ON dbo.Employee_Attendance_Schedule_Assign.ScheduleID = dbo.Attendance_Schedule.ScheduleID INNER JOIN
                         dbo.VW_Emp_Info ON dbo.Employee_Attendance_Schedule_Assign.EmployeeID = dbo.VW_Emp_Info.EmployeeID INNER JOIN
                         dbo.Attendance_Schedule_Day ON dbo.Attendance_Schedule.ScheduleID = dbo.Attendance_Schedule_Day.ScheduleID
WHERE        (DATENAME(WEEKDAY, GETDATE()) = dbo.Attendance_Schedule_Day.Day)
GO
PRINT N'Creating View [dbo].[VW_Attendance_User_Leave]...';


GO
CREATE VIEW [dbo].[VW_Attendance_User_Leave]
AS
SELECT DISTINCT Student.SchoolID, Student.DeviceID, Attendance_Leave.StartDate, Attendance_Leave.EndDate
FROM            Student INNER JOIN
                         StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN
                         Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         Attendance_Leave ON Student.StudentID = Attendance_Leave.StudentID
WHERE        (Student.Status = N'Active') AND (Education_Year.IsActive = 1)
UNION ALL
SELECT DISTINCT VW_Emp_Info.SchoolID, VW_Emp_Info.DeviceID, Employee_Leave.LeaveStartDate, Employee_Leave.LeaveEndDate
FROM            VW_Emp_Info INNER JOIN
                         Employee_Leave ON VW_Emp_Info.SchoolID = Employee_Leave.SchoolID AND VW_Emp_Info.EmployeeID = Employee_Leave.EmployeeID
WHERE        (VW_Emp_Info.Job_Status = N'Active') AND (Employee_Leave.ApproveStatus = N'Approved')
GO
PRINT N'Creating View [dbo].[VW_Attendance_Users]...';


GO
CREATE VIEW dbo.VW_Attendance_Users
AS
SELECT DISTINCT Student.SchoolID, Student.DeviceID, Student.ID, Student.RFID, Student.StudentsName AS Name, 'Student' AS Designation, CAST(1 AS bit) AS Is_Student, Attendance_Schedule_AssignStudent.ScheduleID
FROM            Student INNER JOIN
                         StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN
                         Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN
                         Attendance_Schedule_AssignStudent ON Student.StudentID = Attendance_Schedule_AssignStudent.StudentID
WHERE        (Student.Status = N'Active') AND (Education_Year.IsActive = 1)
UNION ALL
SELECT DISTINCT 
                         VW_Emp_Info.SchoolID, VW_Emp_Info.DeviceID, 'E' + VW_Emp_Info.ID AS ID, VW_Emp_Info.RFID, VW_Emp_Info.FirstName +' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, CAST(0 AS bit) AS Is_Student, 
                         Employee_Attendance_Schedule_Assign.ScheduleID
FROM            VW_Emp_Info LEFT OUTER JOIN
                         Employee_Attendance_Schedule_Assign ON VW_Emp_Info.EmployeeID = Employee_Attendance_Schedule_Assign.EmployeeID
WHERE        (VW_Emp_Info.Job_Status = N'Active')
GO
PRINT N'Creating View [dbo].[VW_Attendance_Users_Image]...';


GO
CREATE VIEW dbo.VW_Attendance_Users_Image
AS
SELECT DISTINCT Student.SchoolID, Student.ID, Student_Image.Image
FROM            Student INNER JOIN
                         StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN
                         Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID INNER JOIN
                         Student_Image ON Student.StudentImageID = Student_Image.StudentImageID
WHERE        (Student.Status = N'Active') AND (Education_Year.IsActive = 1) AND Student_Image.Image <> ''
UNION ALL
SELECT        SchoolID, 'E' +ID AS ID, Image
FROM            VW_Emp_Info
WHERE        (Job_Status = N'Active') AND Image <> ''
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Attendance_Record_SMS]...';


GO

CREATE TRIGGER [dbo].[Tr_Employee_Attendance_Record_SMS] ON [dbo].[Employee_Attendance_Record]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    DECLARE @EmployeeID int
    DECLARE @SchoolID int
    DECLARE @RegistrationID int
    DECLARE @AttendanceStatus nvarchar(50)
    DECLARE @AttendanceDate date
    DECLARE @EntryTime time(7)
    DECLARE @ExitTime time(7)
    DECLARE @ExitStatus nvarchar(50)
    DECLARE @Is_OUT bit
    DECLARE @IsFromDevice bit
    DECLARE @Attendance_ScheduleID int

    DECLARE @EmployeesName nvarchar(128)
    DECLARE @EmployeePhoneNumber nvarchar(50)
    DECLARE @SMSPhoneNo nvarchar(50)
    DECLARE @Is_Abs_SMS bit
    DECLARE @Is_Late_SMS bit
    DECLARE @LateEntryTime time(7)
    DECLARE @StartTime time(7)
    DECLARE @EndTime time(7)
    DECLARE @ScheduleTime time(7)
    DECLARE @SMS_Text nvarchar(500)

    SELECT *
    INTO #Temp_Table_Attendance
    FROM INSERTED

    SELECT TOP 1 @SchoolID = SchoolID
    FROM #Temp_Table_Attendance

    DECLARE @Is_All_SMS_On bit
    DECLARE @Is_Employee_All_SMS_Active bit
    DECLARE @Is_English_SMS bit
    DECLARE @SMS_TimeOut_Minute int
    DECLARE @Is_Employee_Abs_SMS_ON bit
    DECLARE @Is_Employee_Late_SMS_ON bit
    DECLARE @Is_Employee_SMS_OwnNumber bit
    DECLARE @Employee_SMS_Number nvarchar(50)

    SELECT
        @Is_All_SMS_On = Is_All_SMS_On,
        @Is_Employee_All_SMS_Active = Is_Employee_SMS_Active,
        @Is_English_SMS = Is_English_SMS,
        @SMS_TimeOut_Minute = SMS_TimeOut_Minute,
        @Is_Employee_Abs_SMS_ON = Is_Employee_Abs_SMS_ON,
        @Is_Employee_Late_SMS_ON = Is_Employee_Late_SMS_ON,
        @Is_Employee_SMS_OwnNumber = Is_Employee_SMS_OwnNumber,
        @Employee_SMS_Number = Employee_SMS_Number
    FROM Attendance_Device_Setting
    WHERE SchoolID = @SchoolID

    WHILE EXISTS (SELECT 1 FROM #Temp_Table_Attendance)
    BEGIN
        SELECT TOP 1
            @EmployeeID = EmployeeID,
            @RegistrationID = RegistrationID,
            @SchoolID = SchoolID,
            @AttendanceStatus = AttendanceStatus,
            @AttendanceDate = AttendanceDate,
            @EntryTime = EntryTime,
            @ExitTime = ExitTime,
            @ExitStatus = ExitStatus,
            @Is_OUT = Is_OUT,
            @IsFromDevice = IsFromDevice,
            @Attendance_ScheduleID = Attendance_ScheduleID
        FROM #Temp_Table_Attendance

        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[Employee_Attendance_Record]
            WHERE SchoolID = @SchoolID
              AND EmployeeID = @EmployeeID
              AND AttendanceDate = @AttendanceDate
              AND ISNULL(Attendance_ScheduleID, 0) = ISNULL(@Attendance_ScheduleID, 0)
        )
        BEGIN
            INSERT INTO Employee_Attendance_Record
            (
                SchoolID, RegistrationID, EmployeeID, Attendance_ScheduleID,
                AttendanceStatus, AttendanceDate, EntryTime, ExitTime,
                ExitStatus, Is_OUT, IsFromDevice
            )
            VALUES
            (
                @SchoolID, @RegistrationID, @EmployeeID, @Attendance_ScheduleID,
                @AttendanceStatus, @AttendanceDate, @EntryTime, @ExitTime,
                @ExitStatus, @Is_OUT, @IsFromDevice
            )

            IF (@IsFromDevice = 1)
            BEGIN
                IF (@Is_All_SMS_On = 1 AND @Is_Employee_All_SMS_Active = 1 AND @AttendanceStatus <> 'Pre')
                BEGIN
                    SELECT
                        @EmployeesName = Name,
                        @EmployeePhoneNumber = Phone,
                        @Is_Abs_SMS = Is_Abs_SMS,
                        @Is_Late_SMS = Is_Late_SMS,
                        @LateEntryTime = LateEntryTime,
                        @StartTime = StartTime,
                        @EndTime = EndTime
                    FROM VW_Attendance_Emp_Setting
                    WHERE SchoolID = @SchoolID
                      AND EmployeeID = @EmployeeID

                    SET @SMSPhoneNo = CASE @Is_Employee_SMS_OwnNumber
                        WHEN 1 THEN @EmployeePhoneNumber
                        ELSE @Employee_SMS_Number
                    END

                    SET @SMS_Text = NULL

                    IF @Is_OUT = 0 AND @AttendanceStatus = 'Abs' AND @Is_Abs_SMS = 1 AND @Is_Employee_Abs_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = @LateEntryTime
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN @EmployeesName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') absent'
                            ELSE @EmployeesName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') à¦…à¦¨à§à¦ªà¦¸à§à¦¥à¦¿à¦¤'
                        END
                    END
                    ELSE IF @Is_OUT = 0 AND @AttendanceStatus = 'Late Abs' AND @Is_Abs_SMS = 1 AND @Is_Employee_Abs_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = @LateEntryTime
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN @EmployeesName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') late absent. entry time ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '')
                            ELSE @EmployeesName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') à¦¬à¦¿à¦²à¦®à§à¦¬à§‡ à¦…à¦¨à§à¦ªà¦¸à§à¦¥à¦¿à¦¤, à¦à¦¨à§à¦Ÿà§à¦°à¦¿ à¦Ÿà¦¾à¦‡à¦® à¦›à¦¿à¦² ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'à¥¤'
                        END
                    END
                    ELSE IF @Is_OUT = 0 AND @AttendanceStatus = 'Late' AND @Is_Late_SMS = 1 AND @Is_Employee_Late_SMS_ON = 1
                    BEGIN
                        SET @ScheduleTime = ISNULL(@EntryTime, @LateEntryTime)
                        SET @SMS_Text = CASE @Is_English_SMS
                            WHEN 1 THEN @EmployeesName + N' today(' + CONVERT(varchar, @AttendanceDate, 6) + N') late ' + ISNULL(CONVERT(varchar, DATEDIFF(MINUTE, @StartTime, @EntryTime)), '') + N' min, entry time ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '')
                            ELSE @EmployeesName + N' à¦†à¦œ(' + CONVERT(varchar, @AttendanceDate, 6) + N') ' + ISNULL(CONVERT(varchar, DATEDIFF(MINUTE, @StartTime, @EntryTime)), '') + N' à¦®à¦¿: à¦¬à¦¿à¦²à¦®à§à¦¬à§‡, à¦à¦¨à§à¦Ÿà§à¦°à¦¿ à¦Ÿà¦¾à¦‡à¦® à¦›à¦¿à¦² ' + ISNULL(CONVERT(varchar(15), @EntryTime, 100), '') + N'à¥¤'
                        END
                    END

                    IF @SMS_Text IS NOT NULL AND LEN(LTRIM(RTRIM(@SMS_Text))) > 0
                    BEGIN
                        INSERT INTO Attendance_SMS
                        (
                            SchoolID, ScheduleTime, AttendanceDate, SMS_Text, MobileNo,
                            AttendanceStatus, SMS_TimeOut, StudentID, EmployeeID
                        )
                        VALUES
                        (
                            @SchoolID, @ScheduleTime, @AttendanceDate, @SMS_Text, @SMSPhoneNo,
                            @AttendanceStatus, @SMS_TimeOut_Minute, 0, @EmployeeID
                        )
                    END
                END
            END
        END

        DELETE TOP (1) FROM #Temp_Table_Attendance
    END

    DROP TABLE #Temp_Table_Attendance

    SELECT SCOPE_IDENTITY() AS Employee_Attendance_RecordID
END
GO
PRINT N'Creating Function [dbo].[Account_Log_SerialNumber]...';


GO
CREATE FUNCTION [dbo].[Account_Log_SerialNumber](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(Log_SN), 0)  FROM [Account_Log] where SchoolID = @SchoolID
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Employee_Payorder_SN]...';


GO

CREATE FUNCTION [dbo].[Employee_Payorder_SN](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(Employee_Payorder_SN), 0)  FROM [Employee_Payorder] where SchoolID = @SchoolID
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Employee_Staff_ID]...';


GO

CREATE FUNCTION [dbo].[Employee_Staff_ID](@StaffID int)
RETURNS nvarchar(50) 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret nvarchar(50);
    SELECT @ret ='S'+ RIGHT('00'+ CONVERT(VARCHAR,Staff_SN),3) FROM Staff_Info WHERE (StaffID = @StaffID)
    RETURN @ret;
END;
GO
PRINT N'Creating Function [dbo].[Employee_Teacher_ID]...';


GO

CREATE FUNCTION [dbo].[Employee_Teacher_ID](@TeacherID int)
RETURNS nvarchar(50) 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret nvarchar(50);
    SELECT @ret ='T'+ RIGHT('00'+ CONVERT(VARCHAR,T_SN),3) FROM Teacher WHERE (TeacherID  = @TeacherID)
    RETURN @ret;
END;
GO
PRINT N'Creating Function [dbo].[F_CommitteeMoneyReceiptSn]...';


GO
CREATE FUNCTION [dbo].[F_CommitteeMoneyReceiptSn](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
	DECLARE @SN int;
    SELECT @SN =   ISNULL(MAX(CommitteeMoneyReceiptSn), 0)  FROM CommitteeMoneyReceipt where SchoolID=@SchoolID

	if(@SN<100000)
	SET @ret = 100000
	ELSE
	SET @ret=@SN 

    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[F_EducationYear_SN]...';


GO
Create FUNCTION [dbo].[F_EducationYear_SN](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
	DECLARE @SN int;
    SELECT @SN = ISNULL(MAX(SN), 0) FROM [Education_Year] where SchoolID = @SchoolID

    RETURN @SN  + 1;
END;
GO
PRINT N'Creating Function [dbo].[F_InvoiceReceipt_SN]...';


GO

create FUNCTION [dbo].[F_InvoiceReceipt_SN]()
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
	DECLARE @SN int;
    SELECT @SN =   ISNULL(MAX(InvoiceReceipt_SN), 0)  FROM [AAP_Invoice_Receipt] 

	if(@SN<100000)
	SET @ret = 100000
	ELSE
	SET @ret=@SN 

    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[F_MoneyReceipt_SN]...';


GO
CREATE FUNCTION [dbo].[F_MoneyReceipt_SN](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
	DECLARE @SN int;
    SELECT @SN =   ISNULL(MAX(MoneyReceipt_SN), 0)  FROM [Income_MoneyReceipt] where SchoolID=@SchoolID

	if(@SN<100000)
	SET @ret = 100000
	ELSE
	SET @ret=@SN 

    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[F_Stu_Attendance_Summary]...';


GO
CREATE FUNCTION [dbo].[F_Stu_Attendance_Summary](@SchoolID int, @EducationYearID int,@StudentClassID int, @Attendance nvarchar(10), @From_Date date, @To_Date date)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;

SELECT @ret = COUNT(StudentClassID)  FROM Attendance_Record 
 WHERE (SchoolID = @SchoolID) AND 
	   (StudentClassID = @StudentClassID) AND 
	   (EducationYearID = @EducationYearID) AND 
	   (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND 
	   (Attendance = @Attendance)

    RETURN @ret;
END;
GO
PRINT N'Creating Function [dbo].[F_Stu_WorkingDay]...';


GO
CREATE FUNCTION [dbo].[F_Stu_WorkingDay](@SchoolID int, @EducationYearID int, @ClassID int, @From_Date date, @To_Date date)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;

    SELECT  @ret = COUNT(distinct AttendanceDate) FROM Attendance_Record 
	WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))

    RETURN @ret;
END;
GO
PRINT N'Creating Function [dbo].[F_Total_Meassage]...';


GO
CREATE FUNCTION [dbo].[F_Total_Meassage]()
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    DECLARE @Contact_US int;
    DECLARE @Support int;
    SELECT @Contact_US = COUNT(ContactUsID) FROM Public_Contact_US WHERE (Is_Read = 0) 
    SELECT @Support  = COUNT(SupportID) FROM Public_Support WHERE (Is_Read = 0)
    SELECT @ret = @Contact_US + @Support 
    RETURN @ret;
END;
GO
PRINT N'Creating Function [dbo].[fn_GetBillableCommitteeCount]...';


GO

CREATE FUNCTION fn_GetBillableCommitteeCount (@SchoolID INT)
RETURNS INT
AS
BEGIN
    DECLARE @CommitteeCount INT = 0
    
    -- Count active committee members from categories that are:
    -- 1. Included in billing (IsIncluded = 1)
    -- 2. Category is active (IsActive = 1)
    -- 3. Member status is 'Active'
    SELECT @CommitteeCount = COUNT(DISTINCT CM.CommitteeMemberId)
    FROM CommitteeMember CM
    INNER JOIN CommitteeMemberType CMT ON CM.CommitteeMemberTypeId = CMT.CommitteeMemberTypeId
    INNER JOIN CommitteeMember_Billing CMB ON CMT.CommitteeMemberTypeId = CMB.CommitteeMemberTypeId 
                                            AND CM.SchoolID = CMB.SchoolID
    WHERE CM.SchoolID = @SchoolID
    AND ISNULL(CM.Status, 'Active') = 'Active' -- Only active members
    AND CMB.IsIncluded = 1 -- Only categories included in billing
    AND CMB.IsActive = 1 -- Only active categories
    
    RETURN ISNULL(@CommitteeCount, 0)
END
GO
PRINT N'Creating Function [dbo].[Institution_SerialNumber]...';


GO

CREATE FUNCTION [dbo].[Institution_SerialNumber]()
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(School_SN), 0)  FROM [SchoolInfo] 
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Invoice_SerialNumber]...';


GO

CREATE FUNCTION [dbo].[Invoice_SerialNumber](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(Invoice_SN), 0)  FROM [AAP_Invoice] where SchoolID = @SchoolID
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Reference_SerialNumber]...';


GO

CREATE FUNCTION [dbo].[Reference_SerialNumber]()
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(Reference_SN), 0)  FROM [AAP_Reference] 
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Staff_SerialNumber]...';


GO

Create FUNCTION [dbo].[Staff_SerialNumber](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(Staff_SN), 0)  FROM [Staff_Info] where SchoolID = @SchoolID
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Function [dbo].[Teacher_SerialNumber]...';


GO

Create FUNCTION [dbo].[Teacher_SerialNumber](@SchoolID int)
RETURNS int 
AS 
-- Returns the stock level for the product.
BEGIN
    DECLARE @ret int;
    SELECT @ret =   ISNULL(MAX(T_SN), 0)  FROM [Teacher] where SchoolID = @SchoolID
    RETURN @ret + 1;
END;
GO
PRINT N'Creating Default Constraint [dbo].[DF_AAP_Reference_Reference_SN]...';


GO
ALTER TABLE [dbo].[AAP_Reference]
    ADD CONSTRAINT [DF_AAP_Reference_Reference_SN] DEFAULT ([dbo].[Reference_SerialNumber]()) FOR [Reference_SN];


GO
PRINT N'Creating Trigger [dbo].[Tr_AccountIN_Record_INSERT]...';


GO

CREATE TRIGGER [dbo].[Tr_AccountIN_Record_INSERT]
   ON [dbo].[AccountIN_Record]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountIN_Amount float 
	DECLARE @IN_Details nvarchar(500)  
    DECLARE @AccountIN_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 

	SELECT  @AccountIN_Amount = AccountIN_Amount,@SchoolID = SchoolID ,@RegistrationID = RegistrationID, @EducationYearID= EducationYearID, @IN_Details = IN_Details,@AccountIN_Date = AccountIN_Date,@AccountID = AccountID FROM INSERTED

	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	 UPDATE [Account] SET Total_IN += @AccountIN_Amount  WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @AccountIN_Amount ,'Add',
    @IN_Details, 'Deposit','Deposit','Deposit', 'Account Deposited Amount '+ cast(@AccountIN_Amount as varchar(50))+' Tk. '+ ISNULL('Details : '+@IN_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountIN_Date,'In','In')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_AccountIN_Record_DELETE]...';


GO
CREATE TRIGGER [dbo].[Tr_AccountIN_Record_DELETE]
   ON [dbo].[AccountIN_Record]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountIN_Amount float 
	DECLARE @IN_Details nvarchar(256)  
    DECLARE @AccountIN_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 
	

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))

	SELECT @SchoolID = SchoolID,@AccountIN_Amount = AccountIN_Amount, @IN_Details = IN_Details, @AccountIN_Date = AccountIN_Date, @AccountID = AccountID, @EducationYearID = EducationYearID FROM DELETED
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	   BEGIN
	      SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	      UPDATE [Account] SET Total_IN -= @AccountIN_Amount  WHERE (AccountID = @AccountID)
	      SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	   END
	ELSE
	   BEGIN
	      SET @Balance_After = 0
	      SET @Balance_Before = 0
	   END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID, @EducationYearID, @AccountIN_Amount ,'Subtraction',
    @IN_Details, 'Deleted Deposit','Deleted Deposit','Deleted Deposit', 'Deposit Amount Deleted '+ cast(@AccountIN_Amount as varchar(50))+' Tk. '+ ISNULL('Details: '+@IN_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountIN_Date,'In','De')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_AccountIN_Record_UPDATE]...';


GO
CREATE TRIGGER [dbo].[Tr_AccountIN_Record_UPDATE]
   ON [dbo].[AccountIN_Record]
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountIN_Amount float 
	DECLARE @IN_Details nvarchar(256)  
    DECLARE @AccountIN_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 

	DECLARE @AccountIN_Amount_I float
	DECLARE @AccountIN_Amount_D float
	DECLARE @AccountIN_Amount_Changed float

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))
	SELECT  @AccountIN_Amount_I = AccountIN_Amount,   @SchoolID = SchoolID, @EducationYearID = EducationYearID , @IN_Details = IN_Details, @AccountIN_Date = AccountIN_Date,@AccountID = AccountID FROM INSERTED
	SELECT  @AccountIN_Amount_D = AccountIN_Amount, @SchoolID = SchoolID, @EducationYearID = EducationYearID , @IN_Details = IN_Details,@AccountIN_Date = AccountIN_Date, @AccountID = AccountID  FROM DELETED

	SET @AccountIN_Amount_Changed =  isnull(@AccountIN_Amount_I,0) -  isnull(@AccountIN_Amount_D,0) 
 
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_IN += @AccountIN_Amount_Changed WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	if(@AccountIN_Amount_Changed > 0)
	BEGIN
	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID, @EducationYearID, @AccountIN_Amount_Changed, 'Add',
    @IN_Details, 'Updated Deposit','Updated Deposit','Updated Deposit', 'Deposit Amount Changed. Previous Amount Was '+ cast(@AccountIN_Amount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@AccountIN_Amount_I as varchar(50))+' Tk. Total Increased Amount '+cast(@AccountIN_Amount_Changed as varchar(50))+' Tk. '+ ISNULL('Details: '+@IN_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountIN_Date,'In','Up')
	END

	if(@AccountIN_Amount_Changed < 0)
	BEGIN
	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID , ABS(@AccountIN_Amount_Changed), 'Subtraction',
	@IN_Details, 'Updated Deposit','Updated Deposit','Updated Deposit', 'Deposit Amount Changed. Previous Amount Was '+ cast(@AccountIN_Amount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@AccountIN_Amount_I as varchar(50))+' Tk. Total Decreased Amount '+cast(ABS(@AccountIN_Amount_Changed) as varchar(50))+' Tk. '+ ISNULL('Details: '+@IN_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountIN_Date,'In','Up')
	END
END
GO
PRINT N'Creating Trigger [dbo].[Tr_AccountOUT_Record_UPDATE]...';


GO

CREATE TRIGGER [dbo].[Tr_AccountOUT_Record_UPDATE]
   ON [dbo].[AccountOUT_Record]
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountOUT_Amount float 
	DECLARE @Out_Details nvarchar(256)  
    DECLARE @AccountOUT_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 

	DECLARE @AccountOUT_Amount_I float
	DECLARE @AccountOUT_Amount_D float
	DECLARE @AccountOUT_Amount_Changed float

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))
	SELECT  @AccountOUT_Amount_I = AccountOUT_Amount,   @SchoolID = SchoolID,@EducationYearID = EducationYearID,@Out_Details = Out_Details, @AccountOUT_Date = AccountOUT_Date,@AccountID = AccountID FROM INSERTED
	SELECT  @AccountOUT_Amount_D = AccountOUT_Amount, @SchoolID = SchoolID,@Out_Details = Out_Details,@AccountOUT_Date = AccountOUT_Date, @AccountID = AccountID  FROM DELETED

	SET @AccountOUT_Amount_Changed =  isnull(@AccountOUT_Amount_I,0) -  isnull(@AccountOUT_Amount_D,0) 
 
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_OUT += @AccountOUT_Amount_Changed WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	if(@AccountOUT_Amount_Changed > 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @AccountOUT_Amount_Changed, 'Subtraction',
   @Out_Details, 'Updated Withdraw','Updated Withdraw','Updated Withdraw', 'Withdrawal Amount Changed. Previous Amount Was '+ cast(@AccountOUT_Amount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@AccountOUT_Amount_I as varchar(50))+' Tk. Total Increased Amount '+cast(@AccountOUT_Amount_Changed as varchar(50))+' Tk. '+ ISNULL('Details: '+@Out_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountOUT_Date,'Ex','Up')
	END

	if(@AccountOUT_Amount_Changed < 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, ABS(@AccountOUT_Amount_Changed), 'Add',
	@Out_Details, 'Updated Withdraw','Updated Withdraw','Updated Withdraw', 'Withdrawal Amount Changed. Previous Amount Was '+ cast(@AccountOUT_Amount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@AccountOUT_Amount_I as varchar(50))+' Tk. Total Decreased Amount '+cast(ABS(@AccountOUT_Amount_Changed) as varchar(50))+' Tk. '+ ISNULL('Details: '+@Out_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountOUT_Date,'Ex','Up')
	END
END
GO
PRINT N'Creating Trigger [dbo].[Tr_AccountOUT_Record_INSERT]...';


GO

CREATE TRIGGER [dbo].[Tr_AccountOUT_Record_INSERT]
   ON [dbo].[AccountOUT_Record]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountOUT_Amount float 
	DECLARE @Out_Details nvarchar(500)  
    DECLARE @AccountOUT_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 

	SELECT  @AccountOUT_Amount = AccountOUT_Amount,@SchoolID = SchoolID,@RegistrationID = RegistrationID,@EducationYearID = EducationYearID,@Out_Details = Out_Details,@AccountOUT_Date = AccountOUT_Date,@AccountID = AccountID FROM INSERTED

	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID IS NOT NULL)
	BEGIN
	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	 UPDATE [Account] SET Total_OUT += @AccountOUT_Amount  WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @AccountOUT_Amount ,'Subtraction',
    @Out_Details, 'Withdraw','Withdraw','Withdraw','Account Withdrawal Amount '+ cast(@AccountOUT_Amount as varchar(50))+' Tk. '+ ISNULL('Details : '+@Out_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountOUT_Date,'Ex','In')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_AccountOUT_Record_DELETE]...';


GO
--   AccountOUT_Record
CREATE TRIGGER [dbo].[Tr_AccountOUT_Record_DELETE]
   ON [dbo].[AccountOUT_Record]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @AccountOUT_Amount float 
	DECLARE @Out_Details nvarchar(256)  
    DECLARE @AccountOUT_Date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))

	SELECT @SchoolID = SchoolID,@EducationYearID = EducationYearID,@AccountOUT_Amount = AccountOUT_Amount, @Out_Details = Out_Details, @AccountOUT_Date = AccountOUT_Date, @AccountID = AccountID FROM DELETED
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	   BEGIN
	      SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	      UPDATE [Account] SET Total_OUT -= @AccountOUT_Amount  WHERE (AccountID = @AccountID)
	      SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	   END
	ELSE
	   BEGIN
	      SET @Balance_After = 0
	      SET @Balance_Before = 0
	   END
	
	DECLARE @Name nvarchar(128)
	SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @AccountOUT_Amount ,'Add',
    @Out_Details, 'Deleted Withdraw','Deleted Withdraw','Deleted Withdraw', 'Withdrawal Amount Deleted '+ cast(@AccountOUT_Amount as varchar(50))+' Tk. '+ ISNULL('Details: '+@Out_Details,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@AccountOUT_Date,'Ex','De')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_CommitteePaymentRecord_INSERT]...';


GO

CREATE TRIGGER [dbo].[Tr_CommitteePaymentRecord_INSERT]
   ON [dbo].[CommitteePaymentRecord]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @CommitteePaymentRecordId int
	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @PaidAmount float  
	DECLARE	@CommitteeMoneyReceiptId int 
	DECLARE	@CommitteeDonationId int 


SELECT * Into #Temp_Table  FROM INSERTED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
SELECT Top 1 @CommitteePaymentRecordId = CommitteePaymentRecordId, @SchoolID = SchoolID, @RegistrationID = RegistrationID, @PaidAmount = PaidAmount, @CommitteeDonationId =CommitteeDonationId,
	@CommitteeMoneyReceiptId = CommitteeMoneyReceiptId FROM #Temp_Table
  -- Code in here-------------------------------------------------

  	--get @PaidDate, @AccountId &@EducationYearId
	DECLARE @PaidDate date
	DECLARE @AccountId int
	DECLARE @EducationYearId int
	DECLARE @CommitteeMoneyReceiptSn nvarchar(128)

	SELECT @CommitteeMoneyReceiptSn = CommitteeMoneyReceiptSn, @PaidDate = PaidDate, @AccountId = AccountId,  @EducationYearId = EducationYearId FROM CommitteeMoneyReceipt WHERE (CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)
	
	--get @Description 
	DECLARE @Description nvarchar(1024) 
	DECLARE @DonationCategory nvarchar(128) 
	DECLARE @MemberName nvarchar(128)
    SELECT [Description] from CommitteeDonation WHERE CommitteeDonationId = @CommitteeDonationId

	SELECT @Description = CommitteeDonation.[Description],@DonationCategory = CommitteeDonationCategory.DonationCategory,@MemberName = CommitteeMember.MemberName FROM  CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId WHERE (CommitteeDonation.CommitteeDonationId = @CommitteeDonationId)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Income += @PaidAmount WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearId, @PaidAmount ,'Add',
	@Description,'Committee Payment', 'Committee Payment', @DonationCategory ,'Receipt No: '+@CommitteeMoneyReceiptSn + '. Collected  '+@DonationCategory+' '+ cast(@PaidAmount as varchar(50))+' Tk. Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@PaidDate,'In','In')

  --Code in here -------------------------------------------------


   UPDATE CommitteeDonation SET PaidAmount =PaidAmount + @PaidAmount WHERE (CommitteeDonationId = @CommitteeDonationId)

   UPDATE CommitteeMoneyReceipt SET TotalAmount =TotalAmount + @PaidAmount WHERE (CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)

   Delete #Temp_Table Where CommitteePaymentRecordId = @CommitteePaymentRecordId
END
DROP TABLE #Temp_Table
  

END
GO
PRINT N'Creating Trigger [dbo].[Tr_CommitteePaymentRecord_DELETE]...';


GO
CREATE TRIGGER [dbo].[Tr_CommitteePaymentRecord_DELETE]
   ON [dbo].[CommitteePaymentRecord]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @CommitteePaymentRecordId int
	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Login_RegistrationID int
	DECLARE @Record_RegistrationID int
	DECLARE @PaidAmount float  
	DECLARE	@CommitteeMoneyReceiptId int 
	DECLARE	@CommitteeDonationId int 
 

	SELECT @Login_RegistrationID = convert(int,convert(varbinary(4),context_info()))

SELECT *  Into #Temp_Table  FROM DELETED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
	SELECT Top 1 @CommitteePaymentRecordId = CommitteePaymentRecordId, @SchoolID = SchoolID, @Record_RegistrationID = RegistrationID, @PaidAmount = PaidAmount, @CommitteeDonationId =CommitteeDonationId,
	@CommitteeMoneyReceiptId = CommitteeMoneyReceiptId FROM #Temp_Table
  -- Code in here-------------------------------------------------

  	--get @PaidDate, @AccountId &@EducationYearId
	DECLARE @PaidDate date
	DECLARE @AccountId int
	DECLARE @EducationYearId int
	DECLARE @CommitteeMoneyReceiptSn nvarchar(128)

	SELECT @CommitteeMoneyReceiptSn = CommitteeMoneyReceiptSn, @PaidDate = PaidDate, @AccountId = AccountId,  @EducationYearId = EducationYearId FROM CommitteeMoneyReceipt WHERE (CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)
	
	--get @Description 
	DECLARE @Description nvarchar(1024) 
	DECLARE @DonationCategory nvarchar(128) 
	DECLARE @MemberName nvarchar(128)
    SELECT [Description] from CommitteeDonation WHERE CommitteeDonationId = @CommitteeDonationId

	SELECT @Description = CommitteeDonation.[Description],@DonationCategory = CommitteeDonationCategory.DonationCategory,@MemberName = CommitteeMember.MemberName FROM  CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId WHERE (CommitteeDonation.CommitteeDonationId = @CommitteeDonationId)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	   BEGIN
		  	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	         UPDATE [Account] SET Deleted_Income += @PaidAmount WHERE (AccountID = @AccountID)
	         SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	   END
	ELSE
	   BEGIN
	      SET @Balance_After = 0
	      SET @Balance_Before = 0
	   END

    --Set RegistrationID
	if(@Login_RegistrationID is NULL OR @Login_RegistrationID = 0 )
	BEGIN
	SET @RegistrationID = @Record_RegistrationID
	END
	ELSE
	BEGIN
	SET @RegistrationID = @Login_RegistrationID
	END



	IF (@SchoolID is NOT null)
	  BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
         VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @PaidAmount ,'Subtraction', 
		 @Description,'Deleted Committee Payment', 'Deleted Committee Payment', @DonationCategory ,'Receipt No: ' + @CommitteeMoneyReceiptSn  + '. Deleted '+ @DonationCategory + ' ' + cast(@PaidAmount as varchar(50))+' Tk. Member '+ @MemberName + '. Operated By: '+ @Name,
     	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@PaidDate,'In','De')
	  END
--Code in here -------------------------------------------------
   UPDATE CommitteeDonation SET PaidAmount =PaidAmount - @PaidAmount WHERE (CommitteeDonationId = @CommitteeDonationId)

   UPDATE CommitteeMoneyReceipt SET TotalAmount =TotalAmount - @PaidAmount WHERE (CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)

   Delete #Temp_Table Where CommitteePaymentRecordId = @CommitteePaymentRecordId
END
DROP TABLE #Temp_Table

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Payorder_Records_DELETE]...';


GO
CREATE TRIGGER [dbo].[Tr_Employee_Payorder_Records_DELETE]
   ON [dbo].[Employee_Payorder_Records]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Amount float 
	DECLARE @Paid_For nvarchar(256)  
    DECLARE @Paid_date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int
	DECLARE @Employee_PayorderID int
	DECLARE @Employee_Payorder_RecordID int

SELECT *  Into #Temp_Table  FROM DELETED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
	SELECT Top 1 @Employee_Payorder_RecordID = Employee_Payorder_RecordID,  @Employee_PayorderID = Employee_PayorderID, @Amount = Amount,   @SchoolID = SchoolID ,@RegistrationID = RegistrationID,@EducationYearID =EducationYearID , @Paid_For = Paid_For,@Paid_date = Paid_date,@AccountID = AccountID FROM #Temp_Table
  -- Code in here-------------------------------------------------

	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	--Update Employee Pay order
	UPDATE Employee_Payorder SET PaidAmount -= @Amount WHERE (Employee_PayorderID = @Employee_PayorderID)

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Deleted_Expense += @Amount  WHERE (AccountID = @AccountID)
	SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	DECLARE @CategoryName nvarchar(128)

    SELECT @CategoryName = Employee_Payorder_Name.Payorder_Name FROM Employee_Payorder INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID WHERE (Employee_Payorder.Employee_PayorderID = @Employee_PayorderID)
	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For, MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID , @Amount ,'Add',
   @Paid_For,'Deleted Employee Payment','Deleted Employee Payment', @CategoryName, 'Employee Payment Deleted '+ cast(@Amount as varchar(50))+' Tk. '+ ISNULL('Details: '+@Paid_For,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Paid_Date,'Ex','De')


	--Code in here -------------------------------------------------
   Delete #Temp_Table Where Employee_Payorder_RecordID = @Employee_Payorder_RecordID
END
DROP TABLE #Temp_Table

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Employee_Payorder_Records_Insert]...';


GO
CREATE TRIGGER [dbo].[Tr_Employee_Payorder_Records_Insert]
   ON [dbo].[Employee_Payorder_Records]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Amount float 
	DECLARE @Paid_For nvarchar(256)  
    DECLARE @Paid_date date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int
	DECLARE @Employee_PayorderID int
	

	SELECT @Employee_PayorderID = Employee_PayorderID, @Amount = Amount,   @SchoolID = SchoolID ,@RegistrationID = RegistrationID,@EducationYearID =EducationYearID , @Paid_For = Paid_For,@Paid_date = Paid_date,@AccountID = AccountID FROM INSERTED

	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Expense += @Amount  WHERE (AccountID = @AccountID)
	SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	DECLARE @CategoryName nvarchar(128)

    SELECT @CategoryName = Employee_Payorder_Name.Payorder_Name FROM Employee_Payorder INNER JOIN Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID WHERE (Employee_Payorder.Employee_PayorderID = @Employee_PayorderID)
	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID , @Amount ,'Subtraction',
    @Paid_For,'Employee Payment','Employee Payment', @CategoryName, 'Employee Payment inputted '+ cast(@Amount as varchar(50))+' Tk. '+ ISNULL('Details: '+@Paid_For,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Paid_Date,'Ex','In')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Expenditure_UPDATE]...';


GO
CREATE TRIGGER [dbo].[Tr_Expenditure_UPDATE]
   ON [dbo].[Expenditure]
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @ExpenseAmount float 
	DECLARE @ExpenseFor nvarchar(256)  
    DECLARE @ExpenseDate date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int 
	DECLARE @ExpenseCategoryID int

	DECLARE @ExpenseAmount_I float
	DECLARE @ExpenseAmount_D float
	DECLARE @ExpenseAmount_Changed float

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))

	SELECT @ExpenseCategoryID = ExpenseCategoryID,  @ExpenseAmount_I = Amount,    @SchoolID = SchoolID ,@EducationYearID =EducationYearID, @ExpenseFor = ExpenseFor,@ExpenseDate = ExpenseDate,@AccountID = AccountID FROM INSERTED

	SELECT @ExpenseCategoryID = ExpenseCategoryID, @ExpenseAmount_D = Amount,   @SchoolID = SchoolID ,@EducationYearID =EducationYearID, @ExpenseFor = ExpenseFor,@ExpenseDate = ExpenseDate,@AccountID = AccountID  FROM DELETED

	SET @ExpenseAmount_Changed =  isnull(@ExpenseAmount_I,0) -  isnull(@ExpenseAmount_D,0) 
    
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Expense += @ExpenseAmount_Changed  WHERE (AccountID = @AccountID)
	SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	

	DECLARE @CategoryName nvarchar(128)
	SELECT @CategoryName = CategoryName FROM Expense_CategoryName WHERE (ExpenseCategoryID = @ExpenseCategoryID)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	if(@ExpenseAmount_Changed > 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @ExpenseAmount_Changed, 'Subtraction', 
    @ExpenseFor, 'Updated Expense','Updated Expense', @CategoryName, 'Expense Amount Changed. Previous Amount Was '+ cast(@ExpenseAmount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@ExpenseAmount_I as varchar(50))+' Tk. Total Increased Amount '+cast(@ExpenseAmount_Changed as varchar(50))+' Tk. '+ ISNULL('Expense Reason: '+@ExpenseFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@ExpenseDate,'Ex','Up')
	END

	if(@ExpenseAmount_Changed < 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, ABS(@ExpenseAmount_Changed), 'Add',
	@ExpenseFor, 'Updated Expense','Updated Expense', @CategoryName,'Expense Amount Changed. Previous Amount Was '+ cast(@ExpenseAmount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@ExpenseAmount_I as varchar(50))+' Tk. Total Decreased Amount '+cast(ABS(@ExpenseAmount_Changed) as varchar(50))+' Tk. '+ ISNULL('Expense Reason: '+@ExpenseFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@ExpenseDate,'Ex','Up')
	END
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Expenditure_DELETE]...';


GO

CREATE TRIGGER [dbo].[Tr_Expenditure_DELETE]
   ON [dbo].[Expenditure]
   AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SchoolID int
    DECLARE @RegistrationID int
    DECLARE @OperatorRegistrationID int
    DECLARE @ExpanseAmount float
    DECLARE @ExpenseFor nvarchar(256)
    DECLARE @ExpenseDate date
    DECLARE @AccountID int
    DECLARE @EducationYearID int
    DECLARE @ExpenseCategoryID int

    SELECT
        @ExpenseCategoryID = ExpenseCategoryID,
        @ExpanseAmount = Amount,
        @SchoolID = SchoolID,
        @RegistrationID = RegistrationID,
        @EducationYearID = EducationYearID,
        @ExpenseFor = ExpenseFor,
        @ExpenseDate = ExpenseDate,
        @AccountID = AccountID
    FROM DELETED

    IF CONTEXT_INFO() IS NOT NULL AND DATALENGTH(CONTEXT_INFO()) >= 4
        SET @OperatorRegistrationID = CAST(SUBSTRING(CONTEXT_INFO(), 1, 4) AS INT)

    IF ISNULL(@OperatorRegistrationID, 0) = 0
        SET @OperatorRegistrationID = @RegistrationID

    DECLARE @Balance_Before float
    DECLARE @Balance_After float

    IF (@AccountID IS NOT NULL)
    BEGIN
        SELECT @Balance_Before = AccountBalance FROM [Account] WHERE (AccountID = @AccountID)
        UPDATE [Account] SET Deleted_Expense += @ExpanseAmount WHERE (AccountID = @AccountID)
        SELECT @Balance_After = AccountBalance FROM [Account] WHERE (AccountID = @AccountID)
    END
    ELSE
    BEGIN
        SET @Balance_After = 0
        SET @Balance_Before = 0
    END

    DECLARE @CategoryName nvarchar(128)
    SELECT @CategoryName = CategoryName FROM Expense_CategoryName WHERE (ExpenseCategoryID = @ExpenseCategoryID)

    DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName + ' ' + LastName FROM Admin WHERE (RegistrationID = @OperatorRegistrationID)

    INSERT INTO Account_Log (
        AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,
        MainCategory, ClassOrOtherCategory, SubCategory, Details, Log_SN, Balance_Before, Balance_After,
        Activity_Date, In_Ex_type, Insert_Up_De)
    VALUES (
        @AccountID, @SchoolID, @OperatorRegistrationID, @EducationYearID, @ExpanseAmount, 'Add',
        @ExpenseFor, 'Deleted Expense', 'Deleted Expense', @CategoryName,
        'Expense Amount Deleted ' + CAST(@ExpanseAmount AS varchar(50)) + ' Tk. '
            + ISNULL('Expense Reason: ' + @ExpenseFor, '')
            + ' Operated By: ' + @Name + ' ID = ' + CAST(@OperatorRegistrationID AS varchar(20)),
        [dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After, @ExpenseDate, 'Ex', 'De')
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Expenditure_Insert]...';


GO
CREATE TRIGGER [dbo].[Tr_Expenditure_Insert]
   ON [dbo].[Expenditure]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Amount float 
	DECLARE @ExpenseFor nvarchar(256)  
    DECLARE @ExpenseDate date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int
	DECLARE @ExpenseCategoryID int
	

	SELECT @ExpenseCategoryID = ExpenseCategoryID, @Amount = Amount,   @SchoolID = SchoolID ,@RegistrationID = RegistrationID,@EducationYearID =EducationYearID , @ExpenseFor = ExpenseFor,@ExpenseDate = ExpenseDate,@AccountID = AccountID FROM INSERTED

	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Expense += @Amount  WHERE (AccountID = @AccountID)
	SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	DECLARE @CategoryName nvarchar(128)
	SELECT @CategoryName = CategoryName FROM Expense_CategoryName WHERE (ExpenseCategoryID = @ExpenseCategoryID)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID , @Amount ,'Subtraction',
   @ExpenseFor,'Expense','Expense',@CategoryName, 'Expense Amount inputted '+ cast(@Amount as varchar(50))+' Tk. '+ ISNULL('Expense Reason: '+@ExpenseFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@ExpenseDate,'Ex','In')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Extra_Income_UPDATE]...';


GO
CREATE TRIGGER [dbo].[Tr_Extra_Income_UPDATE]
   ON dbo.Extra_Income
   AFTER UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @Extra_IncomeAmount float 
	DECLARE @Extra_IncomeFor nvarchar(256)   
    DECLARE @Extra_IncomeDate date 
	DECLARE @AccountID int 
	DECLARE @EducationYearID int


	DECLARE @Extra_IncomeAmount_I float
	DECLARE @Extra_IncomeAmount_D float
	DECLARE @Extra_IncomeAmount_Changed float

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))

	SELECT  @EducationYearID= EducationYearID, @Extra_IncomeAmount_I = Extra_IncomeAmount,   @SchoolID = SchoolID , @Extra_IncomeCategoryID = Extra_IncomeCategoryID,
	 @Extra_IncomeFor = Extra_IncomeFor, @Extra_IncomeDate = Extra_IncomeDate,@AccountID = AccountID FROM INSERTED

	SELECT @EducationYearID = EducationYearID, @Extra_IncomeAmount_D = Extra_IncomeAmount, @SchoolID = SchoolID ,@Extra_IncomeCategoryID = Extra_IncomeCategoryID,
	 @Extra_IncomeFor = Extra_IncomeFor,@Extra_IncomeDate = Extra_IncomeDate, @AccountID = AccountID  FROM DELETED

	SET @Extra_IncomeAmount_Changed =  isnull(@Extra_IncomeAmount_I,0) -  isnull(@Extra_IncomeAmount_D,0) 
    

    UPDATE [Extra_IncomeCategory] SET Total_Extra_Income += @Extra_IncomeAmount_Changed WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Income += @Extra_IncomeAmount_Changed WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	   	DECLARE @Extra_Income_CategoryName nvarchar(128)
	SELECT @Extra_Income_CategoryName = Extra_Income_CategoryName FROM Extra_IncomeCategory WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
    DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

	if(@Extra_IncomeAmount_Changed > 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @Extra_IncomeAmount_Changed, 'Add', 
	@Extra_IncomeFor,'Updated Other Income','Updated Other Income',@Extra_Income_CategoryName , 'Others payment Changed. Previous Amount Was '+ cast(@Extra_IncomeAmount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@Extra_IncomeAmount_I as varchar(50))+' Tk. Total Increased Amount '+cast(@Extra_IncomeAmount_Changed as varchar(50))+' Tk. '+ ISNULL('Payment For: '+@Extra_IncomeFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Extra_IncomeDate,'In','Up')
	END

	if(@Extra_IncomeAmount_Changed < 0)
	BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, ABS(@Extra_IncomeAmount_Changed), 'Subtraction', 
    @Extra_IncomeFor,'Updated Other Income','Updated Other Income',@Extra_Income_CategoryName , 'Others payment Changed. Previous Amount Was '+ cast(@Extra_IncomeAmount_D as varchar(50)) +' Tk. Updated Amount Is '+ cast(@Extra_IncomeAmount_I as varchar(50))+' Tk. Total Decreased Amount '+cast(ABS(@Extra_IncomeAmount_Changed) as varchar(50))+' Tk. '+ ISNULL('Payment For: '+@Extra_IncomeFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Extra_IncomeDate,'In','Up')
	END
END
GO
PRINT N'Creating Trigger [dbo].[Tr_Extra_Income_INSERT]...';


GO
CREATE TRIGGER [dbo].[Tr_Extra_Income_INSERT]
   ON dbo.Extra_Income
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @Extra_IncomeAmount float 
	DECLARE @Extra_IncomeFor nvarchar(256)  

    DECLARE @Extra_IncomeDate date 
	DECLARE @AccountID int 

	DECLARE @EducationYearID int

	SELECT @EducationYearID = EducationYearID, @Extra_IncomeAmount = Extra_IncomeAmount,   @SchoolID = SchoolID ,@RegistrationID = RegistrationID,@Extra_IncomeCategoryID = Extra_IncomeCategoryID,
	 @Extra_IncomeFor = Extra_IncomeFor,@Extra_IncomeDate = Extra_IncomeDate,@AccountID = AccountID FROM INSERTED

    UPDATE [Extra_IncomeCategory] SET  Total_Extra_Income += @Extra_IncomeAmount  WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	 UPDATE [Account] SET Total_Income += @Extra_IncomeAmount  WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END
	
	   	DECLARE @Extra_Income_CategoryName nvarchar(128)
	SELECT @Extra_Income_CategoryName = Extra_Income_CategoryName FROM Extra_IncomeCategory WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
    DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID, @EducationYearID, @Extra_IncomeAmount ,'Add',
	@Extra_IncomeFor,'Other Income','Other Income',@Extra_Income_CategoryName , 'Others payment inputted '+ cast(@Extra_IncomeAmount as varchar(50))+' Tk. '+ ISNULL('Payment For : '+@Extra_IncomeFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Extra_IncomeDate,'In','In')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Extra_Income_DELETE]...';


GO
--Extra_Income
CREATE TRIGGER [dbo].[Tr_Extra_Income_DELETE]
   ON dbo.Extra_Income
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Extra_IncomeCategoryID int
	DECLARE @Extra_IncomeAmount float 
	DECLARE @Extra_IncomeFor nvarchar(256)  
    DECLARE @Extra_IncomeDate date 
	DECLARE @EducationYearID int
	DECLARE @AccountID int 

	SELECT @RegistrationID = convert(int,convert(varbinary(4),context_info()))

	SELECT @EducationYearID = EducationYearID, @SchoolID = SchoolID , @Extra_IncomeCategoryID = Extra_IncomeCategoryID,@Extra_IncomeAmount = Extra_IncomeAmount, @Extra_IncomeFor = Extra_IncomeFor, @Extra_IncomeDate = Extra_IncomeDate, @AccountID = AccountID FROM DELETED

    UPDATE [Extra_IncomeCategory] SET  Total_Extra_Income -= @Extra_IncomeAmount  WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	   BEGIN
	      SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	      UPDATE [Account] SET Deleted_Income += @Extra_IncomeAmount  WHERE (AccountID = @AccountID)
	      SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	   END
	ELSE
	   BEGIN
	      SET @Balance_After = 0
	      SET @Balance_Before = 0
	   END

	   	DECLARE @Extra_Income_CategoryName nvarchar(128)
	SELECT @Extra_Income_CategoryName = Extra_Income_CategoryName FROM Extra_IncomeCategory WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)
	
    DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @Extra_IncomeAmount ,'Subtraction', 
	@Extra_IncomeFor,'Deleted Other Income','Deleted Other Income',@Extra_Income_CategoryName , 'Others payment Deleted '+ cast(@Extra_IncomeAmount as varchar(50))+' Tk. '+ ISNULL('Payment For: '+@Extra_IncomeFor,'')+' Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@Extra_IncomeDate,'In','De')

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Income_PaymentRecord_DELETE]...';


GO
CREATE TRIGGER [dbo].[Tr_Income_PaymentRecord_DELETE]
   ON [dbo].[Income_PaymentRecord]
   AFTER DELETE
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @PaymentRecordID int
	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @Login_RegistrationID int
	DECLARE @Record_RegistrationID int
	DECLARE @PaidAmount float  
	DECLARE @StudentClassID int 
    DECLARE @PaidDate date 
	DECLARE @AccountID int 
    DECLARE	@EducationYearID int 
	DECLARE	@MoneyReceiptID int 
	DECLARE	@RoleID int 
	DECLARE	@PayFor nvarchar(50) 

    DECLARE	@StudentID int 
	DECLARE	@ID nvarchar(50) 

	SELECT @Login_RegistrationID = convert(int,convert(varbinary(4),context_info()))

SELECT *  Into #Temp_Table  FROM DELETED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
	SELECT Top 1 @PaymentRecordID = PaymentRecordID, @SchoolID = SchoolID, @StudentID = StudentID,  @EducationYearID= EducationYearID, @Record_RegistrationID = RegistrationID,@PaidAmount = PaidAmount,@StudentClassID = StudentClassID,@RoleID =RoleID, @PayFor =PayFor,
	@PaidDate= PaidDate,@AccountID = AccountID,@MoneyReceiptID =MoneyReceiptID FROM #Temp_Table
  -- Code in here-------------------------------------------------
	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	   BEGIN
		  	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	         UPDATE [Account] SET Deleted_Income += @PaidAmount WHERE (AccountID = @AccountID)
	         SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	   END
	ELSE
	   BEGIN
	      SET @Balance_After = 0
	      SET @Balance_Before = 0
	   END

    --Set RegistrationID
	if(@Login_RegistrationID is NULL OR @Login_RegistrationID = 0 )
	BEGIN
	SET @RegistrationID = @Record_RegistrationID
	END
	ELSE
	BEGIN
	SET @RegistrationID = @Login_RegistrationID
	END
	--get ID
	SELECT @ID = ID FROM Student WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID)
	--get class 
	DECLARE @Category_Class nvarchar(50) 
    
	SELECT @Category_Class = CreateClass.Class  FROM  StudentsClass INNER JOIN  CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE  (StudentsClass.StudentClassID = @StudentClassID)

	-- get Paymant Role
	DECLARE @Situation_Role nvarchar(50) 
	SELECT  @Situation_Role = [Role] FROM Income_Roles WHERE (RoleID = @RoleID)



	DECLARE @MoneyReceipt_SN nvarchar(128)
	SELECT @MoneyReceipt_SN = MoneyReceipt_SN FROM Income_MoneyReceipt WHERE (MoneyReceiptID = @MoneyReceiptID)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)


	IF (@SchoolID is NOT null)
	  BEGIN
INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
         VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @PaidAmount ,'Subtraction', 
		 @PayFor,'Deleted Student Payment', @Category_Class, @Situation_Role ,'Receipt No: ' + @MoneyReceipt_SN  + '. Deleted '+ @Situation_Role + ' ' + cast(@PaidAmount as varchar(50))+' Tk. ID = '+ @ID + '. Operated By: '+ @Name,
     	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@PaidDate,'In','De')
	  END
--Code in here -------------------------------------------------
   Delete #Temp_Table Where PaymentRecordID = @PaymentRecordID
END
DROP TABLE #Temp_Table

END
GO
PRINT N'Creating Trigger [dbo].[Tr_Income_PaymentRecord_INSERT]...';


GO

CREATE TRIGGER [dbo].[Tr_Income_PaymentRecord_INSERT]
   ON [dbo].[Income_PaymentRecord]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;

	DECLARE @PaymentRecordID int
	DECLARE @SchoolID int
	DECLARE @RegistrationID int
	DECLARE @PaidAmount float  
	DECLARE @StudentClassID int 
    DECLARE @PaidDate date 
	DECLARE @AccountID int 
    DECLARE	@EducationYearID int 
	DECLARE	@MoneyReceiptID int 
	DECLARE	@RoleID int 
	DECLARE	@PayFor nvarchar(50) 
	


SELECT *  Into #Temp_Table  FROM INSERTED
--loop start ------------------
While EXISTS(SELECT * From #Temp_Table)
Begin
	SELECT Top 1 @PaymentRecordID = PaymentRecordID, @SchoolID = SchoolID, @EducationYearID= EducationYearID, @RegistrationID = RegistrationID,@PaidAmount = PaidAmount,@StudentClassID = StudentClassID,@RoleID =RoleID, @PayFor =PayFor,
	@PaidDate= PaidDate,@AccountID = AccountID,@MoneyReceiptID =MoneyReceiptID FROM #Temp_Table
  -- Code in here-------------------------------------------------


	DECLARE @Balance_Before float 
	DECLARE @Balance_After float 

	IF (@AccountID is NOT null)
	BEGIN
	 SELECT @Balance_Before = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	UPDATE [Account] SET Total_Income += @PaidAmount WHERE (AccountID = @AccountID)
	 SELECT @Balance_After = AccountBalance from [Account] WHERE (AccountID = @AccountID)
	END
	ELSE
	BEGIN
	 SET @Balance_After = 0
	 SET @Balance_Before = 0
	END


	--get class 
	DECLARE @Category_Class nvarchar(50) 
    
	SELECT @Category_Class = CreateClass.Class  FROM  StudentsClass INNER JOIN  CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE  (StudentsClass.StudentClassID = @StudentClassID)

	-- get Paymant Role
	DECLARE @Situation_Role nvarchar(50) 
	SELECT  @Situation_Role = [Role] FROM Income_Roles WHERE (RoleID = @RoleID)





	DECLARE @MoneyReceipt_SN nvarchar(128)
	SELECT @MoneyReceipt_SN = MoneyReceipt_SN FROM Income_MoneyReceipt WHERE (MoneyReceiptID = @MoneyReceiptID)

	DECLARE @Name nvarchar(128)
    SELECT @Name = FirstName +' '+ LastName FROM Admin WHERE (RegistrationID = @RegistrationID)

INSERT INTO Account_Log (AccountID, SchoolID, RegistrationID, EducationYearID, Amount, Add_Subtraction, Pay_For,MainCategory,ClassOrOtherCategory,SubCategory, Details, Log_SN, Balance_Before, Balance_After, Activity_Date,In_Ex_type,Insert_Up_De)
    VALUES (@AccountID,@SchoolID, @RegistrationID,@EducationYearID, @PaidAmount ,'Add',
	@PayFor,'Student Payment', @Category_Class, @Situation_Role ,'Receipt No: '+@MoneyReceipt_SN + '. Collected  '+@Situation_Role+' '+ cast(@PaidAmount as varchar(50))+' Tk. Operated By: '+ @Name,
	[dbo].[Account_Log_SerialNumber](@SchoolID), @Balance_Before, @Balance_After,@PaidDate,'In','In')

  --Code in here -------------------------------------------------
   Delete #Temp_Table Where PaymentRecordID = @PaymentRecordID
END
DROP TABLE #Temp_Table
  

END
GO
PRINT N'Creating Function [dbo].[In_Function_Parameter]...';


GO

CREATE FUNCTION [dbo].[In_Function_Parameter] (@InParameter VARCHAR(MAX))
RETURNS @TempTab TABLE
   (SN INT,id VARCHAR(128) not null)
AS
BEGIN
	SET @InParameter = REPLACE(@InParameter + ',', ',,', ',')
	DECLARE @SP INT
    DECLARE @SN INT = 0
DECLARE @VALUE VARCHAR(1000)
WHILE PATINDEX('%,%', @InParameter ) <> 0 
BEGIN
   SELECT  @SP = PATINDEX('%,%',@InParameter)
   SELECT  @VALUE = LEFT(@InParameter , @SP - 1)
   SELECT  @InParameter = STUFF(@InParameter, 1, @SP, '')

   SET @SN = @SN + 1
   INSERT INTO @TempTab(SN,id) VALUES (@SN,@VALUE)
END
	RETURN
END
GO
PRINT N'Creating Procedure [dbo].[AAP_Auto_Generate_Monthly_Invoice]...';


GO
-- Fix: AAP_Auto_Generate_Monthly_Invoice - Add committee billing count
-- Problem: Auto invoice was not including committee members in billing calculation
-- The manual process (Monthly_Button_Click) correctly adds committee count,
-- but this stored procedure was missing it.

CREATE PROCEDURE [dbo].[AAP_Auto_Generate_Monthly_Invoice]
    @TargetMonth DATE = NULL,
    @RegistrationID INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MonthName NVARCHAR(50);
    DECLARE @MonthDate DATE;
    DECLARE @IssueDate DATE;
    DECLARE @EndDate DATE;

    IF @TargetMonth IS NULL
        SET @MonthDate = EOMONTH(DATEADD(MONTH, -1, GETDATE()));
    ELSE
        SET @MonthDate = EOMONTH(@TargetMonth);

    SET @MonthName = FORMAT(@MonthDate, 'MMM yyyy');
    SET @IssueDate = DATEADD(DAY, 1, @MonthDate);
    SET @EndDate = DATEADD(DAY, 15, @IssueDate);

    PRINT 'Generating invoices for: ' + @MonthName;
    PRINT 'Issue Date: ' + CONVERT(NVARCHAR, @IssueDate, 106);
    PRINT 'End Date: ' + CONVERT(NVARCHAR, @EndDate, 106);

    DECLARE @SchoolID INT;
    DECLARE @SchoolName NVARCHAR(200);
    DECLARE @StudentCount INT;
    DECLARE @ActiveStudent INT;
    DECLARE @CommitteeCount INT;
    DECLARE @BillableCount INT;
    DECLARE @PerStudentRate FLOAT;
    DECLARE @Discount FLOAT;
    DECLARE @Fixed FLOAT;
    DECLARE @IS_ServiceChargeActive BIT;
    DECLARE @TotalAmount FLOAT;
    DECLARE @InvoiceCategoryID INT;
    DECLARE @InvoiceExists INT;

    SELECT @InvoiceCategoryID = InvoiceCategoryID 
    FROM AAP_Invoice_Category 
    WHERE InvoiceCategory = N'Service Charge';

    IF @InvoiceCategoryID IS NULL
    BEGIN
        PRINT 'Error: Service Charge category not found!';
        RETURN;
    END

    DECLARE school_cursor CURSOR FOR
    SELECT 
        si.SchoolID,
        si.SchoolName,
        si.Per_Student_Rate,
        si.IS_ServiceChargeActive,
        ISNULL(si.Discount, 0) AS Discount,
        ISNULL(si.Fixed, 0) AS Fixed,
        scm.StudentCount,
        scm.Active_Student
    FROM SchoolInfo si
    INNER JOIN AAP_Student_Count_Monthly scm ON si.SchoolID = scm.SchoolID
    WHERE FORMAT(scm.Month, 'MMM yyyy') = @MonthName
        AND si.IS_ServiceChargeActive = 1;

    OPEN school_cursor;
    FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;

    DECLARE @SuccessCount INT = 0;
    DECLARE @SkipCount INT = 0;
    DECLARE @ErrorCount INT = 0;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            -- Get committee billable count for this school (if committee billing is enabled)
            SET @CommitteeCount = dbo.fn_GetBillableCommitteeCount(@SchoolID);
            -- Total billable = students + committee members (committee = 0 if option disabled)
            SET @BillableCount = @StudentCount + @CommitteeCount;

            SELECT @InvoiceExists = COUNT(*)
            FROM AAP_Invoice
            WHERE SchoolID = @SchoolID
                AND InvoiceCategoryID = @InvoiceCategoryID
                AND FORMAT(MonthName, 'MMM yyyy') = @MonthName;

            IF @InvoiceExists > 0
            BEGIN
                PRINT 'Invoice already exists for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + ' (' + @SchoolName + ')';
                SET @SkipCount = @SkipCount + 1;
            END
            ELSE
            BEGIN
                IF @Fixed > 0
                    SET @TotalAmount = @Fixed;
                ELSE
                    SET @TotalAmount = @BillableCount * @PerStudentRate;

                INSERT INTO AAP_Invoice (
                    RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate,
                    Invoice_For, TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice
                )
                VALUES (
                    @RegistrationID,
                    @InvoiceCategoryID,
                    @SchoolID,
                    @IssueDate,
                    @EndDate,
                    @MonthName,
                    @TotalAmount,
                    @Discount,
                    @MonthDate,
                    dbo.Invoice_SerialNumber(@SchoolID),
                    @BillableCount,
                    CASE WHEN @Fixed > 0 THEN NULL ELSE @PerStudentRate END
                );

                SET @SuccessCount = @SuccessCount + 1;
                PRINT 'Invoice created for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + 
                      ' (' + @SchoolName + ') - Amount: ' + CAST(@TotalAmount AS NVARCHAR(20)) +
                      ' (Students: ' + CAST(@StudentCount AS NVARCHAR(10)) + 
                      ', Committee: ' + CAST(@CommitteeCount AS NVARCHAR(10)) + ')';
            END
        END TRY
        BEGIN CATCH
            SET @ErrorCount = @ErrorCount + 1;
            PRINT 'Error for School ID: ' + CAST(@SchoolID AS NVARCHAR(10)) + ' - ' + ERROR_MESSAGE();
        END CATCH

        FETCH NEXT FROM school_cursor INTO @SchoolID, @SchoolName, @PerStudentRate, @IS_ServiceChargeActive, @Discount, @Fixed, @StudentCount, @ActiveStudent;
    END

    CLOSE school_cursor;
    DEALLOCATE school_cursor;

    PRINT '';
    PRINT '========================================';
    PRINT 'Invoice Generation Summary for ' + @MonthName;
    PRINT '========================================';
    PRINT 'Successfully Created: ' + CAST(@SuccessCount AS NVARCHAR(10));
    PRINT 'Already Exists (Skipped): ' + CAST(@SkipCount AS NVARCHAR(10));
    PRINT 'Errors: ' + CAST(@ErrorCount AS NVARCHAR(10));
    PRINT '========================================';
END
GO
PRINT N'Creating Procedure [dbo].[AAP_Student_Count_Monthly_Insert]...';


GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Insert monthly student count data with optional month parameter
-- Parameters:	@TargetMonth - Optional. If NULL, uses last day of CURRENT month
-- Schedule:    Runs on 28th of every month to insert that month's data
-- =============================================
CREATE PROCEDURE [dbo].[AAP_Student_Count_Monthly_Insert]
    @TargetMonth DATE = NULL  -- Optional parameter for specific month
AS
BEGIN
    SET NOCOUNT ON;
    
    -- If no month specified, use last day of CURRENT month (not previous)
    IF @TargetMonth IS NULL
    BEGIN
        SET @TargetMonth = EOMONTH(GETDATE());  -- Current month এর শেষ দিন
    END
    ELSE
    BEGIN
        -- Ensure we use the last day of the specified month
        SET @TargetMonth = EOMONTH(@TargetMonth);
    END
    
    -- Check if data already exists for this month
    IF EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly WHERE Month = @TargetMonth)
    BEGIN
        PRINT 'Data for ' + FORMAT(@TargetMonth, 'MMMM yyyy') + ' already exists. Skipping insert.';
        RETURN;
    END
    
    -- Insert StudentClass data with specified month
    INSERT INTO AAP_StudentClass_Count_Monthly
        (SchoolID, EducationYearID, ClassID, Active_Student, Reject_Countable, Reject_Uncountable, Month)
    SELECT 
        SchoolID, 
        EducationYearID, 
        ClassID, 
        ActiveStudent, 
        Reject_Countable, 
        Reject_Uncountable,
        @TargetMonth AS Month
    FROM VW_Payment_Monthly_StudentClass;
    
    -- Insert Student data with specified month
    INSERT INTO AAP_Student_Count_Monthly
        (SchoolID, Active_Student, Reject_Countable, Reject_Uncountable, Month)
    SELECT 
        SchoolID, 
        ActiveStudent, 
        Reject_Countable, 
        Reject_Uncountable,
        @TargetMonth AS Month
    FROM VW_Payment_Monthly_Stu;
    
    PRINT 'Successfully inserted data for ' + FORMAT(@TargetMonth, 'MMMM yyyy');
END
GO
PRINT N'Creating Procedure [dbo].[aspnet_AnyDataInTables]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE [dbo].aspnet_AnyDataInTables
    @TablesToCheck int
AS
BEGIN
    -- Check Membership table if (@TablesToCheck & 1) is set
    IF ((@TablesToCheck & 1) <> 0 AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_MembershipUsers') AND (type = 'V'))))
    BEGIN
        IF (EXISTS(SELECT TOP 1 UserId FROM dbo.aspnet_Membership))
        BEGIN
            SELECT N'aspnet_Membership'
            RETURN
        END
    END

    -- Check aspnet_Roles table if (@TablesToCheck & 2) is set
    IF ((@TablesToCheck & 2) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_Roles') AND (type = 'V'))) )
    BEGIN
        IF (EXISTS(SELECT TOP 1 RoleId FROM dbo.aspnet_Roles))
        BEGIN
            SELECT N'aspnet_Roles'
            RETURN
        END
    END

    -- Check aspnet_Profile table if (@TablesToCheck & 4) is set
    IF ((@TablesToCheck & 4) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_Profiles') AND (type = 'V'))) )
    BEGIN
        IF (EXISTS(SELECT TOP 1 UserId FROM dbo.aspnet_Profile))
        BEGIN
            SELECT N'aspnet_Profile'
            RETURN
        END
    END

    -- Check aspnet_PersonalizationPerUser table if (@TablesToCheck & 8) is set
    IF ((@TablesToCheck & 8) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_WebPartState_User') AND (type = 'V'))) )
    BEGIN
        IF (EXISTS(SELECT TOP 1 UserId FROM dbo.aspnet_PersonalizationPerUser))
        BEGIN
            SELECT N'aspnet_PersonalizationPerUser'
            RETURN
        END
    END

    -- Check aspnet_PersonalizationPerUser table if (@TablesToCheck & 16) is set
    IF ((@TablesToCheck & 16) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'aspnet_WebEvent_LogEvent') AND (type = 'P'))) )
    BEGIN
        IF (EXISTS(SELECT TOP 1 * FROM dbo.aspnet_WebEvent_Events))
        BEGIN
            SELECT N'aspnet_WebEvent_Events'
            RETURN
        END
    END

    -- Check aspnet_Users table if (@TablesToCheck & 1,2,4 & 8) are all set
    IF ((@TablesToCheck & 1) <> 0 AND
        (@TablesToCheck & 2) <> 0 AND
        (@TablesToCheck & 4) <> 0 AND
        (@TablesToCheck & 8) <> 0 AND
        (@TablesToCheck & 32) <> 0 AND
        (@TablesToCheck & 128) <> 0 AND
        (@TablesToCheck & 256) <> 0 AND
        (@TablesToCheck & 512) <> 0 AND
        (@TablesToCheck & 1024) <> 0)
    BEGIN
        IF (EXISTS(SELECT TOP 1 UserId FROM dbo.aspnet_Users))
        BEGIN
            SELECT N'aspnet_Users'
            RETURN
        END
        IF (EXISTS(SELECT TOP 1 ApplicationId FROM dbo.aspnet_Applications))
        BEGIN
            SELECT N'aspnet_Applications'
            RETURN
        END
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Applications_CreateApplication]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_Applications_CreateApplication
    @ApplicationName      nvarchar(256),
    @ApplicationId        uniqueidentifier OUTPUT
AS
BEGIN
    SELECT  @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName

    IF(@ApplicationId IS NULL)
    BEGIN
        DECLARE @TranStarted   bit
        SET @TranStarted = 0

        IF( @@TRANCOUNT = 0 )
        BEGIN
	        BEGIN TRANSACTION
	        SET @TranStarted = 1
        END
        ELSE
    	    SET @TranStarted = 0

        SELECT  @ApplicationId = ApplicationId
        FROM dbo.aspnet_Applications WITH (UPDLOCK, HOLDLOCK)
        WHERE LOWER(@ApplicationName) = LoweredApplicationName

        IF(@ApplicationId IS NULL)
        BEGIN
            SELECT  @ApplicationId = NEWID()
            INSERT  dbo.aspnet_Applications (ApplicationId, ApplicationName, LoweredApplicationName)
            VALUES  (@ApplicationId, @ApplicationName, LOWER(@ApplicationName))
        END


        IF( @TranStarted = 1 )
        BEGIN
            IF(@@ERROR = 0)
            BEGIN
	        SET @TranStarted = 0
	        COMMIT TRANSACTION
            END
            ELSE
            BEGIN
                SET @TranStarted = 0
                ROLLBACK TRANSACTION
            END
        END
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_CheckSchemaVersion]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_CheckSchemaVersion
    @Feature                   nvarchar(128),
    @CompatibleSchemaVersion   nvarchar(128)
AS
BEGIN
    IF (EXISTS( SELECT  *
                FROM    dbo.aspnet_SchemaVersions
                WHERE   Feature = LOWER( @Feature ) AND
                        CompatibleSchemaVersion = @CompatibleSchemaVersion ))
        RETURN 0

    RETURN 1
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_ChangePasswordQuestionAndAnswer]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_ChangePasswordQuestionAndAnswer
    @ApplicationName       nvarchar(256),
    @UserName              nvarchar(256),
    @NewPasswordQuestion   nvarchar(256),
    @NewPasswordAnswer     nvarchar(128)
AS
BEGIN
    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL
    SELECT  @UserId = u.UserId
    FROM    dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
    WHERE   LoweredUserName = LOWER(@UserName) AND
            u.ApplicationId = a.ApplicationId  AND
            LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.UserId = m.UserId
    IF (@UserId IS NULL)
    BEGIN
        RETURN(1)
    END

    UPDATE dbo.aspnet_Membership
    SET    PasswordQuestion = @NewPasswordQuestion, PasswordAnswer = @NewPasswordAnswer
    WHERE  UserId=@UserId
    RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_FindUsersByEmail]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_FindUsersByEmail
    @ApplicationName       nvarchar(256),
    @EmailToMatch          nvarchar(256),
    @PageIndex             int,
    @PageSize              int
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN 0

    -- Set the page bounds
    DECLARE @PageLowerBound int
    DECLARE @PageUpperBound int
    DECLARE @TotalRecords   int
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        UserId uniqueidentifier
    )

    -- Insert into our temp table
    IF( @EmailToMatch IS NULL )
        INSERT INTO #PageIndexForUsers (UserId)
            SELECT u.UserId
            FROM   dbo.aspnet_Users u, dbo.aspnet_Membership m
            WHERE  u.ApplicationId = @ApplicationId AND m.UserId = u.UserId AND m.Email IS NULL
            ORDER BY m.LoweredEmail
    ELSE
        INSERT INTO #PageIndexForUsers (UserId)
            SELECT u.UserId
            FROM   dbo.aspnet_Users u, dbo.aspnet_Membership m
            WHERE  u.ApplicationId = @ApplicationId AND m.UserId = u.UserId AND m.LoweredEmail LIKE LOWER(@EmailToMatch)
            ORDER BY m.LoweredEmail

    SELECT  u.UserName, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
            m.CreateDate,
            m.LastLoginDate,
            u.LastActivityDate,
            m.LastPasswordChangedDate,
            u.UserId, m.IsLockedOut,
            m.LastLockoutDate
    FROM   dbo.aspnet_Membership m, dbo.aspnet_Users u, #PageIndexForUsers p
    WHERE  u.UserId = p.UserId AND u.UserId = m.UserId AND
           p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY m.LoweredEmail

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_FindUsersByName]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_FindUsersByName
    @ApplicationName       nvarchar(256),
    @UserNameToMatch       nvarchar(256),
    @PageIndex             int,
    @PageSize              int
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN 0

    -- Set the page bounds
    DECLARE @PageLowerBound int
    DECLARE @PageUpperBound int
    DECLARE @TotalRecords   int
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        UserId uniqueidentifier
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (UserId)
        SELECT u.UserId
        FROM   dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE  u.ApplicationId = @ApplicationId AND m.UserId = u.UserId AND u.LoweredUserName LIKE LOWER(@UserNameToMatch)
        ORDER BY u.UserName


    SELECT  u.UserName, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
            m.CreateDate,
            m.LastLoginDate,
            u.LastActivityDate,
            m.LastPasswordChangedDate,
            u.UserId, m.IsLockedOut,
            m.LastLockoutDate
    FROM   dbo.aspnet_Membership m, dbo.aspnet_Users u, #PageIndexForUsers p
    WHERE  u.UserId = p.UserId AND u.UserId = m.UserId AND
           p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY u.UserName

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetAllUsers]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetAllUsers
    @ApplicationName       nvarchar(256),
    @PageIndex             int,
    @PageSize              int
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN 0


    -- Set the page bounds
    DECLARE @PageLowerBound int
    DECLARE @PageUpperBound int
    DECLARE @TotalRecords   int
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        UserId uniqueidentifier
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (UserId)
    SELECT u.UserId
    FROM   dbo.aspnet_Membership m, dbo.aspnet_Users u
    WHERE  u.ApplicationId = @ApplicationId AND u.UserId = m.UserId
    ORDER BY u.UserName

    SELECT @TotalRecords = @@ROWCOUNT

    SELECT u.UserName, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
            m.CreateDate,
            m.LastLoginDate,
            u.LastActivityDate,
            m.LastPasswordChangedDate,
            u.UserId, m.IsLockedOut,
            m.LastLockoutDate
    FROM   dbo.aspnet_Membership m, dbo.aspnet_Users u, #PageIndexForUsers p
    WHERE  u.UserId = p.UserId AND u.UserId = m.UserId AND
           p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY u.UserName
    RETURN @TotalRecords
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetNumberOfUsersOnline]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetNumberOfUsersOnline
    @ApplicationName            nvarchar(256),
    @MinutesSinceLastInActive   int,
    @CurrentTimeUtc             datetime
AS
BEGIN
    DECLARE @DateActive datetime
    SELECT  @DateActive = DATEADD(minute,  -(@MinutesSinceLastInActive), @CurrentTimeUtc)

    DECLARE @NumOnline int
    SELECT  @NumOnline = COUNT(*)
    FROM    dbo.aspnet_Users u(NOLOCK),
            dbo.aspnet_Applications a(NOLOCK),
            dbo.aspnet_Membership m(NOLOCK)
    WHERE   u.ApplicationId = a.ApplicationId                  AND
            LastActivityDate > @DateActive                     AND
            a.LoweredApplicationName = LOWER(@ApplicationName) AND
            u.UserId = m.UserId
    RETURN(@NumOnline)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetPassword]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetPassword
    @ApplicationName                nvarchar(256),
    @UserName                       nvarchar(256),
    @MaxInvalidPasswordAttempts     int,
    @PasswordAttemptWindow          int,
    @CurrentTimeUtc                 datetime,
    @PasswordAnswer                 nvarchar(128) = NULL
AS
BEGIN
    DECLARE @UserId                                 uniqueidentifier
    DECLARE @PasswordFormat                         int
    DECLARE @Password                               nvarchar(128)
    DECLARE @passAns                                nvarchar(128)
    DECLARE @IsLockedOut                            bit
    DECLARE @LastLockoutDate                        datetime
    DECLARE @FailedPasswordAttemptCount             int
    DECLARE @FailedPasswordAttemptWindowStart       datetime
    DECLARE @FailedPasswordAnswerAttemptCount       int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    SELECT  @UserId = u.UserId,
            @Password = m.Password,
            @passAns = m.PasswordAnswer,
            @PasswordFormat = m.PasswordFormat,
            @IsLockedOut = m.IsLockedOut,
            @LastLockoutDate = m.LastLockoutDate,
            @FailedPasswordAttemptCount = m.FailedPasswordAttemptCount,
            @FailedPasswordAttemptWindowStart = m.FailedPasswordAttemptWindowStart,
            @FailedPasswordAnswerAttemptCount = m.FailedPasswordAnswerAttemptCount,
            @FailedPasswordAnswerAttemptWindowStart = m.FailedPasswordAnswerAttemptWindowStart
    FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m WITH ( UPDLOCK )
    WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.ApplicationId = a.ApplicationId    AND
            u.UserId = m.UserId AND
            LOWER(@UserName) = u.LoweredUserName

    IF ( @@rowcount = 0 )
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    IF( @IsLockedOut = 1 )
    BEGIN
        SET @ErrorCode = 99
        GOTO Cleanup
    END

    IF ( NOT( @PasswordAnswer IS NULL ) )
    BEGIN
        IF( ( @passAns IS NULL ) OR ( LOWER( @passAns ) <> LOWER( @PasswordAnswer ) ) )
        BEGIN
            IF( @CurrentTimeUtc > DATEADD( minute, @PasswordAttemptWindow, @FailedPasswordAnswerAttemptWindowStart ) )
            BEGIN
                SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
                SET @FailedPasswordAnswerAttemptCount = 1
            END
            ELSE
            BEGIN
                SET @FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount + 1
                SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
            END

            BEGIN
                IF( @FailedPasswordAnswerAttemptCount >= @MaxInvalidPasswordAttempts )
                BEGIN
                    SET @IsLockedOut = 1
                    SET @LastLockoutDate = @CurrentTimeUtc
                END
            END

            SET @ErrorCode = 3
        END
        ELSE
        BEGIN
            IF( @FailedPasswordAnswerAttemptCount > 0 )
            BEGIN
                SET @FailedPasswordAnswerAttemptCount = 0
                SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
            END
        END

        UPDATE dbo.aspnet_Membership
        SET IsLockedOut = @IsLockedOut, LastLockoutDate = @LastLockoutDate,
            FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
            FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
            FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
            FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
        WHERE @UserId = UserId

        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END

    IF( @TranStarted = 1 )
    BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
    END

    IF( @ErrorCode = 0 )
        SELECT @Password, @PasswordFormat

    RETURN @ErrorCode

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetPasswordWithFormat]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetPasswordWithFormat
    @ApplicationName                nvarchar(256),
    @UserName                       nvarchar(256),
    @UpdateLastLoginActivityDate    bit,
    @CurrentTimeUtc                 datetime
AS
BEGIN
    DECLARE @IsLockedOut                        bit
    DECLARE @UserId                             uniqueidentifier
    DECLARE @Password                           nvarchar(128)
    DECLARE @PasswordSalt                       nvarchar(128)
    DECLARE @PasswordFormat                     int
    DECLARE @FailedPasswordAttemptCount         int
    DECLARE @FailedPasswordAnswerAttemptCount   int
    DECLARE @IsApproved                         bit
    DECLARE @LastActivityDate                   datetime
    DECLARE @LastLoginDate                      datetime

    SELECT  @UserId          = NULL

    SELECT  @UserId = u.UserId, @IsLockedOut = m.IsLockedOut, @Password=Password, @PasswordFormat=PasswordFormat,
            @PasswordSalt=PasswordSalt, @FailedPasswordAttemptCount=FailedPasswordAttemptCount,
		    @FailedPasswordAnswerAttemptCount=FailedPasswordAnswerAttemptCount, @IsApproved=IsApproved,
            @LastActivityDate = LastActivityDate, @LastLoginDate = LastLoginDate
    FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
    WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.ApplicationId = a.ApplicationId    AND
            u.UserId = m.UserId AND
            LOWER(@UserName) = u.LoweredUserName

    IF (@UserId IS NULL)
        RETURN 1

    IF (@IsLockedOut = 1)
        RETURN 99

    SELECT   @Password, @PasswordFormat, @PasswordSalt, @FailedPasswordAttemptCount,
             @FailedPasswordAnswerAttemptCount, @IsApproved, @LastLoginDate, @LastActivityDate

    IF (@UpdateLastLoginActivityDate = 1 AND @IsApproved = 1)
    BEGIN
        UPDATE  dbo.aspnet_Membership
        SET     LastLoginDate = @CurrentTimeUtc
        WHERE   UserId = @UserId

        UPDATE  dbo.aspnet_Users
        SET     LastActivityDate = @CurrentTimeUtc
        WHERE   @UserId = UserId
    END


    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetUserByEmail]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetUserByEmail
    @ApplicationName  nvarchar(256),
    @Email            nvarchar(256)
AS
BEGIN
    IF( @Email IS NULL )
        SELECT  u.UserName
        FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
                u.ApplicationId = a.ApplicationId    AND
                u.UserId = m.UserId AND
                m.ApplicationId = a.ApplicationId AND
                m.LoweredEmail IS NULL
    ELSE
        SELECT  u.UserName
        FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
                u.ApplicationId = a.ApplicationId    AND
                u.UserId = m.UserId AND
                m.ApplicationId = a.ApplicationId AND
                LOWER(@Email) = m.LoweredEmail

    IF (@@rowcount = 0)
        RETURN(1)
    RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetUserByName]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetUserByName
    @ApplicationName      nvarchar(256),
    @UserName             nvarchar(256),
    @CurrentTimeUtc       datetime,
    @UpdateLastActivity   bit = 0
AS
BEGIN
    DECLARE @UserId uniqueidentifier

    IF (@UpdateLastActivity = 1)
    BEGIN
        -- select user ID from aspnet_users table
        SELECT TOP 1 @UserId = u.UserId
        FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE    LOWER(@ApplicationName) = a.LoweredApplicationName AND
                u.ApplicationId = a.ApplicationId    AND
                LOWER(@UserName) = u.LoweredUserName AND u.UserId = m.UserId

        IF (@@ROWCOUNT = 0) -- Username not found
            RETURN -1

        UPDATE   dbo.aspnet_Users
        SET      LastActivityDate = @CurrentTimeUtc
        WHERE    @UserId = UserId

        SELECT m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
                m.CreateDate, m.LastLoginDate, u.LastActivityDate, m.LastPasswordChangedDate,
                u.UserId, m.IsLockedOut, m.LastLockoutDate
        FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE  @UserId = u.UserId AND u.UserId = m.UserId 
    END
    ELSE
    BEGIN
        SELECT TOP 1 m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
                m.CreateDate, m.LastLoginDate, u.LastActivityDate, m.LastPasswordChangedDate,
                u.UserId, m.IsLockedOut,m.LastLockoutDate
        FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m
        WHERE    LOWER(@ApplicationName) = a.LoweredApplicationName AND
                u.ApplicationId = a.ApplicationId    AND
                LOWER(@UserName) = u.LoweredUserName AND u.UserId = m.UserId

        IF (@@ROWCOUNT = 0) -- Username not found
            RETURN -1
    END

    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_GetUserByUserId]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_GetUserByUserId
    @UserId               uniqueidentifier,
    @CurrentTimeUtc       datetime,
    @UpdateLastActivity   bit = 0
AS
BEGIN
    IF ( @UpdateLastActivity = 1 )
    BEGIN
        UPDATE   dbo.aspnet_Users
        SET      LastActivityDate = @CurrentTimeUtc
        FROM     dbo.aspnet_Users
        WHERE    @UserId = UserId

        IF ( @@ROWCOUNT = 0 ) -- User ID not found
            RETURN -1
    END

    SELECT  m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
            m.CreateDate, m.LastLoginDate, u.LastActivityDate,
            m.LastPasswordChangedDate, u.UserName, m.IsLockedOut,
            m.LastLockoutDate
    FROM    dbo.aspnet_Users u, dbo.aspnet_Membership m
    WHERE   @UserId = u.UserId AND u.UserId = m.UserId

    IF ( @@ROWCOUNT = 0 ) -- User ID not found
       RETURN -1

    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_ResetPassword]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_ResetPassword
    @ApplicationName             nvarchar(256),
    @UserName                    nvarchar(256),
    @NewPassword                 nvarchar(128),
    @MaxInvalidPasswordAttempts  int,
    @PasswordAttemptWindow       int,
    @PasswordSalt                nvarchar(128),
    @CurrentTimeUtc              datetime,
    @PasswordFormat              int = 0,
    @PasswordAnswer              nvarchar(128) = NULL
AS
BEGIN
    DECLARE @IsLockedOut                            bit
    DECLARE @LastLockoutDate                        datetime
    DECLARE @FailedPasswordAttemptCount             int
    DECLARE @FailedPasswordAttemptWindowStart       datetime
    DECLARE @FailedPasswordAnswerAttemptCount       int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @UserId                                 uniqueidentifier
    SET     @UserId = NULL

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    SELECT  @UserId = u.UserId
    FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a, dbo.aspnet_Membership m
    WHERE   LoweredUserName = LOWER(@UserName) AND
            u.ApplicationId = a.ApplicationId  AND
            LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.UserId = m.UserId

    IF ( @UserId IS NULL )
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    SELECT @IsLockedOut = IsLockedOut,
           @LastLockoutDate = LastLockoutDate,
           @FailedPasswordAttemptCount = FailedPasswordAttemptCount,
           @FailedPasswordAttemptWindowStart = FailedPasswordAttemptWindowStart,
           @FailedPasswordAnswerAttemptCount = FailedPasswordAnswerAttemptCount,
           @FailedPasswordAnswerAttemptWindowStart = FailedPasswordAnswerAttemptWindowStart
    FROM dbo.aspnet_Membership WITH ( UPDLOCK )
    WHERE @UserId = UserId

    IF( @IsLockedOut = 1 )
    BEGIN
        SET @ErrorCode = 99
        GOTO Cleanup
    END

    UPDATE dbo.aspnet_Membership
    SET    Password = @NewPassword,
           LastPasswordChangedDate = @CurrentTimeUtc,
           PasswordFormat = @PasswordFormat,
           PasswordSalt = @PasswordSalt
    WHERE  @UserId = UserId AND
           ( ( @PasswordAnswer IS NULL ) OR ( LOWER( PasswordAnswer ) = LOWER( @PasswordAnswer ) ) )

    IF ( @@ROWCOUNT = 0 )
        BEGIN
            IF( @CurrentTimeUtc > DATEADD( minute, @PasswordAttemptWindow, @FailedPasswordAnswerAttemptWindowStart ) )
            BEGIN
                SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
                SET @FailedPasswordAnswerAttemptCount = 1
            END
            ELSE
            BEGIN
                SET @FailedPasswordAnswerAttemptWindowStart = @CurrentTimeUtc
                SET @FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount + 1
            END

            BEGIN
                IF( @FailedPasswordAnswerAttemptCount >= @MaxInvalidPasswordAttempts )
                BEGIN
                    SET @IsLockedOut = 1
                    SET @LastLockoutDate = @CurrentTimeUtc
                END
            END

            SET @ErrorCode = 3
        END
    ELSE
        BEGIN
            IF( @FailedPasswordAnswerAttemptCount > 0 )
            BEGIN
                SET @FailedPasswordAnswerAttemptCount = 0
                SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
            END
        END

    IF( NOT ( @PasswordAnswer IS NULL ) )
    BEGIN
        UPDATE dbo.aspnet_Membership
        SET IsLockedOut = @IsLockedOut, LastLockoutDate = @LastLockoutDate,
            FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
            FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
            FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
            FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
        WHERE @UserId = UserId

        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END

    IF( @TranStarted = 1 )
    BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_SetPassword]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_SetPassword
    @ApplicationName  nvarchar(256),
    @UserName         nvarchar(256),
    @NewPassword      nvarchar(128),
    @PasswordSalt     nvarchar(128),
    @CurrentTimeUtc   datetime,
    @PasswordFormat   int = 0
AS
BEGIN
    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL
    SELECT  @UserId = u.UserId
    FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a, dbo.aspnet_Membership m
    WHERE   LoweredUserName = LOWER(@UserName) AND
            u.ApplicationId = a.ApplicationId  AND
            LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.UserId = m.UserId

    IF (@UserId IS NULL)
        RETURN(1)

    UPDATE dbo.aspnet_Membership
    SET Password = @NewPassword, PasswordFormat = @PasswordFormat, PasswordSalt = @PasswordSalt,
        LastPasswordChangedDate = @CurrentTimeUtc
    WHERE @UserId = UserId
    RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_UnlockUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_UnlockUser
    @ApplicationName                         nvarchar(256),
    @UserName                                nvarchar(256)
AS
BEGIN
    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL
    SELECT  @UserId = u.UserId
    FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a, dbo.aspnet_Membership m
    WHERE   LoweredUserName = LOWER(@UserName) AND
            u.ApplicationId = a.ApplicationId  AND
            LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.UserId = m.UserId

    IF ( @UserId IS NULL )
        RETURN 1

    UPDATE dbo.aspnet_Membership
    SET IsLockedOut = 0,
        FailedPasswordAttemptCount = 0,
        FailedPasswordAttemptWindowStart = CONVERT( datetime, '17540101', 112 ),
        FailedPasswordAnswerAttemptCount = 0,
        FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 ),
        LastLockoutDate = CONVERT( datetime, '17540101', 112 )
    WHERE @UserId = UserId

    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_UpdateUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_UpdateUser
    @ApplicationName      nvarchar(256),
    @UserName             nvarchar(256),
    @Email                nvarchar(256),
    @Comment              ntext,
    @IsApproved           bit,
    @LastLoginDate        datetime,
    @LastActivityDate     datetime,
    @UniqueEmail          int,
    @CurrentTimeUtc       datetime
AS
BEGIN
    DECLARE @UserId uniqueidentifier
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @UserId = NULL
    SELECT  @UserId = u.UserId, @ApplicationId = a.ApplicationId
    FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a, dbo.aspnet_Membership m
    WHERE   LoweredUserName = LOWER(@UserName) AND
            u.ApplicationId = a.ApplicationId  AND
            LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.UserId = m.UserId

    IF (@UserId IS NULL)
        RETURN(1)

    IF (@UniqueEmail = 1)
    BEGIN
        IF (EXISTS (SELECT *
                    FROM  dbo.aspnet_Membership WITH (UPDLOCK, HOLDLOCK)
                    WHERE ApplicationId = @ApplicationId  AND @UserId <> UserId AND LoweredEmail = LOWER(@Email)))
        BEGIN
            RETURN(7)
        END
    END

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
	SET @TranStarted = 0

    UPDATE dbo.aspnet_Users WITH (ROWLOCK)
    SET
         LastActivityDate = @LastActivityDate
    WHERE
       @UserId = UserId

    IF( @@ERROR <> 0 )
        GOTO Cleanup

    UPDATE dbo.aspnet_Membership WITH (ROWLOCK)
    SET
         Email            = @Email,
         LoweredEmail     = LOWER(@Email),
         Comment          = @Comment,
         IsApproved       = @IsApproved,
         LastLoginDate    = @LastLoginDate
    WHERE
       @UserId = UserId

    IF( @@ERROR <> 0 )
        GOTO Cleanup

    IF( @TranStarted = 1 )
    BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN -1
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_UpdateUserInfo]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_UpdateUserInfo
    @ApplicationName                nvarchar(256),
    @UserName                       nvarchar(256),
    @IsPasswordCorrect              bit,
    @UpdateLastLoginActivityDate    bit,
    @MaxInvalidPasswordAttempts     int,
    @PasswordAttemptWindow          int,
    @CurrentTimeUtc                 datetime,
    @LastLoginDate                  datetime,
    @LastActivityDate               datetime
AS
BEGIN
    DECLARE @UserId                                 uniqueidentifier
    DECLARE @IsApproved                             bit
    DECLARE @IsLockedOut                            bit
    DECLARE @LastLockoutDate                        datetime
    DECLARE @FailedPasswordAttemptCount             int
    DECLARE @FailedPasswordAttemptWindowStart       datetime
    DECLARE @FailedPasswordAnswerAttemptCount       int
    DECLARE @FailedPasswordAnswerAttemptWindowStart datetime

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    SELECT  @UserId = u.UserId,
            @IsApproved = m.IsApproved,
            @IsLockedOut = m.IsLockedOut,
            @LastLockoutDate = m.LastLockoutDate,
            @FailedPasswordAttemptCount = m.FailedPasswordAttemptCount,
            @FailedPasswordAttemptWindowStart = m.FailedPasswordAttemptWindowStart,
            @FailedPasswordAnswerAttemptCount = m.FailedPasswordAnswerAttemptCount,
            @FailedPasswordAnswerAttemptWindowStart = m.FailedPasswordAnswerAttemptWindowStart
    FROM    dbo.aspnet_Applications a, dbo.aspnet_Users u, dbo.aspnet_Membership m WITH ( UPDLOCK )
    WHERE   LOWER(@ApplicationName) = a.LoweredApplicationName AND
            u.ApplicationId = a.ApplicationId    AND
            u.UserId = m.UserId AND
            LOWER(@UserName) = u.LoweredUserName

    IF ( @@rowcount = 0 )
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    IF( @IsLockedOut = 1 )
    BEGIN
        GOTO Cleanup
    END

    IF( @IsPasswordCorrect = 0 )
    BEGIN
        IF( @CurrentTimeUtc > DATEADD( minute, @PasswordAttemptWindow, @FailedPasswordAttemptWindowStart ) )
        BEGIN
            SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
            SET @FailedPasswordAttemptCount = 1
        END
        ELSE
        BEGIN
            SET @FailedPasswordAttemptWindowStart = @CurrentTimeUtc
            SET @FailedPasswordAttemptCount = @FailedPasswordAttemptCount + 1
        END

        BEGIN
            IF( @FailedPasswordAttemptCount >= @MaxInvalidPasswordAttempts )
            BEGIN
                SET @IsLockedOut = 1
                SET @LastLockoutDate = @CurrentTimeUtc
            END
        END
    END
    ELSE
    BEGIN
        IF( @FailedPasswordAttemptCount > 0 OR @FailedPasswordAnswerAttemptCount > 0 )
        BEGIN
            SET @FailedPasswordAttemptCount = 0
            SET @FailedPasswordAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
            SET @FailedPasswordAnswerAttemptCount = 0
            SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )
            SET @LastLockoutDate = CONVERT( datetime, '17540101', 112 )
        END
    END

    IF( @UpdateLastLoginActivityDate = 1 )
    BEGIN
        UPDATE  dbo.aspnet_Users
        SET     LastActivityDate = @LastActivityDate
        WHERE   @UserId = UserId

        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END

        UPDATE  dbo.aspnet_Membership
        SET     LastLoginDate = @LastLoginDate
        WHERE   UserId = @UserId

        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END


    UPDATE dbo.aspnet_Membership
    SET IsLockedOut = @IsLockedOut, LastLockoutDate = @LastLockoutDate,
        FailedPasswordAttemptCount = @FailedPasswordAttemptCount,
        FailedPasswordAttemptWindowStart = @FailedPasswordAttemptWindowStart,
        FailedPasswordAnswerAttemptCount = @FailedPasswordAnswerAttemptCount,
        FailedPasswordAnswerAttemptWindowStart = @FailedPasswordAnswerAttemptWindowStart
    WHERE @UserId = UserId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
	SET @TranStarted = 0
	COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Paths_CreatePath]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Paths_CreatePath
    @ApplicationId UNIQUEIDENTIFIER,
    @Path           NVARCHAR(256),
    @PathId         UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    BEGIN TRANSACTION
    IF (NOT EXISTS(SELECT * FROM dbo.aspnet_Paths WHERE LoweredPath = LOWER(@Path) AND ApplicationId = @ApplicationId))
    BEGIN
        INSERT dbo.aspnet_Paths (ApplicationId, Path, LoweredPath) VALUES (@ApplicationId, @Path, LOWER(@Path))
    END
    COMMIT TRANSACTION
    SELECT @PathId = PathId FROM dbo.aspnet_Paths WHERE LOWER(@Path) = LoweredPath AND ApplicationId = @ApplicationId
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Personalization_GetApplicationId]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Personalization_GetApplicationId (
    @ApplicationName NVARCHAR(256),
    @ApplicationId UNIQUEIDENTIFIER OUT)
AS
BEGIN
    SELECT @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAdministration_DeleteAllState]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAdministration_DeleteAllState (
    @AllUsersScope bit,
    @ApplicationName NVARCHAR(256),
    @Count int OUT)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
        SELECT @Count = 0
    ELSE
    BEGIN
        IF (@AllUsersScope = 1)
            DELETE FROM aspnet_PersonalizationAllUsers
            WHERE PathId IN
               (SELECT Paths.PathId
                FROM dbo.aspnet_Paths Paths
                WHERE Paths.ApplicationId = @ApplicationId)
        ELSE
            DELETE FROM aspnet_PersonalizationPerUser
            WHERE PathId IN
               (SELECT Paths.PathId
                FROM dbo.aspnet_Paths Paths
                WHERE Paths.ApplicationId = @ApplicationId)

        SELECT @Count = @@ROWCOUNT
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAdministration_FindState]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAdministration_FindState (
    @AllUsersScope bit,
    @ApplicationName NVARCHAR(256),
    @PageIndex              INT,
    @PageSize               INT,
    @Path NVARCHAR(256) = NULL,
    @UserName NVARCHAR(256) = NULL,
    @InactiveSinceDate DATETIME = NULL)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
        RETURN

    -- Set the page bounds
    DECLARE @PageLowerBound INT
    DECLARE @PageUpperBound INT
    DECLARE @TotalRecords   INT
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table to store the selected results
    CREATE TABLE #PageIndex (
        IndexId int IDENTITY (0, 1) NOT NULL,
        ItemId UNIQUEIDENTIFIER
    )

    IF (@AllUsersScope = 1)
    BEGIN
        -- Insert into our temp table
        INSERT INTO #PageIndex (ItemId)
        SELECT Paths.PathId
        FROM dbo.aspnet_Paths Paths,
             ((SELECT Paths.PathId
               FROM dbo.aspnet_PersonalizationAllUsers AllUsers, dbo.aspnet_Paths Paths
               WHERE Paths.ApplicationId = @ApplicationId
                      AND AllUsers.PathId = Paths.PathId
                      AND (@Path IS NULL OR Paths.LoweredPath LIKE LOWER(@Path))
              ) AS SharedDataPerPath
              FULL OUTER JOIN
              (SELECT DISTINCT Paths.PathId
               FROM dbo.aspnet_PersonalizationPerUser PerUser, dbo.aspnet_Paths Paths
               WHERE Paths.ApplicationId = @ApplicationId
                      AND PerUser.PathId = Paths.PathId
                      AND (@Path IS NULL OR Paths.LoweredPath LIKE LOWER(@Path))
              ) AS UserDataPerPath
              ON SharedDataPerPath.PathId = UserDataPerPath.PathId
             )
        WHERE Paths.PathId = SharedDataPerPath.PathId OR Paths.PathId = UserDataPerPath.PathId
        ORDER BY Paths.Path ASC

        SELECT @TotalRecords = @@ROWCOUNT

        SELECT Paths.Path,
               SharedDataPerPath.LastUpdatedDate,
               SharedDataPerPath.SharedDataLength,
               UserDataPerPath.UserDataLength,
               UserDataPerPath.UserCount
        FROM dbo.aspnet_Paths Paths,
             ((SELECT PageIndex.ItemId AS PathId,
                      AllUsers.LastUpdatedDate AS LastUpdatedDate,
                      DATALENGTH(AllUsers.PageSettings) AS SharedDataLength
               FROM dbo.aspnet_PersonalizationAllUsers AllUsers, #PageIndex PageIndex
               WHERE AllUsers.PathId = PageIndex.ItemId
                     AND PageIndex.IndexId >= @PageLowerBound AND PageIndex.IndexId <= @PageUpperBound
              ) AS SharedDataPerPath
              FULL OUTER JOIN
              (SELECT PageIndex.ItemId AS PathId,
                      SUM(DATALENGTH(PerUser.PageSettings)) AS UserDataLength,
                      COUNT(*) AS UserCount
               FROM aspnet_PersonalizationPerUser PerUser, #PageIndex PageIndex
               WHERE PerUser.PathId = PageIndex.ItemId
                     AND PageIndex.IndexId >= @PageLowerBound AND PageIndex.IndexId <= @PageUpperBound
               GROUP BY PageIndex.ItemId
              ) AS UserDataPerPath
              ON SharedDataPerPath.PathId = UserDataPerPath.PathId
             )
        WHERE Paths.PathId = SharedDataPerPath.PathId OR Paths.PathId = UserDataPerPath.PathId
        ORDER BY Paths.Path ASC
    END
    ELSE
    BEGIN
        -- Insert into our temp table
        INSERT INTO #PageIndex (ItemId)
        SELECT PerUser.Id
        FROM dbo.aspnet_PersonalizationPerUser PerUser, dbo.aspnet_Users Users, dbo.aspnet_Paths Paths
        WHERE Paths.ApplicationId = @ApplicationId
              AND PerUser.UserId = Users.UserId
              AND PerUser.PathId = Paths.PathId
              AND (@Path IS NULL OR Paths.LoweredPath LIKE LOWER(@Path))
              AND (@UserName IS NULL OR Users.LoweredUserName LIKE LOWER(@UserName))
              AND (@InactiveSinceDate IS NULL OR Users.LastActivityDate <= @InactiveSinceDate)
        ORDER BY Paths.Path ASC, Users.UserName ASC

        SELECT @TotalRecords = @@ROWCOUNT

        SELECT Paths.Path, PerUser.LastUpdatedDate, DATALENGTH(PerUser.PageSettings), Users.UserName, Users.LastActivityDate
        FROM dbo.aspnet_PersonalizationPerUser PerUser, dbo.aspnet_Users Users, dbo.aspnet_Paths Paths, #PageIndex PageIndex
        WHERE PerUser.Id = PageIndex.ItemId
              AND PerUser.UserId = Users.UserId
              AND PerUser.PathId = Paths.PathId
              AND PageIndex.IndexId >= @PageLowerBound AND PageIndex.IndexId <= @PageUpperBound
        ORDER BY Paths.Path ASC, Users.UserName ASC
    END

    RETURN @TotalRecords
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAdministration_GetCountOfState]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAdministration_GetCountOfState (
    @Count int OUT,
    @AllUsersScope bit,
    @ApplicationName NVARCHAR(256),
    @Path NVARCHAR(256) = NULL,
    @UserName NVARCHAR(256) = NULL,
    @InactiveSinceDate DATETIME = NULL)
AS
BEGIN

    DECLARE @ApplicationId UNIQUEIDENTIFIER
    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
        SELECT @Count = 0
    ELSE
        IF (@AllUsersScope = 1)
            SELECT @Count = COUNT(*)
            FROM dbo.aspnet_PersonalizationAllUsers AllUsers, dbo.aspnet_Paths Paths
            WHERE Paths.ApplicationId = @ApplicationId
                  AND AllUsers.PathId = Paths.PathId
                  AND (@Path IS NULL OR Paths.LoweredPath LIKE LOWER(@Path))
        ELSE
            SELECT @Count = COUNT(*)
            FROM dbo.aspnet_PersonalizationPerUser PerUser, dbo.aspnet_Users Users, dbo.aspnet_Paths Paths
            WHERE Paths.ApplicationId = @ApplicationId
                  AND PerUser.UserId = Users.UserId
                  AND PerUser.PathId = Paths.PathId
                  AND (@Path IS NULL OR Paths.LoweredPath LIKE LOWER(@Path))
                  AND (@UserName IS NULL OR Users.LoweredUserName LIKE LOWER(@UserName))
                  AND (@InactiveSinceDate IS NULL OR Users.LastActivityDate <= @InactiveSinceDate)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAdministration_ResetSharedState]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAdministration_ResetSharedState (
    @Count int OUT,
    @ApplicationName NVARCHAR(256),
    @Path NVARCHAR(256))
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
        SELECT @Count = 0
    ELSE
    BEGIN
        DELETE FROM dbo.aspnet_PersonalizationAllUsers
        WHERE PathId IN
            (SELECT AllUsers.PathId
             FROM dbo.aspnet_PersonalizationAllUsers AllUsers, dbo.aspnet_Paths Paths
             WHERE Paths.ApplicationId = @ApplicationId
                   AND AllUsers.PathId = Paths.PathId
                   AND Paths.LoweredPath = LOWER(@Path))

        SELECT @Count = @@ROWCOUNT
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAdministration_ResetUserState]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAdministration_ResetUserState (
    @Count                  int                 OUT,
    @ApplicationName        NVARCHAR(256),
    @InactiveSinceDate      DATETIME            = NULL,
    @UserName               NVARCHAR(256)       = NULL,
    @Path                   NVARCHAR(256)       = NULL)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
        SELECT @Count = 0
    ELSE
    BEGIN
        DELETE FROM dbo.aspnet_PersonalizationPerUser
        WHERE Id IN (SELECT PerUser.Id
                     FROM dbo.aspnet_PersonalizationPerUser PerUser, dbo.aspnet_Users Users, dbo.aspnet_Paths Paths
                     WHERE Paths.ApplicationId = @ApplicationId
                           AND PerUser.UserId = Users.UserId
                           AND PerUser.PathId = Paths.PathId
                           AND (@InactiveSinceDate IS NULL OR Users.LastActivityDate <= @InactiveSinceDate)
                           AND (@UserName IS NULL OR Users.LoweredUserName = LOWER(@UserName))
                           AND (@Path IS NULL OR Paths.LoweredPath = LOWER(@Path)))

        SELECT @Count = @@ROWCOUNT
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAllUsers_GetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAllUsers_GetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @Path              NVARCHAR(256))
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL

    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        RETURN
    END

    SELECT p.PageSettings FROM dbo.aspnet_PersonalizationAllUsers p WHERE p.PathId = @PathId
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAllUsers_ResetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAllUsers_ResetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @Path              NVARCHAR(256))
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL

    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        RETURN
    END

    DELETE FROM dbo.aspnet_PersonalizationAllUsers WHERE PathId = @PathId
    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationAllUsers_SetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationAllUsers_SetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @Path             NVARCHAR(256),
    @PageSettings     IMAGE,
    @CurrentTimeUtc   DATETIME)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        EXEC dbo.aspnet_Paths_CreatePath @ApplicationId, @Path, @PathId OUTPUT
    END

    IF (EXISTS(SELECT PathId FROM dbo.aspnet_PersonalizationAllUsers WHERE PathId = @PathId))
        UPDATE dbo.aspnet_PersonalizationAllUsers SET PageSettings = @PageSettings, LastUpdatedDate = @CurrentTimeUtc WHERE PathId = @PathId
    ELSE
        INSERT INTO dbo.aspnet_PersonalizationAllUsers(PathId, PageSettings, LastUpdatedDate) VALUES (@PathId, @PageSettings, @CurrentTimeUtc)
    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationPerUser_GetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationPerUser_GetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @UserName         NVARCHAR(256),
    @Path             NVARCHAR(256),
    @CurrentTimeUtc   DATETIME)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER
    DECLARE @UserId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL
    SELECT @UserId = NULL

    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @UserId = u.UserId FROM dbo.aspnet_Users u WHERE u.ApplicationId = @ApplicationId AND u.LoweredUserName = LOWER(@UserName)
    IF (@UserId IS NULL)
    BEGIN
        RETURN
    END

    UPDATE   dbo.aspnet_Users WITH (ROWLOCK)
    SET      LastActivityDate = @CurrentTimeUtc
    WHERE    UserId = @UserId
    IF (@@ROWCOUNT = 0) -- Username not found
        RETURN

    SELECT p.PageSettings FROM dbo.aspnet_PersonalizationPerUser p WHERE p.PathId = @PathId AND p.UserId = @UserId
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationPerUser_ResetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationPerUser_ResetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @UserName         NVARCHAR(256),
    @Path             NVARCHAR(256),
    @CurrentTimeUtc   DATETIME)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER
    DECLARE @UserId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL
    SELECT @UserId = NULL

    EXEC dbo.aspnet_Personalization_GetApplicationId @ApplicationName, @ApplicationId OUTPUT
    IF (@ApplicationId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        RETURN
    END

    SELECT @UserId = u.UserId FROM dbo.aspnet_Users u WHERE u.ApplicationId = @ApplicationId AND u.LoweredUserName = LOWER(@UserName)
    IF (@UserId IS NULL)
    BEGIN
        RETURN
    END

    UPDATE   dbo.aspnet_Users WITH (ROWLOCK)
    SET      LastActivityDate = @CurrentTimeUtc
    WHERE    UserId = @UserId
    IF (@@ROWCOUNT = 0) -- Username not found
        RETURN

    DELETE FROM dbo.aspnet_PersonalizationPerUser WHERE PathId = @PathId AND UserId = @UserId
    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_DeleteInactiveProfiles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_DeleteInactiveProfiles
    @ApplicationName        nvarchar(256),
    @ProfileAuthOptions     int,
    @InactiveSinceDate      datetime
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
    BEGIN
        SELECT  0
        RETURN
    END

    DELETE
    FROM    dbo.aspnet_Profile
    WHERE   UserId IN
            (   SELECT  UserId
                FROM    dbo.aspnet_Users u
                WHERE   ApplicationId = @ApplicationId
                        AND (LastActivityDate <= @InactiveSinceDate)
                        AND (
                                (@ProfileAuthOptions = 2)
                             OR (@ProfileAuthOptions = 0 AND IsAnonymous = 1)
                             OR (@ProfileAuthOptions = 1 AND IsAnonymous = 0)
                            )
            )

    SELECT  @@ROWCOUNT
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_GetNumberOfInactiveProfiles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_GetNumberOfInactiveProfiles
    @ApplicationName        nvarchar(256),
    @ProfileAuthOptions     int,
    @InactiveSinceDate      datetime
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
    BEGIN
        SELECT 0
        RETURN
    END

    SELECT  COUNT(*)
    FROM    dbo.aspnet_Users u, dbo.aspnet_Profile p
    WHERE   ApplicationId = @ApplicationId
        AND u.UserId = p.UserId
        AND (LastActivityDate <= @InactiveSinceDate)
        AND (
                (@ProfileAuthOptions = 2)
                OR (@ProfileAuthOptions = 0 AND IsAnonymous = 1)
                OR (@ProfileAuthOptions = 1 AND IsAnonymous = 0)
            )
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_GetProfiles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_GetProfiles
    @ApplicationName        nvarchar(256),
    @ProfileAuthOptions     int,
    @PageIndex              int,
    @PageSize               int,
    @UserNameToMatch        nvarchar(256) = NULL,
    @InactiveSinceDate      datetime      = NULL
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN

    -- Set the page bounds
    DECLARE @PageLowerBound int
    DECLARE @PageUpperBound int
    DECLARE @TotalRecords   int
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        UserId uniqueidentifier
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (UserId)
        SELECT  u.UserId
        FROM    dbo.aspnet_Users u, dbo.aspnet_Profile p
        WHERE   ApplicationId = @ApplicationId
            AND u.UserId = p.UserId
            AND (@InactiveSinceDate IS NULL OR LastActivityDate <= @InactiveSinceDate)
            AND (     (@ProfileAuthOptions = 2)
                   OR (@ProfileAuthOptions = 0 AND IsAnonymous = 1)
                   OR (@ProfileAuthOptions = 1 AND IsAnonymous = 0)
                 )
            AND (@UserNameToMatch IS NULL OR LoweredUserName LIKE LOWER(@UserNameToMatch))
        ORDER BY UserName

    SELECT  u.UserName, u.IsAnonymous, u.LastActivityDate, p.LastUpdatedDate,
            DATALENGTH(p.PropertyNames) + DATALENGTH(p.PropertyValuesString) + DATALENGTH(p.PropertyValuesBinary)
    FROM    dbo.aspnet_Users u, dbo.aspnet_Profile p, #PageIndexForUsers i
    WHERE   u.UserId = p.UserId AND p.UserId = i.UserId AND i.IndexId >= @PageLowerBound AND i.IndexId <= @PageUpperBound

    SELECT COUNT(*)
    FROM   #PageIndexForUsers

    DROP TABLE #PageIndexForUsers
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_GetProperties]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_GetProperties
    @ApplicationName      nvarchar(256),
    @UserName             nvarchar(256),
    @CurrentTimeUtc       datetime
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM dbo.aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN

    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL

    SELECT @UserId = UserId
    FROM   dbo.aspnet_Users
    WHERE  ApplicationId = @ApplicationId AND LoweredUserName = LOWER(@UserName)

    IF (@UserId IS NULL)
        RETURN
    SELECT TOP 1 PropertyNames, PropertyValuesString, PropertyValuesBinary
    FROM         dbo.aspnet_Profile
    WHERE        UserId = @UserId

    IF (@@ROWCOUNT > 0)
    BEGIN
        UPDATE dbo.aspnet_Users
        SET    LastActivityDate=@CurrentTimeUtc
        WHERE  UserId = @UserId
    END
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_RegisterSchemaVersion]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_RegisterSchemaVersion
    @Feature                   nvarchar(128),
    @CompatibleSchemaVersion   nvarchar(128),
    @IsCurrentVersion          bit,
    @RemoveIncompatibleSchema  bit
AS
BEGIN
    IF( @RemoveIncompatibleSchema = 1 )
    BEGIN
        DELETE FROM dbo.aspnet_SchemaVersions WHERE Feature = LOWER( @Feature )
    END
    ELSE
    BEGIN
        IF( @IsCurrentVersion = 1 )
        BEGIN
            UPDATE dbo.aspnet_SchemaVersions
            SET IsCurrentVersion = 0
            WHERE Feature = LOWER( @Feature )
        END
    END

    INSERT  dbo.aspnet_SchemaVersions( Feature, CompatibleSchemaVersion, IsCurrentVersion )
    VALUES( LOWER( @Feature ), @CompatibleSchemaVersion, @IsCurrentVersion )
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Roles_CreateRole]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Roles_CreateRole
    @ApplicationName  nvarchar(256),
    @RoleName         nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (EXISTS(SELECT RoleId FROM dbo.aspnet_Roles WHERE LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId))
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    INSERT INTO dbo.aspnet_Roles
                (ApplicationId, RoleName, LoweredRoleName)
         VALUES (@ApplicationId, @RoleName, LOWER(@RoleName))

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Roles_DeleteRole]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Roles_DeleteRole
    @ApplicationName            nvarchar(256),
    @RoleName                   nvarchar(256),
    @DeleteOnlyIfRoleIsEmpty    bit
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(1)

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    DECLARE @RoleId   uniqueidentifier
    SELECT  @RoleId = NULL
    SELECT  @RoleId = RoleId FROM dbo.aspnet_Roles WHERE LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId

    IF (@RoleId IS NULL)
    BEGIN
        SELECT @ErrorCode = 1
        GOTO Cleanup
    END
    IF (@DeleteOnlyIfRoleIsEmpty <> 0)
    BEGIN
        IF (EXISTS (SELECT RoleId FROM dbo.aspnet_UsersInRoles  WHERE @RoleId = RoleId))
        BEGIN
            SELECT @ErrorCode = 2
            GOTO Cleanup
        END
    END


    DELETE FROM dbo.aspnet_UsersInRoles  WHERE @RoleId = RoleId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.aspnet_Roles WHERE @RoleId = RoleId  AND ApplicationId = @ApplicationId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    RETURN(0)

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Roles_GetAllRoles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Roles_GetAllRoles (
    @ApplicationName           nvarchar(256))
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN
    SELECT RoleName
    FROM   dbo.aspnet_Roles WHERE ApplicationId = @ApplicationId
    ORDER BY RoleName
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Roles_RoleExists]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Roles_RoleExists
    @ApplicationName  nvarchar(256),
    @RoleName         nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(0)
    IF (EXISTS (SELECT RoleName FROM dbo.aspnet_Roles WHERE LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId ))
        RETURN(1)
    ELSE
        RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Setup_RemoveAllRoleMembers]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_Setup_RemoveAllRoleMembers
    @name   sysname
AS
BEGIN
    CREATE TABLE #aspnet_RoleMembers
    (
        Group_name      sysname,
        Group_id        smallint,
        Users_in_group  sysname,
        User_id         smallint
    )

    INSERT INTO #aspnet_RoleMembers
    EXEC sp_helpuser @name

    DECLARE @user_id smallint
    DECLARE @cmd nvarchar(500)
    DECLARE c1 cursor FORWARD_ONLY FOR
        SELECT User_id FROM #aspnet_RoleMembers

    OPEN c1

    FETCH c1 INTO @user_id
    WHILE (@@fetch_status = 0)
    BEGIN
        SET @cmd = 'EXEC sp_droprolemember ' + '''' + @name + ''', ''' + USER_NAME(@user_id) + ''''
        EXEC (@cmd)
        FETCH c1 INTO @user_id
    END

    CLOSE c1
    DEALLOCATE c1
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Setup_RestorePermissions]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_Setup_RestorePermissions
    @name   sysname
AS
BEGIN
    DECLARE @object sysname
    DECLARE @protectType char(10)
    DECLARE @action varchar(60)
    DECLARE @grantee sysname
    DECLARE @cmd nvarchar(500)
    DECLARE c1 cursor FORWARD_ONLY FOR
        SELECT Object, ProtectType, [Action], Grantee FROM #aspnet_Permissions where Object = @name

    OPEN c1

    FETCH c1 INTO @object, @protectType, @action, @grantee
    WHILE (@@fetch_status = 0)
    BEGIN
        SET @cmd = @protectType + ' ' + @action + ' on ' + @object + ' TO [' + @grantee + ']'
        EXEC (@cmd)
        FETCH c1 INTO @object, @protectType, @action, @grantee
    END

    CLOSE c1
    DEALLOCATE c1
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UnRegisterSchemaVersion]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_UnRegisterSchemaVersion
    @Feature                   nvarchar(128),
    @CompatibleSchemaVersion   nvarchar(128)
AS
BEGIN
    DELETE FROM dbo.aspnet_SchemaVersions
        WHERE   Feature = LOWER(@Feature) AND @CompatibleSchemaVersion = CompatibleSchemaVersion
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Users_CreateUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE [dbo].aspnet_Users_CreateUser
    @ApplicationId    uniqueidentifier,
    @UserName         nvarchar(256),
    @IsUserAnonymous  bit,
    @LastActivityDate DATETIME,
    @UserId           uniqueidentifier OUTPUT
AS
BEGIN
    IF( @UserId IS NULL )
        SELECT @UserId = NEWID()
    ELSE
    BEGIN
        IF( EXISTS( SELECT UserId FROM dbo.aspnet_Users
                    WHERE @UserId = UserId ) )
            RETURN -1
    END

    INSERT dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
    VALUES (@ApplicationId, @UserId, @UserName, LOWER(@UserName), @IsUserAnonymous, @LastActivityDate)

    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Users_DeleteUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE [dbo].aspnet_Users_DeleteUser
    @ApplicationName  nvarchar(256),
    @UserName         nvarchar(256),
    @TablesToDeleteFrom int,
    @NumTablesDeletedFrom int OUTPUT
AS
BEGIN
    DECLARE @UserId               uniqueidentifier
    SELECT  @UserId               = NULL
    SELECT  @NumTablesDeletedFrom = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
	SET @TranStarted = 0

    DECLARE @ErrorCode   int
    DECLARE @RowCount    int

    SET @ErrorCode = 0
    SET @RowCount  = 0

    SELECT  @UserId = u.UserId
    FROM    dbo.aspnet_Users u, dbo.aspnet_Applications a
    WHERE   u.LoweredUserName       = LOWER(@UserName)
        AND u.ApplicationId         = a.ApplicationId
        AND LOWER(@ApplicationName) = a.LoweredApplicationName

    IF (@UserId IS NULL)
    BEGIN
        GOTO Cleanup
    END

    -- Delete from Membership table if (@TablesToDeleteFrom & 1) is set
    IF ((@TablesToDeleteFrom & 1) <> 0 AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_MembershipUsers') AND (type = 'V'))))
    BEGIN
        DELETE FROM dbo.aspnet_Membership WHERE @UserId = UserId

        SELECT @ErrorCode = @@ERROR,
               @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

        IF (@RowCount <> 0)
            SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    END

    -- Delete from aspnet_UsersInRoles table if (@TablesToDeleteFrom & 2) is set
    IF ((@TablesToDeleteFrom & 2) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_UsersInRoles') AND (type = 'V'))) )
    BEGIN
        DELETE FROM dbo.aspnet_UsersInRoles WHERE @UserId = UserId

        SELECT @ErrorCode = @@ERROR,
                @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

        IF (@RowCount <> 0)
            SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    END

    -- Delete from aspnet_Profile table if (@TablesToDeleteFrom & 4) is set
    IF ((@TablesToDeleteFrom & 4) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_Profiles') AND (type = 'V'))) )
    BEGIN
        DELETE FROM dbo.aspnet_Profile WHERE @UserId = UserId

        SELECT @ErrorCode = @@ERROR,
                @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

        IF (@RowCount <> 0)
            SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    END

    -- Delete from aspnet_PersonalizationPerUser table if (@TablesToDeleteFrom & 8) is set
    IF ((@TablesToDeleteFrom & 8) <> 0  AND
        (EXISTS (SELECT name FROM sysobjects WHERE (name = N'vw_aspnet_WebPartState_User') AND (type = 'V'))) )
    BEGIN
        DELETE FROM dbo.aspnet_PersonalizationPerUser WHERE @UserId = UserId

        SELECT @ErrorCode = @@ERROR,
                @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

        IF (@RowCount <> 0)
            SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    END

    -- Delete from aspnet_Users table if (@TablesToDeleteFrom & 1,2,4 & 8) are all set
    IF ((@TablesToDeleteFrom & 1) <> 0 AND
        (@TablesToDeleteFrom & 2) <> 0 AND
        (@TablesToDeleteFrom & 4) <> 0 AND
        (@TablesToDeleteFrom & 8) <> 0 AND
        (EXISTS (SELECT UserId FROM dbo.aspnet_Users WHERE @UserId = UserId)))
    BEGIN
        DELETE FROM dbo.aspnet_Users WHERE @UserId = UserId

        SELECT @ErrorCode = @@ERROR,
                @RowCount = @@ROWCOUNT

        IF( @ErrorCode <> 0 )
            GOTO Cleanup

        IF (@RowCount <> 0)
            SELECT  @NumTablesDeletedFrom = @NumTablesDeletedFrom + 1
    END

    IF( @TranStarted = 1 )
    BEGIN
	    SET @TranStarted = 0
	    COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:
    SET @NumTablesDeletedFrom = 0

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
	    ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_AddUsersToRoles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_AddUsersToRoles
	@ApplicationName  nvarchar(256),
	@UserNames		  nvarchar(4000),
	@RoleNames		  nvarchar(4000),
	@CurrentTimeUtc   datetime
AS
BEGIN
	DECLARE @AppId uniqueidentifier
	SELECT  @AppId = NULL
	SELECT  @AppId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
	IF (@AppId IS NULL)
		RETURN(2)
	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames	table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles	table(RoleId uniqueidentifier NOT NULL PRIMARY KEY)
	DECLARE @tbUsers	table(UserId uniqueidentifier NOT NULL PRIMARY KEY)
	DECLARE @Num		int
	DECLARE @Pos		int
	DECLARE @NextPos	int
	DECLARE @Name		nvarchar(256)

	SET @Num = 0
	SET @Pos = 1
	WHILE(@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	  SELECT RoleId
	  FROM   dbo.aspnet_Roles ar, @tbNames t
	  WHERE  LOWER(t.Name) = ar.LoweredRoleName AND ar.ApplicationId = @AppId

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		SELECT TOP 1 Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (SELECT ar.LoweredRoleName FROM dbo.aspnet_Roles ar,  @tbRoles r WHERE r.RoleId = ar.RoleId)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(2)
	END

	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1

	WHILE(@Pos <= LEN(@UserNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @UserNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@UserNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@UserNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbUsers
	  SELECT UserId
	  FROM   dbo.aspnet_Users ar, @tbNames t
	  WHERE  LOWER(t.Name) = ar.LoweredUserName AND ar.ApplicationId = @AppId

	IF (@@ROWCOUNT <> @Num)
	BEGIN
		DELETE FROM @tbNames
		WHERE LOWER(Name) IN (SELECT LoweredUserName FROM dbo.aspnet_Users au,  @tbUsers u WHERE au.UserId = u.UserId)

		INSERT dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate)
		  SELECT @AppId, NEWID(), Name, LOWER(Name), 0, @CurrentTimeUtc
		  FROM   @tbNames

		INSERT INTO @tbUsers
		  SELECT  UserId
		  FROM	dbo.aspnet_Users au, @tbNames t
		  WHERE   LOWER(t.Name) = au.LoweredUserName AND au.ApplicationId = @AppId
	END

	IF (EXISTS (SELECT * FROM dbo.aspnet_UsersInRoles ur, @tbUsers tu, @tbRoles tr WHERE tu.UserId = ur.UserId AND tr.RoleId = ur.RoleId))
	BEGIN
		SELECT TOP 1 UserName, RoleName
		FROM		 dbo.aspnet_UsersInRoles ur, @tbUsers tu, @tbRoles tr, aspnet_Users u, aspnet_Roles r
		WHERE		u.UserId = tu.UserId AND r.RoleId = tr.RoleId AND tu.UserId = ur.UserId AND tr.RoleId = ur.RoleId

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
	SELECT UserId, RoleId
	FROM @tbUsers, @tbRoles

	IF( @TranStarted = 1 )
		COMMIT TRANSACTION
	RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_FindUsersInRole]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_FindUsersInRole
    @ApplicationName  nvarchar(256),
    @RoleName         nvarchar(256),
    @UserNameToMatch  nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(1)
     DECLARE @RoleId uniqueidentifier
     SELECT  @RoleId = NULL

     SELECT  @RoleId = RoleId
     FROM    dbo.aspnet_Roles
     WHERE   LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId

     IF (@RoleId IS NULL)
         RETURN(1)

    SELECT u.UserName
    FROM   dbo.aspnet_Users u, dbo.aspnet_UsersInRoles ur
    WHERE  u.UserId = ur.UserId AND @RoleId = ur.RoleId AND u.ApplicationId = @ApplicationId AND LoweredUserName LIKE LOWER(@UserNameToMatch)
    ORDER BY u.UserName
    RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_GetRolesForUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_GetRolesForUser
    @ApplicationName  nvarchar(256),
    @UserName         nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(1)
    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL

    SELECT  @UserId = UserId
    FROM    dbo.aspnet_Users
    WHERE   LoweredUserName = LOWER(@UserName) AND ApplicationId = @ApplicationId

    IF (@UserId IS NULL)
        RETURN(1)

    SELECT r.RoleName
    FROM   dbo.aspnet_Roles r, dbo.aspnet_UsersInRoles ur
    WHERE  r.RoleId = ur.RoleId AND r.ApplicationId = @ApplicationId AND ur.UserId = @UserId
    ORDER BY r.RoleName
    RETURN (0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_GetUsersInRoles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_GetUsersInRoles
    @ApplicationName  nvarchar(256),
    @RoleName         nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(1)
     DECLARE @RoleId uniqueidentifier
     SELECT  @RoleId = NULL

     SELECT  @RoleId = RoleId
     FROM    dbo.aspnet_Roles
     WHERE   LOWER(@RoleName) = LoweredRoleName AND ApplicationId = @ApplicationId

     IF (@RoleId IS NULL)
         RETURN(1)

    SELECT u.UserName
    FROM   dbo.aspnet_Users u, dbo.aspnet_UsersInRoles ur
    WHERE  u.UserId = ur.UserId AND @RoleId = ur.RoleId AND u.ApplicationId = @ApplicationId
    ORDER BY u.UserName
    RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_IsUserInRole]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_IsUserInRole
    @ApplicationName  nvarchar(256),
    @UserName         nvarchar(256),
    @RoleName         nvarchar(256)
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL
    SELECT  @ApplicationId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
    IF (@ApplicationId IS NULL)
        RETURN(2)
    DECLARE @UserId uniqueidentifier
    SELECT  @UserId = NULL
    DECLARE @RoleId uniqueidentifier
    SELECT  @RoleId = NULL

    SELECT  @UserId = UserId
    FROM    dbo.aspnet_Users
    WHERE   LoweredUserName = LOWER(@UserName) AND ApplicationId = @ApplicationId

    IF (@UserId IS NULL)
        RETURN(2)

    SELECT  @RoleId = RoleId
    FROM    dbo.aspnet_Roles
    WHERE   LoweredRoleName = LOWER(@RoleName) AND ApplicationId = @ApplicationId

    IF (@RoleId IS NULL)
        RETURN(3)

    IF (EXISTS( SELECT * FROM dbo.aspnet_UsersInRoles WHERE  UserId = @UserId AND RoleId = @RoleId))
        RETURN(1)
    ELSE
        RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_UsersInRoles_RemoveUsersFromRoles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_UsersInRoles_RemoveUsersFromRoles
	@ApplicationName  nvarchar(256),
	@UserNames		  nvarchar(4000),
	@RoleNames		  nvarchar(4000)
AS
BEGIN
	DECLARE @AppId uniqueidentifier
	SELECT  @AppId = NULL
	SELECT  @AppId = ApplicationId FROM aspnet_Applications WHERE LOWER(@ApplicationName) = LoweredApplicationName
	IF (@AppId IS NULL)
		RETURN(2)


	DECLARE @TranStarted   bit
	SET @TranStarted = 0

	IF( @@TRANCOUNT = 0 )
	BEGIN
		BEGIN TRANSACTION
		SET @TranStarted = 1
	END

	DECLARE @tbNames  table(Name nvarchar(256) NOT NULL PRIMARY KEY)
	DECLARE @tbRoles  table(RoleId uniqueidentifier NOT NULL PRIMARY KEY)
	DECLARE @tbUsers  table(UserId uniqueidentifier NOT NULL PRIMARY KEY)
	DECLARE @Num	  int
	DECLARE @Pos	  int
	DECLARE @NextPos  int
	DECLARE @Name	  nvarchar(256)
	DECLARE @CountAll int
	DECLARE @CountU	  int
	DECLARE @CountR	  int


	SET @Num = 0
	SET @Pos = 1
	WHILE(@Pos <= LEN(@RoleNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @RoleNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@RoleNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@RoleNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbRoles
	  SELECT RoleId
	  FROM   dbo.aspnet_Roles ar, @tbNames t
	  WHERE  LOWER(t.Name) = ar.LoweredRoleName AND ar.ApplicationId = @AppId
	SELECT @CountR = @@ROWCOUNT

	IF (@CountR <> @Num)
	BEGIN
		SELECT TOP 1 N'', Name
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (SELECT ar.LoweredRoleName FROM dbo.aspnet_Roles ar,  @tbRoles r WHERE r.RoleId = ar.RoleId)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(2)
	END


	DELETE FROM @tbNames WHERE 1=1
	SET @Num = 0
	SET @Pos = 1


	WHILE(@Pos <= LEN(@UserNames))
	BEGIN
		SELECT @NextPos = CHARINDEX(N',', @UserNames,  @Pos)
		IF (@NextPos = 0 OR @NextPos IS NULL)
			SELECT @NextPos = LEN(@UserNames) + 1
		SELECT @Name = RTRIM(LTRIM(SUBSTRING(@UserNames, @Pos, @NextPos - @Pos)))
		SELECT @Pos = @NextPos+1

		INSERT INTO @tbNames VALUES (@Name)
		SET @Num = @Num + 1
	END

	INSERT INTO @tbUsers
	  SELECT UserId
	  FROM   dbo.aspnet_Users ar, @tbNames t
	  WHERE  LOWER(t.Name) = ar.LoweredUserName AND ar.ApplicationId = @AppId

	SELECT @CountU = @@ROWCOUNT
	IF (@CountU <> @Num)
	BEGIN
		SELECT TOP 1 Name, N''
		FROM   @tbNames
		WHERE  LOWER(Name) NOT IN (SELECT au.LoweredUserName FROM dbo.aspnet_Users au,  @tbUsers u WHERE u.UserId = au.UserId)

		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(1)
	END

	SELECT  @CountAll = COUNT(*)
	FROM	dbo.aspnet_UsersInRoles ur, @tbUsers u, @tbRoles r
	WHERE   ur.UserId = u.UserId AND ur.RoleId = r.RoleId

	IF (@CountAll <> @CountU * @CountR)
	BEGIN
		SELECT TOP 1 UserName, RoleName
		FROM		 @tbUsers tu, @tbRoles tr, dbo.aspnet_Users u, dbo.aspnet_Roles r
		WHERE		 u.UserId = tu.UserId AND r.RoleId = tr.RoleId AND
					 tu.UserId NOT IN (SELECT ur.UserId FROM dbo.aspnet_UsersInRoles ur WHERE ur.RoleId = tr.RoleId) AND
					 tr.RoleId NOT IN (SELECT ur.RoleId FROM dbo.aspnet_UsersInRoles ur WHERE ur.UserId = tu.UserId)
		IF( @TranStarted = 1 )
			ROLLBACK TRANSACTION
		RETURN(3)
	END

	DELETE FROM dbo.aspnet_UsersInRoles
	WHERE UserId IN (SELECT UserId FROM @tbUsers)
	  AND RoleId IN (SELECT RoleId FROM @tbRoles)
	IF( @TranStarted = 1 )
		COMMIT TRANSACTION
	RETURN(0)
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_WebEvent_LogEvent]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_WebEvent_LogEvent
        @EventId         char(32),
        @EventTimeUtc    datetime,
        @EventTime       datetime,
        @EventType       nvarchar(256),
        @EventSequence   decimal(19,0),
        @EventOccurrence decimal(19,0),
        @EventCode       int,
        @EventDetailCode int,
        @Message         nvarchar(1024),
        @ApplicationPath nvarchar(256),
        @ApplicationVirtualPath nvarchar(256),
        @MachineName    nvarchar(256),
        @RequestUrl      nvarchar(1024),
        @ExceptionType   nvarchar(256),
        @Details         ntext
AS
BEGIN
    INSERT
        dbo.aspnet_WebEvent_Events
        (
            EventId,
            EventTimeUtc,
            EventTime,
            EventType,
            EventSequence,
            EventOccurrence,
            EventCode,
            EventDetailCode,
            Message,
            ApplicationPath,
            ApplicationVirtualPath,
            MachineName,
            RequestUrl,
            ExceptionType,
            Details
        )
    VALUES
    (
        @EventId,
        @EventTimeUtc,
        @EventTime,
        @EventType,
        @EventSequence,
        @EventOccurrence,
        @EventCode,
        @EventDetailCode,
        @Message,
        @ApplicationPath,
        @ApplicationVirtualPath,
        @MachineName,
        @RequestUrl,
        @ExceptionType,
        @Details
    )
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[Attendance_Employee_API]...';


GO

CREATE PROCEDURE [dbo].[Attendance_Employee_API]
	 @SchoolID int,
	 @Entry_DateTime datetime, 
	 @EmployeeID int
	 
AS
BEGIN

	SET NOCOUNT ON;
	DECLARE  @Attendance_Date date
	DECLARE  @EntryTime time(7)
    DECLARE  @Employee_Attendance_ScheduleID int
	DECLARE  @LateEntryTime time(7)
	DECLARE  @StartTime time(7)
	DECLARE  @EndTime time(7)

	DECLARE  @AttendanceStatus nvarchar(50)


 set @Attendance_Date	=  CONVERT(date, @Entry_DateTime)
 set @EntryTime = cast(@Entry_DateTime as time) 

DECLARE @EducationYearID int
SELECT @EducationYearID = EducationYearID FROM  Education_Year WHERE  (Status = N'True') AND (SchoolID = @SchoolID)


SELECT @Employee_Attendance_ScheduleID = Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID,
 @LateEntryTime = Employee_Attendance_Schedule.LateEntryTime,
 @StartTime= Employee_Attendance_Schedule.StartTime,
 @EndTime = Employee_Attendance_Schedule.EndTime

FROM Employee_Attendance_Schedule_Assign INNER JOIN Employee_Attendance_Schedule ON Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID = Employee_Attendance_Schedule.Employee_Attendance_ScheduleID
WHERE (Employee_Attendance_Schedule_Assign.SchoolID = @SchoolID) AND (Employee_Attendance_Schedule_Assign.EmployeeID = @EmployeeID)




if( @LateEntryTime < @EntryTime)
BEGIN

SELECT Employee_Attendance_Schedule_Assign.Employee_Schedule_AssignID, Employee_Attendance_Schedule_Assign.EmployeeID Into #Temp_Attendance_Assign FROM  dbo.Employee_Attendance_Schedule_Assign INNER JOIN
dbo.Employee_Info ON dbo.Employee_Attendance_Schedule_Assign.EmployeeID = dbo.Employee_Info.EmployeeID
WHERE (dbo.Employee_Attendance_Schedule_Assign.SchoolID = @SchoolID) AND (dbo.Employee_Attendance_Schedule_Assign.Employee_Attendance_ScheduleID = @Employee_Attendance_ScheduleID) AND (dbo.Employee_Info.Job_Status = N'Active')


--loop start ------------------
	DECLARE  @Employee_Schedule_AssignID int
	DECLARE  @Loop_EmployeeID int 


While EXISTS(SELECT * From #Temp_Attendance_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @Employee_Schedule_AssignID = Employee_Schedule_AssignID , @Loop_EmployeeID = EmployeeID  From #Temp_Attendance_Assign
  
  
  IF EXISTS (SELECT * FROM  Employee_Leave WHERE (SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (@Attendance_Date BETWEEN LeaveStartDate AND LeaveEndDate))
 BEGIN
  Set @AttendanceStatus='Leave'

  IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (AttendanceDate = @Attendance_Date))
   BEGIN
     INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate,  ExitConfirmed_Status)
                                       VALUES(@SchoolID,0, @EducationYearID, @Loop_EmployeeID, @AttendanceStatus, @Attendance_Date, 'Leave')
   END
 END 
 ELSE
BEGIN
Set @AttendanceStatus='Abs'

  IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @Loop_EmployeeID) AND (AttendanceDate = @Attendance_Date))
   BEGIN
     INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate,  ExitConfirmed_Status)
                                       VALUES(@SchoolID,0, @EducationYearID, @Loop_EmployeeID, @AttendanceStatus, @Attendance_Date, 'Abs')
   END
END  
    Delete #Temp_Attendance_Assign Where Employee_Schedule_AssignID = @Employee_Schedule_AssignID
 END
DROP TABLE #Temp_Attendance_Assign
END




IF NOT EXISTS (SELECT * FROM  Employee_Attendance_Record WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date))
 BEGIN
 IF(@StartTime >= @EntryTime)
  Set @AttendanceStatus='Pre'

 IF((@StartTime < @EntryTime) AND (@EntryTime <= @LateEntryTime))
   Set @AttendanceStatus='Late'

 IF(@EntryTime < @EndTime)
  INSERT INTO Employee_Attendance_Record (SchoolID, RegistrationID, EducationYearID, EmployeeID, AttendanceStatus, AttendanceDate, EntryTime, ExitConfirmed_Status)
  VALUES(@SchoolID,0, @EducationYearID, @EmployeeID, @AttendanceStatus, @Attendance_Date, @EntryTime,'No')
 END
ELSE
 BEGIN
 --If Employee Entry After Late Entry Time 
   IF((@LateEntryTime < @EntryTime) AND(@EntryTime < @EndTime))
    BEGIN
     Set @AttendanceStatus='Late Abs'
	 UPDATE Employee_Attendance_Record SET  ExitConfirmed_Status = 'No', EntryTime = @EntryTime, AttendanceStatus = @AttendanceStatus WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date) AND (ExitTime IS NULL) AND (AttendanceStatus='Abs')
	END 

  IF(@EndTime <= @EntryTime)
   BEGIN
    UPDATE Employee_Attendance_Record SET ExitTime = @EntryTime, ExitConfirmed_Status = 'Yes' WHERE(SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @Attendance_Date)
   END
 END
END
GO
PRINT N'Creating Procedure [dbo].[Attendance_Students_API]...';


GO

CREATE PROCEDURE [dbo].[Attendance_Students_API]
	 @SchoolID int,
	 @Entry_DateTime datetime, 
	 @StudentID int
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE  @Attendance_Date date
	DECLARE  @EntryTime time(7)
    DECLARE  @ScheduleID int
	DECLARE  @StartTime time(7)
	DECLARE  @EndTime time(7)
    DECLARE  @LateEntryTime time(7)
	DECLARE  @AttendanceStatus nvarchar(50)
	DECLARE  @ClassID int
	DECLARE  @StudentClassID int
	DECLARE  @Day nvarchar(50)

	 set @Attendance_Date	=  CONVERT(date, @Entry_DateTime)
     set @EntryTime = cast(@Entry_DateTime as time) 
	 set @Day = datename(dw,@Entry_DateTime) 

DECLARE @EducationYearID int
SELECT @EducationYearID = EducationYearID FROM  Education_Year WHERE  (Status = N'True') AND (SchoolID = @SchoolID)

SELECT @ScheduleID =  Attendance_Schedule_AssignStudent.ScheduleID,@StartTime = Attendance_Schedule_Day.StartTime,@EndTime = Attendance_Schedule_Day.EndTime,@LateEntryTime = Attendance_Schedule_Day.LateEntryTime
FROM   Attendance_Schedule_Day INNER JOIN Attendance_Schedule_AssignStudent ON Attendance_Schedule_Day.ScheduleID = Attendance_Schedule_AssignStudent.ScheduleID
WHERE  (Attendance_Schedule_AssignStudent.SchoolID = @SchoolID) AND (Attendance_Schedule_AssignStudent.StudentID = @StudentID) AND (Attendance_Schedule_Day.EducationYearID = @EducationYearID) AND (Attendance_Schedule_Day.Day = @Day)


if(@LateEntryTime < @EntryTime)
BEGIN
SELECT Attendance_Schedule_AssignStudent.Schedule_AssignStuID, Attendance_Schedule_AssignStudent.StudentID Into #Temp_Attendance_Assign
FROM Attendance_Schedule_AssignStudent INNER JOIN Student ON Attendance_Schedule_AssignStudent.StudentID = Student.StudentID
WHERE (Attendance_Schedule_AssignStudent.SchoolID = @SchoolID) AND (Attendance_Schedule_AssignStudent.EducationYearID = @EducationYearID) AND (Attendance_Schedule_AssignStudent.ScheduleID = @ScheduleID) AND (Student.Status = N'Active')


--loop start ------------------
	DECLARE  @Schedule_AssignStuID int
	DECLARE  @Loop_StudentID int 


While EXISTS(SELECT * From #Temp_Attendance_Assign)
Begin
--get data row by row into variable 
  SELECT Top 1 @Schedule_AssignStuID = Schedule_AssignStuID , @Loop_StudentID = StudentID From #Temp_Attendance_Assign

  SELECT @StudentClassID = StudentClassID,@ClassID = ClassID FROM StudentsClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentID = @Loop_StudentID)
  
  IF EXISTS (SELECT * FROM  Attendance_Leave WHERE (SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (@Attendance_Date BETWEEN StartDate AND EndDate))
 BEGIN
  Set @AttendanceStatus='Leave'

  IF NOT EXISTS (SELECT * FROM Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
   BEGIN
     INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, Attendance, AttendanceDate, ExitConfirmed_Status)
     VALUES(@SchoolID,0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID, @AttendanceStatus, @Attendance_Date, 'Leave')
   END
 END 
 ELSE
BEGIN
Set @AttendanceStatus='Abs'

  IF NOT EXISTS (SELECT * FROM Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @Loop_StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
   BEGIN
     INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID,ClassID, StudentClassID, Attendance, AttendanceDate, ExitConfirmed_Status)
     VALUES(@SchoolID,0, @EducationYearID, @Loop_StudentID, @ClassID, @StudentClassID,@AttendanceStatus, @Attendance_Date, 'Abs')
   END
END  
    Delete #Temp_Attendance_Assign Where Schedule_AssignStuID = @Schedule_AssignStuID
 END
DROP TABLE #Temp_Attendance_Assign
END



IF NOT EXISTS (SELECT * FROM  Attendance_Record WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID))
 BEGIN
 IF(@StartTime >= @EntryTime)
  Set @AttendanceStatus='Pre'

 IF((@StartTime < @EntryTime) AND (@EntryTime <= @LateEntryTime))
   Set @AttendanceStatus='Late'

if(@EntryTime < @EndTime)
BEGIN
  SELECT @StudentClassID = StudentClassID,@ClassID = ClassID FROM StudentsClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentID = @StudentID)
  INSERT INTO Attendance_Record (SchoolID, RegistrationID, EducationYearID, StudentID,ClassID, StudentClassID, Attendance, AttendanceDate, EntryTime, ExitConfirmed_Status)
  VALUES(@SchoolID,0, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @AttendanceStatus, @Attendance_Date, @EntryTime,'No')
END
 END
ELSE
 BEGIN
 --If Employee Entry After Late Entry Time 
   IF((@LateEntryTime < @EntryTime) AND(@EntryTime < @EndTime))
    BEGIN
     Set @AttendanceStatus='Late Abs'
	 UPDATE Attendance_Record SET ExitConfirmed_Status = 'No', EntryTime = @EntryTime, Attendance = @AttendanceStatus WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID) AND (Attendance = 'Abs')
	END 

  IF(@EndTime <= @EntryTime)
   BEGIN
    UPDATE Attendance_Record SET ExitTime = @EntryTime, ExitConfirmed_Status = 'Yes' WHERE(SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (AttendanceDate = @Attendance_Date) AND (EducationYearID = @EducationYearID)
   END
 END
END
GO
PRINT N'Creating Procedure [dbo].[Emp_Monthly_Salary_Report]...';


GO

CREATE PROCEDURE [dbo].[Emp_Monthly_Salary_Report]
 @SchoolID int ,
 @EducationYearID int,
 @RoleIDs nvarchar(Max) = null
AS
BEGIN
	SET NOCOUNT ON;
IF(@RoleIDs is not null)
BEGIN
SELECT        Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, Employee_Payorder_Monthly.MonthName, SUM(Employee_Payorder.PaidAmount) AS Paid, SUM(Employee_Payorder.Due) AS Due
FROM            Employee_Payorder INNER JOIN
                         VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID INNER JOIN
                         Employee_Payorder_Monthly ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Monthly.Employee_PayorderID
WHERE        (Employee_Payorder.SchoolID = @SchoolID) AND (Employee_Payorder.EducationYearID = @EducationYearID) AND Employee_Payorder.Employee_Payorder_NameID IN (Select id from dbo.In_Function_Parameter(@RoleIDs))
GROUP BY Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName, Employee_Payorder_Monthly.MonthName, Employee_Payorder.PaidAmount, 
                         Employee_Payorder_Monthly.MonthStartDate
ORDER BY Employee_Payorder_Monthly.MonthStartDate, VW_Emp_Info.ID
END
ELSE
BEGIN
SELECT        Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, Employee_Payorder_Monthly.MonthName, SUM(Employee_Payorder.PaidAmount) AS Paid , SUM(Employee_Payorder.Due) AS Due
FROM            Employee_Payorder INNER JOIN
                         VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID INNER JOIN
                         Employee_Payorder_Monthly ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Monthly.Employee_PayorderID
WHERE        (Employee_Payorder.SchoolID = @SchoolID) AND (Employee_Payorder.EducationYearID = @EducationYearID) 
GROUP BY Employee_Payorder.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName, Employee_Payorder_Monthly.MonthName, Employee_Payorder.PaidAmount, 
                         Employee_Payorder_Monthly.MonthStartDate
ORDER BY Employee_Payorder_Monthly.MonthStartDate, VW_Emp_Info.ID
END
END;
GO
PRINT N'Creating Procedure [dbo].[Emp_Salary_Monthly]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Emp_Salary_Monthly]
	 @SchoolID int,
	 @RegistrationID int,
	 @EducationYearID int,
	 @EmployeeID int,
	 @Employee_Payorder_NameID int,

	 @Get_date date,
	 @MonthName nvarchar(50),

     @GeT_Employee_PayorderID int out
AS
BEGIN
	SET NOCOUNT ON;  
	DECLARE  @PayorderAmount float
	DECLARE  @IS_Abs_Deducted bit
	DECLARE  @Abs_Deduction float
	DECLARE  @IS_Late_Count_As_Abs bit
	DECLARE  @Employee_PayorderID int 
	DECLARE  @Late_Days int 


IF NOT EXISTS(SELECT * FROM  Employee_Payorder_Monthly WHERE([MonthName] = @MonthName) AND (EmployeeID = @EmployeeID))
BEGIN

	SELECT @PayorderAmount = Salary, @IS_Abs_Deducted = IS_Abs_Deducted, @Abs_Deduction = Abs_Deduction,@IS_Late_Count_As_Abs = IS_Late_Count_As_Abs , @Late_Days =Late_Days FROM  Employee_Info WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)



	INSERT INTO Employee_Payorder
                         (SchoolID, RegistrationID, EducationYearID, EmployeeID, Employee_Payorder_NameID, PayorderAmount,  Employee_Payorder_SN)
                VALUES (@SchoolID, @RegistrationID,@EducationYearID,@EmployeeID,@Employee_Payorder_NameID, @PayorderAmount, [dbo].[Employee_Payorder_SN](@SchoolID))

--get the Employee_PayorderID
  set  @Employee_PayorderID = (SELECT SCOPE_IDENTITY())

--insert  Employee_Payorder_Monthly Table
  DECLARE @S_date date = DATEADD(mm, DATEDIFF(mm, 0, @Get_date), 0)
  DECLARE @E_date date = DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, @Get_date) + 1, 0))

  DECLARE  @Total_WorkingDays int


-- Total Working days of Employees
	SELECT @Total_WorkingDays = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
    WHERE (SchoolID = @SchoolID)  AND (EmployeeID = @EmployeeID) AND (AttendanceDate BETWEEN @S_date AND @E_date)

--Total Absent in Month
DECLARE  @Total_Abs int
DECLARE  @Total_Late int
DECLARE  @Total_Leave int
DECLARE  @Total_LateCount int
DECLARE  @Fine_Amount float
DECLARE  @FineCountDays int
DECLARE  @Total_Pre int

---Total Pre
SELECT @Total_Pre = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus ='Pre') AND (AttendanceDate BETWEEN @S_date AND @E_date)
--- Total Abs
SELECT @Total_Abs = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus ='Abs') AND (AttendanceDate BETWEEN @S_date AND @E_date)
--- Total Late and Late Abs 
SELECT @Total_Late = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus in('Late','Late Abs')) AND (AttendanceDate BETWEEN @S_date AND @E_date)
---Total Leave
SELECT @Total_Leave = COUNT(AttendanceStatus) FROM Employee_Attendance_Record
WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceStatus = 'Leave') AND (AttendanceDate BETWEEN @S_date AND @E_date)

---------is Late Deducted--------------------
  IF(@IS_Late_Count_As_Abs = 1)
    BEGIN
	  set @Total_LateCount = @Total_Late / @Late_Days
   END
 ELSE
   BEGIN
      SET  @Total_LateCount = 0
   END

   SET @FineCountDays = @Total_Abs + @Total_LateCount

   SET @Fine_Amount =  @FineCountDays * @Abs_Deduction

  INSERT INTO Employee_Payorder_Monthly
                     (Employee_PayorderID, SchoolID, RegistrationID, EducationYearID, EmployeeID, [MonthName], MonthStartDate, MonthEndDate, Amount, WorkingDays,FineCountDays,FineAmount,LateDays, LeaveDays, AbsDays, PerDays)
            VALUES  (@Employee_PayorderID,@SchoolID,@RegistrationID,@EducationYearID,@EmployeeID, @MonthName,  @S_date,        @E_date, @PayorderAmount, @Total_WorkingDays,@FineCountDays,@Fine_Amount, @Total_Late, @Total_Leave, @Total_Abs, @Total_Pre)


--Employee_Allowance_Assign are insert to records
SELECT AllowanceAssignID, AllowanceID, AllowanceAmount, Fixed_Percetage  Into #Temp_Allowance_Assign  FROM  Employee_Allowance_Assign WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)
--loop start ------------------
	DECLARE  @AllowanceAssignID int
	DECLARE  @AllowanceID int 
	DECLARE  @Amount float
	DECLARE  @Fixed_Percetage nvarchar(50)
	DECLARE  @AllowanceAmount float

While EXISTS(SELECT * From #Temp_Allowance_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @AllowanceAssignID = AllowanceAssignID,@AllowanceID =  AllowanceID,  @Amount = AllowanceAmount, @Fixed_Percetage = Fixed_Percetage   From #Temp_Allowance_Assign
  
  if(@Fixed_Percetage ='Fixed')
      set @AllowanceAmount = @Amount
  else
  set @AllowanceAmount = (@PayorderAmount *  @Amount)/100

   INSERT INTO Employee_Allowance_Records
       (SchoolID, RegistrationID, AllowanceID, EmployeeID, Employee_PayorderID, AllowanceAmount)
VALUES (@SchoolID, @RegistrationID, @AllowanceID, @EmployeeID, @Employee_PayorderID, @AllowanceAmount)
   
   Delete #Temp_Allowance_Assign Where AllowanceAssignID = @AllowanceAssignID
 END
 DROP TABLE #Temp_Allowance_Assign


--Employee_Deduction_Assign are insert to records
SELECT DeductionAssignID, DeductionID, DeductionAmount, Fixed_Percetage  Into #Temp_Employee_Deduction_Assign  FROM  Employee_Deduction_Assign WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)
--loop start ------------------
	DECLARE  @DeductionAssignID int
	DECLARE  @DeductionID int 
	DECLARE  @D_Amount float
	DECLARE  @D_Fixed_Percetage nvarchar(50)
	DECLARE  @DeductionAmount float

While EXISTS(SELECT * From #Temp_Employee_Deduction_Assign)
Begin
--get data row by row into variable 
    Select Top 1 @DeductionAssignID = DeductionAssignID,@DeductionID =  DeductionID,  @D_Amount = DeductionAmount, @D_Fixed_Percetage = Fixed_Percetage   From #Temp_Employee_Deduction_Assign
  
  if(@D_Fixed_Percetage ='Fixed')
      set @DeductionAmount = @D_Amount
  else
  set @DeductionAmount = (@PayorderAmount *  @D_Amount)/100

   INSERT INTO Employee_Deduction_Records
       (SchoolID, RegistrationID, DeductionID, EmployeeID, Employee_PayorderID, Deduction_Amount)
VALUES (@SchoolID, @RegistrationID, @DeductionID, @EmployeeID, @Employee_PayorderID, @DeductionAmount)
   
   Delete #Temp_Employee_Deduction_Assign Where DeductionAssignID = @DeductionAssignID
 END
 DROP TABLE #Temp_Employee_Deduction_Assign

 SET @GeT_Employee_PayorderID = @Employee_PayorderID

 RETURN @GeT_Employee_PayorderID
 END
END
GO
PRINT N'Creating Procedure [dbo].[Exam_Mark_Re_Submit]...';


GO
--7. ALTER PROCEDURE [dbo].[Exam_Mark_Re_Submit]


CREATE PROCEDURE [dbo].[Exam_Mark_Re_Submit]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@ExamID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

--  UPDATE FullMark,PassMark,ObtainedPercentage,PassPercentage

 UPDATE       Exam_Obtain_Marks
    SET            FullMark =Exam_Full_Marks.FullMarks, 
                   PassMark =Exam_Full_Marks.Sub_PassMarks, 
				     ObtainedPercentage = ROUND((ISNULL(Exam_Obtain_Marks.MarksObtained ,0) * 100)/Exam_Full_Marks.FullMarks, 2, 0) , 
				     PassPercentage = ROUND((Exam_Full_Marks.Sub_PassMarks * 100 ) /Exam_Full_Marks.FullMarks, 2, 0)
FROM            Exam_Obtain_Marks INNER JOIN
                         Exam_Full_Marks ON Exam_Obtain_Marks.SchoolID = Exam_Full_Marks.SchoolID AND Exam_Obtain_Marks.ExamID = Exam_Full_Marks.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Full_Marks.ClassID AND 
                         Exam_Obtain_Marks.SubjectID = Exam_Full_Marks.SubjectID AND ISNULL(Exam_Obtain_Marks.SubExamID, 0) = ISNULL(Exam_Full_Marks.SubExamID, 0) AND 
                         Exam_Obtain_Marks.EducationYearID = Exam_Full_Marks.EducationYearID
WHERE        (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ExamID = @ExamID)

--  UPDATE GradingID,ObtainedGrades,ObtainedPoint

 UPDATE       Exam_Obtain_Marks
    SET      	   GradingID = Exam_Grading_System.GradingID, 
 				   ObtainedGrades = Exam_Grading_System.Grades, 
 				   ObtainedPoint = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Obtain_Marks ON Exam_Grading_System.MinPercentage <= Exam_Obtain_Marks.ObtainedPercentage AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Obtain_Marks.ObtainedPercentage INNER JOIN
                         Exam_Grading_Assign ON Exam_Obtain_Marks.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Obtain_Marks.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Obtain_Marks.ClassID = Exam_Grading_Assign.ClassID AND Exam_Obtain_Marks.ExamID = Exam_Grading_Assign.ExamID AND Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID AND 
                         Exam_Grading_System.SchoolID = Exam_Grading_Assign.SchoolID
WHERE        (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ExamID = @ExamID)


END


---------------------------------------------------------------------------------------------------------------------------------------------
--8. ALTER PROCEDURE [dbo].[Exam_Mark_Submit]
GO
PRINT N'Creating Procedure [dbo].[Exam_Mark_Submit]...';


GO

CREATE PROCEDURE [dbo].[Exam_Mark_Submit]

    @SchoolID int,
	@RegistrationID int,
	@EducationYearID int,
	@StudentID int,
	@ClassID int,
	@ExamID int,
	@SubjectID int,
	@SubExamID int,

	@MarksObtained float,
	@AbsenceStatus nvarchar(50),
	@FullMark float,
	@PassPercentage float,
	@PassMark float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @StudentResultID int
    DECLARE @StudentClassID int
	DECLARE @StudentRecordID int
	DECLARE @ObtainedPercentage float

	DECLARE @GradingID int
	DECLARE @Grades nvarchar(50)
	DECLARE @Point float


	SELECT @StudentClassID = StudentClassID FROM StudentsClass WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SchoolID = @SchoolID)

	SELECT @StudentRecordID = StudentRecordID FROM  StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SubjectID = @SubjectID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)



 IF NOT EXISTS (SELECT StudentResultID FROM Exam_Result_of_Student WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID)
 BEGIN
	INSERT INTO Exam_Result_of_Student
           (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, ExamID,Date)
    VALUES (@SchoolID,@RegistrationID,@EducationYearID,@StudentID,@StudentClassID,@ClassID,@ExamID,GETDATE())

	set @StudentResultID = SCOPE_IDENTITY();
 END

ELSE
 BEGIN
    SELECT @StudentResultID =  StudentResultID FROM Exam_Result_of_Student WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID
 END

 IF NOT EXISTS (SELECT * FROM Exam_Result_of_Subject WHERE SchoolID =@SchoolID AND EducationYearID = @EducationYearID AND StudentClassID = @StudentClassID AND ExamID = @ExamID AND SubjectID = @SubjectID AND StudentResultID = @StudentResultID)
 BEGIN
 INSERT INTO Exam_Result_of_Subject
         (SchoolID, RegistrationID, EducationYearID, StudentID, StudentClassID, ClassID, ExamID, StudentRecordID, SubjectID, StudentResultID, Date)
 VALUES  (@SchoolID,@RegistrationID,@EducationYearID,@StudentID,@StudentClassID,@ClassID,@ExamID,@StudentRecordID,@SubjectID,@StudentResultID,GETDATE())
 END


 SET @ObtainedPercentage = (ISNULL(@MarksObtained,0) * 100)/@FullMark

 SELECT TOP (1) @GradingID = Exam_Grading_System.GradingID, 
                @Grades = Exam_Grading_System.Grades,
			    @Point  = Exam_Grading_System.Point
FROM   Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_System.MinPercentage <= @ObtainedPercentage) AND (Exam_Grading_Assign.SchoolID = @SchoolID) AND (Exam_Grading_Assign.EducationYearID = @EducationYearID) AND 
                         (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
ORDER BY Exam_Grading_System.Point DESC



  IF NOT EXISTS (SELECT * From Exam_Obtain_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) AND (ExamID = @ExamID) AND (SubjectID = @SubjectID) AND (StudentResultID = @StudentResultID) AND (SubExamID = @SubExamID OR SubExamID IS NULL))
 BEGIN
 INSERT INTO Exam_Obtain_Marks
         (SchoolID, RegistrationID, StudentID, SubjectID, ClassID, ExamID, SubExamID, StudentClassID, EducationYearID, StudentRecordID, StudentResultID, 
		 MarksObtained, AbsenceStatus, FullMark, ObtainedPercentage, PassPercentage, Date,GradingID,ObtainedGrades,ObtainedPoint,PassMark)
 VALUES  (@SchoolID,@RegistrationID,@StudentID,@SubjectID,@ClassID,@ExamID,@SubExamID,@StudentClassID,@EducationYearID,@StudentRecordID,@StudentResultID,
         @MarksObtained,@AbsenceStatus,@FullMark,@ObtainedPercentage,@PassPercentage,GETDATE(),@GradingID,@Grades,@Point,@PassMark)
 END
   ELSE
   BEGIN
    update Exam_Obtain_Marks set MarksObtained = @MarksObtained , SubExamID = @SubExamID ,AbsenceStatus = @AbsenceStatus,FullMark = @FullMark,ObtainedPercentage = @ObtainedPercentage, PassPercentage = @PassPercentage,GradingID = @GradingID,ObtainedGrades = @Grades,ObtainedPoint = @Point, PassMark = @PassMark
	 Where StudentClassID = @StudentClassID and SubjectID = @SubjectID and ExamID = @ExamID and (SubExamID = @SubExamID or SubExamID is null)
   END

END

----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--9. ALTER PROCEDURE [dbo].[SP_Exam_Subject]
GO
PRINT N'Creating Procedure [dbo].[Examinee_Vs_Grade]...';


GO
--24.
--ALTER PROCEDURE [dbo].[Examinee_Vs_Grade]
CREATE PROCEDURE [dbo].[Examinee_Vs_Grade]
 @SchoolID int, 
 @EducationYearID int ,
 @ClassID int ,
 @ExamID int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select *,count(*) as NoOfStudent,100.0 * COUNT(*)/(SELECT count(*) FROM Exam_Result_of_Student
WHERE(SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID))as Percentage from (SELECT Student_Grade FROM Exam_Result_of_Student
WHERE(SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)) as Exam_Result_of_Student_1

GROUP BY Exam_Result_of_Student_1.Student_Grade
END
GO
PRINT N'Creating Procedure [dbo].[HighestMark_Position]...';


GO
--15.
--ALTER PROCEDURE [dbo].[HighestMark_Position]
CREATE PROCEDURE [dbo].[HighestMark_Position]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@ExamID int,
	@Exam_Position_Format nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    

----------------------------------------------------------------------------------------------------------------------------------------------------
---Position_InExam_Class --------HighestMark_InExam_Class---------------Position_InExam_Subsection

declare @HighestMark_InExam_Class float
--for HighestMark_InExam_Class -----
SELECT @HighestMark_InExam_Class = MAX(ObtainedMark_ofStudent) FROM Exam_Result_of_Student WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)


if(@Exam_Position_Format = 'Point')
BEGIN
  UPDATE  Exam_Result_of_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Result_of_Student INNER JOIN
  (
   SELECT DENSE_RANK() OVER (Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.NotGolden, Exam_Result_of_Student.Student_Point DESC,Exam_Result_of_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.NotGolden, Exam_Result_of_Student.Student_Point DESC,Exam_Result_of_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Result_of_Student.Student_Point,Exam_Result_of_Student.ObtainedMark_ofStudent,Exam_Result_of_Student.StudentResultID 
   FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
   WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
   (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
   (Exam_Result_of_Student.ClassID = @ClassID) AND 
   (Exam_Result_of_Student.ExamID = @ExamID) 
  ) as a
  ON Exam_Result_of_Student.StudentResultID = a.StudentResultID  
 END
ELSE
 BEGIN
   UPDATE  Exam_Result_of_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Result_of_Student INNER JOIN
   (
    SELECT DENSE_RANK() OVER (Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Result_of_Student.IsFailed, Exam_Result_of_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Result_of_Student.ObtainedMark_ofStudent,Exam_Result_of_Student.StudentResultID 
    FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
    WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
    (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
    (Exam_Result_of_Student.ClassID = @ClassID) AND 
    (Exam_Result_of_Student.ExamID = @ExamID) 
   ) as a
  ON Exam_Result_of_Student.StudentResultID = a.StudentResultID  
END


----------------------------------------------------------------------------------------------------------------------------------------------
-----------HighestMark_InExam_Subsection

UPDATE  Exam_Result_of_Student
SET       HighestMark_InExam_Subsection = a.HighestMark_InExam_Subsection
FROM  Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Result_of_Student.ObtainedMark_ofStudent)as HighestMark_InExam_Subsection ,
StudentsClass.SectionID,StudentsClass.ShiftID,StudentsClass.SubjectGroupID 

FROM Exam_Result_of_Student INNER JOIN StudentsClass ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
(Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Student.ClassID = @ClassID) AND 
(Exam_Result_of_Student.ExamID = @ExamID)
group by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a

ON StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
(Exam_Result_of_Student.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Student.ClassID = @ClassID) AND 
(Exam_Result_of_Student.ExamID = @ExamID)



--------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------

-----------HighestMark_InSubject_Class

UPDATE Exam_Result_of_Subject
SET HighestMark_InSubject_Class = a.HighestMark_InSubject_Class
FROM Exam_Result_of_Subject INNER JOIN
 (SELECT MAX(ObtainedMark_ofSubject) AS HighestMark_InSubject_Class, SubjectID, SchoolID, EducationYearID, ClassID, ExamID
  FROM  Exam_Result_of_Subject GROUP BY SubjectID, SchoolID, EducationYearID, ClassID, ExamID) AS a ON Exam_Result_of_Subject.SubjectID = a.SubjectID AND Exam_Result_of_Subject.SchoolID = a.SchoolID AND 
  Exam_Result_of_Subject.EducationYearID = a.EducationYearID AND Exam_Result_of_Subject.ClassID = a.ClassID AND Exam_Result_of_Subject.ExamID = a.ExamID
  WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
       (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
       (Exam_Result_of_Subject.ClassID = @ClassID) AND 
       (Exam_Result_of_Subject.ExamID = @ExamID)

-------------------------------------------------------------------------------------------------------------------------------------

--For HighestMark_InSubject_Subsection-------------------------

UPDATE  Exam_Result_of_Subject
SET       HighestMark_InSubject_Subsection = a.Mark_ofSubject
FROM  Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Result_of_Subject.ObtainedMark_ofSubject) as Mark_ofSubject,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Subject.ClassID = @ClassID) AND 
(Exam_Result_of_Subject.ExamID = @ExamID) 
group by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a 
ON Exam_Result_of_Subject.SubjectID = a.SubjectID and StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND 
(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Result_of_Subject.ClassID = @ClassID) AND 
(Exam_Result_of_Subject.ExamID = @ExamID) 


---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

--For Position_InSubject_Class-------- Position_InSubject_Subsection-------------------------


if(@Exam_Position_Format = 'Point')
BEGIN
	UPDATE  Exam_Result_of_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Result_of_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY SubjectPoint DESC, ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Result_of_Subject.SubjectPoint DESC, Exam_Result_of_Subject.ObtainedMark_ofSubject DESC,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Result_of_Subject.SubjectPoint,Exam_Result_of_Subject.ObtainedMark_ofSubject,Exam_Result_of_Subject.SubjectResultID ,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
	FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
	WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND
	(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
	(Exam_Result_of_Subject.ClassID = @ClassID) AND 
	(Exam_Result_of_Subject.ExamID = @ExamID)) as a
	ON Exam_Result_of_Subject.SubjectResultID = a.SubjectResultID
 END
ELSE
 BEGIN


	UPDATE  Exam_Result_of_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Result_of_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Result_of_Subject.ObtainedMark_ofSubject DESC,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Result_of_Subject.ObtainedMark_ofSubject,Exam_Result_of_Subject.SubjectResultID ,Exam_Result_of_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
	FROM Exam_Result_of_Subject INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID
	WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND
	(Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
	(Exam_Result_of_Subject.ClassID = @ClassID) AND 
	(Exam_Result_of_Subject.ExamID = @ExamID)) as a
	ON Exam_Result_of_Subject.SubjectResultID = a.SubjectResultID

END


------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


END
GO
PRINT N'Creating Procedure [dbo].[Income_Category_Report]...';


GO
CREATE PROCEDURE [dbo].[Income_Category_Report]
 @SchoolID int ,
 @EducationYearID int,
 @From_Date date,
 @To_Date date,
 @SectionID nvarchar(50),
 @ClassID int,
 @RoleID nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

SELECT Income_PaymentRecord.RoleID, CreateClass.Class, CreateSection.Section, Income_Roles.Role, Income_PaymentRecord.PaidAmount, StudentsClass.ClassID
FROM StudentsClass INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN
                         Income_PaymentRecord INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID ON StudentsClass.StudentClassID = Income_PaymentRecord.StudentClassID LEFT OUTER JOIN
                         CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_PaymentRecord.EducationYearID = @EducationYearID) AND (CAST(Income_PaymentRecord.PaidDate AS date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (StudentsClass.SectionID like @SectionID) AND ((StudentsClass.ClassID = @ClassID) OR @ClassID = 0) AND (Income_PaymentRecord.RoleID LIKE @RoleID)
ORDER BY CreateClass.ClassID
END;
GO
PRINT N'Creating Procedure [dbo].[Income_Daily_Report]...';


GO

CREATE PROCEDURE [dbo].[Income_Daily_Report]
 @SchoolID int ,
 @From_Date date,
 @To_Date date

AS
BEGIN
	SET NOCOUNT ON;

SELECT * FROM(SELECT CAST(Income_PaymentRecord.PaidDate AS DATE) AS In_Date, Income_Roles.Role AS Category, Income_PaymentRecord.PaidAmount AS Amount
FROM  Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE(Income_PaymentRecord.SchoolID = @SchoolID) AND 
(CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))

union all

SELECT  Extra_Income.Extra_IncomeDate AS In_Date, Extra_IncomeCategory.Extra_Income_CategoryName AS Category, Extra_Income.Extra_IncomeAmount AS Amount
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')

union all

SELECT CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS In_Date, CommitteeDonationCategory.DonationCategory AS Category, CommitteePaymentRecord.PaidAmount AS Amount
FROM  CommitteePaymentRecord INNER JOIN
                         CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN
                         CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId  
WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) AND 
(CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))

) AS T
ORDER BY T.In_Date,T.Category
END;
GO
PRINT N'Creating Procedure [dbo].[Income_Monthly_Report]...';


GO

CREATE PROCEDURE [dbo].[Income_Monthly_Report]
 @SchoolID int ,
 @From_Date date,
 @To_Date date

AS
BEGIN
	SET NOCOUNT ON;

SELECT RIGHT(CONVERT(VARCHAR(11), MAX(T.Month), 106), 8) AS Month,  T.Category, SUM(T.Amount) AS Amount
FROM (
SELECT CAST(Income_PaymentRecord.PaidDate AS DATE) AS Month, Income_Roles.Role AS Category, Income_PaymentRecord.PaidAmount AS Amount
FROM  Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE(Income_PaymentRecord.SchoolID = @SchoolID) AND 
(CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
union all
SELECT Extra_Income.Extra_IncomeDate AS Month, Extra_IncomeCategory.Extra_Income_CategoryName AS Category, Extra_Income.Extra_IncomeAmount AS Amount
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) AND Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')
union all

SELECT CAST(CommitteeMoneyReceipt.PaidDate AS DATE) AS Month, CommitteeDonationCategory.DonationCategory AS Category, CommitteePaymentRecord.PaidAmount AS Amount
FROM  CommitteePaymentRecord INNER JOIN
                         CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN
                         CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId  
WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) AND 
(CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
) AS T
GROUP BY CAST(MONTH(T.Month) AS VARCHAR(2)) + '-' + CAST(YEAR(T.Month) AS VARCHAR(4)), T.Category
ORDER BY MAX(T.Month),T.Category
END;
GO
PRINT N'Creating Procedure [dbo].[Income_Stu_Class_MonthlyReport]...';


GO
CREATE PROCEDURE [dbo].[Income_Stu_Class_MonthlyReport]
 @SchoolID int ,
 @EducationYearID int,
 @SectionID nvarchar(50),
 @ClassID int,
 @RoleIDs nvarchar(Max)
AS
BEGIN
	SET NOCOUNT ON;
IF(@RoleIDs is not null)
BEGIN
SELECT StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8) AS Month, SUM(Income_PayOrder.PaidAmount) AS Amount
FROM  Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) 
 AND Student.Status='Active' AND Income_PayOrder.RoleID IN (Select id from dbo.In_Function_Parameter(@RoleIDs))
GROUP BY StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8)
ORDER BY MAX(Income_PayOrder.EndDate) ,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
END
ELSE
BEGIN
SELECT StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8) AS Month, SUM(Income_PayOrder.PaidAmount) AS Amount
FROM Income_PayOrder INNER JOIN Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN StudentsClass ON Income_PayOrder.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE(Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND Student.Status='Active' AND (StudentsClass.SectionID LIKE @SectionID)
GROUP BY StudentsClass.ClassID, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, RIGHT(CONVERT(VARCHAR(11), Income_PayOrder.EndDate, 106), 8)
ORDER BY MAX(Income_PayOrder.EndDate) ,CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS INT) ELSE 0 END
END
END;
GO
PRINT N'Creating Procedure [dbo].[Income_Stu_Class_Report]...';


GO
create PROCEDURE [dbo].[Income_Stu_Class_Report]
 @SchoolID int ,
 @EducationYearID int,
 @From_Date date,
 @To_Date date,
 @SectionID nvarchar(50),
 @ClassID int,
 @RoleID nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

SELECT StudentsClass.ClassID, Income_PaymentRecord.RoleID, CreateClass.Class, CreateSection.Section, StudentsClass.RollNo, Student.ID, Student.StudentsName, Income_Roles.Role, 
      Income_PaymentRecord.PaidAmount,Income_PaymentRecord.PaidDate, RIGHT(CONVERT(VARCHAR(11), Income_PaymentRecord.PaidDate, 106), 8) AS Month
FROM  StudentsClass INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN
                         Income_PaymentRecord INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID ON StudentsClass.StudentClassID = Income_PaymentRecord.StudentClassID INNER JOIN
                         Student ON Income_PaymentRecord.StudentID = Student.StudentID LEFT OUTER JOIN
                         CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND (Income_PaymentRecord.EducationYearID = @EducationYearID) AND (CAST(Income_PaymentRecord.PaidDate AS date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (StudentsClass.SectionID like @SectionID) AND (StudentsClass.ClassID = @ClassID) AND (Income_PaymentRecord.RoleID LIKE @RoleID)
ORDER BY CreateClass.ClassID
END;
GO
PRINT N'Creating Procedure [dbo].[MoneyReceipt]...';


GO

CREATE PROCEDURE [dbo].[MoneyReceipt]
    @StudentID        INT,
    @RegistrationID   INT,
    @StudentClassID   INT,
    @EducationYearID  INT,
    @PaymentBy        NVARCHAR(128),
    @PaidDate         DATETIME,
    @SchoolID         INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @MoneyReceipt_SN INT
    SET @MoneyReceipt_SN = [dbo].[F_MoneyReceipt_SN](@SchoolID)

    INSERT INTO Income_MoneyReceipt
        (StudentID, RegistrationID, StudentClassID, PaidDate, EducationYearID, PaymentBy, SchoolID, MoneyReceipt_SN, CollectionDate)
    VALUES
        (@StudentID, @RegistrationID, @StudentClassID, @PaidDate, @EducationYearID, @PaymentBy, @SchoolID, @MoneyReceipt_SN, GETDATE())

    SELECT SCOPE_IDENTITY()
END
GO
PRINT N'Creating Procedure [dbo].[Result_of_Cumulative]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[Result_of_Cumulative]

-- Where condition parameters

	@ClassID int,
	@SchoolID int,
    @EducationYearID int

	
AS
BEGIN
select * from (SELECT Exam_Result_of_Student.StudentID, 
ROUND(SUM(Exam_Result_of_Student.TotalMark_ofStudent),2) AS TotalMark,
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent),2) AS ObtainedMark, 
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*),2) AS Avarage,
ROUND(AVG(Exam_Result_of_Student.Student_Point),2) AS Point,
(SELECT Grades FROM Exam_Grading_System WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND ( round (((SUM(Exam_Result_of_Student.ObtainedMark_ofStudent)/SUM(Exam_Result_of_Student.TotalMark_ofStudent))*100),0) BETWEEN MinPercentage AND MaxPercentage)) as Grade, 
DENSE_RANK() OVER (ORDER BY SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*) DESC) AS Position

FROM Exam_Result_of_Student INNER JOIN Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND  Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND   (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND   (Exam_Result_of_Student.ClassID = @ClassID) 
GROUP BY Exam_Result_of_Student.StudentID) as CU 	

INNER JOIN

(SELECT Exam_Result_of_Student.StudentID,
Student.ID, 
 Student.StudentsName, 
 CreateClass.Class, 
 Exam_Name.ExamName,
 Exam_Name.ExamID,
 ROUND(Exam_Result_of_Student.TotalMark_ofStudent, 2) AS exam_TM, 
 ROUND(Exam_Result_of_Student.ObtainedMark_ofStudent, 2) AS exam_OM,
 ROUND(Exam_Result_of_Student.Student_Point, 2) AS Exam_Point, 
 Exam_Result_of_Student.Student_Grade AS Exam_Grade

FROM Exam_Result_of_Student INNER JOIN
Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID INNER JOIN
Student ON Exam_Result_of_Student.StudentID = Student.StudentID INNER JOIN
CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND   (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND   (Exam_Result_of_Student.ClassID = @ClassID)) as Exam ON cu.StudentID = Exam.StudentID 
order by ID,ExamID
END
GO
PRINT N'Creating Procedure [dbo].[Result_of_Cumulative_Full_Class]...';


GO
CREATE PROCEDURE [dbo].[Result_of_Cumulative_Full_Class]
@ClassID int,
@SchoolID int,
@EducationYearID int
AS
BEGIN
SELECT Exam_Result_of_Student.StudentID, 
Student.ID, 
Student.StudentsName,
Student.SMSPhoneNo, 
ROUND(SUM(Exam_Result_of_Student.TotalMark_ofStudent),2) AS TotalMark,
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent),2) AS ObtainedMark, 
ROUND(SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*),2) AS Avarage,
ROUND(AVG(Exam_Result_of_Student.Student_Point),2) AS Point,
(SELECT Grades FROM Exam_Grading_System WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND ( round (((SUM(Exam_Result_of_Student.ObtainedMark_ofStudent)/SUM(Exam_Result_of_Student.TotalMark_ofStudent))*100),0) BETWEEN MinPercentage AND MaxPercentage)) as Grade, 
DENSE_RANK() OVER (ORDER BY SUM(Exam_Result_of_Student.ObtainedMark_ofStudent) / COUNT(*) DESC) AS Position

FROM Exam_Result_of_Student INNER JOIN
Exam_Cumulative_ExamList ON Exam_Result_of_Student.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Student.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
Exam_Result_of_Student.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
Student ON Exam_Result_of_Student.StudentID = Student.StudentID

WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) 
AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) 
AND (Exam_Result_of_Student.ClassID = @ClassID)
GROUP BY Exam_Result_of_Student.StudentID, Student.ID, Student.SMSPhoneNo, Student.StudentsName
END
GO
PRINT N'Creating Procedure [dbo].[SP_Cumulative_Attendance]...';


GO

CREATE PROCEDURE [dbo].[SP_Cumulative_Attendance]
    @SchoolID int,
    @EducationYearID int,
    @ClassID int,
    @CumulativeNameID int,
    @RegistrationID int,
    @From_Date date,
    @To_Date date,
    @ScheduleID int = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Attendance_Student
    WHERE (SchoolID = @SchoolID)
      AND (ClassID = @ClassID)
      AND (EducationYearID = @EducationYearID)
      AND (CumulativeNameID = @CumulativeNameID);

    IF (@From_Date <> '' AND @To_Date <> '')
    BEGIN
        INSERT INTO Attendance_Student
            (SchoolID, RegistrationID, EducationYearID, CumulativeNameID, ClassID, StudentID, StudentClassID,
             WorkingDays, TotalPresent, TotalAbsent, TotalLate, TotalLeave, TotalBunk, TotalLateAbs)
        SELECT
            @SchoolID,
            @RegistrationID,
            @EducationYearID,
            @CumulativeNameID,
            @ClassID,
            StudentsClass.StudentID,
            Attendance_Record.StudentClassID,
            COUNT(Attendance_Record.StudentClassID) AS WorkingDay,
            ISNULL(T_Pre.Pre, 0) AS Pre,
            ISNULL(T_Abs.Abs, 0) AS Abs,
            ISNULL(T_Late.Late, 0) AS Late,
            ISNULL(T_Leave.Leave, 0) AS Leave,
            ISNULL(T_Bunk.Bunk, 0) AS Bunk,
            ISNULL(T_LateAbs.LateAbs, 0) AS LateAbs
        FROM Attendance_Record
        INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Bunk
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Bunk')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Bunk ON Attendance_Record.StudentClassID = T_Bunk.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Abs
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Abs')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Abs ON Attendance_Record.StudentClassID = T_Abs.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Pre
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Pre')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Pre ON Attendance_Record.StudentClassID = T_Pre.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Late
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Late')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Late ON Attendance_Record.StudentClassID = T_Late.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Leave
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Leave')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Leave ON Attendance_Record.StudentClassID = T_Leave.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS LateAbs
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Late Abs')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_LateAbs ON Attendance_Record.StudentClassID = T_LateAbs.StudentClassID
        WHERE (Attendance_Record.SchoolID = @SchoolID)
          AND (Attendance_Record.ClassID = @ClassID)
          AND (Attendance_Record.EducationYearID = @EducationYearID)
          AND (Attendance_Record.AttendanceDate BETWEEN @From_Date AND @To_Date)
          AND (@ScheduleID = 0 OR ISNULL(Attendance_Record.Attendance_ScheduleID, 0) = @ScheduleID)
        GROUP BY Attendance_Record.StudentClassID, T_Abs.Abs, T_Pre.Pre, T_Leave.Leave, T_Late.Late,
                 StudentsClass.StudentID, T_Bunk.Bunk, T_LateAbs.LateAbs;
    END
END
GO
PRINT N'Creating Procedure [dbo].[SP_Cumulative_Exam_Student]...';


GO
--10.
--CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Student]

CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Student]

-- Where condition parameters
	@SchoolID int,
	@RegistrationID int,
    @EducationYearID int,
	@ClassID int,
	@CumulativeNameID int,
	@Cumulative_SettingID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
---[[[[[[[[[[[[[[[[[[[[[[-----------------DELETE----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
DELETE FROM Exam_Cumulative_Student WHERE (CumulativeNameID = @CumulativeNameID) and  (SchoolID = @SchoolID) and  (EducationYearID =@EducationYearID) and (ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[-----------------INSERT----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
INSERT INTO Exam_Cumulative_Student
            (Cumulative_SettingID,
			CumulativeNameID, 
			 SchoolID, 
			 RegistrationID, 
			 EducationYearID,
			 ClassID,

			 StudentID, 
			 StudentClassID, 
			 TotalSubjest_WithOptional,
			 TotalSubject,
			 TotalMark_ofStudent,
			 ObtainedMark_ofStudent,
			 PassPercentage_Student, 
			 TotalPoint)
SELECT 
       @Cumulative_SettingID,@CumulativeNameID,@SchoolID,@RegistrationID,@EducationYearID,@ClassID,       
       StudentID, 
	   StudentClassID,
       ROUND(COUNT(Cumulative_SubjectID), 2, 0) AS Total_Sub_WithOptional, 
	   ROUND(COUNT(Cumulative_SubjectID), 2, 0) AS TotalSubject,
       ROUND(SUM(TotalMark_ofSubject), 2, 0) AS TotalMark, 
	   ROUND(SUM(OMark_ofSub_ConsiderOptional), 2, 0) AS OMark, 
	   AVG(PassPercentage_Subject) AS PassPercentage, 
       ROUND(SUM(SubjectPoint_ConsiderOptional), 2, 0) AS  TotalPoint 

FROM Exam_Cumulative_Subject
WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
GROUP BY StudentID, StudentClassID

---------[[[[[[[[[[[[Update TotalSubject,TotalMark_ofStudent]]]]]]]]]]]]]]]---------
UPDATE Exam_Cumulative_Student
SET  TotalMark_ofStudent = Stu_Sub.TotalMark, TotalSubject = Stu_Sub.Total_Sub
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT        Exam_Cumulative_Subject.SchoolID, Exam_Cumulative_Subject.EducationYearID, Exam_Cumulative_Subject.CumulativeNameID, Exam_Cumulative_Subject.ClassID, 
                                                         Exam_Cumulative_Subject.StudentID, COUNT(Exam_Cumulative_Subject.Cumulative_SubjectID) AS Total_Sub, Exam_Cumulative_Subject.StudentClassID, 
                                                         SUM(Exam_Cumulative_Subject.TotalMark_ofSubject) AS TotalMark
                               FROM            Exam_Cumulative_Subject INNER JOIN
                                                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID AND 
                                                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                                                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID INNER JOIN
                                                         StudentRecord ON Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND 
                                                         Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentID = StudentRecord.StudentID
                               WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                                                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Add_Optional_Mark_In_FullMarks = 0) AND 
                                                         (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
                               GROUP BY Exam_Cumulative_Subject.SchoolID, Exam_Cumulative_Subject.EducationYearID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID, 
                                                         Exam_Cumulative_Subject.ClassID, Exam_Cumulative_Subject.CumulativeNameID) AS Stu_Sub ON Exam_Cumulative_Student.CumulativeNameID = Stu_Sub.CumulativeNameID AND 
                         Exam_Cumulative_Student.SchoolID = Stu_Sub.SchoolID AND Exam_Cumulative_Student.EducationYearID = Stu_Sub.EducationYearID AND Exam_Cumulative_Student.StudentID = Stu_Sub.StudentID AND 
                         Exam_Cumulative_Student.ClassID = Stu_Sub.ClassID AND Exam_Cumulative_Student.StudentClassID = Stu_Sub.StudentClassID

-----------------[[[[[[[[[[[[[[[[[if ObtainedMark_ofStudent > TotalMark_ofStudent]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET ObtainedMark_ofStudent = TotalMark_ofStudent
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (TotalMark_ofStudent < ObtainedMark_ofStudent)

-----------------[[[[[[[[[[[[[[[[[PassMark_Student]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                PassMark_Student = ROUND(TotalMark_ofStudent * PassPercentage_Student / 100, 2, 0)
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)

-----------------[[[[[[[[[[[[[[[[[StudentAbsenceStatus]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET              StudentAbsenceStatus = 'Present'
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_Student.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_Student.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND 
                         Exam_Cumulative_Student.ClassID = Exam_Cumulative_Subject.ClassID AND Exam_Cumulative_Student.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND 
                         Exam_Cumulative_Student.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Subject.SubjectAbsenceStatus = N'PRESENT') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

-----------------[[[[[[[[[[[[[[[[[PassStatus_InSubject]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                PassStatus_InSubject ='F'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID INNER JOIN
                         Exam_Cumulative_Student ON Exam_Cumulative_Subject.StudentID = Exam_Cumulative_Student.StudentID AND Exam_Cumulative_Subject.StudentClassID = Exam_Cumulative_Student.StudentClassID AND 
                         Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Student.Cumulative_SettingID
WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Fail_Enable_Optional_Subject = 0) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND 
                         (Exam_Cumulative_Subject.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

UPDATE Exam_Cumulative_Student
SET                PassStatus_InSubject = 'F'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID INNER JOIN
                         Exam_Cumulative_Student ON Exam_Cumulative_Subject.StudentID = Exam_Cumulative_Student.StudentID AND Exam_Cumulative_Subject.StudentClassID = Exam_Cumulative_Student.StudentClassID AND 
                         Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Student.Cumulative_SettingID
WHERE        (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.IS_Fail_Enable_Optional_Subject = 1) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)

-----------------[[[[[[[[[[[[[[[[[Student_Point]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Cumulative_Student
SET                Student_Point = CASE WHEN Max_P.Max_Point < ROUND(Exam_Cumulative_Student.TotalPoint / Exam_Cumulative_Student.TotalSubject, 2, 0) 
                         THEN Max_P.Max_Point ELSE ROUND(Exam_Cumulative_Student.TotalPoint / Exam_Cumulative_Student.TotalSubject, 2, 0) END
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
WHERE        (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Student.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Student.EducationYearID = Max_P.EducationYearID
WHERE        (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)



---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----Student_Grades,Student_Point-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET           Student_Point =0
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Student.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Student.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Student.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Student.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Cumulative_Student.Student_Point <> 0) AND 
                         (Exam_Cumulative_Student.PassStatus_InSubject = N'F')

-----------------[[[[[[[[[[[[[[[[[Student_Grade,Student_Comments]]]]]]]]]]]]]]]]-----------------------
declare @IS_Grade_BasePoint bit

 SELECT @IS_Grade_BasePoint = IS_Grade_BasePoint FROM  Exam_Cumulative_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)

 IF(@IS_Grade_BasePoint = 1)
 BEGIN
	UPDATE Exam_Cumulative_Student 
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC)
	From Exam_Cumulative_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.CumulativeNameID = @CumulativeNameID)
END
 ELSE
 BEGIN
 	UPDATE Exam_Cumulative_Student 
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID WHERE (Exam_Cumulative_Setting.SchoolID = R.SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = R.EducationYearID) AND (Exam_Cumulative_Setting.ClassID = R.ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = R.CumulativeNameID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC)
	From Exam_Cumulative_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.CumulativeNameID = @CumulativeNameID)
 END

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
 UPDATE       Exam_Cumulative_Student
SET                NotGolden =0
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Cumulative_StudentID NOT IN

(SELECT        Exam_Cumulative_Student.Cumulative_StudentID
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT        Exam_Cumulative_Subject.Cumulative_SettingID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID
                               FROM            Exam_Cumulative_Subject INNER JOIN
                                                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
                                                             WHERE (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
                                                             GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Subject.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Max_P.EducationYearID AND 
                                                         Exam_Cumulative_Subject.SubjectPoint <> Max_P.Max_Point
                               WHERE        (Exam_Cumulative_Subject.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.IS_Add_InExam = 1)
                               GROUP BY Exam_Cumulative_Subject.Cumulative_SettingID, Exam_Cumulative_Subject.StudentID, Exam_Cumulative_Subject.StudentClassID) AS TT ON 
                         Exam_Cumulative_Student.StudentID = TT.StudentID AND Exam_Cumulative_Student.StudentClassID = TT.StudentClassID AND Exam_Cumulative_Student.Cumulative_SettingID = TT.Cumulative_SettingID))

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET                NotGolden = 1
FROM            Exam_Cumulative_Student INNER JOIN
                             (SELECT Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Cumulative_Setting ON Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
                                                             WHERE (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)
                                                             GROUP BY Exam_Cumulative_Setting.SchoolID, Exam_Cumulative_Setting.EducationYearID) AS Max_P ON Exam_Cumulative_Student.SchoolID = Max_P.SchoolID AND Exam_Cumulative_Student.EducationYearID = Max_P.EducationYearID AND 
                         Exam_Cumulative_Student.Student_Point <> Max_P.Max_Point
WHERE        (Exam_Cumulative_Student.NotGolden = 0) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
                         (Exam_Cumulative_Student.ClassID = @ClassID) AND (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)

---[[[[[[[[[[[[[[[[[[[[[[-----Update No optional--NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Student
SET                NotGolden =1
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID) AND (Cumulative_StudentID  NOT IN (SELECT Exam_Cumulative_Student.Cumulative_StudentID
FROM            Exam_Cumulative_Student INNER JOIN
                         Exam_Cumulative_Subject AS Sub_T ON Exam_Cumulative_Student.StudentID = Sub_T.StudentID AND Exam_Cumulative_Student.StudentClassID = Sub_T.StudentClassID AND 
                         Exam_Cumulative_Student.Cumulative_SettingID = Sub_T.Cumulative_SettingID
WHERE        (Sub_T.SubjectType = N'Optional') AND (Sub_T.IS_Add_InExam = 1) AND (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Student.ClassID = @ClassID) AND (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)
GROUP BY Exam_Cumulative_Student.Cumulative_StudentID))
END
GO
PRINT N'Creating Procedure [dbo].[SP_Cumulative_Exam_Subject]...';


GO

--9.
--CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Subject]
CREATE PROCEDURE [dbo].[SP_Cumulative_Exam_Subject]

-- Where condition parameters
	@SchoolID int,
	@RegistrationID int,
    @EducationYearID int,
	@ClassID int,
	@CumulativeNameID int,
	@Cumulative_SettingID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
---[[[[[[[[[[[[[[[[[[[[[[-----------------DELETE----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
DELETE FROM Exam_Cumulative_Subject WHERE (CumulativeNameID = @CumulativeNameID) and  (SchoolID = @SchoolID) and  (EducationYearID =@EducationYearID) and (ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[-----------------INSERT----------------]]]]]]]]]]]]]]]]]]]]]]]]]]---
--(SchoolID, RegistrationID, EducationYearID,ClassID, CumulativeNameID,StudentID, StudentClassID,  SubjectID, TotalMark_ofSubject, ObtainedMark_ofSubject)-------------

INSERT INTO Exam_Cumulative_Subject
                         (Cumulative_SettingID,SchoolID, RegistrationID, EducationYearID,ClassID, CumulativeNameID,StudentID, StudentClassID,  SubjectID, TotalMark_ofSubject, ObtainedMark_ofSubject)
SELECT      @Cumulative_SettingID,@SchoolID ,@RegistrationID,@EducationYearID,@ClassID ,@CumulativeNameID ,
                         Exam_Result_of_Subject.StudentID, Exam_Result_of_Subject.StudentClassID, Exam_Result_of_Subject.SubjectID, Exam_Cumulative_FullMarks.FullMarks, 
                         ROUND(SUM(Exam_Result_of_Subject.ObtainedMark_ofSubject * (Exam_Cumulative_ExamList.ExamAdd_Percentage / 100)) * Exam_Cumulative_FullMarks.FullMarks / SUM(Exam_Result_of_Subject.TotalMark_ofSubject * (Exam_Cumulative_ExamList.ExamAdd_Percentage / 100)), 2, 0) AS Obtained_Mark
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Cumulative_ExamList ON Exam_Result_of_Subject.ExamID = Exam_Cumulative_ExamList.ExamID AND Exam_Result_of_Subject.EducationYearID = Exam_Cumulative_ExamList.EducationYearID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Cumulative_ExamList.SchoolID AND Exam_Result_of_Subject.ClassID = Exam_Cumulative_ExamList.ClassID INNER JOIN
                         Exam_Cumulative_Name ON Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID INNER JOIN
                         Exam_Cumulative_FullMarks ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_FullMarks.CumulativeNameID AND 
                         Exam_Result_of_Subject.SubjectID = Exam_Cumulative_FullMarks.SubjectID 
						 INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID  --  
						 INNER JOIN Student ON Exam_Result_of_Subject.StudentID = Student.StudentID   --without Reject Student 

WHERE        (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) AND (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) 
                         AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Cumulative_FullMarks.Cumulative_SettingID = @Cumulative_SettingID)
						
						 AND (Student.Status = N'Active') AND (StudentsClass.Promotion_Demotion_Year IS NULL) --without Reject Student  & without Promotion Demotion Student

GROUP BY Exam_Result_of_Subject.StudentClassID, Exam_Result_of_Subject.SubjectID, Exam_Cumulative_FullMarks.FullMarks, Exam_Result_of_Subject.StudentID




---[[[[[[[[[[[[[[[[[[[[[[--------SubjectAbsenceStatus-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Cumulative_Subject
SET                SubjectAbsenceStatus = 'Present'
FROM            Exam_Cumulative_ExamList INNER JOIN
                         Exam_Result_of_Subject ON Exam_Cumulative_ExamList.ExamID = Exam_Result_of_Subject.ExamID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Cumulative_ExamList.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Cumulative_ExamList.SchoolID = Exam_Result_of_Subject.SchoolID INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND
                          Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Subject.CumulativeNameID AND Exam_Cumulative_ExamList.ClassID = Exam_Cumulative_Subject.ClassID AND 
                         Exam_Result_of_Subject.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND Exam_Result_of_Subject.SubjectID = Exam_Cumulative_Subject.SubjectID AND 
                         Exam_Result_of_Subject.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) 
                         AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Result_of_Subject.SubjectAbsenceStatus = N'PRESENT')


---[[[[[[[[[[[[[[[[[[[[[[-------Grade Point --OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional---------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                SubjectGrades =Exam_Grading_System.Grades, SubjectPoint = Exam_Grading_System.Point ,OMark_ofSub_ConsiderOptional=ObtainedMark_ofSubject, SubjectPoint_ConsiderOptional = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Grading_System.MinPercentage <= Exam_Cumulative_Subject.ObtainedPercentage_ofSubject AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Cumulative_Subject.ObtainedPercentage_ofSubject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID AND Exam_Grading_System.GradeNameID = Exam_Cumulative_Setting.GradeNameID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND 
                         (Exam_Cumulative_Subject.ClassID = @ClassID)

---[[[[[[[[[[[[[[[[[[[[[[--SubjectType--OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional --Update --------]]]]]]]]]]]]]]]]]]]]]]]]]]---

-----Update to Compulsory 
UPDATE       Exam_Cumulative_Subject
SET               SubjectType = 'Compulsory'
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID AND Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND 
                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID INNER JOIN
                         StudentRecord ON Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Cumulative_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Cumulative_Subject.SubjectType = N'Optional')

----Update to Optional 
UPDATE       Exam_Cumulative_Subject
SET   SubjectType = 'Optional',               
OMark_ofSub_ConsiderOptional = (CASE WHEN Exam_Cumulative_Subject.ObtainedPercentage_ofSubject < Exam_Cumulative_Setting.Optional_Percentage_Deduction THEN 0 ELSE ROUND(Exam_Cumulative_Subject.ObtainedMark_ofSubject - (Exam_Cumulative_Subject.TotalMark_ofSubject * Exam_Cumulative_Setting.Optional_Percentage_Deduction) / 100, 2, 0) END), 
SubjectPoint_ConsiderOptional = (CASE WHEN Exam_Grading_System.Point > Exam_Cumulative_Subject.SubjectPoint THEN 0 ELSE Exam_Cumulative_Subject.SubjectPoint - Exam_Grading_System.Point END)
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID AND 
                         Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID INNER JOIN
                         Exam_Grading_System ON Exam_Cumulative_Setting.Optional_Percentage_Deduction >= Exam_Grading_System.MinPercentage AND 
                         Exam_Cumulative_Setting.Optional_Percentage_Deduction < Exam_Grading_System.MaxPercentage + 1 AND Exam_Cumulative_Setting.GradeNameID = Exam_Grading_System.GradeNameID INNER JOIN
                         StudentRecord ON Exam_Cumulative_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Cumulative_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Cumulative_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Cumulative_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Optional')







---[[[[[[[[[[[[[[[[[[[[[[--------PassPercentage_Subject---------PassMark_Subject-------PassStatus_Subject-----]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                PassPercentage_Subject = ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1, PassMark_Subject = ROUND(Exam_Cumulative_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) 
                         + 1) / 100, 2, 0), PassStatus_Subject = CASE WHEN ObtainedMark_ofSubject < ROUND(Exam_Cumulative_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1) 
                         / 100, 2, 0)  THEN 'F' ELSE 'P' END
FROM            Exam_Cumulative_Setting INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_Setting.Cumulative_SettingID = Exam_Cumulative_Subject.Cumulative_SettingID INNER JOIN
                         Exam_Grading_System ON Exam_Cumulative_Setting.GradeNameID = Exam_Grading_System.GradeNameID
WHERE        (Exam_Grading_System.Grades = 'F') AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID)


---[[[[[[[[[[[[[[[[[[[[[[-----Exam_Cumulative_ExamList.Exam_EnableFail------PassStatus_Subject--------Update To 'F'------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Cumulative_Subject
SET                PassStatus_Subject = Exam_Result_of_Subject.PassStatus_Subject
FROM            Exam_Cumulative_ExamList INNER JOIN
                         Exam_Result_of_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Cumulative_ExamList.ClassID = Exam_Result_of_Subject.ClassID AND 
                         Exam_Cumulative_ExamList.EducationYearID = Exam_Result_of_Subject.EducationYearID AND Exam_Cumulative_ExamList.ExamID = Exam_Result_of_Subject.ExamID INNER JOIN
                         Exam_Cumulative_Subject ON Exam_Cumulative_ExamList.SchoolID = Exam_Cumulative_Subject.SchoolID AND Exam_Cumulative_ExamList.EducationYearID = Exam_Cumulative_Subject.EducationYearID AND
                          Exam_Cumulative_ExamList.CumulativeNameID = Exam_Cumulative_Subject.CumulativeNameID AND Exam_Cumulative_ExamList.ClassID = Exam_Cumulative_Subject.ClassID AND 
                         Exam_Result_of_Subject.SubjectID = Exam_Cumulative_Subject.SubjectID AND Exam_Result_of_Subject.StudentClassID = Exam_Cumulative_Subject.StudentClassID AND 
                         Exam_Result_of_Subject.StudentID = Exam_Cumulative_Subject.StudentID
WHERE        (Exam_Cumulative_ExamList.SchoolID = @SchoolID) AND (Exam_Cumulative_ExamList.ClassID = @ClassID) AND (Exam_Cumulative_ExamList.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_ExamList.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_ExamList.Exam_EnableFail = 1) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F')



---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----SubjectGrades,SubjectPoint----SubjectPoint_ConsiderOptional ---]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Cumulative_Subject
SET                SubjectGrades = 'F', SubjectPoint = 0, SubjectPoint_ConsiderOptional = 0
FROM            Exam_Cumulative_Subject INNER JOIN
                         Exam_Cumulative_Setting ON Exam_Cumulative_Subject.SchoolID = Exam_Cumulative_Setting.SchoolID AND Exam_Cumulative_Subject.EducationYearID = Exam_Cumulative_Setting.EducationYearID AND 
                         Exam_Cumulative_Subject.ClassID = Exam_Cumulative_Setting.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = Exam_Cumulative_Setting.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.CumulativeNameID = @CumulativeNameID) AND (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Cumulative_Setting.ClassID = @ClassID) AND (Exam_Cumulative_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Cumulative_Subject.PassStatus_Subject = 'F') AND (Exam_Cumulative_Subject.SubjectPoint <> 0)
END
GO
PRINT N'Creating Procedure [dbo].[SP_Cumulative_HighestMark_Position]...';


GO

--11.
--CREATE PROCEDURE [dbo].[SP_Cumulative_HighestMark_Position]
CREATE PROCEDURE [dbo].[SP_Cumulative_HighestMark_Position]
    @SchoolID int,
	@EducationYearID int,
	@ClassID int,
	@CumulativeNameID int,
	@Exam_Position_Format nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
---Position_InExam_Class --------HighestMark_InExam_Class---------------Position_InExam_Subsection

declare @HighestMark_InExam_Class float
--for HighestMark_InExam_Class -----
SELECT @HighestMark_InExam_Class = MAX(ObtainedMark_ofStudent) FROM Exam_Cumulative_Student WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)


if(@Exam_Position_Format = 'Point')
BEGIN
  UPDATE  Exam_Cumulative_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Cumulative_Student INNER JOIN
  (
   SELECT DENSE_RANK() OVER (Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.NotGolden, Exam_Cumulative_Student.Student_Point DESC,Exam_Cumulative_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.NotGolden, Exam_Cumulative_Student.Student_Point DESC,Exam_Cumulative_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Cumulative_Student.Student_Point,Exam_Cumulative_Student.ObtainedMark_ofStudent,Exam_Cumulative_Student.Cumulative_StudentID 
           FROM   Exam_Cumulative_Student INNER JOIN
                 StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                 Student ON Exam_Cumulative_Student.StudentID = Student.StudentID
           WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
		                (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
						(Exam_Cumulative_Student.ClassID = @ClassID) AND 
                        (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND 
						(Student.Status = N'Active')) as a
  ON Exam_Cumulative_Student.Cumulative_StudentID = a.Cumulative_StudentID  
 END
ELSE
 BEGIN
   UPDATE  Exam_Cumulative_Student
   SET       Position_InExam_Class = a.Position_In_Class,
          HighestMark_InExam_Class = @HighestMark_InExam_Class, 
          Position_InExam_Subsection = a.Position_Subsection
   FROM  Exam_Cumulative_Student INNER JOIN
   (
    SELECT DENSE_RANK() OVER (Order by Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.ObtainedMark_ofStudent DESC) AS Position_In_Class, DENSE_RANK() OVER (Partition by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID Order by  Exam_Cumulative_Student.IsFailed, Exam_Cumulative_Student.ObtainedMark_ofStudent DESC, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,Exam_Cumulative_Student.ObtainedMark_ofStudent,Exam_Cumulative_Student.Cumulative_StudentID 
FROM            Exam_Cumulative_Student INNER JOIN
                         StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON Exam_Cumulative_Student.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Student.SchoolID = @SchoolID) AND (Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Student.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
  ON Exam_Cumulative_Student.Cumulative_StudentID = a.Cumulative_StudentID  
END


----------------------------------------------------------------------------------------------------------------------------------------------
-----------HighestMark_InExam_Subsection

UPDATE  Exam_Cumulative_Student
SET       HighestMark_InExam_Subsection = a.HighestMark_InExam_Subsection
FROM  Exam_Cumulative_Student INNER JOIN StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Cumulative_Student.ObtainedMark_ofStudent)as HighestMark_InExam_Subsection ,
StudentsClass.SectionID,StudentsClass.ShiftID,StudentsClass.SubjectGroupID 

FROM Exam_Cumulative_Student INNER JOIN StudentsClass ON Exam_Cumulative_Student.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Student.ClassID = @ClassID) AND 
(Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)
group by StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a

ON StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Cumulative_Student.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Student.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Student.ClassID = @ClassID) AND 
(Exam_Cumulative_Student.CumulativeNameID = @CumulativeNameID)



--------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------

-----------HighestMark_InSubject_Class

UPDATE Exam_Cumulative_Subject
SET HighestMark_InSubject_Class = a.HighestMark_InSubject_Class
FROM Exam_Cumulative_Subject INNER JOIN
 (SELECT MAX(ObtainedMark_ofSubject) AS HighestMark_InSubject_Class, SubjectID, SchoolID, EducationYearID, ClassID, CumulativeNameID
  FROM  Exam_Cumulative_Subject GROUP BY SubjectID, SchoolID, EducationYearID, ClassID, CumulativeNameID) AS a ON Exam_Cumulative_Subject.SubjectID = a.SubjectID AND Exam_Cumulative_Subject.SchoolID = a.SchoolID AND 
  Exam_Cumulative_Subject.EducationYearID = a.EducationYearID AND Exam_Cumulative_Subject.ClassID = a.ClassID AND Exam_Cumulative_Subject.CumulativeNameID = a.CumulativeNameID
  WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
       (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
       (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
       (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID)

-------------------------------------------------------------------------------------------------------------------------------------

--For HighestMark_InSubject_Subsection-------------------------

UPDATE  Exam_Cumulative_Subject
SET       HighestMark_InSubject_Subsection = a.Mark_ofSubject
FROM  Exam_Cumulative_Subject INNER JOIN StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID
INNER JOIN
(SELECT MAX(Exam_Cumulative_Subject.ObtainedMark_ofSubject) as Mark_ofSubject,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM Exam_Cumulative_Subject INNER JOIN StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID
WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Subject.ClassID = @ClassID) AND 
(Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) 
group by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) as a 
ON Exam_Cumulative_Subject.SubjectID = a.SubjectID and StudentsClass.SectionID = a.SectionID and StudentsClass.ShiftID= a.ShiftID and  StudentsClass.SubjectGroupID = a.SubjectGroupID
WHERE (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND 
(Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND 
(Exam_Cumulative_Subject.ClassID = @ClassID) AND 
(Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) 


---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

--For Position_InSubject_Class-------- Position_InSubject_Subsection-------------------------


if(@Exam_Position_Format = 'Point')
BEGIN
	UPDATE  Exam_Cumulative_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Cumulative_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY SubjectPoint DESC, ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Cumulative_Subject.SubjectPoint DESC, Exam_Cumulative_Subject.ObtainedMark_ofSubject DESC,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Cumulative_Subject.SubjectPoint,Exam_Cumulative_Subject.ObtainedMark_ofSubject,Exam_Cumulative_Subject.Cumulative_SubjectID ,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM            Exam_Cumulative_Subject INNER JOIN
                         StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
	ON Exam_Cumulative_Subject.Cumulative_SubjectID = a.Cumulative_SubjectID
 END
ELSE
 BEGIN


	UPDATE  Exam_Cumulative_Subject
	SET Position_InSubject_Class = a.Position_Class,
		Position_InSubject_Subsection = a.Position_Subsection

	from Exam_Cumulative_Subject INNER JOIN
	(SELECT DENSE_RANK() OVER (Partition by SubjectID  ORDER BY ObtainedMark_ofSubject DESC) AS Position_Class, DENSE_RANK() OVER (Partition by Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID ORDER BY Exam_Cumulative_Subject.ObtainedMark_ofSubject DESC,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID) AS Position_Subsection,
	Exam_Cumulative_Subject.ObtainedMark_ofSubject,Exam_Cumulative_Subject.Cumulative_SubjectID ,Exam_Cumulative_Subject.SubjectID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID
FROM            Exam_Cumulative_Subject INNER JOIN
                         StudentsClass ON Exam_Cumulative_Subject.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (Exam_Cumulative_Subject.SchoolID = @SchoolID) AND (Exam_Cumulative_Subject.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Subject.ClassID = @ClassID) AND 
                         (Exam_Cumulative_Subject.CumulativeNameID = @CumulativeNameID) AND (Student.Status = N'Active')) as a
	ON Exam_Cumulative_Subject.Cumulative_SubjectID = a.Cumulative_SubjectID

END
END
GO
PRINT N'Creating Procedure [dbo].[SP_Exam_Attendance]...';


GO

CREATE PROCEDURE [dbo].[SP_Exam_Attendance]
    @SchoolID int,
    @EducationYearID int,
    @ClassID int,
    @ExamID int,
    @RegistrationID int,
    @From_Date date,
    @To_Date date,
    @ScheduleID int = 0
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Attendance_Student
    WHERE (SchoolID = @SchoolID)
      AND (ClassID = @ClassID)
      AND (EducationYearID = @EducationYearID)
      AND (ExamID = @ExamID);

    IF (@From_Date <> '' AND @To_Date <> '')
    BEGIN
        INSERT INTO Attendance_Student
            (SchoolID, RegistrationID, EducationYearID, ExamID, ClassID, StudentID, StudentClassID,
             WorkingDays, TotalPresent, TotalAbsent, TotalLate, TotalLeave, TotalBunk, TotalLateAbs)
        SELECT
            @SchoolID,
            @RegistrationID,
            @EducationYearID,
            @ExamID,
            @ClassID,
            StudentsClass.StudentID,
            Attendance_Record.StudentClassID,
            COUNT(Attendance_Record.StudentClassID) AS WorkingDay,
            ISNULL(T_Pre.Pre, 0) AS Pre,
            ISNULL(T_Abs.Abs, 0) AS Abs,
            ISNULL(T_Late.Late, 0) AS Late,
            ISNULL(T_Leave.Leave, 0) AS Leave,
            ISNULL(T_Bunk.Bunk, 0) AS Bunk,
            ISNULL(T_LateAbs.LateAbs, 0) AS LateAbs
        FROM Attendance_Record
        INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Bunk
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Bunk')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Bunk ON Attendance_Record.StudentClassID = T_Bunk.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Abs
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Abs')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Abs ON Attendance_Record.StudentClassID = T_Abs.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Pre
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Pre')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Pre ON Attendance_Record.StudentClassID = T_Pre.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Late
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Late')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Late ON Attendance_Record.StudentClassID = T_Late.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS Leave
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Leave')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_Leave ON Attendance_Record.StudentClassID = T_Leave.StudentClassID
        LEFT OUTER JOIN (
            SELECT StudentClassID, COUNT(StudentClassID) AS LateAbs
            FROM Attendance_Record
            WHERE (SchoolID = @SchoolID)
              AND (ClassID = @ClassID)
              AND (EducationYearID = @EducationYearID)
              AND (AttendanceDate BETWEEN @From_Date AND @To_Date)
              AND (Attendance = 'Late Abs')
              AND (@ScheduleID = 0 OR ISNULL(Attendance_ScheduleID, 0) = @ScheduleID)
            GROUP BY StudentClassID
        ) AS T_LateAbs ON Attendance_Record.StudentClassID = T_LateAbs.StudentClassID
        WHERE (Attendance_Record.SchoolID = @SchoolID)
          AND (Attendance_Record.ClassID = @ClassID)
          AND (Attendance_Record.EducationYearID = @EducationYearID)
          AND (Attendance_Record.AttendanceDate BETWEEN @From_Date AND @To_Date)
          AND (@ScheduleID = 0 OR ISNULL(Attendance_Record.Attendance_ScheduleID, 0) = @ScheduleID)
        GROUP BY Attendance_Record.StudentClassID, T_Abs.Abs, T_Pre.Pre, T_Leave.Leave, T_Late.Late,
                 StudentsClass.StudentID, T_Bunk.Bunk, T_LateAbs.LateAbs;
    END
END
GO
PRINT N'Creating Procedure [dbo].[SP_Exam_Student]...';


GO
CREATE PROCEDURE [dbo].[SP_Exam_Student]

-- Where condition parameters
	@SchoolID int,
    @EducationYearID int,
	@ClassID int,
	@ExamID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	

---------[[[[[[[[[[[[  Update
--				   TotalSubjest_WithOptional,
--				   TotalSubject,
--                 TotalExamObtainedMark_ofStudent, 
--				   ObtainedMark_ofStudent, 
--				   TotalExamFullMark_ofStudent,
--                 TotalMark_ofStudent,
--				   PassPercentage_Student,
--				   TotalPoint 
--reset----- PassStatus_InSubject ]]]]]]]]]]]]]]]---------

UPDATE       Exam_Result_of_Student
SET         
TotalSubjest_WithOptional = S_T.TotalSubjest_WithOptional,
TotalSubject = S_T.TotalSubject,
TotalExamObtainedMark_ofStudent = ROUND(S_T.TotalExamObtainedMark_ofStudent, 2, 0),
ObtainedMark_ofStudent = ROUND(S_T.ObtainedMark_ofStudent, 2, 0), 
TotalExamFullMark_ofStudent = S_T.TotalExamFullMark_ofStudent,
TotalMark_ofStudent = S_T.TotalMark_ofStudent,
PassPercentage_Student = S_T.PassPercentage_Student,
TotalPoint = S_T.TotalPoint,
PassStatus_InSubject = 'P' 
      
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT        COUNT(SubjectResultID) AS TotalSubjest_WithOptional, COUNT(SubjectResultID) AS TotalSubject, SUM(TotalExamObtainedMark_ofSubject) AS TotalExamObtainedMark_ofStudent, 
                                                         SUM(OMark_ofSub_ConsiderOptional) AS ObtainedMark_ofStudent, SUM(TotalExamFullMark_ofSubject) AS TotalExamFullMark_ofStudent, SUM(TotalMark_ofSubject) AS TotalMark_ofStudent, 
                                                         AVG(PassPercentage_Subject) AS PassPercentage_Student, SUM(SubjectPoint_ConsiderOptional) AS TotalPoint, StudentResultID
                               FROM            Exam_Result_of_Subject
                               WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (IS_Add_InExam = 1)
                               GROUP BY StudentResultID) AS S_T ON Exam_Result_of_Student.StudentResultID = S_T.StudentResultID


---------[[[[[[[[[[[[Update TotalSubject,TotalMark_ofStudent]]]]]]]]]]]]]]]---------
UPDATE       Exam_Result_of_Student
SET                TotalSubject = Sub_T.Total_Sub, TotalMark_ofStudent =Sub_T.TotalMark
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT        Exam_Result_of_Subject.StudentResultID, COUNT(Exam_Result_of_Subject.SubjectID) AS Total_Sub, SUM(Exam_Result_of_Subject.TotalMark_ofSubject) AS TotalMark
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND 
                         Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID
WHERE        (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND 
                         (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Publish_Setting.IS_Add_Optional_Mark_In_FullMarks = 0) AND (Exam_Result_of_Subject.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
GROUP BY Exam_Result_of_Subject.StudentResultID) AS Sub_T ON Exam_Result_of_Student.StudentResultID = Sub_T.StudentResultID




-----------------[[[[[[[[[[[[[[[[[if ObtainedMark_ofStudent > TotalMark_ofStudent]]]]]]]]]]]]]]]]--------------------

UPDATE Exam_Result_of_Student
SET ObtainedMark_ofStudent = TotalMark_ofStudent
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (TotalMark_ofStudent < ObtainedMark_ofStudent)


-----------------[[[[[[[[[[[[[[[[[PassMark_Student------ObtainedPercentage_ofStudent-----PassStatus_Student]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET                PassMark_Student = ROUND(TotalMark_ofStudent * PassPercentage_Student / 100, 2, 0),
                   ObtainedPercentage_ofStudent =  ROUND((ObtainedMark_ofStudent * 100)/  TotalMark_ofStudent, 2, 0),
				   PassStatus_Student =  (case when ROUND((ObtainedMark_ofStudent * 100)/  TotalMark_ofStudent, 2, 0)>= PassPercentage_Student then 'P' else 'F' end)
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[Student_Point]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET                Student_Point = CASE WHEN Max_P.Max_Point < ROUND(Exam_Result_of_Student.TotalPoint / Exam_Result_of_Student.TotalSubject, 2, 0) 
                         THEN Max_P.Max_Point ELSE ROUND(Exam_Result_of_Student.TotalPoint / Exam_Result_of_Student.TotalSubject, 2, 0) END
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                               FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                               WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                               GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Student.SchoolID = Max_P.SchoolID AND Exam_Result_of_Student.EducationYearID = Max_P.EducationYearID
WHERE        (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND 
                         (Exam_Result_of_Student.ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[StudentAbsenceStatus]]]]]]]]]]]]]]]]-----------------------
UPDATE Exam_Result_of_Student
SET              StudentAbsenceStatus = 'Present'
FROM            Exam_Result_of_Student INNER JOIN
                         Exam_Result_of_Subject ON Exam_Result_of_Student.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Result_of_Student.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Result_of_Student.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Result_of_Student.StudentClassID = Exam_Result_of_Subject.StudentClassID AND 
                         Exam_Result_of_Student.StudentID = Exam_Result_of_Subject.StudentID
WHERE        (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND 
                         (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Subject.SubjectAbsenceStatus = N'PRESENT') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
						 

-----------------[[[[[[[[[[[[[[[[[Publish_SettingID]]]]]]]]]]]]]]]]-----------------------
UPDATE  Exam_Result_of_Student
SET   Publish_SettingID = Exam_Publish_Setting.Publish_SettingID,
StudentPublishStatus = 'Pub'
FROM Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Student ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Student.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Student.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Student.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Student.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND  (Exam_Publish_Setting.ExamID = @ExamID)


-----------------[[[[[[[[[[[[[[[[[Up --by Condition---------PassStatus_InSubject]]]]]]]]]]]]]]]]-----------------------
UPDATE  Exam_Result_of_Student
SET     PassStatus_InSubject = 'F'
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Result_of_Student ON Exam_Result_of_Subject.StudentID = Exam_Result_of_Student.StudentID AND Exam_Result_of_Subject.StudentClassID = Exam_Result_of_Student.StudentClassID AND 
                         Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID
WHERE        (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
AND (((Exam_Publish_Setting.IS_Fail_Enable_Optional_Subject = 0) AND (Exam_Result_of_Subject.SubjectType = N'Compulsory')) OR (Exam_Publish_Setting.IS_Fail_Enable_Optional_Subject = 1))




---[[[[[[[[[[[[[[[[[[[[[[-------IS_Enable_Grade_as_it_is_if_Fail---Update----Student_Grades,Student_Point-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Student
SET                Student_Point = 0 
FROM            Exam_Result_of_Student INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Student.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Student.EducationYearID = Exam_Publish_Setting.EducationYearID AND 
                         Exam_Result_of_Student.ClassID = Exam_Publish_Setting.ClassID AND Exam_Result_of_Student.ExamID = Exam_Publish_Setting.ExamID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Publish_Setting.ClassID = @ClassID) AND (Exam_Publish_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0) AND (Exam_Result_of_Student.Student_Point <> 0) AND 
                         (Exam_Result_of_Student.PassStatus_InSubject = N'F')


						 
-----------------[[[[[[[[[[[[[[[[[Student_Grade,Student_Comments]]]]]]]]]]]]]]]]-----------------------

declare @IS_Grade_BasePoint bit

 SELECT @IS_Grade_BasePoint = IS_Grade_BasePoint FROM  Exam_Publish_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)

 IF(@IS_Grade_BasePoint = 1)
 BEGIN
	 UPDATE Exam_Result_of_Student
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC),
	Student_Comments = (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.Point <= R.Student_Point) ORDER BY Exam_Grading_System.Point DESC)
	From Exam_Result_of_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.ExamID = @ExamID)
 END
 ELSE
 BEGIN
 	 UPDATE Exam_Result_of_Student
	set Student_Grade =  (SELECT TOP (1) Exam_Grading_System.Grades FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC),
	Student_Comments =  (SELECT TOP (1) Exam_Grading_System.Comments FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID WHERE (Exam_Grading_Assign.SchoolID = R.SchoolID) AND (Exam_Grading_Assign.EducationYearID = R.EducationYearID) AND (Exam_Grading_Assign.ClassID = R.ClassID) AND (Exam_Grading_Assign.ExamID = R.ExamID) AND (Exam_Grading_System.MinPercentage <= R.ObtainedPercentage_ofStudent) ORDER BY Exam_Grading_System.MinPercentage DESC)
	From Exam_Result_of_Student AS R WHERE (R.EducationYearID = @EducationYearID) AND (R.SchoolID = @SchoolID) AND (R.ClassID = @ClassID) AND (R.ExamID = @ExamID)
 END


---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Student
SET                NotGolden =0
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (StudentResultID NOT IN
                             (SELECT        Exam_Result_of_Subject.StudentResultID
                               FROM            Exam_Result_of_Subject INNER JOIN
                                                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                                                             FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                                                             WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                                                             GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Subject.SchoolID = Max_P.SchoolID AND Exam_Result_of_Subject.EducationYearID = Max_P.EducationYearID AND 
                                                         Exam_Result_of_Subject.SubjectPoint <> Max_P.Max_Point
                               WHERE        (Exam_Result_of_Subject.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                                                         (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.IS_Add_InExam = 1)
                               GROUP BY  Exam_Result_of_Subject.StudentResultID))

---[[[[[[[[[[[[[[[[[[[[[[-------NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Student
SET                NotGolden = 1
FROM            Exam_Result_of_Student INNER JOIN
                             (SELECT Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID, MAX(Exam_Grading_System.Point) AS Max_Point
                               FROM Exam_Grading_System INNER JOIN Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
                               WHERE (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.ExamID = @ExamID)
                               GROUP BY Exam_Grading_Assign.SchoolID, Exam_Grading_Assign.EducationYearID) AS Max_P ON Exam_Result_of_Student.SchoolID = Max_P.SchoolID AND Exam_Result_of_Student.EducationYearID = Max_P.EducationYearID AND 
                         Exam_Result_of_Student.Student_Point <> Max_P.Max_Point
WHERE        (Exam_Result_of_Student.NotGolden = 0) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND 
                         (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID)

---[[[[[[[[[[[[[[[[[[[[[[-----Update No optional--NotGolden-------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Student
SET                NotGolden =1
WHERE        (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (StudentResultID NOT IN (SELECT StudentResultID
FROM            Exam_Result_of_Subject
WHERE        (SubjectType = N'Optional') AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (IS_Add_InExam = 1)
GROUP BY StudentResultID))

END
GO
PRINT N'Creating Procedure [dbo].[SP_Exam_Subject]...';


GO
CREATE PROCEDURE [dbo].[SP_Exam_Subject]

-- Where condition parameters
	@SchoolID int,
    @EducationYearID int,
	@ClassID int,
	@ExamID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
---[[[[[[[[[[[[---- TotalExamFullMark_ofSubject , 
--	          TotalExamObtainedMark_ofSubject, 
--		      ObtainedMark_ofSubject,
--			  ObtainedPercentage_ofSubject ,
--			  TotalMark_ofSubject
--reset----SubjectAbsenceStatus
--reset----PassStatus_InSubExam-------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Result_of_Subject
SET               
TotalExamFullMark_ofSubject =U_T.TotalExamFullMark_ofSubject, 
TotalExamObtainedMark_ofSubject =U_T.TotalExamObtainedMark_ofSubject, 
ObtainedPercentage_ofSubject =U_T.ObtainedPercentage_ofSubject, 
ObtainedMark_ofSubject =U_T.ObtainedMark_ofSubject, 
TotalMark_ofSubject =U_T.Countable_Mark,
SubjectAbsenceStatus = 'Absent',
PassStatus_InSubExam = 'P'

FROM            Exam_Result_of_Subject INNER JOIN
                             (SELECT        Exam_Obtain_Marks.StudentResultID, Exam_Obtain_Marks.SubjectID, SUM(Exam_Obtain_Marks.FullMark) AS TotalExamFullMark_ofSubject, SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0)) 
                                                         AS TotalExamObtainedMark_ofSubject, ROUND(SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0) * Exam_Obtain_Marks.AddPercentage / 100) 
                                                         * Exam_Publish_Sub_Countable_Mark.Countable_Mark / SUM(Exam_Obtain_Marks.FullMark * Exam_Obtain_Marks.AddPercentage / 100) 
                                                         * 100 / Exam_Publish_Sub_Countable_Mark.Countable_Mark, 2, 0) AS ObtainedPercentage_ofSubject, ROUND(SUM(ISNULL(Exam_Obtain_Marks.MarksObtained, 0) 
                                                         * Exam_Obtain_Marks.AddPercentage / 100) * Exam_Publish_Sub_Countable_Mark.Countable_Mark / SUM(Exam_Obtain_Marks.FullMark * Exam_Obtain_Marks.AddPercentage / 100), 2, 0) 
                                                         AS ObtainedMark_ofSubject, Exam_Publish_Sub_Countable_Mark.Countable_Mark
                               FROM            Exam_Obtain_Marks INNER JOIN
                                                         Exam_Publish_Sub_Countable_Mark ON Exam_Obtain_Marks.SchoolID = Exam_Publish_Sub_Countable_Mark.SchoolID AND 
                                                         Exam_Obtain_Marks.EducationYearID = Exam_Publish_Sub_Countable_Mark.EducationYearID AND Exam_Obtain_Marks.SubjectID = Exam_Publish_Sub_Countable_Mark.SubjectID AND 
                                                         Exam_Obtain_Marks.ExamID = Exam_Publish_Sub_Countable_Mark.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Publish_Sub_Countable_Mark.ClassID
                               WHERE        (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND 
                                                         (Exam_Obtain_Marks.ClassID = @ClassID)
                               GROUP BY Exam_Obtain_Marks.StudentResultID, Exam_Obtain_Marks.SubjectID, Exam_Publish_Sub_Countable_Mark.Countable_Mark) AS U_T ON 
                         Exam_Result_of_Subject.StudentResultID = U_T.StudentResultID AND Exam_Result_of_Subject.SubjectID = U_T.SubjectID



---[[[[[[[[[[[[[[[[[[[[[[-----Up By Condition---SubjectAbsenceStatus-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE     Exam_Result_of_Subject
SET        SubjectAbsenceStatus ='Present'
FROM            Exam_Obtain_Marks INNER JOIN
                         Exam_Result_of_Subject ON Exam_Obtain_Marks.StudentResultID = Exam_Result_of_Subject.StudentResultID AND Exam_Obtain_Marks.SubjectID = Exam_Result_of_Subject.SubjectID
WHERE        (Exam_Obtain_Marks.AbsenceStatus = 'Present') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                         (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID)



---[[[[[[[[[[[[[[[[[[[[[[-----Up By Condition--- PassStatus_InSubExam-------------]]]]]]]]]]]]]]]]]]]]]]]]]]---

UPDATE       Exam_Result_of_Subject
SET                PassStatus_InSubExam = 'F'
FROM            Exam_Obtain_Marks INNER JOIN
                     Exam_Result_of_Subject ON Exam_Obtain_Marks.StudentResultID = Exam_Result_of_Subject.StudentResultID AND Exam_Obtain_Marks.SubjectID = Exam_Result_of_Subject.SubjectID
WHERE        (Exam_Obtain_Marks.PassStatus = 'F') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND 
                        (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID)




---[[[[[[[[[[[[[[[[[[[[[[--------PassPercentage_Subject---------PassMark_Subject-------PassStatus_Subject-----]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE       Exam_Result_of_Subject
SET                PassPercentage_Subject = ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1, PassMark_Subject = ROUND(Exam_Result_of_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) 
                         + 1) / 100, 2, 0), PassStatus_Subject = CASE WHEN Exam_Result_of_Subject.ObtainedMark_ofSubject < ROUND(Exam_Result_of_Subject.TotalMark_ofSubject * (ROUND(Exam_Grading_System.MaxPercentage, 2, 0) + 1) 
                         / 100, 2, 0)  THEN 'F' ELSE 'P' END
FROM            Exam_Grading_Assign INNER JOIN
                         Exam_Result_of_Subject ON Exam_Grading_Assign.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Grading_Assign.ExamID = Exam_Result_of_Subject.ExamID AND 
                         Exam_Grading_Assign.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Grading_Assign.EducationYearID = Exam_Result_of_Subject.EducationYearID INNER JOIN
                         Exam_Grading_System ON Exam_Grading_Assign.GradeNameID = Exam_Grading_System.GradeNameID AND Exam_Grading_Assign.SchoolID = Exam_Grading_System.SchoolID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Grading_System.Grades = 'F')


---[[[[[[[[[[[[[[[[[[[[[[------Update--PassStatus_Subject-----if~~~IS_Enable_Fail_if_fail_in_sub_Exam--------]]]]]]]]]]]]]]]]]]]]]]]]]]---
UPDATE       Exam_Result_of_Subject
SET                PassStatus_Subject ='F'
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Subject ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Subject.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_InSubExam = N'F') AND (Exam_Publish_Setting.IS_Enable_Fail_if_fail_in_sub_Exam = 1)  AND 
                         (Exam_Result_of_Subject.PassStatus_Subject <> 'F')



---[[[[[[[[[[[[[[[[[[[[[[-------GradingID, 
--	          SubjectGrades, 
--	          SubjectPoint,
--			  SubjectPoint_ConsiderOptional,---------]]]]]]]]]]]]]]]]]]]]]]]]]]---


UPDATE        Exam_Result_of_Subject
SET             GradingID =Exam_Grading_System.GradingID,  SubjectGrades =Exam_Grading_System.Grades, SubjectPoint = Exam_Grading_System.Point ,OMark_ofSub_ConsiderOptional=ObtainedMark_ofSubject, SubjectPoint_ConsiderOptional = Exam_Grading_System.Point
FROM            Exam_Grading_System INNER JOIN
                         Exam_Result_of_Subject ON Exam_Grading_System.MinPercentage <= Exam_Result_of_Subject.ObtainedPercentage_ofSubject AND 
                         Exam_Grading_System.MaxPercentage + 1 > Exam_Result_of_Subject.ObtainedPercentage_ofSubject INNER JOIN
                         Exam_Grading_Assign ON Exam_Result_of_Subject.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Result_of_Subject.ClassID = Exam_Grading_Assign.ClassID AND Exam_Result_of_Subject.ExamID = Exam_Grading_Assign.ExamID AND 
                         Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
WHERE        (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID)



---[[[[[[[[[[[[[[[[[[[[[[----OMark_ofSub_ConsiderOptional --SubjectPoint_ConsiderOptional --Update --------]]]]]]]]]]]]]]]]]]]]]]]]]]---

---update Optional to comuplsory 
UPDATE       Exam_Result_of_Subject
SET                SubjectType = 'Compulsory'
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID INNER JOIN
                         StudentRecord ON Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND 
                         (Exam_Publish_Setting.ClassID = @ClassID) AND (StudentRecord.SubjectType = N'Compulsory') AND (Exam_Result_of_Subject.SubjectType = N'Optional')

---update Optional 

UPDATE       Exam_Result_of_Subject
SET   SubjectType = 'Optional',               
OMark_ofSub_ConsiderOptional = (CASE WHEN Exam_Result_of_Subject.ObtainedPercentage_ofSubject < Exam_Publish_Setting.Optional_Percentage_Deduction THEN 0 ELSE ROUND(Exam_Result_of_Subject.ObtainedMark_ofSubject - (Exam_Result_of_Subject.TotalMark_ofSubject * Exam_Publish_Setting.Optional_Percentage_Deduction) / 100, 2, 0) END), 
SubjectPoint_ConsiderOptional = (CASE WHEN Exam_Grading_System.Point > Exam_Result_of_Subject.SubjectPoint THEN 0 ELSE Exam_Result_of_Subject.SubjectPoint - Exam_Grading_System.Point END)
FROM            Exam_Result_of_Subject INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Subject.ExamID = Exam_Publish_Setting.ExamID AND Exam_Result_of_Subject.ClassID = Exam_Publish_Setting.ClassID AND 
                         Exam_Result_of_Subject.SchoolID = Exam_Publish_Setting.SchoolID AND Exam_Result_of_Subject.EducationYearID = Exam_Publish_Setting.EducationYearID INNER JOIN
                         Exam_Grading_System ON Exam_Publish_Setting.Optional_Percentage_Deduction >= Exam_Grading_System.MinPercentage AND 
                         Exam_Publish_Setting.Optional_Percentage_Deduction < Exam_Grading_System.MaxPercentage + 1 INNER JOIN
                         StudentRecord ON Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID AND 
                         Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID INNER JOIN
                         Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID AND Exam_Grading_System.SchoolID = Exam_Grading_Assign.SchoolID AND 
                         Exam_Publish_Setting.SchoolID = Exam_Grading_Assign.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Grading_Assign.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Grading_Assign.ClassID AND Exam_Publish_Setting.ExamID = Exam_Grading_Assign.ExamID
WHERE        (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (StudentRecord.SubjectType = N'Optional')


--Update  Exam_Result_of_Subject---  Sub Exam Fail Enable------------

UPDATE Exam_Result_of_Subject SET SubjectGrades = N'F', SubjectPoint = 0 ,SubjectPoint_ConsiderOptional = 0
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Result_of_Subject ON Exam_Publish_Setting.SchoolID = Exam_Result_of_Subject.SchoolID AND Exam_Publish_Setting.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
                         Exam_Publish_Setting.ClassID = Exam_Result_of_Subject.ClassID AND Exam_Publish_Setting.ExamID = Exam_Result_of_Subject.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND 
                         (Exam_Publish_Setting.ExamID = @ExamID) AND (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Exam_Publish_Setting.IS_Enable_Grade_as_it_is_if_Fail = 0)
END
GO
PRINT N'Creating Procedure [dbo].[sp_Generate_Monthly_Invoices]...';


GO

CREATE PROCEDURE sp_Generate_Monthly_Invoices
    @TargetMonth DATE = NULL,
    @IssueDate DATE = NULL,
    @GeneratedCount INT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MonthEnd DATE
    DECLARE @ServiceChargeCategoryID INT
    DECLARE @EndDate DATE
    DECLARE @InvoiceFor NVARCHAR(50)
    DECLARE @MonthStr NVARCHAR(20)
    
    BEGIN TRY
        -- If no month specified, use current month
        IF @TargetMonth IS NULL
            SET @TargetMonth = GETDATE()
        
        -- If no issue date specified, use 1st of the month
        IF @IssueDate IS NULL
            SET @IssueDate = DATEFROMPARTS(YEAR(@TargetMonth), MONTH(@TargetMonth), 1)
        
        -- Get end of month
        SET @MonthEnd = EOMONTH(@TargetMonth)
        SET @EndDate = DATEADD(DAY, 15, @IssueDate) -- 15 days payment deadline
        SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107) -- Format: MMM dd, yyyy
        SET @InvoiceFor = LEFT(DATENAME(MONTH, @MonthEnd), 3) + ' ' + CAST(YEAR(@MonthEnd) AS NVARCHAR(4))
        
        -- Get Service Charge category ID
        SELECT @ServiceChargeCategoryID = InvoiceCategoryID 
        FROM AAP_Invoice_Category 
        WHERE InvoiceCategory = N'Service Charge'
        
        IF @ServiceChargeCategoryID IS NULL
        BEGIN
            SET @ErrorMessage = 'Service Charge category not found in AAP_Invoice_Category'
            SET @GeneratedCount = 0
            RETURN
        END
        
        -- Check if student count exists for this month
        IF NOT EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly 
                       WHERE MONTH(Month) = MONTH(@MonthEnd) 
                       AND YEAR(Month) = YEAR(@MonthEnd))
        BEGIN
            SET @ErrorMessage = 'Student count not found for ' + @MonthStr + '. Please generate student count first.'
            SET @GeneratedCount = 0
            RETURN
        END
        
        -- Generate invoices for all institutions with student count
        INSERT INTO AAP_Invoice 
        (RegistrationID, InvoiceCategoryID, SchoolID, IssuDate, EndDate, Invoice_For, 
         TotalAmount, Discount, MonthName, Invoice_SN, Unit, UnitPrice)
        SELECT 
            1 AS RegistrationID, -- System generated
            @ServiceChargeCategoryID,
            SC.SchoolID,
            @IssueDate,
            @EndDate,
            @InvoiceFor,
            CASE 
                WHEN ISNULL(SI.Fixed, 0) > 0 THEN SI.Fixed
                ELSE (ISNULL(SC.StudentCount, 0) + ISNULL(dbo.fn_GetBillableCommitteeCount(SC.SchoolID), 0)) * ISNULL(SI.Per_Student_Rate, 0)
            END AS TotalAmount,
            ISNULL(SI.Discount, 0) AS Discount,
            @MonthEnd AS MonthName,
            dbo.Invoice_SerialNumber(SC.SchoolID) AS Invoice_SN,
            ISNULL(SC.StudentCount, 0) + ISNULL(dbo.fn_GetBillableCommitteeCount(SC.SchoolID), 0) AS Unit,
            CASE WHEN ISNULL(SI.Fixed, 0) > 0 THEN NULL ELSE SI.Per_Student_Rate END AS UnitPrice
        FROM AAP_Student_Count_Monthly SC
        INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
        WHERE MONTH(SC.Month) = MONTH(@MonthEnd) 
        AND YEAR(SC.Month) = YEAR(@MonthEnd)
        AND ISNULL(SI.IS_ServiceChargeActive, 0) = 1 -- Only active institutions
        AND NOT EXISTS (
            SELECT 1 FROM AAP_Invoice 
            WHERE SchoolID = SC.SchoolID 
            AND InvoiceCategoryID = @ServiceChargeCategoryID
            AND MONTH(MonthName) = MONTH(@MonthEnd) 
            AND YEAR(MonthName) = YEAR(@MonthEnd)
        )
        
        SET @GeneratedCount = @@ROWCOUNT
        SET @ErrorMessage = 'Success: Generated ' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' invoices for ' + @MonthStr
        
    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE()
        SET @GeneratedCount = 0
    END CATCH
END
GO
PRINT N'Creating Procedure [dbo].[sp_Generate_Monthly_Student_Count]...';


GO

CREATE PROCEDURE sp_Generate_Monthly_Student_Count
    @TargetMonth DATE = NULL,
    @GeneratedCount INT OUTPUT,
    @ErrorMessage NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MonthEnd DATE
    DECLARE @ClassCount INT
    DECLARE @MonthStr NVARCHAR(20)
    
    BEGIN TRY
        -- If no month specified, use current month
        IF @TargetMonth IS NULL
            SET @TargetMonth = GETDATE()
        
        -- Get end of month
        SET @MonthEnd = EOMONTH(@TargetMonth)
        SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107) -- Format: MMM dd, yyyy
        
        -- Check if data already exists
        IF EXISTS (SELECT 1 FROM AAP_Student_Count_Monthly 
                   WHERE MONTH(Month) = MONTH(@MonthEnd) 
                   AND YEAR(Month) = YEAR(@MonthEnd))
        BEGIN
            SELECT @GeneratedCount = COUNT(*) 
            FROM AAP_Student_Count_Monthly
            WHERE MONTH(Month) = MONTH(@MonthEnd) 
            AND YEAR(Month) = YEAR(@MonthEnd)
            
            SET @ErrorMessage = 'Student count already exists for ' + @MonthStr + ' (' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' institutions)'
            RETURN
        END
        
        -- Generate class-wise student count (PAYMENT ACTIVE sessions + ACTIVE institutions)
        INSERT INTO AAP_StudentClass_Count_Monthly 
        (SchoolID, ClassID, EducationYearID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
        SELECT 
            SC.SchoolID,
            SC.ClassID,
            SC.EducationYearID,
            @MonthEnd AS Month,
            COUNT(DISTINCT CASE 
                WHEN S.Status = 'Active' 
                THEN S.StudentID 
            END) AS Active_Student,
            0 AS Reject_Countable,
            0 AS Reject_Uncountable
        FROM StudentsClass SC
        INNER JOIN Student S ON SC.StudentID = S.StudentID
        INNER JOIN Education_Year EY ON SC.EducationYearID = EY.EducationYearID
        INNER JOIN SchoolInfo SI ON SC.SchoolID = SI.SchoolID
        WHERE S.Status = 'Active' -- Only active students
        AND EY.IsActive = 1 -- ONLY PAYMENT ACTIVE SESSIONS
        AND SI.IS_ServiceChargeActive = 1 -- ONLY ACTIVE INSTITUTIONS
        GROUP BY SC.SchoolID, SC.ClassID, SC.EducationYearID
        HAVING COUNT(DISTINCT CASE 
            WHEN S.Status = 'Active' 
            THEN S.StudentID 
        END) > 0
        
        SET @ClassCount = @@ROWCOUNT
        
        -- Generate school-wise total student count
        INSERT INTO AAP_Student_Count_Monthly 
        (SchoolID, Month, Active_Student, Reject_Countable, Reject_Uncountable)
        SELECT 
            SchoolID,
            @MonthEnd AS Month,
            SUM(Active_Student) AS Active_Student,
            SUM(Reject_Countable) AS Reject_Countable,
            SUM(Reject_Uncountable) AS Reject_Uncountable
        FROM AAP_StudentClass_Count_Monthly
        WHERE MONTH(Month) = MONTH(@MonthEnd) 
        AND YEAR(Month) = YEAR(@MonthEnd)
        GROUP BY SchoolID
        HAVING SUM(Active_Student) > 0
        
        SET @GeneratedCount = @@ROWCOUNT
        SET @ErrorMessage = 'Success: Generated count for ' + CAST(@GeneratedCount AS NVARCHAR(10)) + ' institutions (' + CAST(@ClassCount AS NVARCHAR(10)) + ' classes)'
        
    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE()
        SET @GeneratedCount = 0
    END CATCH
END
GO
PRINT N'Creating Procedure [dbo].[sp_GetAttendanceDataBatch]...';


GO

CREATE PROCEDURE dbo.sp_GetAttendanceDataBatch
    @StudentResultIDs NVARCHAR(MAX), -- Comma-separated list of StudentResultID
    @ExamID INT,
    @SchoolID INT,
    @EducationYearID INT
AS
BEGIN
    SET NOCOUNT ON
    
    -- ✅ FIX: Use dynamic SQL instead of STRING_SPLIT for SQL Server 2012 compatibility
    DECLARE @SQL NVARCHAR(MAX)
    
    SET @SQL = N'
    SELECT 
        ers.StudentResultID,
        sc.StudentID,
        sc.StudentClassID,
        sc.ClassID,
        ISNULL(ast.WorkingDays, 0) as WorkingDays,
        ISNULL(ast.TotalPresent, 0) as TotalPresent,
        ISNULL(ast.TotalAbsent, 0) as TotalAbsent,
        ISNULL(ast.TotalLeave, 0) as TotalLeave,
        ISNULL(ast.TotalLate, 0) as TotalLate,
        ISNULL(ast.TotalLateAbs, 0) as TotalLateAbs
    FROM Exam_Result_of_Student ers
    INNER JOIN StudentsClass sc ON ers.StudentClassID = sc.StudentClassID
    LEFT JOIN Attendance_Student ast ON sc.StudentID = ast.StudentID
        AND sc.StudentClassID = ast.StudentClassID
        AND ast.ExamID = @ExamID
        AND ast.SchoolID = @SchoolID
        AND ast.EducationYearID = @EducationYearID
    WHERE ers.StudentResultID IN (' + @StudentResultIDs + ')
    AND ers.ExamID = @ExamID
    AND ers.SchoolID = @SchoolID
    AND ers.EducationYearID = @EducationYearID'
    
    EXEC sp_executesql @SQL, 
        N'@ExamID INT, @SchoolID INT, @EducationYearID INT',
        @ExamID, @SchoolID, @EducationYearID
END
GO
PRINT N'Creating Procedure [dbo].[sp_GetSubjectResultsBatch]...';


GO

CREATE PROCEDURE dbo.sp_GetSubjectResultsBatch
    @StudentResultIDs NVARCHAR(MAX), -- Comma-separated list
    @SchoolID INT,
    @EducationYearID INT
AS
BEGIN
    SET NOCOUNT ON
    
    -- ✅ FIX: Use dynamic SQL instead of STRING_SPLIT for SQL Server 2012 compatibility
    DECLARE @SQL NVARCHAR(MAX)
    
    SET @SQL = N'
    SELECT 
        ers.StudentResultID,
        CASE 
            WHEN ISNULL(sfg.SubjectType, '''') = ''Optional'' 
            THEN ISNULL(sub.SubjectName, '''') + '' *''
            ELSE ISNULL(sub.SubjectName, '''') 
        END as SubjectName,
        sub.SubjectID,
        ISNULL(sub.SN, 999) as SubjectSN,
        ISNULL(ers.ObtainedMark_ofSubject, 0) as ObtainedMark_ofSubject,
        ISNULL(ers.TotalMark_ofSubject, 0) as TotalMark_ofSubject,
        ISNULL(ers.SubjectGrades, '''') as SubjectGrades,
        ISNULL(ers.SubjectPoint, 0) as SubjectPoint,
        ISNULL(ers.PassStatus_Subject, ''Pass'') as PassStatus_Subject,
        ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam,
        ISNULL(ers.Position_InSubject_Class, 0) as Position_InSubject_Class,
        ISNULL(ers.Position_InSubject_Subsection, 0) as Position_InSubject_Subsection,
        ISNULL(ers.HighestMark_InSubject_Class, 0) as HighestMark_InSubject_Class,
        ISNULL(ers.HighestMark_InSubject_Subsection, 0) as HighestMark_InSubject_Subsection
    FROM Exam_Result_of_Subject ers
    INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
    INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
    INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
    LEFT JOIN SubjectForGroup sfg ON sub.SubjectID = sfg.SubjectID 
        AND sc.ClassID = sfg.ClassID 
        AND sc.SubjectGroupID = sfg.SubjectGroupID
        AND ers.SchoolID = sfg.SchoolID
    WHERE ers.StudentResultID IN (' + @StudentResultIDs + ')
    AND ISNULL(ers.IS_Add_InExam, 1) = 1
    AND ers.SchoolID = @SchoolID
    AND ers.EducationYearID = @EducationYearID
    ORDER BY ers.StudentResultID, ISNULL(sub.SN, 999), sub.SubjectName'
    
    EXEC sp_executesql @SQL,
        N'@SchoolID INT, @EducationYearID INT',
        @SchoolID, @EducationYearID
END
GO
PRINT N'Creating Procedure [dbo].[sp_InstitutionData_Preview]...';


GO

CREATE PROCEDURE [dbo].[sp_InstitutionData_Preview]
    @SchoolID        INT,
    @Mode            VARCHAR(20),          -- SESSION / FULL / PURGE
    @EducationYearID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;

    SET @Mode = UPPER(LTRIM(RTRIM(ISNULL(@Mode, ''))));

    IF @SchoolID IS NULL OR @SchoolID <= 0
    BEGIN
        RAISERROR(N'Invalid SchoolID.', 16, 1);
        RETURN;
    END

    IF @Mode NOT IN ('SESSION', 'FULL', 'PURGE')
    BEGIN
        RAISERROR(N'Mode must be SESSION, FULL or PURGE.', 16, 1);
        RETURN;
    END

    IF @Mode = 'SESSION' AND (@EducationYearID IS NULL OR @EducationYearID <= 0)
    BEGIN
        RAISERROR(N'EducationYearID required for SESSION.', 16, 1);
        RETURN;
    END

    DECLARE @SchoolName NVARCHAR(500) =
        (SELECT TOP 1 SchoolName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID);

    DECLARE @ActiveUsers INT = 0;
    IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
        SELECT @ActiveUsers = COUNT(*)
        FROM dbo.User_Active_Sessions
        WHERE SchoolID = @SchoolID
          AND LastActivity >= DATEADD(MINUTE, -30, GETDATE());

    DECLARE @Counts TABLE (
        TableName SYSNAME NOT NULL,
        RowCnt    BIGINT NOT NULL
    );

    DECLARE @tbl SYSNAME, @sql NVARCHAR(MAX), @cnt BIGINT;

    DECLARE @Tables TABLE (Ord INT IDENTITY(1,1), TableName SYSNAME);
    INSERT INTO @Tables (TableName) VALUES
    (N'StudentsClass'), (N'Student'), (N'StudentRecord'),
    (N'Attendance_Record'), (N'Attendance_Student'), (N'Attendance_Leave'),
    (N'Income_PayOrder'), (N'Income_PaymentRecord'), (N'Income_MoneyReceipt'),
    (N'Exam_Obtain_Marks'), (N'Exam_Result_of_Student'), (N'Exam_Result_of_Subject'),
    (N'Exam_Name'), (N'Teacher'), (N'Subject'), (N'CreateClass'),
    (N'Account_Log'), (N'AccountIN_Record'), (N'AccountOUT_Record'),
    (N'Expenditure'), (N'Extra_Income'),
    (N'SMS_Send_Record'), (N'SMS_OtherInfo'),
    (N'Employee_Info'), (N'Employee_Payorder'),
    (N'Education_Year'), (N'Registration'), (N'AST'), (N'SMS'), (N'SchoolInfo');

    DECLARE @i INT = 1, @max INT;
    SELECT @max = MAX(Ord) FROM @Tables;

    WHILE @i <= @max
    BEGIN
        SELECT @tbl = TableName FROM @Tables WHERE Ord = @i;

        IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.' + @tbl, 'SchoolID') IS NOT NULL
        BEGIN
            IF @Mode = 'SESSION'
               AND COL_LENGTH(N'dbo.' + @tbl, 'EducationYearID') IS NOT NULL
               AND @tbl NOT IN (N'SchoolInfo', N'Registration', N'AST', N'SMS', N'Teacher', N'Subject', N'CreateClass', N'Student')
            BEGIN
                SET @sql = N'SELECT @c = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@tbl) +
                           N' WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID';
                BEGIN TRY
                    EXEC sp_executesql @sql,
                        N'@SchoolID INT, @EducationYearID INT, @c BIGINT OUTPUT',
                        @SchoolID = @SchoolID, @EducationYearID = @EducationYearID, @c = @cnt OUTPUT;
                    IF @cnt > 0 INSERT INTO @Counts VALUES (@tbl, @cnt);
                END TRY BEGIN CATCH END CATCH;
            END
            ELSE IF @Mode IN ('FULL', 'PURGE')
            BEGIN
                -- FULL keeps SchoolInfo/Registration(Admin)/SMS/one year â€” still show counts of operational tables
                IF @Mode = 'FULL' AND @tbl IN (N'SchoolInfo')
                BEGIN
                    SET @i = @i + 1;
                    CONTINUE;
                END

                SET @sql = N'SELECT @c = COUNT_BIG(*) FROM dbo.' + QUOTENAME(@tbl) +
                           N' WHERE SchoolID = @SchoolID';
                BEGIN TRY
                    EXEC sp_executesql @sql,
                        N'@SchoolID INT, @c BIGINT OUTPUT',
                        @SchoolID = @SchoolID, @c = @cnt OUTPUT;
                    IF @cnt > 0 INSERT INTO @Counts VALUES (@tbl, @cnt);
                END TRY BEGIN CATCH END CATCH;
            END
        END

        SET @i = @i + 1;
    END

    -- Membership users for PURGE
    IF @Mode = 'PURGE'
    BEGIN
        DECLARE @mem INT = 0;
        SELECT @mem = COUNT(*)
        FROM dbo.aspnet_Users AU
        WHERE EXISTS (
            SELECT 1 FROM dbo.Registration R
            WHERE R.SchoolID = @SchoolID AND LOWER(LTRIM(RTRIM(R.UserName))) = LOWER(LTRIM(RTRIM(AU.UserName)))
        )
        OR EXISTS (
            SELECT 1 FROM dbo.SchoolInfo S
            WHERE S.SchoolID = @SchoolID AND LOWER(LTRIM(RTRIM(S.UserName))) = LOWER(LTRIM(RTRIM(AU.UserName)))
        );
        IF @mem > 0 INSERT INTO @Counts VALUES (N'aspnet_Users (login)', @mem);
    END

    SELECT
        N'Preview' AS Status,
        @SchoolID AS SchoolID,
        ISNULL(@SchoolName, N'') AS SchoolName,
        @Mode AS Mode,
        @EducationYearID AS EducationYearID,
        @ActiveUsers AS ActiveUsers,
        (SELECT ISNULL(SUM(RowCnt), 0) FROM @Counts) AS TotalRows;

    SELECT TableName, RowCnt
    FROM @Counts
    ORDER BY RowCnt DESC, TableName;
END
GO
PRINT N'Creating Procedure [dbo].[sp_Monthly_Auto_Process]...';


GO

-- Step 5: Update Master Procedure to include logging
CREATE PROCEDURE sp_Monthly_Auto_Process
    @TargetMonth DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @GeneratedCount INT
    DECLARE @ErrorMessage NVARCHAR(500)
    DECLARE @LogMessage NVARCHAR(MAX)
    DECLARE @MonthEnd DATE
    DECLARE @MonthStr NVARCHAR(20)
    
    -- If no month specified, use current month
    IF @TargetMonth IS NULL
        SET @TargetMonth = GETDATE()
    
    SET @MonthEnd = EOMONTH(@TargetMonth)
    SET @MonthStr = CONVERT(NVARCHAR(20), @MonthEnd, 107)
    SET @LogMessage = 'Monthly Auto Process Started for ' + @MonthStr + CHAR(13) + CHAR(10)
    
    -- Step 1: Generate Student Count
    EXEC sp_Generate_Monthly_Student_Count 
        @TargetMonth = @TargetMonth,
        @GeneratedCount = @GeneratedCount OUTPUT,
        @ErrorMessage = @ErrorMessage OUTPUT
    
    SET @LogMessage = @LogMessage + 'Student Count: ' + @ErrorMessage + CHAR(13) + CHAR(10)
    
    -- Log student count result
    INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
    VALUES (@MonthEnd, @ErrorMessage, 'Student Count')
    
    -- Step 2: Generate Invoices (only if student count was successful or already exists)
    IF @GeneratedCount > 0 OR @ErrorMessage LIKE 'Student count already exists%'
    BEGIN
        WAITFOR DELAY '00:00:02' -- Wait 2 seconds
        
        EXEC sp_Generate_Monthly_Invoices 
            @TargetMonth = @TargetMonth,
            @IssueDate = NULL, -- Will use 1st of the month
            @GeneratedCount = @GeneratedCount OUTPUT,
            @ErrorMessage = @ErrorMessage OUTPUT
        
        SET @LogMessage = @LogMessage + 'Invoice Generation: ' + @ErrorMessage
        
        -- Log invoice generation result
        INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
        VALUES (@MonthEnd, @ErrorMessage, 'Invoice Generation')
    END
    ELSE
    BEGIN
        SET @LogMessage = @LogMessage + 'Invoice Generation: Skipped due to student count error'
        
        INSERT INTO AAP_Auto_Process_Log (ProcessMonth, LogMessage, ProcessType)
        VALUES (@MonthEnd, 'Skipped due to student count error', 'Invoice Generation')
    END
    
    -- Print final result
    PRINT @LogMessage
END
GO
PRINT N'Creating Procedure [dbo].[sp_ResetInstitutionData]...';


GO

CREATE PROCEDURE [dbo].[sp_ResetInstitutionData]
    @SchoolID          INT,
    @Mode              VARCHAR(20),           -- 'FULL' or 'SESSION' or 'PURGE'
    @EducationYearID   INT = NULL,            -- required for SESSION
    @ConfirmSchoolID   INT,                   -- must equal @SchoolID
    @TotalRowsEstimate BIGINT = NULL,         -- from preview (for UI progress)
    @DeletedRows       INT = 0 OUTPUT,
    @Message           NVARCHAR(500) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET ANSI_NULLS ON;
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_PADDING ON;
    SET ANSI_WARNINGS ON;
    SET CONCAT_NULL_YIELDS_NULL ON;
    SET ARITHABORT ON;
    -- Must stay OFF: we intentionally skip FK failures and continue deleting.
    -- With XACT_ABORT ON, one failed DELETE dooms the whole transaction
    -- ("cannot be committed and cannot support operations that write to the log file").
    SET XACT_ABORT OFF;

    SET @DeletedRows = 0;
    SET @Message = NULL;
    SET @Mode = UPPER(LTRIM(RTRIM(ISNULL(@Mode, ''))));

    /* Live UI progress (polled by Reset_Institution_Data_API?action=progress) */
    DECLARE @HasProgress BIT = CASE WHEN OBJECT_ID(N'dbo.Institution_Reset_Progress', N'U') IS NOT NULL THEN 1 ELSE 0 END;

    IF @SchoolID IS NULL OR @SchoolID <= 0
    BEGIN
        SET @Message = N'Invalid SchoolID.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @ConfirmSchoolID <> @SchoolID
    BEGIN
        SET @Message = N'Confirm SchoolID does not match.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID)
    BEGIN
        SET @Message = N'School not found.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @Mode NOT IN ('FULL', 'SESSION', 'PURGE')
    BEGIN
        SET @Message = N'Mode must be FULL, SESSION or PURGE.';
        RAISERROR(@Message, 16, 1);
        RETURN;
    END

    IF @Mode = 'SESSION'
    BEGIN
        IF @EducationYearID IS NULL OR @EducationYearID <= 0
        BEGIN
            SET @Message = N'EducationYearID is required for SESSION mode.';
            RAISERROR(@Message, 16, 1);
            RETURN;
        END
        IF NOT EXISTS (
            SELECT 1 FROM dbo.Education_Year
            WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID
        )
        BEGIN
            SET @Message = N'Session not found for this school.';
            RAISERROR(@Message, 16, 1);
            RETURN;
        END
    END

    /* Start progress row for UI polling */
    IF @HasProgress = 1
    BEGIN
        DELETE FROM dbo.Institution_Reset_Progress WHERE SchoolID = @SchoolID;
        INSERT INTO dbo.Institution_Reset_Progress
            (SchoolID, Mode, EducationYearID, TotalRows, DeletedRows, Status, Message, UpdatedAt)
        VALUES
            (@SchoolID, @Mode, @EducationYearID, ISNULL(@TotalRowsEstimate, 0), 0, N'Running', NULL, SYSUTCDATETIME());
    END

    DECLARE @KeepEducationYearID INT = NULL;
    DECLARE @AdminRegistrationID INT = NULL;
    DECLARE @AdminUserName NVARCHAR(256) = NULL;
    DECLARE @Pass INT;
    DECLARE @sql NVARCHAR(MAX);
    DECLARE @tbl SYSNAME;
    DECLARE @schema SYSNAME;
    DECLARE @rc INT;

    SELECT TOP 1
        @AdminRegistrationID = R.RegistrationID,
        @AdminUserName = R.UserName
    FROM dbo.Registration R
    WHERE R.SchoolID = @SchoolID AND R.Category = N'Admin'
    ORDER BY R.RegistrationID;

    IF @Mode = 'FULL'
    BEGIN
        SELECT TOP 1 @KeepEducationYearID = EducationYearID
        FROM dbo.Education_Year
        WHERE SchoolID = @SchoolID
        ORDER BY
            CASE WHEN Status = N'True' OR Status = '1' THEN 0 ELSE 1 END,
            CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
            EducationYearID DESC;
    END

    DECLARE @TriggersDisabled BIT = 0;
    DECLARE @ActiveUsers INT = 0;

    -- Live users on this school (last 30 minutes)
    IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
    BEGIN
        SELECT @ActiveUsers = COUNT(*)
        FROM dbo.User_Active_Sessions
        WHERE SchoolID = @SchoolID
          AND LastActivity >= DATEADD(MINUTE, -30, GETDATE());
    END

    --------------------------------------------------------
    -- SESSION MODE: short locks, NO trigger disable, NO long TRAN
    -- (Trigger disable / long TRAN was hanging the whole server)
    --------------------------------------------------------
    IF @Mode = 'SESSION'
    BEGIN
        BEGIN TRY
            SET DEADLOCK_PRIORITY LOW;
            SET LOCK_TIMEOUT 15000; -- 15 sec wait then skip/retry path

            DECLARE @SessionDel TABLE (Ord INT IDENTITY(1,1), TableName SYSNAME);
            INSERT INTO @SessionDel (TableName) VALUES
            (N'Account_Log'),
            (N'Exam_Obtain_Marks'),
            (N'Exam_Result_of_Subject'),
            (N'Exam_Result_of_Student'),
            (N'Exam_Publish_Sub_Countable_Mark'),
            (N'Exam_Publish_Setting'),
            (N'Exam_Cumulative_Subject'),
            (N'Exam_Cumulative_Student'),
            (N'Exam_Cumulative_FullMarks'),
            (N'Exam_Cumulative_ExamList'),
            (N'Exam_Cumulative_Setting'),
            (N'Exam_Full_Marks'),
            (N'Exam_Grading_Assign'),
            (N'Exam_SubExam_Name'),
            (N'Exam_Name'),
            (N'WeeklyExam'),
            (N'Attendance_Schedule_AssignStudent'),
            (N'Attendance_Schedule_ChangeRecord'),
            (N'Attendance_Schedule_Day'),
            (N'Attendance_Record'),
            (N'Attendance_Record_Device'),
            (N'Attendance_Student'),
            (N'Attendance_Leave'),
            (N'Attendance_SMS'),
            (N'Attendance_SMS_Failed'),
            (N'Attendance_Monthly_Report'),
            (N'Attendance_Fine'),
            (N'Attendance_Schedule'),
            (N'Income_PaymentRecord'),
            (N'Income_MoneyReceipt'),
            (N'Income_Discount_Record'),
            (N'Income_LateFee_Change_Record'),
            (N'Income_LateFee_Discount_Record'),
            (N'Income_PayOrder'),
            (N'Income_Assign_Role'),
            (N'AccountIN_Record'),
            (N'AccountOUT_Record'),
            (N'Expenditure'),
            (N'Extra_Income'),
            (N'StudentRecord'),
            (N'Student_Fault'),
            (N'StudentsClass'),
            (N'RoutineForClass'),
            (N'SMS_OtherInfo'),
            (N'Employee_Holiday');

            DECLARE @sOrd INT = 1, @sMax INT, @batch INT, @batchRows INT;
            SELECT @sMax = MAX(Ord) FROM @SessionDel;

            WHILE @sOrd <= @sMax
            BEGIN
                SELECT @tbl = TableName FROM @SessionDel WHERE Ord = @sOrd;
                IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @tbl, 'SchoolID') IS NOT NULL
                   AND COL_LENGTH(N'dbo.' + @tbl, 'EducationYearID') IS NOT NULL
                BEGIN
                    -- Batched delete: commit each batch so other users are not blocked long
                    SET @batch = 0;
                    WHILE @batch < 500
                    BEGIN
                        SET @batch = @batch + 1;
                        SET @sql = N'
                            DELETE TOP (2000)
                            FROM dbo.' + QUOTENAME(@tbl) + N' WITH (ROWLOCK, READPAST)
                            WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;';
                        BEGIN TRY
                            EXEC sp_executesql @sql,
                                N'@SchoolID INT, @EducationYearID INT',
                                @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                            SET @batchRows = @@ROWCOUNT;
                            SET @DeletedRows = @DeletedRows + @batchRows;
                            IF @HasProgress = 1 AND (@batchRows > 0 OR (@DeletedRows % 5000) = 0)
                                UPDATE dbo.Institution_Reset_Progress
                                SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                WHERE SchoolID = @SchoolID AND Status = N'Running';
                            IF @batchRows = 0 BREAK;
                        END TRY
                        BEGIN CATCH
                            -- lock timeout / FK: try once more without READPAST, then move on
                            BEGIN TRY
                                SET @sql = N'DELETE TOP (500) FROM dbo.' + QUOTENAME(@tbl) +
                                           N' WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;';
                                EXEC sp_executesql @sql,
                                    N'@SchoolID INT, @EducationYearID INT',
                                    @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                                SET @batchRows = @@ROWCOUNT;
                                SET @DeletedRows = @DeletedRows + @batchRows;
                                IF @HasProgress = 1 AND @batchRows > 0
                                    UPDATE dbo.Institution_Reset_Progress
                                    SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                    WHERE SchoolID = @SchoolID AND Status = N'Running';
                                IF @batchRows = 0 BREAK;
                            END TRY
                            BEGIN CATCH
                                BREAK; -- leave remaining for multi-pass
                            END CATCH
                        END CATCH
                    END
                END
                SET @sOrd = @sOrd + 1;
            END

            -- Light multi-pass for remaining SchoolID+Year tables (still no outer TRAN)
            SET @Pass = 0;
            WHILE @Pass < 10
            BEGIN
                SET @Pass = @Pass + 1;
                DECLARE @didWork BIT = 0;

                DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                SELECT t.TABLE_SCHEMA, t.TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                  AND t.TABLE_SCHEMA = 'dbo'
                  AND t.TABLE_NAME NOT IN (
                        N'SchoolInfo', N'Education_Year', N'Education_Year_User',
                        N'Registration', N'Admin', N'AST', N'SMS', N'Account',
                        N'AAP_Invoice', N'AAP_Invoice_Category', N'AAP_Invoice_Receipt',
                        N'AAP_Invoice_Payment_Record', N'AAP_Invoice_OnlinePayment',
                        N'AAP_Reference', N'AAP_Reference_School', N'AAP_Reference_Commission',
                        N'AAP_Reference_PaymentRecord', N'AAP_Reference_PayOrder', N'AAP_Reference_Target',
                        N'aspnet_Applications', N'aspnet_Membership', N'aspnet_Users',
                        N'aspnet_UsersInRoles', N'aspnet_Roles', N'aspnet_Profile',
                        N'Authority_Info', N'Authority_Link_Category', N'Authority_Link_SubCategory',
                        N'Authority_Link_Pages', N'Authority_Link_Users'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME = 'SchoolID'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME = 'EducationYearID'
                  );

                OPEN cur;
                FETCH NEXT FROM cur INTO @schema, @tbl;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    SET @sql = N'DELETE TOP (2000) FROM ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) +
                               N' WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID';
                    BEGIN TRY
                        EXEC sp_executesql @sql,
                            N'@SchoolID INT, @EducationYearID INT',
                            @SchoolID = @SchoolID, @EducationYearID = @EducationYearID;
                        SET @rc = @@ROWCOUNT;
                        IF @rc > 0
                        BEGIN
                            SET @DeletedRows = @DeletedRows + @rc;
                            SET @didWork = 1;
                        END
                    END TRY
                    BEGIN CATCH
                        -- ignore lock/FK for this table this pass
                    END CATCH

                    FETCH NEXT FROM cur INTO @schema, @tbl;
                END
                CLOSE cur;
                DEALLOCATE cur;

                IF @didWork = 0 BREAK;
            END

            BEGIN TRY
                IF @AdminRegistrationID IS NOT NULL
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID
                      AND EducationYearID = @EducationYearID
                      AND RegistrationID <> @AdminRegistrationID;
                ELSE
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
            END TRY BEGIN CATCH END CATCH;

            -- Clear active sessions for this school/year so hang risk drops
            BEGIN TRY
                IF OBJECT_ID(N'dbo.User_Active_Sessions', N'U') IS NOT NULL
                    DELETE FROM dbo.User_Active_Sessions WHERE SchoolID = @SchoolID;
            END TRY BEGIN CATCH END CATCH;

            SET @Message = N'Session data deleted successfully'
                + CASE WHEN @ActiveUsers > 0
                       THEN N' (note: ' + CAST(@ActiveUsers AS NVARCHAR(10)) + N' live user(s) were online; used short batched deletes).'
                       ELSE N'.' END;

            IF @HasProgress = 1
                UPDATE dbo.Institution_Reset_Progress
                SET DeletedRows = @DeletedRows, Status = N'Done', Message = @Message, UpdatedAt = SYSUTCDATETIME()
                WHERE SchoolID = @SchoolID;

            SELECT
                N'Success' AS Status,
                @SchoolID AS SchoolID,
                @Mode AS Mode,
                @EducationYearID AS EducationYearID,
                @KeepEducationYearID AS KeptEducationYearID,
                @DeletedRows AS DeletedRows,
                @Message AS Message;
        END TRY
        BEGIN CATCH
            SET @Message = ERROR_MESSAGE();
            IF @HasProgress = 1
                UPDATE dbo.Institution_Reset_Progress
                SET DeletedRows = @DeletedRows, Status = N'Error', Message = @Message, UpdatedAt = SYSUTCDATETIME()
                WHERE SchoolID = @SchoolID;
            SELECT
                N'Error' AS Status,
                @SchoolID AS SchoolID,
                @Mode AS Mode,
                @EducationYearID AS EducationYearID,
                ERROR_LINE() AS ErrorLine,
                @Message AS Message;
        END CATCH

        RETURN;
    END

    --------------------------------------------------------
    -- FULL / PURGE only below (NO long transaction — avoids server hang)
    --------------------------------------------------------
    BEGIN TRY
        SET DEADLOCK_PRIORITY LOW;
        SET LOCK_TIMEOUT 15000;

        BEGIN
            ------------------------------------------------
            -- FULL/PURGE: batched deletes (auto-commit each batch)
            ------------------------------------------------

            -- Account_Log first (no trigger)
            BEGIN TRY
                WHILE 1 = 1
                BEGIN
                    DELETE TOP (3000) FROM dbo.Account_Log WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1
                        UPDATE dbo.Institution_Reset_Progress
                        SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                        WHERE SchoolID = @SchoolID AND Status = N'Running';
                END
            END TRY BEGIN CATCH END CATCH;

            -- Brief trigger disable ONLY around account/income ledger deletes
            BEGIN TRY
                DISABLE TRIGGER ALL ON dbo.AccountIN_Record;
                DISABLE TRIGGER ALL ON dbo.AccountOUT_Record;
                DISABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
                DISABLE TRIGGER ALL ON dbo.Income_PayOrder;
                IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Income_MoneyReceipt;
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Expenditure;
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL DISABLE TRIGGER ALL ON dbo.Extra_Income;
                SET @TriggersDisabled = 1;

                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.AccountIN_Record WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.AccountOUT_Record WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_PaymentRecord WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_MoneyReceipt WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Income_PayOrder WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Expenditure WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL
                WHILE 1 = 1 BEGIN
                    DELETE TOP (2000) FROM dbo.Extra_Income WITH (ROWLOCK) WHERE SchoolID = @SchoolID;
                    IF @@ROWCOUNT = 0 BREAK; SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    IF @HasProgress = 1 UPDATE dbo.Institution_Reset_Progress SET DeletedRows=@DeletedRows, UpdatedAt=SYSUTCDATETIME() WHERE SchoolID=@SchoolID AND Status=N'Running';
                END
            END TRY BEGIN CATCH END CATCH;

            -- Re-enable ledger triggers immediately so other schools are not blocked
            BEGIN TRY
                ENABLE TRIGGER ALL ON dbo.AccountIN_Record;
                ENABLE TRIGGER ALL ON dbo.AccountOUT_Record;
                ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord;
                ENABLE TRIGGER ALL ON dbo.Income_PayOrder;
                IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Income_MoneyReceipt;
                IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Expenditure;
                IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Extra_Income;
                SET @TriggersDisabled = 0;
            END TRY BEGIN CATCH END CATCH;

            -- Helper macro style: delete if table/column exists
            DECLARE @Del TABLE (Ord INT IDENTITY(1,1), SqlText NVARCHAR(MAX));

            INSERT INTO @Del (SqlText) VALUES
            -- Attendance children first
            (N'DELETE FROM dbo.Attendance_Schedule_AssignStudent WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule_ChangeRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule_Day WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Record_Device WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Leave WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS_Failed WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Monthly_Report WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Fine WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Schedule WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Device_DataUpdateList WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Device_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_Leave_Type WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Attendance_SMS_Sender WHERE SchoolID=@SchoolID'),

            -- Device
            (N'DELETE FROM dbo.Device_Commands WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Device_Finger_Print_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Device_Institution_Mapping WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Up_RFID WHERE SchoolID=@SchoolID'),

            -- Exam
            (N'DELETE FROM dbo.Exam_Obtain_Marks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Result_of_Subject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Result_of_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Publish_Sub_Countable_Mark WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Publish_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Subject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Student WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_FullMarks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_ExamList WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Setting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Cumulative_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Full_Marks WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grading_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grading_System WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Grade_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_SubExam_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_CellData WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_ClassColumns WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_Rows WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Routine_SavedData WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Exam_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.WeeklyExam WHERE SchoolID=@SchoolID'),

            -- Income / Expense
            (N'DELETE FROM dbo.Income_PaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_MoneyReceipt WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Discount_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_LateFee_Change_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_LateFee_Discount_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_PayOrder WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Assign_Role WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Income_Roles WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expenditure WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expense_SubCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Expense_CategoryName WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Extra_Income WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Extra_IncomeCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Temp_Online_PaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Temp_Online_DonationPaymentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.User_Balance_Submission WHERE SchoolID=@SchoolID'),

            -- Account
            (N'DELETE FROM dbo.Account_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AccountIN_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AccountOUT_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Account WHERE SchoolID=@SchoolID'),

            -- Employee / payroll
            (N'DELETE FROM dbo.Employee_Payorder_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Daily WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Weekly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Work_Basis WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Payorder_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Allowance WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Bonus_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Bonus WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Deduction WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Fine_Records WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Fine WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Report WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Attendance_Schedule_Assign WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Leave WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Holiday WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_Info WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Employee_SubCategory WHERE SchoolID=@SchoolID'),

            -- Committee
            (N'DELETE FROM dbo.CommitteePaymentRecord WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMoneyReceipt WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonation WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMember_Billing WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMember WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeMemberType WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonationCategory WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CommitteeDonationTemplate WHERE SchoolID=@SchoolID'),

            -- Student / notices
            (N'DELETE FROM dbo.StudentNoticeClass WHERE StudentNoticeId IN (SELECT StudentNoticeId FROM dbo.StudentNotice WHERE SchoolId=@SchoolID)'),
            (N'DELETE FROM dbo.StudentNotice WHERE SchoolId=@SchoolID'),
            (N'DELETE FROM dbo.NoticeBoard WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.StudentRecord WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Fault WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Act_Deactivate_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student_Image WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.StudentsClass WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Student WHERE SchoolID=@SchoolID'),

            -- Teacher / staff / subject
            (N'DELETE FROM dbo.TeacherSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.TecherSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Achievements WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Additional WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Career_Objective WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_EducationInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_JobInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Language WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher_Skill WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Teacher WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Staff_Info WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SubjectForGroup WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.TechnoSubject WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Subject WHERE SchoolID=@SchoolID'),

            -- Class structure
            (N'DELETE FROM dbo.[Join] WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateSection WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateShift WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateSubjectGroup WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.CreateClass WHERE SchoolID=@SchoolID'),

            -- Routine
            (N'DELETE FROM dbo.RoutineForClass WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineTemporary WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineTime WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineDay WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.RoutineInfo WHERE SchoolID=@SchoolID'),

            -- SMS history (keep SMS wallet row later)
            (N'DELETE FROM dbo.SMS_OtherInfo WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Send_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Recharge_Record WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Group_Phone_Number WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Group_Name WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SMS_Template WHERE SchoolID=@SchoolID'),

            -- Counts / settings
            (N'DELETE FROM dbo.AAP_StudentClass_Count_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AAP_Student_Count_Monthly WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.AAP_Auto_Process_Log WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SchoolInfo_DueNoticeSettings WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.SikkhaitySetting WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.User_Active_Sessions WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.WordOfTheDay WHERE SchoolID=@SchoolID'),
            (N'DELETE FROM dbo.Link_Users WHERE SchoolID=@SchoolID');

            -- Teacher child rows by join (may not have SchoolID column)
            BEGIN TRY
                DELETE X FROM dbo.Teacher_Achievements X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Additional X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Career_Objective X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_EducationInfo X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_JobInfo X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Language X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE X FROM dbo.Teacher_Skill X INNER JOIN dbo.Teacher T ON X.TeacherID = T.TeacherID WHERE T.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
            END TRY BEGIN CATCH END CATCH;

            DECLARE @Ord INT, @maxOrd INT, @delSql NVARCHAR(MAX), @loopN INT, @loopRows INT;
            SELECT @Ord = MIN(Ord), @maxOrd = MAX(Ord) FROM @Del;
            WHILE @Ord <= @maxOrd
            BEGIN
                SELECT @sql = SqlText FROM @Del WHERE Ord = @Ord;
                -- Convert plain DELETE into batched TOP deletes when possible
                IF @sql LIKE N'DELETE FROM dbo.%WHERE SchoolID=@SchoolID'
                   OR @sql LIKE N'DELETE FROM dbo.%WHERE SchoolId=@SchoolID'
                BEGIN
                    SET @tbl = NULL;
                    SET @tbl = SUBSTRING(@sql, CHARINDEX(N'dbo.', @sql) + 4, 200);
                    SET @tbl = LEFT(@tbl, CHARINDEX(N' ', @tbl + N' ') - 1);
                    SET @tbl = REPLACE(REPLACE(@tbl, N'[', N''), N']', N'');
                    IF OBJECT_ID(N'dbo.' + @tbl, N'U') IS NOT NULL
                    BEGIN
                        SET @loopN = 0;
                        WHILE @loopN < 400
                        BEGIN
                            SET @loopN = @loopN + 1;
                            SET @delSql = N'DELETE TOP (2000) FROM dbo.' + QUOTENAME(@tbl) +
                                          N' WITH (ROWLOCK, READPAST) WHERE SchoolID = @SchoolID';
                            BEGIN TRY
                                EXEC sp_executesql @delSql, N'@SchoolID INT', @SchoolID=@SchoolID;
                                SET @loopRows = @@ROWCOUNT;
                                SET @DeletedRows = @DeletedRows + @loopRows;
                                IF @loopRows = 0 BREAK;
                            END TRY
                            BEGIN CATCH
                                BEGIN TRY
                                    EXEC sp_executesql @sql, N'@SchoolID INT', @SchoolID=@SchoolID;
                                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                                END TRY BEGIN CATCH END CATCH;
                                BREAK;
                            END CATCH
                        END
                    END
                END
                ELSE
                BEGIN
                    BEGIN TRY
                        EXEC sp_executesql @sql, N'@SchoolID INT', @SchoolID=@SchoolID;
                        SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    END TRY
                    BEGIN CATCH
                        -- continue; multi-pass below will retry leftovers
                    END CATCH
                END
                SET @Ord = @Ord + 1;
            END

            -- Multi-pass: any remaining dbo tables with SchoolID except keep-list
            SET @Pass = 0;
            WHILE @Pass < 25
            BEGIN
                SET @Pass = @Pass + 1;
                DECLARE @didWork2 BIT = 0;

                DECLARE cur2 CURSOR LOCAL FAST_FORWARD FOR
                SELECT t.TABLE_SCHEMA, t.TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES t
                WHERE t.TABLE_TYPE = 'BASE TABLE'
                  AND t.TABLE_SCHEMA = 'dbo'
                  AND t.TABLE_NAME NOT IN (
                        N'SchoolInfo', N'Education_Year', N'Education_Year_User',
                        N'Registration', N'Admin', N'AST', N'SMS',
                        N'AAP_Invoice', N'AAP_Invoice_Category', N'AAP_Invoice_Receipt',
                        N'AAP_Invoice_Payment_Record', N'AAP_Invoice_OnlinePayment',
                        N'AAP_Reference', N'AAP_Reference_School', N'AAP_Reference_Commission',
                        N'AAP_Reference_PaymentRecord', N'AAP_Reference_PayOrder', N'AAP_Reference_Target',
                        N'aspnet_Applications', N'aspnet_Membership', N'aspnet_Users',
                        N'aspnet_UsersInRoles', N'aspnet_Roles', N'aspnet_Profile',
                        N'aspnet_Paths', N'aspnet_PersonalizationAllUsers', N'aspnet_PersonalizationPerUser',
                        N'aspnet_SchemaVersions', N'aspnet_WebEvent_Events',
                        N'Authority_Info', N'Authority_Link_Category', N'Authority_Link_SubCategory',
                        N'Authority_Link_Pages', N'Authority_Link_Users',
                        N'Link_Category', N'Link_SubCategory', N'Link_Pages'
                  )
                  AND EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                        WHERE c.TABLE_SCHEMA = t.TABLE_SCHEMA AND c.TABLE_NAME = t.TABLE_NAME
                          AND c.COLUMN_NAME IN ('SchoolID', 'SchoolId')
                  );

                OPEN cur2;
                FETCH NEXT FROM cur2 INTO @schema, @tbl;
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    DECLARE @col SYSNAME =
                        CASE WHEN EXISTS (
                            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_SCHEMA=@schema AND TABLE_NAME=@tbl AND COLUMN_NAME='SchoolID'
                        ) THEN 'SchoolID' ELSE 'SchoolId' END;

                    SET @sql = N'
                        BEGIN TRY
                            DELETE FROM ' + QUOTENAME(@schema) + N'.' + QUOTENAME(@tbl) + N'
                            WHERE ' + QUOTENAME(@col) + N' = @SchoolID;
                            SET @rc = @@ROWCOUNT;
                        END TRY
                        BEGIN CATCH
                            SET @rc = 0;
                        END CATCH';
                    BEGIN TRY
                        EXEC sp_executesql
                            @sql,
                            N'@SchoolID INT, @rc INT OUTPUT',
                            @SchoolID = @SchoolID,
                            @rc = @rc OUTPUT;
                        IF @rc > 0
                        BEGIN
                            SET @DeletedRows = @DeletedRows + @rc;
                            SET @didWork2 = 1;
                            IF @HasProgress = 1
                                UPDATE dbo.Institution_Reset_Progress
                                SET DeletedRows = @DeletedRows, UpdatedAt = SYSUTCDATETIME()
                                WHERE SchoolID = @SchoolID AND Status = N'Running';
                        END
                    END TRY
                    BEGIN CATCH
                    END CATCH

                    FETCH NEXT FROM cur2 INTO @schema, @tbl;
                END
                CLOSE cur2;
                DEALLOCATE cur2;

                IF @didWork2 = 0 BREAK;
            END

            -- Extra education years (keep one)
            IF @KeepEducationYearID IS NOT NULL
            BEGIN
                DELETE FROM dbo.Education_Year_User
                WHERE SchoolID = @SchoolID AND EducationYearID <> @KeepEducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.Education_Year
                WHERE SchoolID = @SchoolID AND EducationYearID <> @KeepEducationYearID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Keep only admin on remaining year
                IF @AdminRegistrationID IS NOT NULL
                BEGIN
                    DELETE FROM dbo.Education_Year_User
                    WHERE SchoolID = @SchoolID
                      AND EducationYearID = @KeepEducationYearID
                      AND RegistrationID <> @AdminRegistrationID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.Education_Year_User
                        WHERE SchoolID = @SchoolID
                          AND EducationYearID = @KeepEducationYearID
                          AND RegistrationID = @AdminRegistrationID
                    )
                    BEGIN
                        INSERT INTO dbo.Education_Year_User (EducationYearID, SchoolID, RegistrationID)
                        VALUES (@KeepEducationYearID, @SchoolID, @AdminRegistrationID);
                    END
                END
            END

            -- Remove non-admin registrations for this school
            IF OBJECT_ID('tempdb..#NonAdminUsers') IS NOT NULL DROP TABLE #NonAdminUsers;
            SELECT RegistrationID, UserName
            INTO #NonAdminUsers
            FROM dbo.Registration
            WHERE SchoolID = @SchoolID
              AND (@AdminRegistrationID IS NULL OR RegistrationID <> @AdminRegistrationID);

            DELETE LU FROM dbo.Link_Users LU
            INNER JOIN #NonAdminUsers N ON LU.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            DELETE EYU FROM dbo.Education_Year_User EYU
            INNER JOIN #NonAdminUsers N ON EYU.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            DELETE A FROM dbo.AST A
            INNER JOIN #NonAdminUsers N ON A.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            -- Membership cleanup for non-admin usernames
            DELETE UIR
            FROM dbo.aspnet_UsersInRoles UIR
            INNER JOIN dbo.aspnet_Users U ON U.UserId = UIR.UserId
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE M
            FROM dbo.aspnet_Membership M
            INNER JOIN dbo.aspnet_Users U ON U.UserId = M.UserId
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE U
            FROM dbo.aspnet_Users U
            INNER JOIN #NonAdminUsers N ON N.UserName = U.UserName;

            DELETE R FROM dbo.Registration R
            INNER JOIN #NonAdminUsers N ON R.RegistrationID = N.RegistrationID;
            SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

            IF @Mode = 'FULL'
            BEGIN
                -- Ensure SMS wallet exists and is zeroed
                IF EXISTS (SELECT 1 FROM dbo.SMS WHERE SchoolID = @SchoolID)
                    UPDATE dbo.SMS SET SMS_Balance = 0 WHERE SchoolID = @SchoolID;
                ELSE
                    INSERT INTO dbo.SMS (SchoolID, SMS_Balance, Masking, Date)
                    VALUES (@SchoolID, 0, N'Sikkhaloy', GETDATE());

                SET @Message = N'Institution reset to new-signup state successfully.';
            END
            ELSE IF @Mode = 'PURGE'
            BEGIN
                ------------------------------------------------
                -- PURGE: remove identity / login / school row
                ------------------------------------------------

                -- Platform invoices for this school
                BEGIN TRY
                    DELETE FROM dbo.AAP_Invoice_Payment_Record WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice_OnlinePayment WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice_Receipt WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Invoice WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- Referrer assignment & commission for this school
                BEGIN TRY
                    DELETE FROM dbo.AAP_Reference_Commission WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_PaymentRecord WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_PayOrder WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                    DELETE FROM dbo.AAP_Reference_School WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- All sessions
                DELETE FROM dbo.Education_Year_User WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                DELETE FROM dbo.Education_Year WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- SMS wallet
                DELETE FROM dbo.SMS WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                ------------------------------------------------
                -- Collect ALL usernames BEFORE deleting identity rows
                -- (Registration / SchoolInfo / AST / known admin)
                ------------------------------------------------
                IF OBJECT_ID('tempdb..#PurgeUserNames') IS NOT NULL DROP TABLE #PurgeUserNames;
                CREATE TABLE #PurgeUserNames (UserName NVARCHAR(256) NOT NULL PRIMARY KEY);

                INSERT INTO #PurgeUserNames (UserName)
                SELECT DISTINCT LTRIM(RTRIM(UserName))
                FROM (
                    SELECT UserName FROM dbo.Registration WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT UserName FROM dbo.AST WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT UserName FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID AND UserName IS NOT NULL AND LTRIM(RTRIM(UserName)) <> N''
                    UNION
                    SELECT @AdminUserName WHERE @AdminUserName IS NOT NULL AND LTRIM(RTRIM(@AdminUserName)) <> N''
                ) X;

                IF OBJECT_ID('tempdb..#AllSchoolUsers') IS NOT NULL DROP TABLE #AllSchoolUsers;
                SELECT RegistrationID, LTRIM(RTRIM(UserName)) AS UserName
                INTO #AllSchoolUsers
                FROM dbo.Registration
                WHERE SchoolID = @SchoolID;

                DELETE LU FROM dbo.Link_Users LU
                INNER JOIN #AllSchoolUsers U ON LU.RegistrationID = U.RegistrationID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.Admin WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE FROM dbo.AST WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Membership cleanup (match UserName OR LoweredUserName)
                IF OBJECT_ID('tempdb..#PurgeUserIds') IS NOT NULL DROP TABLE #PurgeUserIds;
                SELECT DISTINCT AU.UserId
                INTO #PurgeUserIds
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                BEGIN TRY
                    DELETE P FROM dbo.aspnet_Profile P INNER JOIN #PurgeUserIds U ON P.UserId = U.UserId;
                    DELETE P FROM dbo.aspnet_PersonalizationPerUser P INNER JOIN #PurgeUserIds U ON P.UserId = U.UserId;
                END TRY BEGIN CATCH END CATCH;

                DELETE UIR
                FROM dbo.aspnet_UsersInRoles UIR
                INNER JOIN #PurgeUserIds U ON UIR.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE M
                FROM dbo.aspnet_Membership M
                INNER JOIN #PurgeUserIds U ON M.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE AU
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserIds U ON AU.UserId = U.UserId;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                DELETE R FROM dbo.Registration R
                WHERE R.SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                BEGIN TRY
                    DELETE FROM dbo.SchoolInfo_DueNoticeSettings WHERE SchoolID = @SchoolID;
                    SET @DeletedRows = @DeletedRows + @@ROWCOUNT;
                END TRY BEGIN CATCH END CATCH;

                -- Capture SchoolInfo.UserName again just before delete (safety)
                INSERT INTO #PurgeUserNames (UserName)
                SELECT DISTINCT LTRIM(RTRIM(UserName))
                FROM dbo.SchoolInfo
                WHERE SchoolID = @SchoolID
                  AND UserName IS NOT NULL
                  AND LTRIM(RTRIM(UserName)) <> N''
                  AND NOT EXISTS (
                        SELECT 1 FROM #PurgeUserNames P WHERE LOWER(P.UserName) = LOWER(LTRIM(RTRIM(SchoolInfo.UserName)))
                  );

                DELETE FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID;
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                -- Final orphan membership sweep for collected usernames
                DELETE UIR
                FROM dbo.aspnet_UsersInRoles UIR
                INNER JOIN dbo.aspnet_Users AU ON AU.UserId = UIR.UserId
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                DELETE M
                FROM dbo.aspnet_Membership M
                INNER JOIN dbo.aspnet_Users AU ON AU.UserId = M.UserId
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);

                DELETE AU
                FROM dbo.aspnet_Users AU
                INNER JOIN #PurgeUserNames P
                    ON LOWER(LTRIM(RTRIM(AU.UserName))) = LOWER(P.UserName)
                    OR LOWER(LTRIM(RTRIM(AU.LoweredUserName))) = LOWER(P.UserName);
                SET @DeletedRows = @DeletedRows + @@ROWCOUNT;

                IF EXISTS (SELECT 1 FROM dbo.SchoolInfo WHERE SchoolID = @SchoolID)
                BEGIN
                    SET @Message = N'Failed to delete SchoolInfo row. Check remaining FK references.';
                    RAISERROR(@Message, 16, 1);
                END

                SET @Message = N'Institution permanently deleted (including login & SchoolInfo).';
            END
        END

        IF @HasProgress = 1
            UPDATE dbo.Institution_Reset_Progress
            SET DeletedRows = @DeletedRows, Status = N'Done', Message = @Message, UpdatedAt = SYSUTCDATETIME()
            WHERE SchoolID = @SchoolID;

        SELECT
            N'Success' AS Status,
            @SchoolID AS SchoolID,
            @Mode AS Mode,
            @EducationYearID AS EducationYearID,
            @KeepEducationYearID AS KeptEducationYearID,
            @DeletedRows AS DeletedRows,
            @Message AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;

        SET @Message = ERROR_MESSAGE();
        IF @HasProgress = 1
            UPDATE dbo.Institution_Reset_Progress
            SET DeletedRows = @DeletedRows, Status = N'Error', Message = @Message, UpdatedAt = SYSUTCDATETIME()
            WHERE SchoolID = @SchoolID;
        SELECT
            N'Error' AS Status,
            @SchoolID AS SchoolID,
            @Mode AS Mode,
            @EducationYearID AS EducationYearID,
            ERROR_LINE() AS ErrorLine,
            @Message AS Message;
    END CATCH

    -- Safety: re-enable ledger triggers if still disabled
    IF @TriggersDisabled = 1
    BEGIN
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountIN_Record; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.AccountOUT_Record; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PaymentRecord; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY ENABLE TRIGGER ALL ON dbo.Income_PayOrder; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Income_MoneyReceipt', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Income_MoneyReceipt; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Expenditure', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Expenditure; END TRY BEGIN CATCH END CATCH;
        BEGIN TRY IF OBJECT_ID(N'dbo.Extra_Income', N'U') IS NOT NULL ENABLE TRIGGER ALL ON dbo.Extra_Income; END TRY BEGIN CATCH END CATCH;
    END
END
GO
PRINT N'Creating Procedure [dbo].[SP_SP_Exam_Subject_MarkCheck]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[SP_SP_Exam_Subject_MarkCheck]

-- Where condition parameters
@SchoolID NVARCHAR(10),
@ClassID NVARCHAR(10),
@EducationYearID NVARCHAR(10),
@SectionID nvarchar(10),
@SubjectGroupID nvarchar(10),
@ShiftID nvarchar(10),
@SubjectID nvarchar(10),
@ExamID nvarchar(10)


AS
BEGIN
SET NOCOUNT ON;
 DECLARE @PivotColumnHeaders NVARCHAR(MAX)

 DECLARE @PivotTableSQL NVARCHAR(MAX)


 declare @Status nvarchar(50) ='Active'



SELECT @PivotColumnHeaders = COALESCE(@PivotColumnHeaders +',[' + ISNULL(Exam_SubExam_Name.SubExamName,'Marks') + ']','[' + ISNULL(Exam_SubExam_Name.SubExamName,'Marks') + ']')
FROM  Exam_Full_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.SubjectID = @SubjectID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ClassID = @ClassID)

ORDER BY Exam_SubExam_Name.Sub_ExamSN

SET @PivotTableSQL = N'SELECT ID, StudentsName as Name, RollNo as Roll, '+ @PivotColumnHeaders + N'
FROM (SELECT StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, Student.StudentsName, StudentsClass.RollNo, ISNULL(Exam_SubExam_Name.SubExamName,''Marks'')AS SubExamName, 
 Exam_Obtain_Marks.MarksObtained FROM StudentsClass INNER JOIN
                                                    Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN
                                                    Exam_Obtain_Marks ON StudentsClass.StudentClassID = Exam_Obtain_Marks.StudentClassID AND StudentsClass.SchoolID = Exam_Obtain_Marks.SchoolID AND 
                                                    StudentsClass.StudentID = Exam_Obtain_Marks.StudentID AND StudentsClass.EducationYearID = Exam_Obtain_Marks.EducationYearID LEFT OUTER JOIN
                                                    Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
                          WHERE  StudentsClass.ClassID = '+ @ClassID + ' AND 
						         StudentsClass.SectionID LIKE '''+ @SectionID + ''' AND 
								 StudentsClass.SubjectGroupID LIKE '''+ @SubjectGroupID + ''' AND 
                                 StudentsClass.EducationYearID = '+ @EducationYearID + ' AND 
								 StudentsClass.ShiftID LIKE '''+ @ShiftID + ''' AND 
								 StudentsClass.SchoolID = '+ @SchoolID + ' AND 
								 Student.Status = ''Active'' AND 
                                 Exam_Obtain_Marks.SubjectID = '+ @SubjectID + ' AND
								 Exam_Obtain_Marks.ExamID = '+ @ExamID + '
								 ) AS PivotData PIVOT (MAX(MarksObtained) FOR [SubExamName] IN ( ' + @PivotColumnHeaders + ' )) AS PivotTable ORDER BY CASE WHEN ISNUMERIC(RollNo) = 1 THEN CAST(REPLACE(REPLACE(RollNo , ''$'' , '''') , '','' , '''') AS INT) ELSE 0 END'

EXECUTE(@PivotTableSQL)
END
GO
PRINT N'Creating Procedure [dbo].[spx_Pager]...';


GO

CREATE PROCEDURE [dbo].[spx_Pager]
	@PageNo int = 1,
	@ItemsPerPage int = 2,
	@TotalRows int out
AS
BEGIN
  SET NOCOUNT ON
  DECLARE
    @StartIdx int,
    @SQL nvarchar(max),  
    @SQL_Conditions nvarchar(max),  
    @EndIdx int
	
	IF @PageNo < 1 SET @PageNo = 1
	IF @ItemsPerPage < 1 SET @ItemsPerPage = 10

	SET @StartIdx = (@PageNo -1) * @ItemsPerPage + 1
	SET @EndIdx = (@StartIdx + @ItemsPerPage) - 1
	SET @SQL = 'SELECT FilePath
                FROM (
                SELECT  ROW_NUMBER() OVER(ORDER BY ID) AS Row, * 
                      FROM  tblFiles ) AS tbl WHERE  Row >= ' 
						+ CONVERT(varchar(9), @StartIdx) + ' AND
                       Row <=  ' + CONVERT(varchar(9), @EndIdx)
	EXEC sp_executesql @SQL

	SET @SQL = 'SELECT @TotalRows=COUNT(*) FROM tblFiles' 
	EXEC sp_executesql 
        @query = @SQL, 
        @params = N'@TotalRows INT OUTPUT', 
        @TotalRows = @TotalRows OUTPUT 
END
GO
PRINT N'Creating Procedure [dbo].[Student_Monthly_AttendanceFine]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Student_Monthly_AttendanceFine]
	 @SchoolID int,
	 @RegistrationID int,
	 @EducationYearID int,
	 @ClassID int,


	 @Get_date date,
	 @MonthName nvarchar(50)

AS
BEGIN
	SET NOCOUNT ON;  
	 DECLARE  @StartDate date = getdate();
	 DECLARE  @EndDate date = DATEADD(month, 1, GETDATE());


	  DECLARE  @Role nvarchar(50) = N'Monthly Attendance Fine'
	  DECLARE  @RoleID int 
	   
	  IF NOT EXISTS(SELECT RoleID FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role))
      BEGIN
      INSERT INTO Income_Roles(SchoolID, RegistrationID, Role, NumberOfPay)VALUES(@SchoolID, @RegistrationID, @Role, 1)
      END
	   
	   SELECT @RoleID = RoleID FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role)
   



	  DECLARE  @StudentClassID int 
	  DECLARE  @StudentID int 
	  DECLARE  @WorkingDays int
	  DECLARE  @TotalPresent int 
      DECLARE  @TotalAbsent int  
	  DECLARE  @TotalLeave int 
	  DECLARE  @TotalBunk int 
      DECLARE  @TotalLateAbs int
	  DECLARE  @TotalLate int
	  DECLARE   @FineAmount float
	  DECLARE   @PayOrderID int

	  DECLARE @From_Date date = DATEADD(mm, DATEDIFF(mm, 0, @Get_date), 0)
      DECLARE @To_Date date   = DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, @Get_date) + 1, 0))


SELECT StudentsClass.StudentID, Attendance_Record.StudentClassID, Attendance_Record.ClassID, COUNT(Attendance_Record.StudentClassID) AS WorkingDay, ISNULL(T_Pre.Pre, 0) AS Pre, ISNULL(T_Abs.Abs, 0) AS Abs,  ISNULL(T_Late.Late, 0) AS Late,ISNULL(T_Leave.Leave, 0) AS Leave, ISNULL(T_Bunk.Bunk, 0) AS Bunk, ISNULL(T_LateAbs.LateAbs, 0) AS LateAbs
 Into #Temp_Table  FROM Attendance_Record INNER JOIN
                         StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Bunk
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Bunk')
                               GROUP BY StudentClassID) AS T_Bunk ON Attendance_Record.StudentClassID = T_Bunk.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Abs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Abs')
                               GROUP BY StudentClassID) AS T_Abs ON Attendance_Record.StudentClassID = T_Abs.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Pre
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Pre')
                               GROUP BY StudentClassID) AS T_Pre ON Attendance_Record.StudentClassID = T_Pre.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Late
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late')
                               GROUP BY StudentClassID) AS T_Late ON Attendance_Record.StudentClassID = T_Late.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS Leave
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Leave')
                               GROUP BY StudentClassID) AS T_Leave ON Attendance_Record.StudentClassID = T_Leave.StudentClassID LEFT OUTER JOIN
                             (SELECT        StudentClassID, COUNT(StudentClassID) AS LateAbs
                               FROM            Attendance_Record 
                               WHERE        (SchoolID = @SchoolID) AND (ClassID = @ClassID  OR @ClassID = 0) AND (EducationYearID = @EducationYearID) AND (AttendanceDate BETWEEN @From_Date AND @To_Date) AND (Attendance = 'Late Abs')
                               GROUP BY StudentClassID) AS T_LateAbs ON Attendance_Record.StudentClassID = T_LateAbs.StudentClassID
WHERE        (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.ClassID = @ClassID  OR @ClassID = 0) AND (Attendance_Record.EducationYearID = @EducationYearID) AND (Attendance_Record.AttendanceDate BETWEEN 
                         @From_Date AND @To_Date)
GROUP BY Attendance_Record.StudentClassID,Attendance_Record.ClassID, T_Abs.Abs, T_Pre.Pre, T_Leave.Leave, T_Late.Late, StudentsClass.StudentID, T_Bunk.Bunk, T_LateAbs.LateAbs




While EXISTS(SELECT * From #Temp_Table)
Begin

  SELECT Top 1 @StudentClassID = StudentClassID,
               @StudentID = StudentID, 
			   @ClassID = ClassID, 
			   @WorkingDays = WorkingDay,
			   @TotalPresent = Pre,
			   @TotalLate =Late,
			   @TotalAbsent = Abs,  
			   @TotalLeave = Leave, 
			   @TotalBunk = Bunk,  
			   @TotalLateAbs= LateAbs
 From #Temp_Table


 IF NOT EXISTS(SELECT PayOrderID FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (StudentClassID = @StudentClassID) AND (ClassID = @ClassID) AND (PayFor = @MonthName) AND (RoleID = @RoleID))
BEGIN
 DECLARE @AbsFineAmount float
 DECLARE @LateFineAmount float
 DECLARE @BunkFineAmount float

SELECT @AbsFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Abs')
SELECT @LateFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Late')
SELECT @BunkFineAmount = ISNULL(FineAmount,0) FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (FineFor ='Bunk')

SET @FineAmount = ((ISNULL(@TotalAbsent,0) +  ISNULL(@TotalLateAbs,0)) * ISNULL(@AbsFineAmount,0)) + (ISNULL(@TotalLate,0) * ISNULL(@LateFineAmount,0)) + (ISNULL(@TotalBunk,0) * ISNULL(@BunkFineAmount,0))

IF(@FineAmount > 0)
BEGIN
INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, Amount, RoleID, PayFor, StartDate, EndDate, EducationYearID) 
VALUES(@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @FineAmount, @RoleID, @MonthName, @StartDate, @EndDate, @EducationYearID)
      
SET @PayOrderID = (SELECT SCOPE_IDENTITY())

INSERT INTO  Attendance_Monthly_Report (SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, [MonthName], MonthStartDate, MonthEndDate, FineAmount, WorkingDays, TotalPresent, TotalAbsent, TotalLateAbs, TotalLate, TotalLeave, TotalBunk, PayOrderID)
VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @MonthName, @From_Date, @To_Date, @FineAmount, @WorkingDays, @TotalPresent, @TotalAbsent, @TotalLateAbs,@TotalLate, @TotalLeave, @TotalBunk, @PayOrderID)
END
END
   Delete #Temp_Table Where StudentClassID = @StudentClassID 
END
 DROP TABLE #Temp_Table

END
GO
PRINT N'Creating Procedure [dbo].[aspnet_Membership_CreateUser]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_Membership_CreateUser
    @ApplicationName                        nvarchar(256),
    @UserName                               nvarchar(256),
    @Password                               nvarchar(128),
    @PasswordSalt                           nvarchar(128),
    @Email                                  nvarchar(256),
    @PasswordQuestion                       nvarchar(256),
    @PasswordAnswer                         nvarchar(128),
    @IsApproved                             bit,
    @CurrentTimeUtc                         datetime,
    @CreateDate                             datetime = NULL,
    @UniqueEmail                            int      = 0,
    @PasswordFormat                         int      = 0,
    @UserId                                 uniqueidentifier OUTPUT
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL

    DECLARE @NewUserId uniqueidentifier
    SELECT @NewUserId = NULL

    DECLARE @IsLockedOut bit
    SET @IsLockedOut = 0

    DECLARE @LastLockoutDate  datetime
    SET @LastLockoutDate = CONVERT( datetime, '17540101', 112 )

    DECLARE @FailedPasswordAttemptCount int
    SET @FailedPasswordAttemptCount = 0

    DECLARE @FailedPasswordAttemptWindowStart  datetime
    SET @FailedPasswordAttemptWindowStart = CONVERT( datetime, '17540101', 112 )

    DECLARE @FailedPasswordAnswerAttemptCount int
    SET @FailedPasswordAnswerAttemptCount = 0

    DECLARE @FailedPasswordAnswerAttemptWindowStart  datetime
    SET @FailedPasswordAnswerAttemptWindowStart = CONVERT( datetime, '17540101', 112 )

    DECLARE @NewUserCreated bit
    DECLARE @ReturnValue   int
    SET @ReturnValue = 0

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    SET @CreateDate = @CurrentTimeUtc

    SELECT  @NewUserId = UserId FROM dbo.aspnet_Users WHERE LOWER(@UserName) = LoweredUserName AND @ApplicationId = ApplicationId
    IF ( @NewUserId IS NULL )
    BEGIN
        SET @NewUserId = @UserId
        EXEC @ReturnValue = dbo.aspnet_Users_CreateUser @ApplicationId, @UserName, 0, @CreateDate, @NewUserId OUTPUT
        SET @NewUserCreated = 1
    END
    ELSE
    BEGIN
        SET @NewUserCreated = 0
        IF( @NewUserId <> @UserId AND @UserId IS NOT NULL )
        BEGIN
            SET @ErrorCode = 6
            GOTO Cleanup
        END
    END

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @ReturnValue = -1 )
    BEGIN
        SET @ErrorCode = 10
        GOTO Cleanup
    END

    IF ( EXISTS ( SELECT UserId
                  FROM   dbo.aspnet_Membership
                  WHERE  @NewUserId = UserId ) )
    BEGIN
        SET @ErrorCode = 6
        GOTO Cleanup
    END

    SET @UserId = @NewUserId

    IF (@UniqueEmail = 1)
    BEGIN
        IF (EXISTS (SELECT *
                    FROM  dbo.aspnet_Membership m WITH ( UPDLOCK, HOLDLOCK )
                    WHERE ApplicationId = @ApplicationId AND LoweredEmail = LOWER(@Email)))
        BEGIN
            SET @ErrorCode = 7
            GOTO Cleanup
        END
    END

    IF (@NewUserCreated = 0)
    BEGIN
        UPDATE dbo.aspnet_Users
        SET    LastActivityDate = @CreateDate
        WHERE  @UserId = UserId
        IF( @@ERROR <> 0 )
        BEGIN
            SET @ErrorCode = -1
            GOTO Cleanup
        END
    END

    INSERT INTO dbo.aspnet_Membership
                ( ApplicationId,
                  UserId,
                  Password,
                  PasswordSalt,
                  Email,
                  LoweredEmail,
                  PasswordQuestion,
                  PasswordAnswer,
                  PasswordFormat,
                  IsApproved,
                  IsLockedOut,
                  CreateDate,
                  LastLoginDate,
                  LastPasswordChangedDate,
                  LastLockoutDate,
                  FailedPasswordAttemptCount,
                  FailedPasswordAttemptWindowStart,
                  FailedPasswordAnswerAttemptCount,
                  FailedPasswordAnswerAttemptWindowStart )
         VALUES ( @ApplicationId,
                  @UserId,
                  @Password,
                  @PasswordSalt,
                  @Email,
                  LOWER(@Email),
                  @PasswordQuestion,
                  @PasswordAnswer,
                  @PasswordFormat,
                  @IsApproved,
                  @IsLockedOut,
                  @CreateDate,
                  @CreateDate,
                  @CreateDate,
                  @LastLockoutDate,
                  @FailedPasswordAttemptCount,
                  @FailedPasswordAttemptWindowStart,
                  @FailedPasswordAnswerAttemptCount,
                  @FailedPasswordAnswerAttemptWindowStart )

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
	    SET @TranStarted = 0
	    COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_PersonalizationPerUser_SetPageSettings]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO
CREATE PROCEDURE dbo.aspnet_PersonalizationPerUser_SetPageSettings (
    @ApplicationName  NVARCHAR(256),
    @UserName         NVARCHAR(256),
    @Path             NVARCHAR(256),
    @PageSettings     IMAGE,
    @CurrentTimeUtc   DATETIME)
AS
BEGIN
    DECLARE @ApplicationId UNIQUEIDENTIFIER
    DECLARE @PathId UNIQUEIDENTIFIER
    DECLARE @UserId UNIQUEIDENTIFIER

    SELECT @ApplicationId = NULL
    SELECT @PathId = NULL
    SELECT @UserId = NULL

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    SELECT @PathId = u.PathId FROM dbo.aspnet_Paths u WHERE u.ApplicationId = @ApplicationId AND u.LoweredPath = LOWER(@Path)
    IF (@PathId IS NULL)
    BEGIN
        EXEC dbo.aspnet_Paths_CreatePath @ApplicationId, @Path, @PathId OUTPUT
    END

    SELECT @UserId = u.UserId FROM dbo.aspnet_Users u WHERE u.ApplicationId = @ApplicationId AND u.LoweredUserName = LOWER(@UserName)
    IF (@UserId IS NULL)
    BEGIN
        EXEC dbo.aspnet_Users_CreateUser @ApplicationId, @UserName, 0, @CurrentTimeUtc, @UserId OUTPUT
    END

    UPDATE   dbo.aspnet_Users WITH (ROWLOCK)
    SET      LastActivityDate = @CurrentTimeUtc
    WHERE    UserId = @UserId
    IF (@@ROWCOUNT = 0) -- Username not found
        RETURN

    IF (EXISTS(SELECT PathId FROM dbo.aspnet_PersonalizationPerUser WHERE UserId = @UserId AND PathId = @PathId))
        UPDATE dbo.aspnet_PersonalizationPerUser SET PageSettings = @PageSettings, LastUpdatedDate = @CurrentTimeUtc WHERE UserId = @UserId AND PathId = @PathId
    ELSE
        INSERT INTO dbo.aspnet_PersonalizationPerUser(UserId, PathId, PageSettings, LastUpdatedDate) VALUES (@UserId, @PathId, @PageSettings, @CurrentTimeUtc)
    RETURN 0
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_DeleteProfiles]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_DeleteProfiles
    @ApplicationName        nvarchar(256),
    @UserNames              nvarchar(4000)
AS
BEGIN
    DECLARE @UserName     nvarchar(256)
    DECLARE @CurrentPos   int
    DECLARE @NextPos      int
    DECLARE @NumDeleted   int
    DECLARE @DeletedUser  int
    DECLARE @TranStarted  bit
    DECLARE @ErrorCode    int

    SET @ErrorCode = 0
    SET @CurrentPos = 1
    SET @NumDeleted = 0
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    WHILE (@CurrentPos <= LEN(@UserNames))
    BEGIN
        SELECT @NextPos = CHARINDEX(N',', @UserNames,  @CurrentPos)
        IF (@NextPos = 0 OR @NextPos IS NULL)
            SELECT @NextPos = LEN(@UserNames) + 1

        SELECT @UserName = SUBSTRING(@UserNames, @CurrentPos, @NextPos - @CurrentPos)
        SELECT @CurrentPos = @NextPos+1

        IF (LEN(@UserName) > 0)
        BEGIN
            SELECT @DeletedUser = 0
            EXEC dbo.aspnet_Users_DeleteUser @ApplicationName, @UserName, 4, @DeletedUser OUTPUT
            IF( @@ERROR <> 0 )
            BEGIN
                SET @ErrorCode = -1
                GOTO Cleanup
            END
            IF (@DeletedUser <> 0)
                SELECT @NumDeleted = @NumDeleted + 1
        END
    END
    SELECT @NumDeleted
    IF (@TranStarted = 1)
    BEGIN
    	SET @TranStarted = 0
    	COMMIT TRANSACTION
    END
    SET @TranStarted = 0

    RETURN 0

Cleanup:
    IF (@TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END
    RETURN @ErrorCode
END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Procedure [dbo].[aspnet_Profile_SetProperties]...';


GO
SET ANSI_NULLS ON;

SET QUOTED_IDENTIFIER OFF;


GO

CREATE PROCEDURE dbo.aspnet_Profile_SetProperties
    @ApplicationName        nvarchar(256),
    @PropertyNames          ntext,
    @PropertyValuesString   ntext,
    @PropertyValuesBinary   image,
    @UserName               nvarchar(256),
    @IsUserAnonymous        bit,
    @CurrentTimeUtc         datetime
AS
BEGIN
    DECLARE @ApplicationId uniqueidentifier
    SELECT  @ApplicationId = NULL

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
       BEGIN TRANSACTION
       SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

    EXEC dbo.aspnet_Applications_CreateApplication @ApplicationName, @ApplicationId OUTPUT

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DECLARE @UserId uniqueidentifier
    DECLARE @LastActivityDate datetime
    SELECT  @UserId = NULL
    SELECT  @LastActivityDate = @CurrentTimeUtc

    SELECT @UserId = UserId
    FROM   dbo.aspnet_Users
    WHERE  ApplicationId = @ApplicationId AND LoweredUserName = LOWER(@UserName)
    IF (@UserId IS NULL)
        EXEC dbo.aspnet_Users_CreateUser @ApplicationId, @UserName, @IsUserAnonymous, @LastActivityDate, @UserId OUTPUT

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    UPDATE dbo.aspnet_Users
    SET    LastActivityDate=@CurrentTimeUtc
    WHERE  UserId = @UserId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF (EXISTS( SELECT *
               FROM   dbo.aspnet_Profile
               WHERE  UserId = @UserId))
        UPDATE dbo.aspnet_Profile
        SET    PropertyNames=@PropertyNames, PropertyValuesString = @PropertyValuesString,
               PropertyValuesBinary = @PropertyValuesBinary, LastUpdatedDate=@CurrentTimeUtc
        WHERE  UserId = @UserId
    ELSE
        INSERT INTO dbo.aspnet_Profile(UserId, PropertyNames, PropertyValuesString, PropertyValuesBinary, LastUpdatedDate)
             VALUES (@UserId, @PropertyNames, @PropertyValuesString, @PropertyValuesBinary, @CurrentTimeUtc)

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
    	SET @TranStarted = 0
    	COMMIT TRANSACTION
    END

    RETURN 0

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END
GO
SET ANSI_NULLS, QUOTED_IDENTIFIER ON;


GO
PRINT N'Creating Extended Property [dbo].[User_Balance_Submission].[MS_Description]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'ইউজার থেকে অথরিটিতে টাকা জমা/প্রদানের রেকর্ড রাখার টেবিল (OTP সহ, CreatedDate এ date ও time সংরক্ষিত)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'User_Balance_Submission';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Emp_Setting].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Emp_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Emp_Setting].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Employee_Attendance_Schedule_Assign"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 283
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Attendance_Schedule"
            Begin Extent = 
               Top = 6
               Left = 321
               Bottom = 136
               Right = 491
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "VW_Emp_Info"
            Begin Extent = 
               Top = 6
               Left = 529
               Bottom = 136
               Right = 739
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Attendance_Schedule_Day"
            Begin Extent = 
               Top = 6
               Left = 777
               Bottom = 136
               Right = 947
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 13', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Emp_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Emp_Setting].[MS_DiagramPane2]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'50
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Emp_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Stu].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Stu';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Stu].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Student"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 304
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "StudentsClass"
            Begin Extent = 
               Top = 6
               Left = 342
               Bottom = 136
               Right = 574
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 4440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Stu';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Stu_Setting].[MS_DiagramPane2]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'   SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Stu_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Stu_Setting].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Stu_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Stu_Setting].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Attendance_Schedule_AssignStudent"
            Begin Extent = 
               Top = 0
               Left = 200
               Bottom = 235
               Right = 478
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Student"
            Begin Extent = 
               Top = 0
               Left = 509
               Bottom = 335
               Right = 775
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Attendance_Schedule"
            Begin Extent = 
               Top = 0
               Left = 0
               Bottom = 201
               Right = 170
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Attendance_Schedule_Day"
            Begin Extent = 
               Top = 6
               Left = 813
               Bottom = 136
               Right = 983
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 13
         Width = 284
         Width = 2775
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 3105
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
      ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Stu_Setting';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_User_Leave].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_User_Leave';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_User_Leave].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_User_Leave';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Users].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Users';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Users].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[31] 4[4] 2[18] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Users';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Users_Image].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Users_Image';


GO
PRINT N'Creating Extended Property [dbo].[VW_Attendance_Users_Image].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Attendance_Users_Image';


GO
PRINT N'Creating Extended Property [dbo].[VW_Expense].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Expense';


GO
PRINT N'Creating Extended Property [dbo].[VW_Expense].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Expense';


GO
PRINT N'Creating Extended Property [dbo].[VW_Payment_Monthly_Stu].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Payment_Monthly_Stu';


GO
PRINT N'Creating Extended Property [dbo].[VW_Payment_Monthly_Stu].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "T_Sch"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 85
               Right = 224
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Re_Uncount"
            Begin Extent = 
               Top = 6
               Left = 262
               Bottom = 102
               Right = 456
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Active"
            Begin Extent = 
               Top = 6
               Left = 494
               Bottom = 102
               Right = 664
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Re_Count"
            Begin Extent = 
               Top = 6
               Left = 702
               Bottom = 102
               Right = 883
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Payment_Monthly_Stu';


GO
PRINT N'Creating Extended Property [dbo].[VW_Payment_Monthly_StudentClass].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Payment_Monthly_StudentClass';


GO
PRINT N'Creating Extended Property [dbo].[VW_Payment_Monthly_StudentClass].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "T_Sch"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 119
               Right = 214
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Re_Uncount"
            Begin Extent = 
               Top = 156
               Left = 260
               Bottom = 286
               Right = 454
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Active"
            Begin Extent = 
               Top = 174
               Left = 522
               Bottom = 304
               Right = 698
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T_Re_Count"
            Begin Extent = 
               Top = 6
               Left = 698
               Bottom = 136
               Right = 879
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 900
         Width = 1485
         Width = 765
         Width = 1500
         Width = 1500
         Width = 1770
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      E', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Payment_Monthly_StudentClass';


GO
PRINT N'Creating Extended Property [dbo].[VW_Payment_Monthly_StudentClass].[MS_DiagramPane2]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'nd
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Payment_Monthly_StudentClass';


GO
PRINT N'Creating Extended Property [dbo].[VW_School_UserID].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_School_UserID';


GO
PRINT N'Creating Extended Property [dbo].[VW_School_UserID].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "AST"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 216
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SchoolInfo"
            Begin Extent = 
               Top = 6
               Left = 254
               Bottom = 136
               Right = 436
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_School_UserID';


GO
PRINT N'Creating Extended Property [dbo].[VW_Student_Details].[MS_DiagramPane2]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'       End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SchoolInfo"
            Begin Extent = 
               Top = 5
               Left = 1446
               Bottom = 135
               Right = 1657
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 30
         Width = 284
         Width = 1500
         Width = 1500
         Width = 2220
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 5760
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Student_Details';


GO
PRINT N'Creating Extended Property [dbo].[VW_Student_Details].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Student_Details';


GO
PRINT N'Creating Extended Property [dbo].[VW_Student_Details].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[17] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Student"
            Begin Extent = 
               Top = 0
               Left = 0
               Bottom = 243
               Right = 266
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "StudentsClass"
            Begin Extent = 
               Top = 0
               Left = 323
               Bottom = 267
               Right = 555
            End
            DisplayFlags = 280
            TopColumn = 2
         End
         Begin Table = "CreateClass"
            Begin Extent = 
               Top = 16
               Left = 752
               Bottom = 146
               Right = 922
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Education_Year"
            Begin Extent = 
               Top = 162
               Left = 606
               Bottom = 292
               Right = 782
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "CreateShift"
            Begin Extent = 
               Top = 79
               Left = 1023
               Bottom = 209
               Right = 1193
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CreateSection"
            Begin Extent = 
               Top = 144
               Left = 1246
               Bottom = 274
               Right = 1416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CreateSubjectGroup"
            Begin Extent = 
               Top = 189
               Left = 1483
               Bottom = 319
               Right = 1655
     ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_Student_Details';


GO
PRINT N'Creating Extended Property [dbo].[VW_TotalStudent_Amount_Report].[MS_DiagramPaneCount]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_TotalStudent_Amount_Report';


GO
PRINT N'Creating Extended Property [dbo].[VW_TotalStudent_Amount_Report].[MS_DiagramPane1]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Student"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 320
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "StudentsClass"
            Begin Extent = 
               Top = 6
               Left = 358
               Bottom = 136
               Right = 606
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SchoolInfo"
            Begin Extent = 
               Top = 6
               Left = 644
               Bottom = 309
               Right = 840
            End
            DisplayFlags = 280
            TopColumn = 8
         End
         Begin Table = "Education_Year"
            Begin Extent = 
               Top = 6
               Left = 878
               Bottom = 192
               Right = 1070
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_TotalStudent_Amount_Report';


GO
PRINT N'Creating Extended Property [dbo].[VW_TotalStudent_Amount_Report].[MS_DiagramPane2]...';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N'
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VW_TotalStudent_Amount_Report';


GO
DECLARE @VarDecimalSupported AS BIT;

SELECT @VarDecimalSupported = 0;

IF ((ServerProperty(N'EngineEdition') = 3)
    AND (((@@microsoftversion / power(2, 24) = 9)
          AND (@@microsoftversion & 0xffff >= 3024))
         OR ((@@microsoftversion / power(2, 24) = 10)
             AND (@@microsoftversion & 0xffff >= 1600))))
    SELECT @VarDecimalSupported = 1;

IF (@VarDecimalSupported > 0)
    BEGIN
        EXECUTE sp_db_vardecimal_storage_format N'EduHybrid', 'ON';
    END


GO
PRINT N'Update complete.';


GO
