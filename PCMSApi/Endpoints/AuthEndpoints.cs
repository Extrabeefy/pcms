using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using PCMSApi.Services;
using PCMSApi.Handlers;
using PCMSApi.Models;

namespace PCMSApi.Endpoints
{
    /// <summary>
    /// Defines the endpoints for authentication-related operations.
    /// </summary>
    public static class AuthEndpoints
    {
        /// <summary>
        /// Maps the authentication endpoints to the application.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/auth/dev-token", () =>
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "dev-user"),
                    new Claim(ClaimTypes.Name, "Developer"),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                    issuer: "dev",
                    audience: "dev-client",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this_is_a_dev_secret_key_change_me")),
                        SecurityAlgorithms.HmacSha256)
                ));

                return Results.Ok(new { token });
            })
            .WithName("GetDevToken")
            .WithDescription("Generates a development JWT token for testing purposes.")
            .Produces(StatusCodes.Status200OK);
        }
    }
}

