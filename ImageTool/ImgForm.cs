using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ImageTool
{
	public partial class ImgForm : Form
	{
		public ImgForm()
		{
			InitializeComponent();
		}

		public void SetFile(string filePath)
		{
			LoadFile(filePath);

			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = Path.GetDirectoryName(filePath);
			watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			watcher.Filter = Path.GetFileName(filePath);

			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnChanged);
			watcher.Deleted += new FileSystemEventHandler(OnChanged);

			watcher.EnableRaisingEvents = true;
		}

		private void LoadFile(string filePath)
		{
			if (!File.Exists(filePath)) return;

            Bitmap internalBmp = null;
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length == 0) return; // it's needed!

                    byte[] imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, imageData.Length);
                    using (var ms = new MemoryStream(imageData))
                    {
                        internalBmp = new Bitmap(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
                return;
            }

			this.Do(item =>
			{
				pictureBox1.Image = internalBmp;
				item.Text = filePath + " - " + DateTime.Now.ToString("hh:mm:ss");
			});
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			LoadFile(e.FullPath);
		}
	}
}
