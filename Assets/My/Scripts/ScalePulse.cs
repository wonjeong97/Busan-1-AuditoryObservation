using UnityEngine;

public class ScalePulse : MonoBehaviour
{
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float speed    = 1.5f;

    private float offset;

    private void Awake()
    {
        offset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float t     = (Mathf.Sin(Time.time * speed + offset) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = Vector3.one * scale;
    }
}
