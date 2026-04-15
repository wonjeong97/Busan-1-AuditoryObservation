using System.Collections.Generic;
using UnityEngine;

public class AnswerBoard : MonoBehaviour
{   
    [SerializeField] private AnswerSlot slotPrefab;
    [SerializeField] private Transform slotParent;
    
    private AnswerSlot[] slots;
    private List<SoundItem> selectedItems;
    private SoundToggleButton[] allButtons;

    public bool IsFull => selectedItems.Count >= slots.Length;
    public IReadOnlyList<SoundItem> SelectedItems => selectedItems;
    
    /// <summary>
    /// 객체 초기화 시 리스트를 할당합니다.
    /// </summary>
    private void Awake()
    {
        selectedItems = new List<SoundItem>();
    }

    /// <summary>
    /// 지정된 정답 개수만큼 슬롯을 동적으로 생성하고 보드를 초기화합니다.
    /// </summary>
    /// <param name="buttons">사용 가능한 전체 사운드 버튼 배열</param>
    /// <param name="requiredSlotCount">현재 테마와 난이도에 요구되는 정답 개수</param>
    public void Init(SoundToggleButton[] buttons, int requiredSlotCount)
    {
        allButtons = buttons;

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i])
                {
                    Destroy(slots[i].gameObject);
                }
            }
        }

        slots = new AnswerSlot[requiredSlotCount];
        for (int i = 0; i < requiredSlotCount; i++)
        {
            slots[i] = Instantiate(slotPrefab, slotParent);
        }

        for (int i = 0; i < allButtons.Length; i++)
        {
            allButtons[i].OnToggled += OnButtonToggled;
        }

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
