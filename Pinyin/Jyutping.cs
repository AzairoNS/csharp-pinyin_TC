using System;
using System.Collections.Generic;

namespace Pinyin
{
    public class Jyutping : ChineseG2p
    {
        public Jyutping() : base("cantonese", new CanTone())
        {
        }

        private static readonly Lazy<Jyutping> JyutpingInstance = new Lazy<Jyutping>(() => new Jyutping());
        public static Jyutping Instance => JyutpingInstance.Value;

        public PinyinResList HanziToPinyin(string hans, CanTone.Style style = CanTone.Style.TONE3,
                                                Error error = Error.Default, bool candidates = true)
        {
            /*
                @param hans : raw utf-8 std::string.
                @param ManTone::Style : Preserve the pinyin tone.
                @param errorType : Ignore words that have failed conversion. Default: Keep original.
                @param candidates : Return all possible pinyin candidates. Default: true.
                @return PinyinResVector.
            */
            return base.HanziToPinyin(hans, (int)style, error, candidates, false, false);
        }

        public PinyinResList HanziToPinyin(List<string> hans, CanTone.Style style = CanTone.Style.TONE3,
                                                Error error = Error.Default, bool candidates = true)
        {
            /*
                @param hans : raw utf-8 std::string vector, each element of the vector is a character.
                @param ManTone::Style : Preserve the pinyin tone.
                @param errorType : Ignore words that have failed conversion. Default: Keep original.
                @param candidates : Return all possible pinyin candidates. Default: true.
                @return PinyinResVector.
            */
            return base.HanziToPinyin(hans, (int)style, error, candidates, false, false);
        }

        //  Convert to Simplified Chinese.  utf-8 std::string
        public List<string> GetDefaultPinyin(string hanzi, CanTone.Style style = CanTone.Style.TONE3)
        {
            return base.GetDefaultPinyin(hanzi, (int)style, false, false);
        }

    }

}
