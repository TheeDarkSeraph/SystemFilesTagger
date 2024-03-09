using FileTagDB.Controllers;
using FileTagDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Data.SQLite;

namespace TagDatabaseTester {
    public class TagFileTests {

        List<string> sampleTags;
        List<string> sampleFiles;
        TagController tc;
        FileController fc;
        #region Prints and Helper functions
        private readonly ITestOutputHelper output;
        private readonly DBLocationManager lm;
        public TagFileTests(ITestOutputHelper output) {
            this.output = output;
            lm = DBLocationManager.Instance;
            Utils.SetPrinter(output.WriteLine);
            lm.DBName = "testTags.db";
            DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
            DataHolder.PopulateFiles();
            sampleFiles = DataHolder.sampleFilePaths;
            DataHolder.PopulateTags();
            sampleTags = DataHolder.sampleTags;
            fc = new FileController();
            tc = new TagController(fc);
            CreateEmptyTestDBWithTables();
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

        #region Tag Related Test (mostly)
        private void AddAllTags() {
            tc.CreateTags(sampleTags);
        }
        private void AddAllFiles() {
            fc.BulkAddFiles(sampleFiles); //w parent 4864 wo parent 4802-4900 // significant improvement
        }
        #endregion



        // tag files, untag files
        [Fact]
        public void ShouldTagAndUntagFile() {
            Assert.True(false);
            AddAllTags();
            AddAllFiles(); 


            CleanupTables();
        }

        // tag file, delete file    (tag relation should be remove, tag stays ok)
        [Fact]
        public void ShouldTagFileAndDeleteFileAndItsLinks() {
            // multiple file tag test (2 tags for 2 files)

            Assert.True(false);
            AddAllTags();
            AddAllFiles();


            CleanupTables();

        }

        // tag file, delete tag     (tag relation should be removed, file stays ok)[Fact]
        [Fact]
        public void ShouldTagFileAndDeleteTagAndItsLinks() {
            Assert.True(false);
            // multiple file tag test (2 tags for 2 files)
            AddAllTags();
            AddAllFiles();


            CleanupTables();
        }

        // tag folder in database only (tag all files starting with X)
        [Fact]
        public void ShouldTagPathInDBOnly() {

            Assert.True(false);
        }
        // add and tag folder (add files [external call], then call the above [tag all files starting with X])
        [Fact]
        public void ShouldAndFolderAndItsFilesAndTagThemInDB() {
            Assert.True(false);

        }
        // tag multiple files (non tagged with the tag)
        [Fact]
        public void ShouldAddOneTagMultipleFiles() {
            Assert.True(false);


        }
        // tag multiple files (some already having the tag)
        [Fact]
        public void ShouldAddOneTagMultipleFilesWithSomeHavingTheTag() {
            Assert.True(false);

        }

        // untag multiple files (some tagged some not)
        [Fact]
        public void UntagMultipleFiles() {
            Assert.True(false);

        }
        [Fact]
        public void UntagPath() {
            Assert.True(false);

        }


        // Here we write it how we expect the user to write it
        [Fact]
        public void GetFileWithTagSearchCombination() {
            // Add necessary user tags first
            // We can use steam's tag list as more tags
            Assert.True(false);

        }

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
