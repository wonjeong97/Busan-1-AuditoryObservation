using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SelectThemeManager : MonoBehaviour
{
    private const float TimeoutDuration = 10f;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Page1 - Theme Buttons")]
    [SerializeField] private Button instrumentButton;
    [SerializeField] private Button natureButton;
    [SerializeField] private Button urbanButton;

    [Header("Page2")]
    [SerializeField] private Text themeNameText;
    [SerializeField] private Text timeoutText;
    [SerializeField] private Button easyButton;
    [SerializeField] private Button hardButton;

    [Header("Common")]
    [SerializeField] private Button backButton;

    private ThemeType selectedTheme;
    private Coroutine timeoutCoroutine;
    private int currentPage;

    private void Start()
    {
        instrumentButton.onClick.AddListener(() => OnThemeSelected(ThemeType.Instrument));
        natureButton.onClick.AddListener(() => OnThemeSelected(ThemeType.Nature));
        urbanButton.onClick.AddListener(() => OnThemeSelected(ThemeType.Urban));
        backButton.onClick.AddListener(OnBackButtonClicked);
        easyButton.onClick.AddListener(() => OnDifficultySelected(DifficultyType.Easy));
        hardButton.onClick.AddListener(() => OnDifficultySelected(DifficultyType.Hard));

        ShowPage(1);
    }

    private void OnDestroy()
    {
        instrumentButton.onClick.RemoveAllListeners();
        natureButton.onClick.RemoveAllListeners();
        urbanButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();
        easyButton.onClick.RemoveAllListeners();
        hardButton.onClick.RemoveAllListeners();
    }

    private void OnBackButtonClicked()
    {
        if (currentPage == 1)
        {
            GameManager.Instance.LoadTitle();
        }
        else
        {
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
            ShowPage(1);
        }
    }

    private void OnThemeSelected(ThemeType theme)
    {
        selectedTheme = theme;
        themeNameText.text = theme switch
        {
            ThemeType.Instrument => "[<color=#FFDC50> 악기 </color>]",
            ThemeType.Nature     => "[<color=#FFDC50> 자연 </color>]",
            ThemeType.Urban      => "[<color=#FFDC50> 도심 </color>]",
            _                    => string.Empty,
        };
        ShowPage(2);

        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(TimeoutRoutine());
    }

    private IEnumerator TimeoutRoutine()
    {
        int remaining = (int)TimeoutDuration;
        while (remaining > 0)
        {
            timeoutText.text = $"<color=#D24E59>{remaining}</color>초 간 선택하지 않으면, 처음으로 돌아가요!";
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        GameManager.Instance.LoadTitle();
    }

    private void ShowPage(int page)
    {
        currentPage = page;
        page1.SetActive(page == 1);
        page2.SetActive(page == 2);
    }

    private void OnDifficultySelected(DifficultyType difficulty)
    {
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);

        GameManager.Instance.SetTheme(selectedTheme);
        GameManager.Instance.SetDifficulty(difficulty);
        GameManager.Instance.LoadGame();
    }

    public ThemeType GetSelectedTheme() => selectedTheme;
}
