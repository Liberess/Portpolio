using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public PlayerFly PlayerFly { get; private set; }
    public PlayerMove PlayerMove { get; private set; }
    public PlayerHealth PlayerHealth { get; private set; }
    public PlayerInteract PlayerInteract { get; private set; }

    private void Awake()
    {
        PlayerFly = GetComponent<PlayerFly>();
        PlayerMove = GetComponent<PlayerMove>();
        PlayerHealth = GetComponent<PlayerHealth>();
        PlayerInteract = GetComponent<PlayerInteract>();
    }

    public void SetComponentsActive(bool value)
    {
        PlayerFly.enabled = value;
        PlayerMove.enabled = value;
        PlayerHealth.enabled = value;
        PlayerInteract.enabled = value;
    }
}
