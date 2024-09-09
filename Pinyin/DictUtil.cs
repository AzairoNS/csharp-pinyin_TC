using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pinyin
{
    public class DictUtil
    {
        public static bool LoadDict(string dictDir, string fileName, Dictionary<string, string> resultMap)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Pinyin.{dictDir}.{fileName}";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"Resource {resourceName} not found.");
                return false;
            }

            var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var keyValuePair = trimmedLine.Split(':');
                if (keyValuePair.Length == 2)
                {
                    resultMap[keyValuePair[0]] = keyValuePair[1];
                }
            }
            return true;
        }

        public static bool LoadDict(string dictDir, string fileName, Dictionary<string, List<string>> resultMap)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Pinyin.{dictDir}.{fileName}";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"Resource {resourceName} not found.");
                return false;
            }

            var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var keyValuePair = trimmedLine.Split(':');
                if (keyValuePair.Length == 2)
                {
                    var values = keyValuePair[1].Split(',').Where(v => !string.IsNullOrEmpty(v)).ToList();
                    resultMap[keyValuePair[0]] = values;
                }
            }
            return true;
        }

        public static bool LoadAdditionalDict(string dictDir, string fileName, Dictionary<string, List<string>> resultMap)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Pinyin.{dictDir}.{fileName}";

            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                Console.WriteLine($"Resource {resourceName} not found.");
                return false;
            }

            var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                var keyValuePair = trimmedLine.Split(':');
                if (keyValuePair.Length == 2)
                {
                    var values = keyValuePair[1].Split(' ').Where(v => !string.IsNullOrEmpty(v)).ToList();
                    resultMap[keyValuePair[0]] = values.Select(x => ManToneUtil.Tone3ToTone(x)).ToList();
                }
            }
            return true;
        }
    }
}
