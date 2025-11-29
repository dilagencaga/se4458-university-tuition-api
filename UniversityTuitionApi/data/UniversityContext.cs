using Microsoft.EntityFrameworkCore;
using UniversityTuitionApi.Models;

namespace UniversityTuitionApi.Data
{
    public class UniversityContext : DbContext
    {
        public UniversityContext(DbContextOptions<UniversityContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<TuitionRecord> TuitionRecords => Set<TuitionRecord>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tablo isimlerini elle eşliyoruz
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<TuitionRecord>().ToTable("Tuitions");   // 🔥 ÖNEMLİ: TuitionRecord -> Tuitions tablosu
            modelBuilder.Entity<Payment>().ToTable("Payments");
        }
    }
}
