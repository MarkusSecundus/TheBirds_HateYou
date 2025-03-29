using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [SerializeField] float EnginePower;
    [SerializeField, Range(0f, 1f)] float Assymetry = 0.5f;

    [SerializeField] Transform _leftEngine;
    [SerializeField] Transform _rightEngine;
    [SerializeField] Transform _engineThrustDirectionPoint;

    [SerializeField] float _maxVelocity = 10f;
    [SerializeField] float _acceleration = 1f;

    Vector3 EngineThrustDirection => (_engineThrustDirectionPoint.position - this.transform.position).normalized;

    Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.centerOfMass = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 totalForce = EngineThrustDirection * EnginePower;
        var (leftForce, rightForce) = (Assymetry * totalForce, (1f - Assymetry) * totalForce);

        //Debug.Log($"Applying forces: {leftForce} | {rightForce}", this);
        _rb.AddForceAtPosition(leftForce, _leftEngine.position);
        _rb.AddForceAtPosition(rightForce, _rightEngine.position);
    }
}
