using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Game.UI
{
    public class GameSettings : MonoBehaviour
    {
        [SerializeField]
        private AudioMixer mixer;
        [SerializeField]
        private Slider masterSlider;
        [SerializeField]
        private Slider bgmSlider;
        [SerializeField]
        private Slider sfxSlider;
        [SerializeField]
        private TMP_Dropdown resolutionDropdown;
        [SerializeField]
        private TMP_Dropdown screenModeDropdown;
        [SerializeField]
        private TMP_Dropdown languageDropdown;

        // 배열은 const 가 안 된다 — 컴파일 시점 상수가 아니다.
        // 드롭다운 항목 순서와 반드시 일치해야 하고, 어긋나도 컴파일은 된다.
        // 그래서 Verify 에서 개수라도 비교한다.
        private static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(1920, 1080),
            new Vector2Int(1600, 900),
        };

        private static readonly string[] LocaleCodes = { "ko", "en" };

        private const string KeyMaster = "master";
        private const string KeyBgm = "bgm";
        private const string KeySfx = "sfx";
        private const string KeyResolution = "resolution";
        private const string KeyFullscreen = "fullscreen";

        private const string ParamMaster = "MasterVolume";
        private const string ParamBgm = "BgmVolume";
        private const string ParamSfx = "SfxVolume";

        // 0 을 그대로 dB 로 바꾸면 -Infinity 다. 하한을 자른다.
        private const float MinDb = -80f;
        private const float MinVolume = 0.0001f;

        // 저장은 0~1 선형으로 한다. dB 를 저장하면 슬라이더 위치를 역산해야 한다 —
        // WeaponInstance 가 배율 대신 레벨을 저장한 것과 같은 이유다.
        private float master = 1f;
        private float bgm = 0.8f;
        private float sfx = 1f;
        private int resolutionIndex;
        private bool fullscreen;

        private void Awake()
        {
            Verify();
            Load();

            // 화면과 소리는 기다릴 것이 없다. 전부 Start 로 미루면
            // 첫 프레임에 기본 해상도로 떴다가 바뀌는 것이 보인다.
            ApplyScreen();
            ApplyAudio();
            PushScreenToUI();
            PushAudioToUI();
            RegisterScreenAndAudio();
        }

        // 언어만 로케일 초기화를 기다린다. AvailableLocales 가 그 전에는 비어 있어
        // 드롭다운이 엉뚱한 값을 보여준다.
        // UniTask 가 아니라 코루틴인 이유는 오브젝트가 죽으면 알아서 멈추기 때문이다 —
        // #101 에서 UniTask.Delay 가 파괴된 오브젝트를 건드려 터진 적이 있다.
        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;

            if (languageDropdown == null)
                yield break;

            PushLanguageToUI();
            languageDropdown.onValueChanged.AddListener(SelectLocale);
        }

        // 항목마다 독립이라 하나가 없다고 컴포넌트를 끄지 않는다.
        // RewardView 는 panel 이 없으면 화면 전체가 안 뜨므로 껐지만,
        // 여기는 믹서가 없어도 해상도는 동작한다.
        private void Verify()
        {
            if (mixer == null)
                Debug.LogWarning($"{gameObject.name}: mixer 미지정 — 볼륨 슬라이더가 아무 일도 안 한다", this);

            if (masterSlider == null || bgmSlider == null || sfxSlider == null)
                Debug.LogWarning($"{gameObject.name}: 볼륨 슬라이더가 비어 있다 — 그 항목은 안 보이고 안 바뀐다", this);

            if (resolutionDropdown == null || screenModeDropdown == null || languageDropdown == null)
                Debug.LogWarning($"{gameObject.name}: 드롭다운이 비어 있다 — 그 항목은 안 보이고 안 바뀐다", this);

            // 개수가 같아도 순서가 어긋날 수 있다. 그건 코드로 못 잡으니 절반만 막는다.
            if (resolutionDropdown != null && resolutionDropdown.options.Count != Resolutions.Length)
                Debug.LogWarning($"{gameObject.name}: 해상도 항목 {resolutionDropdown.options.Count}개, 코드 표 {Resolutions.Length}개 — 고른 것과 적용되는 것이 다를 수 있다", this);

            if (languageDropdown != null && languageDropdown.options.Count != LocaleCodes.Length)
                Debug.LogWarning($"{gameObject.name}: 언어 항목 {languageDropdown.options.Count}개, 코드 표 {LocaleCodes.Length}개 — 고른 것과 적용되는 것이 다를 수 있다", this);
        }

        // 기본값은 코드가 갖는다. 첫 실행에는 키가 없다.
        // 클램프하는 이유는 PlayerPrefs 에 무엇이든 들어갈 수 있고,
        // 나중에 해상도를 지우면 저장된 인덱스가 범위를 넘기 때문이다.
        private void Load()
        {
            master = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMaster, 1f));
            bgm = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyBgm, 0.8f));
            sfx = Mathf.Clamp01(PlayerPrefs.GetFloat(KeySfx, 1f));
            resolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyResolution, 0), 0, Resolutions.Length - 1);
            fullscreen = PlayerPrefs.GetInt(KeyFullscreen, 0) != 0;
        }

        // 자동 저장은 정상 종료 때만 돈다. 설정은 자주 안 바뀌므로 즉시 쓴다.
        private void Save()
        {
            PlayerPrefs.SetFloat(KeyMaster, master);
            PlayerPrefs.SetFloat(KeyBgm, bgm);
            PlayerPrefs.SetFloat(KeySfx, sfx);
            PlayerPrefs.SetInt(KeyResolution, resolutionIndex);
            PlayerPrefs.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        // 해상도와 화면 모드를 한 호출로 묶는다. 그러면 전체화면에서 창으로 돌아올 때
        // 저장된 해상도가 공짜로 따라온다. 따로 두면 그 경우를 위한 코드가 또 생긴다.
        // ExclusiveFullScreen 이 아니라 FullScreenWindow 를 쓴다 — Alt-Tab 이 부드럽다.
        // 에디터에서는 Game 뷰 드롭다운이 이겨서 아무 일도 안 일어난다. 빌드로만 검증된다.
        private void ApplyScreen()
        {
            Vector2Int r = Resolutions[resolutionIndex];
            Screen.SetResolution(r.x, r.y, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        private void ApplyAudio()
        {
            if (mixer == null)
                return;

            SetDb(ParamMaster, master);
            SetDb(ParamBgm, bgm);
            SetDb(ParamSfx, sfx);
        }

        // SetFloat 은 이름이 틀리면 예외가 아니라 false 를 돌려준다.
        // 반환값을 안 보면 "슬라이더가 안 먹는다" 로 시간을 쓴다.
        private void SetDb(string parameter, float value)
        {
            float db = value <= MinVolume ? MinDb : Mathf.Log10(value) * 20f;

            if (!mixer.SetFloat(parameter, db))
                Debug.LogWarning($"{gameObject.name}: 믹서에 '{parameter}' 가 없다 — Exposed Parameters 이름을 확인해라", this);
        }

        // value 를 그냥 대입하면 onValueChanged 가 발동해 시작하자마자 Save 가 돈다.
        private void PushScreenToUI()
        {
            resolutionDropdown?.SetValueWithoutNotify(resolutionIndex);
            screenModeDropdown?.SetValueWithoutNotify(fullscreen ? 1 : 0);
        }

        private void PushAudioToUI()
        {
            masterSlider?.SetValueWithoutNotify(master);
            bgmSlider?.SetValueWithoutNotify(bgm);
            sfxSlider?.SetValueWithoutNotify(sfx);
        }

        private void PushLanguageToUI()
        {
            string code = LocalizationSettings.SelectedLocale != null
                ? LocalizationSettings.SelectedLocale.Identifier.Code
                : null;

            int index = 0;
            for (int i = 0; i < LocaleCodes.Length; i++)
            {
                if (LocaleCodes[i] == code)
                {
                    index = i;
                    break;
                }
            }

            languageDropdown.SetValueWithoutNotify(index);
        }

        // 인스펙터가 아니라 코드로 단다. RewardView 가 이미 그렇고,
        // 인스펙터로 하면 배선이 여섯 개 늘어 실수할 자리가 는다.
        private void RegisterScreenAndAudio()
        {
            if (masterSlider != null)
                masterSlider.onValueChanged.AddListener(v => { master = v; ApplyAudio(); Save(); });
            if (bgmSlider != null)
                bgmSlider.onValueChanged.AddListener(v => { bgm = v; ApplyAudio(); Save(); });
            if (sfxSlider != null)
                sfxSlider.onValueChanged.AddListener(v => { sfx = v; ApplyAudio(); Save(); });

            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.AddListener(i =>
                {
                    resolutionIndex = Mathf.Clamp(i, 0, Resolutions.Length - 1);
                    ApplyScreen();
                    Save();
                });

            if (screenModeDropdown != null)
                screenModeDropdown.onValueChanged.AddListener(i =>
                {
                    fullscreen = i == 1;
                    ApplyScreen();
                    Save();
                });
        }

        // 인덱스가 아니라 코드로 찾는다. AvailableLocales 의 순서는 보장이 없어서,
        // 인덱스로 매핑하면 순서가 바뀔 때 조용히 뒤바뀐다.
        // Save 를 안 부르는 이유는 PlayerPrefLocaleSelector 가
        // SelectedLocaleChanged 를 구독해 selected-locale 키에 알아서 기록하기 때문이다.
        private void SelectLocale(int index)
        {
            if (index < 0 || index >= LocaleCodes.Length)
                return;

            var locale = LocalizationSettings.AvailableLocales.GetLocale(LocaleCodes[index]);
            if (locale == null)
            {
                Debug.LogWarning($"{gameObject.name}: '{LocaleCodes[index]}' 로케일이 없다 — 로케일 목록을 확인해라", this);
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
        }
    }
}