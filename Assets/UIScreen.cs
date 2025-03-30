using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScreen : MonoBehaviour
{
    public UIScreenSequence screenSequence;
    [SerializeField] float ShowDuration;

    [SerializeField] TextMeshProUGUI text;
    [SerializeField] float TextAppearDuration = 0.5f;

    public virtual void ShowScreen()
    {
        this.gameObject.SetActive(true);
        DOVirtual.DelayedCall(ShowDuration, () => HideScreen());
        text.color = Color.clear;
        text.DOColor(Color.white, TextAppearDuration);
    }

    public virtual void HideScreen()
    {
        this.gameObject.SetActive(false);
        screenSequence.CurrentScreenEnded();
    }
}
