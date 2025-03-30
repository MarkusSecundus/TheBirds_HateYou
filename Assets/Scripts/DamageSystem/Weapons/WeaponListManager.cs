using MarkusSecundus.Utils.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class WeaponListManager : MonoBehaviour
{
    public WeaponDescriptor CurrentWeapon { get; private set; }
    Dictionary<KeyCode, WeaponDescriptor> _weapons;

    public UnityEvent<WeaponDescriptor> OnStateUpdated;

    IInputProvider<InputAxis> _input;

    private void Start()
    {
        _input = IInputProvider<InputAxis>.Get(this);
        _weapons = GetComponentsInChildren<WeaponDescriptor>(true).ToDictionary(w=>w.ActivationKey);
        foreach (var (_, w) in _weapons) w.OnStateUpdated.AddListener(OnWeaponUpdated);

        CurrentWeapon = _weapons.Values.First(w => w.IsEnabled);
        foreach (var (_, w) in _weapons) w.gameObject.SetActive(false);
        CurrentWeapon.gameObject.SetActive(true);
        Assert.IsTrue(CurrentWeapon.IsEnabled);
        OnWeaponUpdated(CurrentWeapon);
    }
    private void Update()
    {
        foreach(var (k,w) in _weapons)
        {
            if (w.IsEnabled && _input.GetKeyDown(k))
                SwitchWeapon(w);
        }
    }

    public void SwitchWeapon(WeaponDescriptor newWeapon)
    {
        if (!newWeapon.IsEnabled) return;
        if (newWeapon == CurrentWeapon) return;
        if(CurrentWeapon)
            CurrentWeapon.gameObject.SetActive(false);
        if((CurrentWeapon = newWeapon))
            newWeapon.gameObject.SetActive(true);
        OnWeaponUpdated(newWeapon);
    }

    void OnWeaponUpdated(WeaponDescriptor weapon)
    {
        OnStateUpdated?.Invoke(weapon);
    }
}
