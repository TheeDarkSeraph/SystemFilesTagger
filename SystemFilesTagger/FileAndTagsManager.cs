using FileTagDB.Controllers;
using FileTagDB.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using SystemFilesTagger;
using SystemFilesTagger.Properties;

namespace FileTagDB {
    public partial class FileAndTagsManager : Form {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Links to a file controller to handle database transactions for files
        /// </summary>
        FileController fc;
        /// <summary>
        /// Links to a tag controller to handle database transactions for tags and file tagging
        /// </summary>
        TagController tc;

        // TODO: Consider adding paging later on...

        #region Variables

        /// <summary>
        /// Contains all the Tags currently in the database
        /// </summary>        
        List<Tag> tagsInDB;
        /// <summary>
        /// Provides a quick tag name to id conversion
        /// </summary>     
        Dictionary<string, int> tagsNameToId = new();

        /// <summary>
        /// Provides a quick id to tag name conversion
        /// </summary>     
        Dictionary<int, string> tagsIdToName = new();

        /// <summary>
        /// Used to quickly refer to the node that the tag is in
        /// </summary>     
        Dictionary<string, TreeNode> tagsNode = new();

        /// <summary>
        /// Used to set checkmark for tags that are applied to at least one of the selected files
        /// </summary>     
        HashSet<string> activeTags = new();

        /// <summary>
        /// Used to set node color for tags that are applied to ALL of the selected files
        /// </summary>     
        HashSet<string> activeTagsContainedInAllSelected = new();

        /// <summary>
        /// Tree node containing the file extensions tags (automatically separated)
        /// </summary>     
        TreeNode fileExtNode;
        /// <summary>
        /// Tree node containing the user defined tags (Except the file extensions)
        /// </summary>     
        TreeNode customTagsNode;

        readonly Color defaultNodeColor = Color.FromArgb(224, 224, 224);
        readonly Color someSelectedNodeColor = Color.YellowGreen;

        /// <summary>
        /// Contains the file paths of the files in list view in the same order
        /// </summary>
        List<string> currentActiveFiles = new();

        Icon defaultFileIcon;
        Icon defaultFolderIcon;

        /// <summary>
        /// Contains current browsing folder or file path
        /// </summary>
        string currentPath = string.Empty;

        /// <summary>
        /// Keeps a history of the browsed pathes
        /// </summary>
        List<string> browsedFiles = new();
        /// <summary>
        /// The index of the current path relative to <c>browsedFiles</c>, 
        /// this helps handle going to previous selections and back again to the most recent
        /// </summary>
        int currentPreviousFile = -1;

        /// <summary>
        /// contains the cached icon for specific extensions, 
        /// contained in <c>Consts.savableExtensions</c> (manually added by developer).
        /// They are loaded once during browsing and cached to be reused later
        /// </summary>
        Dictionary<string, Icon> extensionIcon = new();


        // for threading the tag task
        Task? tagAdjustingTask = null;
        CancellationTokenSource tagAdjCancelTokenSource;
        const int dblClickSleepTimeMillis = 400;
        bool fileListView_isDoubleClick = false;

        #endregion

        public FileAndTagsManager() {
            InitializeComponent();

            fc = new();
            tc = new(fc);
            tagsInDB = tc.GetAllTags();
            SetUpNodes();
            foreach (Tag tag in tagsInDB) {
                tagsIdToName.Add(tag.id, tag.name);
                tagsNameToId.Add(tag.name, tag.id);
            }

            tagFilterTextBox.Text = string.Empty;
            tagFilterTextBox.TextChanged += OnFilterBoxTextChanged;
            FilterNodes(string.Empty);

            tagTree.AfterCheck += NodeChecked_AfterCheck;

            SetupPathBoxShortcuts();
            
            Bitmap theBitmap = new Bitmap(Resources.Close, new Size(32, 32));
            IntPtr Hicon = theBitmap.GetHicon();// Get an Hicon for myBitmap.
            defaultFileIcon = Icon.FromHandle(Hicon);// Create a new icon from the handle.
            try {
                defaultFolderIcon = GetFolderIcon(IconSize.Large, FolderType.Open);
            } catch { defaultFolderIcon = defaultFileIcon; }
            fileListView.MouseDoubleClick += FileItem_MouseDoubleClick;
            currentPathTextBox.OnShortcutChosen += CallGoToPath;

            //fileViewList.SelectedIndexChanged += FilesSelectedChanged;
            fileListView.MouseUp += FileListView_MouseUp;
            tagTextBox.KeyDown += TagTextBox_EnterKeyDown;
            tagSearchBox.KeyDown += SearchTagBox_EnterKeyDown;
            StartPosition = FormStartPosition.CenterScreen;
        }

