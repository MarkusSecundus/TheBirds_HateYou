using MarkusSecundus.Utils.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum InputAxis
{
    MouseX, MouseY, Horizontal, Vertical
}

public class ProjectileInputProvider : AbstractRedirectedInputProvider<InputAxis>, ProjectileShooterBase2D.IProjectile
{
    IInputProvider<InputAxis> _sourceField;
    protected override IInputProvider<InputAxis> _source => _sourceField;

    public void OnShot(ProjectileShooterBase2D weapon)
    {
        _sourceField = IInputProvider<InputAxis>.Get(weapon);
    }
}
