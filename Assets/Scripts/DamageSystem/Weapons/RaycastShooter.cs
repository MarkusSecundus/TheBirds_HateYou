using Assets.Scripts.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkusSecundus.Utils.Extensions;
using UnityEngine.Events;
using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Debugging;
using MarkusSecundus.Utils.Input;

public class RaycastShooter : MonoBehaviour
{
    [SerializeField] float _cooldown_seconds = 0.3f;
    [SerializeField] DamageType _damageType;
    [SerializeField] Transform _rayStartPoint;
    [System.Serializable] struct AttackPathDeclaration
    {
        [SerializeField] public float Damage;
        [SerializeField] public Transform RayEndPoint;
    }
    [SerializeField] AttackPathDeclaration[] _attackPaths;

    [SerializeField] UnityEvent OnAttack;
    [SerializeField] UnityEvent OnAttackOutOfAmmo;

    [SerializeField] KeyCode KeyToShoot = KeyCode.Mouse0;
    IInputProvider<InputAxis> input;
    WeaponDescriptor _weaponDescriptor;
    private void Start()
    {
        input = IInputProvider<InputAxis>.Get(this);
        _weaponDescriptor = GetComponentInParent<WeaponDescriptor>();
    }

    private void Update()
    {
        if (input.GetKeyDown(KeyToShoot))
            DoShoot();
    }

    double _nextPermittedShotTime = double.NegativeInfinity;
    void DoShoot()
    {
        if (Time.timeAsDouble < _nextPermittedShotTime)
            return;
        _nextPermittedShotTime = Time.timeAsDouble + _cooldown_seconds;

        if (!_weaponDescriptor.AddAmmo(-1))
        {
            OnAttackOutOfAmmo?.Invoke();
            return;
        }

        var damagePerVictim = new DefaultValDict<IArmorPiece, float>(v => 0f);
        foreach(var attackPath in _attackPaths)
        {
            var attackDirection = attackPath.RayEndPoint.position - _rayStartPoint.position;
            if (!UnityEngine.Physics.Raycast(_rayStartPoint.position, attackDirection, out var hitInfo, attackDirection.magnitude))
                continue;
            var armorPiece = IArmorPiece.Get(hitInfo.collider);
            if (armorPiece.IsNil())
                continue;
            damagePerVictim[armorPiece] += attackPath.Damage;
        }
        foreach(var (victim, damage) in damagePerVictim)
        {
            victim.Attack(new AttackDeclaration { Attacker = this, Damage = damage, Type = _damageType });
        }

        OnAttack.Invoke();
    }







#if UNITY_EDITOR
    [SerializeField] bool _drawGizmos = false; 
#endif
    private void OnDrawGizmos() 
    {
#if UNITY_EDITOR
        if (!_drawGizmos) return;
#endif

        if (!_rayStartPoint) return;

        using (GizmoHelpers.ColorScope(Color.red))
        {
            foreach (var attackPath in _attackPaths)
                if(attackPath.RayEndPoint)
                    Gizmos.DrawLine(_rayStartPoint.position, attackPath.RayEndPoint.position);
        }
    }
}
