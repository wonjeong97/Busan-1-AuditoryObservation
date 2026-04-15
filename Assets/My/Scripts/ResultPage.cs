using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPage : MonoBehaviour
{
    [Header("Dynamic UI")]
    [SerializeField] private Button resultButtonPrefab;
    [SerializeField] private Transform resultButtonParent;
    [SerializeField] private Text resultText;

    [Header("버튼")]
    [SerializeField] private Button homeButton;

    private List<Button> generatedButtons;
    private AudioSource audioSource;

    /// <summary>
    /// 객체 초기화 시 컬렉션과 컴포넌트를 할당합니다.
    /// </summary>
    /// <remarks>
    /// 필드 기본값 초기화를 피하고 런타임에 안전하게 인스턴스를 생성하기 위함.
    /// </remarks>
    private void Awake()
    {
        generatedButtons = new List<Button>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 씬 진입 시 이벤트를 등록합니다.
    /// </summary>
    private void Start()
    {
        if (homeButton) homeButton.onClick.AddListener(GoToTitle);
    }

    /// <summary>
    /// 객체 파괴 시 리소스를 정리합니다.
    /// </summary>
    /// <remarks>
    /// 메모리 누수 및 좀비 이벤트 발생 방지.
    /// </remarks>
    private void OnDestroy()
    {
        if (homeButton) homeButton.onClick.RemoveListener(GoToTitle);
    }

    /// <summary>
    /// 결과 페이지의 UI를 동적으로 설정합니다.
    /// </summary>
    /// <param name="correctItems">해당 테마와 난이도의 실제 정답 아이템 목록</param>
    /// <param name="correctCount">사용자가 맞춘 정답 개수</param>
    /// <param name="totalSlots">현재 게임에서 요구되는 총 정답 개수</param>
    /// <remarks>
    /// GridLayoutGroup을 참조하여 정답 개수에 따라 열 개수를 동적으로 조절함.
    /// 4개일 경우 2x2 배열, 그 외에는 개수만큼 열을 만들어 1줄 배열로 정렬.
    /// </remarks>
    public void Setup(IReadOnlyList<SoundItem> correctItems, int correctCount, int totalSlots)
    {
        // 이전 결과 버튼들을 제거
        foreach (Button btn in generatedButtons)
        {
            if (btn)
            {
                Destroy(btn.gameObject);
            }
        }
        generatedButtons.Clear();

        // GridLayoutGroup 동적 제어
        GridLayoutGroup layoutGroup = resultButtonParent.GetComponent<GridLayoutGroup>();
        
        if (!layoutGroup)
        {
            Debug.LogWarning("[ResultPage] GridLayoutGroup 컴포넌트가 없습니다. Grid 제어가 무시됩니다.");
        }
        else
        {
            layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layoutGroup.constraintCount = totalSlots == 4 ? 2 : totalSlots;
        }

        // 현재 판의 정답 개수만큼 결과 버튼 동적 생성
        for (int i = 0; i < totalSlots; i++)
        {
            Button newButton = Instantiate(resultButtonPrefab, resultButtonParent);
            generatedButtons.Add(newButton);

            if (i < correctItems.Count)
            {
                newButton.GetComponent<Image>().sprite = correctItems[i].icon;

                AudioClip clip = correctItems[i].clip;
                newButton.onClick.RemoveAllListeners();
                newButton.onClick.AddListener(() => PlayClip(clip));
            }
            else
            {
                newButton.interactable = false;
            }
        }

        if (resultText)
        {
            resultText.text = $"<color=#0074AD>{totalSlots}</color>개 중에 <color=#0074AD>{correctCount}</color>개 정답";
        }
    }

    /// <summary>
    /// 결과 버튼 클릭 시 해당 사운드를 재생합니다.
    /// </summary>
    /// <remarks>
    /// 정답 소리를 다시 들어볼 수 있도록 지원.
    /// </remarks>
    private void PlayClip(AudioClip clip)
    {
        if (!clip)
        {
            Debug.LogWarning("[ResultPage] AudioClip이 할당되지 않았습니다.");
            return;
        }

        if (!audioSource) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// 홈 버튼 클릭 시 타이틀 화면으로 돌아갑니다.
    /// </summary>
    /// <remarks>
    /// GameManager를 통해 안전하게 초기 씬으로 복귀.
    /// </remarks>
    private void GoToTitle()
    {
        if (!GameManager.Instance)
        {
            Debug.LogWarning("[ResultPage] GameManager 인스턴스가 존재하지 않습니다.");
            return;
        }
        GameManager.Instance.LoadTitle();
    }
}