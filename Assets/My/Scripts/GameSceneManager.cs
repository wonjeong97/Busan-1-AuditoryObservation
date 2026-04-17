using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wonjeong.Utils;

public class GameSceneManager : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject gamePage;
    [SerializeField] private GameObject resultPage;
    [SerializeField] private ResultPage resultPageController;

    [Header("Answer Board")]
    [SerializeField] private AnswerBoard answerBoard;

    [Header("Sound Buttons (공통)")]
    [SerializeField] private SoundToggleButton[] soundButtons;

    [Header("버튼")]
    [SerializeField] private Button completeButton;
    [SerializeField] private Button backButton;

    [Header("Guide Text")]
    [SerializeField] private Text guideText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text answerboardText;

    [Header("Theme Sound Data")]
    [SerializeField] private ThemeSoundData instrumentData;
    [SerializeField] private ThemeSoundData natureData;
    [SerializeField] private ThemeSoundData urbanData;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonStop;
    [SerializeField] private Button buttonReplay;

    [SerializeField] private AudioClip instrumentEasy;
    [SerializeField] private AudioClip instrumentHard;
    [SerializeField] private AudioClip natureEasy;
    [SerializeField] private AudioClip natureHard;
    [SerializeField] private AudioClip urbanEasy;
    [SerializeField] private AudioClip urbanHard;

    private AnswerBoard    activeBoard;
    private GameObject     activeGuideText;
    private ThemeSoundData activeThemeData;
    private const float    PlayDelay = 0.5f;

    /// <summary>
    /// 씬 진입 시 초기화를 수행합니다.
    /// </summary>
    /// <remarks>
    /// 단일화된 보드 구조에 맞춰 초기 상태를 설정하고 오디오 및 버튼 리스너를 등록함.
    /// </remarks>
    private void Start()
    {
        if (gamePage) gamePage.SetActive(true);
        if (resultPage) resultPage.SetActive(false);
        
        SetupSoundButtons();
        SetupPage();
        SetupAudio();
        
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        StartCoroutine(PlayAfterDelay());
    }
    
    /// <summary>
    /// 설정된 지연 시간 이후 오디오를 자동으로 재생합니다.
    /// </summary>
    /// <remarks>
    /// 씬 로드 직후 발생할 수 있는 프레임 드랍 구간을 회피하여 사운드 끊김 방지.
    /// </remarks>
    /// <returns>IEnumerator 지연 대기 객체</returns>
    private IEnumerator PlayAfterDelay()
    {
        yield return CoroutineData.GetWaitForSeconds(PlayDelay);
        OnPlayClicked();
    }

    /// <summary>
    /// 객체 파괴 시 리소스를 정리합니다.
    /// </summary>
    /// <remarks>
    /// 메모리 누수 방지 및 좀비 이벤트 발생 차단.
    /// </remarks>
    private void OnDestroy()
    {
        completeButton.onClick.RemoveListener(OnCompleteButtonClicked);
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        CleanupAudio();

        if (soundButtons != null)
        {
            for (int i = 0; i < soundButtons.Length; i++)
            {
                if (soundButtons[i])
                {
                    soundButtons[i].OnToggled -= OnSoundButtonToggled;
                }
            }
        }
    }
    
    /// <summary>
    /// 사운드 버튼 토글 이벤트를 수신합니다.
    /// </summary>
    /// <param name="item">상태가 변경된 사운드 아이템</param>
    /// <param name="isOn">활성화 여부</param>
    /// <remarks>
    /// 플레이어의 새로운 입력이 발생했을 때 에러 가이드 텍스트를 즉시 숨겨 시각적 방해를 최소화함.
    /// </remarks>
    private void OnSoundButtonToggled(SoundItem item, bool isOn)
    {
        if (guideText)
        {
            guideText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 현재 난이도에 필요한 슬롯 개수를 계산하여 보드를 초기화하고 가이드 문구를 설정합니다.
    /// </summary>
    /// <remarks>
    /// 테마와 동적 정답 개수에 맞춰 사용자 안내 텍스트를 갱신함.
    /// </remarks>
    private void SetupPage()
    {
        if (!answerBoard) return;

        int requiredSlots = activeThemeData.GetAnswerIndices(GameSession.Difficulty).Length;
        answerBoard.Init(soundButtons, requiredSlots);
        
        if (guideText)
        {
            string themeKeyword = GameSession.Theme switch
            {
                ThemeType.Instrument => "악기를",
                _                    => "소리를"
            };

            guideText.text = $"{themeKeyword} {requiredSlots}개\n선택해주세요.";
            guideText.gameObject.SetActive(false);
        }
        
        if (descriptionText)
        {
            descriptionText.text = GameSession.Theme switch
            {
                ThemeType.Instrument => "음악을 관찰하여 어떤 악기가 사용되었는지,\n모두 선택해주세요.",
                _                    => "음악을 관찰하여 어떤 소리가 들리는지,\n모두 선택해주세요."
            };
        }
        
        if (answerboardText)
        {
            answerboardText.text = GameSession.Theme switch
            {
                ThemeType.Instrument => "선택한 악기",
                _                    => "선택한 소리"
            };
        }
    }

    /// <summary>
    /// 선택된 테마에 맞춰 사운드 버튼의 데이터를 매핑합니다.
    /// </summary>
    /// <remarks>
    /// 하드코딩을 피하고 ScriptableObject 데이터를 동적으로 할당하기 위함.
    /// </remarks>
    private void SetupSoundButtons()
    {
        ThemeSoundData data = GameSession.Theme switch
        {
            ThemeType.Instrument => instrumentData,
            ThemeType.Nature     => natureData,
            ThemeType.Urban      => urbanData,
            _                    => null,
        };

        if (!data) return;

        activeThemeData = data;
        for (int i = 0; i < soundButtons.Length; i++)
        {
            soundButtons[i].Setup(data.items[i]);
            
            soundButtons[i].OnToggled -= OnSoundButtonToggled;
            soundButtons[i].OnToggled += OnSoundButtonToggled;
        }
    }

    /// <summary>
    /// 게임 세션 상태에 맞는 오디오를 준비합니다.
    /// </summary>
    /// <remarks>
    /// 테마와 난이도 조합으로 정확한 사운드 트랙 제공.
    /// </remarks>
    private void SetupAudio()
    {
        AudioClip clip = (GameSession.Theme, GameSession.Difficulty) switch
        {
            (ThemeType.Instrument, DifficultyType.Easy) => instrumentEasy,
            (ThemeType.Instrument, DifficultyType.Hard) => instrumentHard,
            (ThemeType.Nature,     DifficultyType.Easy) => natureEasy,
            (ThemeType.Nature,     DifficultyType.Hard) => natureHard,
            (ThemeType.Urban,      DifficultyType.Easy) => urbanEasy,
            (ThemeType.Urban,      DifficultyType.Hard) => urbanHard,
            _                                           => null,
        };

        if (!clip)
        {
            clip = instrumentEasy;
            Debug.LogWarning("AudioClip missing. Fallback to instrumentEasy.");
        }

        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource missing. AddComponent Fallback.");
        }

        audioSource.clip = clip;

        if (buttonPlay) buttonPlay.onClick.AddListener(OnPlayClicked);
        if (buttonStop) buttonStop.onClick.AddListener(OnStopClicked);
        if (buttonReplay) buttonReplay.onClick.AddListener(OnReplayClicked);

        // TODO: 오디오 클립 로드를 Addressables 등으로 변경하여 메모리 최적화 고려
    }

    /// <summary>
    /// 오디오 관련 버튼 리스너를 제거합니다.
    /// </summary>
    /// <remarks>
    /// 이벤트 중복 호출 및 누수 방지.
    /// </remarks>
    private void CleanupAudio()
    {
        if (buttonPlay) buttonPlay.onClick.RemoveListener(OnPlayClicked);
        if (buttonStop) buttonStop.onClick.RemoveListener(OnStopClicked);
        if (buttonReplay) buttonReplay.onClick.RemoveListener(OnReplayClicked);
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 이벤트를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 테마 선택 씬으로 복귀.
    /// </remarks>
    private void OnBackButtonClicked()
    {
        GameManager.Instance.LoadSelectTheme();
    }

    /// <summary>
    /// 완료 버튼 클릭 시 정답 여부를 확인하고 결과 페이지로 전환합니다.
    /// </summary>
    /// <remarks>
    /// 사용자가 선택한 목록이 아닌 실제 정답 목록을 추출하여 ResultPage에 전달함.
    /// </remarks>
    private void OnCompleteButtonClicked()
    {
        bool isValid = answerBoard.Validate();

        if (!isValid)
        {
            if (guideText)
            {
                guideText.gameObject.SetActive(true);
            }
            return;
        }

        if (audioSource) audioSource.Stop();

        HashSet<SoundItem> correctItemsSet = BuildCorrectItemSet();
        GameSession.IsCorrect = CheckAnswer();
        
        answerBoard.ShowResult(correctItemsSet);
        
        int[] correctIndices = activeThemeData.GetAnswerIndices(GameSession.Difficulty);
        int totalSlots = correctIndices.Length;
        
        List<SoundItem> actualCorrectItems = new List<SoundItem>();
        foreach (int idx in correctIndices)
        {
            actualCorrectItems.Add(activeThemeData.items[idx]);
        }
        
        resultPageController.Setup(actualCorrectItems, CountCorrectAnswers(), totalSlots);
        
        if (guideText) guideText.gameObject.SetActive(false);
        if (gamePage) gamePage.SetActive(false);
        if (resultPage) resultPage.SetActive(true);
    }

    /// <summary>
    /// 오디오 재생을 요청합니다.
    /// </summary>
    /// <remarks>
    /// 중복 재생 시 사운드가 겹치는 현상 방지.
    /// </remarks>
    private void OnPlayClicked()
    {
        if (!audioSource) return;
        if (audioSource.isPlaying) return;

        audioSource.Play();
    }

    /// <summary>
    /// 오디오 정지를 요청합니다.
    /// </summary>
    /// <remarks>
    /// 불필요한 API 호출 비용 절감.
    /// </remarks>
    private void OnStopClicked()
    {
        if (!audioSource) return;
        if (!audioSource.isPlaying) return;

        audioSource.Stop();
    }

    /// <summary>
    /// 오디오를 처음부터 다시 재생합니다.
    /// </summary>
    /// <remarks>
    /// 재생 상태와 무관하게 즉각적인 재시작 보장.
    /// </remarks>
    private void OnReplayClicked()
    {
        if (!audioSource) return;

        audioSource.Stop();
        audioSource.Play();
    }

    private HashSet<SoundItem> BuildCorrectItemSet()
    {
        var set = new HashSet<SoundItem>();
        if (activeThemeData == null) return set;

        foreach (int idx in activeThemeData.GetAnswerIndices(GameSession.Difficulty))
            set.Add(activeThemeData.items[idx]);

        return set;
    }

    private bool CheckAnswer() => CountCorrectAnswers() == activeThemeData.GetAnswerIndices(GameSession.Difficulty).Length;

    /// <summary>
    /// 현재 선택된 아이템 중 정답의 개수를 카운트합니다.
    /// </summary>
    /// <remarks>
    /// 앤서 보드에서 선택된 리스트와 테마 데이터의 정답 인덱스를 비교함.
    /// </remarks>
    /// <returns>정답 개수</returns>
    private int CountCorrectAnswers()
    {
        if (!activeThemeData) return 0;

        int[] correctIndices = activeThemeData.GetAnswerIndices(GameSession.Difficulty);
        IReadOnlyList<SoundItem> selected = answerBoard.SelectedItems;
        int count = 0;

        foreach (int idx in correctIndices)
        {
            if (selected.Contains(activeThemeData.items[idx]))
            {
                count++;
            }
        }

        return count;
    }
}