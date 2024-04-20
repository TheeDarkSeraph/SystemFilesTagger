using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace FileTagEF {
    public class TagDBContextFactory : IDesignTimeDbContextFactory<TagDBContext> {
        public TagDBContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<TagDBContext>();
            optionsBuilder.UseSqlite("Data Source=blog.db");

            return new TagDBContext(optionsBuilder.Options);
        }
    }
}
