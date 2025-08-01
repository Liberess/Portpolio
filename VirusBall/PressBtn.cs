using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressBtn : MonoBehaviour
{
    public GameObject door;

    private void DoorOpen()
    {
        SoundManager.Instance.PlaySFX("Door");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Invoke("DoorOpen", 0.2f);
            SoundManager.Instance.PlaySFX("PressBtn");
            GetComponent<Animator>().SetTrigger("doPress");
            door.GetComponent<Animator>().SetTrigger("doHide");
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}