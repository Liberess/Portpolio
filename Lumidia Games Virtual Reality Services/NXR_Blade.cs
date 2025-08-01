using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NXR_Blade : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Breaker"))
        {
            transform.SetParent(other.transform);
        }
    }

    public void Parentout()
    {
        transform.SetParent(null);
    }
}
