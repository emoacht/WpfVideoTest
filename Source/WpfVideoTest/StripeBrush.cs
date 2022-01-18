using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfVideoTest
{
	internal static class StripeBrush
	{
		/// <summary>
		/// Creates stripe Brush for rectangle { Width: 160, Height: 120 }.
		/// </summary>
		/// <param name="backgroundStripe">Brush for background stripe</param>
		/// <param name="foregroundStripe">Brush for foreground stripe</param>
		/// <returns>DrawingBrush</returns>
		/// <remarks>
		/// This stripe fits only to rectangle whose width is 160 and height is 120 in DPI.
		/// </remarks>
		public static Brush Create(Brush backgroundStripe, Brush foregroundStripe)
		{
			var backgroundDrawing = new GeometryDrawing(backgroundStripe, default,
				new RectangleGeometry(new Rect(0, 0, 10, 12)));

			var foregroundDrawing = new GeometryDrawing(foregroundStripe, default,
				new PathGeometry(new[]
				{
					new PathFigure(
						new Point(0, 0),
						new[]
						{
							new LineSegment(new Point(0, 6), true),
							new LineSegment(new Point(5, 12), true),
							new LineSegment(new Point(10, 12), true),
						},
						true),
					new PathFigure(
						new Point(5, 0),
						new[]
						{
							new LineSegment(new Point(10, 6), true),
							new LineSegment(new Point(10, 0), true),
						},
						true),
				}));

			var drawingGroup = new DrawingGroup();
			drawingGroup.Children.Add(backgroundDrawing);
			drawingGroup.Children.Add(foregroundDrawing);

			var drawingBrush = new DrawingBrush(drawingGroup)
			{
				Stretch = Stretch.UniformToFill,
				TileMode = TileMode.Tile,
				Viewport = new Rect(0, 0, 40, 48),
				ViewportUnits = BrushMappingMode.Absolute,
			};
			drawingBrush.Freeze(); // To be considered.
			return drawingBrush;
		}
	}
}