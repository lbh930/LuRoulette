using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Participant : MonoBehaviour
{
    public delegate void OnDeathDelegate(RoundManager manager, Revolver revolver);
    public OnDeathDelegate onDeathDelegate;
    public delegate void OnMyRoundEndDelegate(RoundManager manager, Revolver revolver);
    public OnMyRoundEndDelegate onMyRoundEndDelegate;
    public delegate void OnMyRoundRefreshDelegate(RoundManager manager, Revolver revolver);
    public OnMyRoundRefreshDelegate onMyRoundRefreshDelegate;
    public delegate bool OnMyRoundDelegate(RoundManager manager, Revolver revolver, Participant opponent);
    public OnMyRoundDelegate onMyRoundDelegate;
    public delegate bool OnRevolverLoadDelegate(RoundManager manager, Revolver revolver);
    public OnRevolverLoadDelegate onRevolverLoadDelegate;

    public Transform revolverHandBase;

    public bool isPlayer = false; //is this participant controlled by a human player
    public bool isDealer;
    public int health = 1;
    public int chips = 0;

    Animator animator;

    bool revolverPicked = false; //ownership of the revolver
    bool revolverInHand = false; //ownership of the revolver in animation sense

    float shootTimer = -99999999;
    bool actionDecided = false;
    Participant shootTarget = null;
    bool endRoundSet = false;
    bool deathAnimationSet = false;

    float timeLockTimer = -1;

    //Player Control Related


    // Start is called before the first frame update
    void Start()
    {
        onMyRoundDelegate = OnMyRound;
        onRevolverLoadDelegate = OnRevolverLoad;
        onMyRoundEndDelegate = OnMyRoundEnd;
        onMyRoundRefreshDelegate = OnMyRoundRefresh;
        onDeathDelegate = OnDeath;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shootTimer > -1)
        {
            shootTimer -= Time.deltaTime;
        }
    }

    void OnMyRoundRefresh(RoundManager manager, Revolver revolver)
    {
        actionDecided = false;
        endRoundSet = false;
        if (!isPlayer)
        {
            //ai logic
            timeLockTimer = Time.time + 2.0f;
        }
    }


    void OnMyRoundEnd(RoundManager manager, Revolver revolver)
    {
        actionDecided = false;
        revolverPicked = false;
        if (!endRoundSet)
        {
            print("play end round animation for: " + gameObject.name);
            animator.SetTrigger("End_Round");
            endRoundSet = true;
        }
    }

    void OnDeath(RoundManager manager, Revolver revolver)
    {
        if (!deathAnimationSet)
        {
            print(gameObject.name + " 's animator set die trigger");
            animator.SetTrigger("Die");
            deathAnimationSet = true;
        }   
    }

    bool OnMyRound(RoundManager manager, Revolver revolver, Participant opponent)
    {
        if (!revolverPicked)
        {
            animator.SetTrigger("Pick");
            revolverPicked = true;
        }

        CheckRevolverPos(revolver);

        if (!isPlayer)
        {
            //AI logic
            if (Time.time > timeLockTimer && !actionDecided)
            {

                float prob = revolver.getShotProbability();
                print("Considering naive Shot probability is: " + prob.ToString());

                if (Random.Range(0.0f, 1.0f) < prob)
                {
                    //choose to shoot opponent
                    print("AI decided to shoot opponent");
                    animator.SetTrigger("Aim_Opponent");
                    shootTarget = opponent;
                }
                else
                {
                    if (Random.Range(0.0f, 1.0f) < 0.3f)
                    {
                        //choose to shoot air
                        print("AI decided to shoot air");
                        animator.SetTrigger("Aim_Air");
                        shootTarget = null;
                    }
                    else
                    {
                        //choose to shoot self
                        print("AI decided to shoot self");
                        animator.SetTrigger("Aim_Self");
                        shootTarget = GetComponent<Participant>();
                    }
                }
                actionDecided = true;
                timeLockTimer = Time.time + 1.5f;
            }

            if (Time.time > timeLockTimer && actionDecided) //shoot
            {
                if (revolver.TryShoot(shootTarget))
                {
                    animator.SetTrigger("Shoot");
                    manager.changeTurnDelegate.Invoke();
                    if (shootTarget == this && health <= 0)
                    {
                        deathAnimationSet = true;
                    }
                }
                else if (shootTarget == opponent)
                {
                    manager.changeTurnDelegate.Invoke();
                }else if (shootTarget == this)
                {
                    chips += revolver.getBulletCount();
                    print(gameObject.name + " now have chips: "+ chips.ToString());
                }
                return true;
            }
        }
        else
        {
            //Player Control Logic
            if (!actionDecided)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    //player choose to shoot opponent
                    animator.SetTrigger("Aim_Opponent");
                    shootTarget = opponent;
                    actionDecided = true;
                    timeLockTimer = Time.time + 1.5f;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    //player choose to shoot self
                    animator.SetTrigger("Aim_Self");
                    shootTarget = this;
                    actionDecided = true;
                    timeLockTimer = Time.time + 1.5f;
                }
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    //player choose to shoot air
                    animator.SetTrigger("Aim_Air");
                    shootTarget = null;
                    actionDecided = true;
                    timeLockTimer = Time.time + 1.5f;
                }
            }

            if (actionDecided && Time.time > timeLockTimer)
            {
                if (revolver.TryShoot(shootTarget))
                {
                    animator.SetTrigger("Shoot");
                    manager.changeTurnDelegate.Invoke();
                }
                else if (shootTarget == opponent)
                {
                    manager.changeTurnDelegate.Invoke();
                }else if (shootTarget == this)
                {
                    chips += revolver.getBulletCount();
                    print(gameObject.name + " now have chips: " + chips.ToString());
                }
                return true;
            }

        }

        return false;
    }

    bool OnRevolverLoad(RoundManager manager, Revolver revolver)
    {
        //play pick revolver animation
        if (!revolverPicked)
        {
            animator.SetTrigger("Pick");
            revolverPicked = true;
        }

        CheckRevolverPos(revolver);

        print("To Be Implemented: Revolver Loading");

        if (!isPlayer)
        {

        }

        for (int i = 0; i <= 3; i++)
        {
            revolver.chamber[Random.Range(0, revolver.chamber.Length)] = true;
        }

        for (int i = 0; i < revolver.chamber.Length; i++)
        {
            if (revolver.chamber[i])
            {
                print("loaded on chamger: " + i.ToString());
            }
        }

        revolver.firePointer = Random.Range(0, revolver.chamber.Length);

        print("pin pointing at: " + revolver.firePointer.ToString());

        return true;
    }

    void CheckRevolverPos(Revolver revolver)
    {
        //check if the revolver should be in the hand of this participant
        if (revolverInHand && revolverPicked)
        {
            revolver.transform.SetParent(revolverHandBase);
            revolver.transform.localPosition = Vector3.zero;
            revolver.transform.localEulerAngles = Vector3.zero;
        }
    }

    public void RevolverInHand()
    {
        print(gameObject.name + ": revolver in hand!");
        revolverInHand = true;
    }
    public void RevolverThrow()
    {
        revolverInHand = false;
        Transform[] rightHandChildren = revolverHandBase.GetComponentsInChildren<Transform>();
        foreach (Transform trans in rightHandChildren)
        {
            if (trans != revolverHandBase && trans.parent == revolverHandBase)
            {
                trans.SetParent(null);
            }
        }
    }

}
