using MarkusSecundus.Utils.Behaviors.Automatization;
using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController2 : MonoBehaviour, IRandomizer
{
    [SerializeField] Transform _model;
    [SerializeField] ColliderActivityTracker _radar;

    [SerializeField] string _enemyTag;
    [SerializeField] Rigidbody2D _enemy;
    [SerializeField] float _enemyPursueFactor;
    [SerializeField] AnimationCurve _enemyPursueProbabilityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    ColliderActivityInfo<BoidController2> _radarData = new ColliderActivityInfo<BoidController2>(c=>(c as Collider2D)?.attachedRigidbody?.GetComponent<BoidController2>());


    [SerializeField] Interval<float> _permittedSpeed = new Interval<float>(1f, 50f);

    [SerializeField] float SeparationWeight;
    [SerializeField] AnimationCurve SeparationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] float AlignmentWeight;
    [SerializeField] AnimationCurve AlignmentCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] float _maxForce = 2f;
    [SerializeField] float CohesionWeight;
    [SerializeField] float _wanderThreshold = 1f;

    [SerializeField] int NeighborsCount = 0;
    [SerializeField] float Velocity;
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
        _enemy = TagSearchable.FindByTag<Rigidbody2D>(_enemyTag);
    }

    void Update()
    {
        if (_model.IsNotNil())
        {
            float rot = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
            _model.rotation = Quaternion.Euler(0, 0, rot);
        }
        NeighborsCount = _radarData.Active.Count;
        Velocity = _rb.velocity.magnitude;
        if (_radarData.Active.IsEmpty() || _rb.velocity.sqrMagnitude < _wanderThreshold.Sqr())
        {
            var direction = _defaultDirection * _permittedSpeed.Average();
            _rb.AddForce(direction - _rb.velocity);
            return;
        }

        float separationWeightSum = 0f;
        float alignmentWeightSum = 0f;

        Vector2 separationVelocity = Vector2.zero;
        Vector2 averageVelocity = Vector2.zero;
        Vector2 centerOfFlockRelative = Vector2.zero;
        
        foreach(var neighbor in _radarData.Active)
        {
            var directionToNeighbor = neighbor._rb.position - _rb.position;
            var directionToNeighborNormalized = directionToNeighbor.Normalized(out float distance);
            {
                float weight = SeparationCurve.Evaluate(distance);
                Vector2 newEntry = (-directionToNeighborNormalized) * weight;
                separationWeightSum += weight;
                separationVelocity += newEntry;
            }
            {
                float weight = AlignmentCurve.Evaluate(distance);
                Vector2 newEntry = neighbor._rb.velocity * weight;
                alignmentWeightSum += weight;
                averageVelocity += newEntry;
            }
            {
                centerOfFlockRelative += directionToNeighbor;
            }
        }

        separationVelocity *= 1f/separationWeightSum;
        averageVelocity *= 1f / alignmentWeightSum;
        centerOfFlockRelative *= 1f / _radarData.Active.Count;

        Vector2 desiredVelocity = averageVelocity.normalized * _permittedSpeed.Max;
        Vector2 alignmentVelocity = (desiredVelocity - _rb.velocity).ClampMagnitude(0f, _maxForce);
        Vector2 cohesionVelocity = centerOfFlockRelative;
        
        separationVelocity *= SeparationWeight;
        alignmentVelocity *= AlignmentWeight;
        cohesionVelocity *= CohesionWeight;

        Separation = separationVelocity;
        Alignment = alignmentVelocity;
        Cohesion = cohesionVelocity;

        Vector2 totalTargetVelocity = (separationVelocity + alignmentVelocity + cohesionVelocity);
        //Vector2 velocityDirection = (totalTargetVelocity - _rb.velocity).ClampMagnitude(_permittedSpeed.Min, _permittedSpeed.Max);

        Vector2 enemyPursueVelocity = (_enemy.position - _rb.position).ClampMagnitude(0f, _permittedSpeed.Max);

        _rb.AddForce(totalTargetVelocity);


    }

    public void Randomize(System.Random random)
    {
        _enemyPursueFactor = _enemyPursueProbabilityCurve.Evaluate(random.NextFloat());
    }
}
