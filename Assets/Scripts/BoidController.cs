using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour
{
    [SerializeField] ColliderActivityTracker _radar;

    ColliderActivityInfo<BoidController> _radarData = new ColliderActivityInfo<BoidController>(c=>(c as Collider2D)?.attachedRigidbody?.GetComponent<BoidController>());

    [SerializeField] Interval<float> _permittedSpeed = new Interval<float>(1f, 50f);

    [SerializeField] float SeparationWeight;
    [SerializeField] AnimationCurve SeparationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] float AlignmentWeight;
    [SerializeField] AnimationCurve AlignmentCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] float CohesionWeight;
    [SerializeField] float _wanderThreshold = 1f;

    [SerializeField] int NeighborsCount = 0;
    [SerializeField] Vector2 Velocity;
    [SerializeField] Vector2 Separation;
    [SerializeField] Vector2 Alignment;
    [SerializeField] Vector2 Cohesion;

    Rigidbody2D _rb;

    static readonly System.Random _rand = new System.Random();

    Vector2 _defaultDirection;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _radar.RegisterListener(_radarData);
        _defaultDirection = _rand.NextUnitVector2();
    }

    void Update()
    {
        NeighborsCount = _radarData.Active.Count;
        Velocity = _rb.velocity;
        if (_radarData.Active.IsEmpty() || _rb.velocity.sqrMagnitude < _wanderThreshold.Sqr())
        {
            var direction = _defaultDirection * _permittedSpeed.Average();
            _rb.AddForce(direction - _rb.velocity);
            return;
        }

        Vector2 separationVelocity = Vector2.zero;
        Vector2 averageVelocity = Vector2.zero;
        Vector2 centerOfFlock = Vector2.zero;
        
        foreach(var neighbor in _radarData.Active)
        {
            var directionToNeighbor = neighbor._rb.position - _rb.position;
            var directionToNeighborNormalized = directionToNeighbor.Normalized(out float distance);
            {
                Vector2 newEntry = (-directionToNeighborNormalized) * SeparationCurve.Evaluate(distance);
                separationVelocity += newEntry;
            }
            {
                Vector2 newEntry = neighbor._rb.velocity * AlignmentCurve.Evaluate(distance);
                averageVelocity += newEntry;
            }
            {
                centerOfFlock += neighbor._rb.position;
            }
        }

        separationVelocity *= 1f/_radarData.Active.Count;
        averageVelocity *= 1f / _radarData.Active.Count;
        centerOfFlock *= 1f / _radarData.Active.Count;

        Vector2 alignmentVelocity = averageVelocity - _rb.velocity;
        Vector2 cohesionVelocity = centerOfFlock - _rb.position;

        separationVelocity *= SeparationWeight;
        alignmentVelocity *= AlignmentWeight;
        cohesionVelocity *= CohesionWeight;

        Separation = separationVelocity;
        Alignment = alignmentVelocity;
        Cohesion = cohesionVelocity;

        Vector2 totalTargetVelocity = (separationVelocity + alignmentVelocity + cohesionVelocity);
        Vector2 velocityDirection = (totalTargetVelocity - _rb.velocity).ClampMagnitude(_permittedSpeed.Min, _permittedSpeed.Max);

        _rb.AddForce(velocityDirection);
    }
}
