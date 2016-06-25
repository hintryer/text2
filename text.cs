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

namespace notepad_mod
{
    public partial class Form2 : Form1
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

        public Form2()
        {
            InitializeComponent();
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
            if (!owntext.Modified)
            {
                return true;
            }

            DialogResult dr = MessageBox.Show(string.Format("{0}文件已经更改", FileTitle()) + ".\n\n" +
                    "是否保存?", strProgName,
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
        public void open_click()
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

            owntext.Clear();
            owntext.ClearUndo();
            owntext.Modified = false;

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
                    owntext.LoadFile(strFileName, RichTextBoxStreamType.RichText);
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
                owntext.Text = sr.ReadToEnd();
                sr.Close();
            }

            this.strFileName = strFileName;

            MakeCaption();

            owntext.SelectionStart = 0;
            owntext.SelectionLength = 0;
            owntext.Modified = false;
            owntext.ClearUndo();
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
            this.Cursor = Cursors.WaitCursor;
            try
            {
                StreamWriter sw = new StreamWriter(strFileName, false, System.Text.Encoding.Default);

                sw.NewLine = strNewLine;
                sw.Write(owntext.Text);
                sw.Close();
            }
            catch (Exception exc)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(exc.Message, strProgName,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }
            owntext.Modified = false;
            this.Cursor = Cursors.Default;
        }

        private void 新建NToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new_click();
        }

        private void 打开OToolStripMenuItem_Click(object sender, EventArgs e)
        {
            open_click();
        }

        private void 保存SToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save_Click();
        }

        private void 另存为AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs_Click();
        }

        private void 退出XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exit_Click();
        }

        private void 打印预览VToolStripMenuItem_Click(object sender, EventArgs e)
        {
            print_Click();
        }

    }
}
