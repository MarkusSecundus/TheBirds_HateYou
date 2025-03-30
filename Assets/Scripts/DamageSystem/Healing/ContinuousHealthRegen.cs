using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DamageSystem.Healing
{
    public class ContinuousHealthRegen : MonoBehaviour
    {
        [SerializeField] Damageable Target;
        [SerializeField] float HpPerSecond;

        private void Start()
        {
            if (!Target) Target = Damageable.Get(this);
        }

        private void Update()
        {
            Target.Heal(HpPerSecond * Time.deltaTime);
        }
    }
}
