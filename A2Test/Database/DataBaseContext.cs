using A2Test.Class;
using A2Test.Class.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2Test.Database
{
    internal class DataBaseContext : DbContext
    {        
        public DbSet<WoodBuy> WoodBuys { get; set; }
        public DbSet<WoodSell> WoodSells { get; set; }

        public DataBaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
        
    }
}
