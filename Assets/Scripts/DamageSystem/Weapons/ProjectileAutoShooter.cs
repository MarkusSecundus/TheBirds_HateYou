using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAutoShooter : ProjectileShooterBase2D
{
    [field: SerializeField] public bool ShouldShoot { get; set; } = true;
    protected override bool CheckIsShootingRequested() => ShouldShoot;
}
