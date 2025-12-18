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

// Add HttpClient for API proxying with custom DNS resolution
builder.Services.AddHttpClient("P4BooksApi", client =>
{
    // Backend URL - app.p4books.cloud is the main API server
    var backendUrl = builder.Configuration["BackendUrl"] ?? "https://app.p4books.cloud";
    client.BaseAddress = new Uri(backendUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new SocketsHttpHandler
    {
        // Use custom DNS callback to resolve using Google DNS
        ConnectCallback = async (context, cancellationToken) =>
        {
            // Resolve DNS using system resolver (which should work)
            // If p4books.cloud domains fail, use hardcoded IP
            string host = context.DnsEndPoint.Host;
            int port = context.DnsEndPoint.Port;

            System.Net.IPAddress? ipAddress = null;

            // Try to resolve the host
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(host, cancellationToken);
                if (addresses.Length > 0)
                {
                    ipAddress = addresses[0];
                }
            }
            catch
            {
                // If DNS resolution fails for p4books.cloud domains, use known IP
                if (host.EndsWith("p4books.cloud", StringComparison.OrdinalIgnoreCase))
                {
                    ipAddress = System.Net.IPAddress.Parse("20.9.134.138");
                }
            }

            if (ipAddress == null)
            {
                throw new Exception($"Could not resolve host: {host}");
            }

            var socket = new System.Net.Sockets.Socket(System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            socket.NoDelay = true;

            try
            {
                await socket.ConnectAsync(new System.Net.IPEndPoint(ipAddress, port), cancellationToken);
                return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };
    return handler;
});

var app = builder.Build();

app.UseCors("AllowAll");

// Serve static files from wwwroot (logo, etc.)
app.UseStaticFiles();

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

    // Do NOT persist authorization - clear on browser close
    options.ConfigObject.AdditionalItems["persistAuthorization"] = false;
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
        </style>
        <script>
            // Session timeout - logout after 10 minutes of inactivity
            (function() {
                const TIMEOUT_MS = 10 * 60 * 1000; // 10 minutes
                let timeoutId;

                function clearAuth() {
                    // Clear localStorage authorization
                    localStorage.removeItem('authorized');
                    // Clear sessionStorage
                    sessionStorage.clear();
                    // Reload page to reset Swagger UI state
                    window.location.reload();
                }

                function resetTimer() {
                    clearTimeout(timeoutId);
                    timeoutId = setTimeout(clearAuth, TIMEOUT_MS);
                }

                // Reset timer on user activity
                ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'].forEach(function(event) {
                    document.addEventListener(event, resetTimer, true);
                });

                // Start the timer
                resetTimer();

                // Also clear auth on page unload (browser close/tab close)
                window.addEventListener('beforeunload', function() {
                    localStorage.removeItem('authorized');
                });
            })();
        </script>";
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

    // Add X-Tenant-Alias header - use test93 until backend "api" alias lookup is deployed
    requestMessage.Headers.TryAddWithoutValidation("X-Tenant-Alias", "test93");

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

// Health check endpoint - pretty HTML page
app.MapGet("/health", (HttpContext context) =>
{
    var uptime = DateTime.UtcNow;
    var html = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>P4Books API - Health Status</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            background: linear-gradient(135deg, #1E40AF 0%, #2563EB 50%, #F59E0B 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .card {{
            background: white;
            border-radius: 20px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            padding: 40px;
            max-width: 500px;
            width: 100%;
            text-align: center;
        }}
        .logo {{
            width: 200px;
            max-width: 100%;
            height: auto;
            margin: 0 auto 20px;
            display: block;
        }}
        h1 {{
            color: #1E40AF;
            font-size: 24px;
            margin-bottom: 10px;
        }}
        .status {{
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background: #10B981;
            color: white;
            padding: 8px 20px;
            border-radius: 50px;
            font-weight: 600;
            margin: 20px 0;
        }}
        .status::before {{
            content: '';
            width: 12px;
            height: 12px;
            background: white;
            border-radius: 50%;
            animation: pulse 2s infinite;
        }}
        @keyframes pulse {{
            0%, 100% {{ opacity: 1; }}
            50% {{ opacity: 0.5; }}
        }}
        .info {{
            margin-top: 30px;
            text-align: left;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            padding: 12px 0;
            border-bottom: 1px solid #E5E7EB;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .info-label {{
            color: #6B7280;
            font-size: 14px;
        }}
        .info-value {{
            color: #1F2937;
            font-weight: 600;
            font-size: 14px;
        }}
        .links {{
            margin-top: 30px;
            display: flex;
            gap: 10px;
            justify-content: center;
        }}
        .links a {{
            padding: 10px 20px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 500;
            font-size: 14px;
            transition: all 0.2s;
        }}
        .links a.primary {{
            background: #2563EB;
            color: white;
        }}
        .links a.primary:hover {{
            background: #1E40AF;
        }}
        .links a.secondary {{
            background: #F3F4F6;
            color: #374151;
        }}
        .links a.secondary:hover {{
            background: #E5E7EB;
        }}
        .footer {{
            margin-top: 30px;
            color: #9CA3AF;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <img src=""/logo.png"" alt=""P4 Software"" class=""logo"">
        <h1>P4Books API</h1>
        <p style=""color: #6B7280;"">Documentation Server</p>

        <div class=""status"">Healthy</div>

        <div class=""info"">
            <div class=""info-row"">
                <span class=""info-label"">Version</span>
                <span class=""info-value"">1.0.1</span>
            </div>
            <div class=""info-row"">
                <span class=""info-label"">Environment</span>
                <span class=""info-value"">Production</span>
            </div>
            <div class=""info-row"">
                <span class=""info-label"">Server Time</span>
                <span class=""info-value"">{uptime:yyyy-MM-dd HH:mm:ss} UTC</span>
            </div>
            <div class=""info-row"">
                <span class=""info-label"">OpenAPI</span>
                <span class=""info-value"">3.0.3</span>
            </div>
        </div>

        <div class=""links"">
            <a href=""/"" class=""primary"">API Documentation</a>
            <a href=""/openapi.json"" class=""secondary"">OpenAPI Spec</a>
        </div>

        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} P4 Software. All rights reserved.
        </div>
    </div>
</body>
</html>";

    context.Response.ContentType = "text/html";
    return Results.Content(html, "text/html");
}).ExcludeFromDescription();

// API version endpoint - pretty HTML page
app.MapGet("/version", (HttpContext context) =>
{
    var html = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>P4Books API - Version Info</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            background: linear-gradient(135deg, #1E40AF 0%, #2563EB 50%, #F59E0B 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .card {{
            background: white;
            border-radius: 20px;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
            padding: 40px;
            max-width: 600px;
            width: 100%;
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            width: 200px;
            max-width: 100%;
            height: auto;
            margin: 0 auto 20px;
            display: block;
        }}
        h1 {{
            color: #1E40AF;
            font-size: 24px;
            margin-bottom: 5px;
        }}
        .subtitle {{
            color: #6B7280;
            font-size: 14px;
        }}
        .version-badge {{
            display: inline-block;
            background: linear-gradient(135deg, #2563EB 0%, #F59E0B 100%);
            color: white;
            padding: 8px 24px;
            border-radius: 50px;
            font-size: 20px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .section {{
            margin-top: 30px;
        }}
        .section-title {{
            color: #1E40AF;
            font-size: 14px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 2px solid #E5E7EB;
        }}
        .grid {{
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 15px;
        }}
        .grid-item {{
            background: #F9FAFB;
            padding: 15px;
            border-radius: 10px;
            border-left: 4px solid #2563EB;
        }}
        .grid-item.orange {{
            border-left-color: #F59E0B;
        }}
        .grid-label {{
            color: #6B7280;
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}
        .grid-value {{
            color: #1F2937;
            font-size: 18px;
            font-weight: 600;
            margin-top: 5px;
        }}
        .features {{
            margin-top: 20px;
        }}
        .feature {{
            display: flex;
            align-items: center;
            gap: 10px;
            padding: 10px 0;
            border-bottom: 1px solid #E5E7EB;
        }}
        .feature:last-child {{
            border-bottom: none;
        }}
        .feature-icon {{
            width: 32px;
            height: 32px;
            background: #EFF6FF;
            border-radius: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #2563EB;
            font-size: 16px;
        }}
        .feature-text {{
            flex: 1;
        }}
        .feature-title {{
            color: #1F2937;
            font-weight: 500;
        }}
        .feature-desc {{
            color: #6B7280;
            font-size: 12px;
        }}
        .links {{
            margin-top: 30px;
            display: flex;
            gap: 10px;
            justify-content: center;
        }}
        .links a {{
            padding: 12px 24px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 500;
            font-size: 14px;
            transition: all 0.2s;
        }}
        .links a.primary {{
            background: #2563EB;
            color: white;
        }}
        .links a.primary:hover {{
            background: #1E40AF;
        }}
        .links a.secondary {{
            background: #F3F4F6;
            color: #374151;
        }}
        .links a.secondary:hover {{
            background: #E5E7EB;
        }}
        .footer {{
            margin-top: 30px;
            text-align: center;
            color: #9CA3AF;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""header"">
            <img src=""/logo.png"" alt=""P4 Software"" class=""logo"">
            <h1>P4Books ERP API</h1>
            <p class=""subtitle"">Enterprise Resource Planning System</p>
            <div class=""version-badge"">v1.0.1</div>
        </div>

        <div class=""section"">
            <div class=""section-title"">Technical Specifications</div>
            <div class=""grid"">
                <div class=""grid-item"">
                    <div class=""grid-label"">API Version</div>
                    <div class=""grid-value"">1.0.1</div>
                </div>
                <div class=""grid-item orange"">
                    <div class=""grid-label"">OpenAPI Spec</div>
                    <div class=""grid-value"">3.0.3</div>
                </div>
                <div class=""grid-item"">
                    <div class=""grid-label"">Swashbuckle</div>
                    <div class=""grid-value"">10.0.1</div>
                </div>
                <div class=""grid-item orange"">
                    <div class=""grid-label"">.NET Runtime</div>
                    <div class=""grid-value"">10.0</div>
                </div>
            </div>
        </div>

        <div class=""section"">
            <div class=""section-title"">Available Modules</div>
            <div class=""features"">
                <div class=""feature"">
                    <div class=""feature-icon"">📦</div>
                    <div class=""feature-text"">
                        <div class=""feature-title"">Inventory Management</div>
                        <div class=""feature-desc"">Stock levels, warehouses, receipts, deliveries</div>
                    </div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">💰</div>
                    <div class=""feature-text"">
                        <div class=""feature-title"">Sales & Invoicing</div>
                        <div class=""feature-desc"">Quotes, orders, invoices, customers</div>
                    </div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">🛒</div>
                    <div class=""feature-text"">
                        <div class=""feature-title"">Purchasing</div>
                        <div class=""feature-desc"">Purchase orders, vendor bills, vendors</div>
                    </div>
                </div>
                <div class=""feature"">
                    <div class=""feature-icon"">📊</div>
                    <div class=""feature-text"">
                        <div class=""feature-title"">Accounting</div>
                        <div class=""feature-desc"">Journal entries, chart of accounts, reports</div>
                    </div>
                </div>
            </div>
        </div>

        <div class=""links"">
            <a href=""/"" class=""primary"">API Documentation</a>
            <a href=""/health"" class=""secondary"">Health Status</a>
        </div>

        <div class=""footer"">
            &copy; {DateTime.UtcNow.Year} P4 Software. All rights reserved.
        </div>
    </div>
</body>
</html>";

    context.Response.ContentType = "text/html";
    return Results.Content(html, "text/html");
}).ExcludeFromDescription();

Console.WriteLine("P4Books API Documentation Server starting...");
Console.WriteLine("P4Books API v1.0.1 - Production Build");
Console.WriteLine("Access Swagger UI at: http://localhost:5000 or https://api.p4books.cloud");

app.Run();
