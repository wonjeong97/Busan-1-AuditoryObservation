using System;
using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private Image  iconImage;
    [SerializeField] private Image  selectedOverlay;

    public event Action<SoundItem, bool> OnToggled;

    public bool IsSelected { get; private set; }
    public bool CanSelect  { get; set; } = true;
    public SoundItem Item  { get; private set; }

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClick);
    }

    public void Setup(SoundItem item)
    {
        Item             = item;
        iconImage.sprite = item.icon;
        SetSelected(false);
    }

    private void OnClick()
    {
        if (!IsSelected && !CanSelect) return;
        SetSelected(!IsSelected);
        OnToggled?.Invoke(Item, IsSelected);
    }

    private void SetSelected(bool selected)
    {
        IsSelected = selected;

        ColorBlock colors = button.colors;
        button.targetGraphic.color = selected
            ? colors.pressedColor * colors.colorMultiplier
            : colors.normalColor  * colors.colorMultiplier;

        if (selectedOverlay != null)
            selectedOverlay.gameObject.SetActive(selected);
    }
}
