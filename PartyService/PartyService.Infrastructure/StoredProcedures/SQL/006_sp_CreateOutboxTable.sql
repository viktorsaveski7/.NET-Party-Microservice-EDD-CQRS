-- ============================================
-- Added because of the Outbox Pattern
-- Create Outbox table for reliable event publishing
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND name = 'OutboxMessages')
BEGIN
    CREATE TABLE OutboxMessages (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        EventType NVARCHAR(100) NOT NULL,
        EventData NVARCHAR(MAX) NOT NULL,
        RoutingKey NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL,
        IsProcessed BIT NOT NULL DEFAULT 0,
        RetryCount INT NOT NULL DEFAULT 0,
        LastError NVARCHAR(MAX) NULL
    );

    -- Index for unprocessed messages
    CREATE INDEX IX_OutboxMessages_IsProcessed_CreatedAt 
    ON OutboxMessages(IsProcessed, CreatedAt);
END
GO

PRINT 'OutboxMessages table created successfully';