using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Data

{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Child> children { get; set; }
        public DbSet<Guardian> guardian { get; set; }
        public DbSet<ScheduleEvent> scheduleEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=C:\\DATA\\DaycareSystem.db");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guardian>().HasData(data: new Guardian
            {
                GuardianId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "MARk M",
                PhoneNumber = 1234567890,
                Address = "123 Main"
                });

            modelBuilder.Entity<Child>().HasData(data: new Child
            {
                ChildId = 1,
                FirstName = "John",
                LastName = "Doe",
                DateofBirth = new DateTime(2015, 5, 15),
                GuardianID = Guid.Parse("11111111-1111-1111-1111-111111111111")
            });
            modelBuilder.Entity<ScheduleEvent>().HasData(data: new ScheduleEvent
            {
                Id = 1,
                Subject = "Soccer Practice",
                StartTime = new DateTime(2025, 10, 9, 16, 0, 0),
                EndTime = new DateTime(2025, 10, 9, 17, 0, 0),
                Description = "Weekly soccer practice",
                Location = "Community Park",
                ChildId = 1
            });


        }


    }

}
