using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
    /// <summary>
    /// 적 머리 위 체력바. World Space Canvas의 자식이라도 적의 Transform 회전과는
    /// 무관하게, 매 프레임 직접 카메라 쪽을 바라보도록 회전을 덮어써서 기울어지지 않게 한다.
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
        
        private Health _health;
        private Transform _cam;
        private bool _isVisible;

        // 체력바는 클릭 대상이 아니므로 입력 처리(Event Camera 필요)를 모두 끈다
        private void Awake()
        {
            // 프리팹은 씬 오브젝트(Main Camera)를 참조할 수 없어서 코드로 채운다
            var canvas = GetComponentInParent<Canvas>(true);
            if (canvas != null && canvas.worldCamera == null)
                canvas.worldCamera = Camera.main;

            var raycaster = GetComponentInParent<GraphicRaycaster>(true);
            if (raycaster != null) raycaster.enabled = false;

            if (hpSlider == null) return;

            hpSlider.interactable = false;
            foreach (var graphic in hpSlider.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        /// <summary> EnemyController가 Health 초기화 직후 호출한다. 풀에서 재사용될 때마다 다시 불린다. </summary>
        public void Initialize(Health health, Transform followTarget)
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= RefreshValue;
                _health.OnDie -= HandleDie;
            }

            _health = health;

            _health.OnHealthChanged += RefreshValue;
            _health.OnDie += HandleDie;

            gameObject.SetActive(true);
            transform.localPosition = worldOffset;
            RefreshValue();
            SetVisible(true);
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= RefreshValue;
                _health.OnDie -= HandleDie;
            }
        }

        private void HandleDie()
        {
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (!_isVisible) return;

            if (_cam == null)
                _cam = CameraController.Instance != null ? CameraController.Instance.transform : null;
            if (_cam == null) return;

            // 빌보드: 적의 회전과 무관하게 카메라와 같은 방향 유지 (보이는 바만 갱신)
            if (transform.rotation != _cam.rotation)
                transform.rotation = _cam.rotation;
        }

        private void RefreshValue()
        {
            if (hpSlider == null || _health == null) return;

            float ratio = _health.MaxHealth > 0 ? (float)_health.CurrentHealth / _health.MaxHealth : 0f;
            hpSlider.value = ratio;
        }

        private void SetVisible(bool visible)
        {
            _isVisible = visible;

            if (hpSlider != null && hpSlider.gameObject.activeSelf != visible)
                hpSlider.gameObject.SetActive(visible);
        }

#if UNITY_EDITOR
        // 인게임 카메라(InGame 씬 Main Camera)의 기울기. 씬/프리팹 모드에 카메라가 없을 때 쓴다
        private static readonly Quaternion FallbackCameraRotation = Quaternion.Euler(55f, 0f, 0f);

        /// <summary> 인게임에서 보이는 그대로 슬라이더를 배치한다 (worldOffset 위치 + 카메라 방향 빌보드). </summary>
        [ContextMenu("Preview In-Game Placement")]
        private void PreviewInGamePlacement()
        {
            UnityEditor.Undo.RecordObject(transform, "Preview HP Slider Placement");

            transform.localPosition = worldOffset;
            transform.rotation = GetEditorCameraRotation();

            UnityEditor.EditorUtility.SetDirty(transform);
        }

        /// <summary> 직접 옮겨 둔 슬라이더의 현재 위치를 worldOffset에 저장한다. 값을 조정한 뒤 이걸 실행하면 인게임에도 반영된다. </summary>
        [ContextMenu("Save Current Position To Offset")]
        private void SaveCurrentPositionToOffset()
        {
            UnityEditor.Undo.RecordObject(this, "Save HP Slider Offset");

            worldOffset = transform.localPosition;

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[EnemyHealthBar] {name} worldOffset = {worldOffset}", this);
        }

        private static Quaternion GetEditorCameraRotation()
        {
            var controller = FindFirstObjectByType<CameraController>();
            if (controller != null) return controller.transform.rotation;

            return Camera.main != null ? Camera.main.transform.rotation : FallbackCameraRotation;
        }
#endif
    }
}
