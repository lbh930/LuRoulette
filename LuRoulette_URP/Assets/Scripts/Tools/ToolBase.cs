using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ToolBase : MonoBehaviour
{
    // Start is called before the first frame update
    public int price;
    public bool onCart; //oncart means not purchased yet
    public int toolType = 0;
    bool pointing = false;
    bool lastPointing = false;
    Participant user;

    int originalLayer = 0;

    bool interactable = true;


    Vector3 lastRot;
    Vector3 lastPos;
    [HideInInspector] public Vector3 angularVelocity;
    [HideInInspector]public Vector3 velocity;

    void Start()
    {
        originalLayer = gameObject.layer;
        onCart = true;
    }

    // Update is called once per frame
    void Update()
    {
        OnUpdating();
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
        else
        {
            //已被买下，可以使用
            if (pointing && user != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    user.toolUsing = this;
                    UseTool(user);
                    interactable = false;
                }
            }
        }

        if (pointing && interactable)
        {
            if (!lastPointing)
            {
                lastPointing = true;
                OnPointerEnter();
            }
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = 6;
            }

            OnPointing();
        }
        else
        {
            lastPointing = false;
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = originalLayer;
            }
        }
    }

    public void Bought()
    {
        onCart = false;
        pointing = false;
        transform.SetParent(null);
    }

    public void OnPurchasing(Participant participant)
    {
        print(gameObject.name + onCart + pointing);
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
                        user = participant;
                        Bought();
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

    public virtual void OnUpdating()
    {
        velocity = (transform.position - lastPos) / Time.deltaTime;
        angularVelocity = (transform.eulerAngles - lastRot) / Time.deltaTime;
        angularVelocity = Vector3.zero;
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
    }

    public virtual void OnPointing()
    {

    }

    public virtual void OnPointerEnter()
    {
        Logger.Display(TextReader.GetText("default"));
    }

    public virtual void UseTool(Participant participant)
    {
        print("Using tool: " + gameObject.name);

        user = participant;

        //to be implemented in child class
        //...
    }

    public virtual void _anim_OpenBottle()
    {

    }

    public virtual void _anim_GrabTool()
    {
        if (user != null)
        {
            transform.SetParent(user.toolHandBase);
            transform.localPosition = Vector3.zero;
        }
    }

    public virtual void _anim_ThrowTool()
    {
        gameObject.AddComponent<Rigidbody>();
        Rigidbody rigid = GetComponent<Rigidbody>();
        rigid.velocity = velocity;
        rigid.angularVelocity = angularVelocity;
    }
}
