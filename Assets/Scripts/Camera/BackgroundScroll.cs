using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("스크롤 속도 설정")]
    public float scrollSpeed = 0.1f;

    [SerializeField] private Renderer quadRenderer;
    private Material _material;
    private Vector2 _savedOffset;

    void Start()
    {
        _material = quadRenderer.material; // 인스턴스 생성 (원본 에셋 보호)
        _savedOffset = _material.mainTextureOffset;
    }

    void Update()
    {
        float x = Mathf.Repeat(Time.time * scrollSpeed, 1f);
        _material.mainTextureOffset = new Vector2(x, _savedOffset.y);
    }

    void OnDisable()
    {
        if (_material != null)
            _material.mainTextureOffset = _savedOffset;
    }
}
