using Game.Gameplay.Rooms;
using Game.Gameplay.Run;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Game.UI
{
    public class RunResultView : MonoBehaviour
    {
        [SerializeField]
        StageRunner source;
        [SerializeField]
        TextMeshProUGUI label;
        [SerializeField]
        GameObject panel;
        [SerializeField]
        LocalizedString clearedTitle;
        [SerializeField]
        LocalizedString diedTitle;
        [SerializeField]
        LocalizedString bodyFormat;
        [SerializeField]
        LocalizedString weaponFormat;
        [SerializeField]
        Button restartButton;

        private void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning($"{gameObject.name}: panel 미지정 — 런 결과 화면이 안 뜬다", this);
                enabled = false;
                return;
            }
            if (source == null)
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 런 결과가 화면에 안 나온다", this);
                enabled = false;
                return;
            }
            if (restartButton == null)
                Debug.LogWarning($"{gameObject.name}: restartButton 미지정 — 결과 화면에서 다시 시작할 수 없다", this);
            else
                restartButton.onClick.AddListener(source.Restart);

            panel.SetActive(false);
        }

        private void Show(RunResultInfo info)
        {
            if (label == null)
            {
                Debug.LogWarning($"{gameObject.name}: 런 결과 글자가 안 나온다", this);
                return;
            }
            if (panel == null)
            {
                Debug.LogWarning($"{gameObject.name}: 런 결과 화면이 안 뜬다", this);
                return;
            }

            LocalizedString title = info.Outcome == RunOutcome.Cleared ? clearedTitle : diedTitle;
            string text = bodyFormat.GetLocalizedString(info.Floor, info.Kills, info.Health);
            string[] parts = new string[info.Weapons.Length];

            for (int i = 0; i < info.Weapons.Length; i++)
            {
                var w = info.Weapons[i];
                string name = w.Name != null ? w.Name.GetLocalizedString() : "빈 칸";
                parts[i] = weaponFormat.GetLocalizedString(name, w.Level);
            }
            string weapons = string.Join(" · ", parts);

            label.text = title.GetLocalizedString() + "\n" + text + "\n" + weapons;

            panel.SetActive(true);
        }

        private void OnEnable()
        {
            if (source == null)
                return;

            source.RunEnded += Show;
        }

        private void OnDisable()
        {
            if (source != null)
            {
                source.RunEnded -= Show;
            }
        }
    }
}
