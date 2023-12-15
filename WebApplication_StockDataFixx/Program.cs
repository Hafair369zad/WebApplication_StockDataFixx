using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<PMIDbContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("DefaultConnection")));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Ukuran maksimum 100 MB (dalam byte)
});
builder.Services.AddControllers()
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; // Mengabaikan nilai null
            options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore; // Mengabaikan nilai default
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; // Mengabaikan loop referensi
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };  
        });


builder.Services.AddSession();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(
    //new SessionOptions
//{
//    IdleTimeout = TimeSpan.FromMinutes(30)
//}
);

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

