using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHUD : UIObject
{
    [Header("Cooldown")]
    [SerializeField] private TextMeshProUGUI m_cooldownText;

    [SerializeField] private Image m_dodgeCooldownImage;

    [Header("equipItem")]
    [SerializeField] private Image m_nothingImage;

    [SerializeField] private Image m_equipItemIconImage;

    [Header("floating Text")]
    [SerializeField] private TextMeshProUGUI m_floatingText;

    [SerializeField] private float m_floatingDuringTime;

    [Header("light state")]
    [SerializeField] private GameObject m_dodgeReinforced;

    [SerializeField] private GameObject m_weaponReinforced;

    [Header("Cooldown")]
    [SerializeField] private TextMeshProUGUI m_interactText;

    private FieldObject m_player;

    public void Init(FieldObject player)
    {
        if (player.IsPlayer == false)
            return;

        m_player = player;

        InitUIObjects();
    }

    private void InitUIObjects()
    {
        m_dodgeCooldownImage.fillAmount = 0f;
        m_floatingText.gameObject.SetActive(false);
        m_dodgeCooldownImage.gameObject.SetActive(true);
        m_nothingImage.gameObject.SetActive(true);
        m_equipItemIconImage.gameObject.SetActive(false);
        m_cooldownText.gameObject.SetActive(false);
        m_dodgeReinforced.SetActive(false);
        m_weaponReinforced.SetActive(false);
    }

    private void Update()
    {
        if (m_player.GO.IsNull())
            return;

        UpdateDodgeState();
        UpdateHasWeaponState();
        UpdateLightState();
        UpdateInteract();
        UpdateSlimeItem();
    }

    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }

    public void ShowFloatingText(string text)
    {
        if (m_floatingText.isActiveAndEnabled)
            return;

        var originalPos = m_floatingText.transform.position;
        var currentPos = m_floatingText.transform.position;
        float targetPos = currentPos.y;

        m_floatingText.text = text;
        m_floatingText.gameObject.SetActive(true);

        m_floatingText.transform.DOMoveY(targetPos, m_floatingDuringTime)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                m_floatingText.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f)
                    .SetDelay(0.7f)
                    .OnComplete(() =>
                    {
                        m_floatingText.gameObject.SetActive(false);
                        m_floatingText.transform.position = originalPos;
                        m_floatingText.transform.localScale = Vector3.one;
                    });
            });
    }

    private void UpdateDodgeState()
    {
        var cmd = m_player.CommandManager.GetCmd<FieldObjectDodgeCommand>((int)FieldObjectCmdType.FieldObjectDodgeTimer);
        if (cmd.IsNotNull())
        {
            if (m_cooldownText.isActiveAndEnabled == false)
                m_cooldownText.gameObject.SetActive(true);

            float remainTime = cmd.RemainTime;

            m_cooldownText.text = remainTime.ToString("F1");
            float fillAmount = (remainTime / cmd.LifeTime);
            m_dodgeCooldownImage.fillAmount = fillAmount;
        }
        else
        {
            if (m_cooldownText.isActiveAndEnabled)
                m_cooldownText.gameObject.SetActive(false);
        }
    }

    private void UpdateHasWeaponState()
    {
        if (m_player.Equip.HasItem)
        {
            if (m_equipItemIconImage.isActiveAndEnabled == false)
                m_equipItemIconImage.gameObject.SetActive(true);

            if (m_nothingImage.isActiveAndEnabled)
                m_nothingImage.gameObject.SetActive(false);

            var image = m_player.Equip.CurrentEquipItem;
            var sprite = image.Body.GetComponent<SpriteRenderer>();
            m_equipItemIconImage.sprite = sprite.sprite;
        }
        else
        {
            if (m_equipItemIconImage.isActiveAndEnabled)
                m_equipItemIconImage.gameObject.SetActive(false);

            if (m_nothingImage.isActiveAndEnabled == false)
                m_nothingImage.gameObject.SetActive(true);
        }
    }

    private void UpdateLightState()
    {
        switch (m_player.LightAreaState)
        {
            case LightAreaStateEnum.Normal:
                {
                    if (m_dodgeReinforced.activeSelf)
                    {
                        m_dodgeReinforced.SetActive(false);
                        SoundManager.Instance.PlayEffectAudio("OutLightHUD", volume: 0.5f);
                    }
                }
                break;

            case LightAreaStateEnum.Light:
                {
                    if (m_dodgeReinforced.activeSelf == false)
                    {
                        m_dodgeReinforced.SetActive(true);
                        SoundManager.Instance.PlayEffectAudio("CanSuperDodge", volume: 0.5f);
                    }
                }
                break;
        }
    }

    private void UpdateInteract()
    {
        if (UIManager.Instance.IsVisibleUI<UIDialog>())
        {
            m_interactText.gameObject.SetActive(false);
        }
        else
        {
            float dist = (m_player.GetBoxCollider().size.x * Mathf.Sqrt(2)) * 0.5f;
            if (m_player.HasContactObject(out GameObject go, m_player.Move.MoveDir, dist))
            {
                //interactive
                if (go.transform.parent.tag != DefineStrings.Interactive && go.transform.parent.tag != DefineStrings.Item)
                    return;

                if (m_interactText.isActiveAndEnabled == false)
                {
                    switch (go.transform.parent.tag)
                    {
                        case DefineStrings.Interactive:
                            m_interactText.text = "Interact(F)";
                            break;

                        case DefineStrings.Item:
                            m_interactText.text = "Equip";
                            break;
                    }

                    m_interactText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (m_interactText.isActiveAndEnabled == true)
                    m_interactText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSlimeItem()
    {
        if (m_player.Equip.HasItem == false)
        {
            if (m_weaponReinforced.activeSelf)
                m_weaponReinforced.SetActive(false);

            return;
        }

        var item = m_player.Equip.CurrentEquipItem;
        if (item.IsNull())
        {
            if (m_weaponReinforced.activeSelf)
                m_weaponReinforced.SetActive(false);

            return;
        }

        if (item.Item.ItemType == ItemTypeEnum.Slime)
        {
            if (m_weaponReinforced.activeSelf == false)
            {
                m_weaponReinforced.SetActive(true);
                SoundManager.Instance.PlayEffectAudio("CanRanged", volume: 0.5f);
            }
        }
        else
        {
            if (m_weaponReinforced.activeSelf)
            {
                m_weaponReinforced.SetActive(false);
                SoundManager.Instance.PlayEffectAudio("CantRanged", volume: 0.5f);
            }
        }
    }
}