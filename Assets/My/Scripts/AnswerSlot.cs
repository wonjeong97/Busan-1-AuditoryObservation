using UnityEngine;
using UnityEngine.UI;

public class AnswerSlot : MonoBehaviour
{
    [SerializeField] private Image outerImage;
    [SerializeField] private Image innerImage;

    private static readonly Color OuterFilledColor  = new(0x32 / 255f, 0x32 / 255f, 0x32 / 255f);
    private static readonly Color OuterEmptyColor   = Color.white;
    private static readonly Color OuterErrorColor   = new(0xD2 / 255f, 0x4E / 255f, 0x59 / 255f);
    private static readonly Color OuterCorrectColor = new(0x00 / 255f, 0x74 / 255f, 0xAD / 255f);
    private static readonly Color OuterWrongColor   = new(0xD2 / 255f, 0x4E / 255f, 0x59 / 255f);
    private static readonly Color InnerEmptyColor   = new(0xFD / 255f, 0xED / 255f, 0xBA / 255f);

    public bool IsEmpty => innerImage.sprite == null;

    public void SetItem(Sprite icon)
    {
        innerImage.sprite = icon;
        innerImage.color  = Color.white;
        outerImage.color  = OuterFilledColor;
    }

    public void SetEmpty()
    {
        innerImage.sprite = null;
        innerImage.color  = InnerEmptyColor;
        outerImage.color  = OuterEmptyColor;
    }

    public void MarkError()   => outerImage.color = OuterErrorColor;
    public void MarkCorrect() => outerImage.color = OuterCorrectColor;
    public void MarkWrong()   => outerImage.color = OuterWrongColor;
}
