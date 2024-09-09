using System;
using System.Collections.Generic;

namespace Pinyin
{
    // https://github.com/mozillazg/python-pinyin/blob/master/pypinyin/constants.py
    public enum Style
    {
        // 普通风格，不带声调。如： 中国 -> ``zhong guo``
        NORMAL = 0,
        // 标准声调风格，拼音声调在韵母第一个字母上（默认风格）。如： 中国 -> ``zhōng guó``
        TONE = 1,
        // 声调风格2，即拼音声调在各个韵母之后，用数字 [1-4] 进行表示。如： 中国 -> ``zho1ng guo2``
        TONE2 = 2,
        // 声调风格3，即拼音声调在各个拼音之后，用数字 [1-4] 进行表示。如： 中国 -> ``zhong1 guo2``
        TONE3 = 8
    }

    public sealed class Pinyin : ChineseG2p
    {
        private static readonly Lazy<Pinyin> PinyinInstance = new Lazy<Pinyin>(() => new Pinyin());
        public static Pinyin Instance => PinyinInstance.Value;
        public Pinyin() : base("mandarin", new ManTone()) { }

        public PinyinResList HanziToPinyin(string hans,
                                                ManTone.Style style = ManTone.Style.TONE,
                                                Error error = Error.Default, bool candidates = true,
                                                bool vToU = false, bool neutralToneWithFive = false)
        {
            /*
                @param hans : raw utf-16 string.
                @param ManTone.Style : Preserve the pinyin tone.
                @param errorType : Ignore words that have failed conversion. Default: Keep original.
                @param candidates : Return all possible pinyin candidates. Default: true.
                @param v_to_u : Convert v to ü. Default: false.
                @param neutral_tone_with_five : Use 5 as neutral tone. Default: false.
                @return PinyinResList.
            */
            return base.HanziToPinyin(hans, (int)style, error, candidates, vToU, neutralToneWithFive);
        }

        public PinyinResList HanziToPinyin(List<string> hans,
                                                ManTone.Style style = ManTone.Style.TONE,
                                                Error error = Error.Default, bool candidates = true,
                                                bool vToU = false, bool neutralToneWithFive = false)
        {
            /*
                @param hans : raw utf-16 List<string>, each element of the vector is a character.
                @param ManTone.Style : Preserve the pinyin tone.
                @param errorType : Ignore words that have failed conversion. Default: Keep original.
                @param candidates : Return all possible pinyin candidates. Default: true.
                @param v_to_u : Convert v to ü. Default: false.
                @param neutral_tone_with_five : Use 5 as neutral tone. Default: false.
                @return PinyinResList.
            */
            return base.HanziToPinyin(hans, (int)style, error, candidates, vToU, neutralToneWithFive);
        }

        //  Get a pronunciation list.
        public List<string> GetDefaultPinyin(string hanzi,
                                             ManTone.Style style = ManTone.Style.TONE,
                                             bool vToU = false, bool neutralToneWithFive = false)
        {
            return base.GetDefaultPinyin(hanzi, (int)style, vToU, neutralToneWithFive);
        }
    }

}
