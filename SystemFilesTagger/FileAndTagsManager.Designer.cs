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
            textBox2 = new TextBox();
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
            textBox1 = new TextBox();
            radioButton3 = new RadioButton();
            radioButton4 = new RadioButton();
            label4 = new Label();
            label5 = new Label();
            radioButton5 = new RadioButton();
            button9 = new Button();
            button10 = new Button();
            button11 = new Button();
            label6 = new Label();
            button12 = new Button();
            label7 = new Label();
            label8 = new Label();
            button13 = new Button();
            button14 = new Button();
            button15 = new Button();
            button16 = new Button();
            label9 = new Label();
            tagFilterTextBox = new TextBox();
            label10 = new Label();
            tagFilterClearBtn = new Button();
            fileViewList = new ListView();
            fileIconsImageList = new ImageList(components);
            panel1 = new Panel();
            goToParentButton = new Button();
            previousFileBtn = new Button();
            nextFileBtn = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(647, 14);
            button1.Name = "button1";
            button1.Size = new Size(56, 23);
            button1.TabIndex = 0;
            button1.Text = "Go";
            button1.UseVisualStyleBackColor = true;
            button1.Click += GoToPath;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 1;
            label1.Text = "Current Folder";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 47);
            label2.Name = "label2";
            label2.Size = new Size(108, 15);
            label2.TabIndex = 2;
            label2.Text = "Custom Tag Search";
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox2.Location = new Point(138, 44);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(503, 23);
            textBox2.TabIndex = 4;
            // 
            // currentPathTextBox
            // 
            currentPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            currentPathTextBox.Location = new Point(138, 16);
            currentPathTextBox.Name = "currentPathTextBox";
            currentPathTextBox.Size = new Size(422, 23);
            currentPathTextBox.TabIndex = 5;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Location = new Point(709, 15);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 6;
            button2.Text = "Browse";
            button2.UseVisualStyleBackColor = true;
            button2.Click += SelectFolderToBrowseTo;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button3.Location = new Point(647, 43);
            button3.Name = "button3";
            button3.Size = new Size(56, 23);
            button3.TabIndex = 7;
            button3.Text = "Search";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button4.Location = new Point(709, 43);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 9;
            button4.Text = "Help";
            button4.UseVisualStyleBackColor = true;
            // 
            // showFolderRadioBtn
            // 
            showFolderRadioBtn.AutoSize = true;
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
            tagTree.CheckBoxes = true;
            tagTree.Location = new Point(581, 123);
            tagTree.Name = "tagTree";
            tagTree.Size = new Size(204, 364);
            tagTree.TabIndex = 15;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button5.Location = new Point(581, 496);
            button5.Name = "button5";
            button5.Size = new Size(187, 23);
            button5.TabIndex = 16;
            button5.Text = "Delete Highlighted Tag";
            button5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button6.Location = new Point(580, 570);
            button6.Name = "button6";
            button6.Size = new Size(90, 23);
            button6.TabIndex = 17;
            button6.Text = "Add Tag";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button7.Location = new Point(676, 570);
            button7.Name = "button7";
            button7.Size = new Size(92, 23);
            button7.TabIndex = 18;
            button7.Text = "Remove Tag";
            button7.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            textBox1.Location = new Point(581, 541);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(187, 23);
            textBox1.TabIndex = 19;
            // 
            // radioButton3
            // 
            radioButton3.Anchor = AnchorStyles.Bottom;
            radioButton3.AutoSize = true;
            radioButton3.Location = new Point(12, 569);
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
            radioButton4.Location = new Point(127, 569);
            radioButton4.Name = "radioButton4";
            radioButton4.Size = new Size(162, 19);
            radioButton4.TabIndex = 22;
            radioButton4.TabStop = true;
            radioButton4.Text = "Show tagged files inside it";
            radioButton4.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom;
            label4.AutoSize = true;
            label4.Location = new Point(12, 549);
            label4.Name = "label4";
            label4.Size = new Size(182, 15);
            label4.TabIndex = 23;
            label4.Text = "How to handle browsing folders :";
            // 
            // label5
            // 
            label5.AutoSize = true;
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
            radioButton5.Location = new Point(295, 569);
            radioButton5.Name = "radioButton5";
            radioButton5.Size = new Size(194, 19);
            radioButton5.TabIndex = 25;
            radioButton5.TabStop = true;
            radioButton5.Text = "Show tagged files under its path";
            radioButton5.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            button9.Anchor = AnchorStyles.Bottom;
            button9.Location = new Point(12, 514);
            button9.Name = "button9";
            button9.Size = new Size(83, 23);
            button9.TabIndex = 26;
            button9.Text = "Add file";
            button9.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            button10.Anchor = AnchorStyles.Bottom;
            button10.Location = new Point(101, 514);
            button10.Name = "button10";
            button10.Size = new Size(83, 23);
            button10.TabIndex = 27;
            button10.Text = "Add path";
            button10.UseVisualStyleBackColor = true;
            // 
            // button11
            // 
            button11.Anchor = AnchorStyles.Bottom;
            button11.Location = new Point(292, 514);
            button11.Name = "button11";
            button11.Size = new Size(83, 23);
            button11.TabIndex = 28;
            button11.Text = "Remove file";
            button11.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Bottom;
            label6.Location = new Point(12, 496);
            label6.Name = "label6";
            label6.Size = new Size(563, 15);
            label6.TabIndex = 29;
            label6.Text = "Addition and removal does not affect the actual files (Press help in top right for more info)";
            label6.TextAlign = ContentAlignment.TopCenter;
            // 
            // button12
            // 
            button12.Anchor = AnchorStyles.Bottom;
            button12.Location = new Point(190, 514);
            button12.Name = "button12";
            button12.Size = new Size(95, 23);
            button12.TabIndex = 30;
            button12.Text = "Remove path";
            button12.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Bottom;
            label7.AutoSize = true;
            label7.Location = new Point(200, 549);
            label7.Name = "label7";
            label7.Size = new Size(305, 15);
            label7.TabIndex = 31;
            label7.Text = "Blue for added files, Red for missing, black for not added";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Bottom;
            label8.Location = new Point(221, 101);
            label8.Name = "label8";
            label8.Size = new Size(88, 15);
            label8.TabIndex = 32;
            label8.Text = "5-_-#  0  #-_-S";
            label8.TextAlign = ContentAlignment.TopCenter;
            // 
            // button13
            // 
            button13.Anchor = AnchorStyles.Bottom;
            button13.Location = new Point(315, 97);
            button13.Name = "button13";
            button13.Size = new Size(25, 25);
            button13.TabIndex = 33;
            button13.Text = ">";
            button13.TextAlign = ContentAlignment.TopRight;
            button13.UseVisualStyleBackColor = true;
            // 
            // button14
            // 
            button14.Anchor = AnchorStyles.Bottom;
            button14.Location = new Point(190, 97);
            button14.Name = "button14";
            button14.Size = new Size(25, 25);
            button14.TabIndex = 34;
            button14.Text = "<";
            button14.TextAlign = ContentAlignment.TopRight;
            button14.UseVisualStyleBackColor = true;
            // 
            // button15
            // 
            button15.Anchor = AnchorStyles.Bottom;
            button15.Location = new Point(381, 514);
            button15.Name = "button15";
            button15.Size = new Size(113, 23);
            button15.TabIndex = 35;
            button15.Text = "Fix missing file";
            button15.UseVisualStyleBackColor = true;
            // 
            // button16
            // 
            button16.Anchor = AnchorStyles.Bottom;
            button16.Location = new Point(500, 514);
            button16.Name = "button16";
            button16.Size = new Size(75, 23);
            button16.TabIndex = 36;
            button16.Text = "Rename";
            button16.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label9.Location = new Point(604, 522);
            label9.Name = "label9";
            label9.Size = new Size(140, 15);
            label9.TabIndex = 37;
            label9.Text = "Enter tag to add/remove";
            label9.TextAlign = ContentAlignment.TopCenter;
            // 
            // tagFilterTextBox
            // 
            tagFilterTextBox.Location = new Point(604, 80);
            tagFilterTextBox.Name = "tagFilterTextBox";
            tagFilterTextBox.Size = new Size(124, 23);
            tagFilterTextBox.TabIndex = 38;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(533, 83);
            label10.Name = "label10";
            label10.Size = new Size(65, 15);
            label10.TabIndex = 39;
            label10.Text = "Filter Tags :";
            // 
            // tagFilterClearBtn
            // 
            tagFilterClearBtn.Location = new Point(734, 79);
            tagFilterClearBtn.Name = "tagFilterClearBtn";
            tagFilterClearBtn.Size = new Size(49, 23);
            tagFilterClearBtn.TabIndex = 40;
            tagFilterClearBtn.Text = "Clear";
            tagFilterClearBtn.UseVisualStyleBackColor = true;
            tagFilterClearBtn.Click += ClearTagFiler;
            // 
            // fileViewList
            // 
            fileViewList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            fileViewList.Font = new Font("Segoe UI", 9F);
            fileViewList.LargeImageList = fileIconsImageList;
            fileViewList.Location = new Point(12, 123);
            fileViewList.Name = "fileViewList";
            fileViewList.Size = new Size(562, 363);
            fileViewList.TabIndex = 41;
            fileViewList.UseCompatibleStateImageBehavior = false;
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
            goToParentButton.BackColor = Color.SlateGray;
            goToParentButton.BackgroundImage = SystemFilesTagger.res.ArrowUp;
            goToParentButton.BackgroundImageLayout = ImageLayout.Stretch;
            goToParentButton.Enabled = false;
            goToParentButton.Location = new Point(101, 16);
            goToParentButton.Name = "goToParentButton";
            goToParentButton.Size = new Size(31, 23);
            goToParentButton.TabIndex = 43;
            goToParentButton.UseVisualStyleBackColor = false;
            goToParentButton.MouseClick += GoToParent_Clicked;
            // 
            // previousFileBtn
            // 
            previousFileBtn.BackColor = Color.DarkOliveGreen;
            previousFileBtn.BackgroundImage = SystemFilesTagger.res.ArrowLeft;
            previousFileBtn.BackgroundImageLayout = ImageLayout.Stretch;
            previousFileBtn.Enabled = false;
            previousFileBtn.Location = new Point(566, 16);
            previousFileBtn.Name = "previousFileBtn";
            previousFileBtn.Size = new Size(32, 23);
            previousFileBtn.TabIndex = 44;
            previousFileBtn.UseVisualStyleBackColor = false;
            previousFileBtn.MouseClick += GoToPreviousFile;
            // 
            // nextFileBtn
            // 
            nextFileBtn.BackColor = Color.DarkOliveGreen;
            nextFileBtn.BackgroundImage = SystemFilesTagger.res.ArrowRight;
            nextFileBtn.BackgroundImageLayout = ImageLayout.Stretch;
            nextFileBtn.Enabled = false;
            nextFileBtn.Location = new Point(604, 16);
            nextFileBtn.Name = "nextFileBtn";
            nextFileBtn.Size = new Size(32, 23);
            nextFileBtn.TabIndex = 45;
            nextFileBtn.UseVisualStyleBackColor = false;
            nextFileBtn.MouseClick += GoToNextFile;
            // 
            // FileAndTagsManager
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(797, 607);
            Controls.Add(nextFileBtn);
            Controls.Add(previousFileBtn);
            Controls.Add(goToParentButton);
            Controls.Add(panel1);
            Controls.Add(fileViewList);
            Controls.Add(tagFilterClearBtn);
            Controls.Add(label10);
            Controls.Add(tagFilterTextBox);
            Controls.Add(label9);
            Controls.Add(button16);
            Controls.Add(button15);
            Controls.Add(button14);
            Controls.Add(button13);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(button12);
            Controls.Add(label6);
            Controls.Add(button11);
            Controls.Add(button10);
            Controls.Add(button9);
            Controls.Add(radioButton5);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(radioButton4);
            Controls.Add(radioButton3);
            Controls.Add(textBox1);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(tagTree);
            Controls.Add(label3);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(currentPathTextBox);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button1);
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
        private TextBox textBox2;
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
        private TextBox textBox1;
        private RadioButton radioButton3;
        private RadioButton radioButton4;
        private Label label4;
        private Label label5;
        private RadioButton radioButton5;
        private Button button9;
        private Button button10;
        private Button button11;
        private Label label6;
        private Button button12;
        private Label label7;
        private Label label8;
        private Button button13;
        private Button button14;
        private Button button15;
        private Button button16;
        private Label label9;
        private TextBox tagFilterTextBox;
        private Label label10;
        private Button tagFilterClearBtn;
        private ListView fileViewList;
        private ImageList fileIconsImageList;
        private Panel panel1;
        private Button goToParentButton;
        private Button previousFileBtn;
        private Button nextFileBtn;
    }
}
