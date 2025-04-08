using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using PCMSApi.Data;
using PCMSApi.Endpoints;
using PCMSApi.Handlers;
using PCMSApi.Services;
using AutoMapper;
using PCMSApi.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "PCMS API", Version = "v1" });
    options.EnableAnnotations();
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Add JWT Authentication
var jwtSecret = "this_is_a_dev_key_pcms";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add PostgreSQL
builder.Services.AddDbContext<AppDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<Patients>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddSingleton<IS3Service, S3Service>();

// Register IAmazonS3 manually for LocalStack
builder.Services.AddSingleton<IAmazonS3>(_ =>
{
    var awsConfig = builder.Configuration.GetSection("AWS");
    var serviceUrl = awsConfig["ServiceURL"] ?? "http://localhost:4566";
    var config = new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = bool.TryParse(
            Environment.GetEnvironmentVariable("AWS_FORCE_PATH_STYLE")
            ?? awsConfig["ForcePathStyle"], out var forcePathStyle) && forcePathStyle,
        UseHttp = bool.TryParse(
            Environment.GetEnvironmentVariable("AWS_USE_HTTP")
            ?? awsConfig["UseHttp"], out var useHttp) && useHttp
    };

    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
                    ?? builder.Configuration["AWS:AccessKey"];
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                    ?? builder.Configuration["AWS:SecretKey"];

    return new AmazonS3Client(accessKey, secretKey, config);
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

// Dev-only tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowUI");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapPatientEndpoints();

app.Run();