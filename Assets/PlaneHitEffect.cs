using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlaneHitEffect))]
public class PlaneHitEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Hit Effect"))
        {            
            ((PlaneHitEffect)target).PlayHitEffect();
        }
    }
}
#endif

public class PlaneHitEffect : MonoBehaviour
{
    [SerializeField] private Transform TextureObject;
    private Material _planeMat;

    [SerializeField] private float _colorBlinkDur = 0.1f;
    [SerializeField] private int _colorBlinkCount = 3;


    const string _colorPropName = "Color";

    private void Start()
    {
        if(TextureObject == null) return;

        _planeMat = TextureObject.GetComponent<Renderer>().material;
    }

    public void PlayHitEffect()
    {
        if (TextureObject == null) return;
        
        _planeMat.DOColor(Color.clear, _colorBlinkDur).OnComplete(() =>
        {
            _planeMat.DOColor(Color.white, _colorBlinkDur);
        }).SetLoops(_colorBlinkCount, LoopType.Restart);
    }
}
