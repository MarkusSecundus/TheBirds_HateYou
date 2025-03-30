using MarkusSecundus.Utils.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.DamageSystem
{
    public class BasicArmorPiece : MonoBehaviour, IArmorPiece
    {
        public Damageable Damageable { get; private set; }


        public float DamageMultiplier = 1f;

        [SerializeField] SerializableDictionary<DamageType, float> DamageModifiers;


        [SerializeField] UnityEvent<AttackDeclaration> OnAttacked;

        void Start()
        {
            Damageable = Damageable.Get(this);
        }

        public void Attack(AttackDeclaration attackDeclaration)
        {
            float damageMultiplier = DamageModifiers.Values.GetValueOrDefault(attackDeclaration.Type, this.DamageMultiplier);
            float damage = attackDeclaration.Damage * damageMultiplier;

            Debug.Log($"Damage: {attackDeclaration.Attacker.name} attacks {Damageable.name} for {attackDeclaration.Damage} HP", this);
            OnAttacked?.Invoke(attackDeclaration);
            Damageable.Damage(attackDeclaration.Damage);
        }
    }
}
