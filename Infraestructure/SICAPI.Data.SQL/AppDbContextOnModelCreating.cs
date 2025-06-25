using Microsoft.EntityFrameworkCore;

namespace SICAPI.Data.SQL;

public partial class AppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
