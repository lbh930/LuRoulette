using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCartConfirm : ButtonBase
{
    float buyDecisionTimer = float.PositiveInfinity;

    public override void OnClicked()
    {
        base.OnClicked();
        if (player != null)
        {
            player.BuyDecisionTrigger();
            buyDecisionTimer = 2;
            interactable = false;
        }
    }

    public override void OnUpdating()
    {
        base.OnUpdating();

        buyDecisionTimer -= Time.deltaTime;

        if (buyDecisionTimer < 0)
        {
            buyDecisionTimer = float.PositiveInfinity;
            interactable = true;
        }
    }
}
