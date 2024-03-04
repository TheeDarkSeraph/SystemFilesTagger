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
using System.Reflection;

namespace TagDatabaseTester {
    [Collection("Sequential")]
    public class FileTests {
        // NOTE: All tests are customized to files listed in filepaths.txt main developer (Salah Elabyad)


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
            Assert.Equal(@"C:\",fc.GetParentPath(testpath));
            testpath =@"C:\";
            Assert.Null(fc.GetParentPath(testpath));

            testpath = @"C:";
            Assert.Null(fc.GetParentPath(testpath));

            testpath = @"hello.txt";
            Assert.Equal("", fc.GetParentPath(testpath));

            testpath = @"test\hello.txt";
            Assert.Equal("", fc.GetParentPath(testpath));

            testpath = @"H:";
            Assert.Null(fc.GetParentPath(testpath));
            
            testpath = @"H:\";
            Assert.Null(fc.GetParentPath(testpath));

            testpath = @"H:\MusicProj+Song";
            Assert.Equal(@"H:\", fc.GetParentPath(testpath));
            testpath = @"H:\MusicProj+Song\";
            Assert.Equal(@"H:\", fc.GetParentPath(testpath));

            testpath = @"H:\MusicProj+Song\GuitarPro\GP2";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", fc.GetParentPath(testpath));
            
            testpath = @"H:\MusicProj+Song\GuitarPro\GP2\";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", fc.GetParentPath(testpath));
        }

        //[Fact]
        //public void printall() {
        //    foreach(string str in sampleFiles) {
        //        output.WriteLine(str);
        //    }
        //}
        
        [Fact]
        public void ShouldGetNonExistingFileParent() {
            Assert.Equal("H:\\",fc.GetParentPath("H:\\MusicProj+Sg22"));
        }
        [Fact]
        public void CreateFile() {
            CreateEmptyTestDBWithTables();
            AddSingleFileAndVerify(0, 0);
            Assert.Equal(-1, fc.AddFile(sampleFiles[0]));
            CleanupTables();
        }

        private void AddSingleFileAndVerify(int count, int index) {
            int fileID = fc.AddFile(sampleFiles[index]);
            Assert.Equal(count + 1, fileID);
            Assert.Equal(fileID, fc.GetFileID(sampleFiles[index]));
            Assert.Equal(FileController.FixFilePath(sampleFiles[index]), fc.GetFilePath(fileID));
        }

        // It seems to be slow to add parent child relationship
        /*
         * For 6831 files it takes a lot more than 6 minutes (stopped)
         * Will output speed logs to
         * 
         */
        [Fact]
        public void CreateFilesAndOneConnection() {
            CreateEmptyTestDBWithTables();
            AddSingleFileAndVerify(0, 0);
            AddSingleFileAndVerify(1, 1);
            AddSingleFileAndVerify(2, 2);
            AddSingleFileAndVerify(3, 3);
            AddSingleFileAndVerify(4, 4);
            FixAllSampleFiles();
            VerifyParentRelation(0, 1); // 1,2
            VerifyParentRelation(2, 3); // 3,4
            CleanupTables();
        }
        [Fact]
        public void CreateFilesThenChildFilesWhichWillBeRelated() {
            CreateEmptyTestDBWithTables();
            for (int i = 0; i < 100; i++) {//sampleFiles.Count; i++) 
                //var watch = System.Diagnostics.Stopwatch.StartNew();
                // TODO: createa  bulk add and test
                AddSingleFileAndVerify(i, i);
                //watch.Stop();
                //var elapsedMs = watch.ElapsedMilliseconds;
                //Utils.LogToOutput(elapsedMs.ToString());
            }
            FixAllSampleFiles();
            VerifyCustomIndicies();
            CleanupTables();
        }
        private void FixAllSampleFiles() {
            for(int i = 0; i < sampleFiles.Count; i++)
                sampleFiles[i] = FileController.FixFilePath(sampleFiles[i]);
        }
        private void VerifyCustomIndicies() {
            VerifyParentRelation(0, 1); // 1,2
            VerifyParentRelation(2, 3); // 3,4, careful that 4 & 5 are one folder appart
            VerifyChildRelation(4, Enumerable.Range(5, 22-6+1).ToList()); // 5, [6 to 22] inclusive
        }
        private void VerifyParentRelation(int parentIndex,int childFileIndex) {
            Assert.Equal(fc.GetFileID(sampleFiles[parentIndex]),
                fc.RetrieveFileParentID(sampleFiles[childFileIndex]));
        }
        private void VerifyChildRelation(int parentIndex, List<int> childIndicies) {
            int parentID = fc.GetFileID(sampleFiles[parentIndex]);
            foreach(int cIndex in childIndicies)
                Assert.Equal(parentID, fc.RetrieveFileParentID(sampleFiles[cIndex]));
        }

        [Fact]
        public void CreateFilesThenParentFilesWhichWillBeRelated() { // for time
            CreateEmptyTestDBWithTables();
            int count = 0; //sampleFiles.Count-1
            for (int i = 100; i>-1 ; i--) {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                AddSingleFileAndVerify(count++, i);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Utils.LogToOutput(elapsedMs.ToString());
            }
            FixAllSampleFiles();
            CleanupTables();
        }



        [Fact]
        public void BulkAddFiles() {
            CreateEmptyTestDBWithTables();
            FixAllSampleFiles();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            fc.BulkAddFiles(sampleFiles);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Utils.LogToOutput(elapsedMs.ToString());

            // DO update first then next test


            //List<(string, int)> fileData = fc.RetrieveFileChildren(1); // 53
            //for (int i = 0; i < fileData.Count; i++) {
            //    Utils.LogToOutput(fileData[i].Item2 + " " + fileData[i].Item1);
            //}
            //VerifyCustomLargeSetOfIndicies_1();
            //CleanupTables();
        }
        public void BulkAddFilesInTheSameFolder() { // parent exists
            // add one file first, then add a parent with it, to see if it will cause problems

        }

        private void VerifyCustomLargeSetOfIndicies_1() {
            VerifyParentRelation(0, 1); // 1,2
            VerifyParentRelation(2, 3); // 3,4, careful that 4 & 5 are one folder appart
            VerifyChildRelation(4, Enumerable.Range(5, 22 - 6 + 1).ToList()); // 5, [6 to 22] inclusive
        }

        // TODO: Test and optimize renaming speed
        //

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
