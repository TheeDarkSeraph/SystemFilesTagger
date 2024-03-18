using FileTagDB.Controllers;

namespace FileTagDB {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // The folder for the roaming current user 
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Combine the base folder with your specific folder....
            string specificFolder = Path.Combine(folder, "TagManager");
            // CreateDirectory will check if every folder in path exists and, if not, create them.
            // If all folders exist then CreateDirectory will do nothing.
            Directory.CreateDirectory(specificFolder);
            string dbName = "file_tagger.db";
            DBLocationManager lm = DBLocationManager.Instance;
            lm.DBLocation = specificFolder;
            lm.DBName = dbName;
            DBController.CreateDBIfNotExist(specificFolder,dbName);
            Application.Run(new FileAndTagsManager());
        }


    }
}