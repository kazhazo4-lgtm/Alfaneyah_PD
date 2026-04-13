using Microsoft.AspNetCore.Http;

namespace ProjectsDashboards.Helpers
{
    public static class HttpContextHelper
    {
        public static string GetClientIPAddress(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress?.ToString();
            }
            return ip ?? "Unknown";
        }

        public static string GetUserAgent(HttpContext context)
        {
            return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }
    }
}