using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace InternetStandards.oEmbed.Network
{
    internal class HttpHeadersDictionary : IDictionary<string, string>
    {
        private readonly HttpHeaders _httpHeaders;

        public HttpHeadersDictionary(HttpHeaders responseHeaders)
        {
            _httpHeaders = responseHeaders;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _httpHeaders.Select(FlattenHeaderValues).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _httpHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _httpHeaders.Contains(
                new KeyValuePair<string, IEnumerable<string>>(item.Key, new[] {item.Value}));
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            foreach (var header in this)
                array[arrayIndex++] = header;
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _httpHeaders.Contains(item.Key) &&
                   _httpHeaders.GetValues(item.Key)
                       .SequenceEqual(item.Value.Split(',').Select(value => value.Trim())) && Remove(item.Key);
        }

        public int Count => _httpHeaders.Count();
        public bool IsReadOnly => false;
        public void Add(string key, string value)
        {
            Remove(key);
            _httpHeaders.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _httpHeaders.Contains(key);
        }

        public bool Remove(string key)
        {
            return _httpHeaders.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_httpHeaders.TryGetValues(key, out var values))
            {
                value = FlattenHeaderValues(values);
                return true;
            }

            value = null;
            return false;
        }

        public string this[string key]
        {
            get => FlattenHeaderValues(_httpHeaders.GetValues(key));
            set => Add(key, value);
        }

        public ICollection<string> Keys => _httpHeaders.Select(kv => kv.Key).ToList();

        public ICollection<string> Values =>
            _httpHeaders.Select(FlattenHeaderValues).Select(kv => kv.Value).ToList();

        private static KeyValuePair<string, string> FlattenHeaderValues(KeyValuePair<string, IEnumerable<string>> header)
        {
            return new KeyValuePair<string, string>(header.Key, FlattenHeaderValues(header.Value));
        }

        private static string FlattenHeaderValues( IEnumerable<string> values)
        {
            return string.Join(", ", values);
        }
    }
}
