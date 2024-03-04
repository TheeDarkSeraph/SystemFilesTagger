using FileTagDB.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileTagDB.Controllers {
    
    public class FileController {
        SQLiteConnection conn;
        public static int bulkSeparation = 200;
        public const int expectedPathLength = 70;
        public FileController() {
            conn = DBController.GetDBConnection();
        }

        // speed clocked at 4874 seconds, which is probably a great improvement (this is with all the logs...
        // 12ms for tree/ 4852ms without the prints
        #region Adding multiple files
        // TODO: Group same root files with the same parent into a folder group
        public void BulkAddFiles(List<string> p_paths) {
            List<string> paths = new(p_paths);
            for (int i = 0; i < paths.Count; i++)
                paths[i] = FixFilePath(paths[i]);
            FileTree ft = new(paths, this);
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                //Utils.LogToOutput("Root count of files to be added : "+ft.RootCount);
                for (int i = 0; i < ft.RootCount; i++) {
                    FileNode node = ft[i];
                    int fileID = AddFileNoConn(node.Path);
                    if (-1 == fileID)
                        fileID = GetFileIDNoConn(node.Path);
                    if (-1 == fileID) {
                        Utils.LogToOutput("Problem inserting file path :" + node.Path);
                        continue;
                    }
                    AddNode(fileID, node);
                }
                conn.Close();
            }
            ft.ClearAll();
            // we need to be smart about this
            // add all children together, and get their IDs together
            // we can return last row ID, and affected rows
        }
        private int AddFileNoConn(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "")
                return -1;
            int lastInsertedRowId = -1;
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$path", filepath);
                if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.filesTName} ({TableConst.filesCoPath}) VALUES ($path);"))
                    return -1;
                cmd.CommandText = "SELECT last_insert_rowid();";
                Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                lastInsertedRowId = (int)LastRowID64;
                Utils.LogToOutput("Inserted row ID " + lastInsertedRowId);
            }
            if (-1 == lastInsertedRowId)
                return -1;
            AddFileConnections(filepath, lastInsertedRowId);
            return lastInsertedRowId;
        }
        private void AddNode(int fileID, FileNode node) {
            //Utils.LogToOutput("Child count "+node.children.Count);
            //Enumerable.Range(5, 22 - 6 + 1).ToList();
            // The parent file was added
            // We want to add ALL the files with the one insert
            if (node.children.Count == 0)
                return;
            if (node.children.Count == 1) {
                int resultID = AddFileNoConn(node.children[0].Path);
                if(-1 == resultID)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path: " + node.Path);
                return;
            }
            for (int i = 0; i < node.children.Count; i += bulkSeparation)
                MultiNodeInsert(node.children, i, Math.Min(bulkSeparation, node.children.Count - i), fileID);
            // but now we need to insert my and child ID for all?
            foreach(FileNode fn in node.children) {
                int nodeID = GetFileIDNoConn(fn.Path);
                if (-1 == nodeID)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path: " + node.Path);
                AddNode(nodeID, fn);
            }
        }
        private void MultiNodeInsert(List<FileNode> children, int start, int count, int parentID) {
            // TODO: Add the IDs
            if (count == 0)
                return;
            StringBuilder sb = new();
            sb.EnsureCapacity(expectedPathLength * count);
            int affectedRows = -1;
            int lastInsertedRowId = -1;
            // insert the files (add all files one by one)
            using (var cmd = new SQLiteCommand(conn)) {
                sb.Append($"INSERT INTO {TableConst.filesTName} ({TableConst.filesCoPath}) VALUES");
                for (int i = 0; i < count; i++) {
                    string param = $"$n{i}";
                    sb.Append($" ({param}),");
                    cmd.Parameters.AddWithValue(param, children[i+start].Path);
                }
                sb.Length--; // remove last extra comma
                cmd.CommandText = sb.ToString();
                affectedRows = cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid();";
                Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                lastInsertedRowId = (int)LastRowID64;
                //Utils.LogToOutput(string.Format("Rows inserted {0}/{1} last inserted row ID {2}", affectedRows, count, lastInsertedRowId));
                cmd.Dispose();
            }
            sb.Clear();
            if (affectedRows < 1)
                return;
            // insert the parent child relation with IDs
            using (var cmd = new SQLiteCommand(conn)) {
                sb.Append($"INSERT INTO {TableConst.fileChildsTName} ({TableConst.fileChildsFID},{TableConst.fileChildsCID}) VALUES");
                int startID = lastInsertedRowId - affectedRows + 1;
                for (int i = 0; i < affectedRows; i++) {
                    string param = $"$c{i}";
                    sb.Append($" ({parentID},{param}),");
                    cmd.Parameters.AddWithValue(param, GetFilePathNoConn(startID + i));
                }
                sb.Length--; // remove last extra comma
                cmd.CommandText = sb.ToString();
                affectedRows = cmd.ExecuteNonQuery();

                //Utils.LogToOutput(string.Format("Rows inserted {0}/{1}", affectedRows, count));
                cmd.Dispose();
            }
        }
        #endregion

        // we will leave the checking for the external function...
        // Time Taken 34, 67-75, 133 ms per add
        #region Adding single file
        public int AddFile(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "") 
                return -1;
            int lastInsertedRowId = -1;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    cmd.Parameters.AddWithValue("$path", filepath);
                    if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.filesTName} ({TableConst.filesCoPath}) VALUES ($path);"))
                        return -1;
                    cmd.CommandText = "SELECT last_insert_rowid();";
                    Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                    lastInsertedRowId = (int)LastRowID64;
                    Utils.LogToOutput("Inserted row ID " + lastInsertedRowId);
                }
                if (-1 == lastInsertedRowId) 
                    return -1;
                AddFileConnections(filepath, lastInsertedRowId);
                conn.Close();
            }
            return lastInsertedRowId;
        }
        public static string FixFilePath(string filepath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                if (filepath.EndsWith(":")) { filepath += @"\"; }
            return filepath;
        }
        private void AddFileConnections(string filepath, int fileID) {
            AddFileParent(filepath, fileID);
            AddFileChilds(filepath, fileID);
        }
        private void AddFileParent(string filepath, int fileID) {
            string? parentPath = GetParentPath(filepath);
            if (parentPath == null)
                return;
            int parentFileID = GetFileIDNoConn(parentPath);
            if (-1 != parentFileID)
                AddOneFileRelation(parentFileID, fileID);
        }
        public string? GetParentPath(string filepath) {
            DirectoryInfo d = new DirectoryInfo(filepath); // works on non existing files
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
        private int GetFileIDNoConn(string filepath) {
            object? result;
            string cmdText = $"SELECT  {TableConst.filesCoID} FROM {TableConst.filesTName} WHERE {TableConst.filesCoPath} = $filepath";
            bool success;
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$filepath", filepath);
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
                cmd.Dispose();
            }
            if (!success || result == null)
                return -1;
            return (int)((Int64)result);
        }
        public int GetFileID(string filepath) {
            object? result;
            filepath = FixFilePath(filepath);
            string cmdText = $"SELECT  {TableConst.filesCoID} FROM {TableConst.filesTName} WHERE {TableConst.filesCoPath} = $filepath";
            bool success;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    cmd.Parameters.AddWithValue("$filepath", filepath);
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
                    cmd.Dispose();
                }
                conn.Close();
            }
            if (!success || result == null) {
                Utils.LogToOutput("Success ? " + success);
                return -1;
            }
            return (int)((Int64)result);
        }
        private void AddOneFileRelation(int parentFileID, int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$parent", parentFileID);
                cmd.Parameters.AddWithValue("$child", fileID);
                int result = DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.fileChildsTName} " +
                    $"({TableConst.fileChildsFID}, {TableConst.fileChildsCID}) VALUES ($parent , $child)");
                if (-1 == result) {
                    Utils.LogToOutput(string.Format("Something wrong with adding parent {0} with child {1} ID", parentFileID, fileID));
                }
                cmd.Dispose();
            }
        }
        private void AddFileChilds(string filepath, int fileID) {
            //if (FileAttributes.Directory == (FileAttributes.Directory & File.GetAttributes(filepath)))
            if (!filepath.EndsWith(Path.DirectorySeparatorChar)) { filepath += Path.DirectorySeparatorChar; }
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$filepath", filepath);
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT {TableConst.filesCoID} FROM {TableConst.filesTName} WHERE
                        {TableConst.filesCoPath} LIKE $filepath || '_%' 
                        AND {TableConst.filesCoPath} NOT LIKE $filepath || '_%' || '{Path.DirectorySeparatorChar}'
                    "); // direct child only! // Also at least 1 character longer, so that it doesn't get itself
                // if it has a separator the its probably not a direct child, our separator is already added
                AddMultipleFileRelation(fileID, reader);
                reader.Close();
            }
            // get all child IDs
        }

        private void AddMultipleFileRelation(int parentFileID, SQLiteDataReader reader) {
            using (var transaction = conn.BeginTransaction()) {
                var command = conn.CreateCommand();
                command.CommandText = @$"INSERT INTO {TableConst.fileChildsTName} 
                    ({TableConst.fileChildsFID},{TableConst.fileChildsCID}) VALUES ({parentFileID},$child)";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "$child";
                command.Parameters.Add(parameter);
                while (reader.Read()) {
                    parameter.Value = ((Int64)reader[$"{TableConst.filesCoID}"]).ToString();
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }
        #endregion

        #region Getting related files
        public List<(string,int)> RetrieveFileChildren(int fileID) {
            List<(string, int)> fileRows = new();
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                        @$"SELECT filep.* FROM {TableConst.filesTName} filep JOIN
                        {TableConst.fileChildsTName} filechilds ON
                        filechilds.{TableConst.fileChildsCID} = filep.{TableConst.filesCoID}
                        WHERE filechilds.{TableConst.fileChildsFID} = {fileID}
                    "); // direct child only!
                    while (reader.Read())
                        fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
                }
                conn.Close();
            }
            return fileRows;
        }
        public int RetrieveFileParentID(string filepath) {
            string? parentPath = GetParentPath(filepath);
            if(parentPath==null || parentPath == string.Empty)
                return -1;
            return GetFileID(parentPath);
        }
        
        #endregion

        #region removing a file
        public bool DeleteFileFromDB(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "") return false;
            int fileID = GetFileID(filepath);
            if (-1 == fileID) return false;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    if (-1 == DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.filesTName} WHERE {TableConst.filesCoID}={fileID};"))
                        return false;
                }
                conn.Close();
            }
            return true;
        }
        #endregion

        /*
         * This will be faster because it will rename all, since all child files
         *  will have the same parent...
         */
        #region Renaming/Fixing a filepath
        public void ReAdjustFileLocationBulk(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileID(filepath);
            if (-1 == fileID) return;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                if(GetParentPath(newPath)!=GetParentPath(filepath)) // because it possibly could be just a name change
                    DeleteParentLink(fileID);
                UpdateAllFileNames(filepath, newPath); // we will only change paths, IDs and file to file rel are same
                conn.Close();
            }
            // Recursive fix
        }
        private void UpdateAllFileNames(string oldPath, string newPath) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$oldPath", oldPath);
                cmd.Parameters.AddWithValue("$newPath", newPath);
                DBController.ExecuteNonQCommand(cmd, $"UPDATE {TableConst.filesTName} SET {TableConst.filesCoPath} = " +
                    $" replace({TableConst.filesCoPath}, $oldPath, $newPath)");
            }
        }
        #endregion
        /* Could be slow, Won't be used because will potentially be VERY slow
         * Renaming all is faster than getting every ID and child id and renaming it one by one
         * Spending multiple hierarchical search queries and updates (amount of updates = # of all files to update)
         */
        #region old renaming method
        protected void ReAdjustFileLocation(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileID(filepath);
            if (-1 == fileID) return;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();

                if (GetParentPath(newPath) != GetParentPath(filepath)) // because it possibly could be just a name change
                    DeleteParentLink(fileID);
                UpdateFileName(fileID, newPath);
                UpdateChildrenToNewPath(fileID, filepath, newPath);
                conn.Close();
            }
            // Recursive fix
        }
        private void DeleteParentLink(int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.fileChildsTName} WHERE {TableConst.fileChildsCID} = {fileID} ");
            }
        }
        private void UpdateFileName(int fileID, string newPath) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$newPath", newPath);
                DBController.ExecuteNonQCommand(cmd, $"UPDATE {TableConst.filesCoID} SET {TableConst.filesCoPath} = $newPath " +
                    $"WHERE {TableConst.filesCoID} = {fileID};");
            }
        }
        private void UpdateChildrenToNewPath(int fileID, string filepath, string newName) {
            using (var cmd = new SQLiteCommand(conn)) {
                // adjust self, then adjust children
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                @$"SELECT {TableConst.fileChildsCID} FROM {TableConst.fileChildsTName}
                        WHERE {TableConst.fileChildsFID} = {fileID}
                    ");
                while (reader.Read()) {
                    int childID = Convert.ToInt32(reader[$"{TableConst.fileChildsCID}"]);
                    string childPath = GetFilePathNoConn(childID);
                    ReAdjustChildFileLocation(cmd, filepath, newName, childPath, childID);
                }
            }
        }
        private void ReAdjustChildFileLocation(SQLiteCommand cmd, string parentPrev, string parentNew, string myPath, int myID) {
            string myNewPath = myPath.Replace(parentPrev, parentNew);
            UpdateFileName(myID, myNewPath);
            UpdateChildrenToNewPath(myID, myPath, myNewPath);
        }
        #endregion

        private string GetFilePathNoConn(int fileID) {
            object? result;
            string cmdText = $"SELECT  {TableConst.filesCoPath} FROM {TableConst.filesTName} WHERE {TableConst.filesCoID} = {fileID};";
            bool success;
            using (var cmd = new SQLiteCommand(conn))
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoPath}", out result);
            if (!success || result == null)
                return "";
            return (string)result;
        }
        public string GetFilePath(int fileID) {
            object? result;
            string cmdText = $"SELECT  {TableConst.filesCoPath} FROM {TableConst.filesTName} WHERE {TableConst.filesCoID} = {fileID};";
            bool success;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoPath}", out result);
                    cmd.Dispose();
                }
                conn.Close();
            }
            if (!success || result == null)
                return "";
            return (string)result;
        }

        #region Helper classes for managing file tree
        internal class FileTree {
            List<FileNode> rootFiles = new();
            public FileTree(List<string> paths, FileController fc) {
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
                    string? parentPath = fc.GetParentPath(path);
                    if (parentPath != null && parentPath != string.Empty && pathsNode.ContainsKey(parentPath)) {
                        FileNode parentNode = pathsNode[parentPath];
                        node.AddParentNode(parentNode);
                    }
                }
                foreach (FileNode node in allNodes) {
                    if (!node.HasParent())
                        rootFiles.Add(node);
                }
                //foreach (FileNode rf in rootFiles)
                //    rf.PrintAll();
                allNodes.Clear();
                pathsNode.Clear();
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
