namespace HelperTools {
    internal class Program {


        public static void PrintParentFirstFileTree(string path, int moreDepth) {
            Console.WriteLine(path);
            if (moreDepth == 0)
                return;
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory) {
                string[] files = Directory.GetFileSystemEntries(path);
                moreDepth--;
                foreach (string file in files) {
                    PrintParentFirstFileTree(file, moreDepth);
                }
            }
        }

        static void Main(string[] args) {
            string outputFile = "filepaths.txt";
            FileStream filestream = new FileStream(outputFile, FileMode.Create);
            var streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);

            string[] rootFolders = { @"H:\MusicProj+Song\GuitarPro\GP2", @"H:\MusicProj+Song\Z Mixcraf done" };
            int referralDepth = 3;
            foreach (string folder in rootFolders)
                PrintParentFirstFileTree(folder, referralDepth);
            Console.Out.Close();
            System.Diagnostics.Process.Start("notepad.exe",outputFile);
        }
    }
}
