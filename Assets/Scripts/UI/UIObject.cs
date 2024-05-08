using DG.Tweening;
using UnityEngine;

public class UIObject : MonoBehaviour
{
    [SerializeField] private CanvasGroup m_canvasGroup;
    [SerializeField] private float m_fadeTime;

    protected virtual void OnAwake()
    { }

    public virtual void OnShow()
    { }

    public virtual void OnHide()
    { }

    private void Awake()
    {
        OnAwake();
    }

    public bool IsVisible()
    {
        return gameObject.activeSelf;
    }

    public void Show()
    {
        if (IsVisible())
            return;

        gameObject.SetActive(true);
        OnShow();
    }

    public void Hide()
    {
        if (isActiveAndEnabled == false)
            return;

        gameObject.SetActive(false);
        OnHide();
    }

    public virtual void OnBackButtonClick()
    {
        Hide();
    }

    public void FadeIn()
    {
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.DOFade(1, m_fadeTime);
    }

    public void FadeOut()
    {
        m_canvasGroup.alpha = 1f;
        m_canvasGroup.DOFade(0, m_fadeTime);
    }

    public void FadeInShow()
    {
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.DOFade(1, m_fadeTime).OnComplete(() =>
        {
            Show();
        });
    }

    public void FadeOutHide()
    {
        m_canvasGroup.alpha = 1f;
        m_canvasGroup.DOFade(0, m_fadeTime).OnComplete(() =>
        {
            Hide();
            m_canvasGroup.alpha = 1f;
        });
    }
}