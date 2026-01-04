using AspNetProject.Application.DTOs;
using AspNetProject.Domain.Ports.In;
using Microsoft.AspNetCore.Mvc;

namespace AspNetProject.Adapters.In.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try 
        {
            var token = await _userService.LoginAsync(request.Email, request.Password, request.TenantId);
            return Ok(new LoginResponse(token));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid credentials or user blocked.");
        }
        catch (KeyNotFoundException)
        {
            return Unauthorized("Invalid credentials or user blocked.");
        }
    }
}
