using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ShopAI.Models;

namespace ShopAI.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor accepting DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Configuring model properties and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // Order -> BillingAddress 
            modelBuilder.Entity<Order>()
                .HasOne(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Order -> ShippingAddress 
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete

            // Order -> Cancellation 
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cancellation)
                .WithOne(c => c.Order)
                .HasForeignKey<Cancellation>(c => c.OrderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid multiple paths

            modelBuilder.Entity<Feedback>()
                .Property(f => f.Rating)
                .HasPrecision(2, 1);

            // Payment -> Refund 
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Refund)
                .WithOne(r => r.Payment)
                .HasForeignKey<Refund>(r => r.PaymentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid multiple paths

            // Feedback -> Customer (Many-to-One)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Feedback -> Product (Many-to-One)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Product)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);






            // SubCategories -> Category (Many-to-One)
            modelBuilder.Entity<SubCategories>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete for subcategories under a category

            // Initial Seed Data


            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    IsActive = true
                },
                new Category
                {
                    Id = 2,
                    Name = "Books",
                    Description = "Books and magazines",
                    IsActive = true
                },
                new Category
                {
                    Id = 16,
                    Name = "Groceries",
                    Description = "Dailylife using products",
                    IsActive = true
                }

            );

            
            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Smartphone",
                    Description = "Latest model smartphone with advanced features.",
                    Price = 699.99m,
                    StockQuantity = 50,
                    DiscountPercentage = 10,
                    CategoryId = 1,
                    IsAvailable = true,
                    ProductImage = "/images/products/smartphone.jpeg"
                },
                new Product
                {
                    Id = 2,
                    Name = "Laptop",
                    Description = "High-performance laptop suitable for all your needs.",
                    Price = 999.99m,
                    StockQuantity = 30,
                    DiscountPercentage = 15,
                    CategoryId = 1,
                    IsAvailable = true,
                    ProductImage = "/images/products/laptop.jpeg"
                },
                new Product
                {
                    Id = 3,
                    Name = "Apple",
                    Description = "Fresh red apples.",
                    Price = 3.99m,
                    StockQuantity = 200,
                    DiscountPercentage = 5,
                    CategoryId = 16,
                    IsAvailable = true,
                    ProductImage = "/images/products/apple.jpeg"
                }
            );
        }

        // DbSets representing tables in the database
        public DbSet<Customer> Customers { get; set; }
        public DbSet<SellerAddress> SellerAddresses { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SubCategories> SubCategories { get; set; } 
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<BankDetails> BankDetails { get; set; }
        public DbSet<ShopDetails> ShopDetails { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MonthlySellerRank> MonthlySellerRanks { get; private set; }
    }
}
