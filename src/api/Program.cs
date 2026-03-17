using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Integration Settings
builder.Services.Configure<DocumentationCompleteness.Api.Models.Configuration.IntegrationSettings>(
    builder.Configuration.GetSection("Integrations"));

// Add Services
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.IGitHubService, DocumentationCompleteness.Api.Services.GitHubService>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.IAzureDevOpsService, DocumentationCompleteness.Api.Services.AzureDevOpsService>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.IGitService, DocumentationCompleteness.Api.Services.GitService>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.IFileService, DocumentationCompleteness.Api.Services.FileService>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.Analysis.ICodeAnalyzer, DocumentationCompleteness.Api.Services.Analysis.UniversalCodeAnalyzer>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.IAnalysisService, DocumentationCompleteness.Api.Services.AnalysisService>();

// Add AI Services
builder.Services.AddSingleton<DocumentationCompleteness.Api.Services.AI.AzureOpenAIClientWrapper>();
builder.Services.AddSingleton<DocumentationCompleteness.Api.Services.AI.PromptTemplateEngine>();
builder.Services.AddSingleton<DocumentationCompleteness.Api.Services.AI.ResponseValidator>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.AIDocumentationService>();
builder.Services.AddScoped<DocumentationCompleteness.Api.Services.DashboardService>();

// Background Job Processing
builder.Services.AddSingleton<DocumentationCompleteness.Api.Services.Background.AnalysisJobQueue>();
builder.Services.AddHostedService<DocumentationCompleteness.Api.Services.Background.AnalysisWorker>();



// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = 
        BackgroundServiceExceptionBehavior.Ignore;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
