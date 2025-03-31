using Cinemachine.Utility;
using DG.Tweening;
using MarkusSecundus.Utils.Behaviors.Automatization;
using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using UnityEngine;

public class BoidController : MonoBehaviour, IRandomizer
{
    [SerializeField] Transform _model;
    public Transform Model => _model;
    [SerializeField] ColliderActivityTracker _radar;
    public Vector2 LastVelocity { get; private set; }

    public Rigidbody2D _enemy;
    [SerializeField] BoidConfig cfg;
    
    [SerializeField] float _enemyPursueFactor;

    [SerializeField] GameObject _deathVFX;

    ColliderActivityInfo<BoidController> _radarData = new ColliderActivityInfo<BoidController>(c=>(c as Collider2D)?.attachedRigidbody?.GetComponent<BoidController>());
      

    Rigidbody2D _rb;



    public void DoDie()
    {

        Rigidbody2D bRb = GetComponentInParent<Rigidbody2D>();
        // spawn boid death vfx
        GameObject vfxObject = Instantiate(_deathVFX, this.transform.position, this.transform.rotation);
        Vector3 rbVelocity = new Vector3(this.LastVelocity.x, this.LastVelocity.y, 0f);
        vfxObject.transform.DOMove(vfxObject.transform.position + rbVelocity, 1.5f).SetEase(Ease.OutSine);
        //boid.Model.gameObject.SetActive(false);
        //boid.Model.GetComponent<Collider2D>().enabled = false;
        //boid.enabled = false;
        //Destroy(boid.gameObject, 2f); 

        Destroy(gameObject);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _radar.RegisterListener(_radarData);
        //_enemy = TagSearchable.FindByTag<Rigidbody2D>(cfg.EnemyTag);
    }

    void FixedUpdate()
    {
        if(_enemy.position.DistanceSqr(_rb.position) > cfg.DistanceToDie.Sqr())
        {
            Destroy(gameObject);
            return;
        }

        ApplySteering(Time.deltaTime);

        UpdateModel();

        LastVelocity = _rb.velocity;
    }

    void ApplySteering(float delta)
    {
        float boidBehaviorWeight = ((1f - _enemyPursueFactor) + cfg.BoidBehaviorWeight).Clamp01();

        Vector2 enemyPursueVelocity = (_enemy.position - _rb.position).normalized * cfg.VelocityRange.Max;
        Vector2 enemyPursueVelocityDistance = (enemyPursueVelocity - _rb.velocity);


        if (_radarData.Active.IsEmpty() || boidBehaviorWeight >= 0.95f)
        {
            Debug.Log($"S_{name}: {_rb.velocity.magnitude}", this);
            _rb.AddForce(enemyPursueVelocityDistance.ClampMagnitude(0f, cfg.VelocityApplicationCap));
            return;
        }



        float separationWeightSum = 0f;
        float alignmentWeightSum = 0f;

        Vector2 separationVelocity = Vector2.zero;
        Vector2 averageVelocity = Vector2.zero;
        Vector2 centerOfFlockRelative = Vector2.zero;

        foreach (var neighbor in _radarData.Active)
        {
            if (neighbor.IsNil()) continue;

            var directionToNeighbor = neighbor._rb.position - _rb.position;
            var directionToNeighborNormalized = directionToNeighbor.Normalized(out float distance);
            {
                float weight = cfg.SeparationCurve.Evaluate(distance / cfg.SeparationDistance);
                Vector2 toAdd = (-directionToNeighborNormalized) * weight;
                separationWeightSum += weight;
                separationVelocity += toAdd;
            }
            {
                float weight = cfg.AlignmentCurve.Evaluate(distance / cfg.AlignmentDistance);
                Vector2 toAdd = neighbor._rb.velocity * weight;
                alignmentWeightSum += weight;
                averageVelocity += toAdd;
            }
            {
                centerOfFlockRelative += directionToNeighbor;
            }
        }

        if (separationWeightSum > 0f) separationVelocity *= 1f / separationWeightSum;
        if (alignmentWeightSum > 0f) averageVelocity *= 1f / alignmentWeightSum;
        centerOfFlockRelative *= 1f / _radarData.Active.Count;

        Vector2 desiredVelocity = averageVelocity.normalized * cfg.VelocityRange.Max;
        Vector2 alignmentVelocity = (desiredVelocity - _rb.velocity).ClampMagnitude(cfg.VelocityRange);
        Vector2 cohesionVelocity = centerOfFlockRelative;

        float boidWeightSumInv = 1f / (cfg.SeparationWeight + cfg.AlignmentWeight + cfg.CohesionWeight);

        separationVelocity *= cfg.SeparationWeight * boidWeightSumInv;
        alignmentVelocity *= cfg.AlignmentWeight * boidWeightSumInv;
        cohesionVelocity *= cfg.CohesionWeight * boidWeightSumInv;

        Vector2 totalTargetVelocity = ((separationVelocity + alignmentVelocity + cohesionVelocity)).ClampMagnitude(cfg.VelocityRange);

        Vector2 actualAppliedForce = Vector2.Lerp(enemyPursueVelocityDistance, totalTargetVelocity, boidBehaviorWeight).ClampMagnitude(0f, cfg.VelocityApplicationCap);
        if (!actualAppliedForce.IsNaN())
            _rb.AddForce(actualAppliedForce);
        Debug.Log($"{name}: {_rb.velocity.magnitude}", this);
    }

    public void Randomize(System.Random random)
    {
        _enemyPursueFactor = cfg.EnemyPursueProbabilityCurve.Evaluate(random.NextFloat()).Clamp01();
    }

    void UpdateModel()
    {
        if (_model.IsNotNil())
        {
            float rot = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
            _model.rotation = Quaternion.Euler(0, 0, rot);
        }
    }
}
