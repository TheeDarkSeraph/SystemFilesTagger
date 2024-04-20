using FileTagEF;
using FileTagEF.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TagEFTester {
    public class EFDBTester {
        public EFDBTester() {
            LocationManager lm = LocationManager.Instance;
            lm.DBName = "testTags.db";
        }
        // dotnet ef --project .\FileTagEF\ migrations add InitialCreate
        [Fact]
        public void TestCreateDB() {
            using (var context = DBController.GetContext()) {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

        }
    }
}