using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using static ScenarioManager.Info;
using static UnityEngine.EventSystems.EventTrigger;
using System;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_Kelly : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;
    public bool Is_Bumping = false;
    bool Is_Holding = false;
    public GameObject Axis;
    Vector3 Attached_Point;
    bool Complete = false;
    Quaternion Attached_Rotation;
    Quaternion Target_Rot;
    Vector3 RV = new Vector3(-13.23f, 30.672f, 14.212f);
    Vector3 IER;
    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponent<NXREntity>();
        Axis = GameObject.Find("Axis");
        switch (NXRSession.Instance.scenarioName.Value)
        {
            case "Femoral Fracture - Easy":
                IER = new Vector3(0.337f, 25.439f, -16.936f);
                break;
            case "Femoral Fracture - Normal":
                IER = new Vector3(2.732f, 25.615f, -15.788f);
                break;
            case "Femoral Fracture - Hard":
                IER = new Vector3(1.273f, 25.591f, -16.487f);
                break;
        }
        StartCoroutine(Hand_Decision());
    }

    IEnumerator Hand_Decision()
    {
        while (GetComponent<NetworkBehaviour>().Hand == null)
            yield return null;
        Hand = GetComponent<NetworkBehaviour>().Hand;
    }

    public void TriggerEnter()
    {
        Attached_Point = transform.position;
        Attached_Rotation = transform.rotation;
        entity.grabMode = NXREntity.GrabMode.Direct;
        entity.EnableTrack(false);
        transform.parent = Axis.transform;
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
        Hand_Mesh.transform.parent = transform;
        entity.PlayVibration(Hand.name);
        StartCoroutine(Bone_Rotating());
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        List<InputDevice> m_Device = new List<InputDevice>();
        InputDevices.GetDevices(m_Device);
        if (m_Device.Count > 0)
        {
            m_Device[1].TryGetFeatureValue(CommonUsages.gripButton, out bool Left_Pressed);
            m_Device[2].TryGetFeatureValue(CommonUsages.gripButton, out bool Right_Pressed);
            if (Left_Pressed)
            {
                Hand = GameObject.Find("LeftHand");
                transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, 180);
            }
            else if (Right_Pressed)
            {
                Hand = GameObject.Find("RightHand");
                transform.GetChild(0).localRotation = Quaternion.Euler(0, 180, 180);
            }
        }
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (Is_Holding)
        {
            Is_Holding = false;
            Hand_Mesh.transform.parent = Hand.transform.GetChild(4);
            Hand_Mesh.transform.localPosition = Vector3.zero;
            Hand_Mesh.transform.localRotation = Quaternion.Euler(45,0,0);
            transform.parent = null;
            transform.position = Attached_Point;
            transform.rotation = Attached_Rotation;
            entity.EnableTrack(true);
            entity.grabMode = NXREntity.GrabMode.Pull;
        }
    }

    public void OnActivated(NXREntity.Hand hand)
    {

    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
        
    }
    //-3.8f, -1.4f, 16.4f
    IEnumerator Bone_Rotating()
    {
        //Quaternion Initial_Distance = Quaternion.Inverse(Quaternion.LookRotation(Axis.transform.position - Hand.transform.position));
        float Hand_Y = Hand.transform.position.y;
        Is_Holding = true;
        Vector3 Initial_Pos = transform.localPosition;
        Quaternion Initial_Rot = transform.localRotation;
        Quaternion Initial_Rotation = Axis.transform.rotation;
        while (Is_Holding)
        {
            if (Mathf.Abs(Rotation(Hand_Y)) < 100)
            {
                Axis.transform.rotation = Quaternion.Euler(IER + (RV-IER) * Rotation(Hand_Y));
            }
            if (Mathf.Abs(14.212f - Axis.transform.rotation.eulerAngles.z) < 5)
            {
                entity.PlayVibration(Hand.name);
            }
            transform.localPosition = Initial_Pos;
            transform.localRotation = Initial_Rot;
            Hand_Mesh.transform.localPosition = new Vector3(0.008800048f, 0.08210001f, 0);
            Hand_Mesh.transform.localRotation = Quaternion.Euler(-180, 0, 0);
            yield return null;
        }
        
        if (Mathf.Abs(14.212f - Axis.transform.rotation.eulerAngles.z) < 5)
        {
            entity.PlayVibration(Hand.name);
            Step6_End();
        }
        else
        {
            Target_Rot = Quaternion.Euler(IER);
        }
        float Timer = 0;
        Initial_Rot = Axis.transform.rotation;
        while (Timer < 1)
        {
            Timer += Time.deltaTime;
            Axis.transform.rotation = Quaternion.Lerp(Initial_Rot, Target_Rot, Timer);
            yield return null;
        }
        Axis.transform.rotation = Target_Rot;
        if (Complete)
        {
            Axis.GetComponent<LocalRotationChecker>().Check();
            Destroy(gameObject);
        }
        else
        {
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SphereCollider>().enabled = true;
        }
    }

    float Rotation(float Hand_Y)
    {
        return MathF.PI * -1.3f * (Hand.transform.position.y - Hand_Y);
    }

    public void Step6_End()
    {
        Target_Rot = Quaternion.Euler(RV);
        Destroy(Axis.transform.GetChild(0).GetComponent<Thigh_Bone_Head_L>());
        Complete = true;
    }
}