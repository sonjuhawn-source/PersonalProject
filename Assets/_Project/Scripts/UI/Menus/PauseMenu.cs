using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject panel;

        private bool paused;

        private void Awake()
        {
            if (panel == null)
            {
                Debug.LogWarning($"{gameObject.name}: panel 미지정 — ESC 를 눌러도 설정이 안 열린다", this);
                enabled = false;
                return;
            }

            panel.SetActive(false);
        }

        private void Update()
        {
            // HitStop 이 finally 에서 timeScale 을 1 로 되돌린다. internal static 이라 못 막으니 매 프레임 다시 주장한다.
            if (paused)
                Time.timeScale = 0f;

            if (Keyboard.current == null)
                return;

            // .inputactions 를 안 건드리는 건 생성 클래스 재생성이 따라와서다 — StageRunner 와 같은 판단.
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Toggle();
        }

        // 버튼은 토글이 아니다. "열려 있는데 paused 가 false" 에서 Toggle 은 오히려 연다. #165
        public void Open()
        {
            paused = true;
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Close()
        {
            paused = false;
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        // ESC 는 둘 중 무엇인지 모르므로 여기서만 뒤집는다.
        public void Toggle()
        {
            if (paused)
                Close();
            else
                Open();
        }

        // 빌드엔 도메인 리로드가 없어 timeScale 이 씬을 넘어 살아남는다. #101
        private void OnDestroy()
        {
            if (paused)
                Time.timeScale = 1f;
        }
    }
}