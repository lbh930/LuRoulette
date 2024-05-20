using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : MonoBehaviour
{

    // Start is called before the first frame update
    public Transform[] bulletModels;
    public Collider[] reloadDetectors;


    public bool[] chamber = { false, false, false, false, false, false };
    public int firePointer = 0;

    public Transform revolverBase;

    public AudioClip blankSound;
    public AudioClip fireSound;

    public Transform fireStart;
    public GameObject fireEffect;

    public Transform reloadCamPos;

    private int currentDisplayBulletIndex;

    AudioSource audio;
    Animator animator;
    void Start()
    {
        audio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null)
        {
            transform.position = revolverBase.position;
            transform.rotation = revolverBase.rotation;
        }
    }
    public bool TryShoot(Participant target)
    {
        animator.SetTrigger("Fire");
        //Try to fire and see if it is a blank or a fire
        if (chamber[firePointer])
        {
            if (target != null)
            {
                target.health -= 1;
            }
            chamber[firePointer] = false;
            firePointer = (firePointer + 1) % chamber.Length;

            Logger.Log("bang!");
            audio.clip = fireSound;
            audio.Play();
            GameObject.Instantiate(fireEffect, fireStart.position, fireStart.rotation);
            return true; //true means we have a shot
        }
        else
        {
            firePointer = (firePointer + 1) % chamber.Length;
            Logger.Log("click.");
            audio.clip = blankSound;
            audio.Play();
            return false;
        }
    }

    public float getShotProbability()
    {
        //get probability without knowledge to the order of the bullets in chamber
        int bulletCount = 0;
        for (int i = 0; i < chamber.Length; i++)
        {
            if (chamber[i])
            {
                bulletCount += 1;
            }
        }
        return (float)bulletCount / (float)chamber.Length;
    }

    public int getBulletCount()
    {
        int bulletCount = 0;
        for (int i = 0; i < chamber.Length; i++)
        {
            if (chamber[i])
            {
                bulletCount += 1;
            }
        }
        return bulletCount;
    }

    public void OnReloadingControl()
    {
        //get control of the revolver and enable reloading contrl
        if (!AnimationLock())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(ray, out hitInfo);

        // show bullet model
        bool mouseOnBullet = false;

        if (hit)
        {
            for (int i = 0; i < reloadDetectors.Length; i++)
            {
                if (hitInfo.collider == reloadDetectors[i])
                {
                    if (i != currentDisplayBulletIndex)
                    {
                        DisplayBulletModel(i);
                    }
                    mouseOnBullet = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        //if pressed, either load or unload this bullet
                        if (!chamber[currentDisplayBulletIndex] && getBulletCount() <= 2)
                        {
                            chamber[currentDisplayBulletIndex] = true;
                        }
                        else if (chamber[currentDisplayBulletIndex])
                        {
                            chamber[currentDisplayBulletIndex] = false;
                        }
                        
                        if (chamber[currentDisplayBulletIndex])
                        {
                            bulletModels[currentDisplayBulletIndex].gameObject.SetActive(true);
                            bulletModels[currentDisplayBulletIndex].localPosition = Vector3.zero;
                        }
                        else
                        {
                            bulletModels[currentDisplayBulletIndex].gameObject.SetActive(false);
                            bulletModels[currentDisplayBulletIndex].localPosition = Vector3.zero;
                        }
                    }
                }
            }
        }

        if (!mouseOnBullet)
        {
            UnselectDisplayBullet();
        }
    }

    void DisplayBulletModel(int index)
    {
        UnselectDisplayBullet();

        currentDisplayBulletIndex = index;

        if (index >= 0 && index < bulletModels.Length)
        {
            //Logger.Log("showing bullet: "+ currentDisplayBulletIndex.ToString());
            bulletModels[currentDisplayBulletIndex].gameObject.SetActive(true);
            bulletModels[currentDisplayBulletIndex].localPosition = new Vector3(0, 0, 0.015f);
        }
    }
    void UnselectDisplayBullet()
    {
        if (currentDisplayBulletIndex >= 0 && bulletModels[currentDisplayBulletIndex] != null)
        {
            bulletModels[currentDisplayBulletIndex].localPosition = Vector3.zero;

            if (!chamber[currentDisplayBulletIndex])
            {
                //Logger.Log("hiding bullet: "+ currentDisplayBulletIndex.ToString());
                bulletModels[currentDisplayBulletIndex].gameObject.SetActive(false);
            }

            currentDisplayBulletIndex = -1;
        }
    }

    public void ClearChamber()
    {
        for (int i = 0; i < chamber.Length; i++)
        {
            chamber[i] = false;
        }

        for (int i = 0; i < bulletModels.Length; i++)
        {
            bulletModels[i].gameObject.SetActive(false);
        }
    }

    public void OpenRevolver()
    {
        animator.ResetTrigger("Close");
        animator.SetTrigger("Open");
    }

    public void CloseRevolver()
    {
        animator.ResetTrigger("Open");
        animator.SetTrigger("Close");
    }

    public void ReadyRevolver()
    {
        animator.SetTrigger("Ready");
    }

    bool AnimationLock(float threshold = 0.01f)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f - threshold && !animator.IsInTransition(0))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
