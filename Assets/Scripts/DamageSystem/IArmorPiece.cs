using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.DamageSystem
{
    [System.Serializable]public enum DamageType
    {
        Piercing, Sharp, Blunt, Fire, Explosion
    }

    [System.Serializable] public struct AttackDeclaration
    {
        public Object Attacker;
        public DamageType Type;
        public float Damage;
    }

    public interface IArmorPiece
    {
        private static readonly ConditionalWeakTable<GameObject, IArmorPiece> _armorPieceCache = new();
        public static IArmorPiece Get(GameObject o)
        {
            if (_armorPieceCache.TryGetValue(o, out var ret)) return ret;
            ret = o.GetComponentInParent<IArmorPiece>();
            if (ret.IsNil()) ret = null; //instead of a potentially allocated but invalid object use a true null
            // When the objects doesn't have any armor, we want to cache that info as well, because it still means we'll be looking that info up more often. And unsuccessfull GetComponent lookups are the most expensive.
            _armorPieceCache.Add(o, ret);
            return ret;
        }
        public static IArmorPiece Get(Component c) => Get(c.gameObject);

        public void Attack(AttackDeclaration attackDeclaration);

        public Damageable Damageable { get; }
    }
}
