using System.Data.SQLite;

namespace FileTagDB.Controllers {
    public partial class TagController {
        internal void TagFileConnected(int tagId, int fileId) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileId);
                cmd.Parameters.AddWithValue("$tagid", tagId);
                DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.fileTagsTName} " +
                    $"({TableConst.fileTagsCoFID} , {TableConst.fileTagsCoTID}) VALUES ($fileid , $tagid)");
            }
        }

        internal void TagFilesConnected(int tagId, List<int> fileIds) {
            using (var transaction = conn.BeginTransaction()) {
                foreach (int fileID in fileIds) {
                    if (fileID == -1)
                        continue;
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
                        parameter2.Value = tagId;
                        command.ExecuteNonQuery();
                    } catch (Exception e) {
                        Utils.LogToOutput("tag multiple file error is " + e);
                    }
                }
                transaction.Commit();
            }
        }

        internal void UntagFileConnected(int tagId, int fileId) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileId);
                cmd.Parameters.AddWithValue("$tagid", tagId);
                DBController.ExecuteNonQCommand(cmd, $"DELETE FROM {TableConst.fileTagsTName} " +
                    $" WHERE {TableConst.fileTagsCoFID} =  $fileid AND {TableConst.fileTagsCoTID} =  $tagid");
            }
        }

        internal void UntagFilesConnected(int tagId, List<int> fileIds) {
            using (var transaction = conn.BeginTransaction()) {
                foreach (int fileID in fileIds) {
                    if (fileID == -1)
                        continue;
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
                        parameter2.Value = tagId;
                        command.ExecuteNonQuery();
                    } catch (Exception) { }
                }
                transaction.Commit();
            }
        }

        internal List<int> GetFileTagsConnected(int fileId) {
            List<int> fileTags = new();

            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileId);
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, $"SELECT {TableConst.fileTagsCoTID} FROM {TableConst.fileTagsTName} " +
                    $" WHERE {TableConst.fileTagsCoFID} =  $fileid ");
                while (reader.Read()) {
                    fileTags.Add(Convert.ToInt32(reader[$"{TableConst.fileTagsCoTID}"]));
                }
            }

            return fileTags;
        }
        internal List<List<int>> GetFilesTagsConnected(List<int> fileIds) {
            List<List<int>> filesTags = new();
            using (var transaction = conn.BeginTransaction()) {
                foreach (int id in fileIds)
                    filesTags.Add(GetFileTagsConnected(id));
                transaction.Commit();
            }
            return filesTags;
        }

        internal void RemoveFilesWithoutTagsConnected() {
            using (var cmd = new SQLiteCommand(conn)) {
                int filesDeleted=DBController.ExecuteNonQCommand(cmd, 
                    $"DELETE FROM {TableConst.filesTName} WHERE {TableConst.filesCoID} NOT IN (" +
                    $"SELECT DISTINCT {TableConst.fileTagsCoFID} FROM {TableConst.fileTagsTName})");
                Utils.LogToOutput("files deleted " + filesDeleted);
            }
        }
    }
}