        #region Handling the treeview nodes and their functionallity
        private void SetUpNodes() {
            fileExtNode = tagTree.Nodes.Add(Consts.fileExtensions);
            customTagsNode = tagTree.Nodes.Add(Consts.customTags);
        }
        private void FilterNodes(string filterWord) {

            Point scrollOffset = GetTreeViewScrollPos(tagTree);
            fileExtNode.Nodes.Clear();
            customTagsNode.Nodes.Clear();
            tagsNode.Clear();
            foreach (Tag tag in tagsInDB) {
                if (!MatchesFilter(filterWord, tag.name))
                    continue;
                AddNode(tag.name);
            }
            tagTree.Sort();
            SetTreeViewScrollPos(tagTree, scrollOffset);
        }
        private void AddNode(string tagName) {

            if (tagsNode.ContainsKey(tagName))
                return;
            TreeNode node;
            if (tagName[0] == '.') {
                node = fileExtNode.Nodes.Add(tagName);
            } else {
                node = customTagsNode.Nodes.Add(tagName);
            }
            tagsNode.Add(tagName, node);
            UpdateNodeCheckedStatus(node, tagName);

        }
        bool nodeCheckIsUserApplied = true;
        private void UpdateNodeCheckedStatus(TreeNode node, string tagName) {
            nodeCheckIsUserApplied = false;
            //tagTree.AfterCheck -= NodeChecked_AfterCheck; // for some reason, it is not being removed...?
            if (activeTags.Contains(tagName)) {
                node.Checked = true;
                node.ForeColor = activeTagsContainedInAllSelected.Contains(tagName) ? defaultNodeColor : someSelectedNodeColor;
            } else {
                node.Checked = false;
                node.ForeColor = defaultNodeColor;
            }
            nodeCheckIsUserApplied = true;
            //tagTree.AfterCheck += NodeChecked_AfterCheck;
        }

        private void OnFilterBoxTextChanged(object? Sender, EventArgs e) {
            if (Regex.IsMatch(tagFilterTextBox.Text, @"\s+")) {
                tagFilterTextBox.Text = Regex.Replace(tagFilterTextBox.Text, @"\s+", ""); // will recall text changed
            } else {
                FilterNodes(tagFilterTextBox.Text);
            }
        }

        private bool MatchesFilter(string filterWord, string tagWord) {
            return tagWord.Contains(filterWord);
        }

        private void NodeChecked_AfterCheck(object? sender, TreeViewEventArgs e) {
            if (!nodeCheckIsUserApplied)
                return;


            TreeNode? currentNode = e.Node;
            if (currentNode == null)
                return;

            if (currentNode.Text == Consts.fileExtensions || currentNode.Text == Consts.customTags) {
                MessageBox.Show("Checking/Unchecking parent categories does not do anything, " +
                    "they are here because they look bad in the child nodes in winforms if removed with the work around",
                    "Please don't do that, it is ineffective", MessageBoxButtons.OK);
                return;
            }
            bool isChecked = currentNode.Checked;

            if (isChecked) {
                activeTags.Add(currentNode.Text);
                activeTagsContainedInAllSelected.Add(currentNode.Text);
                TagSelectedFiles(tagsNameToId[currentNode.Text]);
            } else {
                activeTags.Remove(currentNode.Text);
                activeTagsContainedInAllSelected.Remove(currentNode.Text);
                UntagSelectedFiles(tagsNameToId[currentNode.Text]);
            }
            UpdateNodeCheckedStatus(currentNode, currentNode.Text);
        }
        #endregion


