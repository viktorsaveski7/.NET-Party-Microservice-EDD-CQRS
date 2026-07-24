// Added because of the Outbox Pattern
using PartyService.Domain.Entities;

namespace PartyService.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message);
    Task<IEnumerable<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize = 10);
    Task MarkAsProcessedAsync(Guid id);
    Task IncrementRetryCountAsync(Guid id, string error);
}