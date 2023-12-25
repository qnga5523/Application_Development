using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GCH211211.Models;

namespace GCH211211.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GCH211211.Models.Category> Category { get; set; } = default!;
        public DbSet<GCH211211.Models.Product> Product { get; set; } = default!;
        public DbSet<GCH211211.Models.Order> Order { get; set; } = default!;
    }
}
