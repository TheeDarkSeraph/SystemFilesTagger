using FileTagDB.Controllers;
using FileTagDB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Diagnostics;

namespace TagDatabaseTester {
    [Collection("Sequential")]
    public class FileTests {
        // TODO: add the ability to navigate to file's parent
        // TODO: Note I have considered not adding references to child files and such, and leaving it to the system.
        //          but I can't fully support the export/import fix links without it. User would have to redo the
        //          files and folders and the inner files and folders.

        // TODO: the + should translated to  (A OR B)
        List<string> sampleFiles;
        FileController fc;
        

        /*
         * 
         * https://www.techonthenet.com/sqlite/and_or.php#:~:text=The%20SQLite%20AND%20condition%20and,operations%20in%20Math%20class!)
         * SELECT p.*
         * FROM Pizza p
         * JOIN PT pt ON p.ID = pt.PizzaID
         * JOIN Toppings t ON pt.ToppingID = t.ID
         * WHERE t.name LIKE 't%'
         *  AND (t.name = 'cheese' OR t.name = 'sauce');
         * 
         * Another approach
         * I think one long comparison is better than multiple runs of small comparisons... Disk scanning wise
         * 
         * SELECT p.*
         * FROM Pizza p
         * JOIN PT pt ON p.ID = pt.PizzaID
         * WHERE pt.ToppingID IN (SELECT ID FROM Toppings WHERE name LIKE 't%')
         *      AND pt.ToppingID IN (SELECT ID FROM Toppings WHERE name IN ('cheese', 'sauce'));
         * 
         * 
         * 
         * 
         * 
         * 
         */


        #region Prints and Helper functions

        private readonly ITestOutputHelper output;
        private readonly DBLocationManager lm;
        public FileTests(ITestOutputHelper output) {
            this.output = output;
            lm = DBLocationManager.Instance;
            Utils.SetPrinter(output.WriteLine);
            lm.DBName = "testTags.db";
            DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
            DataHolder.PopulateFiles();
            sampleFiles = DataHolder.sampleFilePaths;
            fc = new FileController();
        }
        internal void CreateTestDB() {
            DBController.CreateDBIfNotExist(lm.DBLocation, lm.DBName);
            Assert.True(File.Exists(Path.Combine(lm.DBLocation, lm.DBName)));
        }
        internal void DeleteTestDB() {
            DBController.DeleteDB(lm.DBLocation, lm.DBName);
            Assert.True(!File.Exists(Path.Combine(lm.DBLocation, lm.DBName)));
        }
        internal void CreateEmptyTestDBWithTables() {
            CreateTestDB();
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                foreach (string name in TableConst.allTables)
                    using (var cmd = new SQLiteCommand(conn)) {
                        cmd.CommandText = $"select * from {name}";
                        var result = cmd.ExecuteReader();
                        Assert.Equal(2, result.FieldCount);
                        cmd.Dispose(); // this is important to instantly unlock...
                    }
                conn.Close();
            }
        }
        internal void CleanupTables() {
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                DBController.DeleteTablesData(conn);
                conn.Close();
            }
        }
        #endregion


        [Fact]
        public void GetRootPaths() {
            string testpath;

            testpath = @"C:\Windpws";
            Assert.Equal(@"C:\",fc.GetRootPath(testpath));
            testpath =@"C:\";
            Assert.Null(fc.GetRootPath(testpath));

            testpath = @"C:";
            Assert.Null(fc.GetRootPath(testpath));

            testpath = @"hello.txt";
            Assert.Equal("", fc.GetRootPath(testpath));

            testpath = @"test\hello.txt";
            Assert.Equal("", fc.GetRootPath(testpath));

            testpath = @"H:";
            Assert.Null(fc.GetRootPath(testpath));
            
            testpath = @"H:\";
            Assert.Null(fc.GetRootPath(testpath));

            testpath = @"H:\MusicProj+Song";
            Assert.Equal(@"H:\", fc.GetRootPath(testpath));
            testpath = @"H:\MusicProj+Song\";
            Assert.Equal(@"H:\", fc.GetRootPath(testpath));

            testpath = @"H:\MusicProj+Song\GuitarPro\GP2";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", fc.GetRootPath(testpath));
            
            testpath = @"H:\MusicProj+Song\GuitarPro\GP2\";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", fc.GetRootPath(testpath));
        }

        //[Fact]
        //public void printall() {
        //    foreach(string str in sampleFiles) {
        //        output.WriteLine(str);
        //    }
        //}

        [Fact]
        public void CreateFile() {

        }


        [Fact]
        public void CreateFilesThenChildFilesWhichWillBeRelated() {



        }
        [Fact]
        public void CreateFilesThenParentFilesWhichWillBeRelated() {

        }

        // no tags here, just file validity (does it still exists) and parent child links check to see if broken, and possible recursive repair
        // TODO: make sure what file paths are (we dont want folders to end with '\' or '/'
        /*
         * Insert multiple transactions
         * using (var transaction = connection.BeginTransaction())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO data
                    VALUES ($value)
                ";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "$value";
                command.Parameters.Add(parameter);

                // Insert a lot of data
                var random = new Random();
                for (var i = 0; i < 150_000; i++)
                {
                    parameter.Value = random.Next();
                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
         * 
         * 
         */

        [Fact]
        public void FixFileAndChildrenPath() {
            // Add all files, then fix a specific file
            //  fail when new path does not exist, succeed when file exists, succeed in searching for and adding child files


        }
        
        // fix broken links (user input changing folder to folder)

        // tag


        // map expression to AND, OR, NOT AND
        // like a AND b //// % and \% for actual remainders...
        // TODO: We need to know what like even means... https://www.sqlitetutorial.net/sqlite-like/  https://www.tutorialspoint.com/sqlite/sqlite_and_or_clauses.htm

        // TODO: Remember that what we need to use ID to search for tagged files (tag relation)
        //      do we union and search?

        // TODO: We will not use -d+e because it is simply -d -e
        // query tag regex a* aa b+c -d -d+e is like -d -e?, what does -d+e even mean?
        // so like a% AND has aa AND has (b or C) and NOT d AND NOT (d or e) = not d and not e so basically -d -e


    }
}
