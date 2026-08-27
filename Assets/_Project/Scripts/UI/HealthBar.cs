using Game.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField]
        Health source;
        [SerializeField]
        Image fill;

        private void Start()
        {
            Redraw();
        }


        private void Redraw()
        {
            if (source == null || fill == null)
                return;
            if (source.MaxHealth <= 0)
                return;
            fill.fillAmount = (float)source.CurrentHealth / source.MaxHealth;
        }

        private void OnEnable()
        {
            if (source != null)
            {
                source.Changed += Redraw;
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: source 미지정 — 체력이 화면에 안 나온다", this);
            }
        }

        private void OnDisable()
        {
            if (source != null)
            {
                source.Changed -= Redraw;
            }
        }
    }
}
