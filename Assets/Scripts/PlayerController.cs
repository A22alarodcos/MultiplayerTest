using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 1;

    private Vector2 inputVector;
    private bool jumpRequest = false;

    private Rigidbody rb;
    private AudioSource audioSource;

    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>();
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    private Renderer playerRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        playerColor.OnValueChanged += OnColorChanged;
        playerName.OnValueChanged += OnNameChanged;

        if (IsServer)
        {
            rb.isKinematic = false;

            playerColor.Value = Random.ColorHSV();
            playerName.Value = "Jugador " + OwnerClientId;
        }
        else
        {
            rb.isKinematic = true;
        }

        ApplyColor(playerColor.Value);

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                h -= 1f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                h += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                v -= 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                v += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SubmitJumpServerRpc();

            if (Keyboard.current.cKey.wasPressedThisFrame)
                ChangeColorServerRpc();
        }

        Vector2 input = new Vector2(h, v).normalized;

        SubmitInputServerRpc(input);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        Vector3 targetVelocity = moveDirection * speed;

        rb.linearVelocity = new Vector3(
            targetVelocity.x,
            rb.linearVelocity.y,
            targetVelocity.z
        );

        transform.LookAt(transform.position + moveDirection);

        if (moveDirection.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(moveDirection, Vector3.up));

        if (jumpRequest &&
            Physics.Raycast(transform.position, Vector3.down, groundCheckDistance))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            PlayJumpSoundClientRpc();
        }

        jumpRequest = false;
    }

    private void ApplyColor(Color color)
    {
        playerRenderer.material.color = color;
    }

    private void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor(newColor);
    }
    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        Debug.Log("Nombre del jugador: " + newName);
    }

    [ServerRpc]
    private void SubmitInputServerRpc(Vector2 movementDirection)
    {
        inputVector = movementDirection;
    }

    [ServerRpc]
    private void SubmitJumpServerRpc()
    {
        jumpRequest = true;
    }

    [ClientRpc]
    private void PlayJumpSoundClientRpc()
    {
        audioSource.Play();
    }

    [ServerRpc]
    private void ChangeColorServerRpc()
    {
        playerColor.Value = Random.ColorHSV();
    }
}

