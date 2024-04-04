using System.Data.SQLite;
using System.Text;

namespace FileTagDB.Controllers {
    public partial class FileController {
        // DBC functions expect to be called while DB are connected, thus they are not public
        //      the public functions will connect and call connected

        #region Adding multiple files
        internal void AddNode(int fileID, FileNode node) {
            // We want to add ALL the files with the one insert
            AddFileChilds(GetFilePathDBC(fileID), fileID); // in case previous children exists in the database to this file
            if (node.children.Count == 0) {
                return;
            }
            if (node.children.Count == 1) { // forgot to add the child...
                int resultID = AddFileDBC(node.children[0].Path);
                if (-1 == resultID)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path: " + node.Path);
                AddNode(resultID, node.children[0]);
                return;
            }
            for (int i = 0; i < node.children.Count; i += bulkSeparation) {
                MultiNodeInsert(node.children, i, System.Math.Min(bulkSeparation, node.children.Count - i), fileID);
                //Utils.LogToOutput($" start i/total = {i} / {node.children.Count} Move {Math.Min(bulkSeparation, node.children.Count - i)} // par: {node.Path}");
            }
            // but now we need to insert my and child ID for all?
            foreach (FileNode fn in node.children) {
                int nodeID = GetFileIDDBC(fn.Path);
                if (-1 == nodeID)
                    Utils.LogToOutput("Possible problem in bulk add Mini node path2: " + node.Path);
                AddNode(nodeID, fn);
            }
        }
        internal void MultiNodeInsert(List<FileNode> children, int start, int count, int parentID, bool hasParent = true) {
            if (count == 0)
                return;
            StringBuilder sb = new();
            sb.EnsureCapacity(expectedPathLength * count);
            int affectedRows = -1;
            int lastInsertedRowId = -1;
            // insert the files (add all files one by one)
            try {
                (affectedRows, lastInsertedRowId) = BulkInsertMultipleFileEntries(children, start, count, sb);
            } catch (Exception) { // Insert them manually (since all of them together failed
                foreach (FileNode childNode in children)
                    AddFileDBC(childNode.Path); // takes care of parent and child (if child exists)
                return;  // no parent, single file that was already added insert (root file), So... we add each file individually
            }
            sb.Clear();
            if (affectedRows < 1)
                return;
            if (!hasParent)
                return;
            // insert the parent child relation with IDs
            AssociateChildrenWithParentFile(children, affectedRows, lastInsertedRowId, parentID, sb);
        }
        internal (int, int) BulkInsertMultipleFileEntries(List<FileNode> children, int start, int count, StringBuilder sb) { //re-use of sb
            int affectedRows = -1;
            int lastInsertedRowId = -1;
            using (var cmd = new SQLiteCommand(conn)) {
                sb.Append($"INSERT INTO {TableConst.filesTName} ({TableConst.filesCoPath}) VALUES");
                for (int i = 0; i < count; i++) {
                    string param = $"$n{i}";
                    sb.Append($" ({param}),");
                    cmd.Parameters.AddWithValue(param, children[i + start].Path);
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
            return (affectedRows, lastInsertedRowId);
        }
        internal void AssociateChildrenWithParentFile(List<FileNode> children, int affectedRows, int lastInsertedRowId, int parentID, StringBuilder sb) {
            using (var cmd = new SQLiteCommand(conn)) {
                sb.Append($"INSERT INTO {TableConst.fileChildsTName} ({TableConst.fileChildsFID},{TableConst.fileChildsCID}) VALUES");
                int startID = lastInsertedRowId - affectedRows + 1;
                for (int i = 0; i < affectedRows; i++) {
                    string param = $"$c{i}";
                    sb.Append($" ({parentID},{param}),");
                    cmd.Parameters.AddWithValue(param, startID + i);
                }
                sb.Length--; // remove last extra comma
                cmd.CommandText = sb.ToString();
                affectedRows = cmd.ExecuteNonQuery();
            }

        }


        #endregion

        #region Adding single file

        internal int AddFileDBC(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetParentPath(filepath) == "")
                return -1;
            int lastInsertedRowId = -1;
            int existingID = GetFileIDDBC(filepath);
            if (-1 != existingID)
                return existingID;
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$path", filepath);
                if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.filesTName} ({TableConst.filesCoPath}) VALUES ($path);"))
                    return -1;
                cmd.CommandText = "SELECT last_insert_rowid();";
                Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                lastInsertedRowId = (int)LastRowID64;
                //Utils.LogToOutput("Inserted row ID " + lastInsertedRowId);
            }
            if (-1 == lastInsertedRowId)
                return -1;
            AddFileConnections(filepath, lastInsertedRowId);
            return lastInsertedRowId;
        }

