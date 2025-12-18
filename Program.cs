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

// Add HttpClient for API proxying
builder.Services.AddHttpClient("P4BooksApi", client =>
{
    // Backend URL from environment variable or default
    var backendUrl = builder.Configuration["BackendUrl"] ?? "https://test93.p4books.cloud";
    client.BaseAddress = new Uri(backendUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

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

    // Custom branding - P4 Software colors (Blue & Orange)
    options.HeadContent = @"
        <link rel=""icon"" type=""image/svg+xml"" href=""data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Cdefs%3E%3ClinearGradient id='g1' x1='0%25' y1='0%25' x2='100%25' y2='100%25'%3E%3Cstop offset='0%25' stop-color='%232563EB'/%3E%3Cstop offset='100%25' stop-color='%23F59E0B'/%3E%3C/linearGradient%3E%3C/defs%3E%3Ccircle cx='50' cy='50' r='45' fill='url(%23g1)'/%3E%3Ctext x='50' y='65' font-family='Arial' font-size='40' font-weight='bold' fill='white' text-anchor='middle'%3EP4%3C/text%3E%3C/svg%3E"">
        <style>
            :root {
                --p4-blue: #2563EB;
                --p4-blue-light: #60A5FA;
                --p4-blue-dark: #1E40AF;
                --p4-orange: #F59E0B;
                --p4-orange-light: #FBBF24;
                --p4-orange-dark: #D97706;
            }
            /* Top bar with gradient matching logo */
            .swagger-ui .topbar {
                background: linear-gradient(135deg, var(--p4-blue) 0%, var(--p4-blue-dark) 50%, var(--p4-orange) 100%);
                padding: 12px 0;
            }
            .swagger-ui .topbar .download-url-wrapper .select-label { color: #fff; }
            .swagger-ui .topbar-wrapper img {
                content: url('data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 280 50%22%3E%3Cdefs%3E%3ClinearGradient id=%22wave%22 x1=%220%25%22 y1=%220%25%22 x2=%22100%25%22 y2=%220%25%22%3E%3Cstop offset=%220%25%22 stop-color=%22%2360A5FA%22/%3E%3Cstop offset=%2250%25%22 stop-color=%22%232563EB%22/%3E%3Cstop offset=%22100%25%22 stop-color=%22%23F59E0B%22/%3E%3C/linearGradient%3E%3C/defs%3E%3Ctext x=%225%22 y=%2235%22 font-family=%22Arial%22 font-size=%2228%22 font-weight=%22bold%22 fill=%22white%22%3EP4%3C/text%3E%3Ctext x=%2245%22 y=%2235%22 font-family=%22Arial%22 font-size=%2228%22 fill=%22white%22%3ESOFTWARE%3C/text%3E%3Ctext x=%22185%22 y=%2235%22 font-family=%22Arial%22 font-size=%2220%22 fill=%22%23FBBF24%22%3EAPI%3C/text%3E%3C/svg%3E');
                height: 50px;
            }
            /* 2-Column Header Layout */
            .swagger-ui .info { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; align-items: start; }
            .swagger-ui .info hgroup { grid-column: 1; }
            .swagger-ui .info .description { grid-column: 2; grid-row: 1 / 3; border-left: 3px solid var(--p4-orange); padding-left: 20px; margin: 0; }
            .swagger-ui .info .base-url, .swagger-ui .info .link { grid-column: 1; }
            .swagger-ui .info .title { color: var(--p4-blue-dark); font-size: 2.2em; margin-bottom: 10px; }
            .swagger-ui .info .title small.version-stamp { background-color: var(--p4-orange); }
            .swagger-ui .info .description .markdown p { line-height: 1.6; }
            .swagger-ui .info .description .markdown h3 { color: var(--p4-blue-dark); border-bottom: 1px solid #e2e8f0; padding-bottom: 5px; margin-top: 15px; }
            .swagger-ui .info .description .markdown ul { padding-left: 20px; }
            .swagger-ui .info .description .markdown code { background: #FEF3C7; color: var(--p4-orange-dark); padding: 2px 6px; border-radius: 3px; font-size: 0.9em; }
            @media (max-width: 1200px) { .swagger-ui .info { grid-template-columns: 1fr; } .swagger-ui .info .description { grid-column: 1; grid-row: auto; border-left: none; border-top: 3px solid var(--p4-orange); padding-left: 0; padding-top: 20px; margin-top: 20px; } }
            /* Operation blocks */
            .swagger-ui .opblock { border-radius: 8px; margin-bottom: 10px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
            .swagger-ui .opblock.opblock-post { border-color: #10B981; background: rgba(16, 185, 129, 0.1); }
            .swagger-ui .opblock.opblock-get { border-color: var(--p4-blue); background: rgba(37, 99, 235, 0.1); }
            .swagger-ui .opblock.opblock-put { border-color: var(--p4-orange); background: rgba(245, 158, 11, 0.1); }
            .swagger-ui .opblock.opblock-delete { border-color: #EF4444; background: rgba(239, 68, 68, 0.1); }
            .swagger-ui .opblock.opblock-patch { border-color: #8B5CF6; background: rgba(139, 92, 246, 0.1); }
            .swagger-ui .opblock .opblock-summary { padding: 10px 15px; }
            .swagger-ui .opblock .opblock-summary-method { border-radius: 4px; font-weight: 600; }
            /* Tag sections */
            .swagger-ui .opblock-tag { font-size: 1.3em; border-bottom: 2px solid var(--p4-blue); padding-bottom: 10px; margin-bottom: 15px; }
            .swagger-ui .opblock-tag:hover { background: rgba(37, 99, 235, 0.05); }
            /* Authorize button - Orange accent */
            .swagger-ui .btn.authorize { background-color: var(--p4-orange); border-color: var(--p4-orange); border-radius: 4px; }
            .swagger-ui .btn.authorize svg { fill: #fff; }
            .swagger-ui .btn.authorize:hover { background-color: var(--p4-orange-dark); }
            /* Execute button - Blue */
            .swagger-ui .btn.execute { background-color: var(--p4-blue); border-radius: 4px; }
            .swagger-ui .btn.execute:hover { background-color: var(--p4-blue-dark); }
            /* Cancel button */
            .swagger-ui .btn.cancel { border-color: var(--p4-orange); color: var(--p4-orange); }
            /* Models section */
            .swagger-ui section.models { border-radius: 8px; }
            .swagger-ui section.models .model-container { background: #f8fafc; border-radius: 4px; }
            .swagger-ui .responses-wrapper { border-radius: 8px; }
            .swagger-ui .response { border-radius: 4px; }
            .swagger-ui .loading-container { padding: 40px; }
            /* Links */
            .swagger-ui a { color: var(--p4-blue); }
            .swagger-ui a:hover { color: var(--p4-blue-dark); }
            /* Hide Servers section only - keep Authorize button visible */
            .swagger-ui .servers { display: none !important; }
            .swagger-ui .servers-title { display: none !important; }
            .swagger-ui .scheme-container .servers { display: none !important; }
            .swagger-ui label[for=""servers""] { display: none !important; }
            .swagger-ui .scheme-container > label:first-child { display: none !important; }
            .swagger-ui .scheme-container > .servers { display: none !important; }
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

// Proxy all /api/* requests to the P4Books backend
app.Map("/api/{**path}", async (HttpContext context, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("P4BooksApi");

    // Build the target URL
    var path = context.Request.Path.Value;
    var query = context.Request.QueryString.Value;
    var targetUrl = $"{path}{query}";

    // Create the proxy request
    var requestMessage = new HttpRequestMessage
    {
        Method = new HttpMethod(context.Request.Method),
        RequestUri = new Uri(targetUrl, UriKind.Relative)
    };

    // Copy headers (except Host)
    foreach (var header in context.Request.Headers)
    {
        if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    // Copy request body for POST/PUT/PATCH
    if (context.Request.ContentLength > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
    {
        requestMessage.Content = new StreamContent(context.Request.Body);
        if (context.Request.ContentType != null)
        {
            requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
        }
    }

    try
    {
        var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

        // Copy response status
        context.Response.StatusCode = (int)response.StatusCode;

        // Copy response headers
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in response.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        // Remove transfer-encoding if present (Kestrel handles this)
        context.Response.Headers.Remove("transfer-encoding");

        // Copy response body
        await response.Content.CopyToAsync(context.Response.Body);
    }
    catch (HttpRequestException ex)
    {
        context.Response.StatusCode = 502;
        await context.Response.WriteAsJsonAsync(new { error = "Backend unavailable", message = ex.Message });
    }
}).ExcludeFromDescription();

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
