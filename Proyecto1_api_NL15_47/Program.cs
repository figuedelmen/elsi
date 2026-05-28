using Microsoft.AspNetCore.Http.Features;

using Proyecto1_api_NL15_47.Data;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5256");

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddScoped<llave_maestra>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524288000; // 500MB
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 524288000; // 500MB
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

//app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();