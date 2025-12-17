var builder = WebApplication.CreateBuilder(args);

// Configure CORS to allow Swagger UI to call the P4Books API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add endpoint routing for minimal APIs
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("AllowAll");

// Enable routing
app.UseRouting();

// Serve Swagger UI at root - MUST come before MapGet endpoints
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi.json", "P4Books ERP API v1");
    options.RoutePrefix = string.Empty; // Serve at root

    // Core UI Configuration
    options.DocumentTitle = "P4Books API Documentation";
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.ShowExtensions();
    options.EnableDeepLinking();
    options.DisplayOperationId();
    options.EnableTryItOutByDefault();

    // Swashbuckle 10.x Enhanced Features
    options.ShowCommonExtensions();
    options.EnableValidator();
    options.SupportedSubmitMethods(
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Get,
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Post,
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Put,
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Delete,
        Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod.Patch
    );

    // Persist authorization tokens and syntax highlighting
    options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    options.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
    {
        ["activated"] = true,
        ["theme"] = "monokai"
    };
    options.ConfigObject.AdditionalItems["tryItOutEnabled"] = true;
    options.ConfigObject.AdditionalItems["requestSnippetsEnabled"] = true;

    // Custom branding with 2-column header layout
    options.HeadContent = @"
        <link rel=""icon"" type=""image/svg+xml"" href=""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Ctext y='.9em' font-size='90'%3E📚%3C/text%3E%3C/svg%3E"">
        <style>
            .swagger-ui .topbar { background-color: #1a365d; padding: 10px 0; }
            .swagger-ui .topbar .download-url-wrapper .select-label { color: #fff; }
            .swagger-ui .topbar-wrapper img { content: url('data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 200 40%22%3E%3Ctext x=%2210%22 y=%2228%22 font-family=%22Arial%22 font-size=%2224%22 fill=%22white%22 font-weight=%22bold%22%3EP4Books API%3C/text%3E%3C/svg%3E'); height: 40px; }
            .swagger-ui .info { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; align-items: start; }
            .swagger-ui .info hgroup { grid-column: 1; }
            .swagger-ui .info .description { grid-column: 2; grid-row: 1 / 3; border-left: 3px solid #3182ce; padding-left: 20px; margin: 0; }
            .swagger-ui .info .base-url, .swagger-ui .info .link { grid-column: 1; }
            .swagger-ui .info .title { color: #1a365d; font-size: 2.2em; margin-bottom: 10px; }
            .swagger-ui .info .title small.version-stamp { background-color: #3182ce; }
            .swagger-ui .info .description .markdown p { line-height: 1.6; }
            .swagger-ui .info .description .markdown h3 { color: #1a365d; border-bottom: 1px solid #e2e8f0; padding-bottom: 5px; margin-top: 15px; }
            .swagger-ui .info .description .markdown ul { padding-left: 20px; }
            .swagger-ui .info .description .markdown code { background: #edf2f7; padding: 2px 6px; border-radius: 3px; font-size: 0.9em; }
            @media (max-width: 1200px) { .swagger-ui .info { grid-template-columns: 1fr; } .swagger-ui .info .description { grid-column: 1; grid-row: auto; border-left: none; border-top: 3px solid #3182ce; padding-left: 0; padding-top: 20px; margin-top: 20px; } }
            .swagger-ui .opblock { border-radius: 8px; margin-bottom: 10px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
            .swagger-ui .opblock.opblock-post { border-color: #49cc90; background: rgba(73, 204, 144, 0.1); }
            .swagger-ui .opblock.opblock-get { border-color: #61affe; background: rgba(97, 175, 254, 0.1); }
            .swagger-ui .opblock.opblock-put { border-color: #fca130; background: rgba(252, 161, 48, 0.1); }
            .swagger-ui .opblock.opblock-delete { border-color: #f93e3e; background: rgba(249, 62, 62, 0.1); }
            .swagger-ui .opblock.opblock-patch { border-color: #50e3c2; background: rgba(80, 227, 194, 0.1); }
            .swagger-ui .opblock .opblock-summary { padding: 10px 15px; }
            .swagger-ui .opblock .opblock-summary-method { border-radius: 4px; font-weight: 600; }
            .swagger-ui .opblock-tag { font-size: 1.3em; border-bottom: 2px solid #1a365d; padding-bottom: 10px; margin-bottom: 15px; }
            .swagger-ui .opblock-tag:hover { background: rgba(26, 54, 93, 0.05); }
            .swagger-ui .btn.authorize { background-color: #3182ce; border-color: #3182ce; border-radius: 4px; }
            .swagger-ui .btn.authorize svg { fill: #fff; }
            .swagger-ui .btn.authorize:hover { background-color: #2c5282; }
            .swagger-ui .btn.execute { background-color: #3182ce; border-radius: 4px; }
            .swagger-ui .btn.execute:hover { background-color: #2c5282; }
            .swagger-ui section.models { border-radius: 8px; }
            .swagger-ui section.models .model-container { background: #f8fafc; border-radius: 4px; }
            .swagger-ui .responses-wrapper { border-radius: 8px; }
            .swagger-ui .response { border-radius: 4px; }
            .swagger-ui .loading-container { padding: 40px; }
        </style>";
});

// Serve the static OpenAPI spec file with caching headers
app.MapGet("/openapi.json", async context =>
{
    var filePath = Path.Combine(app.Environment.ContentRootPath, "openapi.json");
    if (File.Exists(filePath))
    {
        context.Response.ContentType = "application/json";
        context.Response.Headers.CacheControl = "public, max-age=300"; // 5 min cache
        await context.Response.SendFileAsync(filePath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("OpenAPI spec not found");
    }
}).ExcludeFromDescription();

// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/index.html")).ExcludeFromDescription();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new {
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "10.0.1",
    service = "P4Books API Documentation"
})).WithTags("System").WithName("HealthCheck");

// API version endpoint
app.MapGet("/version", () => Results.Ok(new {
    api = "P4Books ERP",
    version = "1.0.0",
    openapi = "3.0.3",
    swashbuckle = "10.0.1"
})).WithTags("System").WithName("GetVersion");

Console.WriteLine("P4Books API Documentation Server starting...");
Console.WriteLine("Swashbuckle.AspNetCore v10.0.1 - OpenAPI 3.1 Support");
Console.WriteLine("Access Swagger UI at: http://localhost:5000 or https://api.p4books.cloud");

app.Run();
