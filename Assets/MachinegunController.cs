using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachinegunController : MonoBehaviour
{
    [SerializeField] private List<Machinegun> _machineguns = new List<Machinegun>();

    private List<float> _fireCooldowns = new List<float>();
    private List<bool> _machinegunDamaged = new List<bool>();

    [SerializeField] private float _machinegunCooldown = 0.25f;
    [SerializeField] private float _machinegunDamagedCooldown = 0.4f;
    private void Start()
    {
        _fireCooldowns = new List<float>(_machineguns.Count);
        _machinegunDamaged = new List<bool>(_machineguns.Count);
        for (int i = 0; i < _machineguns.Count; i++)
        {
            _fireCooldowns.Add(0f);
            _machinegunDamaged.Add(false);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _machineguns.Count; i++)
        {
            _fireCooldowns[i] -= Time.deltaTime;            
        }
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.JoystickButton0) 
            || Input.GetKey(KeyCode.JoystickButton1) || Input.GetKey(KeyCode.JoystickButton2) || Input.GetKey(KeyCode.JoystickButton3) 
            || Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.JoystickButton6) 
            || Input.GetKey(KeyCode.JoystickButton7) || Input.GetKey(KeyCode.JoystickButton8) || Input.GetKey(KeyCode.JoystickButton9)
            || Input.GetKey(KeyCode.JoystickButton10) || Input.GetKey(KeyCode.JoystickButton11) || Input.GetKey(KeyCode.JoystickButton12)
        )
            FireInput();
    }

    public void FireInput()
    {
        for (int i = 0; i < _machineguns.Count; i++)
        {
            if (_fireCooldowns[i] <= 0)
            {
                _machineguns[i].Fire(Vector3.zero);
                _fireCooldowns[i] = (_machinegunDamaged[i]) ? _machinegunDamagedCooldown : _machinegunCooldown;
            }
        }
    }

    public void MachinegunDamaged(int index)
    {
        _machinegunDamaged[index] = true;        
    }
}
