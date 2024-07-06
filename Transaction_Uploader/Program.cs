using Microsoft.EntityFrameworkCore;
using Transaction_Uploader.Data;
using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Repositories;
using Transaction_Uploader.Services;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TransactionContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<ITransaction, TransactionRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheKeyManager>();
builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<TransactionValidator>());
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
