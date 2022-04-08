using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PiCast.Model
{
    public class EntityContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly string _dbFile;
        public DbSet<Configuration> Configurations { get; set; }

        public EntityContext(string dbFile = "Filename=data.db")
        {
            _dbFile = dbFile;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_dbFile);
        }
    }
}
