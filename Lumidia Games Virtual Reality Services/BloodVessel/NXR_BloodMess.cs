using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NXR_BloodMess : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Dummy004")
            HandleCut(other, true);
        else if(other.name == "Dummy005")
            HandleCut(other, false);
    }

    private void HandleCut(Collider other, bool isFirst)
    {
        var aorta = GameObject.Find("Abdominal_Aorta").GetComponent<NXR_Aorta_CS>();

        string animName;
        if (isFirst)
        {
            aorta.isCut_First = true;
            animName = "Blood_Vessel_First_Cut";
        }
        else
        {
            aorta.isCut_Second = true;
            animName = "Blood_Vessel_Second_Cut";
        }
        
        other.GetComponent<SphereCollider>().enabled = false;
        other.GetComponentInParent<Animation>().Play(animName);
        
        App.Instance.UngrabAll(App.Instance.xrLeftHandDirectInteractor);
        App.Instance.UngrabAll(App.Instance.xrRightHandDirectInteractor);
            
        Destroy(gameObject);
    }
}
