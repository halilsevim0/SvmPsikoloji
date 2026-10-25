using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Models // Projendeki namespace adına göre değişebilir
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Mevcut tablolarımız
        public DbSet<Hasta> Hastalar { get; set; }
        public DbSet<Psikolog> Psikologlar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<IletisimTalep> IletisimTalepleri { get; set; }

        public DbSet<SeansNotu> SeansNotlari { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filters (Silinmiş kayıtları otomatik gizler)
            modelBuilder.Entity<Hasta>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<Randevu>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<SeansNotu>().HasQueryFilter(s => !s.IsDeleted);
        }
    }
}