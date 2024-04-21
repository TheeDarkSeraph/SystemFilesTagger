using FileTagDB;
using FileTagEF;
using FileTagEF.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace TagEFTester {
    [Collection("Sequential")] 
    public class TagBasicEFTests {
        // add the tag tests

        static List<string> sampleTags = null!;
        static readonly TagController tc = new TagController();
        static readonly FileController fc = new FileController();
        static bool hasInitialized = false;

        #region Prints, Helper and Init functions
        private readonly ITestOutputHelper output = null!;
        private readonly LocationManager lm = null!;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public TagBasicEFTests(ITestOutputHelper p_output) {
            lock (fc) {
                if (!hasInitialized) {
                    output = p_output;
                    lm = LocationManager.Instance;
                    Utils.SetPrinter(output.WriteLine);
                    lm.DBName = "testTags.db";
                    using (var context = DBController.GetContext()) {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();
                    }
                    DataHolder.PopulateTags();
                    sampleTags = DataHolder.sampleTags;
                    hasInitialized = true;
                }
            }
        }
        internal void CleanupTables() {
            using (var context = DBController.GetContext()) {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
        #endregion

        #region Creating individual tags
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldCreateTag(int tagIndex) {
            lock (tc) {
                int tagID = tc.CreateTag(sampleTags[tagIndex]);
                Assert.NotEqual(-1, tagID);
                CleanupTables();
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void ShouldNotCreateDuplicateTag(int tagIndex) {
            lock (tc) {
                tc.CreateTag(sampleTags[tagIndex]);
                int tagID = tc.CreateTag(sampleTags[tagIndex]);
                Assert.NotEqual(-1, tagID);
                CleanupTables();
            }
        }
        [Theory]
        [InlineData(new int[] { 0, 1, 2, 2, 3 }, 4)]
        [InlineData(new int[] { 0, 5, 7, 9, 10, 5, 9, 8, 10 }, 6)]
        [InlineData(new int[] { 10, 20, 15, 24, 31 }, 5)]
        public void ShouldCreateTags(int[] tagIDs, int totalCreated) {
            lock (tc) {
                foreach (int tagID in tagIDs) {
                    tc.CreateTag(sampleTags[tagID]);
                }
                Assert.Equal(totalCreated, tc.CountTags());
                CleanupTables();
            }
        }

        [Fact]
        public void ShouldCreateTagsWithStrangeCharacters() {
            // test arabic, test many quotations (even and odd) test '$'
            lock (tc) {
                sampleTags[0] += "'''";
                sampleTags[1] += "''";
                sampleTags[2] += "''\"''";
                sampleTags[3] += "غثغثyes' fed' \"يبنثبثشي";
                int id1 = tc.CreateTag(sampleTags[0]);
                int id2 = tc.CreateTag(sampleTags[1]);
                int id3 = tc.CreateTag(sampleTags[2]);
                int id4 = tc.CreateTag(sampleTags[3]);
                Assert.Equal(sampleTags[0], tc.GetTagName(id1));
                Assert.Equal(sampleTags[1], tc.GetTagName(id2));
                Assert.Equal(sampleTags[2], tc.GetTagName(id3));
                Assert.Equal(sampleTags[3], tc.GetTagName(id4));
                Assert.Equal(4, tc.CountTags());
                CleanupTables();
            }
        }

        [Fact]
        public void ShouldCreateTagsWithDBCharacters() {
            // test arabic, test many quotations (even and odd) test '$'
            lock (tc) {
                sampleTags[0] += "_%";
                int tagId = tc.CreateTag(sampleTags[0]);
                Utils.LogToOutput($"Tag id {tagId}");
                Assert.Equal(sampleTags[0], tc.GetTagName(tagId));
                CleanupTables();
            }
        }
        #endregion
        [Fact]
        public void CreateAndRenameTag() { // rename twice, one should have effect , second repeated time we don't know?
            lock (tc) {
                int id1 = tc.CreateTag(sampleTags[0]);
                int id2 = tc.CreateTag(sampleTags[1]);
                tc.CreateTag(sampleTags[2]);
                tc.CreateTag(sampleTags[3]);
                Assert.Equal(-1, tc.RenameTag(id1, sampleTags[1])); // existing name
                Assert.Equal(1, tc.RenameTag(id1, sampleTags[4])); // new name
                Assert.Equal(1, tc.RenameTag(id1, sampleTags[4])); // unchanged
                Assert.Equal(-1, tc.RenameTag(id2, sampleTags[4])); // violates uniqueness

                Assert.Equal(0, tc.RenameTag(sampleTags[4], sampleTags[4])); // we forced it to 0
                Assert.Equal(-1, tc.RenameTag(sampleTags[2], sampleTags[4])); // does not work

                Assert.Equal(-1, tc.GetTagID(sampleTags[0]));

                Assert.Equal(sampleTags[4], tc.GetTagName(1));
                CleanupTables();
            }
        }


        private void AddAllTags() {
            tc.CreateTags(sampleTags);
        }
        [Fact]
        public void CreateBulkTagsNothingAdded() {
            lock (tc) {
                AddAllTags();
                Assert.NotEqual(-1, tc.GetTagID(sampleTags[1]));
                Assert.NotEqual(-1, tc.GetTagID(sampleTags[23]));
                Assert.NotEqual(-1, tc.GetTagID(sampleTags[14]));
                Assert.NotEqual(-1, tc.GetTagID(sampleTags[19]));
                Assert.NotEqual(-1, tc.GetTagID(sampleTags[31]));
                CleanupTables();
            }
        }
        [Fact]
        public void CreateBulkTagsSomePreAdded() {
            lock (tc) {
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
        }

        [Fact]
        public void ShouldGetAllTags() {
            lock (tc) {
                AddAllTags();
                List<string> insertedTags = tc.GetAllTagsAsStrings();
                List<TagDto> insertedTagsAsTags = tc.GetAllTags();
                insertedTags.Sort();
                insertedTagsAsTags.Sort();
                sampleTags.Sort();
                Assert.Equal(sampleTags.Count, insertedTags.Count);
                for (int i = 0; i < sampleTags.Count; i++) {
                    Assert.Equal(sampleTags[i], insertedTags[i]);
                    Assert.Equal(sampleTags[i], insertedTagsAsTags[i].name);
                }
                CleanupTables();
            }
        }




        [Fact]
        public void DeleteTag() {
            lock (tc) {
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
}