        internal void AddFileConnections(string filepath, int fileID) {
            AddFileParent(filepath, fileID);
            AddFileChilds(filepath, fileID);
        }
        internal void AddFileParent(string filepath, int fileID) {
            string? parentPath = GetParentPath(filepath);
            if (parentPath == null)
                return;
            int parentFileID = GetFileIDDBC(parentPath);
            if (-1 != parentFileID)
                AddOneFileRelation(parentFileID, fileID);
        }
        internal void AddOneFileRelation(int parentFileID, int fileID) {
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
        internal void AddFileChilds(string filepath, int fileID) {
            //if (FileAttributes.Directory == (FileAttributes.Directory & File.GetAttributes(filepath)))
            if (!filepath.EndsWith(Path.DirectorySeparatorChar)) { filepath += Path.DirectorySeparatorChar; }
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$filepath", filepath);
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT {TableConst.filesCoID} FROM {TableConst.filesTName} WHERE
                        {TableConst.filesCoID} NOT IN (SELECT {TableConst.fileChildsCID} FROM {TableConst.fileChildsTName} 
                                WHERE {TableConst.fileChildsFID} = {fileID})
                        AND {TableConst.filesCoPath} LIKE $filepath || '_%' 
                        AND {TableConst.filesCoPath} NOT LIKE $filepath || '_%' || '{Path.DirectorySeparatorChar}' || '%'
                    "); // direct child only! // Also at least 1 character longer, so that it doesn't get itself
                // if it has a separator the its probably not a direct child, our separator is already added
                // Also select all ID that are not already children from the file ID
                AddMultipleFileRelation(fileID, reader);
                reader.Close();
            }
            // get all child IDs
        }


        internal void AddMultipleFileRelation(int parentFileID, SQLiteDataReader reader) {
            using (var transaction = conn.BeginTransaction()) {
                while (reader.Read()) {
                    var command = conn.CreateCommand(); // we create a new command each time because a unique error cause it to permenantly fail
                    command.CommandText = @$"INSERT INTO {TableConst.fileChildsTName} 
                    ({TableConst.fileChildsFID},{TableConst.fileChildsCID}) VALUES ({parentFileID},$child)";

                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "$child";
                    command.Parameters.Add(parameter);
                    try {
                        parameter.Value = ((Int64)reader[$"{TableConst.filesCoID}"]).ToString();
                        command.ExecuteNonQuery();
                    } catch (Exception e) { 
                        Utils.LogToOutput("Failed " + e.Message + " id to insert " + parentFileID + " " + parameter.Value);
                    }
                }
                transaction.Commit();
            }
        }

        #endregion


        #region Getting File Data

        internal int GetFileIDDBC(string filepath) {
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

        internal string GetFilePathDBC(int fileID) {
            object? result;
            string cmdText = $"SELECT  {TableConst.filesCoPath} FROM {TableConst.filesTName} WHERE {TableConst.filesCoID} = {fileID};";
            bool success;
            using (var cmd = new SQLiteCommand(conn))
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoPath}", out result);
            if (!success || result == null)
                return "";
            return (string)result;
        }

        internal List<(string, int)> GetFileChildrenDBC(int fileID) {
            List<(string, int)> fileRows = new();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT f.* FROM {TableConst.filesTName} f JOIN
                    {TableConst.fileChildsTName} fc ON
                    fc.{TableConst.fileChildsCID} = f.{TableConst.filesCoID}
                    WHERE fc.{TableConst.fileChildsFID} = {fileID}
                "); // direct child only!
                while (reader.Read())
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
            }
            return fileRows;
        }
        internal List<(string, int)> GetFilesWithPathDBC(string filepath) {
            List<(string, int)> fileRows = new();
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$filepath", filepath);
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT * FROM {TableConst.filesTName} WHERE {TableConst.filesCoPath} LIKE $filepath || '%'");
                while (reader.Read())
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
            }
            return fileRows;
        }
        #endregion

        #region Updating filepath
        internal void RenameFilePathDBC(string oldPath, string newPath) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$oldPath", oldPath);
                cmd.Parameters.AddWithValue("$newPath", newPath);
                DBController.ExecuteNonQCommand(cmd, $"UPDATE {TableConst.filesTName} SET {TableConst.filesCoPath} = " +
                    $" replace({TableConst.filesCoPath}, $oldPath, $newPath)");
            }
        }
        #endregion

        #region File Deletion
        internal void DeleteParentLinkDBC(int fileID) {
            using (var cmd = new SQLiteCommand(conn))
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.fileChildsTName} WHERE {TableConst.fileChildsCID} = {fileID} ");
        }
        internal void DeleteFileDBC(string filepath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileIDDBC(filepath);
            if (-1 == fileID) return;
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fp", filepath);
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.filesTName} WHERE {TableConst.filesCoPath} = $fp");
            }
        }

        internal void DeleteDirectoryDBC(string filepath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileIDDBC(filepath);
            if (-1 == fileID) return;
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fp", filepath);
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.filesTName} WHERE {TableConst.filesCoPath} LIKE $fp ||'%'");
            }
        }

        #endregion
    }
}
