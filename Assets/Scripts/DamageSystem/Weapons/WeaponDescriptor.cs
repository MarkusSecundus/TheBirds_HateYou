using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponDescriptor : MonoBehaviour
{
    public KeyCode ActivationKey;
    public WeaponType WeaponType;
    public string GunName;
    public int MaxAmmo;
    public bool IsEnabled = false;

    [SerializeField] float _currentAmmo = -1f;
    public float CurrentAmmo { get => _currentAmmo < 0f ? _currentAmmo = MaxAmmo : _currentAmmo; private set => _currentAmmo = value; }
    public bool HasAmmo => CurrentAmmo != 0f;
    public bool HasInfiniteAmmo => MaxAmmo < 0;

    public UnityEvent<WeaponDescriptor> OnStateUpdated;

    public struct AmmoUpdateEventArgs
    {
        public int OriginalAmmo { get; init; }
    }

    public bool AddAmmo(float amount)
    {
        if (HasInfiniteAmmo) return true;
        if (amount == 0f) return true;

        var oldAmmo = CurrentAmmo;
        var newAmmo = CurrentAmmo = Mathf.Clamp(oldAmmo + amount, 0f, MaxAmmo);
        OnStateUpdated?.Invoke(this);
        return oldAmmo != newAmmo;
    }

    private void Awake()
    {
        if (CurrentAmmo < 0) CurrentAmmo = MaxAmmo;
    }
}
