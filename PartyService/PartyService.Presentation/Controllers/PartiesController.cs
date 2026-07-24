using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartyService.Application.Commands.CreateParty;
using PartyService.Application.Commands.DeleteParty;
using PartyService.Application.Commands.UpdateParty;
using PartyService.Application.Queries.GetAllParties;
using PartyService.Application.Queries.GetPartyById;

namespace PartyService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateParty([FromBody] CreatePartyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateParty), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPartyById(Guid id)
    {
        var result = await _mediator.Send(new GetPartyByIdQuery(id));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllParties()
    {
        var result = await _mediator.Send(new GetAllPartiesQuery());
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateParty(Guid id, [FromBody] UpdatePartyCommand command)
    {
        // Ensure URL ID matches body ID
        if (id != command.Id)
            return BadRequest(new { message = "ID in URL does not match ID in body" });

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteParty(Guid id)
    {
        await _mediator.Send(new DeletePartyCommand(id));
        return NoContent(); // 204 No Content - Standard for successful DELETE
    }
}