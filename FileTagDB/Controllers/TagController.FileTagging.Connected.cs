using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagDB.Controllers {
    public partial class TagController {
        public void TagFileConnected(int tagID, int fileID) {
            using (var cmd = new SQLiteCommand(conn)) {
                cmd.Parameters.AddWithValue("$fileid", fileID);
                cmd.Parameters.AddWithValue("$tagid", tagID);
                DBController.ExecuteNonQCommand(cmd, $"INSERT INTO {TableConst.fileTagsTName} " +
                    $"({TableConst.fileTagsCoFID},{TableConst.fileTagsCoTID}) VALUES ($fileid,$tagid)");
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
    }
}
