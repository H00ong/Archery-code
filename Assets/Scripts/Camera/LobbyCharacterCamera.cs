using UnityEngine;

/// <summary>
/// RenderTexture 전용 카메라. UI/입력 로직 없이 카메라와 RT만 관리한다.
/// </summary>
public class LobbyCharacterCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera characterCamera;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1f, -3f);
    [SerializeField] private Vector3 cameraRotation = new Vector3(0f, 180f, 0f);

    [Header("RenderTexture Settings")]
    [SerializeField] private int textureWidth = 512;
    [SerializeField] private int textureHeight = 512;

    private RenderTexture _renderTexture;

    private void Awake()
    {
        if (characterCamera != null)
        {
            // RT를 먼저 할당해야 화면에 직접 렌더링되지 않는다
            GetOrCreateRenderTexture();
            characterCamera.enabled = false;
        }
    }

    public RenderTexture GetOrCreateRenderTexture()
    {
        if (_renderTexture == null)
        {
            _renderTexture = new RenderTexture(textureWidth, textureHeight, 16, RenderTextureFormat.ARGB32);
            characterCamera.targetTexture = _renderTexture;
            characterCamera.clearFlags = CameraClearFlags.SolidColor;
            characterCamera.backgroundColor = Color.clear;
        }
        return _renderTexture;
    }

    public void SetupPosition(Vector3 targetPosition)
    {
        characterCamera.transform.position = targetPosition + cameraOffset;
        characterCamera.transform.LookAt(targetPosition + Vector3.up * cameraOffset.y);
        characterCamera.transform.rotation = Quaternion.Euler(cameraRotation);
    }

    public void EnableCamera() => characterCamera.enabled = true;
    public void DisableCamera() => characterCamera.enabled = false;

    private void OnDestroy()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
        }
    }
}
