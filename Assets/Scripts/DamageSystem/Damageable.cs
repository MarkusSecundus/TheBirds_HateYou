using MarkusSecundus.Utils.Primitives;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class Damageable : MonoBehaviour
{
    public static Damageable Get(Component o) => o.GetComponentInParent<Damageable>();

    [field: SerializeField] public float MaxHP { get; private set; } = 100;
    public const float MinHP = 0;

    [field: SerializeField]public float HP { get; private set; }

    [SerializeField] public UnityEvent<HealthChangeInfo> OnUpdate;
    [SerializeField] public UnityEvent<HealthChangeInfo> OnDamaged;
    [SerializeField] public UnityEvent<HealthChangeInfo> OnHealed;
    [SerializeField] public UnityEvent<HealthChangeInfo> OnDeath;
    [SerializeField] public UnityEvent OnDestroyed;

    private void OnDestroy() => OnDestroyed?.Invoke();

    private void Awake()
    {
        if(MaxHP <= MinHP)
        {
            Debug.LogError($"MaxHP must be greater than {MinHP} but is {MaxHP}", this);
            MaxHP = MinHP + 1f;
        }
        if (HP <= MinHP || HP > MaxHP) ResetHealth();
            
    }
    private void Start()
    {
        ForceUpdate();
    }

    public bool IsDead => HP <= MinHP;

    [System.Serializable] public struct HealthChangeInfo
    {
        public static HealthChangeInfo Compute(Damageable damageable, float delta, bool isForcedUpdate = false) => new (){ Damageable = damageable, OriginalHP = damageable.HP, RequestedDeltaHP = delta, DidDie = !damageable.IsDead && (damageable.HP + delta) <= MinHP, IsForcedUpdate = isForcedUpdate };
        public Damageable Damageable { get; init; }
        public float OriginalHP { get; init; }
        public float RequestedDeltaHP { get; init; }
        public float ActualDeltaHP => ResultHP - OriginalHP;
        public float ResultHP => Mathf.Clamp(OriginalHP + RequestedDeltaHP, Damageable.MinHP, Damageable.MaxHP);
        public bool DidDie { get; init; }
        public bool IsForcedUpdate { get; init; }
    }

    public void ForceUpdate()
    {
        OnUpdate?.Invoke(HealthChangeInfo.Compute(this, 0f, true));
    }

    public void ResetHealth()
    {
        HP = MaxHP;
    }

    public bool Heal(float amount) => ChangeHP(amount, OnHealed);
    public bool Damage(float amount) => ChangeHP(-amount, OnDamaged);

    private bool ChangeHP(float amount, UnityEvent<HealthChangeInfo> @event)
    {
        if (IsDead)
        {
            Debug.Log($"Ignoring health change for already dead entity '{name}'", this);
            return false;
        }

        var info = HealthChangeInfo.Compute(this, amount);
        this.HP = info.ResultHP;
        //Debug.Log($"{name} damaged for {amount}({info.ActualDeltaHP}) HP -> now has {this.HP} HP (originally had {info.OriginalHP})");

        OnUpdate?.Invoke(info);
        @event?.Invoke(info);
        if (info.DidDie)
        {
            OnDeath?.Invoke(info);
        }
        return !info.ActualDeltaHP.IsNegligible();
    }
}
