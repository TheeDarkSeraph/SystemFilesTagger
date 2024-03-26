using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTagDB {
    public partial class FileAndTagsManager{
        // TODO: Show the full path of selected file (first selected index only), or no file selected (put a label for it)

        #region Searching tagged files
        // TODO: view search result of files, and mark searching tags as true
        //          when user searches the search bar, mark searching tags as false
        //      Depending on certain "radio button checks" we will call update the browsing
        //          with files containing certain features
        // Adjust browsing to call "Tagged browsing" functions
        //      we could separate this part into another class

        // TODO: Adjust the searching tags boolean in regular browsing class
        bool isSearchingTags = false;

        void SearchTag_Clicked() {
            if (!CheckTagQueryValidity())
                return;
            currentPathTextBox.Text = "*Tag Search*";
            isSearchingTags = true;
            List<(string, int)> fileData = tc.GetFilesWithTagQuery(tagSearchBox.Text, ignoreCaseCheck.Checked);
            currentActiveFiles.Clear();
            foreach((string,int)fileEntry in fileData) 
                currentActiveFiles.Add(fileEntry.Item1);
            FilterNodes(tagFilterTextBox.Text);
            ReadjustFileListTagMode();
        }
        private void ReadjustFileListTagMode() {
            // TODO: if file info is null, or path does not exist then add full path with red
            fileListView.Clear();
            fileIconsImageList.Images.Clear();
            int filesAdded = 0;
            foreach (string filepath in currentActiveFiles) {
                //Debug.WriteLine("HERE ! " + filepath);
                Icon? fileIcon = GetFileIcon(filepath);
                if (fileIcon == null)
                    fileIcon = defaultFileIcon;
                fileIconsImageList.Images.Add(fileIcon);
                fileListView.Items.Add(Utils.ShortenFileName(filepath), filesAdded);
                filesAdded++;
            }
            AdjustSelectedFilesTags();
        }
        private bool CheckTagQueryValidity() {
            tagSearchBox.Text = Utils.AnyWhiteSpace().Replace(tagSearchBox.Text, " ");
            string query = tagSearchBox.Text;
            string[] parts = Utils.AnyWhiteSpace().Split(query);
            bool validTags = true;
            string errorMsg = "";
            foreach(string tag in parts) {
                if (tag[0] == '+') {
                    errorMsg = "Can't start with '+'";
                    validTags = false;
                    break;
                }
                if (tag.Contains("++") || tag.Contains("-+") || tag.Contains("**")) {
                    errorMsg = "Can't have '++' or '-+' or '**'";
                    validTags = false;
                    break;
                }
                if (tag[0]=='-') {
                    if (tag.Length == 1) {
                        errorMsg = "'-' can't be a tag";
                        validTags = false;
                        break;
                    }
                    if (tag[1] == '-') {
                        errorMsg = "Can't start with --";
                        validTags = false;
                        break;
                    }
                    if (tag.Contains("+")) {
                        errorMsg = "Exclusions can't contain '+' because it is meaningless";
                        validTags = false;
                        break;
                    }
                }
            }
            if (!validTags) {
                MessageBox.Show(errorMsg, "Invalid search, please fix", MessageBoxButtons.OK);
            }
            return validTags;
        }

        void BrowseFilesInFolder() {
            currentPathTextBox.Text = "path/";
            // load files under the folder directly if it is a folder and the folder exists    
            // otherwise just show a copyable link path
        }
        void BrowseFilesInPath() {
            currentPathTextBox.Text = "path/*";
            // load files under the folder's path if it is a folder and the folder exists
            // otherwise just show a copyable link path
        }


        #endregion



    }
}
