using DG.Tweening;
using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SmokeBurstEmitter : MonoBehaviour
{
    [SerializeField] private float _emissionRate;
    [SerializeField] private Interval<float> _burstDuration;
    [SerializeField] private Interval<float> _burstDelayDuration;

    private VisualEffect smokeEmitter;
    const string _emissionName = "EmissionRate";
    // Start is called before the first frame update
    void Start()
    {
        smokeEmitter = GetComponent<VisualEffect>();
        //smokeEmitter.SetFloat(_emissionName, 0f);
        DoBurst();
    }

    void DoBurst()
    {
        smokeEmitter.SetFloat(_emissionName, _emissionRate);
        float burstDur = Random.Range(_burstDuration.Min, _burstDuration.Max);
        DOVirtual.DelayedCall(burstDur, () => smokeEmitter.SetFloat(_emissionName, 0f));
        float delayDur = Random.Range(_burstDelayDuration.Min, _burstDelayDuration.Max);
        DOVirtual.DelayedCall(burstDur + delayDur, () => DoBurst());
    }
}
