using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace WpfVideoTest
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.ThumbnailGrid.Background = StripeBrush.Create(Brushes.Silver, Brushes.Gray);
		}

		private async void Browse_Click(object sender, RoutedEventArgs e)
		{
			var ofd = new OpenFileDialog
			{
				Filter = "*|*.mp4;*.mov;*.wmv;*.avi",
				Multiselect = false
			};
			if (ofd.ShowDialog() is true)
			{
				try
				{
					await SetFile(ofd.FileName);
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
			}
		}

		private async Task SetFile(string filePath)
		{
			var dpi = VisualTreeHelper.GetDpi(TargetThumbnailImage);
			var size = new Size(
				TargetThumbnailImage.Width * dpi.DpiScaleX,
				TargetThumbnailImage.Height * dpi.DpiScaleY);

			TargetThumbnailImage.Source = await VideoManager.GetSnapshotImageAsync(
				filePath,
				TimeSpan.Zero,
				size);

			TargetPreviewImage.Source = await VideoManager.GetSnapshotImageAsync(
				filePath,
				TimeSpan.Zero);

			TargetVideoBox.SourcePath = filePath;
		}
	}
}