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
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<ChildGuardian> ChildGuardians { get; set; }
        public DbSet<Invite> Invites { get; set; }
        public DbSet<ScheduleRequest> ScheduleRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // A kid can have several guardians, and siblings share one
            modelBuilder.Entity<ChildGuardian>().HasKey(l => new { l.ChildId, l.GuardianId });
            modelBuilder.Entity<Invite>().HasIndex(i => i.CodeHash);

            modelBuilder.Entity<ChildGuardian>().HasData(new ChildGuardian
            {
                ChildId = 1,
                GuardianId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            });

            modelBuilder.Entity<Guardian>().HasData(data: new Guardian
            {
                GuardianId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FullName = "MARk M",
                PhoneNumber = "1234567890",
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
                StartTime = new DateTime(2026, 3, 28, 16, 0, 0),
                EndTime = new DateTime(2026, 3, 28, 17, 0, 0),
                Description = "Weekly soccer practice",
                Location = "Community Park",
                ChildId = 1
            });


        }


    }

}
