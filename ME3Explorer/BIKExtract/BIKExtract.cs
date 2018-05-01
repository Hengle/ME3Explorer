﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ME3Explorer
{
    public partial class BIKExtract : Form
    {
        public struct Entry
        {
            public int off;
            public int size;
        }

        byte[] memory = new byte[0];
        int memsize;
        public List<Entry> entr;

        public BIKExtract()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = string.Empty;
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "Movies.tfc|Movies.tfc"
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    memsize = (int)fileStream.Length;
                    memory = new byte[memsize];
                    entr = new List<Entry>();
                    for (int i = 0; i < memsize; i++)
                    {
                        memory[i] = (byte)fileStream.ReadByte();
                    }
                }
                for (int i = 0; i < memsize -3; i++)
                {
                    if (memory[i] == 0x42)
                        if (memory[i + 1] == 0x49
                        && memory[i + 2] == 0x4B
                        && memory[i + 3] == 0x69)
                        {
                            int n = entr.Count;
                            Entry temp = new Entry();
                            if (n == 0)
                            {
                                temp.off = i;
                                entr.Add(temp);
                            }
                            else
                            {
                                temp.off = i;
                                Entry temp2 = entr[n - 1];
                                temp2.size = temp.off - temp2.off;
                                entr[n - 1] = temp2;
                                entr.Add(temp);
                            }
                        }
                }

                if (entr.Count > 0)
                {
                    int n = entr.Count;
                    Entry temp = entr[n - 1];
                    temp.size = memsize - temp.off;
                    entr[n - 1] = temp;
                }
                listBox1.Items.Clear();
                for (int i = 0; i < entr.Count; i++)
                    listBox1.Items.Add("#" + i + " Offset:" + entr[i].off.ToString("X") + " Size:" + entr[i].size.ToString("X"));
                listBox2.Items.Clear();
                listBox2.Items.Add("Loaded " + Path.GetFileName(path) + " \nCount: " + entr.Count);
            }
        }

        private void selectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1) return;
            string path = string.Empty;
            SaveFileDialog FileDialog1 = new SaveFileDialog
            {
                Filter = "BIK files (*.bik)|*.bik"
            };
            if (FileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = FileDialog1.FileName;
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    Entry t = entr[n];
                    for (int i = 0; i < t.size; i++)
                        fileStream.WriteByte(memory[t.off + i]);
                }
            }
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog m = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = true,
                Title = "Select Folder to Output to"
            };
            if (m.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string dir = m.FileName;
                for (int i = 0; i < entr.Count; i++)
                {
                    Entry t = entr[i];
                    string filePath = Path.Combine(dir, t.off.ToString("X") + ".bik");
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        listBox2.Items.Add("Extracting " + filePath);
                        listBox2.SelectedIndex = listBox2.Items.Count - 1;
                        Application.DoEvents();
                        for (int j = 0; j < t.size; j++)
                            fileStream.WriteByte(memory[t.off + j]);
                    }
                }
            }
            MessageBox.Show("Done.");
        }
    }
}
