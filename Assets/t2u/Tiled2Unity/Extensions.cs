using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Tiled2Unity
{
	public static class Extensions
	{
		public static IEnumerable<T> ToEnumerable<T>(this T t)
		{
			yield return t;
		}

		public static string ToBase64(this string text)
		{
			return Convert.ToBase64String(Encoding.ASCII.GetBytes(text));
		}

		public static byte[] Base64ToBytes(this string data)
		{
			return Convert.FromBase64String(data);
		}

		public static byte[] GzipDecompress(this byte[] bytesCompressed)
		{
			MemoryStream stream = new MemoryStream(bytesCompressed);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
				{
					gZipStream.CopyTo(memoryStream);
					return memoryStream.ToArray();
				}
			}
		}

		public static byte[] ZlibDeflate(this byte[] bytesCompressed)
		{
			MemoryStream memoryStream = new MemoryStream(bytesCompressed);
			memoryStream.ReadByte();
			memoryStream.ReadByte();
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
				{
					deflateStream.CopyTo(memoryStream2);
					return memoryStream2.ToArray();
				}
			}
		}

		public static uint[] ToUInts(this byte[] bytes)
		{
			uint[] array = new uint[bytes.Length / 4];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = BitConverter.ToUInt32(bytes, i * 4);
			}
			return array;
		}

		public static bool IsFull<T>(this List<T> list)
		{
			return list.Count == list.Capacity;
		}
	}
}
