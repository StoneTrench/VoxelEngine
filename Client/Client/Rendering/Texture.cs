using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using static OpenGL.GL;

namespace VoxelEngine.Client.Rendering {
	class Texture {
		private readonly uint TEXTURE_ID;
		private readonly int TEXTURE_UNIT;

		public unsafe Texture(string textureSourceFilePath, int unit = 0) {
			TEXTURE_UNIT = unit;

			Image image = Image.FromFile(textureSourceFilePath);
			image.RotateFlip(RotateFlipType.RotateNoneFlipX);
			byte[] fileData = ImageToBitmapByteArray(image);

			TEXTURE_ID = glGenTexture();
			BindTexture();

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

			string PixelFormatName = image.PixelFormat.ToString();
			fixed (byte* ptr = &fileData[0]) {
				if (PixelFormatName.EndsWith("Argb"))
					glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB_ALPHA, image.Width, image.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, ptr);
				else if (PixelFormatName.EndsWith("Rgb"))
					glTexImage2D(GL_TEXTURE_2D, 0, GL_SRGB, image.Width, image.Height, 0, GL_BGR, GL_UNSIGNED_BYTE, ptr);
				else if (PixelFormatName.EndsWith("GrayScale"))
					glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, image.Width, image.Height, 0, GL_RED, GL_UNSIGNED_BYTE, ptr);
			}

			glGenerateMipmap(GL_TEXTURE_2D);

			UnbindTexture();
		}

		public void BindTexture(int unit) {
			glActiveTexture(GL_TEXTURE0 + unit);
			glBindTexture(GL_TEXTURE_2D, TEXTURE_ID);
		}
		public void BindTexture() {
			BindTexture(TEXTURE_UNIT);
		}
		public void UnbindTexture() {
			glBindTexture(GL_TEXTURE_2D, 0);
		}

		public void Delete() {
			glDeleteTexture(TEXTURE_ID);
		}

		public static byte[] FileToBitmapByteArray(string filepath) {
			Image image = Image.FromFile(filepath);
			if (image == null) return null;
			image.RotateFlip(RotateFlipType.RotateNoneFlipX);

			return ImageToBitmapByteArray(image);
		}
		public static byte[] ImageToBitmapByteArray(Image imageIn) {
			using (var ms = new MemoryStream()) {
				imageIn.Save(ms, ImageFormat.Bmp);
				return ms.ToArray().Skip(54).ToArray();
			}
		}
	}
}
