using System.Security.Claims;
using ChaoDesi.Application.Features.UserProfile.Requests;
using ChaoDesi.Application.Features.UserProfile.Responses;
using ChaoDesi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChaoDesi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileMeResponse>> GetMe(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _userProfileService.GetMyProfileAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("me")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserProfileMeResponse>> UpdateMe(
        [FromForm] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var response = await _userProfileService.UpdateMyProfileAsync(userId, request, cancellationToken);
            return Ok(response);
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

    private bool TryGetCurrentUserId(out int userId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out userId);
    }
}
