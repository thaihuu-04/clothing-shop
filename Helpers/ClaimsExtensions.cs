using System.Security.Claims;

namespace QA.Helpers
{
    public static class ClaimsExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(value, out int id) ? id : 0;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("FullName")
                ?? user.Identity?.Name
                ?? "";
        }
    }
}
