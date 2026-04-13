using System.Collections.Generic;
using UnityEngine;

public class AnswerBoard : MonoBehaviour
{
    [SerializeField] private AnswerSlot[] slots;

    private readonly List<SoundItem>          selectedItems   = new();
    private          SoundToggleButton[]       allButtons;

    public bool IsFull => selectedItems.Count >= slots.Length;
    public IReadOnlyList<SoundItem> SelectedItems => selectedItems;

    public void Init(SoundToggleButton[] buttons)
    {
        allButtons = buttons;

        foreach (var btn in allButtons)
            btn.OnToggled += OnButtonToggled;

        Refresh();
    }

    private void OnDestroy()
    {
        if (allButtons == null) return;
        foreach (var btn in allButtons)
            btn.OnToggled -= OnButtonToggled;
    }

    private void OnButtonToggled(SoundItem item, bool isOn)
    {
        if (isOn)
            selectedItems.Add(item);
        else
            selectedItems.Remove(item);

        Refresh();
        UpdateCanSelect();
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < selectedItems.Count)
                slots[i].SetItem(selectedItems[i].icon);
            else
                slots[i].SetEmpty();
        }
    }

    public void ShowResult(HashSet<SoundItem> correctItems)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < selectedItems.Count)
            {
                if (correctItems.Contains(selectedItems[i]))
                    slots[i].MarkCorrect();
                else
                    slots[i].MarkWrong();
            }
        }
    }

    // 빈 슬롯에 에러 표시. 모두 채워진 경우 true 반환
    public bool Validate()
    {
        bool allFilled = true;
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.MarkError();
                allFilled = false;
            }
        }
        return allFilled;
    }

    private void UpdateCanSelect()
    {
        bool canSelect = !IsFull;
        foreach (var btn in allButtons)
        {
            if (!btn.IsSelected)
                btn.CanSelect = canSelect;
        }
    }
}
