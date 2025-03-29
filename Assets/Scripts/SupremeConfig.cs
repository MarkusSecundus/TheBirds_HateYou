using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupremeConfig : ScriptableObject
{
    [System.Serializable]
    public struct AirplaneConfig
    {

    }
    [System.Serializable]
    public struct BoidConfig
    {
    }

    public AirplaneConfig Aeroplane;
    public BoidConfig Boid;
}
