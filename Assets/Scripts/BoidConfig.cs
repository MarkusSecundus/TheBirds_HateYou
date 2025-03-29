using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boid", menuName = "Config/BoidConfig", order = 1)]
public class BoidConfig : ScriptableObject
{
    public float DistanceToDie = 70f;
    public AnimationCurve EnemyPursueProbabilityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public Interval<float> VelocityRange = new Interval<float>(8f, 25f);

    [Range(0f,1f)] public float BoidBehaviorWeight = 0.8f;

    [Header("Separation")]
    public float SeparationWeight = 1f;
    public float SeparationDistance = 3.5f;
    public AnimationCurve SeparationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Alignment")]
    public float AlignmentWeight = 0.4f;
    public float AlignmentDistance = 15f;
    public AnimationCurve AlignmentCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Cohesion")]
    public float CohesionWeight = 0.22f;
}
