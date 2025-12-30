-- Add ProductImage table for multiple product images
USE KLDShop;
GO

-- Create ProductImage table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductImage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ProductImage] (
        [ProductImageId] INT IDENTITY(1,1) NOT NULL,
        [ProductId] INT NOT NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 1,
        [IsMain] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_ProductImage] PRIMARY KEY CLUSTERED ([ProductImageId] ASC),
        CONSTRAINT [FK_ProductImage_Product] FOREIGN KEY ([ProductId]) 
            REFERENCES [dbo].[Product] ([ProductId]) ON DELETE CASCADE
    );

    -- Create index for better performance
    CREATE NONCLUSTERED INDEX [IX_ProductImage_ProductId_DisplayOrder]
        ON [dbo].[ProductImage] ([ProductId], [DisplayOrder]);
        
    PRINT 'ProductImage table created successfully!';
END
ELSE
BEGIN
    PRINT 'ProductImage table already exists.';
END
GO

-- Mark migrations as applied (so EF knows about them)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Insert migration records
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251229164158_AddNewsletterAndEmailCampaign')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251229164158_AddNewsletterAndEmailCampaign', '10.0.0');
    PRINT 'Migration 20251229164158_AddNewsletterAndEmailCampaign marked as applied.';
END

IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251229174511_AddProductImagesTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251229174511_AddProductImagesTable', '10.0.0');
    PRINT 'Migration 20251229174511_AddProductImagesTable marked as applied.';
END
GO

PRINT 'All done! ProductImage table is ready to use.';
GO
