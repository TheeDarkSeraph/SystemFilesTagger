using FileTagEF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF {
    public class TagDto {
        public int id;
        public string name;
        public TagDto(Tag tag) {
            id = tag.Id;
            name = tag.Name;
        }
    }
}
