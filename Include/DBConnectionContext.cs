using Microsoft.EntityFrameworkCore;

namespace QnA.Include
{
    public class DBConnectionContext : DbContext
    {
        public DBConnectionContext(DbContextOptions<DBConnectionContext> options) : base(options) { }
    }
}