﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagDatabaseTester {
    public static class DataHolder {
        public static List<string> sampleTags = new List<string>();
        public static void PopulateTags() {
            sampleTags = File.ReadAllLines("taglist.txt").ToList<string>();
        }


        public static List<string> sampleFilePaths = new List<string>();
        public static void PopulateFiles() {
            sampleFilePaths = File.ReadAllLines("filepaths.txt").ToList<string>();
        }



    }
}
