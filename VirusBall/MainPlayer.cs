using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    private void Start() => StartCoroutine(Wings());

    IEnumerator Wings()
    {
        GameObject wings = Instantiate(Resources.Load<GameObject>("Particles/Wings"), transform.position, Quaternion.identity);
        
        Destroy(wings, 1f);

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(Wings());
    }
}