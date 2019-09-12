using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A dual dictionary to access both value and key in a constant time
/// Note: "key to value" and "value to key" must be in a one-to-one relationship
/// </summary>
namespace WSCAD_Demo.Utility
{
    class DualDictionary<T1, T2>
    {
        private Dictionary<T1, T2> dicKeyValue = new Dictionary<T1, T2>();
        private Dictionary<T2, T1> dicValueKey = new Dictionary<T2, T1>();

        public T2 Value(T1 key)
        {
            return dicKeyValue[key];
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            return dicKeyValue.TryGetValue(key, out value);
        }

        public T1 Key(T2 value)
        {
            return dicValueKey[value];
        }

        public bool TryGetKey(T2 value, out T1 key)
        {
            return dicValueKey.TryGetValue(value, out key);
        }

        public void Add(T1 key, T2 value)
        {
            dicKeyValue.Add(key, value);
            dicValueKey.Add(value, key);
        }

        public bool Remove(T1 key)
        {
            T2 value = dicKeyValue[key];
            return dicKeyValue.Remove(key) && dicValueKey.Remove(value);
        }

        public bool Remove(T2 value)
        {
            T1 key = dicValueKey[value];
            return dicKeyValue.Remove(key) && dicValueKey.Remove(value);
        }

        public void Clear()
        {
            dicKeyValue.Clear();
            dicValueKey.Clear();
        }
    }
}
