using Microsoft.EntityFrameworkCore;
using OBManagementAPI;
using OBManagementAPI.Models;
//using OBManagementAPI.Models;

var builder = WebApplication.CreateBuilder(args);

////// ✅ Database Connection
builder.Services.AddDbContext<ObmanagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ✅ CORS - Mobile + Flutter + PC sab allow
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ✅ VERY IMPORTANT
// Server localhost + network dono par listen karega
builder.WebHost.UseUrls(
    "http://localhost:5000",   // 👈 Browser auto open
    "http://0.0.0.0:5000"      // 👈 Mobile access
);
//app.Environment.IsDevelopment();
var app = builder.Build();


// ✅ Swagger enable
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ❌ HTTPS band (Flutter HTTP use kar raha hai)
// app.UseHttpsRedirection();


// ✅ Enable CORS

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();