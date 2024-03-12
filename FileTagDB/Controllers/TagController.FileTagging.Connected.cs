using FileTagDB.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FileTagDB.Controllers {
    public partial class TagController {
        public void TagFileConnected(int tagID, int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileID);
                cmd.Parameters.AddWithValue("$tagid", tagID);
                DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.fileTagsTName} " +
                    $"({TableConst.fileTagsCoFID} , {TableConst.fileTagsCoTID}) VALUES ($fileid , $tagid)");
            }
        }

        public void TagFilesConnected(int tagID, List<int> fileIDs) {
            using (var transaction = conn.BeginTransaction()) {
                foreach (int fileID in fileIDs) {
                    var command = conn.CreateCommand(); // we create a new command each time because a unique error cause it to permenantly fail
                    command.CommandText = $"INSERT INTO {TableConst.fileTagsTName} " +
                    $"({TableConst.fileTagsCoFID} , {TableConst.fileTagsCoTID}) VALUES ($fileid , $tagid)";
                    var parameter1 = command.CreateParameter();
                    var parameter2 = command.CreateParameter();
                    parameter1.ParameterName = "$fileid";
                    parameter2.ParameterName = "$tagid";
                    command.Parameters.Add(parameter1);
                    command.Parameters.Add(parameter2);
                    try {
                        parameter1.Value = fileID;
                        parameter2.Value = tagID;
                        command.ExecuteNonQuery();
                    } catch (Exception e) {
                        Utils.LogToOutput("tag multiple file error is " + e);
                    }
                }
                transaction.Commit();
            }
        }

        public void UntagFileConnected(int tagID, int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileID);
                cmd.Parameters.AddWithValue("$tagid", tagID);
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.fileTagsTName} " +
                    $" WHERE {TableConst.fileTagsCoFID} =  $fileid AND {TableConst.fileTagsCoTID} =  $tagid");
            }
        }

        public void UntagFilesConnected(int tagID, List<int> fileIDs) {
            using (var transaction = conn.BeginTransaction()) {
                foreach (int fileID in fileIDs) {
                    var command = conn.CreateCommand(); // we create a new command each time because a unique error cause it to permenantly fail
                    command.CommandText = $"DELETE FROM {TableConst.fileTagsTName} " +
                    $" WHERE {TableConst.fileTagsCoFID} =  $fileid AND {TableConst.fileTagsCoTID} =  $tagid";
                    var parameter1 = command.CreateParameter();
                    var parameter2 = command.CreateParameter();
                    parameter1.ParameterName = "$fileid";
                    parameter2.ParameterName = "$tagid";
                    command.Parameters.Add(parameter1);
                    command.Parameters.Add(parameter2);
                    try {
                        parameter1.Value = fileID;
                        parameter2.Value = tagID;
                        command.ExecuteNonQuery();
                    } catch (Exception) { }
                }
                transaction.Commit();
            }
        }


    }
}
