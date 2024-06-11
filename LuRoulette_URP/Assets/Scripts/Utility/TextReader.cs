using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TextReader : MonoBehaviour
{
    // ȫ�־�̬�ֵ䣬���ڴ洢��������
    public static Dictionary<string, string> textDic = new Dictionary<string, string>();

    public static int language = 0;
    void Start()
    {
        LoadConfig();
        // �������
        foreach (var kvp in textDic)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }

    void LoadConfig()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "texts.json");

        // ����ļ��Ƿ����
        if (File.Exists(filePath))
        {
            // ��ȡJSON�ļ�
            string jsonContent = File.ReadAllText(filePath);

            // ��JSON���ݷ����л�Ϊ�ֵ�
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