        #region Taging and untagging Selected files
        private void TagSelectedFiles(int tagId) {
            // Now we can implement it... // get selected indicies
            if (fileListView.SelectedIndices.Count < 1)
                return;
            List<string> filesToTag = GetSelectedFiles(tagRecursivelyCheck.Checked);
            fc.BulkAddFiles(filesToTag);
            List<int> fileIds = fc.GetFilesIds(filesToTag);
            tc.TagFiles(tagId, fileIds);
            TagFilesWithTheirExtensions(fileIds, filesToTag);
        }
        private List<string> GetSelectedFiles(bool recusrive) {
            List<string> filesToTag = new();
            if (recusrive) {
                foreach (int i in fileListView.SelectedIndices)
                    AddInnerFilesToList(filesToTag, currentActiveFiles[i]);
            } else {
                foreach (int i in fileListView.SelectedIndices)
                    filesToTag.Add(currentActiveFiles[i]);
            }
            return filesToTag;
        }
        private void AddInnerFilesToList(List<string> filesToTag, string path) {
            filesToTag.Add(path);
            if ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory) {
                string[] files = Directory.GetFileSystemEntries(path);
                foreach (string file in files)
                    AddInnerFilesToList(filesToTag, file);
            }
        }
        private void TagFilesWithTheirExtensions(List<int> fileIds, List<string> filePaths) {
            // First build a set of tags, and tag to file dictionary
            HashSet<string> extTags = new();
            Dictionary<string, List<int>> filesWithTag = new();
            for (int i = 0; i < filePaths.Count; i++) {
                if (!File.Exists(filePaths[i])) // not a file I think
                    continue;
                string? ext = Utils.GetFileExtension(filePaths[i]);
                if (ext == null)
                    continue;
                if (!extTags.Contains(ext)) {
                    extTags.Add(ext);
                    filesWithTag.Add(ext, new List<int>());
                }
                filesWithTag[ext].Add(fileIds[i]);
            }
            bool refreshNodeList = false;
            foreach (string tag in extTags) {
                int tagId = -1;
                if (!tagsNameToId.ContainsKey(tag)) {
                    tagId = tc.CreateTag(tag);
                    tagsInDB.Add(new(tagId, tag));
                    tagsNameToId.Add(tag, tagId);
                    tagsIdToName.Add(tagId, tag);
                    refreshNodeList = true;
                }
                tagId = tagsNameToId[tag];
                tc.TagFiles(tagId, filesWithTag[tag]);
            }
            if (refreshNodeList)
                FilterNodes(tagFilterTextBox.Text);
            AdjustSelectedFilesTags();
        }
        private void UntagSelectedFiles(int tagId) {
            if (fileListView.SelectedIndices.Count < 1)
                return;
            List<string> filesToTag = GetSelectedFiles(tagRecursivelyCheck.Checked);
            List<int> fileIds = fc.GetFilesIds(filesToTag);
            tc.UntagFiles(tagId, fileIds);
        }
        #endregion

        #region Adding and removing tags
        private void TagTextBox_EnterKeyDown(object? sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                AddTag_Clicked(sender, e);
        }
        private void AddTag_Clicked(object? sender, EventArgs e) {
            string tagToAdd = tagTextBox.Text.Trim();
            //if (tagToAdd[0]=='.') {
            //    MessageBox.Show("For organizing purposes, you can't start a tag with a dot character '.'",
            //        "Invalid operation", MessageBoxButtons.OK);
            //    return;
            //}
            if (Regex.IsMatch(tagToAdd, @"\s+") || tagToAdd.Contains("+")) {
                MessageBox.Show("Tags can't have spaces or '+', as they are used for tag search",
                    "Invalid operation", MessageBoxButtons.OK);
                return;
            }
            if (tagToAdd[0] == '-') {
                MessageBox.Show("Tags can't start with '-' as it is a special format in tag search",
                    "Invalid operation", MessageBoxButtons.OK);
                return;
            }
            if (tagToAdd.Contains("~") || tagToAdd.Contains("*")) {
                MessageBox.Show("~ is a character wild card and * is a sequence wild card, they can't be used",
                    "Cant add tag", MessageBoxButtons.OK);
                return;
            }
            if (tagsNameToId.ContainsKey(tagToAdd)) {
                MessageBox.Show("Tag already exists",
                    "Cant add tag", MessageBoxButtons.OK);
                return;
            }

            int id = tc.CreateTag(tagToAdd);
            if (id == -1) {
                MessageBox.Show("Tag already exists, but there is a bug having it not shown Report code #1001\n" +
                    "try restarting the program, the tag should already exist if not report the above code",
                    "Cant add tag", MessageBoxButtons.OK);
                return;
            }
            tagsInDB.Add(new Tag(id, tagToAdd));
            tagsIdToName[id] = tagToAdd;
            tagsNameToId[tagToAdd] = id;
            FilterNodes(tagFilterTextBox.Text);
        }
        private void RemoveTag_Clicked(object sender, EventArgs e) {

            string tagToRemove = tagTextBox.Text.Trim();
            if (tagToRemove.Length == 0)
                return;


            if (Regex.IsMatch(tagToRemove, @"\s+") || tagToRemove.Contains("+")) {
                MessageBox.Show("Tags can't have spaces or '+', as they are used for tag search",
                    "Invalid operation", MessageBoxButtons.OK);
                return;
            }
            if (tagToRemove[0] == '-') {
                MessageBox.Show("Tags can't start with '-' as it is a special format in tag search",
                    "Invalid operation", MessageBoxButtons.OK);
                return;
            }
            if (!tagsNameToId.ContainsKey(tagToRemove)) {
                MessageBox.Show("Tag does not exist",
                    "Cant Remove tag", MessageBoxButtons.OK);
                return;
            }
            DeleteTag(tagToRemove);
        }
        private void DeleteTag(string tagToRemove) {
            tc.DeleteTag(tagsNameToId[tagToRemove]);
            for (int i = 0; i < tagsInDB.Count; i++) {
                if (tagsInDB[i].name == tagToRemove) {
                    tagsInDB.RemoveAt(i);
                    break;
                }
            }
            tagsIdToName.Remove(tagsNameToId[tagToRemove]);//get id and remove it first
            tagsNameToId.Remove(tagToRemove);
            FilterNodes(tagFilterTextBox.Text);
        }
        private void DeleteHighlightedTag_Clicked(object sender, EventArgs e) {
            TreeNode? node = tagTree.SelectedNode;
            if (node == null)
                return;
            string tagToDelete = node.Text;
            DeleteTag(tagToDelete);
        }
        #endregion

