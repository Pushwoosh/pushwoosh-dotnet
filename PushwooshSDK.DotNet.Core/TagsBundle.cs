using System;
using System.Collections;
using System.Collections.Generic;

namespace PushwooshSDK.DotNet.Core
{
    public class TagsBundle
    {
        public class IncrementalInteger {
            public int Value { get; set; }
        }
        Dictionary<string, object> innerData = new Dictionary<string, object>();

        public Dictionary<string, object> ToDictionary { get { return new Dictionary<string, object>(innerData); } }

        public void PutInteger(string key, int value)
        {
            innerData[key] = value;
        }
        public void PutString(string key, string value)
        {
            innerData[key] = value;
        }
        public void PutIncrementInteger(string key, int value)
        {
            innerData[key] = new IncrementalInteger() { Value = value };
        }
        public void PutList(string key, IList<string> value)
        {
            innerData[key] = value;
        }
        public void PutAll(Dictionary<string, object> values)
        {
            foreach (var val in values) 
            {
                if (val.Value is int? ||
                    val.Value is string ||
                    val.Value is IncrementalInteger ||
                    val.Value is IList<string>)
                    innerData[val.Key] = val.Value;
            }
        }
        /// <summary>
        /// Reads an integer tag.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No tag with that key.</exception>
        /// <exception cref="InvalidCastException">The tag holds a different type.</exception>
        public int GetInt(string key)
        {
            return Get<int>(key);
        }

        /// <summary>
        /// Reads a string tag.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No tag with that key.</exception>
        /// <exception cref="InvalidCastException">The tag holds a different type.</exception>
        public string GetString(string key)
        {
            return Get<string>(key);
        }

        /// <summary>
        /// Reads a list tag.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No tag with that key.</exception>
        /// <exception cref="InvalidCastException">The tag holds a different type.</exception>
        public IList<string> GetList(string key)
        {
            return Get<IList<string>>(key);
        }

        // The three getters used to disagree with each other on the same mistake: GetInt did
        // `(value as int?).Value` and threw NullReferenceException, while GetString and GetList
        // returned null and let the problem surface somewhere else entirely. Neither said which
        // key or what it actually held.
        T Get<T>(string key)
        {
            if (!innerData.TryGetValue(key, out var value))
                throw new KeyNotFoundException($"There is no tag '{key}' in this bundle.");

            if (value is T typed)
                return typed;

            throw new InvalidCastException(
                $"Tag '{key}' holds {value?.GetType().Name ?? "null"}, not {typeof(T).Name}.");
        }
    }
}
