using Dodo.BlazorThemeSwitcher;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MongoDB.Driver;
using ParTboT.Web.Data;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Databases.MongoDB.Types;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddThemeSwitcher(new List<string>() { "light", "dark" });
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton(new MongoCRUD(new MongoCRUDConnectionOptions()
{
    Database = "TestingStreamersAgain",
    ConnectionString = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false"
}));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
