using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
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
    public delegate bool OnMyPurchaseDelegate(ToolBase[] tools);
    public OnMyPurchaseDelegate onMyPurchaseDelegate;

    public Transform revolverHandBase;
    public Transform toolHandBase;
    public Transform armorModel;
    public Transform[] toolPos;
    public ToolBase[] myTools;

    public bool isPlayer = false; //is this participant controlled by a human player
    public bool isDealer;
    public int health = 1;
    public int armor = 1;
    public int chips = 0;

    [HideInInspector]public ToolBase toolUsing;

    Animator animator;

    bool revolverPicked = false; //ownership of the revolver
    bool revolverInHand = false; //ownership of the revolver in animation sense

    float shootTimer = -99999999;
    float unloadRevolverTimer = float.PositiveInfinity; //timer to play revolver animation after pick
    float aiReloadTimer = float.PositiveInfinity;
    float reloadDoneTimer = float.PositiveInfinity; //timer to reset camera after loading phase done 
    bool actionDecided = false;
    Participant shootTarget = null;
    bool endRoundSet = false;
    bool deathAnimationSet = false;
    bool reloadDone = false;
    bool unloadAnimationPlayed = false;
    bool openRevolverPlayed = false;

    float timeLockTimer = -1;

    float aiBuyTimer = float.PositiveInfinity;

    float aiDecideTimer = float.PositiveInfinity; //模拟ai思考射击对象的时间

    bool buyDecisionTrigger = false; //购买完成按钮触发

    //Player Control Related


    // Start is called before the first frame update
    void Start()
    {
        onMyRoundDelegate = OnMyRound;
        onRevolverLoadDelegate = OnRevolverLoad;
        onMyRoundEndDelegate = OnMyRoundEnd;
        onMyRoundRefreshDelegate = OnMyRoundRefresh;
        onDeathDelegate = OnDeath;
        onMyPurchaseDelegate = OnMyPurchase;
        animator = GetComponent<Animator>();

        if (isPlayer)
        {
            foreach (ButtonBase button in FindObjectsByType<ButtonBase>(FindObjectsSortMode.None))
            {
                button.RegisterPlayer(this);
            }
        }

        myTools = new ToolBase[toolPos.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if (shootTimer > -1)
        {
            shootTimer -= Time.deltaTime;
        }

        UpdateArmorModel();
    }

    public void BuyDecisionTrigger()
    {
        buyDecisionTrigger = true;
    }

    public bool RegisterTool (ToolBase tool)
    {
        bool foundVacancy = false;
        for (int i = 0; i < myTools.Length; i++)
        {
            if (myTools[i] == null)
            {
                myTools[i] = tool;
                foundVacancy = true;
                chips -= tool.price;
                tool.transform.position = toolPos[i].position;

                break;
            }
        }

        return foundVacancy;
    }

    public void TakeDamage(int damage)
    {
        if (armor > 0)
        {
            armor -= damage;
            armor = Mathf.Max(armor, 0);
        }
        else
        {
            health -= damage;
        }
    }

    void UpdateArmorModel()
    {
        if (armorModel == null)
        {
            UnityEngine.Debug.LogWarning(gameObject.name + " 's armor model not assigned.");
        }
        if (armor > 0)
        {
            armorModel.gameObject.SetActive(true);
        }
        else
        {
            armorModel.gameObject.SetActive(false);
        }
    }

    void ResetStates()
    {
        actionDecided = false;
    }

    public void OnDealerSwitch()
    {
        unloadRevolverTimer = float.PositiveInfinity; //timer to play revolver animation after pick
        aiReloadTimer = float.PositiveInfinity;
        reloadDoneTimer = float.PositiveInfinity;
        openRevolverPlayed = false;
        unloadAnimationPlayed = false;
        revolverPicked = false;
        reloadDone = false;
    }

    void OnMyRoundRefresh(RoundManager manager, Revolver revolver)
    {
        ResetStates();

        if (!isPlayer)
        {
            //ai logic
            timeLockTimer = Time.time + 2.0f;
        }
    }


    void OnMyRoundEnd(RoundManager manager, Revolver revolver)
    {
        ResetStates();
        revolverPicked = false;

        unloadRevolverTimer = float.PositiveInfinity;

        if (!endRoundSet)
        {
            Logger.Log("play end round animation for: " + gameObject.name);
            animator.SetTrigger("End_Round");
            endRoundSet = true;
        }
    }

    void OnDeath(RoundManager manager, Revolver revolver)
    {
        if (!deathAnimationSet)
        {
            Logger.Log(gameObject.name + " 's animator set die trigger");
            animator.SetTrigger("Die");
            deathAnimationSet = true;
        }   
    }

    void LerpCam (Transform target)
    {
        Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position,
            target.position, Time.deltaTime * 2);
        Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation,
            target.rotation, Time.deltaTime * 3.33f);
    }

    public bool OnMyPurchase(ToolBase[] tools)
    {
        if (isPlayer)
        {
            //player control logic
            LerpCam(GameObject.FindGameObjectWithTag("CamPosBuy").transform);

            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i] != null && tools[i].onCart)
                {
                    tools[i].OnPurchasing(this);
                }
            }
            if (Input.GetKeyDown(KeyCode.Return) || buyDecisionTrigger)
            {
                buyDecisionTrigger = false;
                return true;
            }
            return false;
        }
        else
        {
            //ai logic

            aiBuyTimer -= Time.deltaTime;
            if (aiBuyTimer > 2.5f) aiBuyTimer = 2.5f;

            bool decideNotToBuy = true;
            if (aiBuyTimer < 0)
            {
                for (int i = Random.Range(0, tools.Length); i < tools.Length; i++)
                {
                    if (tools[i] != null && tools[i].onCart && tools[i].price <= chips)
                    {
                        if (Random.Range(0.0f, 1.0f) < 0.25f)
                        {
                            if (RegisterTool(tools[i]))
                            {
                                tools[i].Bought();
                                aiBuyTimer = 1f;
                                decideNotToBuy = true;
                                break;
                            }
                        }
                    }
                }

                if (decideNotToBuy)
                {
                    aiBuyTimer = 2.5f;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
    bool OnMyRound(RoundManager manager, Revolver revolver, Participant opponent)
    {
        endRoundSet = false;

        if (!revolverPicked)
        {
            animator.SetTrigger("Pick");
            revolverPicked = true;
        }
        else
        {
            animator.ResetTrigger("Pick");
        }

        CheckRevolverPos(revolver);

        if (!isPlayer)
        {
            //AI logic
            if (aiDecideTimer > 1.5f) aiDecideTimer = 1.5f;
            aiDecideTimer -= Time.deltaTime;

            if (aiDecideTimer <= 0 && Time.time > timeLockTimer && !actionDecided)
            {

                float prob = revolver.getShotProbability();
                Logger.Log("Considering naive Shot probability is: " + prob.ToString());

                if (Random.Range(0.0f, 1.0f) < prob)
                {
                    //choose to shoot opponent
                    Logger.Log("AI decided to shoot opponent");
                    animator.SetTrigger("Aim_Opponent");
                    revolver.ReadyRevolver();
                    shootTarget = opponent;
                }
                else
                {
                    if (Random.Range(0.0f, 1.0f) < 0.3f)
                    {
                        //choose to shoot air
                        Logger.Log("AI decided to shoot air");
                        animator.SetTrigger("Aim_Air");
                        revolver.ReadyRevolver();
                        shootTarget = null;
                    }
                    else
                    {
                        //choose to shoot self
                        Logger.Log("AI decided to shoot self");
                        animator.SetTrigger("Aim_Self");
                        revolver.ReadyRevolver();
                        shootTarget = GetComponent<Participant>();
                    }
                }
                actionDecided = true;
                aiDecideTimer = float.PositiveInfinity;
                timeLockTimer = Time.time + 0.8f;
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
                    Logger.Log(gameObject.name + " now have chips: "+ chips.ToString());
                }
                return true;
            }
        }
        else
        {
            LerpCam(GameObject.FindGameObjectWithTag("CamPosNormal").transform);
            //Player Control Logic
            if (!actionDecided)
            {
                //tool use logic
                for (int i = 0; i < myTools.Length; i++)
                {
                    if (myTools[i] != null)
                    {
                        myTools[i].OnPossessed(this);
                    }
                }

                
                //revolver shoot control logic
                if (Input.GetKeyDown(KeyCode.W))
                {
                    //player choose to shoot opponent
                    animator.SetTrigger("Aim_Opponent");
                    revolver.ReadyRevolver();
                    shootTarget = opponent;
                    actionDecided = true;
                    timeLockTimer = Time.time + 1.5f;
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    //player choose to shoot self
                    animator.SetTrigger("Aim_Self");
                    revolver.ReadyRevolver();
                    shootTarget = this;
                    actionDecided = true;
                    timeLockTimer = Time.time + 1.5f;
                }
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    //player choose to shoot air
                    animator.SetTrigger("Aim_Air");
                    revolver.ReadyRevolver();
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
                    Logger.Log(gameObject.name + " now have chips: " + chips.ToString());
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
        else
        {
            animator.ResetTrigger("Pick");
        }

        CheckRevolverPos(revolver);

        if (!isPlayer)
        {
            if (AnimationLock() || unloadRevolverTimer < 1)
            {
                if (unloadRevolverTimer > 0.4f)
                {
                    revolver.ClearChamber();
                    unloadRevolverTimer = 0.4f;
                }

                unloadRevolverTimer -= Time.deltaTime;

                if (unloadRevolverTimer < 0.8f)
                {
                    if (!reloadDone && !unloadAnimationPlayed)
                    {
                        animator.SetTrigger("Start_Unload");
                        unloadAnimationPlayed = true;
                    }
                }

                if (unloadRevolverTimer < 0)
                {   
                    if (!reloadDone && !openRevolverPlayed)
                    {
                        openRevolverPlayed = true;
                        revolver.OpenRevolver();
                    }

                    if (unloadRevolverTimer < -0.4f)
                    {
                        if (!reloadDone)
                        {
                            //ai load
                            for (int i = 0; i <= 1; i++)
                            {
                                revolver.chamber[Random.Range(0, revolver.chamber.Length)] = true;
                            }
                            revolver.firePointer = Random.Range(0, revolver.chamber.Length);
                            reloadDone = true;
                            animator.ResetTrigger("Start_Unload");

                            revolver.CloseRevolver();
                        }
                    }

                    if (unloadRevolverTimer < -2f)
                    {
                        Logger.Log("Puppet: reload done");
                        return true;
                    }
                }


            }

        }
        else
        {
            //player control
            if (!reloadDone)          
                animator.SetTrigger("Start_Unload");

            if (unloadRevolverTimer > 2f) unloadRevolverTimer = 2f; //set to 1s
            unloadRevolverTimer -= Time.deltaTime;

            if (unloadRevolverTimer < 0)
            {
                if (!reloadDone)
                    revolver.OpenRevolver();

                if (unloadRevolverTimer > -0.33f)
                {
                    //clear before loading start
                    revolver.ClearChamber();
                }
                else
                {

                    if (!reloadDone)
                    {
                        //loading time
                        Camera.main.transform.position = revolver.reloadCamPos.position;
                        Camera.main.transform.rotation = revolver.reloadCamPos.rotation;
                        revolver.OnReloadingControl();
                    }
                    else {
                        if (reloadDoneTimer > 0.5f) reloadDoneTimer = 0.5f;
                        reloadDoneTimer -= Time.deltaTime;
                        if (reloadDoneTimer < 0)
                        {
                            Camera.main.transform.position = Camera.main.transform.parent.position;
                            Camera.main.transform.rotation = Camera.main.transform.parent.rotation;
                            return true; //decision made
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                    {
                        if (revolver.getBulletCount() > 0)
                        {
                            Logger.Log("Player: reload done");
                            animator.SetTrigger("Start_Load");
                            animator.ResetTrigger("Start_Unload");
                            revolver.CloseRevolver();
                            reloadDone = true;
                        }
                        else
                        {
                            Logger.Log("Load at least 1 bullet");
                        }
                    }
                }
            }
        }
        return false;
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
        Logger.Log(gameObject.name + ": revolver in hand!");
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

    public void _anim_OpenBottle()
    {
        if (toolUsing) toolUsing._anim_OpenBottle();
    }

    public void _anim_GrabTool()
    {
        if (toolUsing) toolUsing._anim_GrabTool();
    }

    public void _anim_ThrowTool()
    {
        if (toolUsing) toolUsing._anim_ThrowTool();
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
