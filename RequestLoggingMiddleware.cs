using System.Diagnostics;
namespace TmsApi;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger ){
        _next = next;
        _logger= logger;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];//correlation id 8 char
        var stopwatch = Stopwatch.StartNew();//start stopwatch
    

  //log request entry
        _logger.LogInformation("START request {Method} {Path} correlationId = {correlationId}",
         context.Request.Method,
         context.Request.Path,
         correlationId);
 // add header before response is sent
      context.Response.OnStarting(() =>
      {
          context.Response.Headers["X-Correlation-ID"] = correlationId;
          return Task.CompletedTask;
      });

      try
        {
            await _next(context);//calls next middlware
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "END Request {Method} {Path} StatusCode={StatusCode} ElapsedMs={Elapsed} CorrelationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId
            );
            
        }
    }
}