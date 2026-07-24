IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAllParties')
    DROP PROCEDURE sp_GetAllParties;
GO

CREATE PROCEDURE sp_GetAllParties
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
    ORDER BY CreatedAt DESC;
END
GO