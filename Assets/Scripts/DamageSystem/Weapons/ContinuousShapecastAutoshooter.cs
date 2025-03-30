using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousShapecastAutoshooter : ContinuousShapecastShooterBase
{
    [SerializeField] bool IsShooting = false;
    protected override bool CheckIsActivated() => IsShooting;
}
