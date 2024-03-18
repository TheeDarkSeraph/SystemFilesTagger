using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagDB.Models {
    public class Tag{
        public int id;
        public string name;
        public Tag(int p_id, string p_name) {
            id = p_id;
            name = p_name;
        }
    }
}
