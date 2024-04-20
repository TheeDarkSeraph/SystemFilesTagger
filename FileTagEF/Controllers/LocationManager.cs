namespace FileTagEF.Controllers {
    public class LocationManager {
        private const string defaultDBLocation = "./";
        private const string defaultDBName = "fts.db"; // FileTagSystem
        public string DBLocation = defaultDBLocation;
        public string DBName = defaultDBName;

        private static LocationManager? _instance = null;
        public static LocationManager Instance {
            get {
                if (_instance == null) {
                    _instance = new LocationManager();
                }
                return _instance;
            }
            private set { }
        }
        private LocationManager() {

        }
    }
}
