using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FileTagEF.Models {
    public class Tag {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<FilePath> TaggedFiles { get; set; } = null!;
    }
}
