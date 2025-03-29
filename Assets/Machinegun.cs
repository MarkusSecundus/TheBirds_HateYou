using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machinegun : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;

    [SerializeField] private float _bulletSpeed;    

    public void Fire(Vector3 parentVelocity)
    {
        Bullet newBullet = Instantiate(_bulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Bullet>();
        newBullet.Initialise(_bulletSpeed + parentVelocity.magnitude);
    }

    private void Start()
    {
        DOVirtual.DelayedCall(0.25f, () => { Fire(Vector3.zero); }).SetLoops(-1);
    }
}
