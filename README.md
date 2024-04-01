# SystemFilesTagger
A complementary file system to help organize and retrieve files faster.
Helps add tags (categories) to files, and has the ability to do a complex search to retrieve files with a certain search criteria specific to associated tags.

This project is made with the idea that even with regular folder naming and organizing files according to the folders they are in. There is a level of limitation to how organized it can be + you can also forget where the files are, and what their names were.

This System allows you to create tags and add them to files to be able to search for those files according to those tags (you can add any amount of tags to a single file).

This way when you don't know the name/location of what your searching for, or when you want to retrieve certain files that correlate to something, you can use the tags search system to look for files with the tags (like categories) that you want.

For more on how to use the file, the ProgramGuide.cs file has a description. You can also see this description by running the program and pressing the help button. 

Supports win7 and above.

Winforms build link [here](https://drive.google.com/file/d/1Pb2wncjAGtJSwL17-cvEiAkMT6kyhcY4/view?usp=drive_link)

### About

Used .net6 for more compatibility (and .net8.0.3 currently has a bug error msg when using builds with treeviews, on selection update)

GUI is made with winforms currently

DB is tested with xunit, and is separated into its own project.

SQLite ADO.NET was used for the DB system for portability and simplicity.

### Future

* Support to entity frame work (and their corresponding xunit tests)
* A WPF variant for better cross platform support
