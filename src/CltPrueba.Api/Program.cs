using CltPrueba.Api.Api;
using CltPrueba.Api.Application.Addresses.Commands;
using CltPrueba.Api.Application.Addresses.Queries;
using CltPrueba.Api.Application.Currencies.Commands;
using CltPrueba.Api.Application.Currencies.Queries;
using CltPrueba.Api.Application.CurrencyConversion;
using CltPrueba.Api.Application.Users.Commands;
using CltPrueba.Api.Application.Users.Queries;
using CltPrueba.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) =>
    member is null ? null : char.ToLowerInvariant(member.Name[0]) + member.Name[1..];

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-API-KEY",
        Description = "API Key requerida en el header X-API-KEY",
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetUserByIdHandler>();
builder.Services.AddScoped<ListUsersHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<BulkCreateUsersHandler>();
builder.Services.AddScoped<CreateAddressHandler>();
builder.Services.AddScoped<UpdateAddressHandler>();
builder.Services.AddScoped<DeleteAddressHandler>();
builder.Services.AddScoped<ListAddressesByUserHandler>();
builder.Services.AddScoped<CreateCurrencyHandler>();
builder.Services.AddScoped<ListCurrenciesHandler>();
builder.Services.AddScoped<ConvertCurrencyHandler>();

var app = builder.Build();

var factory = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
{
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
    await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
}

app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger/index.html"));

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapUserEndpoints();
app.MapAddressEndpoints();
app.MapCurrencyEndpoints();

app.Run();