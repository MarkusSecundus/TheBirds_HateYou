using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInSeconds : MonoBehaviour
{
    [SerializeField] float _lifetime = 1f;
    void Start()
    {
        Destroy(gameObject, _lifetime);
    } 
}
