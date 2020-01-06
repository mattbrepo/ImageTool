using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ImageTool
{
	/// <summary>
    /// helpful extension for multithreading
	/// </summary>
	public static class ControlExtensions
	{
		public static void Do<TControl>(this TControl control, Action<TControl> action) where TControl : Control
		{
			if (control.InvokeRequired) control.Invoke(action, control);
			else action(control);
		}
	}
	
	class ImgUtil
	{
		enum ImageRelation
		{
			Equal,
			PixelInqeuality,
			PixelFormatDiffer,
			PixelFormatUndefined,
			DimensionsUnequal,
			UnsupportedPixelFormat,
		}

		public static void Combine(string filePathIn1, string filePathIn2, string filePathOut)
		{
			Image first = Image.FromFile(filePathIn1);
			Image second = Image.FromFile(filePathIn2);

			int width = 2 * Math.Max(first.Width, second.Width);
			int height = Math.Max(first.Height, second.Height);

			using (Bitmap bitmap = new Bitmap(width, height))
			{
				using (Graphics flagGraphics = Graphics.FromImage(bitmap))
				{
					flagGraphics.DrawImage(first, 0, 0, first.Width, first.Height);
					flagGraphics.DrawImage(second, first.Width, 0, second.Width, second.Height);
					flagGraphics.Save();
				}

				bitmap.Save(filePathOut, System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		public static double Compare(string filePathIn1, string filePathIn2)
		{
			using (Bitmap first = new Bitmap(filePathIn1))
			using (Bitmap second = new Bitmap(filePathIn2))
			{
				if (first.Size != second.Size) return 100; //per il momento così...

				double total = first.Width * first.Height;
				double diff = 0;
				for (int i = 0; i < first.Width; i++)
					for (int j = 0; j < first.Height; j++)
						if (first.GetPixel(i, j) != second.GetPixel(i, j)) diff++;

				return (100 * diff) / total;
			}
		}

		public static Boolean FastCompare(string filePathIn1, string filePathIn2)
		{
			using (Bitmap first = new Bitmap(filePathIn1))
			using (Bitmap second = new Bitmap(filePathIn2))
			{
				if (first.Size != second.Size) return false;
				
				for (int i = 0; i < first.Width; i++)
					for (int j = 0; j < first.Height; j++)
						if (first.GetPixel(i, j) != second.GetPixel(i, j)) return false;

				return true;
			}
		}

		public static bool FastCompare2(string filePathIn1, string filePathIn2)
		{
			using (Bitmap first = new Bitmap(filePathIn1))
			using (Bitmap second = new Bitmap(filePathIn2))
			{
				return CompareImages(first, second) == ImageRelation.Equal;
			}
		}

		private static ImageRelation CompareImages(Bitmap FirstImage, Bitmap SecondImage)
		{
			BitmapData bmdFirstImage, bmdSecondImage;
			Int32 intPixelSize;

			// Don't compare images with different pixelformats
			if (FirstImage.PixelFormat != SecondImage.PixelFormat)
			{
				return (ImageRelation.PixelFormatDiffer);
			}

			// Don't compare images with undefined pixelformats
			if (FirstImage.PixelFormat == PixelFormat.Undefined)
			{
				return (ImageRelation.PixelFormatUndefined);
			}

			// Images of different dimensions can't be equal
			if (FirstImage.Size != SecondImage.Size)
			{
				return (ImageRelation.DimensionsUnequal);
			}


			// Calculate the pixel size (bytes per pixel)
			switch (FirstImage.PixelFormat)
			{
				// 8 bit - 1 byte
				case (PixelFormat.Format8bppIndexed):
					{
						intPixelSize = 1;
						break;
					}

				// 16 bit - 2 bytes
				case (PixelFormat.Format16bppArgb1555):
					{
						intPixelSize = 2;
						break;
					}
				case (PixelFormat.Format16bppGrayScale):
					{
						intPixelSize = 2;
						break;
					}
				case (PixelFormat.Format16bppRgb555):
					{
						intPixelSize = 2;
						break;
					}
				case (PixelFormat.Format16bppRgb565):
					{
						intPixelSize = 2;
						break;
					}

				// 24 bit - 3 bytes
				case (PixelFormat.Format24bppRgb):
					{
						intPixelSize = 3;
						break;
					}

				// 32 bit - 4 bytes
				case (PixelFormat.Format32bppArgb):
					{
						intPixelSize = 4;
						break;
					}
				case (PixelFormat.Format32bppPArgb):
					{
						intPixelSize = 4;
						break;
					}
				case (PixelFormat.Format32bppRgb):
					{
						intPixelSize = 4;
						break;
					}

				// 48 bit - 5 bytes
				case (PixelFormat.Format4bppIndexed):
					{
						intPixelSize = 5;
						break;
					}

				// 64 bit - 6 bytes
				case (PixelFormat.Format64bppArgb):
					{
						intPixelSize = 6;
						break;
					}
				case (PixelFormat.Format64bppPArgb):
					{
						intPixelSize = 6;
						break;
					}

				// Unsupported size
				default:
					{
						return (ImageRelation.UnsupportedPixelFormat);
					}
			}

			// Lock both bitmap bits to initialize comparison of pixels
			bmdFirstImage = FirstImage.LockBits(new Rectangle(0, 0, FirstImage.Width, FirstImage.Height),
												 ImageLockMode.ReadOnly,
												 FirstImage.PixelFormat);

			bmdSecondImage = SecondImage.LockBits(new Rectangle(0, 0, SecondImage.Width, SecondImage.Height),
												   ImageLockMode.ReadOnly,
												   SecondImage.PixelFormat);

			// Compare each pixel in the images
			unsafe
			{
				for (Int32 y = 0; y < bmdFirstImage.Height; ++y)
				{
					byte* rowFirstImage = (byte*)bmdFirstImage.Scan0 + (y * bmdFirstImage.Stride);
					byte* rowSecondImage = (byte*)bmdSecondImage.Scan0 + (y * bmdSecondImage.Stride);

					for (Int32 x = 0; x < bmdFirstImage.Width; ++x)
					{
						if (rowFirstImage[x * intPixelSize] != rowSecondImage[x * intPixelSize])
						{
							// Unlock bitmap bits
							FirstImage.UnlockBits(bmdFirstImage);
							SecondImage.UnlockBits(bmdSecondImage);

							return (ImageRelation.PixelInqeuality);
						}
					}
				}
			}

			// Unlock bitmap bits
			FirstImage.UnlockBits(bmdFirstImage);
			SecondImage.UnlockBits(bmdSecondImage);

			return ImageRelation.Equal;
		}

	}
}
