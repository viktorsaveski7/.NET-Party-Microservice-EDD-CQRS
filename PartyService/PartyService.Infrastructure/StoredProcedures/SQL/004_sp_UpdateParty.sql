IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateParty')
    DROP PROCEDURE sp_UpdateParty;
GO

CREATE PROCEDURE sp_UpdateParty
    @Id UNIQUEIDENTIFIER,
    @BirthdayChildName NVARCHAR(100),
    @Title NVARCHAR(100) = NULL,
    @BirthdayChildPhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if party exists
    IF NOT EXISTS (SELECT 1 FROM Parties WHERE Id = @Id)
    BEGIN
        RETURN 0; -- Return 0 if not found
    END
    
    -- Update the party
    UPDATE Parties
    SET 
        BirthdayChildName = @BirthdayChildName,
        Title = @Title,
        BirthdayChildPhotoUrl = @BirthdayChildPhotoUrl
    WHERE Id = @Id;
    
    -- Return the updated party
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