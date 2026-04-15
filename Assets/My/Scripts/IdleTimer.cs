using UnityEngine;

public class IdleTimer : MonoBehaviour
{
    [SerializeField] private float timeoutDuration = 20f;

    private float timer;

    private void OnEnable()
    {
        timer = timeoutDuration;
    }

    private void Update()
    {
        if (HasInput())
        {
            timer = timeoutDuration;
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
            GameManager.Instance.LoadTitle();
    }

    private bool HasInput()
    {
        if (Input.anyKeyDown) return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
                return true;
        }

        return false;
    }
}
