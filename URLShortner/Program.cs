using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using URLShortner.Data;
using URLShortner.Repository;
using URLShortner.Repository.UserRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddResponseCaching();
builder.Services.AddScoped<IUrlRepository, UrlRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder
    .Services
    .AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddSlidingWindowLimiter(
            "sliding",
            option =>
            {
                option.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                option.Window = TimeSpan.FromSeconds(60);
                option.PermitLimit = 3;
                option.SegmentsPerWindow = 30;
            }
        );
    });

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder
    .Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
;

builder
    .Services
    .AddDbContext<ApplicationDbContext>(
        options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnectionString"))
    );
builder.Services.AddControllers();
builder
    .Services
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme()
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. \r\n\r\n "
                    + "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n"
                    + "Example: \"Bearer 12345abcdef\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "Bearer"
            }
        );
        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            }
        );

        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Version = "v1.0",
                Title = "URL Shortner V1",
                Description = "3 requests every 30 seconds",
            }
        );
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter();
app.MapControllers();

app.MapFallback(
    async (HttpContext ctx, ApplicationDbContext db) =>
    {
        var path = ctx.Request.Path.ToUriComponent().Trim('/');

        var pathMatch = await db.Urls.FirstOrDefaultAsync(u => u.Shorturl.Trim() == path.Trim());

        if (pathMatch == null)
        {
            return Results.NotFound();
        }

        return Results.Redirect(pathMatch.LongUrl);
    }
);
app.Run();
