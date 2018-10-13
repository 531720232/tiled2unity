using System.Collections.Generic;

namespace Tiled2Unity
{
	internal interface IGenericDatabase<T>
	{
		List<T> List
		{
			get;
		}

		int AddToDatabase(T value);
	}
}
