using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemFilesTagger.FormComp {
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8618 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8602 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

    public class AutoCompleteTextBox : TextBox {
        // TODO: Rework this class as follows:
        /* 
         * 1- Stop all ding sounds by handling them if more appear
         * 2- Enable clicking the dropdown items to choose them
         * 3- The auto complete list seems to remove items already inserted,
         *      this is a bonus I believe so we will leave it
         * 4- We want a variant for the search box as a dropdown (for drives, then desktop, documents, music, image [assuming windows here])
         * 
         * 5- We will be in control of the list. Just make sure it works as intended
         *      (by reading the code) and rework any code if needed
         *      
         */
        
        private ListBox _listBox;
        private bool _isAdded;
        private string _formerValue = string.Empty;
        // this is the array to check from for match
        public List<(string, string)> shortNFullPath; // short name => full path
        public Action OnShortcutChosen;
        private void InvokeShortcutChosen() {
            if (OnShortcutChosen != null) {
                OnShortcutChosen();
            }
        }
        public AutoCompleteTextBox() {
            InitializeComponent();
            ResetListBox();
            Click += OnTextBoxClicked;
            _listBox.Click += OnListBoxClicked;
            _listBox.LostFocus += HasLostFocus;
            this.LostFocus += HasLostFocus;
        }
        private void HasLostFocus(object? sender, EventArgs e) {
            if (!this.Focused && !_listBox.Focused)
                ResetListBox();
        }
        private void ApplySelectedItemInList() {
            if (_listBox.SelectedItem != null) {
                string word = (string)_listBox.SelectedItem;
                foreach (var value in shortNFullPath) {
                    if (value.Item1.ToLower() == word.ToLower()) {
                        InsertWord(value.Item2);
                        break;
                    }
                }
            }
            ResetListBox();
            _formerValue = Text;
            InvokeShortcutChosen();
        }
        private void OnTextBoxClicked(object? o, EventArgs e) {
            UpdateListBox("");
            ShowListBox();
        }

        private void OnListBoxClicked(object? o, EventArgs e) {
            if (_listBox.SelectedItem != null) {
                ApplySelectedItemInList();
            }
        }
        private void InitializeComponent() {
            _listBox = new ListBox();
            KeyDown += this_KeyDown;
            KeyUp += this_KeyUp;
        }

        private void ShowListBox() {
            if (!_isAdded) {
                Parent?.Controls.Add(_listBox);
                _listBox.Left = Left;
                _listBox.Top = Top + Height;
                _isAdded = true;
            }
            _listBox.Visible = true;
            _listBox.BringToFront();
        }

        private void ResetListBox() {
            _listBox.Visible = false;
        }

        private void this_KeyUp(object sender, KeyEventArgs e) {
            UpdateListBox();
        }

        private void this_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.Tab:
                case Keys.Enter: {
                        if (_listBox.Visible) {
                            ApplySelectedItemInList();
                            e.Handled = true;
                            e.SuppressKeyPress = true;
                        }
                        break;
                    }
                case Keys.Down: {
                        if ((_listBox.Visible) && (_listBox.SelectedIndex < _listBox.Items.Count - 1))
                            _listBox.SelectedIndex++;

                        break;
                    }
                case Keys.Up: {
                        if ((_listBox.Visible) && (_listBox.SelectedIndex > 0))
                            _listBox.SelectedIndex--;

                        break;
                    }
            }
        }

        protected override bool IsInputKey(Keys keyData) {
            switch (keyData) {
                case Keys.Tab:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        private void UpdateListBox() {
            if (Text == _formerValue) return;
            _formerValue = Text;
            string word = GetWord().ToLower();
            if (shortNFullPath != null && word.Length > 0) {
                UpdateListBox(word);
            } else {
                ResetListBox();
            }
        }
        private void UpdateListBox(string word) {
            List<string> matches = new List<string>();
            foreach (var value in shortNFullPath) {
                if (value.Item1.ToLower().StartsWith(word)) {
                    matches.Add(value.Item1);
                }
            }
            if (matches.Count > 0) {
                ShowListBox();
                _listBox.Items.Clear();
                foreach (string match in matches)
                    _listBox.Items.Add(match);
                _listBox.SelectedIndex = 0;
                _listBox.Height = 0;
                _listBox.Width = 0;
                Focus();
                using (Graphics graphics = _listBox.CreateGraphics()) {
                    for (int i = 0; i < _listBox.Items.Count; i++) {
                        _listBox.Height += _listBox.GetItemHeight(i);
                        // it item width is larger than the current one
                        // set it to the new max item width
                        // GetItemRectangle does not work for me
                        // we add a little extra space by using '_'
                        int itemWidth = (int)graphics.MeasureString(((string)_listBox.Items[i]) + "_", _listBox.Font).Width;
                        _listBox.Width = (_listBox.Width < itemWidth) ? itemWidth : _listBox.Width;
                    }
                }
            } else {
                ResetListBox();
            }
        }

        private string GetWord() {
            string text = Text;
            int pos = SelectionStart;

            int posStart = text.LastIndexOf(' ', (pos < 1) ? 0 : pos - 1);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOf(' ', pos);
            posEnd = (posEnd == -1) ? text.Length : posEnd;

            int length = ((posEnd - posStart) < 0) ? 0 : posEnd - posStart;

            return text.Substring(posStart, length);
        }

        private void InsertWord(string newTag) {
            string text = Text;
            int pos = SelectionStart;

            int posStart = text.LastIndexOf(' ', (pos < 1) ? 0 : pos - 1);
            posStart = (posStart == -1) ? 0 : posStart + 1;
            int posEnd = text.IndexOf(' ', pos);

            string firstPart = text.Substring(0, posStart) + newTag;
            string updatedText = firstPart + ((posEnd == -1) ? "" : text.Substring(posEnd, text.Length - posEnd));


            Text = updatedText;
            SelectionStart = firstPart.Length;
        }

        // this checks and returns words already in the textBox to remove from suggestion
        //public List<string> SelectedValues {
        //    get {
        //        string[] result = Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //        return new List<string>(result);
        //    }
        //}

    }
#pragma warning restore CS8602
#pragma warning restore CS8618
#pragma warning restore CS8622

}
