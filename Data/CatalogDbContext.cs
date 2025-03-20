using Microsoft.EntityFrameworkCore;
using ADLoginAPI.Models;
using System;

namespace ADLoginAPI.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<area> area { get; set; }
        public DbSet<industry> industry { get; set; }
        public DbSet<package> package { get; set; }
        public DbSet<package_web> package_web { get; set; }
        public DbSet<mago_version> mago_version { get; set; }
        public DbSet<module_main> module_main { get; set; }
        public DbSet<module_description> module_description { get; set; }
        public DbSet<module_feature> module_feature { get; set; }
        public DbSet<mago_edition> mago_edition { get; set; }
        public DbSet<module_x_industry_cloud> module_x_industry_cloud { get; set; }
        public DbSet<module_x_package_mago> module_x_package_mago { get; set; }
        public DbSet<module_x_package_web> module_x_package_web { get; set; }
        public DbSet<dictionary_available> dictionary_available { get; set; }
        public DbSet<fiscal_localization> fiscal_localization { get; set; }
        public DbSet<paragraph> paragraph { get; set; }
        public DbSet<module_edition> module_edition { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<module_main>()
                .Property(m => m.mago_valid_date)
                .HasColumnType("date");  

            modelBuilder.Entity<module_main>()
                .Property(m => m.cloud_valid_date)
                .HasColumnType("date");

            modelBuilder.Entity<module_main>()
                .Property(m => m.web_valid_date)
                .HasColumnType("date");

            modelBuilder.Entity<area>()
                .HasKey(e => new { e.id });

            modelBuilder.Entity<industry>()
                .HasKey(e => new { e.id });

            modelBuilder.Entity<package>()
                .HasKey(e => new { e.id });
            
            modelBuilder.Entity<package_web>()
                .HasKey(e => new { e.id });

            modelBuilder.Entity<mago_version>()
                .HasKey(e => new { e.id });

            modelBuilder.Entity<module_main>()
                .HasOne(m => m.area)  
                .WithMany()           
                .HasForeignKey(m => m.app_area)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<module_main>()
                .HasMany(m => m.description_info)
                .WithOne()                         
                .HasForeignKey(d => d.module_id)   
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_main>()
                .HasMany(f => f.features)
                .WithOne()
                .HasForeignKey(f => f.module_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<mago_edition>()
                .HasKey(e => e.id); 

            modelBuilder.Entity<mago_edition>()
                .HasOne(e => e.version)
                .WithMany() 
                .HasForeignKey(e => e.version_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<paragraph>()
                 .HasKey(e => e.id);

            modelBuilder.Entity<paragraph>()
                .HasOne(e => e.version)
                .WithMany()
                .HasForeignKey(e => e.version_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<dictionary_available>()
                .HasKey(e => e.id);

            modelBuilder.Entity<fiscal_localization>()
                .HasKey(e => e.id);

            modelBuilder.Entity<module_x_package_mago>()
                 .HasKey(mm => new { mm.module_id, mm.package_id, mm.edition_id });

            modelBuilder.Entity<module_x_industry_cloud>()
                .HasKey(mi => new { mi.module_id, mi.industry_id, mi.edition_id });

            modelBuilder.Entity<module_x_package_web>()
                .HasKey(mpw => new { mpw.module_id, mpw.package_id, mpw.edition_id });

            modelBuilder.Entity<module_x_package_mago>()
                .HasOne(mm => mm.Module)
                .WithMany(m => m.module_packages_mago)
                .HasForeignKey(mm => mm.module_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_package_mago>()
                .HasOne(mm => mm.Package)
                .WithMany()
                .HasForeignKey(mm => mm.package_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_package_mago>()
                .HasOne(mm => mm.Edition)
                .WithMany()
                .HasForeignKey(mm => mm.edition_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_industry_cloud>()
                .HasOne(mi => mi.Module)
                .WithMany(m => m.module_industries_cloud)
                .HasForeignKey(mi => mi.module_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_industry_cloud>()
                .HasOne(mi => mi.Industry)
                .WithMany()
                .HasForeignKey(mi => mi.industry_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_industry_cloud>()
                .HasOne(mi => mi.Edition)
                .WithMany()
                .HasForeignKey(mi => mi.edition_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_package_web>()
                .HasOne(mpw => mpw.Module)
                .WithMany(m => m.module_packages_web)
                .HasForeignKey(mpw => mpw.module_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_package_web>()
                .HasOne(mpw => mpw.PackageWeb)
                .WithMany()
                .HasForeignKey(mpw => mpw.package_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_x_package_web>()
                .HasOne(mpw => mpw.Edition)
                .WithMany()
                .HasForeignKey(mpw => mpw.edition_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<module_edition>()
                .HasKey(me => new { me.edition_id, me.module_id });

            modelBuilder.Entity<module_edition>()
                .HasOne(me => me.Edition)
                .WithMany()
                .HasForeignKey(me => me.edition_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<module_edition>()
                .HasOne(me => me.Module)
                .WithMany(m => m.editions)
                .HasForeignKey(me => me.module_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
