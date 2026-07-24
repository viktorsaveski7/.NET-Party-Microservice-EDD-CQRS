using MediatR;
using Microsoft.AspNetCore.Mvc;
using GuestService.Application.Commands.CreateGuest;
using GuestService.Application.Commands.UpdateGuest;
using GuestService.Application.Commands.DeleteGuest;
using GuestService.Application.Queries.GetGuestById;
using GuestService.Application.Queries.GetAllGuests;

namespace GuestService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GuestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGuest([FromBody] CreateGuestCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetGuestById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGuestById(Guid id)
    {
        var result = await _mediator.Send(new GetGuestByIdQuery(id));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllGuests()
    {
        var result = await _mediator.Send(new GetAllGuestsQuery());
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGuest(Guid id, [FromBody] UpdateGuestCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { message = "ID in URL does not match ID in body" });

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGuest(Guid id)
    {
        await _mediator.Send(new DeleteGuestCommand(id));
        return NoContent();
    }
}