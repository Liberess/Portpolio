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
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_Forceps : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;
    public GameObject Hand;
    GameObject Hand_Mesh;
    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponent<NXREntity>();
        //Hand = NXRSession.Instance.Hand_Decision();
    }

    // Update is called once per frame
    void Update()
    {

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
                transform.GetChild(0).localRotation = Quaternion.Euler(90, 0, 90);
            }
            else if (Right_Pressed)
            {
                Hand = GameObject.Find("RightHand");
                transform.GetChild(0).localRotation = Quaternion.Euler(90, 0, -90);
            }
        }
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
        Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;
        Hand_Mesh.transform.GetChild(0).gameObject.SetActive(false);
        Hand_Mesh.transform.GetChild(2).gameObject.SetActive(true);
        Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Grab");
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
        Hand_Mesh.transform.GetChild(0).gameObject.SetActive(true);
        Hand_Mesh.transform.GetChild(2).gameObject.SetActive(false);
        Hand_Mesh.transform.parent = Hand.transform.GetChild(4);
    }

    public void OnActivated(NXREntity.Hand hand)
    {
        Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Action");
        StartCoroutine(Forceps_Action());
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
        Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Release");
        StartCoroutine(Forceps_Release());
    }

    IEnumerator Forceps_Action()
    {
        float Timer = 0;
        while (Timer < 0.5)
        {
            Timer += Time.deltaTime;
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.up * -18), Quaternion.Euler(Vector3.zero), Timer * 2);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.up * 18), Quaternion.Euler(Vector3.zero), Timer * 2);
            yield return null;
        }
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.zero);
    }

    IEnumerator Forceps_Release()
    {
        float Timer = 0;
        while (Timer < 0.5)
        {
            Timer += Time.deltaTime;
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(Vector3.up * -18), Timer * 2);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(Vector3.up * 18), Timer * 2);
            yield return null;
        }
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
    }
}
