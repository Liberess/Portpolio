using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NXR_Bit : NetworkBehaviour, IEntityInteractable
{
    private GameObject curPatient;
    public GameObject Drill;
    
    /// <summary>
    /// 플레이어가 Scalpel를 쥐고 있는가?
    /// </summary>
    private bool isActive = false;

    /// <summary>
    /// 현재 절개를 진행하는 중인가?
    /// </summary>

    /// <summary>
    /// 절개를 완료했는가?
    /// </summary>
    public bool IsIncision { get; private set; } = false;
    public void SetIncision(bool value) => IsIncision = value;

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (Drill != null)
        {
            if (Drill.name == "Screw_driver")
            {
                Drill.GetComponent<NXR_ScrewDriver>().Attach_Screw();
                return;
            }
            else if (Drill.name == "Screw_driver_H")
            {
                Drill.GetComponent<NXR_ScrewDriver_H>().Attach_Screw();
                return;
            }
            if (name == "LongDrill_Bit")
            {
                Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit = Drill.transform.GetChild(1).GetChild(0).gameObject;
                Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit.SetActive(true);
                return;
            }
            else if (Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit == null)
            {
                if (name == "Reaming_Bit_Step12" || name == "Recon_Screw_driver" || name == "Recon_Screw" || name == "Drill_Bit_H")
                {
                    Drill.GetComponent<NXRDrillFollowReamingrod>().UnGrab(name);
                }
                return;
            }
            else switch (name.Substring(11, 1))
                {
                    case "S":
                        Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit_Edge = Drill.transform.GetChild(1).GetChild(0).GetChild(2).Find("ReamingBit_S").gameObject;
                        break;
                    case "M":
                        Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit_Edge = Drill.transform.GetChild(1).GetChild(0).GetChild(2).Find("ReamingBit_M").gameObject;
                        break;
                    case "L":
                        Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit_Edge = Drill.transform.GetChild(1).GetChild(0).GetChild(2).Find("ReamingBit_L").gameObject;
                        break;
                }
            Drill.GetComponent<NXRDrillFollowReamingrod>().Reaming_Bit_Edge.SetActive(true);
        }
    }

    public void OnActivated(NXREntity.Hand hand)
    {
    
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
      
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name == "ReamingDrill" || other.transform.root.name == "Screw_driver" || other.transform.root.name == "Screw_driver_H")
        {
            Drill = other.transform.root.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.name == "ReamingDrill" || other.transform.root.name == "Screw_driver" || other.transform.root.name == "Screw_driver_H")
        {
            Drill = null;
        }
    }
}
