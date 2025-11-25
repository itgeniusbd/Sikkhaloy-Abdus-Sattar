-- SMS Template Table for ALL SMS Types (Updated Version)
-- Drop existing table if you want to recreate
-- DROP TABLE IF EXISTS SMS_Template

CREATE TABLE SMS_Template (
  TemplateID INT PRIMARY KEY IDENTITY(1,1),
    SchoolID INT NOT NULL,
    TemplateName NVARCHAR(100) NOT NULL,
    TemplateCategory NVARCHAR(50) NOT NULL, -- 'ExamResult', 'Payment', 'Attendance', 'Notice', 'General'
    TemplateType NVARCHAR(50) NOT NULL, -- For ExamResult: 'Passed'/'Failed', For Payment: 'Payment', etc.
 MessageTemplate NVARCHAR(MAX) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SMS_Template_School FOREIGN KEY (SchoolID) REFERENCES SchoolInfo(SchoolID)
)

-- If table already exists, add TemplateCategory column
-- ALTER TABLE SMS_Template ADD TemplateCategory NVARCHAR(50) DEFAULT 'ExamResult' NOT NULL

-- Update existing records to have ExamResult category
-- UPDATE SMS_Template SET TemplateCategory = 'ExamResult' WHERE TemplateCategory IS NULL OR TemplateCategory = ''

-- Available Placeholders by Category:
/*
ExamResult Templates:
- {StudentName}, {ID}, {ExamName}, {TotalMarks}, {Grade}, {Point}, {ClassPosition}, {SectionPosition}, {SchoolName}

Payment Templates:
- {StudentName}, {ID}, {Amount}, {ReceiptNo}, {PaymentDetails}, {SchoolName}

Attendance Templates:
- {StudentName}, {ID}, {Date}, {Status}, {SchoolName}

Notice/General Templates:
- {StudentName}, {ID}, {Message}, {SchoolName}
*/
