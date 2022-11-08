using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NhuQuynh.Models;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<NhuQuynh.Models.Student> Student { get; set; } = default!;

        public DbSet<NhuQuynh.Models.Employee> Employee { get; set; } = default!;

        public DbSet<NhuQuynh.Models.Person> Person { get; set; } = default!;

        public DbSet<NhuQuynh.Models.Customer> Customer { get; set; } = default!;
    }
