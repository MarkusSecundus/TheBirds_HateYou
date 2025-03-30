using Assets.Scripts.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyArmor : MonoBehaviour, IArmorPiece
{
    public Damageable Damageable => null;

    public void Attack(AttackDeclaration attackDeclaration) {}

    public override bool Equals(object other) => ReferenceEquals(this, null) || ReferenceEquals(this, other) || base.Equals(other);

    // Override just to silence the warnings
    public override int GetHashCode() => base.GetHashCode();
}
