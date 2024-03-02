using FileTagDB.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileTagDB.Controllers {
    public class FileController {
        SQLiteConnection conn;
        public FileController() {
            conn = DBController.GetDBConnection();
        }
        #region Adding a file
        // we will leave the checking for the external function...
        public int AddFile(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetRootPath(filepath) == "")
                return -1;
            int lastInsertedRowId = -1;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    cmd.Parameters.AddWithValue("$path", filepath);
                    if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.tagsTName} ({TableConst.tagsCoName}) VALUES ($path);"))
                        return -1;
                    cmd.CommandText = "SELECT last_insert_rowid();";
                    Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                    lastInsertedRowId = (int)LastRowID64;
                }
                if (-1 == lastInsertedRowId)
                    return -1;
                AddFileConnections(filepath, lastInsertedRowId);
                conn.Close();
            }
            return lastInsertedRowId;
        }
        private string FixFilePath(string filepath) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                if (filepath.EndsWith(":")) { filepath += @"\"; }
            return filepath;
        }
        private void AddFileConnections(string filepath, int fileID) {
            AddFileParent(filepath, fileID);
            AddFileChilds(filepath, fileID);
        }
        private void AddFileParent(string filepath, int fileID) {
            string? parentPath = GetRootPath(filepath);
            if (parentPath == null)
                return;
            int parentFileID = GetFileIDNoConn(parentPath);
            if (-1 != parentFileID)
                AddOneFileRelation(parentFileID, fileID);
        }
        public string? GetRootPath(string filepath) {
            DirectoryInfo d = new DirectoryInfo(filepath);
            if (d.Parent == null) {
                Utils.LogToOutput("Folder/File is root : " + filepath);
                return null;
            }
            if (!filepath.Contains(d.Parent.FullName)) {
                Utils.LogToOutput("This is a relative path :" + d.Parent.FullName + " To " + filepath);
                return string.Empty;
            }
            Utils.LogToOutput("Parent folder is " + d.Parent.FullName + " ;;of;; " + filepath);
            return d.Parent.FullName;
        }
        private int GetFileIDNoConn(string filepath) {
            object? result;
            string cmdText = $"select  {TableConst.filesCoID} from {TableConst.filesTName} where {TableConst.filesCoPath} = '{filepath}';";
            bool success;
            using (var cmd = new SQLiteCommand(conn))
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
            if (!success || result == null)
                return -1;
            return (int)((Int64)result);
        }
        public int GetFileID(string filepath) {
            object? result;
            string cmdText = $"select  {TableConst.filesCoID} from {TableConst.filesTName} where {TableConst.filesCoPath} = '{filepath}';";
            bool success;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
                conn.Close();
            }
            if (!success || result == null)
                return -1;
            return (int)((Int64)result);
        }

        private void AddOneFileRelation(int parentFileID, int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$parent", parentFileID);
                cmd.Parameters.AddWithValue("$child", fileID);
                int result = DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.fileChildsTName} VALUES ($parent,$child)");
                if (-1 == result) {
                    Utils.LogToOutput(string.Format("Something wrong with adding parent {0} with child {1} ID", parentFileID, fileID));
                }
            }
        }
        private void AddFileChilds(string filepath, int fileID) {
            if (FileAttributes.Directory == (FileAttributes.Directory & File.GetAttributes(filepath)))
                if (!filepath.EndsWith(Path.DirectorySeparatorChar)) { filepath += Path.DirectorySeparatorChar; }
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT {TableConst.filesCoID} FROM {TableConst.filesTName} WHERE
                        {TableConst.filesCoPath} LIKE '{filepath}%' AND 
                        {TableConst.filesCoPath} NOT LIKE '{filepath}%{Path.DirectorySeparatorChar}'
                    "); // direct child only!
                // if it has a separator the its probably not a direct child, our separator is already added
                AddMultipleFileRelation(fileID, reader);
                DBController.ExecuteNonQCommand(cmd, $"Insert INTO {TableConst.fileChildsTName} VALUES ($parent,$child)");
            }
            // get all child IDs
        }

        private void AddMultipleFileRelation(int parentFileID, SQLiteDataReader reader) {
            using (var transaction = conn.BeginTransaction()) {
                var command = conn.CreateCommand();
                command.CommandText = @$"
                    INSERT INTO {TableConst.fileChildsTName} VALUES ({parentFileID},$child)
                ";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "$child";
                command.Parameters.Add(parameter);
                while (reader.Read()) {
                    parameter.Value = (string)reader[$"{TableConst.filesCoID}"];
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
        }
        #endregion

        #region Getting related files
        public List<(string,int)> RetrieveFileChildren(int fileID) {
            List<(string, int)> fileRows = new();

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
            return fileRows;
        }
        public int RetrieveFileParent(string filepath) {
            string? parentPath = GetRootPath(filepath);
            if(parentPath==null || parentPath == string.Empty)
                return -1;
            return GetFileID(parentPath);
        }
        #endregion

        #region removing a file
        public bool DeleteFileFromDB(string filepath) {
            filepath = FixFilePath(filepath);
            if (GetRootPath(filepath) == "") return false;
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
        /* Get child files
         * for every child file call readjust
         * fix my name
         * 
         * 
         * 
         */
        public void ReAdjustFileLocation(string filepath, string newPath) {
            filepath = FixFilePath(filepath);
            int fileID = GetFileID(filepath);
            if (-1 == fileID) return;
            using (conn = DBController.GetDBConnection()) {
                conn.Open();
                UpdateFileName(fileID, newPath);
                UpdateChildrenToNewPath(fileID, filepath, newPath);
                conn.Close();
            }
            // Recursive fix
        }
        private void UpdateFileName(int fileID, string newPath) {
            using (var cmd = new SQLiteCommand(conn)) {
                DBController.ExecuteNonQCommand(cmd, $"UPDATE {TableConst.filesCoID} SET {TableConst.filesCoPath} = '{newPath}' " +
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

        private string GetFilePathNoConn(int fileID) {
            object? result;
            string cmdText = $"SELECT  {TableConst.filesCoPath} FROM {TableConst.filesTName} WHERE {TableConst.filesCoID} = {fileID};";
            bool success;
            using (var cmd = new SQLiteCommand(conn))
                success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
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
                using (var cmd = new SQLiteCommand(conn))
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.filesCoID}", out result);
                conn.Close();
            }
            if (!success || result == null)
                return "";
            return (string)result;
        }


    }
}
