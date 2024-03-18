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
using System.Collections;

namespace TagDatabaseTester {
    [Collection("Sequential")] // this stops the parallel with other classes named the same
    public class TagFileTests {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        static List<string> sampleTags;
        static List<string> sampleFiles;
        static FileController fc = new();
        static TagController tc;
        static bool hasInitialized = false;
        
        static bool addedFilesAndTags = false;


        #region Prints and Helper functions
        static ITestOutputHelper output;
        static DBLocationManager lm;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TagFileTests(ITestOutputHelper p_output) {
            lock (fc) {
                if (!hasInitialized) {
                    output = p_output;
                    lm = DBLocationManager.Instance;
                    Utils.SetPrinter(output.WriteLine);
                    lm.DBName = "testTags.db";
                    DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
                    DataHolder.PopulateFiles();
                    sampleFiles = DataHolder.sampleFilePaths;
                    DataHolder.PopulateTags();
                    sampleTags = DataHolder.sampleTags;
                    tc = new TagController(fc);
                    CreateEmptyTestDBWithTables();
                    hasInitialized = true;
                    AddTagAndFilesOnce();
                }
            }
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
        internal void ClearTagFiles() {
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                DBController.DeleteTableData(conn, TableConst.fileTagsTName);
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
            FixAllSampleFiles();
        }
        private void FixAllSampleFiles() {
            for (int i = 0; i < sampleFiles.Count; i++)
                sampleFiles[i] = FileController.FixFilePath(sampleFiles[i]);
        }
        private void AddTagAndFilesOnce() {
            lock (tc) {
                if (!addedFilesAndTags) {
                    AddAllFiles();
                    AddAllTags();
                    addedFilesAndTags = true;
                }
            }
        }
        #endregion



        // tag files, untag files
        [Theory]
        [InlineData(1, 4)]
        [InlineData(2, 4)]
        [InlineData(5, 8)]
        [InlineData(10, 45)]
        [InlineData(7, 65)]
        public void ShouldTagAndUntagFile(int tagIndex, int fileIndex) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                tc.TagFile(tagIDs[sampleTags[tagIndex]], fc.GetFileID(sampleFiles[fileIndex]));

                List<(string, int)> taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                Assert.True(taggedFiles[0].Item1 == sampleFiles[fileIndex]);

                tc.UntagFile(tagIDs[sampleTags[tagIndex]], fc.GetFileID(sampleFiles[fileIndex]));
                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                Assert.Empty(taggedFiles);
                ClearTagFiles();
            }
        }

