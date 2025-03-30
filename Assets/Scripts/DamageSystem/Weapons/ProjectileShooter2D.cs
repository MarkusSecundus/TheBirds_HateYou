using Assets.Scripts.DamageSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Input;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using UnityEngine.Events;



public abstract class ProjectileShooterBase2D : MonoBehaviour
{
    public interface IProjectile
    {
        public void OnShot(ProjectileShooterBase2D weapon);

        public bool CheckCanShoot() => true;
    }

    [SerializeField] float ShootForce;
    [SerializeField] float Cooldown_seconds = 0.3f;
    [SerializeField] int MaxProjectilesInExistence = -1;
    [SerializeField] Transform ShootDirection;
    [SerializeField] Rigidbody2D BulletPrototype;

    [SerializeField] UnityEvent OnAttack;
    [SerializeField] UnityEvent OnAttackOutOfAmmo;

    IProjectile[] _bulletPrototypeScripts;

    WeaponDescriptor _weaponDescriptor;


    LinkedList<Rigidbody2D> _projectiles = new();

    

    void _removeDeadProjectiles()
    {
        for (var node = _projectiles.First; node != null; )
        {
            var next = node.Next;
            if(node.Value.IsNil()) node.List.Remove(node);
            node = next;
        }
    }

    protected virtual void Start()
    {
        _weaponDescriptor = GetComponentInParent<WeaponDescriptor>();
        _bulletPrototypeScripts = BulletPrototype.GetComponentsInChildren<IProjectile>(true);

        StartCoroutine(deadProjectileRemover());
        IEnumerator deadProjectileRemover()
        {
            while (true)
            {
                _removeDeadProjectiles();
                yield return new WaitForSeconds(2f);
            }
        }
    }

    protected abstract bool CheckIsShootingRequested();

    private void Update()
    {
        if (CheckIsShootingRequested())
                DoShoot();
    }

    double _nextPermittedShotTime = double.NegativeInfinity;
    public void DoShoot()
    {
        _removeDeadProjectiles();
        if (MaxProjectilesInExistence >= 0 && _projectiles.Count >= MaxProjectilesInExistence)
        {
            Debug.Log($"Cannot shoot - max projectiles in existence count reached!");
            OnAttackOutOfAmmo?.Invoke();
            return;
        }
        if (Time.timeAsDouble < _nextPermittedShotTime)
            return;
        if (_bulletPrototypeScripts?.Length > 0 && _bulletPrototypeScripts.Any(sc => !sc.CheckCanShoot()))
        {
            Debug.Log($"Cannot shoot - Bullet refuses!");
            OnAttackOutOfAmmo?.Invoke();
            return;
        }
        _nextPermittedShotTime = Time.timeAsDouble + Cooldown_seconds;

        if (_weaponDescriptor?.AddAmmo(-1) == false)
        {
            OnAttackOutOfAmmo?.Invoke();
            return;
        }

        var bullet = Instantiate(BulletPrototype, null);
        bullet.transform.position = BulletPrototype.transform.position;
        bullet.transform.rotation = BulletPrototype.transform.rotation;
        bullet.gameObject.SetActive(true);
        foreach (IProjectile script in bullet.GetComponentsInChildren<IProjectile>(true)) script.OnShot(this);

        _projectiles.AddLast(bullet);

        if (ShootDirection && !bullet.isKinematic)
        {
            var shootForce = (ShootDirection.position - transform.position).normalized * ShootForce;
            bullet.AddForce(shootForce, ForceMode2D.Impulse);
        }
        OnAttack?.Invoke();
    }
}

public class ProjectileShooter2D : ProjectileShooterBase2D
{
    [SerializeField] KeyCode KeyToShoot = KeyCode.Mouse0;
    IKeyInputSource _input;

    protected override void Start()
    {
        _input = IKeyInputSource.Get(this);
        base.Start();
    }
    protected override bool CheckIsShootingRequested() => _input?.GetKeyDown(KeyToShoot) == true;
}
