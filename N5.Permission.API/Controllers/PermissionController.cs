using MediatR;
using Microsoft.AspNetCore.Mvc;
using N5.Permissions.Application;
using N5.Permissions.Application.Commands;
using N5.Permissions.Application.Queries;
using N5.Permissions.Domain.DTO;
using N5.Permissions.Domain.Interfaces;
using N5.Permissions.Infrastructure.Repositories;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace N5.Permissions.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionController(IMediator mediator)
        {
            _mediator = mediator;

        }

        // POST api/<PermissionController>
        [HttpPost]
        public async Task<IActionResult> RequestPermission([FromBody] RequestPermissionCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var permissionId = await _mediator.Send(command, cancellationToken);
                return Ok(new { Id = permissionId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken)
        {
            try
            {
                var permissions = await _mediator.Send(new GetPermissionsQuery(), cancellationToken);

                return permissions is { Count: > 0 }
                    ? Ok(permissions)
                    : NotFound("No permissions found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> ModifyPermission(int id, [FromBody] ModifyPermissionCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest("ID in the URL does not match ID in the body.");

            try
            {
                var result = await _mediator.Send(command, cancellationToken);
                return result ? Ok("Permission updated") : NotFound();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
