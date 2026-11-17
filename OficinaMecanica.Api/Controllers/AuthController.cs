using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Api.Auth;
using OficinaMecanica.Application.Contracts.Auth;
using OficinaMecanica.Infrastructure.Identity;

using Microsoft.AspNetCore.Authorization;

namespace OficinaMecanica.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    JwtTokenService jwtTokenService) : ApiControllerBase
{
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [EndpointSummary("Register a new API user.")]
    [EndpointDescription("Creates a user account and assigns the Admin or User role based on the isAdmin field.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = await userManager.FindByNameAsync(request.Username);
            if (existingUser is not null)
            {
                return Conflict(new { error = "Usuario ja existe." });
            }

            var user = new AppUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(x => x.Description) });
            }

            var role = request.IsAdmin ? "Admin" : "User";
            await userManager.AddToRoleAsync(user, role);

            return Created(string.Empty, new { message = "Usuario criado com sucesso." });
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EndpointSummary("Authenticate a user.")]
    [EndpointDescription("Validates username and password and returns a JWT token for protected endpoints.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await userManager.FindByNameAsync(request.Username);
            if (user is null)
            {
                return Unauthorized(new { error = "Credenciais invalidas." });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = "Credenciais invalidas." });
            }

            var roles = await userManager.GetRolesAsync(user);
            return Ok(jwtTokenService.Generate(user.UserName!, roles));
        }
        catch (Exception exception)
        {
            return HandleException(exception);
        }
    }
}

