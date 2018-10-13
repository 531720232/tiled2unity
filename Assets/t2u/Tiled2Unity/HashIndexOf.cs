using System.Collections.Generic;

namespace Tiled2Unity
{
	public class HashIndexOf<T> : IGenericDatabase<T>
	{
		private Dictionary<T, int> dictionary = new Dictionary<T, int>();

		public List<T> List
		{
			get;
			private set;
		}

		public HashIndexOf()
		{
			List = new List<T>();
		}

		public int Add(T value)
		{
			if (dictionary.ContainsKey(value))
			{
				return dictionary[value];
			}
			int count = dictionary.Count;
			List.Add(value);
			dictionary[value] = count;
			return count;
		}

		public int IndexOf(T value)
		{
			return dictionary[value];
		}

		public int AddToDatabase(T value)
		{
			return Add(value);
		}
	}
}
