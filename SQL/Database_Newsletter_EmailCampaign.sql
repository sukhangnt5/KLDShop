-- Newsletter and Email Campaign Tables for KLDShop
-- Run this script to add newsletter functionality

USE KLDShop;
GO

-- Create Newsletter table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Newsletter')
BEGIN
    CREATE TABLE Newsletter (
        NewsletterId INT PRIMARY KEY IDENTITY(1,1),
        Email NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        SubscribedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsActive BIT NOT NULL DEFAULT 1,
        Status NVARCHAR(50) NOT NULL DEFAULT 'subscribed',
        MailChimpSubscriberId NVARCHAR(255) NULL,
        UnsubscribedAt DATETIME2 NULL,
        Source NVARCHAR(50) NULL,
        CONSTRAINT UQ_Newsletter_Email UNIQUE (Email)
    );
    PRINT 'Newsletter table created successfully';
END
ELSE
BEGIN
    PRINT 'Newsletter table already exists';
END
GO

-- Create EmailCampaign table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EmailCampaign')
BEGIN
    CREATE TABLE EmailCampaign (
        CampaignId INT PRIMARY KEY IDENTITY(1,1),
        Subject NVARCHAR(255) NOT NULL,
        HtmlContent NVARCHAR(MAX) NOT NULL,
        PreviewText NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        SentAt DATETIME2 NULL,
        ScheduledAt DATETIME2 NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'draft',
        RecipientCount INT NULL,
        OpenCount INT NULL,
        ClickCount INT NULL,
        MailChimpCampaignId NVARCHAR(255) NULL,
        FromName NVARCHAR(255) NULL,
        FromEmail NVARCHAR(255) NULL,
        ReplyTo NVARCHAR(255) NULL,
        CreatedByUserId INT NULL
    );
    PRINT 'EmailCampaign table created successfully';
END
ELSE
BEGIN
    PRINT 'EmailCampaign table already exists';
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Newsletter_IsActive')
BEGIN
    CREATE INDEX IX_Newsletter_IsActive ON Newsletter(IsActive);
    PRINT 'Index IX_Newsletter_IsActive created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Newsletter_Status')
BEGIN
    CREATE INDEX IX_Newsletter_Status ON Newsletter(Status);
    PRINT 'Index IX_Newsletter_Status created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmailCampaign_Status')
BEGIN
    CREATE INDEX IX_EmailCampaign_Status ON EmailCampaign(Status);
    PRINT 'Index IX_EmailCampaign_Status created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_EmailCampaign_CreatedAt')
BEGIN
    CREATE INDEX IX_EmailCampaign_CreatedAt ON EmailCampaign(CreatedAt);
    PRINT 'Index IX_EmailCampaign_CreatedAt created';
END
GO

PRINT 'Newsletter and EmailCampaign setup completed!';
