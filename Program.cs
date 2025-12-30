using KLDShop.Data;
using KLDShop.Services.PaymentGateway;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework Core
// Support Railway environment variable for database (PostgreSQL)
// Use PostgreSQL for both local development and Railway production
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrEmpty(databaseUrl)}");
Console.WriteLine($"DATABASE_URL starts with: {(string.IsNullOrEmpty(databaseUrl) ? "NULL" : databaseUrl.Substring(0, Math.Min(20, databaseUrl.Length)))}...");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway uses PostgreSQL with DATABASE_URL environment variable
    // Convert postgres:// or postgresql:// format to Npgsql format if needed
    var connectionString = databaseUrl;
    
    if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
    {
        try
        {
            var uri = new Uri(databaseUrl);
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
            Console.WriteLine("Converted DATABASE_URL format successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting DATABASE_URL: {ex.Message}");
        }
    }
    
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Local development uses PostgreSQL with connection string from appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Payment Gateway Services
builder.Services.AddScoped<IPaymentGateway, VNPayService>();
builder.Services.AddScoped<VNPayService>();
builder.Services.AddScoped<PayPalService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Add MailChimp Service
builder.Services.AddScoped<KLDShop.Services.MailChimpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Only redirect to HTTPS in development, Railway handles SSL at edge
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// Add Session Middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
