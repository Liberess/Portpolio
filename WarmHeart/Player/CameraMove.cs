using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public GameObject Target;
    public float CameraZ = -10;
    // Start is called before the first frame update
    void FixedUpdate()
    {
        Vector3 TargetPos = new Vector3(Target.transform.position.x, Target.transform.position.y, CameraZ);
        transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 5f);
    }

    // Update is called once per frame
}
