using System;
using System.Collections.Generic;

namespace Pinyin
{
    public class ToneConverter
    {
        public enum Style
        {
            // 普通风格，不带声调。如：中国 -> "zhong guo"
            NORMAL = 0,
            // 标准声调风格，拼音声调在韵母第一个字母上（默认风格）。如：中国 -> "zhōng guó"
            TONE = 1
        }

        public ToneConverter() { }

        public string Convert(string str, int style, bool vToU = false, bool neutralToneWithFive = false)
        {
            if (!m_converts.TryGetValue(style, out var converter))
            {
                return str;
            }

            return converter(str, vToU, neutralToneWithFive);
        }

        protected Dictionary<int, Func<string, bool, bool, string>> m_converts =
            new Dictionary<int, Func<string, bool, bool, string>>();
    }
}
