using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Mingley.API.Hubs;
using Mingley.API.Middleware;
using Mingley.API.Services;
using Mingley.Application;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure;
using Mingley.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR hub notifier (depends on both ChatHub + NotificationHub contexts)
builder.Services.AddSignalR(o => { o.EnableDetailedErrors = true; o.MaximumReceiveMessageSize = 102400; });
builder.Services.AddScoped<IHubNotifier, SignalRHubNotifier>();

// JWT
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"] ?? throw new Exception("Jwt:Secret missing in config");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
        // Allow SignalR to use token from query string
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"].FirstOrDefault();
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) && (path.StartsWithSegments("/hubs")))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// CORS — allow any origin for dev; tighten in prod
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddHttpClient(); // required for Razorpay Orders API calls
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mingley API", Version = "v1", Description = "Dating App API" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {{
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }},
        Array.Empty<string>()
    }});
});

var app = builder.Build();

// ── DB Init ──────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MingleyDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Log.Information("✅ Database ready");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Database init failed");
    }
}

// ── Middleware ───────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mingley API v1"); c.RoutePrefix = "swagger"; });
//}
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mingley API v1"); c.RoutePrefix = "swagger"; });

app.UseStaticFiles(); // serve /wwwroot/admin
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");
//app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<NotificationHub>("/hubs/notify");

Log.Information("🚀 Mingley API starting on {Url}", builder.Configuration["App:Url"] ?? "http://localhost:7001");
app.Run();

//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using Mingley.API.Hubs;
//using Mingley.API.Middleware;
//using Mingley.API.Services;
//using Mingley.Application;
//using Mingley.Application.Interfaces;
//using Mingley.Infrastructure;
//using Mingley.Infrastructure.Persistence;
//using Serilog;

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
//    .CreateLogger();

//var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog();

//// ── Services ────────────────────────────────────────────────────────────────
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

//// SignalR hub notifier (depends on both ChatHub + NotificationHub contexts)
//builder.Services.AddSignalR(o => { o.EnableDetailedErrors = true; o.MaximumReceiveMessageSize = 102400; });
//builder.Services.AddScoped<IHubNotifier, SignalRHubNotifier>();

//// JWT
//var jwtSection = builder.Configuration.GetSection("Jwt");
//var secret = jwtSection["Secret"] ?? throw new Exception("Jwt:Secret missing in config");
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(opt =>
//    {
//        opt.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
//            ValidateIssuer           = true,
//            ValidIssuer              = jwtSection["Issuer"],
//            ValidateAudience         = true,
//            ValidAudience            = jwtSection["Audience"],
//            ValidateLifetime         = true,
//            ClockSkew                = TimeSpan.Zero,
//        };
//        // Allow SignalR to use token from query string
//        opt.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = ctx =>
//            {
//                var token = ctx.Request.Query["access_token"].FirstOrDefault();
//                var path  = ctx.HttpContext.Request.Path;
//                if (!string.IsNullOrEmpty(token) && (path.StartsWithSegments("/hubs")))
//                    ctx.Token = token;
//                return Task.CompletedTask;
//            }
//        };
//    });
//builder.Services.AddAuthorization();

//// CORS — allow any origin for dev; tighten in prod
//builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
//    p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "Mingley API", Version = "v1", Description = "Dating App API" });
//    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
//        Scheme = "bearer", BearerFormat = "JWT", In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//    });
//    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {{
//        new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }},
//        Array.Empty<string>()
//    }});
//});

//var app = builder.Build();

//// ── DB Init ──────────────────────────────────────────────────────────────────
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<MingleyDbContext>();
//    try
//    {
//        db.Database.EnsureCreated();
//        Log.Information("✅ Database ready");
//    }
//    catch (Exception ex)
//    {
//        Log.Error(ex, "❌ Database init failed");
//    }
//}

//// ── Middleware ───────────────────────────────────────────────────────────────
//app.UseMiddleware<ExceptionMiddleware>();

////if (app.Environment.IsDevelopment())
////{
////    app.UseSwagger();
////    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mingley API v1"); c.RoutePrefix = "swagger"; });
////}
//app.UseSwagger();
//app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mingley API v1"); c.RoutePrefix = "swagger"; });

//app.UseStaticFiles(); // serve /wwwroot/admin
//app.UseCors();
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();

//// SignalR Hubs
//app.MapHub<ChatHub>("/hubs/chat");
////app.MapHub<NotificationHub>("/hubs/notifications");
//app.MapHub<NotificationHub>("/hubs/notify");

//Log.Information("🚀 Mingley API starting on {Url}", builder.Configuration["App:Url"] ?? "http://localhost:7001");
//app.Run();
