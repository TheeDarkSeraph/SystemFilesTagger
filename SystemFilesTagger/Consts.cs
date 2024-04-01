
namespace SystemFilesTagger {
    //".([^"]+)" , replace with : ($1, value), // $1 is the first match
    // https://learn.microsoft.com/en-us/visualstudio/ide/using-regular-expressions-in-visual-studio?view=vs-2022
    //
    public class Consts {
        public const string defaultTags = "Default Tags";
        public const string fileExtensions = "File Extensions";
        public const string customTags = "Custom Tags";

        public const string regularFile = "File";
        public const string folderFile = "Folder";
        public const string unknownFile = "Unknown";
        public static readonly string[] nonUsableTags = new string[]{regularFile, folderFile, unknownFile };
        
        // TODO: change this to a dictionary for optimallity?
        /// <summary>
        /// Some of the common extensions that share an icon in windows
        /// </summary>
        public static readonly HashSet<string> savableExtensions = new (){
            ".dat",  ".resx", ".sln", ".cs",  ".dll",  ".html", ".cpp", ".css", ".php", ".csproj",
            ".mp4",  ".avi",  ".wmv", ".mov", ".mkv",  ".flv",  ".webm", ".m4v", ".3gp", // videos
            ".md",   ".txt",  ".ini",                                       // text
            ".jpeg", ".png",  ".jpg", ".gif", ".webp", ".tiff", ".bmp",     // images
            ".mp2",  ".mp3",  ".ogg", ".aac", ".flac",                      // audio
            ".rar",  ".zip",  ".tar", ".7z",                                // compressions
            ".docx", ".pdf",  ".csv", ".xlsx",".xls"                        // doc related
        };

    }
}
