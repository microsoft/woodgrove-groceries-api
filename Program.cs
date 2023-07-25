using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

// Add Azure stream log service
builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "azure-diagnostics-";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 5;
});
builder.Logging.AddFilter((provider, category, logLevel) =>
{
    return provider.ToLower().Contains("woodgroveapi");
});

ConfigurationSection WoodgroveGroceriesDownstreamApi = (ConfigurationSection)builder.Configuration.GetSection("WoodgroveGroceriesDownstreamApi");

// Add authentication scheme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamApi("WoodgroveGroceriesDownstreamApi", WoodgroveGroceriesDownstreamApi)
    .AddInMemoryTokenCaches();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                                                 options => 
                                                 {  
                                                     options.TokenValidationParameters.NameClaimType = "name";
                                                 }); 
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Woodgrove Groceries web API",
        Description = "This dotnet Web API entpoint demonstrate how to secure access to API using Microsoft Entra External ID. Checkout the [source code](https://github.com/microsoft/woodgrove-groceries-api) <br> <br> Assembly version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
    });
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
