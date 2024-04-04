using System.Data.SQLite;
using System.Diagnostics;

namespace FileTagDB.Controllers {
    public partial class DBController {
        // AC -> AutoConnect to database (has open and close)
        static DBLocationManager lm = DBLocationManager.Instance;
        public static string GetConnPath() {
            return Path.Combine(lm.DBLocation, lm.DBName);
        }
        public static int ExecuteNonQCommand(SQLiteCommand cmd, string tcc) {
            cmd.CommandText = tcc;
            int affectedRows;
            try {
                affectedRows = cmd.ExecuteNonQuery();
            } catch (SQLiteException e) {
                Utils.LogToOutput("Here "+e.Message+" CMD "+tcc);
                return -1;
            }
            return affectedRows;
        }
        public static SQLiteDataReader ExecuteSelect(SQLiteCommand cmd, string tcc) {
                cmd.CommandText = tcc;
                return cmd.ExecuteReader();
        }
        public static bool TryExecuteSingleRead(SQLiteCommand cmd, string cmdText, string columnName, out object? result) {
            cmd.CommandText = cmdText;
            var reader = cmd.ExecuteReader();
            if (!reader.Read()) {
                result = null;
                reader.Close();
                return false;
            }
            result = reader[columnName];
            reader.Close();
            return true;
        }
        public static SQLiteConnection GetDBConnection() {
            return new SQLiteConnection("Data Source=" +GetConnPath()+ "");
        }
        public static void CreateDBIfNotExist(string dbLocation, string dbName) {
            if (!DBFileExists(dbLocation, dbName)) { // we fully assume that the file cannot exist without the correct tables
                Utils.LogToOutput("Location of run " + Directory.GetCurrentDirectory());
                Debug.WriteLine("Creating db file");
                SQLiteConnection.CreateFile(Path.Combine(dbLocation, dbName));
                using (var conn = GetDBConnection()) {
                    CreateTablesAutoConn(conn); // we will not handle user meddling...
                }
            }
        }
        public static bool DBFileExists(string dbLocation, string dbName) {
            return File.Exists(Path.Combine(dbLocation,dbName));
        }
        public static bool DBConnectionOk(string dbLocation, string dbName) {
            Utils.LogToOutput("Location of run " + Directory.GetCurrentDirectory());
            if (!DBFileExists(dbLocation, dbName))
                return false;
            try {
                SQLiteConnection sqlConn = GetDBConnection();
                sqlConn.Open();
                sqlConn.Close();
            } catch (Exception e) {
                Utils.LogToOutput(e.Message);
                return false;
            }
            return true;
        }
        public static void DeleteDB(string dbLocation, string dbName) {
            File.Delete(Path.Combine(dbLocation, dbName));
            Utils.LogToOutput("Bombs away!");
        }
        public static void DeleteTablesData(SQLiteConnection conn) {
            using (SQLiteCommand cmd = new SQLiteCommand(conn)) {
                foreach (string tableName in TableConst.allTables) {
                    ExecuteNonQCommand(cmd, "DELETE FROM "+tableName+";");
                }
            }
        }
        public static void DeleteTableData(SQLiteConnection conn, string tableName) {
            using (SQLiteCommand cmd = new SQLiteCommand(conn)) {
                ExecuteNonQCommand(cmd, "DELETE FROM " + tableName + ";");
            }
        }
        /*
         * The ExecuteNonQuery Method returns the number of row(s) affected by either an INSERT, an UPDATE or a DELETE. 
         *      This method is to be used to perform DML (data manipulation language) statements as stated previously.
         * The ExecuteReader Method will return the result set of a SELECT. This method is to be used when you're querying
         *      for a bunch of results, such as rows from a table, view, whatever.
         * The ExecuteScalar Method will return a single value in the first row, first column from a SELECT statement. 
         *      This method is to be used when you expect only one value from the query to be returned.
        */
    }
}
