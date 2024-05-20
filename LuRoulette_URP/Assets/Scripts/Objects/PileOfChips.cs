using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PileOfChips : MonoBehaviour
{
    public Participant trackedParticipant;
    public Transform[] chips;
    Vector3[] chipPos;
    
    // Start is called before the first frame update
    void Start()
    {
        chipPos = new Vector3[chips.Length];
        for (int i = 0; i < chips.Length; i++)
        {
            chipPos[i] = chips[i].position;
            chips[i].position = chipPos[i] + Vector3.right * 10 + Vector3.up * 0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedParticipant != null)
        {
            for (int i = 0; i < chips.Length; i++)
            {
                if (i < trackedParticipant.chips)
                {
                    chips[i].position = Vector3.MoveTowards(chips[i].position,
                        chipPos[i], Time.deltaTime * 16);
                }
                else
                {
                    chips[i].position = Vector3.MoveTowards(chips[i].position,
                        chipPos[i] + Vector3.right*10 + Vector3.up*0.5f, Time.deltaTime * 16);
                }
            }
        }
    }
}
