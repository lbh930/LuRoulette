using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine;



public class RoundManager : MonoBehaviour
{
    public enum RoundState
    {
        Start,
        LoadRevolver,
        Buying,
        Rolling,
        Wait,
        ChangeTurn,
        End
    }

    public delegate void ChangeTurnDelegate();
    public ChangeTurnDelegate changeTurnDelegate;

    public Participant[] participants;
    public Revolver revolver;
    public int participantPointer = 1;

    [HideInInspector]public RoundState roundState = RoundState.Start;

    //shopping cart
    public ShoppingCart shoppingCart;


    int lastBulletCount = 0;

    float timeLockTimer = -1;

    bool shouldChangeTurn = false;
    
    // Start is called before the first frame update
    void Start()
    {
        changeTurnDelegate = ChangeTurn;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        switch (roundState)
        {
            case RoundState.Start:
                roundState = RoundState.LoadRevolver;

                //random dealer at start
                for (int i = 0; i < participants.Length; i++)
                {
                    participants[i].isDealer = false;
                }
                //participantPointer = Random.Range(0, participants.Length);
                //participantPointer = 0;
                participants[participantPointer].isDealer = true;

                lastBulletCount = revolver.getBulletCount();

                break;

            case RoundState.LoadRevolver:
                //dealer loads the revolver
                for (int i = 0; i < participants.Length; i++)
                {
                    if (participants[i].isDealer)
                    {
                        bool revolverActionMade = participants[i].onRevolverLoadDelegate.Invoke(this, revolver);
                        if (revolverActionMade)
                        {
                            Logger.Log("Loading Phase Ended");
                            lastBulletCount = revolver.getBulletCount();
                            participants[participantPointer].onMyRoundRefreshDelegate.Invoke(this, revolver);
                            roundState = RoundState.Buying;
                        }
                    }
                }

                break;

            case RoundState.Buying:
                shoppingCart.ShowCart();

                if (participants[participantPointer].health <= 0)
                {
                    participantPointer = (participantPointer + 1) % (participants.Length);
                }

                bool buyActionMade = participants[participantPointer].onMyPurchaseDelegate.Invoke(shoppingCart.tools);
                if (buyActionMade)
                {
                    shoppingCart.HideCart();
                    roundState = RoundState.Rolling;
                }

                break;


            case RoundState.Rolling:
                //participants rolls to make action
                if (participants[participantPointer].health <= 0)
                {
                    participantPointer = (participantPointer + 1) % (participants.Length);
                }

                int opponentPointer = (participantPointer + 1) % (participants.Length);
                bool roundActionMade = participants[participantPointer].onMyRoundDelegate.Invoke(this, revolver, participants[opponentPointer]);
                if (roundActionMade)
                {
                    int aliveCount = 0;
                    for (int i = 0; i < participants.Length; i++)
                    {
                        if (participants[i].health > 0)
                        {
                            aliveCount += 1;
                        }
                        else
                        {
                            if (participantPointer != i)
                            {
                                participants[i].onDeathDelegate.Invoke(this, revolver);
                            }
                        }
                    }

                    if (aliveCount > 0)
                    {
                        if (aliveCount == 1 && CheckWinCondition())
                        {
                            roundState = RoundState.End;
                        }
                        else if (shouldChangeTurn)
                        {
                            lastBulletCount = revolver.getBulletCount();
                            timeLockTimer = Time.time + 4.0f;
                            roundState = RoundState.ChangeTurn;

                            shouldChangeTurn = false;
                        }
                        else
                        {
                            timeLockTimer = Time.time + 1.5f;
                            roundState = RoundState.Wait;
                        }
                    }else {
                        roundState = RoundState.End;
                    }
                }

                break;

            case RoundState.Wait:
                if (Time.time > timeLockTimer)
                {
                    if (revolver.getBulletCount() > 0)
                    {
                        roundState = RoundState.Rolling;
                    }
                    else
                    {
                        NextDealer();
                        participantPointer = (participantPointer + 1) % (participants.Length);
                        roundState = RoundState.LoadRevolver;
                    }
                    lastBulletCount = revolver.getBulletCount();
                    participants[participantPointer].onMyRoundRefreshDelegate.Invoke(this, revolver);
                }

                break;

            case RoundState.ChangeTurn:
                if (Time.time > timeLockTimer - 2.0f)
                {
                    if (participants[participantPointer].health > 0)
                    {
                        participants[participantPointer].onMyRoundEndDelegate.Invoke(this, revolver);
                    }
                }

                if (Time.time > timeLockTimer)
                {

                    if (revolver.getBulletCount() > 0)
                    {
                        roundState = RoundState.Buying;
                    }
                    else
                    {
                        NextDealer();
                        roundState = RoundState.LoadRevolver;
                    }

                    //next participant's turn
                    participantPointer = (participantPointer + 1) % (participants.Length);
                    participants[participantPointer].onMyRoundRefreshDelegate.Invoke(this, revolver);
                }

                break;

            case RoundState.End:
                int winnerIndex = GetWinnerIndex();
                Logger.Log("The winner is: " + participants[winnerIndex].name);
                break;
        }

    }

    bool CheckWinCondition()
    {
        //check if the only alive participant has more chips than any other dead players
        int survivorChips = 0;
        //int opponentChips = 0;
        bool playerSurvive = false;
        for (int i = 0; i < participants.Length; i++)
        {
            if (participants[i].health > 0)
            {
                if (participants[i].isPlayer) playerSurvive = true;
                survivorChips = participants[i].chips;
            }
        }

        for (int i = 0; i < participants.Length; i++)
        {
            if (participants[i].health <= 0)
            {
                if (participants[i].chips > survivorChips)
                {
                    if (playerSurvive)
                    {
                        Logger.Log("Opponent Died. Gain a total of " + participants[i].chips + " chips to win.");
                    }

                    return false;
                }
            }
        }
        return true;
    }

    int GetWinnerIndex()
    {
        int maxChips = -1;
        int maxChipsIndex = -1;
        for (int i = 0; i < participants.Length; i++)
        {
            if ((participants[i].health > 0 && participants[i].chips >= maxChips) ||
                (participants[i].health <= 0 && participants[i].chips > maxChips))
            {
                maxChips = participants[i].chips;
                maxChipsIndex = i;
            }
        }

        return maxChipsIndex;
    }

    void ChangeTurn()
    {
        shouldChangeTurn = true;
    }

    void NextDealer()
    {
        //switch to next dealer

        int dealerIndex = 0;
        for (int i = 0; i < participants.Length; i++)
        {
            if (participants[i].isDealer)
            {
                dealerIndex = i;
                participants[dealerIndex].isDealer = false;
                Logger.Log(participants[dealerIndex].name + " is no longer Dealer.");
                break;
            }
        }

        participants[(dealerIndex + 1) % participants.Length].isDealer = true;
        Logger.Log(participants[(dealerIndex + 1) % participants.Length].name + " is new Dealer.");

        for (int i = 0; i < participants.Length; i++)
        {
            participants[i].OnDealerSwitch();
        }
    }

    
}
