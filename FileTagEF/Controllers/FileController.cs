using FileTagDB;
using FileTagEF.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF.Controllers {
    public partial class FileController {
        internal TagDBContext context = null!;
        // TODO: check if needs implementation

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

        #region Adding multiple files

        public void BulkAddFiles(List<string> p_paths) { // needs to be reworked somehow

            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (context = DBController.GetContext()) {
                List<string> paths = new(p_paths);
                for (int i = 0; i < paths.Count; i++)
                    paths[i] = FixFilePath(paths[i]);
                List<FileGroup> groups;
                List<FileNode> nodes;
                FileTree ft = new FileTree(paths);
                //ft.PrintAll();
                (groups, nodes) = FileGroup.MakeFileGroups(ft.GetRootFiles());

                using (var transaction = context.Database.BeginTransaction()) {
                    //Utils.LogToOutput("Root count of files to be added : "+ft.RootCount);
                    foreach (FileNode node in nodes) {
                        FilePath? fp = AddAndGetFileIC(node.Path, true, false);
                        if (fp == null) {
                            Utils.LogToOutput("Problem inserting file path :" + node.Path);
                            continue;
                        }
                        AddNodeEF(fp, node);
                    }
                    foreach (FileGroup group in groups) {
                        Utils.LogToOutput("Group log");
                        if (group.parentPath == null)
                            continue;
                        FilePath? fp = AddAndGetFileIC(group.parentPath);
                        if (fp == null)
                            continue;
                        MultiNodeInsert(group.childNodes, fp, -1 != fp.Id);
                        foreach (FileNode fn in group.childNodes) {
                            int nodeID = GetFileIdIC(fn.Path);
                            if (-1 == nodeID)
                                Utils.LogToOutput("Possible problem in Group add Parent: " + group.parentPath);
                            AddNodeEF(fp, fn);
                        }
                    }
                    context.SaveChanges();
                    transaction.Commit();
                }
                ft.ClearAll();
            }
            watch.Stop();

            Utils.LogToOutput("Bulk insert time " + watch.ElapsedMilliseconds);
            // we need to be smart about this
            // add all children together, and get their IDs together
            // we can return last row ID, and affected rows
        }
        #endregion

        #region Adding single file
        public int AddFile(string filepath) {
            using (context = DBController.GetContext()) {
                int fileId = AddFileIC(filepath);
                return fileId;
            }
        }
        #endregion

        #region Getting File Data
        public int CountFiles() {
            using (context = DBController.GetContext())
                return context.FilePaths.AsNoTracking().Count();
        }
        public int GetFileId(string filepath) {
            using (context = DBController.GetContext()) {
                int fileID;
                fileID = GetFileIdIC(FixFilePath(filepath));
                return fileID;
            }
        }
        public List<int> GetFilesIds(List<string> filepaths) {
            List<int> filesIds = new();
            using (context = DBController.GetContext()) {
                foreach (string filepath in filepaths)
                    filesIds.Add(GetFileIdIC(FixFilePath(filepath)));
            }
            return filesIds;
        }
        public string GetFilePath(int fileID) {
            using (context = DBController.GetContext()) {
                string result = GetFilePathIC(fileID);
                return result;
            }
        }
        public List<(string, int)> GetFileChildren(string filepath) {
            using (context = DBController.GetContext()) {
                int fileID = GetFileIdIC(filepath);
                if (-1 == fileID)
                    return new();
                return GetFileChildrenIC(fileID);
            }
        }
        public List<(string, int)> GetFileChildren(int fileID) {
            using (context = DBController.GetContext())
                return GetFileChildrenIC(fileID);
        }
        public List<(string, int)> GetFilesWithPath(string path) { // for use with tag folder insertions (mostly only)
            using (context = DBController.GetContext())
                return GetFilesWithPathIC(path);
        }
        public int GetFilesParentID(string filepath) {
            string? parentPath = GetParentPath(filepath);
            if (parentPath == null || parentPath == string.Empty)
                return -1;
            return GetFileId(parentPath);
        }
        #endregion


        /*
         * This will be faster because it will rename all, since all child files
         *  will have the same parent...
         */
        #region Updating a filepath
        public void ReAdjustFilepathBulk(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            using (context = DBController.GetContext()) {
                int fileID = GetFileIdIC(filepath);
                if (-1 == fileID) 
                    return;
                if (GetParentPath(newPath) != GetParentPath(filepath)) // because it possibly could be just a name change
                    DeleteParentLinkIC(fileID);
                RenameFilePathIC(filepath, newPath); // we will only change paths, IDs and file to file rel are same
            }
        }
        #endregion

        #region File deletion
        public void DeleteFileOnly(string filepath) {
            using (context = DBController.GetContext()) {
                DeleteFileIC(filepath);
                context.SaveChanges();
            }
        }
        // NOTE: deleting directory will delete everything under it, whether they are connected or not
        // And this is intended behaviour. You are removing the folder and everything below from the system
        //   if there is a folder that was not included but its subfiles/folders were included they will get Exodiad
        public void DeleteDirectory(string filepath) {
            using (context = DBController.GetContext()) {
                DeleteDirectoryIC(filepath);
                context.SaveChanges();
            }
        }
        public void DeleteFiles(List<string> filepaths) {
            using (context = DBController.GetContext()) {
                foreach (string path in filepaths)
                    DeleteFileIC(path);
                context.SaveChanges();
            }
        }
        public void DeleteDirectories(List<string> filepaths) {
            using (context = DBController.GetContext()) {
                foreach (string path in filepaths)
                    DeleteDirectoryIC(path);
                context.SaveChanges();
            }
        }
        #endregion

        #region Helper classes for managing file tree
        internal class FileGroup {
            public string? parentPath;
            public List<FileNode> childNodes = new();
            FileGroup(string? path, FileNode firstNode) {
                parentPath = path;
                childNodes.Add(firstNode);
            }
            public static (List<FileGroup>, List<FileNode>) MakeFileGroups(List<FileNode> rootFiles) {
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
                Dictionary<string, FileNode> pathsNode = new();
                List<FileNode> allNodes = new();
                foreach (string path in paths) {
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
            public string Path {
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
