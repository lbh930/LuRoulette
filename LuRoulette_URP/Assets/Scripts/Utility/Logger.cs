using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static string log;
    TextMeshPro tmp;
    public static string last = "";
    // Start is called before the first frame update
    void Start()
    {
        log = "";
        tmp = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        tmp.text = log;
    }

    public static void Log(string s)
    {
        if (last != s)
        {
            log += "\n" + s;
            last = s;
        }
        print(s);
    }

    public static void Display(string s)
    {
        log = s;
    }
}
