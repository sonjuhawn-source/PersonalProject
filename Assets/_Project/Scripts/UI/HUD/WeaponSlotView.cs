using Game.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Game.UI
{
    public class WeaponSlotView : MonoBehaviour
    {
        [SerializeField]
        private WeaponHolder source;
        [SerializeField]
        private TextMeshProUGUI activeLabel;
        [SerializeField]
        private TextMeshProUGUI standbyLabel;
        [SerializeField]
        private Image swapFill;
        [SerializeField]
        private LocalizedString slotFormat;

        private void Awake()
        {
            if(source == null)
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 무기 슬롯이 화면에 안 나온다", this);
                enabled = false;
                return;
            }
            if (swapFill == null)
            {
                Debug.LogWarning($"{gameObject.name}: swapFill 미지정 — 스왑 쿨타임이 안 보인다", this);
            }
        }

        private void OnEnable()
        {
            source.SlotsChanged += RedrawLabels;
        }

        private void OnDisable()
        {
            source.SlotsChanged -= RedrawLabels;
        }

        private void Start()
        {
            RedrawLabels();
        }

        private void Update()
        {
            swapFill.fillAmount = source.StandbyReadyRatio;
        }

        private void RedrawLabels()
        {
            if (activeLabel != null)
            {
                string weaponName = source.ActiveName != null
                    ? source.ActiveName.GetLocalizedString()
                    : "?";
                activeLabel.text = slotFormat.GetLocalizedString(weaponName, source.ActiveUpgradeLevel);
            }

            if (standbyLabel != null)
            {
                string weaponName = source.StandbyName != null
                    ? source.StandbyName.GetLocalizedString()
                    : "?";
                standbyLabel.text = slotFormat.GetLocalizedString(weaponName, source.StandbyUpgradeLevel);
            }
        }
    }
}
