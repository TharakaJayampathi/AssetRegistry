using AssetRegistry.Exceptions;
using AssetRegistry.Extensions;
using AssetRegistry.Handlers;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using AssetRegistry.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secret = jwtConfig["Secret"];
builder.Services.AddJwtAuthentication(secret);

builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IDateTimeService, DateTimeService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorizationPermissions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var url = context.Request.Path.Value;
    if (url.Contains("/identity/account"))
    {
        context.Request.Path = "/";
    }

    string specialChar = @"|!#$/()»«@£§€{};~`'<>,";

    if (context.Request.QueryString.HasValue)
    {
        foreach (var item in specialChar)
        {
            if (context.Request.QueryString.Value.Contains(item))
            {
                context.Response.Headers.Clear();
                context.Response.Redirect("error/500");
                break;
            }
        }

    }
    context.Response.Headers.Remove("X-Powered-By");
    context.Response.Headers.Remove("X-AspNet-Version");
    context.Response.Headers.Remove("X-AspNetMvc-Version");
    context.Response.Headers.Remove("X-Original-URL");

    context.Response.Headers.Add("X-Frame-Options", "deny");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("X-Content-Type-Options", "nosnoff");

    if (context.Response.StatusCode == 200)
    {
        try
        {
            await next();
        }
        catch (JWTInvalidException ex)
        {
            context.Response.Headers.Clear();
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { code = 401, msg = $"{ex.Message}", data = "" }));
        }
        catch (PermissionDeniedException ex)
        {
            context.Response.Headers.Clear();
            context.Response.StatusCode = 403;
            context.Response.Redirect($"/error/{ex.Message.ToLower()}");
        }
        catch (Exception ex)
        {
            context.Response.Headers.Clear();
            context.Response.StatusCode = 500;
            context.Response.Redirect($"/error/{500}");
        }
    }
    else
    {
        context.Response.Headers.Clear();
        context.Response.Redirect($"/error/{context.Response.StatusCode}");
        await next();
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
