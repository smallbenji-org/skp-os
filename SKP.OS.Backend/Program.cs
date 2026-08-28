using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SKP.OS.Backend;
using SKP.OS.Base;
using SKP.OS.Base.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

var settings = new Settings();
builder.Configuration.GetSection("Database").Bind(settings);
builder.Services.AddSingleton(settings);

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.AllowReadingFromString |
            JsonNumberHandling.WriteAsString;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(settings.ConnectionString);
});

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
        options.Tokens.AuthenticatorIssuer = "SKP OS";

        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/accessdenied";

    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    }
});

builder.Services.AddMemoryCache();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<XmlDocsOperationTransformer>();
    builder.Services.AddOpenApi(options =>
    {
        options.AddOperationTransformer<XmlDocsOperationTransformer>();
    });
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapFallback(async context =>
{
    var filePath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "dist", "index.html");
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(filePath);
}).RequireAuthorization();

app.Run();