using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdHitScreen : UIScreen
{
    public IntroBirdStrike strikeFX;
    public float hitTime;
    public override void ShowScreen()
    {
        base.ShowScreen();

        DOVirtual.DelayedCall(hitTime, () => strikeFX.PlayEffect());
    }
}
