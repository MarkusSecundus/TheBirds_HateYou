using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [SerializeField] AirplaneConfig cfg;


    float _currentEnginePower = 0f;
    float _desiredRotation;

    Rigidbody2D _rb;

    [SerializeField] TempThrottleUI _tempThrottleUI;



    void EvaluateForces(float speedChange, float rotationChange)
    {
        _currentEnginePower = (_currentEnginePower + speedChange * Time.deltaTime).Clamp(cfg.VelocityRange);
        Vector2 desiredVelocity = transform.up * _currentEnginePower;

        _desiredRotation = (_desiredRotation + rotationChange * Time.deltaTime);
        float neededRotationChange = _desiredRotation - _rb.rotation;
        float desiredTorque = (neededRotationChange - _rb.angularVelocity).DegreesNormalize();

        _rb.AddForce((desiredVelocity - _rb.velocity).ClampMagnitude(0f, cfg.VelocityApplicationCap));
        _rb.AddTorque(desiredTorque.Clamp(-cfg.RotationApplicationCap, cfg.RotationApplicationCap));
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.centerOfMass = transform.localPosition;

        _desiredRotation = _rb.rotation;
        _currentEnginePower = cfg.VelocityRange.Average();
        _airplaneModel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg);
    }

    void Update()
    {
        GetInput(out float enginePowerDelta, out float rotationDelta);
        EvaluateForces(enginePowerDelta, rotationDelta);

        UpdateModel();

        _tempThrottleUI.Slider.value = 1 - (_currentEnginePower - cfg.VelocityRange.Min) / (cfg.VelocityRange.Max - cfg.VelocityRange.Min);
    }


    [SerializeField] Transform _airplaneModel;
    [SerializeField] Vector3 modelUp;
    [SerializeField] Vector3 _modelShift;
    void UpdateModel()
    {
        if (_airplaneModel.IsNil()) return;
        float desiredRotation = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
        //if(Mathf.Abs(desiredRotation - _airplaneModel.rotation.eulerAngles.z) < 90f)
            _airplaneModel.rotation = Quaternion.Euler(0, 0, desiredRotation);

    }


    void GetInput(out float enginePowerDelta, out float rotationDelta)
    {
        enginePowerDelta = 0f;
        rotationDelta = 0f;

        if (Input.GetKey(KeyCode.A))
            rotationDelta += cfg.RotationChangePerSeconds_degrees;
        if (Input.GetKey(KeyCode.D))
            rotationDelta -= cfg.RotationChangePerSeconds_degrees;
        if (Input.GetKey(KeyCode.W))
            enginePowerDelta += cfg.VelocityChangePerSecond;
        if (Input.GetKey(KeyCode.S))
            enginePowerDelta -= cfg.VelocityChangePerSecond;
    }
}
