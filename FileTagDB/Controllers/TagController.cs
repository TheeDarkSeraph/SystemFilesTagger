﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FileTagDB.Models;

namespace FileTagDB.Controllers {
    public class TagController {
        // TODO: Renamable box selection item...
        // TODO: Select from list where tag = regexp (translated FROM user)
        FileController _fileController;
        public TagController(FileController fc) {
            _fileController = fc;
        }

        public List<Tag> GetAllTags() {
            return null;
        }  
        
        public int CreateTag(string tag) {
            Utils.LogToOutput("Tag to be inserted :" + tag);
            int lastInsertedRowId = -1;
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    cmd.Parameters.AddWithValue("$name", tag);
                    if (-1 == DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.tagsTName} ({TableConst.tagsCoName}) VALUES ($name);"))
                        return -1;
                    cmd.CommandText = "SELECT last_insert_rowid();";
                    Int64 LastRowID64 = (Int64)cmd.ExecuteScalar();
                    lastInsertedRowId = (int)LastRowID64;
                    // The row ID is a 64-bit value - cast the Command result to an Int64.
                    //

                }
                conn.Close();
            }
            return lastInsertedRowId;
        }

        #region Retrieving Tag info
        public int CountTags() {
            string cmdText = $"SELECT COUNT({TableConst.tagsCoName}) FROM {TableConst.tagsTName};";
            object result = DBController.ExecuteScalerAutoConn(cmdText);
            return Convert.ToInt32(result);
        }
        public string GetTagName(int tagID) {
            object? result;
            string cmdText = $"SELECT {TableConst.tagsCoName} FROM {TableConst.tagsTName} WHERE {TableConst.tagsCoID} = {tagID};";
            bool success;
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn))
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.tagsCoName}", out result);
                conn.Close();
            }
            if (!success || result == null)
                return "+NotFound";
            return (string)result;
        }

        // won't be used, tags will be kept in memory prolly for faster processing of user requests and suggestions, 1 mil of average 7 chars is 11MB roughly which is still nothing
        // there isn't even 1 mil words...
        public int GetTagID(string tag) {
            object? result;
            string cmdText = $"SELECT  {TableConst.tagsCoID} FROM {TableConst.tagsTName} WHERE {TableConst.tagsCoName} = $tag";
            bool success;
            using (var conn = DBController.GetDBConnection()) {
                conn.Open();
                using (var cmd = new SQLiteCommand(conn)) {
                    cmd.Parameters.AddWithValue("$tag", tag);
                    success = DBController.TryExecuteSingleRead(cmd, cmdText, $"{TableConst.tagsCoID}", out result);
                }
                conn.Close();
            }
            if (!success||result==null)
                return -1;
            return (int)((Int64)result);
        }
        #endregion

        public int RenameTag(int tagID, string newName) {
            return GetAffectedRowsFromQueryAutoCon($"UPDATE {TableConst.tagsTName} SET {TableConst.tagsCoName} = $newname" +
                $" WHERE {TableConst.tagsCoID} = {tagID}", new List<string>{"$newname"}, new List<string> { newName });
        }
        public int RenameTag(string tagName, string newName) { // won't be used... 
            if (tagName == newName)
                return 0; // otherwise 1 will be returned

            return GetAffectedRowsFromQueryAutoCon($"UPDATE {TableConst.tagsTName} SET {TableConst.tagsCoName} = $newname" +
                $" WHERE {TableConst.tagsCoName} = $tagName", new List<string> { "$newname", "$tagName" },
                new List<string> { newName, tagName });
        }

        public int DeleteTag(int tagID) {
            return GetAffectedRowsFromQueryAutoCon($"DELETE FROM {TableConst.tagsTName} WHERE tid = {tagID}");
        }
        public int GetAffectedRowsFromQueryAutoCon(string cmdText) {
            var conn = DBController.GetDBConnection();
            int rowsAffected = 0;
            conn.Open();
            using (var cmd = new SQLiteCommand(conn)) {
                rowsAffected = DBController.ExecuteNonQCommand(cmd, cmdText);
                cmd.Dispose();
            }
            conn.Close();
            return rowsAffected;
        }
        public int GetAffectedRowsFromQueryAutoCon(string cmdText, List<string> paramName, List<string> paramValue) {
            var conn = DBController.GetDBConnection();
            int rowsAffected = 0;
            conn.Open();
            using (var cmd = new SQLiteCommand(conn)) {
                for (int i = 0; i < paramName.Count; i++)
                    cmd.Parameters.AddWithValue(paramName[i], paramValue[i]);
                rowsAffected = DBController.ExecuteNonQCommand(cmd, cmdText);
                cmd.Dispose();
            }
            conn.Close();
            return rowsAffected;
        }
        public void TagFile(int tagID, int fileID) {
            
        }
        public void UntagFile(int tagID, int fileID) {

            // if file has no more tags, remove file
        }

        public void TagFile(int tagID, string filePath) {
            int fileID = _fileController.AddFile(filePath);
        }


        public void RenameTag() {

        }

    }
}