using FileTagDB;
using FileTagEF;
using FileTagEF.Controllers;
using Xunit.Abstractions;

namespace TagEFTester {
    // NOTE: All tests here can run in parallel (so just run the main class from the menu),
    //          this is thanks to the locking which prevents the threads from doing their parallel work.
    //      Tests here are not optimized to speed up each others work. Each tests runs as if it was an alone test
    //          which causes multiple full re-inserts and full deletes

    [Collection("Sequential")]
    public class FileEFTests {
        // NOTE: All tests are customized to files listed in filepaths.txt main developer (Salah Elabyad)

        List<string> sampleFiles;
        FileController fc;
        
        #region Prints and Helper functions

        private readonly ITestOutputHelper output = null!;
        private readonly LocationManager lm = null!;
        public FileEFTests(ITestOutputHelper output) {
            this.output = output;
            Utils.SetPrinter(output.WriteLine);
            lm = LocationManager.Instance;
            lm.DBName = "testTags.db";
            using (var context = DBController.GetContext()) {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
            DataHolder.PopulateFiles();
            sampleFiles = DataHolder.sampleFilePaths;
            fc = new FileController();
        }
        internal void CleanupTables() {
            using (var context = DBController.GetContext()) {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
        #endregion

        #region Finished Tests
        [Fact]
        public void GetRootPaths() {
            string testpath;

            testpath = @"C:\Windpws";
            Assert.Equal(@"C:\",FileController.GetParentPath(testpath));
            testpath =@"C:\";
            Assert.Null(FileController.GetParentPath(testpath));

            testpath = @"C:";
            Assert.Null(FileController.GetParentPath(testpath));

            testpath = @"hello.txt";
            Assert.Equal("", FileController.GetParentPath(testpath));

            testpath = @"test\hello.txt";
            Assert.Equal("", FileController.GetParentPath(testpath));

            testpath = @"H:";
            Assert.Null(FileController.GetParentPath(testpath));
            
            testpath = @"H:\";
            Assert.Null(FileController.GetParentPath(testpath));

            testpath = @"H:\MusicProj+Song";
            Assert.Equal(@"H:\", FileController.GetParentPath(testpath));
            testpath = @"H:\MusicProj+Song\";
            Assert.Equal(@"H:\", FileController.GetParentPath(testpath));

            testpath = @"H:\MusicProj+Song\GuitarPro\GP2";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", FileController.GetParentPath(testpath));
            
            testpath = @"H:\MusicProj+Song\GuitarPro\GP2\";
            Assert.Equal(@"H:\MusicProj+Song\GuitarPro", FileController.GetParentPath(testpath));
        }

        [Fact]
        public void ShouldGetNonExistingFileParent() {
            Assert.Equal("H:\\",FileController.GetParentPath("H:\\MusicProj+Sg22"));
        }
        [Fact]
        public void CreateFile() {
            lock (fc) {
                AddSingleFileAndVerify(0, 0);
                Assert.NotEqual(-1, fc.AddFile(sampleFiles[0]));
                CleanupTables();
            }
        }

        private void AddSingleFileAndVerify(int count, int index) {
            int fileID = fc.AddFile(sampleFiles[index]);
            Assert.Equal(count + 1, fileID);
            Assert.Equal(fileID, fc.GetFileId(sampleFiles[index]));
            Assert.Equal(FileController.FixFilePath(sampleFiles[index]), fc.GetFilePath(fileID));
        }

        [Fact]
        public void CreateFilesAndOneConnection() {
            lock (fc) {
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
        }
        [Fact]
        public void CreateFilesThenChildFilesWhichWillBeRelated() {
            lock (fc) {
                FixAllSampleFiles();
                for (int i = 0; i < 100; i++) 
                    AddSingleFileAndVerify(i, i);
                VerifyCustomIndicies();
                CleanupTables();
            }
        }
        bool fixedSampleFiles = false;
        private void FixAllSampleFiles() {
            if (fixedSampleFiles)
                return;
            fixedSampleFiles = true;
            for(int i = 0; i < sampleFiles.Count; i++)
                sampleFiles[i] = FileController.FixFilePath(sampleFiles[i]);
        }


        [Fact]
        public void CreateFilesThenParentFilesWhichWillBeRelated() { // for time
            lock (fc) {
                for (int i = sampleFiles.Count - 1; i > (sampleFiles.Count - 200); i--)
                    fc.AddFile(sampleFiles[i]);
                for (int i = 100; i > -1; i--)
                    fc.AddFile(sampleFiles[i]);
                FixAllSampleFiles();
                List<(string, int)> fileChilds = fc.GetFileChildren(sampleFiles[4 - 1]);
                for (int i = 0; i < fileChilds.Count; i++) {
                    Assert.NotEqual(5, fileChilds[i].Item2);
                    Assert.NotEqual(6, fileChilds[i].Item2);
                    Assert.NotEqual(7, fileChilds[i].Item2);
                    Assert.NotEqual(8, fileChilds[i].Item2);
                }


                CleanupTables();
            }
        }
        [Fact]
        public void ShouldBulkAddFiles() {
            lock (fc) {
                FixAllSampleFiles();
                for (int i = 0; i < 10; i++)
                    Utils.LogToOutput($"File {i + 1,-3} {sampleFiles[i]}");
                fc.BulkAddFiles(sampleFiles); //w parent 4864 wo parent 4802-4900 // significant improvement
                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[6830 - 1]));

                VerifyCustomLargeSetOfIndicies_1_AllIncluded();
                CleanupTables();
            }
        }

        [Fact]
        public void ShouldBulkAddFilesWithPreaddedFiles() {
            lock (fc) {
                FixAllSampleFiles();

                fc.AddFile(sampleFiles[5]); // commenting this line removes the parent
                fc.AddFile(sampleFiles[6]); // commenting this line removes the parent

                //sampleFiles.RemoveAt(4);  // removing this will cause issue with verify index, since it does need the order
                fc.BulkAddFiles(sampleFiles); //w parent 4864 wo parent 4802-4900 // significant improvement
                                              //fc.ACBulkAddFilesV1(sampleFiles); //w parent 9547 wo parent 4706-4805

                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[6830 - 1]));

                VerifyCustomLargeSetOfIndicies_1_AllIncluded();
                CleanupTables();
            }
        }
        [Fact]
        public void ShouldBulkAddFilesWithChildsExisting_1() {
            lock (fc) {
                FixAllSampleFiles();

                // having re-inserts (inserting here, then re-inserting below) will be a bit slower (about 60-70% more time)
                fc.BulkAddFiles(sampleFiles.Skip(22).ToList()); //w parent 4864 wo parent 4802-4900 // significant improvement
                fc.BulkAddFiles(sampleFiles.Take(22).ToList());

                //sampleFiles.RemoveAt(4);  // removing this will cause issue with verify index, since it does need the order
                fc.BulkAddFiles(sampleFiles); //w parent 4864 wo parent 4802-4900 // significant improvement

                //fc.ACBulkAddFilesV1(sampleFiles); //w parent 9547 wo parent 4706-4805


                // DO update first then next test

                VerifyCustomLargeSetOfIndicies_1_AllIncluded();
                CleanupTables();
            }
        }

        [Fact]
        public void ShouldBulkAddFilesWithChildsExisting_RepeatedChilds() {
            lock (fc) {
                // this causes a lot of issues re-inserting things
                FixAllSampleFiles();
                fc.BulkAddFiles(sampleFiles.Skip(19).ToList()); //w parent 4864 wo parent 4802-4900 // significant improvement
                fc.BulkAddFiles(sampleFiles.Take(22).ToList()); // repeated childs

                //sampleFiles.RemoveAt(4);  // removing this will cause issue with verify index, since it does need the order
                fc.BulkAddFiles(sampleFiles); // this causes major delay because it tries to re-insert everything

                // DO update first then next test

                VerifyCustomLargeSetOfIndicies_1_AllIncluded();
                CleanupTables();
            }
        }
        //List<(string, int)> fileData = fc.RetrieveFileChildren(1); // 53
        //for (int i = 0; i < fileData.Count; i++) {
        //    Utils.LogToOutput(fileData[i].Item2 + " " + fileData[i].Item1);
        //}
        [Theory]
        [InlineData(4, new int[] { 4, 5, 6, 7, 8, 100, 599, 600, 700, 1000, 2400, 4000, 4400, 6400 }, new int[] { 1, 2, 3 })]
        [InlineData(3, new int[] { 3, 4, 5, 6, 7, 8, 100, 599, 600, 700, 1000, 2400, 4000, 4400, 6400 }, new int[] { 1, 2 })]
        [InlineData(1, new int[] { 1, 2 }, new int[] { 3, 4, 5, 6, 7, 8, 100, 599, 600, 700, 1000, 2400, 4000, 4400, 6400 })]
        [InlineData(2, new int[] { 2 }, new int[] { 1, 3, 4, 5, 6, 7, 8, 100, 599, 600, 700, 1000, 2400, 4000, 4400, 6400 })]
        [InlineData(5, new int[] { 5, 6, 7, 8, 20, 30, 100, 1000, 4000, 6750 }, new int[] { 6790, 6800, 1, 2, 3, 4 })]
        public void ShouldGetAllFilesUnderCertainPath(int fileLine, int[] existing, int[] nonExisting) {
            lock (fc) {
                // folder and all files in it or in its folder recursively (And the folders)
                fc.BulkAddFiles(sampleFiles); // this causes major delay because it tries to re-insert everything
                FixAllSampleFiles();
                Utils.LogToOutput($"File to search {sampleFiles[fileLine - 1]}");
                List<(string, int)> files = fc.GetFilesWithPath(sampleFiles[fileLine - 1]);
                Dictionary<string, int> childFiles = new();
                for (int i = 0; i < files.Count; i++) {
                    childFiles.Add(files[i].Item1, 1);
                }
                foreach (int exist in existing) // assert they exist, but log problem line
                    Assert.Equal(-1, childFiles.ContainsKey(sampleFiles[exist - 1]) ? -1 : exist);

                foreach (int exist in nonExisting)
                    Assert.Equal(-1, childFiles.ContainsKey(sampleFiles[exist - 1]) ? exist : -1);
                CleanupTables();
            }
        }

        #endregion

        #region Verification
        private void VerifyParentByLineNumber(int parentLine, int childLine) {
            Utils.LogToOutput(string.Format("Fetching parent relations of lines {0} , {1} : corresponding to: \n {2}  ::  {3}",
                parentLine, childLine, sampleFiles[parentLine - 1], sampleFiles[childLine - 1]));
            VerifyParentRelation(parentLine - 1, childLine - 1);
        }

        private void VerifyParentRelation(int parentIndex, int childFileIndex) {
            Utils.LogToOutput("Index print " + parentIndex + " , " + childFileIndex);
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[parentIndex]));
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[childFileIndex]));
            Assert.Equal(fc.GetFileId(sampleFiles[parentIndex]),
                fc.GetFilesParentID(sampleFiles[childFileIndex]));
        }
        private void VerifyMultipleParentRelation(int parentIndex, List<int> childIndicies) {
            foreach (int cIndex in childIndicies)
                VerifyParentRelation(parentIndex, cIndex);
        }
        private void VerifyCustomLargeSetOfIndicies_1_AllIncluded() {
            VerifyParentByLineNumber(1, 2); // 1,2
            VerifyParentByLineNumber(3, 4); // 3,4, careful that 4 & 5 are one folder appart
            VerifyParentByLineNumber(5, 6); // 6 is the start of the list
            VerifyParentByLineNumber(5, 22); // 6 is the end of the list so everything in between would be fine I would assume
            VerifyParentByLineNumber(22, 23); // 6 is the end of the list
            VerifyParentByLineNumber(22, 36); // 6 is the end of the list
            VerifyParentByLineNumber(65, 66); // 6 is the end of the list
            VerifyParentByLineNumber(65, 6702); // 6 is the end of the list
            VerifyParentByLineNumber(6702, 6703); // 6 is the end of the list
            VerifyParentByLineNumber(6702, 6750); // 6 is the end of the list
            VerifyParentByLineNumber(6752, 6753); // 6 is the end of the list
            VerifyParentByLineNumber(6752, 6766); // 6 is the end of the list
        }
        private void VerifyCustomIndicies() {
            VerifyParentRelation(0, 1); // 1,2
            VerifyParentRelation(2, 3); // 3,4, careful that 4 & 5 are one folder appart
            VerifyMultipleParentRelation(4, Enumerable.Range(5, 22 - 6 + 1).ToList()); // 5, [6 to 22] inclusive
        }
        #endregion

