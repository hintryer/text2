using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;
using Microsoft.Win32;
using System.Threading;
using System.IO;

namespace text_file
{
    public partial class Form2 : Form
    {
        protected string strFileName;

        protected string strProgName;
        protected ResourceManager resources;
        int filterIndex;
        const string strNewLine = "NewLine";
        const string strFileEncoding = "FileEncoding";        // For registry
        const string strFilterIndex = "FilterIndex";        // For registry
        const string strFilterSave =
            "Text Documents (*.txt)|*.txt|Web Pages (*.htm;*.html)|*.htm;*.html|All Files (*.*)|*.*";
        const string strFilterOpen =
            "Text Documents (*.txt)|*.txt|Web Pages (*.htm;*.html)|*.htm;*.html|Rich Text Format (*.rtf)|*.rtf|All Files (*.*)|*.*";
        const string strMruList = "MruList";
        public  RichTextBox rich2;
        
        List<string> mruList = new List<string>();

        public Form2()
        {
            
            strProgName = "Notepad Clone with File";
            MakeCaption();

        }

        protected string FileTitle()
        {
            return (strFileName != null && strFileName.Length > 1) ?
                Path.GetFileName(strFileName) : "Untitled";
        }
        protected bool OkToTrash()
        {
            if (!rich2.Modified)
            {
                return true;
            }

            DialogResult dr = MessageBox.Show(string.Format("The_text_in_the_{0}_has_changed", FileTitle()) + ".\n\n" +
                    "Do_you_want_to_save_the_changes ?", strProgName,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation);
            switch (dr)
            {
                case DialogResult.Yes:
                    return SaveFileDlg();

                case DialogResult.No:
                    return true;

                case DialogResult.Cancel:
                    return false;
            }
            return false;
        }
        public  void open_click()
        {
            if (!OkToTrash())
                return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = strFilterOpen;
            ofd.FilterIndex = filterIndex;

            if (filterIndex == 1)
                ofd.FileName = "*.txt";
            else if (filterIndex == 2)
                ofd.FileName = "*.htm;*.html";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadFile(ofd.FileName);
                filterIndex = ofd.FilterIndex;
            }
        }
        private void new_click()
        {
            if (!OkToTrash())
                return;

            rich2.Clear();
            rich2.ClearUndo();
            rich2.Modified = false;

            strFileName = null;
            MakeCaption();
        }
        // Utility routines
        protected void LoadFile(string strFileName)
        {
            this.Cursor = Cursors.WaitCursor;
            if (strFileName.EndsWith(".rtf"))
            {
                try
                {
                    rich2.LoadFile(strFileName, RichTextBoxStreamType.RichText);
                }
                catch (Exception exc)
                {
                    //logger.Error(exc);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(exc.Message, strProgName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }
            }
            else
            {
                StreamReader sr;

                try
                {
                    sr = new StreamReader(strFileName, System.Text.Encoding.Default, true);
                }
                catch (Exception exc)
                {
                    //logger.Error(exc);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(exc.Message, strProgName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Asterisk);
                    return;
                }
                rich2.Text = sr.ReadToEnd();
                sr.Close();
            }

            this.strFileName = strFileName;
            updateMRUList(strFileName);

            MakeCaption();

            rich2.SelectionStart = 0;
            rich2.SelectionLength = 0;
            rich2.Modified = false;
            rich2.ClearUndo();
            this.Cursor = Cursors.Default;
        }
        bool SaveFileDlg()
        {
            SaveFileDialog sfd = new SaveFileDialog();

            if (strFileName != null && strFileName.Length > 1)
            {
                sfd.InitialDirectory = Path.GetDirectoryName(strFileName);
                sfd.FileName = Path.GetFileName(strFileName);
            }
            else if (filterIndex == 1)
                sfd.FileName = "*.txt";
            else if (filterIndex == 2)
                sfd.FileName = "*.htm;*.html";

            sfd.Filter = strFilterSave;
            sfd.FilterIndex = filterIndex;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                strFileName = sfd.FileName;
                filterIndex = sfd.FilterIndex;
                SaveFile();
                MakeCaption();
                return true;
            }
            else
            {
                return false;       // Return values are for OkToTrash.
            }
        }
        protected void MakeCaption()
        {
            Text = FileTitle() + " - " + strProgName;
        }

