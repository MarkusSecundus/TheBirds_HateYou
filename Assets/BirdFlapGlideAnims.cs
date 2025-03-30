using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlapGlideAnims : MonoBehaviour
{
    string[] stateNames = new string[] { "FlapState", "GlideState" };

    [SerializeField] private Interval<float> _stateHoldInterval;
    [SerializeField] private float _flapSpeed = 1f;
    private int stateIndex;
    Animator _animator;
    private float _switchTimer;
    private void Start()
    {
        stateIndex = Random.Range(0, 2);
        _animator = GetComponent<Animator>();
        SetState(stateNames[stateIndex]);
        _switchTimer = Random.Range(0f, _stateHoldInterval.Max);
    }

    private void Update()
    {
        _switchTimer -= Time.deltaTime;
        if (_switchTimer <= 0f)
            SwitchState();
    }
    void SwitchState()
    {
        stateIndex = 1 - stateIndex;
        SetState(stateNames[stateIndex]);
        _switchTimer = Random.Range(_stateHoldInterval.Min, _stateHoldInterval.Max);
    }

    void SetState(string name)
    {
        switch(name)
        {
            case "FlapState":
                _animator.Play("FlapState");
                _animator.speed = _flapSpeed;
                break;
            case "GlideState":
                _animator.Play("GlideState");
                _animator.speed = 0f;
                break;
            default:
                break;
        }
    }
}
