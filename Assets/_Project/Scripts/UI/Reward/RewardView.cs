using Game.Gameplay.Rooms;
using Game.Gameplay.Run;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Game.UI
{
    public class RewardView : MonoBehaviour
    {
        [SerializeField]
        private StageRunner source;
        [SerializeField]
        private GameObject panel;
        [SerializeField]
        private Button[] buttons;
        [SerializeField]
        private TextMeshProUGUI[] labels;
        [SerializeField]
        private LocalizedString weaponFormat;
        [SerializeField]
        private LocalizedString healFormat;
        [SerializeField]
        private LocalizedString upgradeFormat;
        [SerializeField]
        private Color weaponColor;
        [SerializeField]
        private Color healColor;
        [SerializeField]
        private Color upgradeColor;

        private void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning($"{gameObject.name}: panel 미지정 — 보상 선택 화면이 안 뜬다", this);
                enabled = false;
                return;
            }
            if (source == null)
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 보상이 화면에 안 나온다", this);
                enabled = false;
                return;
            }
            if (buttons == null || labels == null || buttons.Length != labels.Length)
            {
                Debug.LogWarning($"{gameObject.name}: buttons 와 labels 개수가 다르다 — 카드에 엉뚱한 글자가 들어간다", this);
                enabled = false;
                return;
            }
            if (buttons.Length < 3)
                Debug.LogWarning($"{gameObject.name}: 버튼이 {buttons.Length}개다 — 선택지가 3개일 때 뒤가 잘린다", this);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null || labels[i] == null)
                {
                    Debug.LogWarning($"{gameObject.name}: {i}번 칸의 버튼이나 라벨이 비었다 — 그 칸은 눌러도 반응이 없다", this);
                    continue;
                }

                // for 의 i 는 변수 하나라 람다 셋이 같은 것을 본다. 루프가 끝나면 셋 다 Length 를 잡는다.
                int index = i;
                buttons[i].onClick.AddListener(() => Choose(index));
            }

            panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (source == null)
                return;
            source.RewardOffered += Show;
        }

        private void Show(RewardOptionInfo[] offered)
        {
            if (offered == null || offered.Length == 0)
            {
                Debug.LogWarning($"{gameObject.name}: 선택지가 비었다 — 화면을 띄우지 않는다", this);
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null || labels[i] == null)
                    continue;

                // 남는 칸을 끄지 않으면 지난 방의 글자가 그대로 남아 눌러도 안 먹는 카드가 된다.
                bool used = i < offered.Length;
                buttons[i].gameObject.SetActive(used);
                if (!used)
                    continue;

                labels[i].text = Describe(offered[i]);
                labels[i].color = ColorOf(offered[i].Kind);
            }

            panel.SetActive(true);
        }

        private string Describe(RewardOptionInfo info)
        {
            switch (info.Kind)
            {
                case RewardKind.Weapon:
                    // LocalizedString 을 인자로 그냥 넘기면 타입 이름이 찍힌다. 먼저 문자열로 푼다.
                    string gained = info.WeaponName != null ? info.WeaponName.GetLocalizedString() : "?";
                    string dropped = info.DroppedName != null ? info.DroppedName.GetLocalizedString() : "?";
                    return weaponFormat.GetLocalizedString(gained, info.Amount, dropped);

                case RewardKind.Heal:
                    return healFormat.GetLocalizedString(info.Amount);

                case RewardKind.Upgrade:
                    return upgradeFormat.GetLocalizedString(info.Amount);

                default:
                    return info.Kind.ToString();
            }
        }

        private Color ColorOf(RewardKind kind)
        {
            switch (kind)
            {
                case RewardKind.Weapon: return weaponColor;
                case RewardKind.Heal: return healColor;
                case RewardKind.Upgrade: return upgradeColor;
                default: return Color.white;
            }
        }

        private void Choose(int index)
        {
            panel.SetActive (false);
            source.ChooseReward(index);
        }

        private void OnDisable()
        {
            if (source == null)
                return;
            source.RewardOffered -= Show;
        }
    }
}
