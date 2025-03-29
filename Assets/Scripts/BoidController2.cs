using Cinemachine.Utility;
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
    [SerializeField] float _distanceToDie = 40f;
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
    [SerializeField] float _boidWeight = 1f;

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
        if(_enemy.position.Distance(_rb.position) > _distanceToDie)
        {
            Destroy(gameObject);
            return;
        }

        if (_model.IsNotNil())
        {
            float rot = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
            _model.rotation = Quaternion.Euler(0, 0, rot);
        }

        Vector2 enemyPursueVelocity = (_enemy.position - _rb.position).normalized * _permittedSpeed.Max;
        Vector2 enemyPursueVelocityDistance = (enemyPursueVelocity - _rb.velocity).ClampMagnitude(0f, _maxForce);

        if (_radarData.Active.IsEmpty() || _rb.velocity.sqrMagnitude < _wanderThreshold.Sqr() || _enemyPursueFactor >= 0.95f)
        {
            _rb.AddForce(enemyPursueVelocityDistance);
            return;
        }

        float separationWeightSum = 0f;
        float alignmentWeightSum = 0f;

        Vector2 separationVelocity = Vector2.zero;
        Vector2 averageVelocity = Vector2.zero;
        Vector2 centerOfFlockRelative = Vector2.zero;
        
        foreach(var neighbor in _radarData.Active)
        {
            if (neighbor.IsNil()) continue;

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

        if(separationWeightSum > 0f) separationVelocity *= 1f / separationWeightSum;
        if (alignmentWeightSum > 0f) averageVelocity *= 1f / alignmentWeightSum;
        centerOfFlockRelative *= 1f / _radarData.Active.Count;

        Vector2 desiredVelocity = averageVelocity.normalized * _permittedSpeed.Max;
        Vector2 alignmentVelocity = (desiredVelocity - _rb.velocity).ClampMagnitude(0f, _maxForce);
        Vector2 cohesionVelocity = centerOfFlockRelative;
        
        separationVelocity *= SeparationWeight;
        alignmentVelocity *= AlignmentWeight;
        cohesionVelocity *= CohesionWeight;

        Vector2 totalTargetVelocity = (separationVelocity + alignmentVelocity + cohesionVelocity) * _boidWeight;
        //Vector2 velocityDirection = (totalTargetVelocity - _rb.velocity).ClampMagnitude(_permittedSpeed.Min, _permittedSpeed.Max);

        Vector2 actualAppliedForce = Vector2.Lerp(totalTargetVelocity, enemyPursueVelocity, _enemyPursueFactor);
        if(!actualAppliedForce.IsNaN())
            _rb.AddForce(actualAppliedForce);


    }

    public void Randomize(System.Random random)
    {
        _enemyPursueFactor = _enemyPursueProbabilityCurve.Evaluate(random.NextFloat()).Clamp01();
    }
}
