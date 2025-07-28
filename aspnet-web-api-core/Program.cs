
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetWebApiCore.Common;
using AspNetWebApiCore.DataRepository;
using AspNetWebApiCore.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Settings.Configuration;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()     
     .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{

    Log.Information("Starting My WEB API Core application");
    var builder = WebApplication.CreateBuilder(args);   // Initializes a new instance of the WebApplicationBuilder class with preconfigured defaults.

    // ======== Add services to the container. =====
   
    // Setup Serilog BEFORE building the app
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()       
        .CreateLogger();
    builder.Host.UseSerilog(); // Use Serilog for logging 
    builder.Services.AddControllers() //Add controllers Services to Services Collection
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = null; //Disable: ASP.NET Core Web API uses the System.Text.Json serializer (by default) uses camelCase naming for JSON property names.
        });  

    //========JWT Token ==========
    // Load configuration
    var array_Audiences = builder.Configuration.GetSection("JwtSettings:ValidAudiences").Get<string[]>();
    GlobalAppSettings.Issuer = builder.Configuration["JwtSettings:Issuer"]; 
    GlobalAppSettings.ApiSecretKey = builder.Configuration["JwtSettings:JwtKey"];  
    var SigningKey = Encoding.UTF8.GetBytes(GlobalAppSettings.ApiSecretKey);  
    var jwtUsersSection = builder.Configuration.GetSection("JwtUsers");  //  Get the raw section from User Roles configuration   
    var jwtUserChildren = jwtUsersSection.GetChildren().ToList(); // IConfigurationSection list: Get the children (each user config as a key-value object)
    
    // Convert each IConfigurationSection to UserAccount
    var userConfigs = jwtUserChildren
        .Select(child => child.Get<UserAccount>()!)
        .ToList();

    var _roles = new JsonRoles(userConfigs);
    builder.Services.AddSingleton<IRoles>(_roles); //Registers a singleton service (one instance for the whole app lifetime)
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                //  issuer 
                ValidateIssuer = true,
                ValidIssuer = GlobalAppSettings.Issuer,
                // -- audience 
                ValidateAudience = true,
                ValidAudiences = array_Audiences, //userConfigs.SelectMany(u => u.Audiences).Distinct(),
                ValidateIssuerSigningKey = true, 
                IssuerSigningKey = new SymmetricSecurityKey(SigningKey), // can supply an array of ALL signing keys of differect Roles
                ValidateLifetime = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Headers["Authorization"];
                    accessToken = accessToken.ToString().Replace("Bearer ", "");                   
                    Log.Information("OnMessageReceived: Request: " + context.Request.Path.ToString() );                  
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        //TODO: We can do any acton and can send custom response on invalid token                        
                    }
                    Log.Information("OnAuthenticationFailed: Request: " + context.Request.Path.ToString() + ", Exception: " + context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Token was valid - you can access claims

                    var userPrincipal = context.Principal;                    
                    // Accessing specific claims
                    var client_key_Guid = userPrincipal?.FindFirst(ClaimTypes.Name)?.Value;
                    var client_Id = userPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;                    
                    var role = userPrincipal?.FindFirst(ClaimTypes.Role)?.Value;
                    var aud = userPrincipal?.FindFirst("aud")?.Value;
                    string UserName = userPrincipal?.FindFirst(JwtRegisteredClaimNames.Name).Value;
                    Log.Information("OnTokenValidated: [AUTH SUCCESS] Client_id: " + client_Id + ", Role: " + role + ", audience: " + aud + ", client_key: " + client_key_Guid + ", user_name: " + UserName);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // Runs when the token is missing or invalid and response is about to be sent
                    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();                 
                    Log.Information("OnChallenge: [AUTH CHALLENGE] Request: " + context.Request.Path.ToString() + ", Error: " + context.Error);
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();                   
                    var _user = context.HttpContext.User;
                    string client_key_Guid = _user.FindFirst(ClaimTypes.Name).Value;
                    string client_Id = _user.FindFirst(ClaimTypes.NameIdentifier).Value;                   
                    string role = _user.FindFirst(ClaimTypes.Role).Value;
                    var aud = _user.FindFirst("aud")?.Value;
                    string UserName = _user.FindFirst(JwtRegisteredClaimNames.Name).Value;   
                    Log.Information("[OnForbidden] Token does not have required role or permission: Client_id: " + client_Id + ", Role: " + role + ", audience: " + aud + ", client_key: " + client_key_Guid + ", user_name: " + UserName);                  
                    return Task.CompletedTask;
                }

            }; 
        });

    //====== Add Authorization Schemes for extra security layer:
    // Add One Policy Per Audience
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("PatientAudience", policy =>
           policy.RequireAssertion(context =>
           {
               var aud = context.User.FindFirst("aud")?.Value;
               return aud == "patient-aud";
           }));

        options.AddPolicy("HealthCareUnitAudience", policy =>
            policy.RequireAssertion(context =>
            {
                var aud = context.User.FindFirst("aud")?.Value;
                return aud == "HealthCareUnit-aud";
            }));

        options.AddPolicy("TeleCommAudience", policy =>
            policy.RequireAssertion(context =>
            {
                var aud = context.User.FindFirst("aud")?.Value;
                return aud == "TeleComm-aud";
            }));

        options.AddPolicy("SharedAudience", policy =>
           policy.RequireAssertion(context =>
           {
               var aud = context.User.FindFirst("aud")?.Value;
               return aud.Contains("patient-aud") || aud.Contains("HealthCareUnit-aud") || aud.Contains("TeleComm-aud");

           }));
    });
    // Register API versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);          // Set default version to 1.0
        options.AssumeDefaultVersionWhenUnspecified = true;        // Fallback if no version is specified
        options.ReportApiVersions = true;                          // Include version headers in response
        options.ApiVersionReader = ApiVersionReader.Combine(
            new HeaderApiVersionReader("x-api-version"),           // Read from custom header
            new QueryStringApiVersionReader("api-version")         // Or from query string
        );
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // e.g., v1, v2
         options.SubstituteApiVersionInUrl = true;
      });  

    // Add Swagger generator
    builder.Services.AddEndpointsApiExplorer();  
    builder.Services.AddSwaggerGen();
     builder.Services.ConfigureOptions<SwaggerConfigs>();   
    
    // Adding CORS (Cross-Origin Resource Sharing) policy to allow requests from the frontend application(Browsers).
    // NOTE: Replace http://localhost:3000 with your actual frontend application.
    // 
    var array_url = builder.Configuration.GetSection("API_CORS_SETTING:URL").Get<string[]>();  //var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy => policy.WithOrigins(array_url) //policy => policy.AllowAnyOrigin()  // policy => policy.WithOrigins(url) 
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials());                             
    });

    //******
    builder.Services.AddHttpContextAccessor(); // To Read HttpContext in Non-Controller Classes(e.g.,to Read HttpContext.User in Based controller)
    builder.Services.AddSingleton<IConfiguration>(builder.Configuration); // single instance per app-wide,used to Read appsettings.json in BaseRepository


    // Lifetime: Scoped services are created once per request. In other words, a new instance of the service is created for each HTTP request but is shared across all components (controllers, services, etc.).
    // And AddTransient services are created every time they are requested.
    
    builder.Services.AddScoped<PatientRepository>();
    builder.Services.AddScoped<ErrorLogRepository>();
    builder.Services.AddScoped<HealthCareRepository>();

    //NOTE:  Don't add services after this line
    var app = builder.Build();
    app.UseCors("AllowFrontend");
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
  
    //if (app.Environment.IsDevelopment())
    //{
        //app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"API {description.GroupName.ToUpperInvariant()}");
            }
        });       
        // Enable PII logging
        //IdentityModelEventSource.ShowPII = true;
    //}   
    app.UseMiddleware<ExceptionMiddleware>();  // Use Global custom middleware for exception handling
    // Log Request Time: i.e.,  Add Middleware Logging in ASP.NET Core
    app.Use(async (context, next) =>
    {
        var sw = Stopwatch.StartNew();
        await next.Invoke();
        sw.Stop();
        app.Logger.LogInformation($"Request took {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Request took {sw.ElapsedMilliseconds} ms");
    });

    //=== Enable middleware===
    app.UseHttpsRedirection();      // Redirect HTTP to HTTPS
    app.UseAuthentication();        // Check JWT token
    app.UseAuthorization();         // Apply [Authorize] rules
    app.MapControllers();           // Route to Controller endpoints

    app.Logger.LogInformation("Starting the app:");   
    app.Run();
    Log.Information("Application stopped cleanly.");  
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
   await Log.CloseAndFlushAsync();
}
