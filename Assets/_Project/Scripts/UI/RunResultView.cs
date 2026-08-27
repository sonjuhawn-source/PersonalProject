using Game.Gameplay.Rooms;
using TMPro;
using UnityEngine;

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

        private void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning($"{gameObject.name}: 런 결과 화면이 안 뜬다", this);
                return;
            }
            panel.SetActive(false);
        }

        private void Show(string text)
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
            label.text = text;
            panel.SetActive(true);
        }

        private void OnEnable()
        {
            if (source != null)
            {
                source.RunEnded += Show;
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 런 결과가 화면에 안 나온다", this);
            }
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
