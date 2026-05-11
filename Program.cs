using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using System.Text.Json.Serialization;
using P4_Backend_Car_App.Interfaces;
using P4_Backend_Car_App.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=carapp.db"));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddScoped<ICloudinaryService>(x =>
    new CloudinaryService(
        builder.Configuration["Cloudinary:CloudinaryUrl"]
    )
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
