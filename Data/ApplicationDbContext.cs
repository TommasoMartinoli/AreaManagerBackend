using Microsoft.EntityFrameworkCore;
using ADLoginAPI.Models;
using System;

namespace ADLoginAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<PAI_EmployeesActivities> PAI_EmployeesActivities { get; set; }
        public DbSet<PAI_EmployeesActivitiesPrivateNotes> PAI_EmployeesActivitiesPrivateNotes { get; set; }
        public DbSet<PAI_EmployeesActivitiesRowsFinal> PAI_EmployeesActivitiesRowsFinal { get; set; }
        public DbSet<PAI_JobActivityCodes> PAI_JobActivityCodes { get; set; }
        public DbSet<PAI_EmployeesMaster> PAI_EmployeesMaster { get; set; }
        public DbSet<PAI_TeamRolesFunction> PAI_TeamRolesFunction { get; set; }
        public DbSet<PAI_EmployeesMasterForRoles> PAI_EmployeesMasterForRoles { get; set; }
        public DbSet<PAI_EmployeesActivitiesPrivateNotes_Count> CountResults { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PAI_EmployeesActivities>()
                .HasKey(e => new { e.ActivityYear, e.Week, e.PICEmployee }); 

            modelBuilder.Entity<PAI_EmployeesActivitiesPrivateNotes>()
                .HasKey(e => new { e.ActivityYear, e.Week, e.PICEmployee });

            modelBuilder.Entity<PAI_EmployeesActivitiesRowsFinal>()
                .HasKey(e => new { e.ActivityYear, e.Week, e.PICEmployee });

            modelBuilder.Entity<PAI_JobActivityCodes>()
                .HasKey(e => new { e.TaskCode });

            modelBuilder.Entity<PAI_EmployeesMaster>()
                .HasKey(e => new { e.PICEmployee });

            modelBuilder.Entity<PAI_EmployeesMasterForRoles>().HasNoKey();

            modelBuilder.Entity<PAI_TeamRolesFunction>().HasNoKey();

            modelBuilder.Entity<PAI_EmployeesActivitiesPrivateNotes_Count>().HasNoKey();

        }
    }
}
