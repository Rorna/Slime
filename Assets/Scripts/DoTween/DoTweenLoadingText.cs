using DG.Tweening;
using TMPro;
using UnityEngine;

public class DoTweenLoadingText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_textMeshPro;

    private DOTweenTMPAnimator m_textMeshProAnim;
    private Sequence m_sequence;

    private void Start()
    {
        m_textMeshPro.DOFade(0, 0);
        StartAnimation();
    }

    private void StartAnimation()
    {
        if (m_textMeshProAnim == null)
            m_textMeshProAnim = new DOTweenTMPAnimator(m_textMeshPro);

        m_sequence = DOTween.Sequence();
        for (int i = 0; i < m_textMeshPro.textInfo.characterCount; ++i)
        {
            Vector3 currCharOffset = m_textMeshProAnim.GetCharOffset(i);
            Sequence charSequence = DOTween.Sequence();
            charSequence.Append(m_textMeshProAnim.DOOffsetChar(i, currCharOffset + new Vector3(0, 30, 0), 0.4f)
                    .SetEase(Ease.OutFlash, 2))
                .Join(m_textMeshProAnim.DOFadeChar(i, 1, 0.4f))
                .Join(m_textMeshProAnim.DOScaleChar(i, 1, 0.4f).SetEase(Ease.OutBack))
                .Append(m_textMeshProAnim.DOOffsetChar(i, currCharOffset, 0.4f).SetEase(Ease.InFlash));

            m_sequence.Insert(0.07f * i, charSequence);
        }

        m_sequence.OnComplete(() =>
        {
            m_textMeshPro.DOFade(0, 0).OnComplete(StartAnimation);
        });
    }
}