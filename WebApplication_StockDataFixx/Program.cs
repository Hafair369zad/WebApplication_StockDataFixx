using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PMIDbContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));


builder.Services.AddSession();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(new SessionOptions
{
    IdleTimeout = TimeSpan.FromMinutes(30)
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "api_warehouse",
    pattern: "api/{controller=WarehouseApi}/{action=Get}/{id?}");

app.MapControllerRoute(
    name: "api_production",
    pattern: "api/{controller=ProductionApi}/{action=GetItems}/{id?}");

app.MapControllerRoute(
    name: "api_usertb",
    pattern: "api/{controller=AccountApi}/{action=GetItems}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PMIDbContext>();
    context.Database.Migrate();
}


app.Run();

