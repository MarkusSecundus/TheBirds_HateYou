using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float _timeToLive;
    [SerializeField] LayerMask _boidLayerMask;

    [SerializeField] GameObject _birdDeathVFX;

    float _velocity;
    Rigidbody2D _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = transform.right * _velocity;
    }
    // Start is called before the first frame update
    public void Initialise(float velocity)
    {
        _velocity = velocity;
        Destroy(gameObject, _timeToLive);        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collision with {collision.gameObject.name}");
        if (LayerContains(collision.gameObject.layer, _boidLayerMask))
        {
            collision.gameObject.GetComponentInParent<BoidController>().DoDie();

            //Destroy(gameObject);
        }
        
    }

    bool LayerContains(int queryLayer, LayerMask mask)
    {
        if ((mask.value & (1 << queryLayer)) > 0) return true;
        return false;
    }


}
