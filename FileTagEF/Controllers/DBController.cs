using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagEF.Controllers {
    public class DBController {
        static LocationManager lm = LocationManager.Instance;
        public static string GetConnPath() {
            return Path.Combine(lm.DBLocation, lm.DBName);
        }
        public static TagDBContext GetContext() {
            string connectionString = "Data Source=" + GetConnPath();
            var builder = new DbContextOptionsBuilder<TagDBContext>();
            builder.UseSqlite(connectionString);
            return new TagDBContext(builder.Options);
        }
    }
}
