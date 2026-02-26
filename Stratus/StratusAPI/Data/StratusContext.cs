using Microsoft.EntityFrameworkCore;

namespace StratusAPI.Data
{
    public class StratusContext : DbContext
    {
        public StratusContext(DbContextOptions<StratusContext> options) : base(options) { }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.RefreshToken> RefreshTokens { get; set; }
        public DbSet<Models.FileModel> Files { get; set; }
        public DbSet<Models.UserFile> UserFiles { get; set; }
        public DbSet<Models.ShareRequest> ShareRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure ShareRequest relationships to avoid cascade conflicts
            modelBuilder.Entity<Models.ShareRequest>()
                .HasOne(sr => sr.Sender)
                .WithMany()
                .HasForeignKey(sr => sr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Models.ShareRequest>()
                .HasOne(sr => sr.Receiver)
                .WithMany()
                .HasForeignKey(sr => sr.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Models.ShareRequest>()
                .HasOne(sr => sr.File)
                .WithMany()
                .HasForeignKey(sr => sr.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserFile relationships
            modelBuilder.Entity<Models.UserFile>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Models.UserFile>()
                .HasOne(uf => uf.File)
                .WithMany()
                .HasForeignKey(uf => uf.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
