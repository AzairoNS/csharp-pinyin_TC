using System;
using System.Collections.Generic;

namespace Pinyin
{
    public sealed class CanTone : ToneConverter
    {
        public new enum Style
        {
            // 普通风格，不带声调。如： 中国 -> "zung gwok"
            NORMAL = 0,
            // 声调风格3，即拼音声调在各个拼音之后，用数字 [1-4] 进行表示。如： 中国 -> "zung1 gwok3"
            TONE3 = 8
        }

        public CanTone()
        {
            m_converts = new Dictionary<int, Func<string, bool, bool, string>>
            {
                { (int)Style.NORMAL, Tone3ToNormal }
            };
        }

        private static string Tone3ToNormal(string pinyin, bool vToU = false, bool neutralToneWithFive = false)
        {
            return pinyin[..^1];
        }
    }
}
