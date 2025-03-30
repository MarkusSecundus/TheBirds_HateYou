using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DamageSystem.Damagers
{
    public abstract class IOnRequestDamager : MonoBehaviour
    {
        public abstract void PerformAttack();
    }
}
