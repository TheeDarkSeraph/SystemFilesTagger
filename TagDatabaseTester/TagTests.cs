using FileTagDB.Controllers;
using FileTagDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.Data.SQLite;
using FileTagDB.Models;

namespace TagDatabaseTester {
    [Collection("Sequential")] // this stops the parallel problem with deleting DB file
    // also it makes them run sequentially
    public class TagTests {
        // TODO: Can add tags from a file

        // add static populate data and delete data
        // TODO: Test all languages for tags
        // TODO: When user types in a space or an illegal character, show a tooltip for a few seconds
        //          and delete that character OR REPLACE space with _ underscore
        //      note that user can't start a tag with '-' or use '+' anywhere in the string
        // TODO: for files with 3-4 character extensions, auto add that extension as a tag if it doesn't exist
        // ( a b+c) (a -b+c) (a -b) (a+b c+d) (a* b)

        // TODO: Be sure to escape the users '_' underscore and '%' also replace '*' with non escaped %

        List<string> sampleTags;
        TagController tc;
        FileController fc;

        #region Prints and Helper functions
        private readonly ITestOutputHelper output;
        private readonly DBLocationManager lm;
        public TagTests(ITestOutputHelper output) {
            this.output = output;
            lm = DBLocationManager.Instance;
            Utils.SetPrinter(output.WriteLine);
            lm.DBName = "testTags.db";
            DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
            DataHolder.PopulateTags();
            sampleTags = DataHolder.sampleTags;
            fc = new FileController();
            tc = new TagController(fc);
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











        #region Creating individual tags
        [Fact]
        public void CreateTagsAndGetID() {
            CreateEmptyTestDBWithTables();
            int tagID = tc.CreateTag(sampleTags[0]);
            Assert.Equal(1, tagID);
            tagID = tc.CreateTag(sampleTags[1]);
            Assert.Equal(2, tagID);
            tagID = tc.CreateTag(sampleTags[2]);
            Assert.Equal(3, tagID);
            tagID = tc.CreateTag(sampleTags[2]);
            Assert.Equal(-1, tagID);
            Assert.Equal(sampleTags[0], tc.GetTagName(1));
            Assert.Equal(1, tc.GetTagID(sampleTags[0]));
            CleanupTables();
        }
        [Fact]
        public void ShouldCreateTags() {
            CreateEmptyTestDBWithTables();
            tc.CreateTag(sampleTags[0]);
            tc.CreateTag(sampleTags[1]);
            tc.CreateTag(sampleTags[2]);
            tc.CreateTag(sampleTags[2]);
            tc.CreateTag(sampleTags[3]);
            // Now count rows
            Assert.Equal(4, tc.CountTags());
            CleanupTables();
        }

        [Fact]
        public void ShouldCreateTagsWithStrangeCharacters() {
            // test arabic, test many quotations (even and odd) test '$'
            CreateEmptyTestDBWithTables();
            sampleTags[0] += "'''";
            sampleTags[1] += "''";
            sampleTags[2] += "''\"''";
            sampleTags[4] += "غثغثyes' fed' \"يبنثبثشي";
            tc.CreateTag(sampleTags[0]);
            tc.CreateTag(sampleTags[1]);
            tc.CreateTag(sampleTags[2]);
            tc.CreateTag(sampleTags[3]);
            Assert.Equal(sampleTags[0], tc.GetTagName(1));
            Assert.Equal(sampleTags[1], tc.GetTagName(2));
            Assert.Equal(sampleTags[2], tc.GetTagName(3));
            Assert.Equal(sampleTags[3], tc.GetTagName(4));
            Assert.Equal(4, tc.CountTags());
            CleanupTables();
        }

        [Fact]
        public void ShouldCreateTagsWithDBCharacters() {
            // test arabic, test many quotations (even and odd) test '$'
            CreateEmptyTestDBWithTables();
            sampleTags[0] += "_%";
            tc.CreateTag(sampleTags[0]);
            Assert.Equal(sampleTags[0], tc.GetTagName(1));
            CleanupTables();
        }
        #endregion
        [Fact]
        public void CreateAndRenameTag() { // rename twice, one should have effect , second repeated time we don't know?
            CreateEmptyTestDBWithTables();
            tc.CreateTag(sampleTags[0]);
            tc.CreateTag(sampleTags[1]);
            tc.CreateTag(sampleTags[2]);
            tc.CreateTag(sampleTags[3]);
            Assert.Equal(-1,tc.RenameTag(1, sampleTags[1])); // existing name
            Assert.Equal(1,tc.RenameTag(1, sampleTags[4])); // new name
            Assert.Equal(1, tc.RenameTag(1, sampleTags[4])); // unchanged
            Assert.Equal(-1, tc.RenameTag(2, sampleTags[4])); // violates uniqueness
            
            Assert.Equal(0, tc.RenameTag(sampleTags[4], sampleTags[4])); // we forced it to 0
            Assert.Equal(-1, tc.RenameTag(sampleTags[2], sampleTags[4])); // does not work

            Assert.Equal(-1, tc.GetTagID(sampleTags[0]));

            Assert.Equal(sampleTags[4], tc.GetTagName(1));
            CleanupTables();
        }


        [Fact]
        public void CreateBulkTagsNothingAdded() {

            CreateEmptyTestDBWithTables();
            AddAllTags();
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[1]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[23]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[14]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[19]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[31]));
            CleanupTables();
        }
        [Fact]
        public void CreateBulkTagsSomePreAdded() {
            CreateEmptyTestDBWithTables();
            tc.CreateTag(sampleTags[1]);
            tc.CreateTag(sampleTags[23]);
            tc.CreateTag(sampleTags[14]);
            AddAllTags();
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[1]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[23]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[14]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[19]));
            Assert.NotEqual(-1, tc.GetTagID(sampleTags[31]));
            CleanupTables();
        }

        [Fact]
        public void ShouldGetAllTags() {
            CreateEmptyTestDBWithTables();
            AddAllTags();
            List<string> insertedTags = tc.GetAllTagsAsStrings();
            List<Tag> insertedTagsAsTags = tc.GetAllTags();
            insertedTags.Sort();
            insertedTagsAsTags.Sort();
            sampleTags.Sort();
            Assert.Equal(sampleTags.Count, insertedTags.Count);
            for (int i = 0; i < sampleTags.Count; i++) {
                Assert.Equal(sampleTags[i], insertedTags[i]);
                Assert.Equal(sampleTags[i], insertedTagsAsTags[i].name);
            }
        }
        private void AddAllTags() {
            tc.CreateTags(sampleTags);
        }




        [Fact]
        public void DeleteTag() {
            CreateEmptyTestDBWithTables();
            tc.CreateTag(sampleTags[0]);
            tc.CreateTag(sampleTags[1]);
            tc.CreateTag(sampleTags[2]);
            int id = tc.CreateTag(sampleTags[3]);

            Assert.Equal(id, tc.GetTagID(sampleTags[3]));
            tc.DeleteTag(id);

            Assert.Equal("+NotFound", tc.GetTagName(id));
            Assert.Equal(-1, tc.GetTagID(sampleTags[3]));
            CleanupTables();
        }
    }
}
