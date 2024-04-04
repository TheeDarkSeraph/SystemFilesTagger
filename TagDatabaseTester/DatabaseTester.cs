
using FileTagDB;
using FileTagDB.Controllers;
using System.Data.SQLite;
using System.Diagnostics;
using System.Reflection;
using Xunit.Abstractions;

namespace TagDatabaseTester {
    [Collection("Sequential")] // this stops the parallel problem with deleting DB file
    public class DatabaseTester {

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
    }
}