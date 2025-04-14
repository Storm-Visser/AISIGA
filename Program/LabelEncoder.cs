using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISIGA.Program
{
    static class LabelEncoder
    {
        private static Dictionary<string, int> _labelToInt = new();
        private static Dictionary<int, string> _intToLabel = new();
        private static int _nextLabel = 0;

        public static int Encode(string label)
        {
            if (!_labelToInt.ContainsKey(label))
            {
                _labelToInt[label] = _nextLabel;
                _intToLabel[_nextLabel] = label;
                _nextLabel++;
            }
            return _labelToInt[label];
        }

        public static int ClassCount => _labelToInt.Count;

        public static string? Decode(int encoded)
        {
            return _intToLabel.TryGetValue(encoded, out string? value) ? value : null;
        }

        public static Dictionary<string, int> GetMapping()
        {
            return new Dictionary<string, int>(_labelToInt);
        }

        public static void Clear()
        {
            _labelToInt.Clear();
            _intToLabel.Clear();
            _nextLabel = 0;
        }
    }
}
