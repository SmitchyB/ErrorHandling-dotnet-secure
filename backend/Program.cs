// Program.cs for .NET 8 Secure Build (Secure Error Handling)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics; // For IExceptionHandlerPathFeature

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Required for MVC controllers like ErrorController
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CORS Configuration (Standard for React frontend communication) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000") // React dev server default port
                       .AllowAnyHeader()
                       .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// --- SECURE IMPLEMENTATION: Secure Error Handling ---
// In a production environment, or for a secure build,
// you must NOT use app.UseDeveloperExceptionPage().
// Instead, use app.UseExceptionHandler() with a generic error page/API response.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Keep this for now, it's bypassed by UseExceptionHandler in production
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Handler for Production-like Error Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // Log the full exception details internally (server-side only)
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "--- SECURE ERROR HANDLING: An unhandled exception occurred ---");
        logger.LogError($"--- Request Path: {context.Request.Path} ---");

        // Return a generic, non-descriptive error message to the client.
        await context.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred. Please try again later.",
            // In a real app, you might provide a correlation ID for support:
            // correlationId = context.TraceIdentifier
        });
    });
});


// --- Standard Middleware ---
app.UseRouting(); // Ensures routing attributes on controllers work
app.UseCors("AllowFrontend"); // Apply the CORS policy
app.UseAuthorization(); // Standard authorization middleware

app.MapControllers(); // Maps controller routes (like /api/error)

app.Run();

// This record defines the simple payload expected by the error trigger endpoint.
public record GenerateRequest(string? SimulatedInput);


// === ErrorController Class ===
[ApiController] // This attribute indicates that this class is an API controller.
[Route("api/[controller]")] // This will make the base route for this controller /api/error
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    // Inject the logger via the constructor (ASP.NET Core's DI)
    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [HttpPost("trigger")] // This maps to POST /api/error/trigger
    // Accepts a simple JSON payload for demonstration, but the error triggers regardless of content.
    public IActionResult TriggerError([FromBody] GenerateRequest request)
    {
        _logger.LogInformation("--- RECEIVED REQUEST TO TRIGGER ERROR ---");
        _logger.LogInformation($"Input received from client: \"{request.SimulatedInput ?? "null"}\"");
        _logger.LogInformation("--- SECURE ERROR HANDLING DEMO: Triggering NullReferenceException ---");
        _logger.LogInformation("--- CLIENT SHOULD RECEIVE GENERIC 500 ERROR, DETAILS LOGGED SERVER-SIDE ---");

        // --- INTENTIONAL EXCEPTION ---
        // This line will deliberately cause a NullReferenceException.
        // The app.UseExceptionHandler() middleware will catch this.
        string[]? nullArray = null;
        string value = nullArray[0]; // This is the line that will throw NullReferenceException

        // This return statement will not be reached due to the exception above.
        return Ok($"This should not be reached. Value: {value}");
    }
}