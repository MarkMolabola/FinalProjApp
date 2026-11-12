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
        public DbSet<PhotoChild> PhotoChildren { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // A kid can have several guardians, and siblings share one
            modelBuilder.Entity<ChildGuardian>().HasKey(l => new { l.ChildId, l.GuardianId });
            modelBuilder.Entity<Invite>().Property(i => i.CodeHash).HasMaxLength(64);
            modelBuilder.Entity<Invite>().HasIndex(i => i.CodeHash);
            modelBuilder.Entity<AppUser>().Property(u => u.Email).HasMaxLength(256);
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<PhotoChild>().HasKey(l => new { l.PhotoId, l.ChildId });


        }


    }

}
