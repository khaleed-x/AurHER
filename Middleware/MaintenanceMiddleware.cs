using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AurHER.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions and returns a fully self-contained maintenance page.
    /// It deliberately does NOT use _Layout or the database, so it still works when the DB is down.
    /// </summary>
    public class MaintenanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MaintenanceMiddleware> _logger;

        public MaintenanceMiddleware(RequestDelegate next, ILogger<MaintenanceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                if (context.Response.HasStarted) throw;

                var isDb = IsDatabaseProblem(ex);
                context.Response.Clear();
                context.Response.StatusCode = isDb ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status500InternalServerError;
                context.Response.Headers["Retry-After"] = "120";

                var wantsJson = context.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase)
                                || context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (wantsJson)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"success\":false,\"message\":\"AurHER is under maintenance. Please try again shortly.\"}");
                    return;
                }

                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(Page);
            }
        }

        private static bool IsDatabaseProblem(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e is NpgsqlException || e is SocketException || e is TimeoutException
                    || e is DbUpdateException || e is InvalidOperationException { Message: var m } && m.Contains("connection", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private const string Page = """
<!DOCTYPE html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>AurHER — Under Maintenance</title>
<style>
body{margin:0;min-height:100vh;display:flex;align-items:center;justify-content:center;background:#fdf2f6;font-family:system-ui,-apple-system,Segoe UI,Roboto,sans-serif;color:#1f2937}
.card{max-width:520px;margin:24px;padding:48px 32px;text-align:center}
.icon{font-size:4rem;margin-bottom:12px}
h1{font-size:1.8rem;margin:0 0 12px}
p{color:#6b7280;line-height:1.7;margin:0 0 28px;font-size:.95rem}
a{display:inline-block;padding:12px 28px;background:#be185d;color:#fff;border-radius:999px;font-weight:700;text-decoration:none;font-size:.9rem}
</style></head><body><div class="card">
<div class="icon">🛠️</div>
<h1>We'll Be Right Back</h1>
<p>AurHER is currently under maintenance. Please check back in a few minutes — we're working to get things running again.</p>
<a href="/">Try Again</a>
</div></body></html>
""";
    }
}
