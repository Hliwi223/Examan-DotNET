using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TournoisE_Sports.Data;
using TournoisE_Sports.Data.Seed;
using TournoisE_Sports.Repositories.Implementations;
using TournoisE_Sports.Repositories.Interfaces;
using TournoisE_Sports.Services;

namespace TournoisE_Sports;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Connexion ---
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // --- Identity ---
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // --- JWT ---
        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key manquante (appsettings ou user-secrets).");
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EsportApi";
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EsportClient";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        builder.Services.AddAuthorization();

        // --- CORS (Angular) ---
        builder.Services.AddCors(options =>
            options.AddPolicy("AngularClient", policy =>
                policy.WithOrigins("http://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()));

        // --- Repositories & Services ---
        builder.Services.AddScoped<IJeuRepository, JeuRepository>();
        builder.Services.AddScoped<IEquipeRepository, EquipeRepository>();
        builder.Services.AddScoped<IJoueurRepository, JoueurRepository>();
        builder.Services.AddScoped<ITournoiRepository, TournoiRepository>();
        builder.Services.AddScoped<IInscriptionRepository, InscriptionRepository>();
        builder.Services.AddScoped<IMatchRepository, MatchRepository>();
        builder.Services.AddSingleton<IJwtService, JwtService>();
        builder.Services.AddScoped<IRankingService, RankingService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // --- Swagger avec Authorize ---
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tournois E-Sport API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Collez votre token : Bearer <token>"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[0]
                }
            });
        });

        var app = builder.Build();

        // --- Seed ---
        await DataSeeder.SeedAsync(app.Services, app.Configuration, app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder"));

        // --- Pipeline ---
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AngularClient");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
