using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedCornerImage : MonoBehaviour
{
    [SerializeField] private float radius = 20f;

    private Image image;
    private Material material;

    private void Awake()
    {
        image = GetComponent<Image>();
        material = new Material(Shader.Find("Custom/RoundedCorner"));
        image.material = material;
    }

    private void Start()
    {
        UpdateMaterial();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (material != null)
            UpdateMaterial();
    }
#endif

    private void UpdateMaterial()
    {
        RectTransform rt = GetComponent<RectTransform>();
        material.SetFloat("_Radius", radius);
        material.SetFloat("_Width",  rt.rect.width);
        material.SetFloat("_Height", rt.rect.height);
    }
}