        private void CleanUpDB_Clicked(object sender, EventArgs e) {
            tc.RemoveFilesWithoutTags();
        }


        #region Adjusting tags to match those on selected files (while considering double click)
        private async void FileListView_MouseDown(object? sender, EventArgs e) {
            await CancelOrFinishTagAdjustment();
        }
        private async void FileListView_MouseUp(object? sender, EventArgs e) {
            if (fileListView_isDoubleClick) {
                fileListView_isDoubleClick = false;
                return;
            }
            await CancelOrFinishTagAdjustment(); //gets cancelled in double click
            tagAdjCancelTokenSource = new CancellationTokenSource(); // previous was marked canceled from the above
            tagAdjustingTask = Task.Run(() => AdjustTagsThread(tagAdjCancelTokenSource), tagAdjCancelTokenSource.Token);
            if (fileListView.SelectedIndices.Count > 0) {
                selectedFileLabel.Text = "File: " + currentActiveFiles[fileListView.SelectedIndices[0]];
                fixMissingFileBtn.Enabled = fileListView.SelectedIndices.Count == 1 &&
                    !File.Exists(currentActiveFiles[fileListView.SelectedIndices[0]]) &&
                    !Directory.Exists(currentActiveFiles[fileListView.SelectedIndices[0]]);
            }
        }
        private async Task CancelOrFinishTagAdjustment() {
            if (tagAdjustingTask == null)
                return;
            try {
                if (tagAdjCancelTokenSource != null)
                    tagAdjCancelTokenSource.Cancel();
                await tagAdjustingTask;
            } catch (Exception exc) {
                Debug.WriteLine("canceling or finishing tag Exception " + exc.Message);
            }
        }
        private async void AdjustTagsThread(CancellationTokenSource tokenSource) {
            try {
                await Task.Delay(dblClickSleepTimeMillis, tokenSource.Token);
                fileListView.Invoke(AdjustSelectedFilesTags);
                Debug.WriteLine("Invokation fired!");
            } catch (Exception exc) {
                Debug.WriteLine("canceling delay Exception " + exc.Message);
            }
        }
        private void AdjustSelectedFilesTags() {

            Point scrollOffset = GetTreeViewScrollPos(tagTree);
            activeTags.Clear();
            activeTagsContainedInAllSelected.Clear();
            if (fileListView.SelectedIndices.Count < 1) {
                FilterNodes(tagFilterTextBox.Text);
                return;
            }
            List<string> filesSelected = GetSelectedFiles(false);
            List<List<int>> filesTagIds = tc.GetFilesTags(filesSelected);
            HashSet<int> allTags = new();
            HashSet<int> tagsInAll = new();
            HashSet<int> fileTagIds = new();
            foreach (int tagId in filesTagIds[0])
                tagsInAll.Add(tagId);
            allTags.UnionWith(tagsInAll);
            for (int i = 1; i < filesTagIds.Count; i++) {
                foreach (int tagId in filesTagIds[i])
                    fileTagIds.Add(tagId);
                allTags.UnionWith(fileTagIds);
                tagsInAll.IntersectWith(fileTagIds);
                fileTagIds.Clear();
            }
            foreach (int tagId in allTags)
                activeTags.Add(tagsIdToName[tagId]);
            foreach (int tagId in tagsInAll)
                activeTagsContainedInAllSelected.Add(tagsIdToName[tagId]);
            FilterNodes(tagFilterTextBox.Text);
            SetTreeViewScrollPos(tagTree, scrollOffset);
        }
        #endregion



