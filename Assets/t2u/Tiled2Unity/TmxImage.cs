

using System;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public class TmxImage
	{
		public string AbsolutePath
		{
			get;
			private set;
		}

		public Size Size
		{
			get;
			private set;
		}

		public string TransparentColor
		{
			get;
			set;
		}

		public string ImageName
		{
			get;
			private set;
		}

		public UnityEngine.Texture2D ImageBitmap
		{
			get;
			private set;
		}

		public static TmxImage FromXml(XElement elemImage, string prefix, string postfix)
		{
			TmxImage tmxImage = new TmxImage();
			tmxImage.AbsolutePath = TmxHelper.GetAttributeAsFullPath(elemImage, "source");
			tmxImage.ImageName = $"{prefix}{Path.GetFileNameWithoutExtension(tmxImage.AbsolutePath)}{postfix}";
			int attributeAsInt = TmxHelper.GetAttributeAsInt(elemImage, "width", 0);
			int attributeAsInt2 = TmxHelper.GetAttributeAsInt(elemImage, "height", 0);
			tmxImage.Size = new Size(attributeAsInt, attributeAsInt2);
			bool flag = true;
			if (!Settings.IsAutoExporting)
			{
				try
				{
					if (!tmxImage.Size.IsEmpty)
					{

                        UnityEngine.Texture2D texture2 = new UnityEngine.Texture2D(tmxImage.Size.Width, tmxImage.Size.Height);
                   

                    //bitmapInfo.AlphaType = SKAlphaType.Unpremul;
                    //bitmapInfo.Width = tmxImage.Size.Width;
                    //bitmapInfo.Height = tmxImage.Size.Height;
                    var v=  File.ReadAllBytes(tmxImage.AbsolutePath);
                        //   ImageMagick.MagickImage img = new ImageMagick.MagickImage(v);

                        texture2.LoadImage(v);
                        //texture2.LoadRawTextureData(v);
                        tmxImage.ImageBitmap = texture2;
                        //using (FileStream stream = File.Open(tmxImage.AbsolutePath, FileMode.Open))
                        //{


                        //                      tmxImage.ImageBitmap = SKBitmap.Decode(stream, bitmapInfo);
                        //}
                    }
					else
					{
                        UnityEngine.Texture2D texture2 = new UnityEngine.Texture2D(tmxImage.Size.Width, tmxImage.Size.Height);

                        var v = File.ReadAllBytes(tmxImage.AbsolutePath);
                        texture2.LoadImage(v);
                        tmxImage.ImageBitmap = texture2;
                        flag = false;
					}
					TmxImage tmxImage2 = tmxImage;
					tmxImage2.Size = new Size(tmxImage2.ImageBitmap.width, tmxImage.ImageBitmap.height);
				}
				catch (FileNotFoundException inner)
				{
					throw new TmxException($"Image file not found: {tmxImage.AbsolutePath}", inner);
				}
				catch (Exception ex)
				{
					Logger.WriteError("Skia Library exception: {0}\n\tStack:\n{1}", ex.Message, ex.StackTrace);
					Settings.DisablePreviewing();
				}
			}
			tmxImage.TransparentColor = TmxHelper.GetAttributeAsString(elemImage, "trans", "");
			if (!string.IsNullOrEmpty(tmxImage.TransparentColor) && tmxImage.ImageBitmap != null)
			{
				if (flag)
				{
					Logger.WriteInfo("Removing alpha from transparent pixels.");
					UnityEngine.Color color = TmxHelper.ColorFromHtml(tmxImage.TransparentColor);
                    color.a = 0;
					for (int i = 0; i < tmxImage.ImageBitmap.width; i++)
					{
						for (int j = 0; j < tmxImage.ImageBitmap.height; j++)
						{
                            UnityEngine.Color pixel = tmxImage.ImageBitmap.GetPixel(i, j);
							if (pixel.r == color.r && pixel.g == color.g && pixel.b == color.b)
							{
								tmxImage.ImageBitmap.SetPixel(i, j, color);
							}
						}
					}
				}
				else
				{
					Logger.WriteWarning("Cannot make transparent pixels for viewing purposes. Save tileset with newer verion of Tiled.");
				}
			}
			return tmxImage;
		}
	}
}
