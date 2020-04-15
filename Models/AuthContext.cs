using Microsoft.EntityFrameworkCore;
using TodoApi.Models.ModelConfigurations;

public class AuthContext : DbContext
{
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.ApplyConfiguration(new AuthConfiguration());

}
