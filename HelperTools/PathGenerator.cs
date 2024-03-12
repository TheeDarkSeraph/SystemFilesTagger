using System.Diagnostics;
using System.Text;

namespace HelperTools {
    internal class PathGenerator {


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
            List<string> nT =
                [
                    "acting" , "action" , "action_based" , "action_rpg" , "action's" , "action\"s%_@!#$" ,
                    "active" , "adventure" , "animals" , "Apocalypse" , "beatemup" , "boss_based" , "easy" ,
                    "fight_based" , "fighting" , "flow" , "grind" , "hard" , "kids" , "platformer" , "roguelike" ,
                    "roguelite" , "roleplay" , "roleplay_adventure" , "roleplay_solo" , "sad" , "sandbox" ,
                    "singleplayer" , "solo" , "strategy" , "strategy_based" , "tactics" , "time" , "time_based" , 
                    "turn_based" , "zoo"
                ];
            nT.Sort();
            Console.WriteLine(string.Join("\" , \"",nT));
            if (true)
                return;
            string outputFile = "filepaths.txt";
            FileStream filestream = new FileStream(outputFile, FileMode.Create);
            var streamwriter = new StreamWriter(filestream, Encoding.UTF8);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);

            string[] rootFolders = { @"H:\MusicProj+Song\GuitarPro\GP2", @"H:\MusicProj+Song\Z Mixcraf done" };
            int referralDepth = 3;
            foreach (string folder in rootFolders)
                PrintParentFirstFileTree(folder, referralDepth);
            Console.Out.Close();
            //System.Diagnostics.Process.Start("notepad.exe",outputFile);
            Process.Start("explorer.exe", "/select," + outputFile);
        }
    }
}
