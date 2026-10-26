using System.Security.Cryptography;
using System.Text;

namespace CltPrueba.Api.Infrastructure;

public class ApiKeyMiddleware
{
    private const string HeaderName = "X-API-KEY";
    private readonly RequestDelegate _next;
    private readonly byte[] _expectedKey;
    
    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        var key = configuration["ApiKey"]
                  ?? throw new InvalidOperationException("Falta ApiKey en la configuración");
        _expectedKey = Encoding.UTF8.GetBytes(key);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }
        
        var header = context.Request.Headers[HeaderName].ToString();
        var provided = Encoding.UTF8.GetBytes(header);
        
        if (!CryptographicOperations.FixedTimeEquals(provided, _expectedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        
        await _next(context);

    }
}