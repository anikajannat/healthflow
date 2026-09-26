using System.Security.Claims;

namespace Healthcare.Api.Helpers;

public static class ClaimsExtensions
{
    public static Guid UserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing user id claim"));

    public static string Role(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role) ?? "";
}
