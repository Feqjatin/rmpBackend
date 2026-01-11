using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rmpBackend.BackgroundJobs;
using rmpBackend.middleware;
using rmpBackend.Models;
using rmpBackend.Models.DTOs;
using rmpBackend.Services;
using rmpBackend.Services.Email;
using rmpBackend.Services.Evalution;
using rmpBackend.Services.Upload;


var builder = WebApplication.CreateBuilder(args);
 

builder.Services.AddControllers();
builder.Services.AddScoped<RankingService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var provider=builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(config.GetConnectionString("dbcs")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,  
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
builder.Services.AddHostedService<rmpBackend.BackgroundJobs.DbWorker>();
builder.Services.AddHostedService<InterviewReminderService>();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary")
    );
builder.Services.AddScoped<IApplicationEvaluationService, ApplicationEvaluationService>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")  
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

 if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<RoleClaimsMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
