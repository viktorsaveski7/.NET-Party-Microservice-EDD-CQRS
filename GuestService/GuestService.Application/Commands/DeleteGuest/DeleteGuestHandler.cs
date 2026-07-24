using MediatR;
using GuestService.Application.Exceptions;
using GuestService.Application.Interfaces;

namespace GuestService.Application.Commands.DeleteGuest;

public class DeleteGuestHandler : IRequestHandler<DeleteGuestCommand, bool>
{
    private readonly IGuestRepository _guestRepository;

    public DeleteGuestHandler(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    public async Task<bool> Handle(DeleteGuestCommand request, CancellationToken cancellationToken)
    {
        // Check if guest exists
        var existingGuest = await _guestRepository.GetByIdAsync(request.Id);

        if (existingGuest == null)
            throw new NotFoundException("Guest", request.Id);

        // Delete via EF Core
        var deleted = await _guestRepository.DeleteAsync(request.Id);

        if (!deleted)
            throw new Exception("Failed to delete guest");

        return true;
    }
}