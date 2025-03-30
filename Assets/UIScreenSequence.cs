using DG.Tweening;
using MarkusSecundus.Utils.Behaviors.Cosmetics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIScreenSequence : MonoBehaviour
{
    [SerializeField] Image Blackscreen;
    [SerializeField] List<UIScreen> screens;
    int screenIndex = 0;

    public bool IsIntro;
    public bool IsOutro;

    [SerializeField] float screenBlinkDuration = 0.1f;

    [SerializeField] UnityEvent OnSequenceFinished;

    private void Start()
    {
        foreach (var screen in screens)
        {
            screen.gameObject.SetActive(false);
            screen.screenSequence = this;
        }

        if (IsIntro)
        {
            Blackscreen.gameObject.SetActive(true);
            Blackscreen.color = Color.black;
            //Blackscreen.GetComponent<FadeEffect>().FadeIn();
            DOVirtual.DelayedCall(1f, () => StartSequence());
        }
    }

    public void StartSequence()
    {
        screenIndex = 0;
        Blackscreen.gameObject.SetActive(true);
        Blackscreen.color = Color.black;
        screens.First().ShowScreen();
    }

    public void CurrentScreenEnded()
    {
        DOVirtual.DelayedCall(0.1f, () => ShowNext());
    }

    public void ShowNext()
    {
        screenIndex++;
        if(screenIndex < screens.Count)
        {
            screens[screenIndex].ShowScreen();
        }
        else
        {
            EndSequence();
        }
    }

    void EndSequence()
    {
        Blackscreen.GetComponent<FadeEffect>().FadeOut();
        if (IsOutro)
        {
            DOVirtual.DelayedCall(1f, () => Application.Quit());
        }
        OnSequenceFinished?.Invoke();
    }


}
