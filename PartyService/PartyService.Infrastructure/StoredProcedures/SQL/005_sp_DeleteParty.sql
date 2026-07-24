IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_DeleteParty')
    DROP PROCEDURE sp_DeleteParty;
GO

CREATE PROCEDURE sp_DeleteParty
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if party exists
    IF NOT EXISTS (SELECT 1 FROM Parties WHERE Id = @Id)
    BEGIN
        RETURN 0; -- Return 0 if not found
    END
    
    -- Delete the party
    DELETE FROM Parties
    WHERE Id = @Id;
    
    -- Return number of rows affected
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO