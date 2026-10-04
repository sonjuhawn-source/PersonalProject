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

        // 여는 버튼(시작 화면)과 닫는 버튼(설정 패널)이 각각 Open · Close 를 부른다.
        // 토글을 걸면 안 되는 이유는 버튼이 토글이 아니기 때문이다 — 닫기 버튼은
        // 눌렀을 때 항상 닫혀야 하는데, Toggle 은 "열려 있는데 paused 가 false" 인
        // 상태에서 오히려 연다. 시작 화면이 실제로 그 상태를 만들고 있었다 (#165).
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

        // 빌드에는 도메인 리로드가 없어 timeScale 이 씬을 넘어 살아남는다 (#101).
        // 멈춘 채로 재시작하거나 타이틀로 가면 다음 씬이 멈춘 채 뜬다.
        private void OnDestroy()
        {
            if (paused)
                Time.timeScale = 1f;
        }
    }
}