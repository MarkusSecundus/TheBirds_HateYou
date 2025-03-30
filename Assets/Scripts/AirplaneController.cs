using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    [SerializeField] AirplaneConfig cfg;


    float _currentEnginePower = 0f;

    const float MAX_DESIRED_ROTATION_CHANGE = 120f;
    float _desiredRotationChange = 0f;
    float _tilt;

    [SerializeField] float _tiltRate = 1f;
    [SerializeField] float _maxTiltAngle = 30f;

    Rigidbody2D _rb;

    [SerializeField] TempThrottleUI _tempThrottleUI;


    float _lastRotation;
    void EvaluateForces()
    {
        _currentEnginePower = (_currentEnginePower + speedInput).Clamp(cfg.VelocityRange);
        Vector2 desiredVelocity = transform.up * _currentEnginePower;

        _desiredRotationChange -= (_rb.rotation - _lastRotation).DegreesNormalize();
        _lastRotation = _rb.rotation;

        _desiredRotationChange = (_desiredRotationChange + rotationInput).Clamp(-MAX_DESIRED_ROTATION_CHANGE, MAX_DESIRED_ROTATION_CHANGE);
        float desiredTorque = (_desiredRotationChange - _rb.angularVelocity);

        _rb.AddForce((desiredVelocity - _rb.velocity).ClampMagnitude(0f, cfg.VelocityApplicationCap));
        _rb.AddTorque(desiredTorque.Clamp(-cfg.RotationApplicationCap, cfg.RotationApplicationCap));

        speedInput = 0f;
        rotationInput = 0f;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.centerOfMass = transform.localPosition;

        _lastRotation = _rb.rotation;
        _currentEnginePower = cfg.VelocityRange.Average();
        _airplaneModel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg);
    }

    float speedInput = 0f, rotationInput = 0f;
    void Update()
    {
        ReadInput();
        UpdateModel();

        _tempThrottleUI.Slider.value = Mathf.Lerp(_tempThrottleUI.Slider.value, 1 - (_currentEnginePower - cfg.VelocityRange.Min) / (cfg.VelocityRange.Max - cfg.VelocityRange.Min), 0.9f);
    }

    private void FixedUpdate()
    {
        EvaluateForces();
    }


    [SerializeField] Transform _airplaneModel;
    [SerializeField] Transform _airplaneTiltNode;
    [SerializeField] Vector3 modelUp;
    [SerializeField] Vector3 _modelShift;
    [SerializeField] List<TrailRenderer> _wingTipTrails;
    [SerializeField] float _wingTrailEmissionThreshold = 0.35f;
    void UpdateModel()
    {
        if (_airplaneModel.IsNil()) return;
        float desiredRotation = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
        //if(Mathf.Abs(desiredRotation - _airplaneModel.rotation.eulerAngles.z) < 90f)
        _airplaneModel.rotation = Quaternion.Euler(0f, 0f, desiredRotation);

        //tilt
        _airplaneTiltNode.localRotation = Quaternion.Euler(_tilt * _maxTiltAngle, 0f, 0f);

        // set wing trails to emit
        if(Mathf.Abs(_tilt) >= _wingTrailEmissionThreshold)
        {
            foreach (TrailRenderer rend in _wingTipTrails)
                rend.emitting = true;
        }
        else // stop emission
        {
            foreach (TrailRenderer rend in _wingTipTrails)
                rend.emitting = false;
        }
    }


    void ReadInput()
    {
        bool tiltChanged = false;

        if (Input.GetKey(KeyCode.A))
        {
            _tilt = Mathf.Clamp(_tilt + _tiltRate * Time.deltaTime, -1f, 1f);
            tiltChanged = true;
            rotationInput += cfg.RotationChangePerSeconds_degrees * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _tilt = Mathf.Clamp(_tilt - _tiltRate * Time.deltaTime, -1f, 1f);
            tiltChanged = true;
            rotationInput -= cfg.RotationChangePerSeconds_degrees * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))        
            speedInput += cfg.VelocityChangePerSecond * Time.deltaTime;        
        if (Input.GetKey(KeyCode.S))
            speedInput -= cfg.VelocityChangePerSecond * Time.deltaTime;

        if(!tiltChanged)
        {
            float tiltChange = Mathf.Min(Mathf.Abs(_tilt), _tiltRate * Time.deltaTime);
            _tilt -= _tilt > 0f ?  tiltChange: -tiltChange;
        }
    }
}
