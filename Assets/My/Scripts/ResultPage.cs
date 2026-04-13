using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPage : MonoBehaviour
{
    [SerializeField] private Button[]     resultButtons = new Button[5];
    [SerializeField] private Text         resultText;

    [Header("버튼")]
    [SerializeField] private Button       homeButton;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        homeButton.onClick.AddListener(GoToTitle);
    }

    private void OnDestroy()
    {
        homeButton.onClick.RemoveListener(GoToTitle);
    }

    public void Setup(IReadOnlyList<SoundItem> selectedItems, DifficultyType difficulty, int correctCount)
    {
        int total = difficulty == DifficultyType.Easy ? 3 : 5;

        for (int i = 0; i < resultButtons.Length; i++)
        {
            bool isActive = i < total;
            resultButtons[i].gameObject.SetActive(isActive);

            if (!isActive) continue;

            resultButtons[i].GetComponent<Image>().sprite = selectedItems[i].icon;

            int     index = i;
            AudioClip clip = selectedItems[i].clip;
            resultButtons[i].onClick.RemoveAllListeners();
            resultButtons[i].onClick.AddListener(() => PlayClip(clip));
        }

        resultText.text = $"<color=#0074AD>{total}</color>개 중에 <color=#0074AD>{correctCount}</color>개 정답";
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[ResultPage] AudioClip이 할당되지 않았습니다.");
            return;
        }
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void GoToTitle()
    {
        SceneManager.LoadScene(SceneNames.Title);
    }
}
