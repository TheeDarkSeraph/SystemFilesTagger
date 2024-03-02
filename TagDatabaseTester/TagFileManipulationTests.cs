using FileTagDB.Controllers;
using FileTagDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace TagDatabaseTester {
    public class TagFileManipulationTests {

        #region Prints and Helper functions
        private readonly ITestOutputHelper output;
        private readonly DBLocationManager lm;
        public TagFileManipulationTests(ITestOutputHelper output) {
            this.output = output;
            lm = DBLocationManager.Instance;
            Utils.SetPrinter(output.WriteLine);
            lm.DBName = "testTags.db";
            DBController.DeleteDB(lm.DBLocation, lm.DBName); // make sure its not there to begin with
        }
        private void PrintMsg(string msg) {
            output.WriteLine(msg);
        }
        #endregion

        #region Tag Related Test (mostly)


        #endregion


        // tag files, untag files

        // tag files delete tag relation should delete (cascade test)

        [Fact]
        public void AreYouReady() {

        }

        #region File Related Tests

        #endregion
    }
}
