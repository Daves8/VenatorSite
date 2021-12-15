using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseWork.Models
{
    public class VenatorContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<AllRoles> AllRoles { get; set; }

        public VenatorContext(DbContextOptions<VenatorContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}