using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTequila : ToolBase
{
    public Transform cork;


    public override void UseTool(Participant participant)
    {
        base.UseTool(participant);
        participant.GetComponent<Animator>().SetFloat("ToolType", toolType);
        participant.GetComponent<Animator>().SetTrigger("UseTool");
    }

    public override void OnPointerEnter()
    {
        base.OnPointerEnter();
        Logger.Display(TextReader.GetText("tequilaDesc"));
    }

    public override void _anim_OpenBottle()
    {
        base._anim_OpenBottle();
        cork.gameObject.AddComponent<Rigidbody>();
        Rigidbody rigid = cork.gameObject.GetComponent<Rigidbody>();
        rigid.velocity = velocity;
        rigid.velocity += Vector3.up;
        rigid.angularVelocity = angularVelocity;
    }
}
