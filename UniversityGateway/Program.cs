using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ocelot konfigürasyonunu ocelot.json'dan oku
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Ocelot'u ekle
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// 🔍 REQUEST / RESPONSE LOGGING MIDDLEWARE
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("GatewayLogger");

    var req = context.Request;
    var sw = Stopwatch.StartNew();

    logger.LogInformation(
        "[REQUEST] {Method} {Path}{Query} | IP: {IP} | Size: {Size} | AuthHeader: {Auth}",
        req.Method,
        req.Path,
        req.QueryString.ToString(),
        context.Connection.RemoteIpAddress?.ToString(),
        req.ContentLength ?? 0,
        req.Headers.ContainsKey("Authorization") ? "Present" : "None"
    );

    await next();

    sw.Stop();
    var res = context.Response;

    logger.LogInformation(
        "[RESPONSE] {Method} {Path}{Query} -> {StatusCode} | Latency: {Elapsed} ms | Size: {Size}",
        req.Method,
        req.Path,
        req.QueryString.ToString(),
        res.StatusCode,
        sw.ElapsedMilliseconds,
        res.ContentLength ?? 0
    );
});

// HTTPS yönlendirme (istersen kaldırabilirsin)
app.UseHttpsRedirection();

// Ocelot'u pipeline'a ekle
app.UseOcelot().Wait();

app.Run();
