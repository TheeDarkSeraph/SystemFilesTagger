using FileTagDB.Controllers;
using FileTagDB.Models;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SystemFilesTagger;
using SystemFilesTagger.FormComp;
using static System.Windows.Forms.LinkLabel;
namespace FileTagDB {
    public partial class FileAndTagsManager : Form {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        FileController fc;
        TagController tc;
        List<Tag> tagsInDB;
        Dictionary<string, int> tagsNameToId = new();
        Dictionary<int, string> tagsIdToName = new();

        Dictionary<string, TreeNode> tagsNode = new();
        Dictionary<string, bool> activeTags = new();

        TreeNode fileExtNode, customTagsNode;

        List<string> currentActiveFiles = new();

        Icon defaultFileIcon;
        Icon defaultFolderIcon;

        string currentPath = string.Empty;
        List<string> browsedFiles = new();
        int currentPreviousFile = -1;

        Dictionary<string, Icon> extensionIcon = new();
        // TODO: tags cant start with a DOT as it is reserved for extensions

        public FileAndTagsManager() {
            InitializeComponent();

            fc = new();
            tc = new(fc);
            tagsInDB = tc.GetAllTags();
            SetUpNodes();
            foreach (Tag tag in tagsInDB) {
                tagsIdToName.Add(tag.id, tag.name);
                tagsNameToId.Add(tag.name, tag.id);
                activeTags.Add(tag.name, false);
            }
            tagFilterTextBox.Text = string.Empty;
            tagFilterTextBox.TextChanged += OnFilterBoxTextChanged;
            FilterNodes(string.Empty);
            tagTree.AfterCheck += NodeChecked_AfterCheck;
            SetupPathBoxShortcuts();
            Bitmap theBitmap = new Bitmap(Image.FromFile("icons/Close.png"), new Size(32, 32));
            IntPtr Hicon = theBitmap.GetHicon();// Get an Hicon for myBitmap.
            defaultFileIcon = Icon.FromHandle(Hicon);// Create a new icon from the handle.
            try {
                defaultFolderIcon = GetFolderIcon(IconSize.Large, FolderType.Open);
            } catch { defaultFolderIcon = defaultFileIcon; }
            fileViewList.MouseDoubleClick += fileItem_MouseDoubleClick;
            currentPathTextBox.OnShortcutChosen += CallGoToPath;
        }
        #region Handling the treeview nodes and their functionallity
        private void SetUpNodes() {
            fileExtNode = tagTree.Nodes.Add(Consts.fileExtensions);
            customTagsNode = tagTree.Nodes.Add(Consts.customTags);
        }
        private void FilterNodes(string filterWord) {
            fileExtNode.Nodes.Clear();
            customTagsNode.Nodes.Clear();
            tagsNode.Clear();
            foreach (Tag tag in tagsInDB) {
                if (!MatchesFilter(filterWord, tag.name))
                    continue;
                TreeNode node;
                if (tag.name[0] == '.') {
                    node = fileExtNode.Nodes.Add(tag.name);
                } else {
                    node = customTagsNode.Nodes.Add(tag.name);
                }
                tagsNode.Add(tag.name, node);
                node.Checked = activeTags[tag.name];
            }
        }
        private void OnFilterBoxTextChanged(object? Sender, EventArgs e) {
            if (Utils.AnyWhiteSpace().IsMatch(tagFilterTextBox.Text)) {
                tagFilterTextBox.Text = Utils.AnyWhiteSpace().Replace(tagFilterTextBox.Text, ""); // will recall text changed
            } else {
                FilterNodes(tagFilterTextBox.Text);
            }
        }

        private bool MatchesFilter(string filterWord, string tagWord) {
            return tagWord.Contains(filterWord);
        }

