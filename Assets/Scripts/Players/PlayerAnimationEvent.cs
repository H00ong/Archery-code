using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    [Header("Wizard")]
    [SerializeField] GameObject magicBeam;
    [SerializeField] Transform castingPosition;

    public void CastMagicBeam() 
    {
        if (PlayerManager.CurrentState != PlayerState.Attack)
            return;

        Transform newBeam = Instantiate(magicBeam).GetComponent<Transform>();

        newBeam.position = castingPosition.position;
        newBeam.rotation = castingPosition.rotation;

        Rigidbody rb = newBeam.GetComponent<Rigidbody>();
        Vector3 flyingDir = (PlayerAttack.CurrentTarget.transform.position - newBeam.position).normalized;
        rb.linearVelocity = flyingDir * PlayerAttack.projectileSpeed;
    }
}
