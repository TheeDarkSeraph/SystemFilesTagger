using FileTagEF.Models;
using Microsoft.EntityFrameworkCore;

namespace FileTagEF {
    public class TagDBContext: DbContext{
        public TagDBContext(DbContextOptions<TagDBContext> opt) : base(opt) { }
        public DbSet<FilePath> FilePaths { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique(true);
            modelBuilder.Entity<FilePath>().HasIndex(f => f.Fullpath).IsUnique(true);

            //modelBuilder.Entity<Tag>().HasMany(e => e.TaggedFiles).WithMany(e => e.FileTags);
            //modelBuilder.Entity<Filepath>().HasMany(e => e.InnerFiles).WithOne(e => e.ParentFolder)
            //    .HasForeignKey(e => e.ParentFolderId).IsRequired(false);
        }
    }
}
