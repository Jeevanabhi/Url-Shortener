using Microsoft.EntityFrameworkCore;

public class UrlShorten : DbContext
{
    public UrlShorten(DbContextOptions<UrlShorten> options)
    :base(options)//passing options(database configurations to dbcontext class)
    {
    }
    public DbSet<Click> Clicks { get; set; }
    
}