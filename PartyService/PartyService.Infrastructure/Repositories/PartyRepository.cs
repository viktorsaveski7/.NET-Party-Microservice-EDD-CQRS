using Dapper;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;
using PartyService.Infrastructure.Database;
using PartyService.Infrastructure.StoredProcedures;
using System.Data;

namespace PartyService.Infrastructure.Repositories;

public class PartyRepository : IPartyRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PartyRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Party> CreateAsync(Party party)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", party.Id);
        parameters.Add("@BirthdayChildName", party.BirthdayChildName);
        parameters.Add("@Title", party.Title);
        parameters.Add("@BirthdayChildPhotoUrl", party.BirthdayChildPhotoUrl);

        var result = await connection.QueryFirstOrDefaultAsync<Party>(
            StoredProcedureNames.CreateParty,
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result ?? throw new Exception("Failed to create party");
    }

    public async Task<Party?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Party>(
            StoredProcedureNames.GetPartyById,
            new { Id = id },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Party>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Party>(
            StoredProcedureNames.GetAllParties,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Party> UpdateAsync(Party party)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", party.Id);
        parameters.Add("@BirthdayChildName", party.BirthdayChildName);
        parameters.Add("@Title", party.Title);
        parameters.Add("@BirthdayChildPhotoUrl", party.BirthdayChildPhotoUrl);

        var result = await connection.QueryFirstOrDefaultAsync<Party>(
            StoredProcedureNames.UpdateParty,
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result ?? throw new Exception("Failed to update party");
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var result = await connection.QueryFirstOrDefaultAsync<int>(
            StoredProcedureNames.DeleteParty,
            new { Id = id },
            commandType: CommandType.StoredProcedure
        );

        return result > 0;
    }
}