        private void updateMRUList(String fileName)
        {
            if (mruList.Contains(fileName))
            {
                mruList.Remove(fileName);
            }

            mruList.Insert(0, fileName);

            if (mruList.Count > 10)
            {
                mruList.RemoveAt(10);
            }

            updateMRUMenu();
        }
        /// <summary>
        /// Update MRU Submenu.
        /// </summary>
        private void updateMRUMenu()
        {
            //this.recentFilesToolStripMenuItem.DropDownItems.Clear();

            //if (mruList.Count == 0)
            //{
            //    this.recentFilesToolStripMenuItem.DropDownItems.Add(resources.GetString("No_Recent_Files"));
            //}
            //else
            //{
            //    EventHandler eh = new EventHandler(MenuRecentFilesOnClick);

            //    foreach (string fileName in mruList)
            //    {
            //        ToolStripItem item = this.recentFilesToolStripMenuItem.DropDownItems.Add(fileName);
            //        item.Click += eh;
            //    }
            //    this.recentFilesToolStripMenuItem.DropDownItems.Add("-");
            //    strClearRecentFiles = resources.GetString("Clear_Recent_Files");
            //    ToolStripItem clearItem = this.recentFilesToolStripMenuItem.DropDownItems.Add(strClearRecentFiles);
            ////    clearItem.Click += eh;
            //}
        }
        private void save_Click()
        {
            if (strFileName == null || strFileName.Length == 0)
                SaveFileDlg();
            else
                SaveFile();
        }

        private void saveAs_Click()
        {
            SaveFileDlg();
        }
        protected virtual void pageSetup_Click()
        {
            MessageBox.Show("Print not yet implemented!", strProgName);
        }

        protected virtual void preview_Click()
        {
            MessageBox.Show("Print not yet implemented!", strProgName);
        }

        protected virtual void print_Click()
        {
            MessageBox.Show("Print not yet implemented!", strProgName);
        }

        private void exit_Click()
        {
            if (OkToTrash())
            {
                this.Close();
                Application.Exit();
            }
        }

        void SaveFile()
        {
            //if (IsUnicode(richTextBox1.Text) )
            //{
            //    if (DialogResult.No == MessageBox.Show(
            //        ("The_file_appears_to_contain_Unicode_characters") + ".\n" +
            //        ("Saving_as_Windows_ANSI_will_result_in_loss_of_those_characters") + ".\n\n" +
            //        ("Do_you_still_want_to_proceed") + "?\n" +
            //        "\u2022 " + ("To_save_click_Yes") + ".\n" +
            //        "\u2022 " + ("To_preserve_them_click_No_Then_save_file_in_a_Unicode_format") + ".",
            //        ("Confirm_file_save"),
            //        MessageBoxButtons.YesNo,
            //        MessageBoxIcon.Warning))
            //        return;
            //}

            this.Cursor = Cursors.WaitCursor;
            try
            {
                StreamWriter sw = new StreamWriter(strFileName, false, System.Text.Encoding.Default);

                sw.NewLine = strNewLine;

                //if (strNewLine == "\r\n")
                //{
                //    sw.Write(richTextBox1.Text.Replace("\n", "\r\n"));
                //}
                //else
                {
                    sw.Write(rich2.Text);
                }
                sw.Close();
                updateMRUList(strFileName);
            }
            catch (Exception exc)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(exc.Message, strProgName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }
            rich2.Modified = false;
            this.Cursor = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            save_Click();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void InitializeComponent()
        {
            this.rich2 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rich2
            // 
            this.rich2.Location = new System.Drawing.Point(0, 0);
            this.rich2.Name = "rich2";
            this.rich2.Size = new System.Drawing.Size(100, 96);
            this.rich2.TabIndex = 0;
            this.rich2.Text = "";
            // 
            // Form2
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.rich2);
            this.Name = "Form2";
            this.ResumeLayout(false);

        }
    }
    class Class1
    {
        public static void ope(RichTextBox rtb)
        {
        }
    }

}
