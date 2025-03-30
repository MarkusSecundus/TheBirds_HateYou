using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    public float RotationSpeed;    

    private void Update()
    {
        this.transform.Rotate(new Vector3(RotationSpeed * Time.deltaTime, 0f, 0f));
    }
}
