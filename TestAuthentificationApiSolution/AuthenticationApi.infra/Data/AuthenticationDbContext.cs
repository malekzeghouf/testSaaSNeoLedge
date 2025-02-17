using Microsoft.EntityFrameworkCore;
using AuthenticationApi.Domain.Entities; 

namespace AuthenticationApi.Infrastructure.Data
{


    public class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : DbContext (options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb; 
                                      Initial Catalog=testUser; 
                                      Integrated Security=True;");
        }

    }
}
