using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemFilesTagger {
    public class Consts {
        public const string fileExtensions = "File Extensions";
        public const string customTags = "Custom Tags";

        public const string regularFile = "File";
        public const string folderFile = "Folder";
        public const string unknownFile = "Unknown";
        public static readonly string[] nonUsableTags = new string[]{regularFile, folderFile, unknownFile };
        public static readonly string[] savableExtensions = new string[] { ".dll", ".txt", ".jpeg", ".png", ".jpg", ".gif", ".webp", ".ini", ".rar", ".zip", ".tar", ".7z", ".docx", ".pdf", ".csv", ".xlsx", ".xls" };

    }
}
