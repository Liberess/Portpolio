using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoryBlock : MonoBehaviour
{
    public GameObject Block;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Block == null)
        {
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }
}
