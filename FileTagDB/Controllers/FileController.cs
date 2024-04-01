using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace FileTagDB.Controllers {
    // TODO: Have custom default tags that are auto added (unmodifiable)
    /* .extension tagging (depends) .3 -4 letters
     * no ext
     * Folders
     * 
     * renaming a 'file' and changing its extension will cause change of file extension
     *  so untag previous tag, then add new tag (folder, no ext, new ext)...
     * .ext tags can be manually removed. This has to be tested...
     * This should all be programatically... and manually adjusted
     * 
     * So adding file and adding extension should be associated (auto create tags)
     * 
     * Maybe we can have the tagging and untaggin of the files separate to avoid confusion
     * So we first insert the file, then we pass it to a function that would auto tag it
     * renaming a 'file' will also cause auto tagging
     * 
     * 
     */
    // TODO: Show start of path, and end of path (file name) in a label below shown files/folders
    // Add folder icon to folders and files to 
    public partial class FileController {
        internal SQLiteConnection conn;
        public static int bulkSeparation = 200;
        public const int expectedPathLength = 70;
        public FileController() {
            conn = DBController.GetDBConnection();
        }

        private void ConnectDB() {
            conn = DBController.GetDBConnection();
            conn.Open();
        }
        private void ActivateForeignKey() {
            try {
                using (SQLiteCommand command = new SQLiteCommand("PRAGMA foreign_keys = ON;", conn)) {
                    command.ExecuteNonQuery();
                }
            } catch (Exception e) {
                Utils.LogToOutput("Error pragma " + e.Message);
            }
        }
        private void DisconnectDB() {
            conn.Dispose();
        }
        // TODO: When adding a folder, process how many files to add first.
        // TODO: Warn user when adding more than 10k files total that it will take some time

        #region Helper Static Functions
        public static string FixFilePath(string filepath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                if (filepath.EndsWith(":")) { filepath += @"\"; }
            return filepath;
        }
        public static string? GetParentPath(string filepath) {
            DirectoryInfo d; // works on non existing files
            try {
                d = new DirectoryInfo(filepath);
            } catch {
                return string.Empty;
            }
            if (d.Parent == null) {
                //Utils.LogToOutput("Folder/File is root : " + filepath);
                return null;
            }
            if (!filepath.Contains(d.Parent.FullName)) {
                //Utils.LogToOutput("This is a relative path :" + d.Parent.FullName + " To " + filepath);
                return string.Empty;
            }
            //Utils.LogToOutput("Parent folder is " + d.Parent.FullName + " ;;of;; " + filepath);
            return d.Parent.FullName;
        }
        #endregion

        // speed clocked at 4874 seconds, which is probably a great improvement (this is with all the logs...
        // 12ms for tree/ 4852ms without the prints
        //V1 9547 V2 When faced with multiple files in same folder BUT parent already exists
        // Conclusion: Grouping roots is faster when the parent is in the DB, but if it isn't, then its the same mostly.
        #region Adding multiple files
        public void BulkAddFilesV1(List<string> p_paths) {
            List<string> paths = new(p_paths);
            for (int i = 0; i < paths.Count; i++)
                paths[i] = FixFilePath(paths[i]);
            FileTree ft = new(paths);
            ConnectDB();
            using (var transaction = conn.BeginTransaction()) {
                //Utils.LogToOutput("Root count of files to be added : "+ft.RootCount);
                for (int i = 0; i < ft.RootCount; i++) {
                    FileNode node = ft[i];
                    int fileID = AddFileDBC(node.Path);
                    if (-1 == fileID)
                        fileID = GetFileIDDBC(node.Path);
                    if (-1 == fileID) {
                        Utils.LogToOutput("Problem inserting file path :" + node.Path);
                        continue;
                    }
                    AddNode(fileID, node);
                }
                transaction.Commit();
            }
            DisconnectDB();
            ft.ClearAll();
            // we need to be smart about this
            // add all children together, and get their IDs together
            // we can return last row ID, and affected rows
        }

        public void BulkAddFiles(List<string> p_paths) {
            //Utils.LogToOutput("COUNT OF FILES " + p_paths.Count);
            List<string> paths = new(p_paths);
            for (int i = 0; i < paths.Count; i++)
                paths[i] = FixFilePath(paths[i]);
            List<FileGroup> groups;
            List<FileNode> nodes;
            FileTree ft = new FileTree(paths);
            //ft.PrintAll();
            (groups, nodes) = FileGroup.MakeFileGroups(ft.GetRootFiles());
            ConnectDB();

            using (var transaction = conn.BeginTransaction()) {
                //Utils.LogToOutput("Root count of files to be added : "+ft.RootCount);
                foreach (FileNode node in nodes) {
                    int fileID = AddFileDBC(node.Path);
                    if (-1 == fileID)
                        fileID = GetFileIDDBC(node.Path);
                    if (-1 == fileID) {
                        Utils.LogToOutput("Problem inserting file path :" + node.Path);
                        continue;
                    }
                    AddNode(fileID, node);
                }
                foreach (FileGroup group in groups) {
                    Utils.LogToOutput("Group log");
                    if (group.parentPath == null)
                        continue;
                    int parentID = GetFileIDDBC(group.parentPath);
                    for (int i = 0; i < group.childNodes.Count; i += bulkSeparation)
                        MultiNodeInsert(group.childNodes, i, Math.Min(bulkSeparation, group.childNodes.Count - i), parentID, -1 != parentID);
                    foreach (FileNode fn in group.childNodes) {
                        int nodeID = GetFileIDDBC(fn.Path);
                        if (-1 == nodeID)
                            Utils.LogToOutput("Possible problem in Group add Parent: " + group.parentPath);
                        AddNode(nodeID, fn);
                    }
                }
                transaction.Commit();
            }
            DisconnectDB();
            ft.ClearAll();
            // we need to be smart about this
            // add all children together, and get their IDs together
            // we can return last row ID, and affected rows
        }
        #endregion

        // we will leave the checking for the external function...
        // Time Taken 34, 67-75, 133 ms per add
        // NOTE: this time is calculated with having to open a connection EVERY time
        //          we have not time tested adding multiple on the same connection
        #region Adding single file
        public int AddFile(string filepath) {
            ConnectDB();
            int lastInsertedRowId = AddFileDBC(filepath);
            DisconnectDB();
            return lastInsertedRowId;
        }
        #endregion

        #region Getting File Data
        public int CountFiles() {
            ConnectDB();
            int count;
            string cmdText = $"SELECT  COUNT(*) FROM {TableConst.filesTName}";
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.CommandText = cmdText;
                count = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Dispose();
            }
            DisconnectDB();
            return (int)((Int64)count);
        }
        public int GetFileID(string filepath) {
            int fileID;
            ConnectDB();
            fileID = GetFileIDDBC(filepath);
            DisconnectDB();
            return fileID;
        }
        public List<int>GetFilesIds(List<string> filepaths) {
            List<int> filesIds = new();
            ConnectDB();
            foreach (string filepath in filepaths)
                filesIds.Add(GetFileIDDBC(filepath));
            DisconnectDB();
            return filesIds;
        }
        public string GetFilePath(int fileID) {
            ConnectDB();
            string result = GetFilePathDBC(fileID);
            DisconnectDB();
            return result;
        }
        public List<(string, int)> GetFileChildren(string filepath) {
            ConnectDB();
            int fileID = GetFileIDDBC(filepath);
            if (-1 == fileID) {
                DisconnectDB();
                return new();
            }
            List<(string, int)> fileRows = GetFileChildrenDBC(fileID);
            DisconnectDB();
            return fileRows;
        }
        public List<(string, int)> GetFileChildren(int fileID) {
            ConnectDB();
            List<(string, int)> fileRows = GetFileChildrenDBC(fileID);
            DisconnectDB();
            return fileRows;
        }
        public List<(string, int)> GetFilesWithPath(string path) { // for use with tag folder insertions (mostly only)
            ConnectDB();
            List<(string, int)> fileRows = GetFilesWithPathDBC(path);
            DisconnectDB();
            return fileRows;
        }
        public int GetFilesParentID(string filepath) {
            string? parentPath = GetParentPath(filepath);
            if (parentPath == null || parentPath == string.Empty)
                return -1;
            return GetFileID(parentPath);
        }
        #endregion


        /*
         * This will be faster because it will rename all, since all child files
         *  will have the same parent...
         */
        #region Updating a filepath
        public void ReAdjustFilepathBulk(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            ConnectDB();
            int fileID = GetFileIDDBC(filepath);
            if (-1 == fileID) {
                DisconnectDB();
                return;
            }
            if (GetParentPath(newPath)!=GetParentPath(filepath)) // because it possibly could be just a name change
                DeleteParentLinkDBC(fileID);
            RenameFilePathDBC(filepath, newPath); // we will only change paths, IDs and file to file rel are same
            DisconnectDB();
            // Recursive fix
        }
        #endregion

        #region File deletion
        public void DeleteFileOnly(string filepath) {
            ConnectDB();
            ActivateForeignKey();
            DeleteFileDBC(filepath);
            DisconnectDB();
        }
        // NOTE: deleting directory will delete everything under it, whether they are connected or not
        // And this is intended behaviour. You are removing the folder and everything below from the system
        //   if there is a folder that was not included but its subfiles/folders were included they will get Exodiad
        public void DeleteDirectory(string filepath) {
            ConnectDB();
            ActivateForeignKey();
            DeleteDirectoryDBC(filepath);
            DisconnectDB();
        }
        public void DeleteFiles(List<string> filepaths) {
            ConnectDB();
            ActivateForeignKey();
            foreach (string path in filepaths)
                DeleteFileDBC(path);
            DisconnectDB();
        }
        public void DeleteDirectories(List<string> filepaths) {
            ConnectDB();
            foreach (string path in filepaths)
                DeleteDirectoryDBC(path);
            DisconnectDB();
        }
        #endregion

        /* Could be slow, Won't be used because will potentially be VERY slow
         * Renaming all is faster than getting every ID and child id and renaming it one by one
         * Spending multiple hierarchical search queries and updates (amount of updates = # of all files to update)
         */
        #region Old renaming method (DO NOT USE)
        internal void ReAdjustFileLocation(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileID(filepath);
            if (-1 == fileID) return;
            ConnectDB();
            if (GetParentPath(newPath) != GetParentPath(filepath)) // because it possibly could be just a name change
                DeleteParentLinkDBC(fileID);
            UpdateFileName(fileID, newPath);
            UpdateChildrenToNewPath(fileID, filepath, newPath);
            DisconnectDB();
            // Recursive fix
        }
        internal void UpdateFileName(int fileID, string newPath) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$newPath", newPath);
                DBController.ExecuteNonQCommand(cmd, $"UPDATE {TableConst.filesCoID} SET {TableConst.filesCoPath} = $newPath " +
                    $"WHERE {TableConst.filesCoID} = {fileID};");
            }
        }
        internal void UpdateChildrenToNewPath(int fileID, string filepath, string newName) {
            using (var cmd = new SQLiteCommand(conn)) {
                // adjust self, then adjust children
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                @$"SELECT {TableConst.fileChildsCID} FROM {TableConst.fileChildsTName}
                        WHERE {TableConst.fileChildsFID} = {fileID}
                    ");
                while (reader.Read()) {
                    int childID = Convert.ToInt32(reader[$"{TableConst.fileChildsCID}"]);
                    string childPath = GetFilePathDBC(childID);
                    ReAdjustChildFileLocation(cmd, filepath, newName, childPath, childID);
                }
            }
        }
        internal void ReAdjustChildFileLocation(SQLiteCommand cmd, string parentPrev, string parentNew, string myPath, int myID) {
            string myNewPath = myPath.Replace(parentPrev, parentNew);
            UpdateFileName(myID, myNewPath);
            UpdateChildrenToNewPath(myID, myPath, myNewPath);
        }
        #endregion


        #region Helper classes for managing file tree
        internal class FileGroup {
            public string? parentPath;
            public List<FileNode> childNodes = new();
            FileGroup(string? path,FileNode firstNode) {
                parentPath = path;
                childNodes.Add(firstNode);
            }
            public static (List<FileGroup>,List<FileNode>) MakeFileGroups(List<FileNode> rootFiles) {
                Dictionary<string, FileGroup> filesGroup = new();
                List<FileGroup> allGroups = new();
                List<FileNode> volume = new();
                foreach (FileNode rootFile in rootFiles) {
                    string path = rootFile.Path;
                    string? parentPath = FileController.GetParentPath(path);
                    if (parentPath == null) {
                        volume.Add(rootFile);
                    } else if (!filesGroup.ContainsKey(parentPath)) {
                        FileGroup fg = new FileGroup(parentPath, rootFile);
                        allGroups.Add(fg);
                        filesGroup.Add(parentPath, fg);
                    } else {
                        filesGroup[parentPath].childNodes.Add(rootFile);
                    }
                }
                return (allGroups, volume);
            }
        }
        internal class FileTree {
            List<FileNode> rootFiles = new();
            public FileTree(List<string> paths) {
                // Build all Nodes and their dictionary
                // For every node, find their parents, add self as child to  parent, add parent to child
                Dictionary<string, FileNode> pathsNode= new();
                List<FileNode>allNodes = new();
                foreach(string path in paths) {
                    FileNode node = new FileNode(path);
                    allNodes.Add(node);
                    pathsNode.Add(path, node);
                }
                foreach (FileNode node in allNodes) {
                    string path = node.Path;
                    string? parentPath = FileController.GetParentPath(path);
                    if (parentPath != null && parentPath != string.Empty && pathsNode.ContainsKey(parentPath)) {
                        FileNode parentNode = pathsNode[parentPath];
                        node.AddParentNode(parentNode);
                    }
                }
                foreach (FileNode node in allNodes) {
                    if (!node.HasParent())
                        rootFiles.Add(node);
                }

                allNodes.Clear();
                pathsNode.Clear();
            }
            public void PrintAll() {
                foreach (FileNode rf in rootFiles)
                    rf.PrintAll();
            }
            public List<FileNode> GetRootFiles() {
                return rootFiles;
            }

            /* Review Logic
             * If we add all the nodes, we now have all files to be added
             *  now we can go through all paths, if there is a node that is a parent, then we can connect
             *  the node to parent and parent to node.
             * This way every node will have connected to its parent if a parent exists, and thus all parents will have connected to children
             *  we connect to parent because it is a faster dictionary query (get parent, instead of check all nodes I am a direct parent of)
             */
            public int RootCount {
                private set { }
                get { return rootFiles.Count; }
            }
            public FileNode this[int index] {
                private set { }
                get {
                    return rootFiles[index];
                }
            }

            public void ClearAll() {
                foreach (FileNode file in rootFiles)
                    file.ClearAllData();
            }
        }
        internal class FileNode {
            public List<FileNode> children;
            private FileNode? parent;
            private string path;
            public FileNode(string p_path) {
                path = p_path;
                children = new List<FileNode>();
            }
            public string Path{
                private set { }
                get { return path; }
            }
            public override int GetHashCode() { return path.GetHashCode(); }
            public void AddParentNode(FileNode parentNode) {
                parent = parentNode;
                parentNode.children.Add(this);
            }
            public bool HasParent() {
                return parent != null;
            }
            public void ClearAllData() {
                parent = null;
                foreach (FileNode child in children)
                    child.ClearAllData();
                children.Clear();
            }
            public void PrintAll() {
                Utils.LogToOutput(path);
                foreach (FileNode child in children)
                    child.PrintAll();
            }
        }
        #endregion
    }
}
