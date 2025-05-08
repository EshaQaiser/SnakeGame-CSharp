using Microsoft.EntityFrameworkCore;
using Snake.Model;

public class SnakeDbContext : DbContext
{
    public DbSet<Player> Players { get; set; }

    public SnakeDbContext(DbContextOptions<SnakeDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-BE80RDB\\SQLEXPRESS;Database=SnakeGameDB;Trusted_Connection=True;");
        }
    }
}
