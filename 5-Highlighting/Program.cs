using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using search.Shared;
using BlazorStrap;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorStrap();

// Setup configuration
var cfgBuilder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
var config = cfgBuilder.Build();

// Search engine setup
var randValue = config["BogusConfig:Rand"];
var waffleCountValue = config["BogusConfig:WaffleCount"];
if (randValue != null && waffleCountValue != null)
{
    SearchEngine.GetData(Int32.Parse(randValue), Int32.Parse(waffleCountValue));
}
SearchEngine.Index();

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
