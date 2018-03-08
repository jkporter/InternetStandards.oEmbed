using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace InternetStandards.oEmbed.Network
{
    internal class MultiHttpHeadersDictionary : IDictionary<string, string>
    {
        private readonly IEnumerable<IDictionary<string, string>> _httpHeadersDictionaries;

        public MultiHttpHeadersDictionary(IEnumerable<IDictionary<string, string>> httpHeaders)
        {
            _httpHeadersDictionaries = httpHeaders;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _httpHeadersDictionaries.SelectMany(headers => headers)
                .GroupBy(header => header.Key, header => header.Value).Select(header =>
                    new KeyValuePair<string, string>(header.Key, string.Join(", ", header))).GetEnumerator();
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
            foreach(var httpHeaders in _httpHeadersDictionaries)
                httpHeaders.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _httpHeadersDictionaries.Any(headers => headers.Contains(item));
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            foreach (var header in this)
                array[arrayIndex++] = header;
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return _httpHeadersDictionaries.Aggregate(false,
                (current, httpHeaders) => current || httpHeaders.Remove(item));
        }

        public int Count => _httpHeadersDictionaries.Select(httpHeaders => httpHeaders.Keys).Distinct().Count();

        public bool IsReadOnly => false;

        public void Add(string key, string value)
        {
            Remove(key);
            // _httpHeadersDictionaries.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _httpHeadersDictionaries.Any(headers => headers.ContainsKey(key));
        }

        public bool Remove(string key)
        {
            return _httpHeadersDictionaries.Aggregate(false,
                (current, httpHeaders) => current || httpHeaders.Remove(key));
        }

        public bool TryGetValue(string key, out string value)
        {
            var success = false;
            var values = new List<string>();
            foreach (var httpHeaders in _httpHeadersDictionaries)
                if (httpHeaders.TryGetValue(key, out var aValue))
                {
                    success = true;
                    values.Add(aValue);
                }

            value = success ? string.Join(", ", values) : null;
            return success;
        }

        public string this[string key]
        {
            get => string.Join(", ",
                _httpHeadersDictionaries.Select(headers => headers.TryGetValue(key, out var value) ? value : null));
            set => Add(key, value);
        }

        public ICollection<string> Keys => this.Select(header => header.Key).ToList();

        public ICollection<string> Values => this.Select(header => header.Value).ToList();
    }
}