        // tag file, delete file    (tag relation should be remove, tag stays ok)
        [Theory]
        [InlineData(1, 4)]
        [InlineData(2, 4)]
        [InlineData(5, 8)]
        [InlineData(10, 45)]
        [InlineData(7, 65)]
        public void ShouldTagFileAndDeleteFileAndItsLinks(int tagIndex, int fileIndex) {
            lock (tc) { // files are readded at the end
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                List<(string, int)> taggedFiles;
                tc.TagFile(tagIDs[sampleTags[tagIndex]], fc.GetFileID(sampleFiles[fileIndex]));
                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                Assert.True(taggedFiles[0].Item1 == sampleFiles[fileIndex]);
                fc.DeleteFileOnly(sampleFiles[fileIndex]);
                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);

                Assert.Empty(taggedFiles);
                allTagList = tc.GetAllTags();

                tagIDs.Clear();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                Assert.True(tagIDs.ContainsKey(sampleTags[tagIndex]));
                Assert.Equal(-1, fc.GetFileID(sampleFiles[fileIndex]));


                fc.AddFile(sampleFiles[fileIndex]);
            }
        }

        // tag file, delete tag     (tag relation should be removed, file stays ok)[Fact]
        [Theory]
        [InlineData(1, 4)]
        [InlineData(2, 4)]
        [InlineData(5, 8)]
        [InlineData(10, 45)]
        [InlineData(7, 65)]
        public void ShouldTagFileAndDeleteTagAndItsLinks(int tagIndex, int fileIndex) {
            lock (tc) { // files are readded at the end
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);

                List<(string, int)> taggedFiles;
                tc.TagFile(tagIDs[sampleTags[tagIndex]], fc.GetFileID(sampleFiles[fileIndex]));
                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                Assert.True(taggedFiles[0].Item1 == sampleFiles[fileIndex]);

                // delete tag
                tc.DeleteTag(tagIDs[sampleTags[tagIndex]]);
                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);

                Assert.Empty(taggedFiles);
                allTagList = tc.GetAllTags();

                tagIDs.Clear();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                Assert.False(tagIDs.ContainsKey(sampleTags[tagIndex]));
                Assert.NotEqual(-1, fc.GetFileID(sampleFiles[fileIndex]));

                tc.CreateTag(sampleTags[tagIndex]);
                ClearTagFiles();
            }


        }

        // tag folder in database only (tag all files starting with X)
        [Theory]
        [InlineData(0, 1, new int[] { 1, 2 })]
        [InlineData(0, 2, new int[] { 2 })]
        [InlineData(0, 4, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(0, 5, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(0, 6790, new int[] { 6790, 6800 })]
        [InlineData(1, 1, new int[] { 1, 2 })]
        [InlineData(2, 2, new int[] { 2 })]
        [InlineData(3, 4, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(4, 5, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(5, 6790, new int[] { 6790, 6800 })]
        public void ShouldTagPathInDB(int tagIndex, int parentFolderLine, int[] taggedFilesLine) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                List<(string, int)> taggedFiles;
                
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                List<string> filepaths= new();
                List<int> fileIdsShouldbeTagged;
                List<(string, int)> filesInFolder;
                List<int> fileIdsInFolder = new();
                foreach(int fileline in taggedFilesLine)
                    filepaths.Add(sampleFiles[fileline - 1]);


                fileIdsShouldbeTagged = fc.GetFilesIds(filepaths);
                int tagID = tagIDs[sampleTags[tagIndex]];

                filesInFolder = fc.GetFilesWithPath(sampleFiles[parentFolderLine - 1]);
                foreach (var file in filesInFolder)
                    fileIdsInFolder.Add(file.Item2);
                

                tc.TagFiles(tagID, fileIdsInFolder);
                

                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                HashSet<int> setOftaggedFilesIds = new HashSet<int>();
                foreach(var fileID in taggedFiles)
                    setOftaggedFilesIds.Add(fileID.Item2);
                foreach(int shouldBeTagged in fileIdsShouldbeTagged)
                    Assert.Contains(shouldBeTagged, setOftaggedFilesIds);

                ClearTagFiles();
            }
        }
        
        // tag multiple files (non tagged with the tag)
        [Theory]
        [InlineData(0, new int[] { 1, 2 })]
        [InlineData(0, new int[] { 2 })]
        [InlineData(0, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(0, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(0, new int[] { 6790, 6800 })]
        [InlineData(1, new int[] { 1, 2 })]
        [InlineData(2, new int[] { 2 })]
        [InlineData(3, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(4, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(5, new int[] { 6790, 6800 })]
        public void ShouldTagMultipleFiles(int tagIndex, int[] fileLinesToBeTagged) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                List<(string, int)> taggedFiles;

                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                int tagID = tagIDs[sampleTags[tagIndex]];
                Utils.LogToOutput("Tag id " + tagID);
                List<string> filepaths = new();
                List<int> fileIdsShouldbeTagged;
                List<int> fileIdsToBeTagged = new();
                foreach (int fileline in fileLinesToBeTagged)
                    filepaths.Add(sampleFiles[fileline - 1]);

                fileIdsShouldbeTagged = fc.GetFilesIds(filepaths);

                tc.TagFiles(tagID, fileIdsShouldbeTagged);

                taggedFiles = tc.GetFilesWithTag(tagID);
                HashSet<int> setOftaggedFilesIds = new HashSet<int>();
                foreach (var fileID in taggedFiles) {
                    setOftaggedFilesIds.Add(fileID.Item2);
                    Utils.LogToOutput("tagged file ids found " + fileID.Item2);
                }
                foreach (int shouldBeTagged in fileIdsShouldbeTagged)
                    Assert.Contains(shouldBeTagged, setOftaggedFilesIds);

                ClearTagFiles();
            }
        }
        // tag multiple files (some already having the tag)
        [Theory]
        [InlineData(4, new int[] { 1, 2 }, new int[] { 1 })]
        [InlineData(8, new int[] { 4, 5, 100, 1000, 6700, 6800 }, new int[] { 5, 6700, 6800 })]
        [InlineData(14, new int[] { 5, 100, 1000, 2000 }, new int[] { 5, 2000 })]
        [InlineData(16, new int[] { 6790, 6800 }, new int[] { 6800 })]
        public void ShouldTagMultipleFilesWithSomeHavingTheTagPreAdded(int tagIndex, int[] fileLinesToBeTagged, int[] preaddedSubsetFileLines) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                List<(string, int)> taggedFiles;

                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                List<string> filepaths = new();
                List<int> fileIdsShouldbeTagged;
                int tagID = tagIDs[sampleTags[tagIndex]];
                // preadd tags


                foreach (int fileline in preaddedSubsetFileLines)
                    filepaths.Add(sampleFiles[fileline - 1]);
                fileIdsShouldbeTagged = fc.GetFilesIds(filepaths);
                tc.TagFiles(tagID, fileIdsShouldbeTagged);

                filepaths.Clear();
                // add all tags
                foreach (int fileline in fileLinesToBeTagged)
                    filepaths.Add(sampleFiles[fileline - 1]);
                fileIdsShouldbeTagged = fc.GetFilesIds(filepaths);
                tc.TagFiles(tagID, fileIdsShouldbeTagged);

                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                HashSet<int> setOftaggedFilesIds = new HashSet<int>();
                foreach (var fileID in taggedFiles)
                    setOftaggedFilesIds.Add(fileID.Item2);
                foreach (int shouldBeTagged in fileIdsShouldbeTagged)
                    Assert.Contains(shouldBeTagged, setOftaggedFilesIds);

                ClearTagFiles();
            }

        }

        // untag multiple files (some tagged some not)


        [Theory]
        [InlineData(0, 1, new int[] { 1, 2 })]
        [InlineData(0, 2, new int[] { 2 })]
        [InlineData(0, 4, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(0, 5, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(0, 6790, new int[] { 6790, 6800 })]
        [InlineData(1, 1, new int[] { 1, 2 })]
        [InlineData(2, 2, new int[] { 2 })]
        [InlineData(3, 4, new int[] { 4, 5, 100, 1000, 6700, 6800 })]
        [InlineData(4, 5, new int[] { 5, 100, 1000, 2000 })]
        [InlineData(5, 6790, new int[] { 6790, 6800 })]
        public void ShouldUntagMultipleFiles(int tagIndex, int parentFolderLine, int[] untaggedFilesLine) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                List<(string, int)> taggedFiles;

                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                List<string> filepaths = new();
                List<int> fileIdsShouldNotbeTagged;
                List<(string, int)> filesInFolder;
                List<int> fileIdsInFolder = new();
                foreach (int fileline in untaggedFilesLine)
                    filepaths.Add(sampleFiles[fileline - 1]);


                fileIdsShouldNotbeTagged = fc.GetFilesIds(filepaths);
                int tagID = tagIDs[sampleTags[tagIndex]];
                var watch = System.Diagnostics.Stopwatch.StartNew();

                filesInFolder = fc.GetFilesWithPath(sampleFiles[parentFolderLine - 1]);
                watch.Stop();
                Utils.LogToOutput("Getting file with path time "+watch.ElapsedMilliseconds);

                foreach (var file in filesInFolder)
                    fileIdsInFolder.Add(file.Item2);

                watch = System.Diagnostics.Stopwatch.StartNew();
                tc.TagFiles(tagID, fileIdsInFolder);
                watch.Stop();
                Utils.LogToOutput("Tagging time " + watch.ElapsedMilliseconds); // 240 seconds? for file line 4 

                watch = System.Diagnostics.Stopwatch.StartNew();
                tc.UntagFiles(tagID, fileIdsShouldNotbeTagged);
                watch.Stop();
                Utils.LogToOutput("untagging time " + watch.ElapsedMilliseconds);


                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                HashSet<int> setOftaggedFilesIds = new HashSet<int>();
                foreach (var fileID in taggedFiles)
                    setOftaggedFilesIds.Add(fileID.Item2);
                foreach (int shouldNotBeTagged in fileIdsShouldNotbeTagged)
                    Assert.DoesNotContain(shouldNotBeTagged, setOftaggedFilesIds);
                ClearTagFiles();
            }
        }


        [Theory]
        [InlineData(0, 1,2, new int[] { 2 })]
        public void ShouldUntagPath(int tagIndex, int parentFolderLine, int folderLineToUntag, int[] untaggedFilesLine) {
            lock (tc) {
                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                List<(string, int)> taggedFiles;

                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);
                List<string> filepaths = new();
                List<int> fileIdsShouldNotbeTagged;
                List<(string, int)> filesInFolder;
                List<int> fileIdsInFolder = new();
                foreach (int fileline in untaggedFilesLine)
                    filepaths.Add(sampleFiles[fileline - 1]);


                fileIdsShouldNotbeTagged = fc.GetFilesIds(filepaths);
                int tagID = tagIDs[sampleTags[tagIndex]];

                filesInFolder = fc.GetFilesWithPath(sampleFiles[parentFolderLine - 1]);
                foreach (var file in filesInFolder)
                    fileIdsInFolder.Add(file.Item2);


                tc.TagFiles(tagID, fileIdsInFolder);

                filesInFolder = fc.GetFilesWithPath(sampleFiles[folderLineToUntag - 1]);
                fileIdsInFolder.Clear();
                foreach (var file in filesInFolder)
                    fileIdsInFolder.Add(file.Item2);
                tc.UntagFiles(tagID, fileIdsInFolder);


                taggedFiles = tc.GetFilesWithTag(tagIDs[sampleTags[tagIndex]]);
                HashSet<int> setOftaggedFilesIds = new HashSet<int>();
                foreach (var fileID in taggedFiles)
                    setOftaggedFilesIds.Add(fileID.Item2);
                foreach (int shouldNotBeTagged in fileIdsShouldNotbeTagged)
                    Assert.DoesNotContain(shouldNotBeTagged, setOftaggedFilesIds);
                ClearTagFiles();
            }
        }

        private List<string> SubListFiles(int startLine, int endLine) {
            List<string> files = new();
            for(int i = startLine - 1; i < endLine; i++)
                files.Add(sampleFiles[i]);
            return files;
        }
        // TODO: customize this to do multiple queries and assert them.
        /* query format:
         * 1- search query
         * 2- file step (how much to move the start by when including tags to files)
         *      (affects overlapping region)
         * 3- Word indicies to be included
         * 4- Word indicies to be excluded 
         * 
         * This test wouldnt work, we need something like, unions and intersections
         * Modify '3' to be arrays of unions. every array will contain things that will be unioned together
         * And then the union results are all intersected together.
         */
        // Here we write it how we expect the user to write it
        [Theory]
        [InlineData("action_rpg -acting", 5, new int[] { 0 },false, new int[] { 3 })]
        
        [InlineData("a*", 15, new int[] { }, false, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 })]
        [InlineData("A*", 15, new int[] { }, false, new int[] { 9 })]
        
        [InlineData("a*", 15, new int[] { },true, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        [InlineData("A*", 15, new int[] { }, true, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]

        [InlineData("*a*", 15, new int[] { },false, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 17, 19, 22, 23, 24, 25, 26, 27, 29, 30, 31, 33, 34 })]

        [InlineData("r* t*", 15, new int[] { }, false, new int[] { 20, 21, 22, 23, 24 }, new int[] { 31, 32, 33, 34 })]
        [InlineData("r* t* -role*", 15, new int[] { 22, 23, 24 }, false, new int[] { 20, 21, 22, 23, 24 }, new int[] { 31, 32, 33, 34 })]

        [InlineData("a* -act*", 15, new int[] { 0, 1, 2, 3, 4, 5, 6 },false, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 })]
        [InlineData("acting fighting strategy", 15, new int[] { }, false, new int[] { 0 }, new int[] { 14 }, new int[] { 29 })]



        public void GetFilesWithTagSearchCombination(string query, int filestep, int[]excludedIndicies, bool ignoreCase, params int[][] includedIndicies) {
            lock (tc) {
                // for this test I think it is better to create our tags and test the queries ourselves
                // act* act___ a* A* (one ignore case 'A' and one dont)
                
                List<string> nT=
                [   // 0           1            2               3             4               5
                    "acting" , "action" , "action_based" , "action_rpg" , "action's" , "action\"s%_@!#$" ,
                    // 6            7            8            9             10            11          12
                    "active" , "adventure" , "animals" , "Apocalypse" , "beatemup" , "boss_based" , "easy" ,
                    //   13             14         15       16        17      18           19            20
                    "fight_based" , "fighting" , "flow" , "grind" , "hard" , "kids" , "platformer" , "roguelike" ,
                    //   21           22                23                  24           25         26
                    "roguelite" , "roleplay" , "roleplay_adventure" , "roleplay_solo" , "sad" , "sandbox" ,
                    //    27           28         29              30             31         32          33
                    "singleplayer" , "solo" , "strategy" , "strategy_based" , "tactics" , "time" , "time_based" ,
                    //   34         35
                    "turn_based" , "zoo"
                ];
                // total 36 for now so we need  to have overlaps to understand what will be visible
                // 1, 1..2, 1..3,1..4,.... till 1..end, then 2..end,3..end and so on
                // But in reverse... hmmm, maybe do the tags 1 matches 1..36, 2 matches 2..37 and so on
                tc.CreateTags(nT);

                List<Tag> allTagList = tc.GetAllTags();
                Dictionary<string, int> tagIDs = new();
                foreach (Tag tag in allTagList)
                    tagIDs.Add(tag.name, tag.id);

                Dictionary<string, int> filesLines = new();

                for (int i = 0; i < sampleFiles.Count; i++)
                    filesLines.Add(sampleFiles[i], i + 1);

                int filestart = 1;
                int step = filestep;
                var watch = System.Diagnostics.Stopwatch.StartNew();
                // NOTE: This can be optimized twice, first opt, is to get all ids at once and split list send
                //          2nd would require a function call tagfiles to take a list of tags and specify steps (it would be custom
                for (int i = 0; i < nT.Count; i++) {
                    tc.TagFiles(tagIDs[nT[i]], fc.GetFilesIds(SubListFiles(filestart + i*step, filestart + i * step + nT.Count)));
                    //Utils.LogToOutput($"This tag {nT[i]} with files from lines {filestart + i * step} to {filestart + i * step + nT.Count}");
                }
                watch.Stop();
                Utils.LogToOutput("inserting time " + watch.ElapsedMilliseconds);

                // -acting is 1 to 127 (121 to 127, because they have action_rpg, so they have action rpg tags) which is clearly, then 151 to rest
                // Both intersect and AND seem to work as expected, but not as intended...
                // The way -word works is it translates to anything that does not only have "acting"
                //
                watch = System.Diagnostics.Stopwatch.StartNew();

                List<(string,int)> filesGot = tc.GetFilesWithTagQuery(query, ignoreCase);
                watch.Stop();
                Utils.LogToOutput("compound query time " + watch.ElapsedMilliseconds);


                SortedSet<int> linesContained = new();
                for (int i = 0; i < filesGot.Count; i++) {
                    try {
                        linesContained.Add(filesLines[filesGot[i].Item1]);
                    } catch { }
                }
                List<SortedSet<int>> intersections= new();
                foreach (int[] unionIndicies in includedIndicies) {
                    SortedSet<int> unionSet = new SortedSet<int>();
                    foreach (int index in unionIndicies) {
                        int endj = filestart + index * step + nT.Count;
                        for (int j = filestart + index * step; j <= endj; j++)
                            unionSet.Add(j);
                    }
                    intersections.Add(unionSet);
                }
                SortedSet<int> linesToHave = new();
                linesToHave.UnionWith(intersections[0]);
                for(int i = 1; i < intersections.Count; i++)
                    linesToHave.IntersectWith(intersections[1]);
                foreach (int index in excludedIndicies) {
                    int endj = filestart + index * step + nT.Count;
                    for (int j = filestart + index * step; j <= endj; j++)
                        linesToHave.Remove(j);
                }
                Utils.LogToOutput("Compare: ");
                Utils.LogToOutput(string.Join(",",linesToHave));
                Utils.LogToOutput("Got: ");
                Utils.LogToOutput(string.Join(",", linesContained));
                Assert.Equal(linesToHave, linesContained);
                ClearTagFiles();
            }
        }



        [Fact]
        public void ShouldGetFileTags() {
            // return list of tag ids
            lock (tc) {
                // First we tag a file with multiple tags, then we retrieve its tags
                // generate random integers, tag them to a file, verify that the file has those tags
                Random rnd = new Random();
                List<Tag> tags = tc.GetAllTags().OrderBy(x => rnd.Next()).Take(12).ToList();
                List<int> tagIds = tags.Select(x => x.id).ToList();
                int fileId = fc.GetFileID(sampleFiles[10]);
                tagIds.ForEach(tagId => tc.TagFile(tagId,fileId));
                
                tagIds.Sort();
                List<int> fileTags = tc.GetFileTags(sampleFiles[10]);
                fileTags.Sort();
                Assert.Equal(fileTags, tagIds);
                
                fileTags = tc.GetFileTags(fileId);
                fileTags.Sort();
                Assert.Equal(fileTags, tagIds);

                ClearTagFiles();
            }
        }
        [Fact]
        public void ShouldListOfFilesAssociatedTagsIndividually() { // return list of list
            lock (tc) {
                // same as above but with multiple files
                Random rnd = new Random();
                List<List<int>> filesTagsIdsExpected = new();
                List<int> fileIds = new();
                List<string> files = [sampleFiles[3], sampleFiles[8], sampleFiles[13]];
                files.ForEach(filename => {
                    List<Tag> tags = tc.GetAllTags().OrderBy(x => rnd.Next()).Take(12).ToList();
                    List<int> tagIds = tags.Select(x => x.id).ToList();

                    int fileId = fc.GetFileID(filename);
                    tagIds.ForEach(tagId => tc.TagFile(tagId, fileId));
                });

                var watch = System.Diagnostics.Stopwatch.StartNew();
                List<List<int>> filesTagsIdsActual = tc.GetFilesTags(files);
                watch.Stop();
                Utils.LogToOutput("gettags with file names: " + watch.ElapsedMilliseconds);


                Assert.Equal(filesTagsIdsActual.Count,filesTagsIdsExpected.Count);
                for(int i = 0; i < filesTagsIdsExpected.Count; i++) {
                    filesTagsIdsExpected[i].Sort();
                    filesTagsIdsActual[i].Sort();
                    Assert.Equal(filesTagsIdsActual, filesTagsIdsExpected);
                }
                // now with ids
                watch = System.Diagnostics.Stopwatch.StartNew();
                filesTagsIdsActual = tc.GetFilesTags(fileIds);
                watch.Stop();
                Utils.LogToOutput("gettags with file ids: " + watch.ElapsedMilliseconds);

                Assert.Equal(filesTagsIdsActual.Count, filesTagsIdsExpected.Count);
                for (int i = 0; i < filesTagsIdsExpected.Count; i++) {
                    filesTagsIdsExpected[i].Sort();
                    filesTagsIdsActual[i].Sort();
                    Assert.Equal(filesTagsIdsActual, filesTagsIdsExpected);
                }


                ClearTagFiles();
            }
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
