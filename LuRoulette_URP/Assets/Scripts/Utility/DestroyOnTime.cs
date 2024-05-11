using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    // Start is called before the first frame update
    public float destroyTimer = 2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        destroyTimer -= Time.deltaTime;

        if (destroyTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}
