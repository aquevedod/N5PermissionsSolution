using MediatR;
using Microsoft.AspNetCore.Mvc;
using N5.Permissions.Application.DTO;
using System.ComponentModel.DataAnnotations;
using N5.Permissions.Infrastructure.Repositories;
using N5.Permissions.Domain.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N5.Permissions.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionTypeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PermissionTypeController(IMediator mediator, IPermissionTypeRepository permissionTypeRepository)
        {
            _mediator = mediator;
        }

        // POST api/<PermissionTypeController>
        [HttpPost("PermissionTypes")]
        public async Task<IActionResult> CreatePermissionType([FromBody] CreatePermissionTypeCommand command)
        {
            try
            {
                var id = await _mediator.Send(command);
                return CreatedAtAction(nameof(CreatePermissionType), new { id }, command);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
