using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class RectangleRotatiu : NetworkBehaviour
{
    public float velocity = 90f;

    private void FixedUpdate()
    {

        if (!IsServer) return;

        transform.Rotate(0, velocity * Time.deltaTime, 0);



    }
}
