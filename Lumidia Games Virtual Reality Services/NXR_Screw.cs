using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NXR_Screw : MonoBehaviour
{
    [SerializeField] private SpinePoint curSpinePoint;
    public SpinePoint CurSpinePoint => curSpinePoint;

    [SerializeField] private Transform startTorqueTs;
    public Transform StartTorqueTs => startTorqueTs;

    [SerializeField] private Transform startCapDriverTs;
    public Transform StartCapDriverTs => startCapDriverTs;
  
    [SerializeField] private Transform stopCapDriverTs;
    public Transform StopCapDriverTs => stopCapDriverTs;

    [SerializeField] private Transform endCapDriverTs;
    public Transform EndCapDriverTs => endCapDriverTs;

    public NXRCapDriver CurCapDriver { get; private set; }

    /// <summary>
    /// Torque를 장착했는지
    /// </summary>
    public bool isEquipTorque = false;

    /// <summary>
    /// Torque 공정이 끝났는지
    /// </summary>
    public bool isCompleteTorque = false;

    public void SetCapDriver(NXRCapDriver driver) => CurCapDriver = driver;

    public GameObject Rod;

    public void SetEnabledCollider(bool enabled)
    {
        var cols = GetComponentsInChildren<BoxCollider>();
        foreach (var col in cols)
            col.enabled = enabled;
    }

    public void Rod_Set()
    {
        switch (transform.parent.name)
        {
            case "Level_1":
                Rod.transform.position = new Vector3(1.86f, 0.57967f, -0.04783f);
                Rod.transform.rotation = Quaternion.Euler(107.561f, -24.448f, -20.385f);
                break;    
            case "Level_2":
                Rod.transform.position = new Vector3(1.86f, 0.579f, -0.048f);
                Rod.transform.rotation = Quaternion.Euler(106.691f, -25.578f, -21.465f);
                break; 
            case "Level_3":
                Rod.transform.position = new Vector3(1.86f, 0.57848f, -0.04817f);
                Rod.transform.rotation = Quaternion.Euler(105.906f, -26.708f, -22.549f);                
                break;
        }
    }

    public void Enable_Collider(bool TF)
    {
        transform.GetChild(3).GetComponent<BoxCollider>().enabled = TF;
        transform.GetChild(5).GetComponent<BoxCollider>().enabled = TF;
        transform.GetChild(6).GetComponent<BoxCollider>().enabled = TF;
    }
}