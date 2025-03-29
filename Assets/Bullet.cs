using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] float _timeToLive;

    float _velocity;
    Rigidbody2D _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = transform.up * _velocity;
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
        Destroy(gameObject);
    }
}
