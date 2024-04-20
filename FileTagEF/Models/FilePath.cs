using System.ComponentModel.DataAnnotations;

namespace FileTagEF.Models {
    public class FilePath {
        [Key]
        public int Id { get; set; }
        public string Fullpath { get; set; } = null!;

        public int? ParentFolderId;
        public FilePath? ParentFolder { get; set; }
        public ICollection<Tag> FileTags { get; set; } = null!;
        public ICollection<FilePath> InnerFiles { get; set; } = null!;
    }
}
