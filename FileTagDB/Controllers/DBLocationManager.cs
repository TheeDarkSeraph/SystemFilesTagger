using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagDB.Controllers {
    public class DBLocationManager {
        private const string defaultDBLocation = "./";
        private const string defaultDBName = "fts.db"; // FileTagSystem
        public string DBLocation = defaultDBLocation;
        public string DBName = defaultDBName;

        private static DBLocationManager? _instance = null;
        public static DBLocationManager Instance {
            get {
                if (_instance == null) {
                    _instance = new DBLocationManager();
                }
                return _instance;
            }
            private set { }
        }
        private DBLocationManager() {

        }
    }
}
