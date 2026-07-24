// Added because of the Outbox Pattern
using Dapper;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;
using PartyService.Infrastructure.Database;
using System.Data;

namespace PartyService.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public OutboxRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(OutboxMessage message)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            INSERT INTO OutboxMessages (Id, EventType, EventData, RoutingKey, CreatedAt, IsProcessed, RetryCount)
            VALUES (@Id, @EventType, @EventData, @RoutingKey, @CreatedAt, @IsProcessed, @RetryCount)";

        await connection.ExecuteAsync(sql, message);
    }

    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 10)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Added because of the Outbox Pattern - Removed retry limit, exponential backoff handles it
        var sql = @"
        SELECT TOP (@BatchSize) 
            Id, EventType, EventData, RoutingKey, CreatedAt, ProcessedAt, IsProcessed, RetryCount, LastError
        FROM OutboxMessages
        WHERE IsProcessed = 0
        ORDER BY CreatedAt";

        return await connection.QueryAsync<OutboxMessage>(sql, new { BatchSize = batchSize });
    }

    public async Task MarkAsProcessedAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            UPDATE OutboxMessages
            SET IsProcessed = 1, ProcessedAt = GETUTCDATE()
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task IncrementRetryCountAsync(Guid id, string error)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            UPDATE OutboxMessages
            SET RetryCount = RetryCount + 1, LastError = @Error
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id, Error = error });
    }
}