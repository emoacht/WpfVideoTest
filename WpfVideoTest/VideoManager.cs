using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace WpfVideoTest
{
	public static class VideoManager
	{
		public static Task<BitmapImage> GetThumbnailAsync(TimeSpan timeOfFrame, string? sourceFilePath)
		{
			return GetThumbnailAsync(timeOfFrame, sourceFilePath, null, 0, 0, 0);
		}

		public static Task<BitmapImage> GetThumbnailAsync(TimeSpan timeOfFrame, string? sourceFilePath, double width = 0, double height = 0)
		{
			return GetThumbnailAsync(timeOfFrame, sourceFilePath, null, 0, width, height);
		}

		public static async Task<BitmapImage> GetThumbnailAsync(TimeSpan timeOfFrame, string? sourceFilePath, string? savedFilePath, int qualityLevel = 0, double width = 0, double height = 0)
		{
			if (string.IsNullOrEmpty(sourceFilePath))
				throw new ArgumentNullException(nameof(sourceFilePath));
			if (timeOfFrame < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeOfFrame));

			var (stream, actualWidth, actualHeight) = await GetThumbnailStreamAsync(sourceFilePath, timeOfFrame);

			var ratio = Math.Min(width / actualWidth, height / actualHeight);
			if (ratio > 0)
			{
				(actualWidth, actualHeight) = ((int)(actualWidth * ratio), (int)(actualHeight * ratio));
			}

			var bitmapImage = ConvertStreamToBitmapImage(stream, actualWidth, actualHeight);

			await (!string.IsNullOrWhiteSpace(savedFilePath)
				? Task.Run(() => SaveBitmapSourceToFile(bitmapImage, savedFilePath, qualityLevel))
				: Task.CompletedTask);

			return bitmapImage;
		}

		private static async Task<(Stream stream, int width, int height)> GetThumbnailStreamAsync(string filePath)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			const string frameWidthName = "System.Video.FrameWidth";
			const string frameHeightName = "System.Video.FrameHeight";

			// Get video resolution.
			var frameProperties = await videoFile.Properties.RetrievePropertiesAsync(new[] { frameWidthName, frameHeightName });
			uint frameWidth = (uint)frameProperties[frameWidthName];
			uint frameHeight = (uint)frameProperties[frameHeightName];

			var thumbnail = await videoFile.GetThumbnailAsync(ThumbnailMode.VideosView); // This will return the thumbnail around 1 second passed.

			return (thumbnail.AsStream(), (int)frameWidth, (int)frameHeight);
		}

		private static async Task<(Stream stream, int width, int height)> GetThumbnailStreamAsync(string filePath, TimeSpan timeOfFrame)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			const string frameWidthName = "System.Video.FrameWidth";
			const string frameHeightName = "System.Video.FrameHeight";

			// Get video resolution.
			var frameProperties = await videoFile.Properties.RetrievePropertiesAsync(new[] { frameWidthName, frameHeightName });
			uint frameWidth = (uint)frameProperties[frameWidthName];
			uint frameHeight = (uint)frameProperties[frameHeightName];

			// Use Windows.Media.Editing to get ImageStream.
			var clip = await MediaClip.CreateFromFileAsync(videoFile);
			var composition = new MediaComposition();
			composition.Clips.Add(clip);

			// Prevent time from passing the end of timeline.
			var timeOfEnd = composition.Duration - TimeSpan.FromMilliseconds(1);
			if (timeOfFrame > timeOfEnd)
				timeOfFrame = timeOfEnd;

			var imageStream = await composition.GetThumbnailAsync(timeOfFrame, (int)frameWidth, (int)frameHeight, VideoFramePrecision.NearestFrame);

			return (imageStream.AsStream(), (int)frameWidth, (int)frameHeight);
		}

		private static BitmapImage ConvertStreamToBitmapImage(Stream stream, int width, int height)
		{
			Debug.WriteLine($"{width}-{height}");

			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var image = new BitmapImage();
			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.StreamSource = stream;
			image.DecodePixelWidth = width;
			image.DecodePixelHeight = height;
			image.EndInit();
			image.Freeze(); // This is necessary for other thread to use the image.

			return image;
		}

		private static void SaveBitmapSourceToFile(BitmapSource source, string filePath, int qualityLevel = 0)
		{
			var encoder = new JpegBitmapEncoder();

			if (qualityLevel > 0)
				encoder.QualityLevel = qualityLevel;

			encoder.Frames.Add(BitmapFrame.Create(source));

			using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			encoder.Save(fileStream);
		}

		private static void SaveStreamToFile(Stream stream, string filePath, int qualityLevel = 0)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var encoder = new JpegBitmapEncoder();

			if (qualityLevel > 0)
				encoder.QualityLevel = qualityLevel;

			encoder.Frames.Add(BitmapFrame.Create(stream));

			using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			encoder.Save(fileStream);
		}
	}
}