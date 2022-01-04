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

		private async Task SetFile(string sourceFilePath)
		{
			string savedFilePath = Path.Combine(
				Path.GetDirectoryName(sourceFilePath) ?? string.Empty,
				$"{Path.GetFileNameWithoutExtension(sourceFilePath)}.jpg");

			var dpi = VisualTreeHelper.GetDpi(TargetImage);

			Debug.WriteLine($"{TargetImage.Width * dpi.DpiScaleX}-{TargetImage.Height * dpi.DpiScaleY}");

			TargetImage.Source = await VideoManager.GetThumbnailAsync(
				TimeSpan.Zero,
				sourceFilePath,
				savedFilePath,
				0,
				TargetImage.Width * dpi.DpiScaleX,
				TargetImage.Height * dpi.DpiScaleY);

			TargetMedia.LoadedBehavior = MediaState.Manual;
			TargetMedia.ScrubbingEnabled = true;
			TargetMedia.Source = new Uri(sourceFilePath, UriKind.RelativeOrAbsolute);
			TargetMedia.Position = TimeSpan.Zero;
			TargetMedia.Pause();
		}
	}
}