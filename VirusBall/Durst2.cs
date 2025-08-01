using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Durst2 : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 0.8f);
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}