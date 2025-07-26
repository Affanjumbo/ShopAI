using Microsoft.EntityFrameworkCore;
using ShopAI.Data;
using ShopAI.Services;
using System.Diagnostics;
using ShopAI.Models;
using Microsoft.Extensions.Options;

namespace ShopAI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            // Add services to the container.
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // This will use the property names as defined in the C# model
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure EF Core with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EFCoreDBConnection")));

            

            // Registering services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<AddressService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<ShoppingCartService>();
            builder.Services.AddScoped<OrderService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<CancellationService>();
            builder.Services.AddHostedService<PendingPaymentService>();
            builder.Services.AddScoped<RefundService>();
            builder.Services.AddHostedService<RefundProcessingBackgroundService>();
            builder.Services.AddScoped<FeedbackService>();
            builder.Services.AddScoped<SellerService>();
            builder.Services.AddScoped<SellerAddressService>();
            builder.Services.AddScoped<CustomerForgetPasswordService>();
            builder.Services.AddScoped<ShopDetailsService>();
            builder.Services.AddScoped<SellerForgetPasswordService>();
            builder.Services.AddScoped<BankDetailsService>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<SellerProfileService>();
            builder.WebHost.UseWebRoot("wwwroot");
            builder.Services.AddLogging();
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Services.AddScoped<SellerDashboardService>();
            builder.Services.AddScoped<CheckoutService>();









            //----------------------------------------------------------------------------------\\
            //AI
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "start_ai_services.py",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start AI services: {ex.Message}");
            }

            var app = builder.Build();





            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger(); // No need to pass any options here

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }


            app.UseHttpsRedirection();   
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }
}