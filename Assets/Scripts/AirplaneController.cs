using MarkusSecundus.Utils;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        _rb.centerOfMass = Vector3.zero;// transform.localPosition;
        _rb.inertia = 1;

        _lastRotation = _rb.rotation;
        _currentEnginePower = cfg.VelocityRange.Average();
        _airplaneModel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg);
    }

    float speedInput = 0f, rotationInput = 0f;
    void Update()
    {
        ReadInput();
        UpdateModel();
        UpdateHeight(Time.deltaTime);

        _tempThrottleUI.Slider.value = Mathf.Lerp(_tempThrottleUI.Slider.value, 1 - (_currentEnginePower - cfg.VelocityRange.Min) / (cfg.VelocityRange.Max - cfg.VelocityRange.Min), 0.9f);
    }

    private void FixedUpdate()
    {
        EvaluateForces();
    }



    [SerializeField]
    SerializableDictionary<float, UnityEvent> _actionsOnHealthDrop;
    [SerializeField]
    SerializableDictionary<float, UnityEvent> _actionsOnHeightDrop;


    [SerializeField] AnimationCurve DamageToHeightLossMapping;
    public float MaxHeightLoss = 20f;

    [SerializeField] float _heightLossRate = 0.5f;

    
    public float CurrentHeight = 20000;

    public void HandleDamageChange(Damageable.HealthChangeInfo info)
    {
        return;
        _heightLossRate = DamageToHeightLossMapping.Evaluate(1f - info.Damageable.HpRatio) * MaxHeightLoss;
        var (originalHpRatio, hpRatio) = (info.OriginalHP / info.Damageable.MaxHP, info.ResultHP / info.Damageable.MaxHP);
        foreach(var (actionRatio, action) in _actionsOnHealthDrop.Values)
        {
            if (hpRatio <= actionRatio && actionRatio < originalHpRatio)
                action?.Invoke();
        }
    }


    [System.Serializable]
    public struct HeightLayer
    {
        public Transform Root;
        public ParallaxEffect Parallax;
        public float MinHeight;
        public float FadeStartHeight;
        public float MaxHeight;
        public float MaxScale;
        public Vector3 StartParallax;
        public Vector3 EndParallax;
    }
    public HeightLayer[] HeightLayers;

    void UpdateHeight(float delta)
    {
        CurrentHeight -= _heightLossRate * delta;
        foreach(var layer in HeightLayers)
        {
            if (layer.Root.IsNil() || !layer.Root.gameObject.activeInHierarchy) continue;
            if (CurrentHeight >= layer.MaxHeight) continue;
            if(CurrentHeight < layer.MinHeight)
            {
                layer.Root.gameObject.SetActive(false);
                continue;
            }

            float fadeRatio = (CurrentHeight - layer.MinHeight) / (layer.FadeStartHeight - layer.MinHeight);
            foreach(var rnd in layer.Root.GetComponentsInChildren<RawImage>())
            {
                rnd.color = rnd.color.WithAlpha(fadeRatio);
            }

            float scaleRatio = (CurrentHeight - layer.MinHeight) / (layer.MaxHeight - layer.MinHeight);
            float scale = Mathf.Lerp(layer.MaxScale, 1f, scaleRatio);
            layer.Root.ScaleAroundPoint(new Vector3(scale, scale, 1f), this.transform.position);
            if (layer.Parallax.IsNotNil())
            {
                layer.Parallax.MovementSpeed = Vector3.Lerp(layer.StartParallax, layer.EndParallax, scaleRatio);
            }
        }
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
