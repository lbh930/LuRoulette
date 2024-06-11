using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TextReader : MonoBehaviour
{
    // 全局静态字典，用于存储配置数据
    public static Dictionary<string, string> textDic = new Dictionary<string, string>();

    public static int language = 0;
    void Start()
    {
        LoadConfig();
        // 测试输出
        foreach (var kvp in textDic)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }

    void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "texts.json");

        // 检查文件是否存在
        if (File.Exists(filePath))
        {
            // 读取JSON文件
            string jsonContent = File.ReadAllText(filePath);

            // 将JSON内容反序列化为字典
            textDic = JsonUtility.FromJson<DictionaryWrapper>(jsonContent).ToDictionary();
        }
        else
        {
            Debug.LogError("JSON file not found.");
        }
    }

    public static string GetText(string key)
    {
        if (textDic.TryGetValue(key + "_" + language.ToString(), out string value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"Key {key + "_" + language.ToString()} not found in the configuration.");
            return null;
        }
    }

    [System.Serializable]
    private class DictionaryWrapper
    {
        public List<Entry> entries = new List<Entry>();

        [System.Serializable]
        public class Entry
        {
            public string key;
            public string value;
        }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (Entry entry in entries)
            {
                dict[entry.key] = entry.value;
            }
            return dict;
        }
    }
}
