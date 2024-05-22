using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ToolBase : MonoBehaviour
{
    // Start is called before the first frame update
    public int price;
    public bool onCart; //oncart means not purchased yet
    bool pointing = true;
    Participant user;

    int originalLayer = 0;

    void Start()
    {
        originalLayer = gameObject.layer;
        onCart = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (onCart)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(ray, out hitInfo);

            pointing = false;
            if (hit)
            {
                if (hitInfo.transform.gameObject == gameObject)
                {
                    pointing = true;
                }
            }
        }

        if (pointing)
        {
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = 6;
            }
        }
        else
        {
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = originalLayer;
            }
        }
    }

    public void OnPurchasing(Participant participant)
    {
        // called when in participants' turn and the participant is in buying state
        if (onCart && pointing)
        {
            //ready to be bought
            if (Input.GetMouseButtonDown(0))
            {
                print("participant's chips : " + participant.chips.ToString() + " and price is: " + price.ToSafeString());
                if (participant.chips >= price)
                {
                    if (participant.RegisterTool(this))
                    {
                        onCart = false;
                        pointing = false;
                        transform.SetParent(null);
                    }
                    else
                    {
                        Logger.Log("You do not have additional place for tools.");
                    }
                }
                else
                {
                    Logger.Log("Come on. You don't have enough chips to buy this.");
                }
            }
        }
    }

    public void OnPossessed(Participant participant)
    {
        if (onCart)
        {
            Debug.LogWarning("Tool on cart while possessed. Must be logic error.");
            return; //this should not be happending
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(ray, out hitInfo);

        pointing = false;
        if (hit)
        {
            if (hitInfo.transform.gameObject == gameObject)
            {
                pointing = true;
            }
        }
    }

    public virtual void UseTool(Participant participant)
    {

        user = participant;

        //to be implemented in child class
        //...
    }
}
