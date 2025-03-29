using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [SerializeField] float _maxVelocityChange = 10f;
    [SerializeField] float _maxTorqueChange = 2f;

    [SerializeField] float _maxVelocity = 10f;
    [SerializeField] float _maxTorque = 1f;
    [SerializeField] float _acceleration = 1f;
    [SerializeField] float _angularAcceleration = 1f;


    [SerializeField] float _enginePower = 0f;
    [SerializeField] float _targetRotation;

    Rigidbody2D _rb;

    [SerializeField] Transform _airplaneModel;
    [SerializeField] Vector3 modelUp;
    [SerializeField] Vector3 _modelShiftPre;
    [SerializeField] Vector3 _modelShiftPost;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.centerOfMass = transform.localPosition;
        _targetRotation = _rb.rotation;
        _enginePower = _maxVelocity * 0.65f;

        _airplaneModel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg);
    }

    // Update is called once per frame
    void Update()
    {
        float enginePowerDelta = 0f;
        float rotationDelta = 0f;

        if (Input.GetKey(KeyCode.A))
            rotationDelta += _angularAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            rotationDelta -= _angularAcceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.W))
            enginePowerDelta += _acceleration * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            enginePowerDelta -= _acceleration * Time.deltaTime;

        _enginePower = (_enginePower + enginePowerDelta).Clamp(0, _maxVelocity);
        _targetRotation += rotationDelta.Clamp(-_maxTorque, _maxTorque);

        Vector2 targetVelocity = transform.up * _enginePower;
        Vector2 velocityDistance = (targetVelocity - _rb.velocity).ClampMagnitude(0f, _maxVelocityChange);

        float rotationDistance = _targetRotation - _rb.rotation;
        float torqueDistance = (rotationDistance - _rb.angularVelocity).Clamp(-_maxTorqueChange, _maxTorqueChange);

        _rb.AddForce(velocityDistance);
        _rb.AddTorque(torqueDistance);

        UpdateModel();
    }

    void UpdateModel()
    {
        if (_airplaneModel.IsNil()) return;
        float desiredRotation = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
        //if(Mathf.Abs(desiredRotation - _airplaneModel.rotation.eulerAngles.z) < 90f)
            _airplaneModel.rotation = Quaternion.Euler(0, 0, desiredRotation);

    }
}
