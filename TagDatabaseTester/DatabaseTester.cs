
using FileTagDB;
using FileTagDB.Controllers;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;

namespace TagDatabaseTester {
    [Collection("Sequential")] // this stops the parallel problem with deleting DB file
    public class DatabaseTester {

        // Dont forget to Add project reference

        // Add control folder for control?
        // TODO: Add test list
        #region Prints and Helper functions
        private readonly ITestOutputHelper output;
        private readonly DBLocationManager lm;
        public DatabaseTester(ITestOutputHelper output) {
            this.output = output;
            lm = DBLocationManager.Instance;
            Utils.SetPrinter(output.WriteLine);
            lm.DBName = "testTags.db";
            DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
        }
        #endregion

        
        [Fact]
        public void CreateTestDB() {
            DBController.CreateDBIfNotExist(lm.DBLocation, lm.DBName);
            Assert.True(File.Exists(Path.Combine(lm.DBLocation, lm.DBName)));
        }
        [Fact]
        public void DeleteTestDB() {
            DBController.DeleteDB(lm.DBLocation, lm.DBName);
            Assert.True(!File.Exists(Path.Combine(lm.DBLocation, lm.DBName)));
        }
        [Fact]
        public void CreateEmptyTestDBWithTables() {
            CreateTestDB();
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                foreach (string name in TableConst.allTables)
                    using (var cmd = new SQLiteCommand(conn)) {
                        output.WriteLine("Table testing " + name);
                        cmd.CommandText = $"select * from {name}";
                        var result = cmd.ExecuteReader();
                        output.WriteLine("col 1 " + result.GetName(0));
                        output.WriteLine("col 2 " + result.GetName(1));
                        Assert.Equal(2, result.FieldCount);
                        cmd.Dispose(); // this is important to instantly unlock...
                    }
            }
            // cleanup
            DeleteTestDB();
        }
        // we should have a default (#Untagged) tag, shows in a selection thats there on its own (if there are any)
        // User cannot define it as a tag because it is not actually a tag, it is a select all files without a tag associated to them


        // some tests adding data to tag,
        // some tests adding data to file
        
        // before next tests the above ensures bogus tests exist
        
        // some tests adding data to folders/files and auto associating children files and parent folders
        // deassociation file from child when links are broken OR file is removed (make sure)
        // some tests tagging files
        // some tests adding folder and its children (existing to so auto)
        // some tests adding folder and its children (non-existing forcefully)
        // searching for a file's root missing folder missing link
        // fixing a file's path

        /// The rest is in the next file

    }
    // TODO: if file does not exist, check if its parent exists in the file DB, if it does switch check to it
    //          until there is no more a parent. then this specific folder/file Is the missing link (ask to fix link/delete reference)
    //          
}