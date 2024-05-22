using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoppingCart : MonoBehaviour
{
    // Start is called before the first frame update
    public ToolBase[] tools;
    public Transform[] toolPos;
    public Vector3 cartStorePos;
    public Vector3 cartIdlePos;
    
    bool showCart = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (showCart)
        {
            transform.position = Vector3.MoveTowards(transform.position, cartStorePos, Time.deltaTime * 2);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, cartIdlePos, Time.deltaTime * 2);
        }
    }

    public void ShowCart()
    {
        showCart = true;
    }

    public void HideCart()
    {
        showCart = false;
    }
}
