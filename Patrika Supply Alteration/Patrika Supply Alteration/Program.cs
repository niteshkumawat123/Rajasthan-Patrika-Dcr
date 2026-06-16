using DCRSupplyApp.Services;
using DCRSupplyApp.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.MaxAge = TimeSpan.FromMinutes(60);
});
builder.Services.AddScoped<OracleDbService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<SessionAuthFilter>();
builder.Services.AddSingleton<FirebaseNotificationService>();
builder.Services.AddScoped<NotifyService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
