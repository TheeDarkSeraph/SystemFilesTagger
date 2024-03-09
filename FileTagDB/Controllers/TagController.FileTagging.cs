using FileTagDB.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileTagDB.Controllers {
    public partial class TagController {
        // NOTE: We will not handle ensuring file IDs here
        public void TagFile(int tagID, int fileID) {
            ConnectDB();
            TagFileConnected(tagID, fileID);
            DisconnectDB();
        }
        public void TagFiles(int tagID, List<int> fileIDs) { // files are assumed to be in DB
            ConnectDB();
            foreach(int fileID in fileIDs) {
                TagFileConnected(tagID, fileID);
            }
            DisconnectDB();
            // get all IDs, then untag them
        }
        public void UntagFile(int tagID, int fileID) {
            ConnectDB();
            UntagFileConnected(tagID, fileID);
            DisconnectDB();
        }
        public void UntagFiles(int tagID, List<int> fileIDs) {
            ConnectDB();
            foreach (int fileID in fileIDs) {
                UntagFileConnected(tagID, fileID);
            }
            DisconnectDB();
        }
        
        public List<(string,int)> GetFilesWithTag(int tagID) {
            List<(string, int)> fileRows = new();
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd, 
                    @$"SELECT filep.* FROM {TableConst.filesTName} filep JOIN
                    {TableConst.fileTagsTName} filetags ON
                    filetags.{TableConst.fileTagsCoFID} = filep.{TableConst.filesCoID}
                    WHERE filetags.{TableConst.fileTagsCoTID} = {tagID}
                "); // direct child only!
                while (reader.Read())
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
            }
            DisconnectDB();
            return fileRows;
        }
        public List<(string,int)>GetFilesWithTagQuery(string tagQuery) {
            Utils.LogToOutput("Original query " + tagQuery);
            tagQuery = AdjustQuery(tagQuery, $"t.{TableConst.tagsCoID}", TableConst.tagsTName, TableConst.tagsCoID, TableConst.tagsCoName);
            Utils.LogToOutput("Translated query " + tagQuery);
            List<(string, int)> fileRows = new();
            ConnectDB();
            using (var cmd = new SQLiteCommand(conn)) {
                SQLiteDataReader reader = DBController.ExecuteSelect(cmd,
                    @$"SELECT f.* FROM {TableConst.filesTName} f
                    JOIN {TableConst.fileTagsTName} ft ON ft.{TableConst.fileTagsCoFID} = f.{TableConst.filesCoID}
                    JOIN {TableConst.tagsTName} t ON filetags.{TableConst.fileTagsCoTID} = t.{TableConst.tagsCoID}
                    WHERE {tagQuery}
                "); // direct child only!
                while (reader.Read())
                    fileRows.Add(new((string)reader[$"{TableConst.filesCoPath}"], Convert.ToInt32(reader[$"{TableConst.filesCoID}"])));
            }
            DisconnectDB();
            return fileRows;
        }
        // test searching for tags with DB characters where * means '%' and ~ means '_'
        //  and user given '%' and '_' and '/' are all escaped with '/" character first
        // So, escapify characters, then replace with suitable
        // Note we assume here the string is correctly formatted
        // TODO: NOTE: You CANNOT have -a+b , user can't use - ( ) + * ~ for tag names
        //* 
        //* SELECT p.*
        //* FROM Pizza p
        //* JOIN PT pt ON p.ID = pt.PizzaID
        //* WHERE pt.ToppingID IN (SELECT ID FROM Toppings WHERE name LIKE 't%')
        //*      AND pt.ToppingID IN (SELECT ID FROM Toppings WHERE name IN ('cheese', 'sauce'));
        //* 
        //* 
        const string endingAnd = " AND";
        private string AdjustQuery(string tagQuery, string idPrefixed, string tableName, string idCol, string nameCol, bool ignoreCase=false) {

            int addedStringLength = $" {idPrefixed} IN (SELECT {idCol} FROM {tableName} WHERE {nameCol} LIKE SOMETHING){endingAnd}".Length;
            StringBuilder sb = new StringBuilder();
            // escape the escape character first, then escape the DB characters
            tagQuery = tagQuery.Replace(@"\", @"\\").Replace("_", @"\_").Replace("%", @"\%")
                .Replace('*','%').Replace('~','_');
            string[] parts = AnyWhiteSpace().Split(tagQuery);
            // a good approximation
            sb.EnsureCapacity(tagQuery.Length + parts.Length * addedStringLength + (7 + addedStringLength) * (Utils.Count(tagQuery, '+')) + 3000); // ( OR )

            foreach (string part in parts) {
                if (tagQuery.Contains('+')) {
                    string[] orGroup = part.Split();

                    sb.Append($" {idPrefixed} IN (SELECT {idCol} FROM {tableName} WHERE ");
                    for (int i = 0; i < orGroup.Length; i++)
                        orGroup[i] = ignoreCase ? $"LOWER({nameCol}) LIKE LOWER({part})" : $"{nameCol} LIKE {part}";
                    sb.Append(string.Join(" OR ", orGroup));
                    sb.Append(")");
                    sb.Append(endingAnd);
                } else {
                    string addNot = string.Empty;
                    if (tagQuery[0] == '-') {
                        tagQuery = tagQuery.Substring(1);
                        addNot = "NOT";
                    }
                    if (ignoreCase)
                        sb.Append($" {idPrefixed} {addNot} IN (SELECT {idCol} FROM {tableName} WHERE LOWER({nameCol}) LIKE LOWER({part})){endingAnd}");
                    else
                        sb.Append($" {idPrefixed} {addNot} IN (SELECT {idCol} FROM {tableName} WHERE {nameCol} LIKE {part}){endingAnd}");
                } 
            }
            sb.Length -= 4;
            return AnyWhiteSpace().Replace(sb.ToString(), " ");
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex AnyWhiteSpace();
    }
}
