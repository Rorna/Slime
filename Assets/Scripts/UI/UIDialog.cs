using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDialog : UIObject
{
    [SerializeField] private TextMeshProUGUI m_dialogText;
    private List<string> m_dialogs;
    private int m_index = 0;

    public void Init(List<string> dialogs)
    {
        if (dialogs.Count == 0)
            return;

        m_dialogs = new List<string>(dialogs);
    }

    public override void OnShow()
    {
        ShowMessage();
        CameraManager.Instance.ZoomIn();
    }

    public override void OnHide()
    {
        CameraManager.Instance.ZoomOut();
    }

    public void ShowMessage()
    {
        SoundManager.Instance.PlayEffectAudio("Dialog", volume: 0.7f);
        if (DOTween.IsTweening(m_dialogText))
        {
            int index = m_index == 0 ? 0 : m_index - 1;
            m_dialogText.text = m_dialogs[index];
            m_dialogText.DOKill();
            return;
        }

        if (m_index > m_dialogs.Count - 1)
        {
            m_dialogs.Clear();
            m_index = 0;
            PlayerInputController.Instance.UpdateInputLock(false);
            Hide();
            return;
        }

        m_dialogText.text = "";
        m_dialogText.DOText(m_dialogs[m_index], m_dialogs[m_index].Length * 0.1f);
        m_index++;
    }
}