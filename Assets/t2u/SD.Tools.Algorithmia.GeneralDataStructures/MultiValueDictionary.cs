using System.Collections.Generic;

namespace SD.Tools.Algorithmia.GeneralDataStructures
{
	public class MultiValueDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
	{
		public void Add(TKey key, TValue value)
		{
			HashSet<TValue> value2 = null;
			if (!TryGetValue(key, out value2))
			{
				value2 = new HashSet<TValue>();
				Add(key, value2);
			}
			value2.Add(value);
		}

		public bool ContainsValue(TKey key, TValue value)
		{
			bool result = false;
			HashSet<TValue> value2 = null;
			if (TryGetValue(key, out value2))
			{
				result = value2.Contains(value);
			}
			return result;
		}

		public void Remove(TKey key, TValue value)
		{
			HashSet<TValue> value2 = null;
			if (TryGetValue(key, out value2))
			{
				value2.Remove(value);
				if (value2.Count <= 0)
				{
					Remove(key);
				}
			}
		}

		public void Merge(MultiValueDictionary<TKey, TValue> toMergeWith)
		{
			if (toMergeWith != null)
			{
				foreach (KeyValuePair<TKey, HashSet<TValue>> item in toMergeWith)
				{
					foreach (TValue item2 in item.Value)
					{
						Add(item.Key, item2);
					}
				}
			}
		}

		public HashSet<TValue> GetValues(TKey key, bool returnEmptySet)
		{
			HashSet<TValue> value = null;
			if (!TryGetValue(key, out value) && returnEmptySet)
			{
				value = new HashSet<TValue>();
			}
			return value;
		}
	}
}
