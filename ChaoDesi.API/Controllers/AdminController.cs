using ChaoDesi.Application.Features.Admin.Requests;
using ChaoDesi.Application.Features.Admin.Responses;
using ChaoDesi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChaoDesi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminPortalAccess")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        return Ok(await _adminService.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("users")]
    public async Task<ActionResult<AdminUserListResponse>> GetUsers(
        [FromQuery] AdminUserQueryRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _adminService.GetUsersAsync(request, cancellationToken));
    }

    [HttpGet("users/{userId:int}")]
    public async Task<ActionResult<AdminUserResponse>> GetUser(int userId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _adminService.GetUserAsync(userId, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("users/admin-account")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<AdminUserResponse>> CreateAdminAccount(
        [FromBody] CreateAdminAccountRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminService.CreateAdminAccountAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<AdminUserResponse>> CreateUser(
        [FromBody] UpsertAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _adminService.CreateUserAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, user);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("users/{userId:int}")]
    public async Task<ActionResult<AdminUserResponse>> UpdateUser(
        int userId,
        [FromBody] UpsertAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _adminService.UpdateUserAsync(userId, request, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("users/{userId:int}")]
    public async Task<IActionResult> DeleteUser(int userId, CancellationToken cancellationToken)
    {
        try
        {
            await _adminService.DeleteUserAsync(userId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
