

using MarkusSecundus.Utils.Primitives;
using UnityEngine;

[CreateAssetMenu(fileName = "Airplane", menuName = "Config/AirplaneConfig", order = 1)]
public class AirplaneConfig : ScriptableObject
{
    public Interval<float> VelocityRange = new Interval<float>(10f, 30f);
    public float VelocityChangePerSecond = 70f;

    public float RotationChangePerSeconds_degrees = 40;

    public float VelocityApplicationCap = 30;
    public float RotationApplicationCap = 90;
}