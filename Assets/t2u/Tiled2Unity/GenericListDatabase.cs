using System.Collections.Generic;

namespace Tiled2Unity
{
	internal class GenericListDatabase<T> : IGenericDatabase<T>
	{
		public List<T> List
		{
			get;
			private set;
		}

		public GenericListDatabase()
		{
			List = new List<T>();
		}

		public int AddToDatabase(T value)
		{
			List.Add(value);
			return List.Count - 1;
		}
	}
}
