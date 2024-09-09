using System;
using System.Collections.Generic;

namespace Pinyin
{
    public static class ManToneUtil
    {
        // 定义 phoneticSymbolReverse 映射表
        private static readonly Dictionary<string, char> phoneticSymbolReverse = new Dictionary<string, char>
        {
            {"a1", 'ā'}, {"a2", 'á'}, {"a3", 'ǎ'}, {"a4", 'à'},
            {"e1", 'ē'}, {"e2", 'é'}, {"e3", 'ě'}, {"e4", 'è'},
            {"i1", 'ī'}, {"i2", 'í'}, {"i3", 'ǐ'}, {"i4", 'ì'},
            {"o1", 'ō'}, {"o2", 'ó'}, {"o3", 'ǒ'}, {"o4", 'ò'},
            {"u1", 'ū'}, {"u2", 'ú'}, {"u3", 'ǔ'}, {"u4", 'ù'},
            {"v1", 'ǖ'}, {"v2", 'ǘ'}, {"v3", 'ǚ'}, {"v4", 'ǜ'},
        };

        // https://github.com/mozillazg/python-pinyin/blob/master/pypinyin/style/_tone_rule.py
        private static int RightMarkIndex(string pinyinNoTone)
        {
            // 'iou', 'uei', 'uen': 根据还原前的拼音进行标记
            if (pinyinNoTone.Contains("iou"))
            {
                return pinyinNoTone.IndexOf('u');
            }
            if (pinyinNoTone.Contains("uei"))
            {
                return pinyinNoTone.IndexOf('i');
            }
            if (pinyinNoTone.Contains("uen"))
            {
                return pinyinNoTone.IndexOf('u');
            }

            // 有 'a' 不放过, 没 'a' 找 'o'、'e'
            var vowels = new List<char> { 'a', 'o', 'e' };
            foreach (char c in vowels)
            {
                int pos = pinyinNoTone.IndexOf(c);
                if (pos != -1)
                {
                    return pos;
                }
            }

            // 'i'、'u' 若是连在一起，谁在后面就标谁
            var combos = new List<string> { "iu", "ui" };
            foreach (string combo in combos)
            {
                int pos = pinyinNoTone.IndexOf(combo, StringComparison.Ordinal);
                if (pos != -1)
                {
                    return pos + 1;
                }
            }

            // 'i'、'u'、'v'、'ü'
            var otherVowels = new List<char> { 'i', 'u', 'v', 'ü' };
            foreach (char c in otherVowels)
            {
                int pos = pinyinNoTone.IndexOf(c);
                if (pos != -1)
                {
                    return pos;
                }
            }

            // 'n', 'm', 'ê'
            var finalChars = new List<char> { 'n', 'm', 'ê' };
            foreach (char c in finalChars)
            {
                int pos = pinyinNoTone.IndexOf(c);
                if (pos != -1)
                {
                    return pos;
                }
            }

            // 如果没有找到合适的位置，则返回-1表示没有可以标记的位置
            return -1;
        }

        private static bool IsToneNumber(char c)
        {
            return c >= '0' && c <= '4';
        }

        private static bool IsPhoneticSymbol(char c)
        {
            return "aeiouüv".Contains(c);
        }

        private static string ToneToTone(string tone2)
        {
            // 替换 "ü" 为 "v" 并去掉 5 和 0
            string modifiedTone = tone2.Replace('ü', 'v').Replace("5", "").Replace("0", "");

            List<char> result = new List<char>();

            int pos = 0;
            while (pos < modifiedTone.Length)
            {
                char currentChar = modifiedTone[pos];
                if (IsPhoneticSymbol(currentChar))
                {
                    if (pos + 1 < modifiedTone.Length && IsToneNumber(modifiedTone[pos + 1]))
                    {
                        string str = modifiedTone.Substring(pos, 2);
                        if (phoneticSymbolReverse.TryGetValue(str, out char mappedChar))
                        {
                            result.Add(mappedChar);
                            pos += 2;
                        }
                        else
                        {
                            result.Add(currentChar);
                            pos++;
                        }
                    }
                    else
                    {
                        result.Add(currentChar);
                        pos++;
                    }
                }
                else
                {
                    result.Add(currentChar);
                    pos++;
                }
            }

            return new string(result.ToArray()).Replace('ü', 'v');
        }

        private static string Tone3ToTone2(string pinyin, bool vToU = false)
        {
            string noNumberTone3 = pinyin.Substring(0, pinyin.Length - 1);
            int markIndex = RightMarkIndex(noNumberTone3);
            if (markIndex == -1)
                markIndex = noNumberTone3.Length - 1;

            string before = noNumberTone3.Substring(0, markIndex + 1);
            string after = noNumberTone3.Substring(markIndex + 1);
            string number = pinyin.Substring(pinyin.Length - 1);

            return before + number + after;
        }

        public static string Tone3ToTone(string pinyin)
        {
            string tone2 = Tone3ToTone2(pinyin, true);
            return ToneToTone(tone2);
        }
    }
}
