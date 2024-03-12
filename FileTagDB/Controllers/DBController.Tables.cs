using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FileTagDB.Controllers {
    public partial class DBController {
        // TODO: Ignore the ../ in paths
        // TCC - Table Creation Command
        const string fileTCC = @$"
            CREATE TABLE {TableConst.filesTName}(
               {TableConst.filesCoID} INTEGER PRIMARY KEY,
               {TableConst.filesCoPath} TEXT NOT NULL UNIQue
             );";

        const string tagTCC =@$"
            CREATE TABLE {TableConst.tagsTName}(
               {TableConst.tagsCoID} INTEGER PRIMARY KEY,
               {TableConst.tagsCoName} TEXT NOT NULL UNIQue
            );";
        const string fileTagsTCC =@$"
            Create Table {TableConst.fileTagsTName}(
              {TableConst.fileTagsCoFID} INTEGER NOT NULL,
              {TableConst.fileTagsCoTID} INTEGER NOT NULL,
              PRIMARY key ({TableConst.fileTagsCoFID},{TableConst.fileTagsCoTID}),
              FOREIGN KEY ({TableConst.fileTagsCoFID}) REFERENCES {TableConst.filesTName} ({TableConst.filesCoID}) ON DELETE CASCADE,
              FOREIGN KEY ({TableConst.fileTagsCoTID}) REFERENCES {TableConst.tagsTName} ({TableConst.tagsCoID}) ON DELETE CASCADE
            );";
        const string fileChildsTCC =@$"
            Create Table {TableConst.fileChildsTName}(
              {TableConst.fileChildsFID} INTEGER NOT NULL,
              {TableConst.fileChildsCID} INTEGER NOT NULL,
              PRIMARY key ({TableConst.fileChildsFID}, {TableConst.fileChildsCID}),
              FOREIGN KEY ({TableConst.fileChildsFID}) REFERENCES {TableConst.filesTName} ({TableConst.filesCoID}) ON DELETE CASCADE,
              FOREIGN KEY ({TableConst.fileChildsCID}) REFERENCES {TableConst.filesTName} ({TableConst.filesCoID}) ON DELETE CASCADE
            );";
        const string fileChildsNonEqualTrigger = @$"
            CREATE TRIGGER IF NOT EXISTS {TableConst.fileChildsConstraintName}
            BEFORE INSERT ON {TableConst.fileChildsTName}
            WHEN NEW.{TableConst.fileChildsFID} = NEW.{TableConst.fileChildsCID}
            BEGIN
	            SELECT RAISE(ABORT, 'fid and cid cannot be the same');
            END;";
        public static void CreateTablesAutoConn(SQLiteConnection conn) {
            conn.Open();
            using (SQLiteCommand cmd = new SQLiteCommand(conn)) {
                ExecuteNonQCommand(cmd, tagTCC);
                ExecuteNonQCommand(cmd, fileTCC);
                ExecuteNonQCommand(cmd, fileTagsTCC);
                ExecuteNonQCommand(cmd, fileChildsTCC);
                ExecuteNonQCommand(cmd, fileChildsNonEqualTrigger);
            }
            conn.Close();
        }
    }
}