        private void NodeChecked_AfterCheck(object? sender, TreeViewEventArgs e) {
            if (e.Node == null)
                return;
            TreeNode currentNode = e.Node;
            if (e.Node.Text == Consts.fileExtensions || e.Node.Text == Consts.customTags) {
                MessageBox.Show("Checking/Unchecking parent categories does not do anything, " +
                    "they are here because they look bad in the child nodes in winforms if removed with the work around",
                    "Please don't do that, it is ineffective", MessageBoxButtons.OK);
                return;
            }
            bool isChecked = currentNode.Checked;
            activeTags[currentNode.Text] = currentNode.Checked;
            if (isChecked) {
                TagSelectedFiles(tagsNameToId[currentNode.Text]);
            } else {
                UntagSelectedFiles(tagsNameToId[currentNode.Text]);
            }
            // Use the currentNode and isChecked as needed
            // ...
        }
        #endregion

        private void TagSelectedFiles(int tagId) {
            // Now we can implement it... // get selected indicies
            // TODO: Requires image view for more logic
        }
        private void UntagSelectedFiles(int tagId) {
            // TODO: Requires image view for more logic
        }

        private void AddFile(object sender, EventArgs e) {

        }
        private void AddPath(object sender, EventArgs e) {

        }
        private void RemoveFile(object sender, EventArgs e) {

        }
        private void RemovePath(object sender, EventArgs e) {

        }

        private void OnItemSelected() {
            // TODO: Missing, get file tags(ids), get files tags (all)
            // Has tags in both = union, does not has tag = union, has partial = intersection???
        }


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
        private void GoToPreviousFile(object sender, MouseEventArgs e) {
            currentPreviousFile--;
            previousFileBtn.Enabled = currentPreviousFile > 0;
            nextFileBtn.Enabled = true;
            LoadDirectoryInListView(browsedFiles[currentPreviousFile]);
        }
        private void GoToNextFile(object sender, MouseEventArgs e) {
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
            currentPathTextBox.Text = currentPathTextBox.Text.Trim();
            if (currentPathTextBox.Text == string.Empty) {
                MessageBox.Show("Can't go to an empty path", "Empty Path", MessageBoxButtons.OK);
                return;
            }
            string path = currentPathTextBox.Text;
            BrowseDirectory(path);
        }
        private void BrowseDirectory(string path) {
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
            fileViewList.Clear();
            fileIconsImageList.Images.Clear();
            int filesAdded = 0;
            foreach (string filepath in currentActiveFiles) {
                Debug.WriteLine("HERE ! " + filepath);
                Icon? fileIcon = GetFileIcon(filepath);

                if (fileIcon == null)
                    fileIcon = defaultFileIcon;
                fileIconsImageList.Images.Add(fileIcon);
                FileInfo fi = new FileInfo(filepath);
                fileViewList.Items.Add(fi.Name, filesAdded);
                filesAdded++;
            }
        }
        private void fileItem_MouseDoubleClick(object? sender, EventArgs e) {
            if (fileViewList.SelectedItems.Count < 1)
                return;
            string filepath = currentActiveFiles[fileViewList.SelectedItems[0].Index];
            if (Directory.Exists(filepath) && browseFolderRadioBtn.Checked) {
                BrowseDirectory(filepath);
                return;
            }

            string shortcutTarget = string.Empty;
            if (filepath.EndsWith(".lnk")) {
                shortcutTarget = Utils.GetShortcutTarget(filepath);
            }

            if (shortcutTarget!=null && shortcutTarget != string.Empty) { // is shortcut
                Debug.WriteLine("Target link " + shortcutTarget);
                string? parent = FileController.GetParentPath(shortcutTarget);
                if (parent != null && parent != string.Empty) {
                    BrowseDirectory(parent);
                } else {
                    if(!File.Exists(shortcutTarget)&&!Directory.Exists(shortcutTarget))
                        Process.Start("explorer.exe", filepath);
                    else {
                        BrowseDirectory(shortcutTarget);
                    }
                }
            } else {
                //Process.Start("explorer.exe", filepath); // will try to open the file
                Process.Start("explorer.exe", "/select,"+filepath);
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
        #endregion


        private void SelectFolderToBrowseTo(object sender, EventArgs e) {
            FolderBrowserDialog fbd = new();
            if (fbd.ShowDialog() == DialogResult.OK) {
                BrowseDirectory(fbd.SelectedPath);
            }
        }














        #region Handling Icon getting
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

            if (res == IntPtr.Zero)
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());

            // Load the icon from an HICON handle  
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup    

            return icon;
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
