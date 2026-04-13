using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject gamePage;
    [SerializeField] private GameObject resultPage;
    [SerializeField] private ResultPage resultPageController;
    [SerializeField] private GameObject easyPage;
    [SerializeField] private GameObject hardPage;

    [Header("Answer Boards")]
    [SerializeField] private AnswerBoard easyAnswerBoard;
    [SerializeField] private AnswerBoard hardAnswerBoard;

    [Header("Sound Buttons (공통)")]
    [SerializeField] private SoundToggleButton[] soundButtons;

    [Header("버튼")]
    [SerializeField] private Button completeButton;
    [SerializeField] private Button backButton;

    [Header("Guide Text")]
    [SerializeField] private GameObject easyGuideText;
    [SerializeField] private GameObject hardGuideText;

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
    /// 뷰 설정, 사운드 버튼 매핑, 오디오 준비 및 이벤트 리스너 등록을 위함.
    /// </remarks>
    private void Start()
    {
        gamePage.SetActive(true);
        resultPage.SetActive(false);
        SetupPage();
        SetupSoundButtons();
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
        yield return new WaitForSeconds(PlayDelay);
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
    }

    /// <summary>
    /// 난이도에 따른 UI 페이지를 설정합니다.
    /// </summary>
    /// <remarks>
    /// GameSession의 Difficulty 값을 기반으로 활성화할 보드와 안내 텍스트 분기.
    /// </remarks>
    private void SetupPage()
    {
        bool isEasy = GameSession.Difficulty == DifficultyType.Easy;
        easyPage.SetActive(isEasy);
        hardPage.SetActive(!isEasy);

        activeBoard = isEasy ? easyAnswerBoard : hardAnswerBoard;
        activeGuideText = isEasy ? easyGuideText : hardGuideText;

        activeBoard.Init(soundButtons);
        activeGuideText.SetActive(false);
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
            soundButtons[i].Setup(data.items[i]);
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
        SceneManager.LoadScene(SceneNames.SelectTheme);
    }

    /// <summary>
    /// 완료 버튼 클릭 이벤트를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 보드 입력 값 검증 후 실패 시 가이드 텍스트 노출, 성공 시 다음 단계 진입.
    /// </remarks>
    private void OnCompleteButtonClicked()
    {
        bool isValid = activeBoard.Validate();

        if (!isValid)
        {
            activeGuideText.SetActive(true);
            return;
        }

        audioSource.Stop();
        HashSet<SoundItem> correctItems = BuildCorrectItemSet();
        GameSession.IsCorrect = CheckAnswer();
        activeBoard.ShowResult(correctItems);
        resultPageController.Setup(activeBoard.SelectedItems, GameSession.Difficulty, CountCorrectAnswers());
        activeGuideText.SetActive(false);
        gamePage.SetActive(false);
        resultPage.SetActive(true);
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

    private int CountCorrectAnswers()
    {
        if (activeThemeData == null) return 0;

        int[]  correctIndices = activeThemeData.GetAnswerIndices(GameSession.Difficulty);
        var    selected       = activeBoard.SelectedItems;
        int    count          = 0;

        foreach (int idx in correctIndices)
        {
            if (selected.Contains(activeThemeData.items[idx]))
                count++;
        }

        return count;
    }
}