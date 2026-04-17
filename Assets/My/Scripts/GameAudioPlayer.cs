using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Wonjeong.Utils;

[RequireComponent(typeof(AudioSource))]
public class GameAudioPlayer : MonoBehaviour
{
    private const float PlayDelay = 0.5f;

    [Header("Instrument")]
    [SerializeField] private AudioClip instrumentEasy;
    [SerializeField] private AudioClip instrumentHard;

    [Header("Nature")]
    [SerializeField] private AudioClip natureEasy;
    [SerializeField] private AudioClip natureHard;

    [Header("Urban")]
    [SerializeField] private AudioClip urbanEasy;
    [SerializeField] private AudioClip urbanHard;

    [Header("버튼")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button replayButton;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
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

        audioSource.clip = clip;

        playButton.onClick.AddListener(OnPlayClicked);
        stopButton.onClick.AddListener(OnStopClicked);
        replayButton.onClick.AddListener(OnReplayClicked);

        StartCoroutine(PlayAfterDelay());
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(OnPlayClicked);
        stopButton.onClick.RemoveListener(OnStopClicked);
        replayButton.onClick.RemoveListener(OnReplayClicked);
    }

    private IEnumerator PlayAfterDelay()
    {
        yield return CoroutineData.GetWaitForSeconds(PlayDelay);
        Play();
    }

    private void Play()
    {
        if (!audioSource.clip) return;
        audioSource.Play();
    }

    private void OnPlayClicked()   => Play();
    private void OnStopClicked()   => audioSource.Stop();
    private void OnReplayClicked() { audioSource.Stop(); Play(); }
}
