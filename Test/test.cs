using Pinyin;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Test
{
    class Program
    {
        static string[] ReadData(string sourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetName().Name + "." + sourceName;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string content = reader.ReadToEnd();
                        return content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    }
                }
                else
                {
                    Console.WriteLine("Resource not found.");
                    return new string[0];
                }
            }
        }
        static void Main(string[] args)
        {
            string[] dataLines;

            // mandarin or cantonese
            bool mandarin = true;
            bool resDisplay = false;

            Pinyin.Pinyin pinyinInstance = Pinyin.Pinyin.Instance;
            Pinyin.Jyutping jyutpingInstance = Pinyin.Jyutping.Instance;

            if (mandarin)
            {
                dataLines = ReadData("op_lab.txt");
            }
            else
            {
                dataLines = ReadData("jyutping_test.txt");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            StreamWriter writer = new StreamWriter("out.txt");
            int count = 0;
            int error = 0;
            if (dataLines.Length > 0)
            {
                foreach (string line in dataLines)
                {
                    string trimmedLine = line.Trim();
                    string[] keyValuePair = trimmedLine.Split('|');


                    if (keyValuePair.Length == 2)
                    {
                        string key = keyValuePair[0];
                        string value = keyValuePair[1];

                        List<string> resWords;
                        if (mandarin)
                        {
                            resWords = pinyinInstance.HanziToPinyin(key, ManTone.Style.NORMAL, Error.Default, false, false, false).ToStrList();
                        }
                        else
                        {
                            resWords = jyutpingInstance.HanziToPinyin(key, CanTone.Style.NORMAL, Error.Default, false).ToStrList();
                        }

                        var words = value.Split(" ");
                        int wordSize = words.Length;
                        count += wordSize;

                        bool diff = false;
                        for (int i = 0; i < wordSize; i++)
                        {
                            if (words[i] != resWords[i] && !words[i].Split("/").Contains(resWords[i]))
                            {
                                diff = true;
                                error++;
                            }
                        }

                        if (resDisplay && diff)
                        {
                            Console.WriteLine("text: " + key);
                            Console.WriteLine("raw: " + value);
                            Console.Write("out:");
                            writer.WriteLine(trimmedLine);

                            for (int i = 0; i < wordSize; i++)
                            {
                                if (words[i] != resWords[i] && !words[i].Split("/").Contains(resWords[i]))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write(" " + resWords[i]);
                                }
                                else
                                {
                                    Console.ResetColor();
                                    Console.Write(" " + resWords[i]);
                                }
                            }
                            Console.ResetColor();
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    }
                }
                writer.Close();
                stopwatch.Stop();

                double percentage = Math.Round(((double)error / (double)count) * 100.0, 2);
                Console.WriteLine("错误率: " + percentage + "%");
                Console.WriteLine("错误数: " + error);
                Console.WriteLine("总字数: " + count);

                TimeSpan elapsedTime = stopwatch.Elapsed;
                Console.WriteLine("函数执行时间: " + elapsedTime.TotalMilliseconds + " 毫秒");
            }
        }
    }
}