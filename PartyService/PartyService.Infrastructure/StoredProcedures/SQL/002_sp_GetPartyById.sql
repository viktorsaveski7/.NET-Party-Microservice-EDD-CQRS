IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetPartyById')
    DROP PROCEDURE sp_GetPartyById;
GO

CREATE PROCEDURE sp_GetPartyById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        BirthdayChildName,
        Title,
        BirthdayChildPhotoUrl,
        CreatedAt
    FROM Parties
    WHERE Id = @Id;
END
GO