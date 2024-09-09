using System;
using System.Collections.Generic;
using System.Text;

namespace Pinyin
{
    public sealed class ManTone : ToneConverter
    {
        public new enum Style
        {
            NORMAL = 0,
            TONE = 1,
            TONE2 = 2,
            TONE3 = 8
        }

        public ManTone()
        {
            m_converts = new Dictionary<int, Func<string, bool, bool, string>>
            {
                { (int)Style.NORMAL, ToneToNormal },
                { (int)Style.TONE, ToneToTone },
                { (int)Style.TONE2, ToneToTone2 },
                { (int)Style.TONE3, ToneToTone3 }
            };
        }

        private static string ToneToNormal(string pinyin, bool vToU = false, bool neutralToneWithFive = false)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < pinyin.Length; i++)
            {
                char ch = pinyin[i];
                if ('a' <= ch && ch <= 'z')
                {
                    result.Append(ch);
                }
                else if (toneToNum.TryGetValue(ch, out var value))
                {
                    result.Append(value.Item1);
                }
                else
                {
                    result.Append(ch);
                }
            }

            if (!vToU)
            {
                result.Replace('ü', 'v');
            }

            return result.ToString();
        }

        private static string ToneToTone(string pinyin, bool vToU = false, bool neutralToneWithFive = false)
        {
            if (vToU)
                return pinyin;

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < pinyin.Length; i++)
            {
                char ch = pinyin[i];
                if ('a' <= ch && ch <= 'z')
                {
                    result.Append(ch);
                }
                else
                {
                    result.Append(ch == 'ü' ? 'v' : ch);
                }
            }

            return result.ToString();
        }

        private static string ToneToTone2(string pinyin, bool vToU = false, bool neutralToneWithFive = false)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < pinyin.Length; i++)
            {
                char ch = pinyin[i];
                if ('a' <= ch && ch <= 'z')
                {
                    result.Append(ch);
                }
                else if (toneToNum.TryGetValue(ch, out var value))
                {
                    result.Append(value.Item1);
                    char toneNumber = value.Item2;
                    if (!(neutralToneWithFive == false && toneNumber == '5'))
                    {
                        result.Append(toneNumber);
                    }
                }
                else
                {
                    if (!vToU && ch == 'ü')
                        ch = 'v';
                    result.Append(ch);
                }
            }

            return result.ToString();
        }

        private static string ToneToTone3(string pinyin, bool vToU = false, bool neutralToneWithFive = false)
        {
            StringBuilder result = new StringBuilder();
            char toneNumber = '5';

            for (int i = 0; i < pinyin.Length; i++)
            {
                char ch = pinyin[i];
                if ('a' <= ch && ch <= 'z')
                {
                    result.Append(ch);
                }
                else if (toneToNum.TryGetValue(ch, out var value))
                {
                    result.Append(value.Item1);
                    toneNumber = value.Item2;
                }
                else
                {
                    if (!vToU && ch == 'ü')
                        ch = 'v';
                    result.Append(ch);
                }
            }

            result.Append(toneNumber);

            if (!neutralToneWithFive && toneNumber == '5')
                result.Length -= 1;

            return result.ToString();
        }

        private static readonly Dictionary<char, (char, char)> toneToNum = new Dictionary<char, (char, char)>
        {
            {'ā', ('a', '1')}, {'á', ('a', '2')}, {'ǎ', ('a', '3')}, {'à', ('a', '4')},
            {'ō', ('o', '1')}, {'ó', ('o', '2')}, {'ǒ', ('o', '3')}, {'ò', ('o', '4')},
            {'ē', ('e', '1')}, {'é', ('e', '2')}, {'ě', ('e', '3')}, {'è', ('e', '4')},
            {'ī', ('i', '1')}, {'í', ('i', '2')}, {'ǐ', ('i', '3')}, {'ì', ('i', '4')},
            {'ū', ('u', '1')}, {'ú', ('u', '2')}, {'ǔ', ('u', '3')}, {'ù', ('u', '4')},
            {'ǖ', ('v', '1')}, {'ǘ', ('v', '2')}, {'ǚ', ('v', '3')}, {'ǜ', ('v', '4')},
            {'ń', ('n', '2')}, {'ň', ('n', '3')}, {'ǹ', ('n', '4')},
            {'ḿ', ('m', '2')}
        };
    }
}
