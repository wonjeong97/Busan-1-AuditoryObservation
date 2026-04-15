using UnityEngine;
using UnityEngine.SceneManagement;
using Wonjeong.Reporter;
using Wonjeong.Utils;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private Reporter   reporter;
    [SerializeField] private GameObject systemCanvas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (systemCanvas != null)
            DontDestroyOnLoad(systemCanvas);
        TimestampLogHandler.Attach();
    }

    private void Start()
    {
        Cursor.visible = false;
        if (reporter && reporter.show) reporter.show = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) && reporter)
        {
            reporter.showGameManagerControl = !reporter.showGameManagerControl;
            if (reporter.show) reporter.show = false;
        }
        else if (Input.GetKeyDown(KeyCode.M)) 
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // ───────────────────────────────
    // 씬 전환
    // ───────────────────────────────
    public void LoadTitle()
    {
        ResetSession();
        SceneManager.LoadScene(SceneNames.Title);
    }

    public void LoadSelectTheme()
    {
        ResetSession();
        SceneManager.LoadScene(SceneNames.SelectTheme);
    }
    public void LoadGame()        => SceneManager.LoadScene(SceneNames.Game);

    // ───────────────────────────────
    // 세션 설정
    // ───────────────────────────────
    public void SetTheme(ThemeType theme)          => GameSession.Theme      = theme;
    public void SetDifficulty(DifficultyType diff) => GameSession.Difficulty = diff;

    // ───────────────────────────────
    // 세션 초기화
    // ───────────────────────────────
    private void ResetSession()
    {
        GameSession.Theme      = default;
        GameSession.Difficulty = default;
        GameSession.IsCorrect  = false;
    }
}