        #region Deletion

        private void SetupAndInsertAllFiles() {
            FixAllSampleFiles();
            fc.BulkAddFiles(sampleFiles);
        }

        [Fact]
        public void DeleteFileAndItsRelations_1() {
            lock (fc) {
                SetupAndInsertAllFiles();
                DeleteFileAtLine(5);
                Assert.Equal(-1, fc.GetFileId(sampleFiles[5 - 1]));

                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[6 - 1]));
                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[7 - 1]));
                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[8 - 1]));

                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[6 - 1]));
                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[7 - 1]));
                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[8 - 1]));
                CleanupTables();
            }
        }
        [Fact]
        public void DeleteFileAndItsRelations_2() {
            lock (fc) {
                SetupAndInsertAllFiles();
                DeleteFileAtLine(22); // has parent and children
                Assert.Equal(-1, fc.GetFileId(sampleFiles[22 - 1]));
                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[23 - 1]));
                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[24 - 1]));
                Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[25 - 1]));

                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[23 - 1]));
                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[24 - 1]));
                Assert.NotEqual(-1, fc.GetFileId(sampleFiles[25 - 1]));

                List<(string, int)> childNamesAndID = fc.GetFileChildren(sampleFiles[5 - 1]);
                foreach (var nameAndID in childNamesAndID)
                    Assert.NotEqual(nameAndID.Item1, sampleFiles[22 - 1]);

                CleanupTables();
            }
        }

        private void DeleteFileAtLine(int line) {
            fc.DeleteFileOnly(sampleFiles[line - 1]);
        }

        [Theory]
        [InlineData(5, new int[] { 5, 6, 7, 8, 22, 23, 24, 94 }, new int[] { 6830, 1, 2, 3 })]
        [InlineData(22, new int[] { 22, 23, 24 }, new int[] { 5, 6, 7, 8, 94, 6830, 1, 2, 3 })]
        [InlineData(3, new int[] { 5, 6, 7, 8, 22, 23, 24, 94, 3, 4, 6830, 10, 100, 153, 5043 }, new int[] { 1, 2 })]
        public void DeleteFileAndChildren(int fileLineToDelete, int[] fileLinesDeleted, int[] fileLinesUnaffected) {
            lock (fc) {
                SetupAndInsertAllFiles();
                DeleteFileAndChildrenAtLine(fileLineToDelete);
                foreach (int line in fileLinesDeleted)
                    Assert.Equal(-1, fc.GetFileId(sampleFiles[line - 1]));
                foreach (int line in fileLinesUnaffected)
                    Assert.NotEqual(-1, fc.GetFileId(sampleFiles[line - 1]));

                CleanupTables();
            }
        }

        private void DeleteFileAndChildrenAtLine(int line) {
            fc.DeleteDirectory(sampleFiles[line - 1]);
        }

        [Fact]
        public void DeleteListOfFiles() {
            SetupAndInsertAllFiles();
            List<string> filesToDelete = new();
            filesToDelete.Add(sampleFiles[5 - 1]);
            filesToDelete.Add(sampleFiles[22 - 1]);
            filesToDelete.Add(sampleFiles[79 - 1]);
            filesToDelete.Add(sampleFiles[6792 - 1]);
            fc.DeleteFiles(filesToDelete);

            Assert.Equal(-1, fc.GetFileId(sampleFiles[5 - 1]));
            Assert.Equal(-1, fc.GetFileId(sampleFiles[22 - 1]));
            Assert.Equal(-1, fc.GetFileId(sampleFiles[79 - 1]));
            Assert.Equal(-1, fc.GetFileId(sampleFiles[6792 - 1]));

            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[6 - 1]));
            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[7 - 1]));
            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[8 - 1]));

            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[6 - 1]));
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[7 - 1]));
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[8 - 1]));

            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[23 - 1]));
            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[24 - 1]));
            Assert.Equal(-1, fc.GetFilesParentID(sampleFiles[25 - 1]));

            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[23 - 1]));
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[24 - 1]));
            Assert.NotEqual(-1, fc.GetFileId(sampleFiles[25 - 1]));

            Assert.NotEqual(-1, fc.GetFilesParentID(sampleFiles[6791 - 1]));


            CleanupTables();
        }
        [Fact]
        public void DeleteDrivesInTable() {
            SetupAndInsertAllFiles();
            List<string> filesToDelete = new();
            filesToDelete.Add(sampleFiles[1 - 1]);
            filesToDelete.Add(sampleFiles[3 - 1]);
            fc.DeleteDirectories(filesToDelete);
            Assert.Equal(0, fc.CountFiles());
            CleanupTables();
        }
        #endregion

        [Theory]
        [InlineData(65, @"H:\MusicProj+Song\GuitarPro\GP2\Mysongbook.Guitar.Pro.January.2006(55294.tabs) - Copy",
            new int[] { 65, 66, 67, 2000, 3000, 4000, 5000, 6000 }, new int[] { 65, 66, 67, 2000, 3000, 4000, 5000, 6000 })]
        [InlineData(5, @"H:\MusicProj+Song\GuitarPro\GP2",
            new int[] {}, new int[] { 1, 2, 3, 4, 6795 })]
        public void RenameOrFixFilePathAndChildren(int lineIndex, string newPath, int[] equal, int[]notEqual) {
            lock (fc) {
                SetupAndInsertAllFiles();
                // Can't rename to an existing DB file
                string oldPath = sampleFiles[lineIndex - 1];
                fc.ReAdjustFilepathBulk(oldPath, newPath);
                foreach (int index in equal)
                    Assert.Equal(-1, fc.GetFileId(sampleFiles[index - 1]));
                foreach (int index in notEqual) {
                    Utils.LogToOutput(sampleFiles[index - 1]);
                    Assert.NotEqual(-1, fc.GetFileId(sampleFiles[index - 1].Replace(oldPath, newPath)));
                }

                // Add all files, then fix a specific file
                //  fail when new path does not exist, succeed when file exists, succeed in searching for and adding child files

                CleanupTables();
            }
        }
    }
}



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