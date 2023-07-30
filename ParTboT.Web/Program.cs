using Dodo.BlazorThemeSwitcher;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MongoDB.Driver;
using ParTboT.Web.Data;
using YarinGeorge.Databases.MongoDB;
using YarinGeorge.Databases.MongoDB.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
//using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddThemeSwitcher(new List<string>() { "light", "dark" });
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddDiscord(discord =>
    {
        discord.ClientId = "805864960776208404";
        discord.ClientSecret = "-lp5B7VnbBTJu3jUIpKZRDnuAk0vOCFq";
        discord.SaveTokens = true;
        discord.CallbackPath = "/counter";

        discord.Scope.Add("identify");
        discord.Scope.Add("email");
        discord.Scope.Add("connections");
        discord.Scope.Add("guilds");

    });
builder.Services.AddHttpContextAccessor();
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
app.UseAuthentication();
app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
