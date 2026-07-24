using MediatR;

namespace GuestService.Application.Commands.DeleteGuest;

public record DeleteGuestCommand(Guid Id) : IRequest<bool>;