using Microsoft.EntityFrameworkCore;
using Transaction_Uploader.Data;
using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Repositories;
using Transaction_Uploader.Services;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TransactionContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<ITransaction, TransactionRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheKeyManager>();
builder.Services.AddTransient<IFileProcessor, FileProcessor>();
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<TransactionValidator>());
builder.Services.AddControllersWithViews();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transaction}/{action=Transaction}/{id?}");

app.Run();
