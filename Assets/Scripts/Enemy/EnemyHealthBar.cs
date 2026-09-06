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
        [SerializeField] private bool hideWhenFull = true;

        private Health _health;
        private Transform _followTarget;
        private Transform _cam;

        /// <summary> EnemyController가 Health 초기화 직후 호출한다. 풀에서 재사용될 때마다 다시 불린다. </summary>
        public void Initialize(Health health, Transform followTarget)
        {
            if (_health != null)
                _health.OnDie -= HandleDie;

            _health = health;
            _followTarget = followTarget;

            _health.OnDie += HandleDie;

            gameObject.SetActive(true);
            RefreshValue();
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.OnDie -= HandleDie;
        }

        private void HandleDie()
        {
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_health == null || _followTarget == null) return;

            if (_cam == null)
                _cam = CameraController.Instance != null ? CameraController.Instance.transform : null;
            if (_cam == null) return;

            transform.position = _followTarget.position + worldOffset;
            transform.rotation = _cam.rotation; // 빌보드: 적의 회전과 무관하게 항상 카메라를 향한다

            RefreshValue();
        }

        private void RefreshValue()
        {
            if (hpSlider == null || _health == null) return;

            float ratio = _health.MaxHealth > 0 ? (float)_health.CurrentHealth / _health.MaxHealth : 0f;
            hpSlider.value = ratio;

            if (hideWhenFull)
                gameObject.SetActive(ratio > 0f && ratio < 1f);
        }
    }
}
