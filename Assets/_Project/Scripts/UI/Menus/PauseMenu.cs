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
            // 멈춰 있는 동안은 이 컴포넌트가 timeScale 의 주인이다.
            // HitStop 이 언스케일로 돌다가 finally 에서 1 로 되돌리는데,
            // internal static 이라 Game.UI 에서 손댈 수 없다. 매 프레임 다시 주장한다.
            // Update 는 timeScale 과 무관하게 돌아서 확실하다.
            if (paused)
                Time.timeScale = 0f;

            if (Keyboard.current == null)
                return;

            // .inputactions 를 건드리지 않는 이유는 자산 저장과 생성 클래스 재생성이
            // 따라오기 때문이다 — StageRunner 의 R · 숫자키 폴링과 같은 판단이다.
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                Toggle();
        }

        // 설정 패널의 닫기 버튼도 이것을 부르면 상태가 한 곳에서만 바뀐다.
        public void Toggle()
        {
            paused = !paused;
            panel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
        }

        // 빌드에는 도메인 리로드가 없어 timeScale 이 씬을 넘어 살아남는다 (#101).
        // 멈춘 채로 재시작하거나 타이틀로 가면 다음 씬이 멈춘 채 뜬다.
        private void OnDestroy()
        {
            if (paused)
                Time.timeScale = 1f;
        }
    }
}