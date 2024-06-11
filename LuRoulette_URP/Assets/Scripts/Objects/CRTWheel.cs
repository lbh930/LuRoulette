using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRTWheel : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform wheel;
    public Transform[] bulletModel;
    public Revolver revolver;
    public RoundManager manager;
    bool[] chamber;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (revolver != null)
        {
            if (manager.roundState == RoundManager.RoundState.LoadRevolver)
            {
                chamber = new bool[revolver.chamber.Length];
                for (int i = 0; i < chamber.Length; i++)
                    chamber[i] = revolver.chamber[i];
            }
        }

        if (chamber != null && revolver.getBulletCount() > 0)
        {
            for (int i = 0; i < chamber.Length; i++)
            {
                if (chamber[i])
                {
                    bulletModel[i].gameObject.SetActive(true);
                }
                else
                {
                    bulletModel[i].gameObject.SetActive(false);
                }
            }
            wheel.Rotate(Vector3.right * Time.deltaTime * -30f);

            if (manager.roundState == RoundManager.RoundState.Rolling ||
                manager.roundState == RoundManager.RoundState.LoadRevolver
                || manager.roundState == RoundManager.RoundState.Wait || 
                manager.roundState == RoundManager.RoundState.ChangeTurn ||
                manager.roundState == RoundManager.RoundState.Buying)
            {
                wheel.gameObject.SetActive(true);
            }
            else
            {
                wheel.gameObject.SetActive(false);
            }
        }
        else
        {
            wheel.gameObject.SetActive(false);
        }
    }
}
