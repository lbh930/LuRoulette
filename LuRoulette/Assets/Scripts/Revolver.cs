using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : MonoBehaviour
{
    // Start is called before the first frame update
    public bool[] chamber = { false, false, false, false, false, false };
    public int firePointer = 0;

    public Transform revolverBase;

    public AudioClip blankSound;
    public AudioClip fireSound;

    public Transform fireStart;
    public GameObject fireEffect;

    AudioSource audio;
    void Start()
    {
        audio = GetComponent<AudioSource>();
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
        //Try to fire and see if it is a blank or a fire
        if (chamber[firePointer])
        {
            if (target != null)
            {
                target.health -= 1;
            }
            chamber[firePointer] = false;
            firePointer = (firePointer + 1) % chamber.Length;

            print("bang!");
            audio.clip = fireSound;
            audio.Play();
            GameObject.Instantiate(fireEffect, fireStart.position, fireStart.rotation);
            return true; //true means we have a shot
        }
        else
        {
            firePointer = (firePointer + 1) % chamber.Length;
            print("click.");
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
}
