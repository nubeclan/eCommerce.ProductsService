using eCommerce.ProductsService.Infrastructure;
using eCommerce.ProductsService.Application;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

builder.Services.AddControllers();

// Configurar OpenAPI integrado
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "eCommerce Products Service API";
        document.Info.Version = "v1";
        document.Info.Description = "API para gestión de productos del sistema eCommerce";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Exponer el JSON OpenAPI
    app.MapOpenApi();

    // Transformar y exponer una versión compatible con Swagger UI (OpenAPI 3.0.x)
    app.MapGet("/swagger/v1/swagger.json", async (HttpContext http) =>
    {
        var scheme = http.Request.Scheme;
        var host = http.Request.Host.Value; // e.g. localhost:5146
        var openApiUrl = $"{scheme}://{host}/openapi/v1.json";

        using var client = new HttpClient();
        var resp = await client.GetAsync(openApiUrl, http.RequestAborted);
        if (!resp.IsSuccessStatusCode)
        {
            http.Response.StatusCode = (int)resp.StatusCode;
            await http.Response.WriteAsync(await resp.Content.ReadAsStringAsync(http.RequestAborted), http.RequestAborted);
            return;
        }

        var body = await resp.Content.ReadAsStringAsync(http.RequestAborted);

        // Parse and modify
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(body);
        }
        catch
        {
            node = null;
        }

        // If the returned content is not an object, wrap it
        JsonObject root = node as JsonObject ?? new JsonObject();

        // If original was an array, include it under a key to avoid breaking
        if (node is JsonArray arr)
        {
            root["items"] = arr;
        }

        // Force openapi version to 3.0.0 for Swagger UI compatibility if missing or invalid
        root["openapi"] = "3.0.0";

        // Ensure servers array exists
        if (root["servers"] == null)
        {
            var serverObj = new JsonArray();
            var server = new JsonObject { ["url"] = $"{scheme}://{host}" };
            serverObj.Add(server);
            root["servers"] = serverObj;
        }

        http.Response.ContentType = "application/json";
        await http.Response.WriteAsync(root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }), http.RequestAborted);
    });

    // UI clásico de Swagger consumiendo la versión transformada
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "eCommerce Products Service API v1");
        c.RoutePrefix = "swagger"; // UI accesible en /swagger
    });

    // También dejar la referencia de Scalar en la raíz (opcional)
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("eCommerce Products Service API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