        private void ClearTagFiler(object sender, EventArgs e) {
            tagFilterTextBox.Text = string.Empty; // will auto call on text changed
        }

        #region Setting up the path shortcuts
        private void SetupPathBoxShortcuts() {
            List<(string, string)> shortNameAndPath = new();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives) {
                shortNameAndPath.Add(new(d.Name + " Drive", d.Name));
            }
            (string, string)[] envPaths = new (string, string)[] {
                new ("Desktop",Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)),
                new ("User",Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)),
                new ("MyDocuments",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                new ("MyMusic", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
                new ("MyPictures", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
                new ("MyVideos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
                new ("ProgramFiles",Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
                new ("Programs",Environment.GetFolderPath(Environment.SpecialFolder.Programs)),
                new ("StartMenu",Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)),
            };
            foreach (var pathGroup in envPaths) {
                if (pathGroup.Item2 != string.Empty) {
                    shortNameAndPath.Add(new(pathGroup.Item1, pathGroup.Item2));
                }
            }
            currentPathTextBox.shortNFullPath = shortNameAndPath;
        }
        #endregion

        #region File browsing and navigation
        private void GoToPreviousFile_BtnClicked(object sender, MouseEventArgs e) {
            isSearchingTags = false;

            currentPreviousFile--;
            previousFileBtn.Enabled = currentPreviousFile > 0;
            nextFileBtn.Enabled = true;
            LoadDirectoryInListView(browsedFiles[currentPreviousFile]);
        }
        private void GoToNextFile_BtnClicked(object sender, MouseEventArgs e) {
            isSearchingTags = false;

            previousFileBtn.Enabled = true;
            currentPreviousFile++;
            LoadDirectoryInListView(browsedFiles[currentPreviousFile]);
            if (currentPreviousFile == browsedFiles.Count - 1) // there is no more next file
                nextFileBtn.Enabled = false;
        }
        private void CallGoToPath() {
            GoToPath(null, new());
        }

        private void GoToPath(object? sender, EventArgs e) {
            isSearchingTags = false;

            currentPathTextBox.Text = currentPathTextBox.Text.Trim();
            if (currentPathTextBox.Text == string.Empty) {
                MessageBox.Show("Can't go to an empty path", "Empty Path", MessageBoxButtons.OK);
                return;
            }
            string path = currentPathTextBox.Text;
            BrowseDirectory(path);
        }

        private void BrowseDirectory(string path) {
            isSearchingTags = false;

            if (!File.Exists(path) && !Directory.Exists(path)) {
                MessageBox.Show("Invalid path, maybe you are lacking permission to view file", "Invalid Path", MessageBoxButtons.OK);
                return;
            }
            if (currentPath != string.Empty && browsedFiles.Count > 1) {
                if (currentPreviousFile + 1 < browsedFiles.Count) {
                    browsedFiles.RemoveRange(currentPreviousFile + 1, browsedFiles.Count - currentPreviousFile - 1);
                }
            }
            browsedFiles.Add(path);
            currentPreviousFile = browsedFiles.Count - 1;
            nextFileBtn.Enabled = false;
            previousFileBtn.Enabled = currentPreviousFile > 0;
            LoadDirectoryInListView(path);
        }
        private void LoadDirectoryInListView(string path) {
            currentPathTextBox.Text = path;
            currentPath = path;
            goToParentButton.Enabled = true;
            currentActiveFiles.Clear();
            if (Directory.Exists(path)) {
                foreach (string item in Directory.GetFileSystemEntries(path)) {
                    currentActiveFiles.Add(item);
                }
            } else {
                currentActiveFiles.Add(path);
            }
            ReadjustFileList();
        }
        private void ReadjustFileList() {
            fileListView.Clear();
            fileIconsImageList.Images.Clear();
            int filesAdded = 0;
            foreach (string filepath in currentActiveFiles) {
                Icon? fileIcon = GetFileIcon(filepath);
                if (fileIcon == null)
                    fileIcon = defaultFileIcon;
                fileIconsImageList.Images.Add(fileIcon);
                FileInfo fi = new FileInfo(filepath);
                fileListView.Items.Add(fi.Name, filesAdded);
                filesAdded++;
            }
            AdjustSelectedFilesTags();
        }
        //  NOTE: fires before mouse up
        private async void FileItem_MouseDoubleClick(object? sender, EventArgs e) {
            fileListView_isDoubleClick = true;
            await CancelOrFinishTagAdjustment();
            if (fileListView.SelectedItems.Count < 1)
                return;
            string filepath = currentActiveFiles[fileListView.SelectedItems[0].Index];

            if (!File.Exists(filepath) && !Directory.Exists(filepath)) {
                MessageBox.Show("The file chosen does not exists, nothing to do here.", "Invalid operation", MessageBoxButtons.OK);
                return;
            }
            if (Directory.Exists(filepath) && browseFolderRadioBtn.Checked) {
                BrowseDirectory(filepath);
                return;
            }


            string shortcutTarget = string.Empty;
            if (filepath.EndsWith(".lnk")) {
                shortcutTarget = Utils.GetShortcutTarget(filepath);
            }

            if (shortcutTarget != null && shortcutTarget != string.Empty) { // is shortcut
                Debug.WriteLine("Target link " + shortcutTarget);
                string? parent = FileController.GetParentPath(shortcutTarget);
                if (parent != null && parent != string.Empty) {
                    BrowseDirectory(parent);
                } else {
                    if (!File.Exists(shortcutTarget) && !Directory.Exists(shortcutTarget))
                        Process.Start("explorer.exe", filepath);
                    else {
                        BrowseDirectory(shortcutTarget);
                    }
                }
            } else {
                Process.Start("explorer.exe", filepath); // will try to open the file
                //Process.Start("explorer.exe", "/select,"+filepath); 
                // this selects the file (this is not useful since we give user clipboard...)
            }
        }
        private void GoToParent_Clicked(object sender, MouseEventArgs e) {
            if (currentPath != string.Empty) {
                string? parent = FileController.GetParentPath(currentPath);
                if (parent != null && parent != string.Empty) {
                    BrowseDirectory(parent);
                } else {
                    MessageBox.Show("Unable to go to parent folder", "No parent folder", MessageBoxButtons.OK);
                }
            }
        }
        private void SelectFolderToBrowseTo(object sender, EventArgs e) {
            FolderBrowserDialog fbd = new();
            if (fbd.ShowDialog() == DialogResult.OK) {
                BrowseDirectory(fbd.SelectedPath);
            }
        }

        #endregion





        #region Searching tagged files

        bool isSearchingTags = false;
        string lastTagSearched = "";
        private void SearchTagBox_EnterKeyDown(object? sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter)
                SearchTag_Clicked(sender, e);
        }
        private void SearchTag_Clicked(object? sender, EventArgs e) {
            if (!CheckTagQueryValidity())
                return;
            currentPathTextBox.Text = "*Tag Search*";
            isSearchingTags = true;
            lastTagSearched = tagSearchBox.Text;
            List<(string, int)> fileData = tc.GetFilesWithTagQuery(tagSearchBox.Text, ignoreCaseCheck.Checked);
            currentActiveFiles.Clear();
            foreach ((string, int) fileEntry in fileData)
                currentActiveFiles.Add(fileEntry.Item1);
            FilterNodes(tagFilterTextBox.Text);
            ReadjustFileListTagMode();
        }
        private void ReadjustFileListTagMode() {
            fileListView.Clear();
            fileIconsImageList.Images.Clear();
            int filesAdded = 0;
            foreach (string filepath in currentActiveFiles) {
                Icon? fileIcon = GetFileIcon(filepath);
                if (fileIcon == null)
                    fileIcon = defaultFileIcon;
                fileIconsImageList.Images.Add(fileIcon);
                ListViewItem? item = fileListView.Items.Add(Utils.ShortenFileName(filepath), filesAdded);

                if (!File.Exists(filepath) && !Directory.Exists(filepath))
                    item.ForeColor = Color.DarkRed;

                filesAdded++;
            }
            AdjustSelectedFilesTags();
        }
        private bool CheckTagQueryValidity() {
            tagSearchBox.Text = Regex.Replace(tagSearchBox.Text, @"\s+"," ").Trim();
            string query = tagSearchBox.Text;
            if (query == string.Empty)
                return false;
            string[] parts = Regex.Split(query, @"\s+");
            bool validTags = true;
            string errorMsg = "";
            foreach (string tag in parts) {
                if (tag[0] == '+') {
                    errorMsg = "Can't start with '+'";
                    validTags = false;
                    break;
                }
                if (tag.Contains("++") || tag.Contains("-+") || tag.Contains("**")) {
                    errorMsg = "Can't have '++' or '-+' or '**'";
                    validTags = false;
                    break;
                }
                if (tag[0] == '-') {
                    if (tag.Length == 1) {
                        errorMsg = "'-' can't be a tag";
                        validTags = false;
                        break;
                    }
                    if (tag[1] == '-') {
                        errorMsg = "Can't start with --";
                        validTags = false;
                        break;
                    }
                    if (tag.Contains("+")) {
                        errorMsg = "Exclusions can't contain '+' because it is meaningless";
                        validTags = false;
                        break;
                    }
                }
            }
            if (!validTags) {
                MessageBox.Show(errorMsg, "Invalid search, please fix", MessageBoxButtons.OK);
            }
            return validTags;
        }


        //void BrowseFilesInFolder() {
        //    currentPathTextBox.Text = "path/";
        //    // load files under the folder directly if it is a folder and the folder exists    
        //    // otherwise just show a copyable link path
        //}
        //void BrowseFilesInPath() {
        //    currentPathTextBox.Text = "path/*";
        //    // load files under the folder's path if it is a folder and the folder exists
        //    // otherwise just show a copyable link path
        //}


        #endregion

        private void CopyToClipboard(object sender, EventArgs e) {
            if (fileListView.SelectedIndices.Count > 0)
                Clipboard.SetText(currentActiveFiles[fileListView.SelectedIndices[0]]);
        }

        private void RemoveFiles_Clicked(object sender, EventArgs e) {
            if (fileListView.SelectedIndices.Count < 1)
                return;
            List<string> filesToTag = GetSelectedFiles(false);
            fc.DeleteFiles(filesToTag);

            if (isSearchingTags) {
                tagSearchBox.Text = lastTagSearched;
                SearchTag_Clicked(sender, e);
            }
            // remove from active files the selected ones...
        }
        private void FixMissingFile_Clicked(object sender, EventArgs e) {
            DialogResult result = MessageBox.Show("Is this supposed to be a file (Not a folder)?", "File or folder", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes) {
                OpenFileDialog ofd = new();
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK) {
                    fc.ReAdjustFilepathBulk(currentActiveFiles[fileListView.SelectedIndices[0]], ofd.FileName);
                    currentActiveFiles[fileListView.SelectedIndices[0]] = ofd.FileName;
                    fixMissingFileBtn.Enabled = false;
                    ReadjustFileListTagMode();
                }
            } else {
                FolderBrowserDialog fbd = new();
                if (fbd.ShowDialog() == DialogResult.OK) {
                    fc.ReAdjustFilepathBulk(currentActiveFiles[fileListView.SelectedIndices[0]], fbd.SelectedPath);
                    currentActiveFiles[fileListView.SelectedIndices[0]] = fbd.SelectedPath;
                    fixMissingFileBtn.Enabled = false;
                    ReadjustFileListTagMode();
                }
            }
            // show dialog to choose a file/folder

            // fixMissingFileBtn = false
        }

        private void Help_Clicked(object sender, EventArgs e) {
            new ProgramGuide().Show();
        }


        #region Handling Icon getting
        /// <summary>
        /// Gets an Icon used to represent the file by the system or a previously defined default
        /// </summary>
        /// <param name="filepath"> The path to the file or directory to get the Icon for</param>
        /// <returns>An icon for the file if it exists, or a default file or folder icon</returns>
        private Icon? GetFileIcon(string filepath) {
            Icon? fileIcon = null;
            try {
                if (Directory.Exists(filepath))
                    return defaultFolderIcon;
                if (!File.Exists(filepath))
                    return defaultFileIcon;

                string? fileExt = Utils.GetFileExtension(filepath);
                if (fileExt != null && Consts.savableExtensions.Contains(fileExt)) {
                    if (!extensionIcon.ContainsKey(fileExt)) {
                        fileIcon = Icon.ExtractAssociatedIcon(filepath);
                        if (fileIcon == null)
                            fileIcon = defaultFileIcon;
                        extensionIcon.Add(fileExt, fileIcon);
                    }
                    fileIcon = extensionIcon[fileExt];
                } else {
                    fileIcon = Icon.ExtractAssociatedIcon(filepath);
                }
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
                fileIcon = File.Exists(filepath) ? defaultFileIcon : defaultFolderIcon;
            }
            return fileIcon;
        }



        //Call function with the path to the folder you want the icon for
        //SHGetFileInfo(
        //    "C:\\Users\\Public\\Music",
        //    0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
        //    SHGFI_ICON | SHGFI_LARGEICON);

        //using (Icon i = System.Drawing.Icon.FromHandle(shinfo.hIcon)) {
        //    //Convert icon to a Bitmap source
        //    ImageSource img = Imaging.CreateBitmapSourceFromHIcon(
        //                            i.Handle,
        //                            new Int32Rect(0, 0, i.Width, i.Height),
        //                            BitmapSizeOptions.FromEmptyOptions());

        //    //WPF Image control
        //    m_image.Source = img;
        //}



        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public enum FolderType {
            Closed,
            Open
        }

        public enum IconSize {
            Large,
            Small
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DestroyIcon(IntPtr hIcon);

        public const uint SHGFI_ICON = 0x000000100;
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        public const uint SHGFI_OPENICON = 0x000000002;
        public const uint SHGFI_SMALLICON = 0x000000001;
        public const uint SHGFI_LARGEICON = 0x000000000;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public static Icon GetFolderIcon(IconSize size, FolderType folderType) {
            // Need to add size check, although errors generated at present!    
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType) {
                flags += SHGFI_OPENICON;
            }
            if (IconSize.Small == size) {
                flags += SHGFI_SMALLICON;
            } else {
                flags += SHGFI_LARGEICON;
            }
            // Get the folder icon    
            var shfi = new SHFILEINFO();

            var res = SHGetFileInfo(@"C:\Windows",
                FILE_ATTRIBUTE_DIRECTORY,
                out shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            if (res == IntPtr.Zero) {
                Exception? e = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                if(e!=null)
                    throw e;
            }
            // Load the icon from an HICON handle  
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup    

            return icon;
        }
        #endregion



        #region Using dark mode somehow 
        // refer to https://stackoverflow.com/questions/11862315/changing-the-color-of-the-title-bar-in-winform

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            const int WM_NCPAINT = 0x85;
            if (m.Msg == WM_NCPAINT) {
                UseImmersiveDarkMode(m.HWnd, true);
                //IntPtr hdc = GetWindowDC(m.HWnd);
                //if ((int)hdc != 0) {
                //    Graphics g = Graphics.FromHdc(hdc);
                //    g.FillRectangle(Brushes.Green, new Rectangle(0, 0, 4800, 23));
                //    g.Flush();
                //    ReleaseDC(m.HWnd, hdc);
                //}
            }
        }
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr,
        ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        internal static bool UseImmersiveDarkMode(IntPtr handle, bool enabled) {
            if (IsWindows10OrGreater(17763)) {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985)) {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }

            return false;
        }

        private static bool IsWindows10OrGreater(int build = -1) {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
        #endregion


        #region Scrollview of treeview
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;

        private Point GetTreeViewScrollPos(TreeView treeView) {
            return new Point(GetScrollPos(treeView.Handle, SB_HORZ),
                GetScrollPos(treeView.Handle, SB_VERT));
        }

        private void SetTreeViewScrollPos(TreeView treeView, Point scrollPosition) {
            SetScrollPos(treeView.Handle, SB_HORZ, scrollPosition.X, true);
            SetScrollPos(treeView.Handle, SB_VERT, scrollPosition.Y, true);
        }
        #endregion
    }
    //void treeview1_DrawNode(object? sender, DrawTreeNodeEventArgs e) {
    //    if (e.Node == null)
    //        return;
    //    e.DrawDefault = true;
    //    if (e.Node.Level == 0) {
    //        e.Node.HideCheckBox(true);
    //    }
    //}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
