using Microsoft.EntityFrameworkCore;

namespace ProjectsDashboards.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<PaymentClaim> PaymentClaims { get; set; }
        public DbSet<VariationOrder> VariationOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedByUser)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PaymentClaim>()
                .HasOne(pc => pc.Project)
                .WithMany(p => p.PaymentClaims)
                .HasForeignKey(pc => pc.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VariationOrder>()
                .HasOne(vo => vo.Project)
                .WithMany(p => p.VariationOrders)
                .HasForeignKey(vo => vo.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed initial data
            modelBuilder.Entity<User>().HasData(
                new User { ID = 1, FullName = "Admin Owner", Email = "owner@example.com", PasswordHash = "owner123", Role = "Owner" },
                new User { ID = 2, FullName = "Accountant", Email = "accountant@example.com", PasswordHash = "acc123", Role = "Accountant" },
                new User { ID = 3, FullName = "Staff", Email = "staff@example.com", PasswordHash = "staff123", Role = "Staff" }
            );
        }
    }
}
