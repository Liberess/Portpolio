using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class NXR_RodHolder : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;

    private XRSocketInteractorTag socket;
    
    public SpinePoint SpinePoint { get; private set; }

    [SerializeField]
    private Transform interactTs;

    public bool isDone = false;
    private Vector3 rotPivot;
    float Timer = 0;
    public NXR_Rod rod;
    bool Attached = false;
    ActionBasedSnapTurnProvider STP;
    DynamicMoveProvider DMP;

    private void Start()
    {
        entity = GetComponent<NXREntity>();
        socket = GetComponent<XRSocketInteractorTag>();
        STP = GameObject.Find("XR Origin").GetComponent<ActionBasedSnapTurnProvider>();
        DMP = GameObject.Find("XR Origin").GetComponent<DynamicMoveProvider>();
        StartCoroutine(Hand_Decision());
    }

    IEnumerator Hand_Decision()
    {
        while (GetComponent<NetworkBehaviour>().Hand == null)
            yield return null;
        Hand = GetComponent<NetworkBehaviour>().Hand;
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
    }

    private void FixedUpdate()
    {
        if(isDone)
            return;
        
        if (entity.IsGrabbedByMe())
        {
            if (SpinePoint && rod && IsForwardJoystickInput())
            {                
                Timer += Time.deltaTime * 0.5f;
                GetComponent<Animator>().Play("Rod_Holder_Rotation", 0, Timer);
                if (Timer >= 1)
                {
                    isDone = true;
                    
                    Debug.Log("RodHolder Complete");
                    entity.ShowTooltip("RodHolder Complete");
                    rod.SetCalm();
                    Hand_Out();
                    for (int i = 0; i < 3; i++)
                    {
                        GameObject.Find("Spine").transform.GetChild(i).GetChild(1).GetComponent<NXR_Screw>().Rod = rod.gameObject;
                    }
                    SpinePoint.isInsertRod = true;
                    socket.socketActive = false;
                    entity.EnableTrack(true);
                    gameObject.SetActive(false);
                }
                
                if (!Mathf.Approximately(transform.position.z, rotPivot.z - 0.2f))
                {
                    Vector3 newPos = (Vector3.forward * 0.02f) + (-Vector3.up * 0.002f); 
                    transform.Translate(newPos * Time.fixedDeltaTime);
                }
            }
        }
    }

    private bool IsForwardJoystickInput()
    {
        InputDevice rCtrlDevice = new InputDevice();
        switch (Hand.name)
        {
            case "LeftHand":
                rCtrlDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
                break;
            case "RightHand":
                rCtrlDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                break;
        }
        if (rCtrlDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var rCtrlPos))
        {
            // 1,2사분면(앞)
            if (rCtrlPos.y > 0)
                return true;
        }

        return false;
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        if (Attached)
            return;
        List<InputDevice> m_Device = new List<InputDevice>();
        InputDevices.GetDevices(m_Device);
        if (m_Device.Count > 0)
        {
            m_Device[1].TryGetFeatureValue(CommonUsages.gripButton, out bool Left_Pressed);
            m_Device[2].TryGetFeatureValue(CommonUsages.gripButton, out bool Right_Pressed);
            if (Left_Pressed)
            {
                Hand = GameObject.Find("LeftHand");
            }
            else if (Right_Pressed)
            {
                Hand = GameObject.Find("RightHand");
            }
        }
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
        STP.enabled = false;
        DMP.enabled = false;
        Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;        
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (Attached)
        {
            Hand_Out();
        }
    }

    public void OnActivated(NXREntity.Hand hand)
    {
        
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {

    }

    void Hand_Out()
    {
        STP.enabled = true;
        DMP.enabled = true;
        Hand_Mesh.transform.parent = Hand.transform.GetChild(4).transform;
        Hand_Mesh.transform.localPosition = Vector3.zero;
        Hand_Mesh.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isDone)
            return;
        
        if (other.transform.parent.name == "Rod")
        {
            rod = other.transform.parent.GetComponent<NXR_Rod>();
        }

        if (!SpinePoint && other.TryGetComponent(out SpinePoint spinePoint))
        {
            if (spinePoint.isRemoveSmallDilator)
            {
                Attached = true;
                SpinePoint = spinePoint;
                Vector3 curPos = SpinePoint.transform.position;
                Vector3 newPos = new Vector3(curPos.x, curPos.y + 0.03f, curPos.z);
                transform.position = newPos;
                transform.rotation = Quaternion.identity;
                GetComponent<Animator>().enabled = true;
                GetComponent<Animator>().speed = 0;
                rotPivot = curPos;
                rotPivot.z -= 0.2f;
                STP.enabled = false;
                DMP.enabled = false;
                Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;
                entity.PlayVibration(Hand.name);
                entity.EnableTrack(false);
            }
        }
    }
}
