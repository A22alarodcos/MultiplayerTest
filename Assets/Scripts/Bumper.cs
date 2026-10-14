using Unity.Netcode;
using UnityEngine;

public class Bumper : NetworkBehaviour
{
    [SerializeField] private float bumpForce;

    private void OnTriggerEnter(Collider other)
    {

        if (!IsServer) return;

        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player contact");
            PlayerController player = other.GetComponentInParent<PlayerController>();

            if (player != null)
            {
                player.Bump(-transform.forward * bumpForce);
            }
        }
    }
}
