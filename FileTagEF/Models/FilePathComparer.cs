using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF.Models {
    public class FilePathComparer : IEqualityComparer<FilePath> {
        bool IEqualityComparer<FilePath>.Equals(FilePath? x, FilePath? y) {
            if (x == y)
                return true;
            if (x == null || y == null)
                return false;
            return x.Id == y.Id;
        }

        int IEqualityComparer<FilePath>.GetHashCode(FilePath obj) {
            return obj.Id.GetHashCode();
        }
    }
}
