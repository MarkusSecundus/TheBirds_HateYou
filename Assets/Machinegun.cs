using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machinegun : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;

    [SerializeField] private float _bulletSpeed;

    [SerializeField] private Transform _bulletSpawnPoint;

    public void Fire(Vector3 parentVelocity)
    {
        Bullet newBullet = Instantiate(_bulletPrefab, _bulletSpawnPoint.position, _bulletSpawnPoint.rotation).GetComponent<Bullet>();
        newBullet.Initialise(_bulletSpeed + parentVelocity.magnitude);
    }

    private void Start()
    {
        //DOVirtual.DelayedCall(0.25f, () => { Fire(_bulletSpawnPoint.right * 10f); }).SetLoops(-1);
    }
}
