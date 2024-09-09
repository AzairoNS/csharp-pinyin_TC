using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pinyin
{
    public struct PinyinRes
    {
        public string hanzi;
        public string pinyin;
        public List<string> candidates;
        public bool error;
        public PinyinRes(string lyric)
        {
            hanzi = lyric;
            pinyin = lyric;
            candidates = new List<string>();
            error = true;
        }
    }

    public class PinyinResList : List<PinyinRes>
    {
        public PinyinResList() : base() { }

        public PinyinResList(int capacity) : base(capacity) { }

        //  Convert to utf-16 string list.
        public List<string> ToStrList()
        {
            List<string> res = new List<string>();
            for (int i = 0; i < this.Count; i++)
            {
                res.Add(this[i].error ? this[i].hanzi : this[i].pinyin);
            }
            return res;
        }

        //  Convert to utf-16 string with delimiter(default: " ").
        public string ToStr(string delimiter = " ")
        {
            StringBuilder result = new StringBuilder();
            bool first = true;

            foreach (var res in this)
            {
                if (!first)
                {
                    result.Append(delimiter);
                }
                result.Append(res.error ? res.hanzi : res.pinyin);
                first = false;
            }

            return result.ToString();
        }
    }

    public enum Error
    {
        // Keep original characters
        Default = 0,
        // Ignore this character (do not export)
        Ignore = 1
    }

    public class ChineseG2p
    {
        private readonly Dictionary<string, string> PhrasesMap = new Dictionary<string, string>();
        private readonly Dictionary<string, string> TransDict = new Dictionary<string, string>();
        private readonly Dictionary<string, List<string>> WordDict = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, List<string>> PhrasesDict = new Dictionary<string, List<string>>();

        readonly ToneConverter converter;

        protected ChineseG2p(string language, ToneConverter toneConverter)
        {
            string dictDir = "dict." + language;
            converter = toneConverter;

            DictUtil.LoadDict(dictDir, "phrases_map.txt", PhrasesMap);
            DictUtil.LoadDict(dictDir, "phrases_dict.txt", PhrasesDict);
            DictUtil.LoadAdditionalDict(dictDir, "user_dict.txt", PhrasesDict);
            DictUtil.LoadDict(dictDir, "word.txt", WordDict);
            DictUtil.LoadDict(dictDir, "trans_word.txt", TransDict);
        }

        public static List<string> SplitString(string input)
        {
            string pattern = @"(?![ー゜])([a-zA-Z]+|[+-]|[0-9]|[\u4e00-\u9fa5]|[\u3040-\u309F\u30A0-\u30FF][ャュョゃゅょァィゥェォぁぃぅぇぉ]?)";
            return Regex.Matches(input, pattern).Cast<Match>().Select(m => m.Value).ToList();
        }

        private static PinyinResList ResetZH(List<string> input, PinyinResList res, List<int> positions)
        {
            PinyinResList result = new PinyinResList(input.Count);

            for (int i = 0; i < input.Count; i++)
            {
                int index = positions.IndexOf(i);

                if (index != -1)
                {
                    result.Add(new PinyinRes
                    {
                        hanzi = input[i],
                        pinyin = res[index].pinyin,
                        candidates = res[index].candidates,
                        error = false
                    });
                }
                else
                {
                    result.Add(new PinyinRes
                    {
                        hanzi = input[i],
                        pinyin = input[i],
                        candidates = new List<string> { input[i] },
                        error = true
                    });
                }
            }

            return result;
        }

        private void ZhPosition(List<string> input, List<string> res, List<int> positions)
        {
            foreach (var (item, index) in input.Select((item, index) => (item, index)))
            {
                if (item == null)
                    continue;

                if (WordDict.ContainsKey(item) || TransDict.ContainsKey(item))
                {
                    res.Add(TradToSim(item));
                    positions.Add(index);
                }
            }
        }

        protected PinyinResList HanziToPinyin(string input, int style = 0, Error error = Error.Default, bool candidates = true, bool v_to_u = false, bool neutral_tone_with_five = false)
        {
            return HanziToPinyin(SplitString(input), style, error, candidates, v_to_u, neutral_tone_with_five);
        }

        protected PinyinResList HanziToPinyin(List<string> input, int style = 0, Error error = Error.Default, bool candidates = true, bool v_to_u = false, bool neutral_tone_with_five = false)
        {
            var inputList = new List<string>();
            var inputPos = new List<int>();
            ZhPosition(input, inputList, inputPos);
            var result = new PinyinResList();
            int cursor = 0;

            while (cursor < inputList.Count)
            {
                var currentChar = inputList[cursor];

                if (!IsHanzi(currentChar))
                {
                    result.Add(new PinyinRes(currentChar));
                    cursor++;
                    continue;
                }

                if (!IsPolyphonic(currentChar))
                {
                    var pinyin = GetDefaultPinyin(currentChar, style, v_to_u, neutral_tone_with_five);
                    var g2pRes = new PinyinRes()
                    {
                        hanzi = currentChar,
                        candidates = pinyin,
                        pinyin = pinyin.FirstOrDefault() ?? "",
                        error = false
                    };
                    result.Add(g2pRes);
                    cursor++;
                }
                else
                {
                    var found = false;
                    for (var length = 4; length >= 2 && !found; length--)
                    {
                        if (cursor + length <= inputList.Count)
                        {
                            // cursor: 地, subPhrase: 地久天长
                            var subPhrase = string.Join("", inputList.GetRange(cursor, length));

                            if (PhrasesDict.TryGetValue(subPhrase, out var subRes))
                            {
                                subRes = subRes.Select(pinyin => converter.Convert(pinyin, style, v_to_u, neutral_tone_with_five)).ToList();
                                for (int i = 0; i < subPhrase.Length; i++)
                                {
                                    result.Add(new PinyinRes()
                                    {
                                        hanzi = inputList[cursor + i],
                                        pinyin = subRes[i],
                                        candidates = GetDefaultPinyin(inputList[cursor + i], style, v_to_u, neutral_tone_with_five),
                                        error = false
                                    });
                                }
                                cursor += length;
                                found = true;
                            }
                        }

                        if (cursor >= 1 && cursor + length <= inputList.Count && !found)
                        {
                            // cursor: 重, subPhrase_1: 语重心长
                            var subPhrase = string.Join("", inputList.GetRange(cursor - 1, length));
                            if (PhrasesDict.TryGetValue(subPhrase, out var subRes))
                            {
                                subRes = subRes.Select(pinyin => converter.Convert(pinyin, style, v_to_u, neutral_tone_with_five)).ToList();
                                result.RemoveAt(result.Count - 1);
                                for (int i = 0; i < subPhrase.Length; i++)
                                {
                                    result.Add(new PinyinRes()
                                    {
                                        hanzi = inputList[cursor - 1 + i],
                                        pinyin = subRes[i],
                                        candidates = GetDefaultPinyin(inputList[cursor - 1 + i], style, v_to_u, neutral_tone_with_five),
                                        error = false
                                    });
                                }
                                cursor += length - 1;
                                found = true;
                            }
                        }

                        if (cursor + 1 - length >= 0 && !found && cursor + 1 <= inputList.Count)
                        {
                            // cursor: 好, xSubPhrase: 各有所好
                            var subPhrase = string.Join("", inputList.GetRange(cursor + 1 - length, length));
                            if (PhrasesDict.TryGetValue(subPhrase, out var subRes))
                            {
                                subRes = subRes.Select(pinyin => converter.Convert(pinyin, style, v_to_u, neutral_tone_with_five)).ToList();
                                result.RemoveRange(result.Count - (length - 1), length - 1);
                                for (int i = 0; i < subPhrase.Length; i++)
                                {
                                    result.Add(new PinyinRes()
                                    {
                                        hanzi = inputList[cursor + 1 - length + i],
                                        pinyin = subRes[i],
                                        candidates = GetDefaultPinyin(inputList[cursor + 1 - length + i], style, v_to_u, neutral_tone_with_five),
                                        error = false
                                    });
                                }
                                cursor += 1;
                                found = true;
                            }
                        }

                        if (cursor + 2 - length >= 0 && cursor + 2 <= inputList.Count && !found)
                        {
                            // cursor: 好, xSubPhrase: 叶公好龙
                            var subPhrase = string.Join("", inputList.GetRange(cursor + 2 - length, length));
                            if (PhrasesDict.TryGetValue(subPhrase, out var subRes))
                            {
                                subRes = subRes.Select(pinyin => converter.Convert(pinyin, style, v_to_u, neutral_tone_with_five)).ToList();
                                result.RemoveRange(result.Count - (length - 2), length - 2);
                                for (int i = 0; i < subPhrase.Length; i++)
                                {
                                    result.Add(new PinyinRes()
                                    {
                                        hanzi = inputList[cursor + 2 - length + i],
                                        pinyin = subRes[i],
                                        candidates = GetDefaultPinyin(inputList[cursor + 2 - length + i], style, v_to_u, neutral_tone_with_five),
                                        error = false
                                    });
                                }
                                cursor += 2;
                                found = true;
                            }
                        }
                    }

                    if (!found)
                    {
                        var pinyin = GetDefaultPinyin(currentChar, style, v_to_u, neutral_tone_with_five);
                        var g2pRes = new PinyinRes()
                        {
                            hanzi = currentChar,
                            candidates = pinyin,
                            pinyin = pinyin.FirstOrDefault() ?? "",
                            error = false
                        };
                        result.Add(g2pRes);
                        cursor++;
                    }
                }
            }

            return ResetZH(input, result, inputPos);
        }

        public bool IsHanzi(string text)
        {
            return WordDict.ContainsKey(TradToSim(text));
        }

        public bool IsPolyphonic(string text)
        {
            return PhrasesMap.ContainsKey(text);
        }

        public string TradToSim(string text)
        {
            return TransDict.ContainsKey(text) ? TransDict[text] : text;
        }

        protected List<string> GetDefaultPinyin(string hanzi, int style = 0, bool vToU = false, bool neutralToneWithFive = false)
        {
            if (!WordDict.TryGetValue(hanzi, out var candidates))
            {
                return new List<string> { hanzi };
            }

            List<string> toneCandidates = new List<string>(candidates.Count);
            HashSet<string> seen = new HashSet<string>(candidates.Count);

            foreach (string pinyin in candidates)
            {
                string convertedPinyin = converter.Convert(pinyin, style, vToU, neutralToneWithFive);
                if (seen.Add(convertedPinyin))
                {
                    toneCandidates.Add(convertedPinyin);
                }
            }

            if (toneCandidates.Count == 0)
            {
                return new List<string> { hanzi };
            }

            return toneCandidates;
        }
    }
}