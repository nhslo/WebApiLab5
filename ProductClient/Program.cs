using ProductClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
var apiAddress = builder.Configuration["ProductApi:BaseAddress"]
    ?? throw new InvalidOperationException("ProductApi:BaseAddress is missing from configuration.");
builder.Services.AddHttpClient("ProductApi", client =>
{
    client.BaseAddress = new Uri(apiAddress);
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IProductApiService, ProductApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
