using Microsoft.EntityFrameworkCore;
using PRProject.Api.Hubs;
using PRProject.Api.Services;
using PRProject.Bll.Interfaces;
using PRProject.Bll.Services;
using PRProject.Common.Configuration;
using PRProject.Dal.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()
    ));

builder.Services.Configure<MailSettings>(
    builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ITaskRealtimeNotifier, SignalRTaskRealtimeNotifier>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapHub<TaskHub>("/taskHub");

app.Run();