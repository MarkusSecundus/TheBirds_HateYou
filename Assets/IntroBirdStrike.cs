using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;



#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(IntroBirdStrike))]
public class IntroBridEffect : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Hit Effect"))
        {
            ((IntroBirdStrike)target).PlayEffect();
        }
    }
}
#endif


public class IntroBirdStrike : MonoBehaviour
{
    public RectTransform Bird;
    public Image BirdImage;
    public RectTransform BirdEndPos;
    public RectTransform BirdStartPos;
    public Image WhiteFlashImage;

    public float EffectDuration;
    public float ImpactTime;
    public float ImpactDuration;       

    public void PlayEffect()
    {
        BirdImage.color = Color.white;
        Bird.localPosition = BirdStartPos.localPosition;
        Bird.DOLocalMove(BirdEndPos.localPosition, EffectDuration).SetEase(Ease.Linear);
        DOVirtual.DelayedCall(ImpactTime, () => Impact() );
    }

    void Impact()
    {
        WhiteFlashImage.color = Color.white;
        BirdImage.color = Color.black;
        DOVirtual.DelayedCall(ImpactDuration, () =>
        {
            WhiteFlashImage.color = Color.clear;
            BirdImage.color = Color.white;
        });
    }
}
