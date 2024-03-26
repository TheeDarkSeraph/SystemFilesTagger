namespace FileTagDB {
    partial class FileAndTagsManager {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            button1 = new Button();
            label1 = new Label();
            label2 = new Label();
            tagSearchBox = new TextBox();
            currentPathTextBox = new SystemFilesTagger.FormComp.AutoCompleteTextBox();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            showFolderRadioBtn = new RadioButton();
            browseFolderRadioBtn = new RadioButton();
            label3 = new Label();
            tagTree = new TreeView();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            tagTextBox = new TextBox();
            radioButton3 = new RadioButton();
            radioButton4 = new RadioButton();
            label4 = new Label();
            label5 = new Label();
            radioButton5 = new RadioButton();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            button13 = new Button();
            button14 = new Button();
            button15 = new Button();
            label9 = new Label();
            tagFilterTextBox = new TextBox();
            label10 = new Label();
            tagFilterClearBtn = new Button();
            fileListView = new ListView();
            fileIconsImageList = new ImageList(components);
            panel1 = new Panel();
            goToParentButton = new Button();
            previousFileBtn = new Button();
            nextFileBtn = new Button();
            tagRecursivelyCheck = new CheckBox();
            cleanDBBtn = new Button();
            button8 = new Button();
            ignoreCaseCheck = new CheckBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.BackColor = Color.FromArgb(64, 64, 64);
            button1.FlatAppearance.BorderSize = 0;
            button1.FlatStyle = FlatStyle.Flat;
            button1.ForeColor = Color.FromArgb(224, 224, 224);
            button1.Location = new Point(664, 16);
            button1.Name = "button1";
            button1.Size = new Size(56, 23);
            button1.TabIndex = 0;
            button1.Text = "Go";
            button1.UseVisualStyleBackColor = false;
            button1.Click += GoToPath;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.FromArgb(224, 224, 224);
            label1.Location = new Point(12, 19);
            label1.Name = "label1";
            label1.Size = new Size(81, 15);
            label1.TabIndex = 1;
            label1.Text = "Current folder";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.FromArgb(224, 224, 224);
            label2.Location = new Point(12, 48);
            label2.Name = "label2";
            label2.Size = new Size(106, 15);
            label2.TabIndex = 2;
            label2.Text = "Custom tag search";
            // 
            // tagSearchBox
            // 
            tagSearchBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tagSearchBox.BackColor = Color.FromArgb(64, 64, 64);
            tagSearchBox.BorderStyle = BorderStyle.FixedSingle;
            tagSearchBox.ForeColor = Color.White;
            tagSearchBox.Location = new Point(138, 44);
            tagSearchBox.Name = "tagSearchBox";
            tagSearchBox.Size = new Size(422, 23);
            tagSearchBox.TabIndex = 4;
            // 
            // currentPathTextBox
            // 
            currentPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            currentPathTextBox.BackColor = Color.FromArgb(64, 64, 64);
            currentPathTextBox.BorderStyle = BorderStyle.FixedSingle;
            currentPathTextBox.ForeColor = Color.White;
            currentPathTextBox.Location = new Point(138, 16);
            currentPathTextBox.Name = "currentPathTextBox";
            currentPathTextBox.Size = new Size(452, 23);
            currentPathTextBox.TabIndex = 5;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.BackColor = Color.FromArgb(64, 64, 64);
            button2.FlatAppearance.BorderSize = 0;
            button2.FlatStyle = FlatStyle.Flat;
            button2.ForeColor = Color.FromArgb(224, 224, 224);
            button2.Location = new Point(726, 15);
            button2.Name = "button2";
            button2.Size = new Size(58, 23);
            button2.TabIndex = 6;
            button2.Text = "Browse";
            button2.UseVisualStyleBackColor = false;
            button2.Click += SelectFolderToBrowseTo;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.BackColor = Color.FromArgb(64, 64, 64);
            button3.FlatAppearance.BorderSize = 0;
            button3.FlatStyle = FlatStyle.Flat;
            button3.ForeColor = Color.FromArgb(224, 224, 224);
            button3.Location = new Point(664, 43);
            button3.Name = "button3";
            button3.Size = new Size(56, 23);
            button3.TabIndex = 7;
            button3.Text = "Search";
            button3.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.BackColor = Color.FromArgb(64, 64, 64);
            button4.FlatAppearance.BorderColor = Color.Black;
            button4.FlatAppearance.BorderSize = 0;
            button4.FlatStyle = FlatStyle.Flat;
            button4.ForeColor = Color.FromArgb(224, 224, 224);
            button4.Location = new Point(726, 43);
            button4.Name = "button4";
            button4.Size = new Size(58, 23);
            button4.TabIndex = 9;
            button4.Text = "Help";
            button4.UseVisualStyleBackColor = false;
            // 
            // showFolderRadioBtn
            // 
            showFolderRadioBtn.AutoSize = true;
            showFolderRadioBtn.ForeColor = Color.FromArgb(224, 224, 224);
            showFolderRadioBtn.Location = new Point(82, 3);
            showFolderRadioBtn.Name = "showFolderRadioBtn";
            showFolderRadioBtn.Size = new Size(132, 19);
            showFolderRadioBtn.TabIndex = 12;
            showFolderRadioBtn.Text = "Show in file explorer";
            showFolderRadioBtn.UseVisualStyleBackColor = true;
            // 
            // browseFolderRadioBtn
            // 
            browseFolderRadioBtn.AutoSize = true;
            browseFolderRadioBtn.Checked = true;
            browseFolderRadioBtn.ForeColor = Color.FromArgb(224, 224, 224);
            browseFolderRadioBtn.Location = new Point(3, 3);
            browseFolderRadioBtn.Name = "browseFolderRadioBtn";
            browseFolderRadioBtn.Size = new Size(73, 19);
            browseFolderRadioBtn.TabIndex = 11;
            browseFolderRadioBtn.TabStop = true;
            browseFolderRadioBtn.Text = "Browse it";
            browseFolderRadioBtn.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.ForeColor = Color.FromArgb(224, 224, 224);
            label3.Location = new Point(580, 105);
            label3.Name = "label3";
            label3.Size = new Size(203, 15);
            label3.TabIndex = 13;
            label3.Text = "Adjust selected file's tags";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // tagTree
            // 
            tagTree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            tagTree.BackColor = Color.FromArgb(64, 64, 64);
            tagTree.CheckBoxes = true;
            tagTree.ForeColor = Color.FromArgb(224, 224, 224);
            tagTree.Location = new Point(590, 123);
            tagTree.Name = "tagTree";
            tagTree.Size = new Size(204, 384);
            tagTree.TabIndex = 15;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button5.BackColor = Color.FromArgb(64, 64, 64);
            button5.FlatAppearance.BorderColor = Color.Gray;
            button5.FlatAppearance.BorderSize = 0;
            button5.FlatStyle = FlatStyle.Flat;
            button5.ForeColor = Color.FromArgb(224, 224, 224);
            button5.Location = new Point(590, 513);
            button5.Name = "button5";
            button5.Size = new Size(187, 23);
            button5.TabIndex = 16;
            button5.Text = "Delete Highlighted Tag";
            button5.UseVisualStyleBackColor = false;
            button5.Click += DeleteHighlightedTag_Clicked;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button6.BackColor = Color.FromArgb(64, 64, 64);
            button6.FlatAppearance.BorderColor = Color.Gray;
            button6.FlatAppearance.BorderSize = 0;
            button6.FlatStyle = FlatStyle.Flat;
            button6.ForeColor = Color.FromArgb(224, 224, 224);
            button6.Location = new Point(589, 587);
            button6.Name = "button6";
            button6.Size = new Size(90, 23);
            button6.TabIndex = 17;
            button6.Text = "Add Tag";
            button6.UseVisualStyleBackColor = false;
            button6.Click += AddTag_Clicked;
            // 
            // button7
            // 
            button7.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button7.BackColor = Color.FromArgb(64, 64, 64);
            button7.FlatAppearance.BorderColor = Color.Gray;
            button7.FlatAppearance.BorderSize = 0;
            button7.FlatStyle = FlatStyle.Flat;
            button7.ForeColor = Color.FromArgb(224, 224, 224);
            button7.Location = new Point(685, 587);
            button7.Name = "button7";
            button7.Size = new Size(92, 23);
            button7.TabIndex = 18;
            button7.Text = "Remove Tag";
            button7.UseVisualStyleBackColor = false;
            button7.Click += RemoveTag_Clicked;
            // 
            // tagTextBox
            // 
            tagTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            tagTextBox.BackColor = Color.FromArgb(64, 64, 64);
            tagTextBox.BorderStyle = BorderStyle.FixedSingle;
            tagTextBox.ForeColor = Color.White;
            tagTextBox.Location = new Point(590, 558);
            tagTextBox.Name = "tagTextBox";
            tagTextBox.Size = new Size(187, 23);
            tagTextBox.TabIndex = 19;
            // 
            // radioButton3
            // 
            radioButton3.Anchor = AnchorStyles.Bottom;
            radioButton3.AutoSize = true;
            radioButton3.Checked = true;
            radioButton3.ForeColor = Color.FromArgb(224, 224, 224);
            radioButton3.Location = new Point(12, 585);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new Size(109, 19);
            radioButton3.TabIndex = 21;
            radioButton3.TabStop = true;
            radioButton3.Text = "Clear tag search";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            radioButton4.Anchor = AnchorStyles.Bottom;
            radioButton4.AutoSize = true;
            radioButton4.ForeColor = Color.FromArgb(224, 224, 224);
            radioButton4.Location = new Point(127, 585);
            radioButton4.Name = "radioButton4";
            radioButton4.Size = new Size(162, 19);
            radioButton4.TabIndex = 22;
            radioButton4.Text = "Show tagged files inside it";
            radioButton4.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom;
            label4.AutoSize = true;
            label4.ForeColor = Color.FromArgb(224, 224, 224);
            label4.Location = new Point(12, 565);
            label4.Name = "label4";
            label4.Size = new Size(265, 15);
            label4.TabIndex = 23;
            label4.Text = "How to handle browsing folders from tag search:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.FromArgb(224, 224, 224);
            label5.Location = new Point(12, 81);
            label5.Name = "label5";
            label5.Size = new Size(177, 15);
            label5.TabIndex = 24;
            label5.Text = "How to handle opening folders :";
            // 
            // radioButton5
            // 
            radioButton5.Anchor = AnchorStyles.Bottom;
            radioButton5.AutoSize = true;
            radioButton5.ForeColor = Color.FromArgb(224, 224, 224);
            radioButton5.Location = new Point(295, 585);
            radioButton5.Name = "radioButton5";
            radioButton5.Size = new Size(194, 19);
            radioButton5.TabIndex = 25;
            radioButton5.Text = "Show tagged files under its path";
            radioButton5.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Bottom;
            label6.ForeColor = Color.FromArgb(224, 224, 224);
            label6.Location = new Point(12, 512);
            label6.Name = "label6";
            label6.Size = new Size(563, 15);
            label6.TabIndex = 29;
            label6.Text = "Addition and removal does not affect the actual files (Press help in top right for more info)";
            label6.TextAlign = ContentAlignment.TopCenter;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Bottom;
            label7.AutoSize = true;
            label7.ForeColor = Color.FromArgb(224, 224, 224);
            label7.Location = new Point(283, 565);
            label7.Name = "label7";
            label7.Size = new Size(267, 15);
            label7.TabIndex = 31;
            label7.Text = "(Red files are missing files that have been moved)";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top;
            label8.ForeColor = Color.FromArgb(224, 224, 224);
            label8.Location = new Point(221, 102);
            label8.Name = "label8";
            label8.Size = new Size(88, 15);
            label8.TabIndex = 32;
            label8.Text = "5-_-#  0  #-_-S";
            label8.TextAlign = ContentAlignment.TopCenter;
            // 
            // button13
            // 
            button13.Anchor = AnchorStyles.Top;
            button13.BackColor = Color.FromArgb(64, 64, 64);
            button13.FlatAppearance.BorderSize = 0;
            button13.FlatStyle = FlatStyle.Flat;
            button13.ForeColor = Color.FromArgb(224, 224, 224);
            button13.Location = new Point(315, 98);
            button13.Name = "button13";
            button13.Size = new Size(22, 22);
            button13.TabIndex = 33;
            button13.Text = ">";
            button13.UseVisualStyleBackColor = false;
            // 
            // button14
            // 
            button14.Anchor = AnchorStyles.Top;
            button14.BackColor = Color.FromArgb(64, 64, 64);
            button14.FlatAppearance.BorderSize = 0;
            button14.FlatStyle = FlatStyle.Flat;
            button14.ForeColor = Color.FromArgb(224, 224, 224);
            button14.Location = new Point(190, 98);
            button14.Name = "button14";
            button14.Size = new Size(22, 22);
            button14.TabIndex = 34;
            button14.Text = "<";
            button14.UseVisualStyleBackColor = false;
            // 
            // button15
            // 
            button15.Anchor = AnchorStyles.Bottom;
            button15.BackColor = Color.FromArgb(64, 64, 64);
            button15.Enabled = false;
            button15.FlatAppearance.BorderColor = Color.Gray;
            button15.FlatAppearance.BorderSize = 0;
            button15.FlatStyle = FlatStyle.Flat;
            button15.ForeColor = Color.FromArgb(224, 224, 224);
            button15.Location = new Point(466, 531);
            button15.Name = "button15";
            button15.Size = new Size(108, 23);
            button15.TabIndex = 35;
            button15.Text = "Fix missing file";
            button15.UseVisualStyleBackColor = false;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label9.ForeColor = Color.FromArgb(224, 224, 224);
            label9.Location = new Point(613, 539);
            label9.Name = "label9";
            label9.Size = new Size(140, 15);
            label9.TabIndex = 37;
            label9.Text = "Enter tag to add/remove";
            label9.TextAlign = ContentAlignment.TopCenter;
            // 
            // tagFilterTextBox
            // 
            tagFilterTextBox.BackColor = Color.FromArgb(64, 64, 64);
            tagFilterTextBox.BorderStyle = BorderStyle.FixedSingle;
            tagFilterTextBox.ForeColor = Color.White;
            tagFilterTextBox.Location = new Point(604, 80);
            tagFilterTextBox.Name = "tagFilterTextBox";
            tagFilterTextBox.Size = new Size(124, 23);
            tagFilterTextBox.TabIndex = 38;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.ForeColor = Color.FromArgb(224, 224, 224);
            label10.Location = new Point(533, 83);
            label10.Name = "label10";
            label10.Size = new Size(65, 15);
            label10.TabIndex = 39;
            label10.Text = "Filter Tags :";
            // 
            // tagFilterClearBtn
            // 
            tagFilterClearBtn.BackColor = Color.FromArgb(64, 64, 64);
            tagFilterClearBtn.FlatAppearance.BorderSize = 0;
            tagFilterClearBtn.FlatStyle = FlatStyle.Flat;
            tagFilterClearBtn.ForeColor = Color.FromArgb(224, 224, 224);
            tagFilterClearBtn.Location = new Point(734, 79);
            tagFilterClearBtn.Name = "tagFilterClearBtn";
            tagFilterClearBtn.Size = new Size(49, 23);
            tagFilterClearBtn.TabIndex = 40;
            tagFilterClearBtn.Text = "Clear";
            tagFilterClearBtn.UseVisualStyleBackColor = false;
            tagFilterClearBtn.Click += ClearTagFiler;
            // 
            // fileListView
            // 
            fileListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            fileListView.BackColor = Color.FromArgb(64, 64, 64);
            fileListView.Font = new Font("Segoe UI", 9F);
            fileListView.ForeColor = Color.FromArgb(224, 224, 224);
            fileListView.LargeImageList = fileIconsImageList;
            fileListView.Location = new Point(12, 123);
            fileListView.Name = "fileListView";
            fileListView.Size = new Size(562, 383);
            fileListView.TabIndex = 41;
            fileListView.UseCompatibleStateImageBehavior = false;
            // 
            // fileIconsImageList
            // 
            fileIconsImageList.ColorDepth = ColorDepth.Depth32Bit;
            fileIconsImageList.ImageSize = new Size(32, 32);
            fileIconsImageList.TransparentColor = Color.Transparent;
            // 
            // panel1
            // 
            panel1.Controls.Add(browseFolderRadioBtn);
            panel1.Controls.Add(showFolderRadioBtn);
            panel1.Location = new Point(190, 74);
            panel1.Name = "panel1";
            panel1.Size = new Size(231, 24);
            panel1.TabIndex = 42;
            // 
            // goToParentButton
            // 
            goToParentButton.BackColor = Color.DimGray;
            goToParentButton.BackgroundImage = SystemFilesTagger.res.ArrowUp;
            goToParentButton.BackgroundImageLayout = ImageLayout.Stretch;
            goToParentButton.Enabled = false;
            goToParentButton.FlatAppearance.BorderSize = 0;
            goToParentButton.FlatStyle = FlatStyle.Flat;
            goToParentButton.ForeColor = Color.White;
            goToParentButton.Location = new Point(104, 17);
            goToParentButton.Name = "goToParentButton";
            goToParentButton.Size = new Size(28, 20);
            goToParentButton.TabIndex = 43;
            goToParentButton.UseVisualStyleBackColor = false;
            goToParentButton.MouseClick += GoToParent_Clicked;
            // 
            // previousFileBtn
            // 
            previousFileBtn.BackColor = Color.FromArgb(20, 60, 110);
            previousFileBtn.BackgroundImage = SystemFilesTagger.res.ArrowLeft;
            previousFileBtn.BackgroundImageLayout = ImageLayout.Stretch;
            previousFileBtn.Enabled = false;
            previousFileBtn.FlatAppearance.BorderSize = 0;
            previousFileBtn.FlatStyle = FlatStyle.Flat;
            previousFileBtn.Location = new Point(596, 17);
            previousFileBtn.Name = "previousFileBtn";
            previousFileBtn.Size = new Size(28, 20);
            previousFileBtn.TabIndex = 44;
            previousFileBtn.UseVisualStyleBackColor = false;
            previousFileBtn.MouseClick += GoToPreviousFile_BtnClicked;
            // 
            // nextFileBtn
            // 
            nextFileBtn.BackColor = Color.FromArgb(20, 60, 110);
            nextFileBtn.BackgroundImage = SystemFilesTagger.res.ArrowRight;
            nextFileBtn.BackgroundImageLayout = ImageLayout.Stretch;
            nextFileBtn.Enabled = false;
            nextFileBtn.FlatAppearance.BorderSize = 0;
            nextFileBtn.FlatStyle = FlatStyle.Flat;
            nextFileBtn.Location = new Point(630, 17);
            nextFileBtn.Name = "nextFileBtn";
            nextFileBtn.Size = new Size(28, 20);
            nextFileBtn.TabIndex = 45;
            nextFileBtn.UseVisualStyleBackColor = false;
            nextFileBtn.MouseClick += GoToNextFile_BtnClicked;
            // 
            // tagRecursivelyCheck
            // 
            tagRecursivelyCheck.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            tagRecursivelyCheck.AutoSize = true;
            tagRecursivelyCheck.ForeColor = Color.FromArgb(224, 224, 224);
            tagRecursivelyCheck.Location = new Point(13, 534);
            tagRecursivelyCheck.Name = "tagRecursivelyCheck";
            tagRecursivelyCheck.Size = new Size(296, 19);
            tagRecursivelyCheck.TabIndex = 46;
            tagRecursivelyCheck.Text = "Tagging folders applies tag to all files under its path";
            tagRecursivelyCheck.UseVisualStyleBackColor = true;
            // 
            // cleanDBBtn
            // 
            cleanDBBtn.BackColor = Color.FromArgb(64, 64, 64);
            cleanDBBtn.FlatAppearance.BorderColor = Color.Gray;
            cleanDBBtn.FlatAppearance.BorderSize = 0;
            cleanDBBtn.FlatStyle = FlatStyle.Flat;
            cleanDBBtn.ForeColor = Color.FromArgb(224, 224, 224);
            cleanDBBtn.Location = new Point(13, 610);
            cleanDBBtn.Name = "cleanDBBtn";
            cleanDBBtn.Size = new Size(253, 23);
            cleanDBBtn.TabIndex = 47;
            cleanDBBtn.Text = "Remove untagged files in DB (Clean up)";
            cleanDBBtn.UseVisualStyleBackColor = false;
            // 
            // button8
            // 
            button8.BackColor = Color.FromArgb(64, 64, 64);
            button8.FlatAppearance.BorderColor = Color.Gray;
            button8.FlatAppearance.BorderSize = 0;
            button8.FlatStyle = FlatStyle.Flat;
            button8.ForeColor = Color.FromArgb(224, 224, 224);
            button8.Location = new Point(354, 531);
            button8.Name = "button8";
            button8.Size = new Size(106, 23);
            button8.TabIndex = 48;
            button8.Text = "Remove selected files";
            button8.UseVisualStyleBackColor = false;
            // 
            // ignoreCaseCheck
            // 
            ignoreCaseCheck.AutoSize = true;
            ignoreCaseCheck.ForeColor = Color.FromArgb(224, 224, 224);
            ignoreCaseCheck.Location = new Point(572, 46);
            ignoreCaseCheck.Name = "ignoreCaseCheck";
            ignoreCaseCheck.Size = new Size(86, 19);
            ignoreCaseCheck.TabIndex = 49;
            ignoreCaseCheck.Text = "Ignore case";
            ignoreCaseCheck.UseVisualStyleBackColor = true;
            // 
            // FileAndTagsManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(32, 32, 32);
            ClientSize = new Size(797, 644);
            Controls.Add(ignoreCaseCheck);
            Controls.Add(button8);
            Controls.Add(cleanDBBtn);
            Controls.Add(tagRecursivelyCheck);
            Controls.Add(nextFileBtn);
            Controls.Add(previousFileBtn);
            Controls.Add(goToParentButton);
            Controls.Add(panel1);
            Controls.Add(fileListView);
            Controls.Add(tagFilterClearBtn);
            Controls.Add(label10);
            Controls.Add(tagFilterTextBox);
            Controls.Add(label9);
            Controls.Add(button15);
            Controls.Add(button14);
            Controls.Add(button13);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(radioButton5);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(radioButton4);
            Controls.Add(radioButton3);
            Controls.Add(tagTextBox);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(tagTree);
            Controls.Add(label3);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(currentPathTextBox);
            Controls.Add(tagSearchBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
            ForeColor = Color.FromArgb(224, 224, 224);
            MinimumSize = new Size(813, 646);
            Name = "FileAndTagsManager";
            Text = "Form1";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Label label1;
        private Label label2;
        private TextBox tagSearchBox;
        private SystemFilesTagger.FormComp.AutoCompleteTextBox currentPathTextBox;
        private Button button2;
        private Button button3;
        private Button button4;
        private RadioButton showFolderRadioBtn;
        private RadioButton browseFolderRadioBtn;
        private Label label3;
        private TreeView tagTree;
        private Button button5;
        private Button button6;
        private Button button7;
        private TextBox tagTextBox;
        private RadioButton radioButton3;
        private RadioButton radioButton4;
        private Label label4;
        private Label label5;
        private RadioButton radioButton5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Button button13;
        private Button button14;
        private Button button15;
        private Label label9;
        private TextBox tagFilterTextBox;
        private Label label10;
        private Button tagFilterClearBtn;
        private ListView fileListView;
        private ImageList fileIconsImageList;
        private Panel panel1;
        private Button goToParentButton;
        private Button previousFileBtn;
        private Button nextFileBtn;
        private CheckBox tagRecursivelyCheck;
        private Button cleanDBBtn;
        private Button button8;
        private CheckBox ignoreCaseCheck;
    }
}
