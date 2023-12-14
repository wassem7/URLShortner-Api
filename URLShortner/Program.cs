using Microsoft.EntityFrameworkCore;
using URLShortner.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder
    .Services
    .AddDbContext<ApplicationDbContext>(
        options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnectionString"))
    );
builder.Services.AddControllers();

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
