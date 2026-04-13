using UnityEngine;

[CreateAssetMenu(fileName = "ThemeSoundData", menuName = "Game/ThemeSoundData")]
public class ThemeSoundData : ScriptableObject
{
    public ThemeType   themeType;
    public SoundItem[] items = new SoundItem[6];

    [Header("정답 인덱스")]
    public int[] easyAnswerIndices;
    public int[] hardAnswerIndices;

    public int[] GetAnswerIndices(DifficultyType difficulty)
        => difficulty == DifficultyType.Easy ? easyAnswerIndices : hardAnswerIndices;
}
