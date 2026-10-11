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

        // 드롭다운 항목 순서와 일치해야 하는데 어긋나도 컴파일은 된다. Verify 에서 개수만 비교한다.
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

        // 0~1 선형으로 저장한다. dB 로 저장하면 슬라이더 위치를 역산해야 한다.
        private float master = 1f;
        private float bgm = 0.8f;
        private float sfx = 1f;
        private int resolutionIndex;
        private bool fullscreen;

        private void Awake()
        {
            Verify();
            Load();

            // 전부 Start 로 미루면 첫 프레임에 기본 해상도로 떴다가 바뀌는 것이 보인다.
            ApplyScreen();
            ApplyAudio();
            PushScreenToUI();
            PushAudioToUI();
            RegisterScreenAndAudio();
        }

        // 언어만 로케일 초기화를 기다린다. 코루틴인 건 오브젝트가 죽으면 멈추기 때문. #101
        private IEnumerator Start()
        {
            yield return LocalizationSettings.InitializationOperation;

            if (languageDropdown == null)
                yield break;

            PushLanguageToUI();
            languageDropdown.onValueChanged.AddListener(SelectLocale);
        }

        // 항목마다 독립이라 하나가 없다고 컴포넌트를 끄지 않는다.
        private void Verify()
        {
            if (mixer == null)
                Debug.LogWarning($"{gameObject.name}: mixer 미지정 — 볼륨 슬라이더가 아무 일도 안 한다", this);

            if (masterSlider == null || bgmSlider == null || sfxSlider == null)
                Debug.LogWarning($"{gameObject.name}: 볼륨 슬라이더가 비어 있다 — 그 항목은 안 보이고 안 바뀐다", this);

            // 셋을 묶어 검사하면 어느 것이 빠졌는지 경고가 말해주지 못한다.
            if (resolutionDropdown == null)
                Debug.LogWarning($"{gameObject.name}: 해상도 드롭다운이 비어 있다 — 그 항목은 안 보이고 안 바뀐다", this);

            if (screenModeDropdown == null)
                Debug.LogWarning($"{gameObject.name}: 화면 모드 드롭다운이 비어 있다 — 그 항목은 안 보이고 안 바뀐다", this);

            // 전투 씬은 언어를 일부러 비운다 — 누락은 TC-UI-04 가 본다. TC-UI-06

            // 개수가 같아도 순서가 어긋날 수 있다. 그건 코드로 못 잡으니 절반만 막는다.
            if (resolutionDropdown != null && resolutionDropdown.options.Count != Resolutions.Length)
                Debug.LogWarning($"{gameObject.name}: 해상도 항목 {resolutionDropdown.options.Count}개, 코드 표 {Resolutions.Length}개 — 고른 것과 적용되는 것이 다를 수 있다", this);

            if (languageDropdown != null && languageDropdown.options.Count != LocaleCodes.Length)
                Debug.LogWarning($"{gameObject.name}: 언어 항목 {languageDropdown.options.Count}개, 코드 표 {LocaleCodes.Length}개 — 고른 것과 적용되는 것이 다를 수 있다", this);
        }

        // 기본값은 코드가 갖는다. 클램프는 PlayerPrefs 에 무엇이든 들어갈 수 있어서다.
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

        // 한 호출로 묶어야 창으로 돌아올 때 저장된 해상도가 따라온다.
        // FullScreenWindow 는 Alt-Tab 때문. 에디터에선 Game 뷰가 이겨서 빌드로만 검증된다.
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

        // SetFloat 은 이름이 틀리면 예외가 아니라 false 다. 반환값을 본다.
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

        // AvailableLocales 순서는 보장이 없어 인덱스 매핑은 조용히 뒤바뀐다. Save 는 PlayerPrefLocaleSelector 가 한다.
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