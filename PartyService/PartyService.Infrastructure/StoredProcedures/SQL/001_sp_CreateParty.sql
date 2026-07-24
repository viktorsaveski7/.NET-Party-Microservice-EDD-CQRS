IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateParty')
    DROP PROCEDURE sp_CreateParty;
GO

CREATE PROCEDURE sp_CreateParty
    @Id UNIQUEIDENTIFIER,
    @BirthdayChildName NVARCHAR(100),
    @Title NVARCHAR(100) = NULL,
    @BirthdayChildPhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO Parties (Id, BirthdayChildName, Title, BirthdayChildPhotoUrl, CreatedAt)
    VALUES (@Id, @BirthdayChildName, @Title, @BirthdayChildPhotoUrl, GETUTCDATE());
    
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