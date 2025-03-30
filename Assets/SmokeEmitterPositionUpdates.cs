using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SmokeEmitterPositionUpdates : MonoBehaviour
{
    VisualEffect smokeEmitter;
    Vector3 lastPosition;
    Vector3 prevPosition;

    const string lastName = "LastPosition";
    const string currName = "CurrentPosition";
    // Start is called before the first frame update
    void Start()
    {
        smokeEmitter = GetComponent<VisualEffect>();
        lastPosition = transform.position;
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        smokeEmitter.SetVector3(lastName, prevPosition);
        smokeEmitter.SetVector3(currName, lastPosition);
        prevPosition = lastPosition;
        lastPosition = transform.position;
        //Debug.Log(lastPosition + " : " + smokeEmitter.HasVector3(lastName));       
    }
}
