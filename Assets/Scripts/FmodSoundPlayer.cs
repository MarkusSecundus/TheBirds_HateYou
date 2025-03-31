using FMOD.Studio;
using FMODUnity;
using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodSoundPlayer : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference _sound;

    #region OneShot

    double _lastOneshotPlayedTimestamp = float.NegativeInfinity;
    public void PlayOneShot()
    {
        _lastOneshotPlayedTimestamp = Time.timeAsDouble;
        RuntimeManager.PlayOneShotAttached(_sound, this.gameObject);
    }

    public void PlayOneShot(float minimumTimeSinceLastPlay)
    {
        if ((Time.timeAsDouble - _lastOneshotPlayedTimestamp) < minimumTimeSinceLastPlay)
            return;
        PlayOneShot();
    }
    #endregion



    [System.Serializable]
    public struct AdvancedProperties
    {
        [SerializeField] public FMOD.Studio.STOP_MODE DefaultStopMode;
        [SerializeField] public SerializableDictionary<string, float> PropertiesOnStartup;
        [SerializeField] public bool PlayOnStart;
        [SerializeField] public bool InitOnStart;
        [SerializeField] public float InitialVolume;
        [SerializeField] public float VolumeChangeSpeed;
        [SerializeField] public float VolumeTargetGrowth;
        public static AdvancedProperties Default => new AdvancedProperties {
            DefaultStopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT,
            PlayOnStart = false,
            InitOnStart = false,
            InitialVolume = 1f,
            VolumeChangeSpeed = 0f,
            VolumeTargetGrowth = 0f
        };
    }

    [SerializeField] AdvancedProperties _advanced = AdvancedProperties.Default;


    EventInstance _currentInstance = default;

    public void MakeSureIsInitialized()
    {
        if (!_currentInstance.isValid())
        {
            _currentInstance = RuntimeManager.CreateInstance(_sound);
            _currentInstance.set3DAttributes(this.gameObject.To3DAttributes());
            foreach(var (name, val) in _advanced.PropertiesOnStartup.Values)
            {
                _currentInstance.setParameterByName(name, val);
            }
        }
    }


    Coroutine _volumeUpdater = null;
    float _volumeTarget = 1f;
    [SerializeField] float _currentVolume = 1f;
    public void StartPlaying()
    {
        MakeSureIsInitialized();
        if (_currentInstance.isPlaying())
        {
            Debug.LogWarning($"{_sound} is already playing!", this);
            return;
        }
        _currentInstance.setTimelinePosition(0);
        _currentInstance.setVolume(_currentVolume = _advanced.InitialVolume);
        _currentInstance.start();

        if(_advanced.VolumeChangeSpeed != 0.0f && _volumeUpdater.IsNil())
            _volumeUpdater = StartCoroutine(volumeUpdater());
        IEnumerator volumeUpdater()
        {
            while (true)
            {
                yield return null;
                if (!_currentInstance.isPlaying())
                {
                    _volumeUpdater = null;
                    yield break;
                }
                _volumeTarget = (_volumeTarget + _advanced.VolumeTargetGrowth * Time.deltaTime).Clamp01();
                var volumeDirection = _volumeTarget - _currentVolume;
                var volumeDelta = volumeDirection.Clamp(-_advanced.VolumeChangeSpeed, +_advanced.VolumeChangeSpeed) * Time.deltaTime;
                _currentInstance.setVolume(_currentVolume += volumeDelta);
            }
        }
    }

    public void Stop(FMOD.Studio.STOP_MODE stopMode)
    {
        if (!_currentInstance.isValid()) return;

        _currentInstance.stop(stopMode);
    }
    public void Stop() => Stop(_advanced.DefaultStopMode);

    public void SetPaused(bool pausedState)
    {
        MakeSureIsInitialized();
        _currentInstance.setPaused(pausedState);
    }
    public void SetVolume(float volume)
    {
        MakeSureIsInitialized();
        _currentInstance.setVolume(_currentVolume = volume);
    }

    public void SetVolumeTarget(float targetVolume)
    {
        _volumeTarget = targetVolume;
    }

    public void SetParameter(string name, float value, ParameterSettingMode mode = ParameterSettingMode.Always)
    {
        MakeSureIsInitialized();
        if(mode != ParameterSettingMode.Always)
        {
            if(_currentInstance.getParameterByName(name, out float currentValue) != FMOD.RESULT.OK)
            {
                if (mode switch
                {
                    ParameterSettingMode.DecreaseOnly => value >= currentValue,
                    ParameterSettingMode.IncreaseOnly => value <= currentValue
                })
                {
                    Debug.LogWarning($"Skipped {name} = {value}", this);
                    return;
                }
            }
        }
        _currentInstance.setParameterByName(name, value);
    }

    public void SetGlobalParameter(string name, float value, ParameterSettingMode mode = ParameterSettingMode.Always)
    {
        if (mode != ParameterSettingMode.Always)
        {
            if (RuntimeManager.StudioSystem.getParameterByName(name, out float currentValue) != FMOD.RESULT.OK)
            {
                if (mode switch
                {
                    ParameterSettingMode.DecreaseOnly => value >= currentValue,
                    ParameterSettingMode.IncreaseOnly => value <= currentValue
                })
                {
                    Debug.LogWarning($"Skipped {name} = {value}", this);
                    return;
                }
            }
        }
        Debug.LogWarning($"Setting {name} = {value}");
        RuntimeManager.StudioSystem.setParameterByName(name, value);
    }


    double _currentRenewEnd = float.NegativeInfinity;
    Coroutine _renewer = null;
    public void PlayRenewed(float renewWindow)
    {
        MakeSureIsInitialized();
        _currentRenewEnd = Time.timeAsDouble + renewWindow;
        if (_renewer.IsNil())
        {
            StartPlaying();
            _renewer = StartCoroutine(renewer());
        }

        IEnumerator renewer()
        {
            Debug.Log($"Starting renewer", this);
            while (true)
            {
                if(Time.timeAsDouble > _currentRenewEnd)
                {
                    Stop();
                    _renewer = null;
                    Debug.Log($"Exiting renewer", this);
                    yield break;
                }
                yield return null;
            }
        }
    }

    [System.Serializable]
    public enum ParameterSettingMode
    {
        Always,
        IncreaseOnly,
        DecreaseOnly
    }

    bool _tryParseParameterCommand(string nameAndValue, out string name, out float value, out ParameterSettingMode mode,[System.Runtime.CompilerServices.CallerMemberName] string callerName = "<dummy>")
    {
        mode = ParameterSettingMode.Always;
        Debug.Log($"{callerName}({nameAndValue})", this);
        var ret = nameAndValue.Split(" ");
        if (ret.Length < 2 || !float.TryParse(ret[ret.Length-1], out value))
        {
            Debug.LogError($"Invalid request for {callerName}(): '{nameAndValue}'");
            (name, value) = (null, 0);
            return false;
        }
        name = ret[0];
        if (ret.Length == 3) mode = ret[1] switch
        {
            "+" => ParameterSettingMode.IncreaseOnly,
            "-" => ParameterSettingMode.DecreaseOnly,
            _ => ParameterSettingMode.Always
        };
        return true;
    }
    // Only way to be able to do this stuff from UnityEvents in the editor
    public void SetParameter(string nameAndValue)
    {
        if(_tryParseParameterCommand(nameAndValue, out string name, out float value, out var mode))
        SetParameter(name, value, mode);
    }
    public void SetGlobalParameter(string nameAndValue)
    {
        if(_tryParseParameterCommand(nameAndValue, out string name, out float value, out var mode))
        SetGlobalParameter(name, value, mode);
    }




    private void Start()
    {
        if (_advanced.InitOnStart) MakeSureIsInitialized();
        if (_advanced.PlayOnStart) StartPlaying();
    }
    private void OnDestroy() => Stop();
}


public static class FmodExtensions
{
    public static bool isPlaying(this in EventInstance self)
    {
        self.getPlaybackState(out var state);
        return state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.PLAYING;
    }
}