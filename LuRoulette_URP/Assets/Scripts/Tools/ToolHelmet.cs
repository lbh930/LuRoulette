using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolHelmet : ToolBase
{

    public override void UseTool(Participant participant)
    {
        base.UseTool(participant);
           
        //Try to add helmet to participant
        if (participant.armor < 1)
        {
            participant.armor = 1;
            Destroy(gameObject);
        }
        else
        {
            Logger.Display(TextReader.GetText("helmetUnusable"));
        }
    }

    public override void OnPointerEnter()
    {
        base.OnPointerEnter();
        Logger.Display(TextReader.GetText("helmetDesc"));
    }

}
