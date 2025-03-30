using Assets.Scripts.DamageSystem;
using MarkusSecundus.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDamager : MonoBehaviour
{
    [SerializeField] float DamagePerParticle;
    [SerializeField] DamageType DamageType;

    private void OnParticleCollision(GameObject other)
    {
        var armor = IArmorPiece.Get(other);
        if (armor.IsNil())
        {
            Debug.Log($"Unknown target: {other.name}", other);
            return;
        }
        armor.Attack(new AttackDeclaration { Attacker = this, Damage = DamagePerParticle, Type = DamageType });
    }
}
