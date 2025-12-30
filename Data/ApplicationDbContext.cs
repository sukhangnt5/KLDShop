using Microsoft.EntityFrameworkCore;
using KLDShop.Models;

namespace KLDShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartSession> CartSessions { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to match SQL script
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail");
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<CartItem>().ToTable("CartItem");
            modelBuilder.Entity<CartSession>().ToTable("CartSession");
            modelBuilder.Entity<Wishlist>().ToTable("Wishlist");

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // User configuration
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
            
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product configuration
            modelBuilder.Entity<Product>()
                .HasKey(p => p.ProductId);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.OrderDetails)
                .WithOne(od => od.Product)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.CartItems)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order configuration
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderDetail configuration
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.OrderDetailId);

            // Payment configuration
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId);

            // CartItem configuration
            modelBuilder.Entity<CartItem>()
                .HasKey(c => c.CartItemId);

            // CartSession configuration
            modelBuilder.Entity<CartSession>()
                .HasKey(cs => cs.CartSessionId);

            modelBuilder.Entity<CartSession>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartSession>()
                .HasOne(cs => cs.Product)
                .WithMany()
                .HasForeignKey(cs => cs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Wishlist configuration
            modelBuilder.Entity<Wishlist>()
                .HasKey(w => w.WishlistId);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One user can have one product in wishlist only once
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();

            // Newsletter configuration
            modelBuilder.Entity<Newsletter>().ToTable("Newsletter");
            modelBuilder.Entity<Newsletter>()
                .HasKey(n => n.NewsletterId);
            modelBuilder.Entity<Newsletter>()
                .HasIndex(n => n.Email)
                .IsUnique();

            // EmailCampaign configuration
            modelBuilder.Entity<EmailCampaign>().ToTable("EmailCampaign");
            modelBuilder.Entity<EmailCampaign>()
                .HasKey(ec => ec.CampaignId);

            // ProductImage configuration
            modelBuilder.Entity<ProductImage>().ToTable("ProductImage");
            modelBuilder.Entity<ProductImage>()
                .HasKey(pi => pi.ProductImageId);
            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => new { pi.ProductId, pi.DisplayOrder });
        }
    }
}
