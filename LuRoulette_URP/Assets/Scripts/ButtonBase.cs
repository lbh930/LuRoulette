using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBase : MonoBehaviour
{
    bool pointing = false;
    bool lastPointing = false;

    int originalLayer = 0;

    int damn = 0;

    public bool interactable = true;
    public Vector3 deltaOnPointing = Vector3.zero;
    public Vector3 deltaOnClicked = Vector3.zero;

    [HideInInspector]public Participant player;

    Vector3 originPos;

    void Start()
    {
        originPos = transform.localPosition;
        originalLayer = gameObject.layer;
    }

    public void RegisterPlayer(Participant participant)
    {
        player = participant;
    }

    // Update is called once per frame

    void Update()
    {
        if (!interactable)
        {
            foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>())
            {
                trans.gameObject.layer = originalLayer;
            }
            return;
        }

        OnUpdating();

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

        if (pointing)
        {
            transform.localPosition = originPos + deltaOnPointing;
            if (Input.GetMouseButtonDown(0))
            {
                OnClicked();
            }

            if (Input.GetMouseButton(0))
            {
                transform.localPosition = originPos + deltaOnClicked;
            }
        }
        else
        {
            transform.localPosition = originPos;
        }
        
        if (pointing)
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

    public virtual void OnUpdating()
    {

    }

    public virtual void OnPointing()
    {

    }

    public virtual void OnPointerEnter()
    {

    }

    public virtual void OnClicked()
    {
        
    }

    public virtual void OnGrab() { }

    public virtual void OnThrow() { }

    public virtual void OnOpenBottle() { }
}
