using System.Data.Entity;

namespace SingleTableApp.Models
{
    public class MainObjectContext : DbContext
    {
        public DbSet<MainObject> MainObjects { get; set; }
    }
}