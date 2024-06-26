﻿using System.Text.RegularExpressions;

namespace FileTagDB {
    public static partial class Utils {

        static Action<string>? printer;
        public static readonly List<string> Empty= new List<string>();
        public static void SetPrinter(Action<string> outputter) {
            printer = outputter;
        }
        public static void LogToOutput(string msg) {
            if (printer == null) {
                Console.WriteLine(msg);
                return;
            }
            try {
                printer(msg);
            }catch(Exception) {
                Console.WriteLine(msg);
                printer = null;
            }
        }
        public static int Count(string str, char c) {
            int count = 0;
            for(int i = 0; i < str.Length; i++) {
                if (str[i] == c)
                    count++;
            }
            return count;
        }
        public static string? GetFileExtension(string file) {
            if (file.Contains(".") && file[file.Length - 1] != '.') { // does not end with a dot
                return file.Substring(file.LastIndexOf(".")).ToLower();
            } else {
                return null;
            }

        }
        public static string ShortenFileName(string file) {
            if (file.Length <= 28)
                return file;
            string fileStart = file.Substring(0, 13)+"..";
            file = file.Substring(13);
            string fileEnd = "";
            if (file.Contains(Path.PathSeparator)) {
                fileEnd = file.Substring(file.LastIndexOf(Path.PathSeparator));
            } else {
                fileEnd = file.Substring(file.Length - 15, 15); // should have enough characters
            }
            return fileStart + fileEnd;
        }

        public static string GetShortcutTarget(string file) {
            try {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk") {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream)) {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1) {                      // Bit 1 set means we have to
                                                                 // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1) {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    } else {
                        return link;
                    }
                }
            } catch {
                return string.Empty;
            }
        }
        //[GeneratedRegex(@"\s+")]
        //public static partial Regex AnyWhiteSpace();
    }
}
