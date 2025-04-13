// Data/DataContext.cs
using Microsoft.EntityFrameworkCore;
using YummiGoWebApi.Models;

namespace YummiGoWebApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        // YENİ DbSet
        public DbSet<RecipeLike> RecipeLikes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RecipeLike için Composite Primary Key
            modelBuilder.Entity<RecipeLike>()
                .HasKey(rl => new { rl.UserId, rl.RecipeId });

            // İsteğe bağlı: Cascade Delete tanımlamaları (Önceki gibi)
            modelBuilder.Entity<RecipeLike>()
                .HasOne(rl => rl.User)
                .WithMany()
                .HasForeignKey(rl => rl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecipeLike>()
                .HasOne(rl => rl.Recipe)
                .WithMany()
                .HasForeignKey(rl => rl.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}