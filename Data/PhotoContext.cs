using FinalProjApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalProjApp.Data
{
    public class PhotoContext: DbContext
    {
        public PhotoContext(DbContextOptions<PhotoContext> options) : base(options) { }
        public DbSet<Photo> Photos{ get; set; }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlite("Data Source=C:\\DATA\\Photos.db");
        //    }
        //}
    }
}
 