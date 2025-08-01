using System;
using System.Collections;
using System.Collections.Generic;
using Consts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_Clamp : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;

    /// <summary>
    /// 원래 씬에 배치된 오브젝트인지
    /// </summary>
    public bool IsSceneObj { get; private set; }

    /// <summary>
    /// 현재 차단하고 있는지
    /// </summary>
    public bool isBlocking = false;

    public eClampState state = eClampState.None;
    
    private static int blockCount = 0;
    private static readonly string[] NamesToCheck = { "Clamp_1", "Clamp_2", "Clamp_3", "Clamp_4", "Clamp_5" };

    private void Awake()
    {
        foreach (var name in NamesToCheck)
        {
            if (this.name == name)
            {
                IsSceneObj = true;
                break;
            }
        }
    }

    private void Start()
    {
        entity = GetComponent<NXREntity>();
        
        if(IsSceneObj)
            return;
        
        bool nameSet = false;

        foreach (var name in NamesToCheck)
        {
            if (GameObject.Find(name) == null)
            {
                gameObject.name = name;
                nameSet = true;
                state = eClampState.Ready;
                break;
            }
        }

        if (!nameSet)
        {
            Debug.Log(gameObject.name + "에 적합한 이름을 찾을 수 없습니다. 추가 처리가 필요합니다.");
            Destroy(gameObject);
        }
    }

    private void AttachBlockPoint(Collider other)
    {
        if (other.name.Length > 6 && other.name.Substring(0, 7) == "Dummy00")
        {
            int index = int.Parse(other.name.Substring(other.name.Length - 1));
            HandleFirstBlock(index, other);
        }
        else if (other.name.Length > 12 && other.name.Substring(other.name.Length - 12, 12) == "Attach_Point")
        {
            string firstChar = other.name.Substring(0, 1);
            HandleSecondBlock(firstChar == "L", other);
        }
    }

    private void HandleFirstBlock(int index, Collider other)
    {
        isBlocking = true;
        
        Vector3 targetPos = Vector3.zero;
        Quaternion targetRot = Quaternion.identity;
        string animName = "Blood_Vessel_Block_";
        animName += index.ToString();
        
        switch (index)
        {
            case 1:
                targetPos = new Vector3(1.86090004f,0.977999985f,-0.208299994f);
                targetRot = Quaternion.Euler(359.610077f,268.075378f,29.1986008f);
                break;
            case 2:
                targetPos = new Vector3(1.76139998f,0.932299972f,-0.00749999983f);
                targetRot = Quaternion.Euler(344.779694f,45.3015518f,30.0504704f);
                break;
            case 3:
                targetPos = new Vector3(1.8865f,0.94630003f,0.0151000004f);
                targetRot = Quaternion.Euler(350.58316f,127.279144f,15.0871725f);
                break;
            
            //Clamp_old
            /*case 1:
                targetPos = new Vector3(1.8293f, 0.854f, -0.1223f);
                targetRot = Quaternion.Euler(40.761f, 104.373f, 214.592f);
                break;
            case 2:
                targetPos = new Vector3(1.8933f, 0.8399f, -0.0425f);
                targetRot = Quaternion.Euler(-235.35f, 94.619f, 0f);
                break;
            case 3:
                targetPos = new Vector3(1.8219f, 0.8355f, -0.038f);
                targetRot = Quaternion.Euler(47.8217697f,101.415466f,177.322601f);
                break;*/
        }
        
        HandleEnabled(false);
        
        transform.position = targetPos;
        transform.rotation = targetRot;
        
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -5);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 5);

        ++blockCount;
        state = eClampState.Blocking;
        
        if (blockCount >= 3)
            FindObjectOfType<NXR_Aorta_CS>().isBlockClamp_First = true;
        
        other.GetComponentInParent<Animation>().Play(animName);
        other.gameObject.SetActive(false);
    }

    private void HandleSecondBlock(bool isLeft, Collider other)
    {
        isBlocking = true;
        
        Vector3 targetPos = Vector3.zero;
        Quaternion targetRot = Quaternion.identity;
        
        if (isLeft)
        {
            targetPos = new Vector3(1.83179998f,0.954900026f,0.00039999999f);
            targetRot = Quaternion.Euler(2.92591333f,65.8073349f,22.8121033f);
        }
        else
        {
            targetPos = new Vector3(1.90269995f,0.94599998f,-0.0217000004f);
            targetRot = Quaternion.Euler(7.50173855f,112.163406f,17.4481697f);
        }
        
        HandleEnabled(false);
        
        // Step.11의 좌,우측 limb 문합 과정이 아니라면
        if (state != eClampState.BlockAgain)
        {
            state = eClampState.Blocking;
            ++other.GetComponentInParent<NXR_Aorta_CS>().blockYGraftCount;
            other.gameObject.SetActive(false);
        }
        
        transform.position = targetPos;
        transform.rotation = targetRot;
        
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -5);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 5);
    }

    public void HandleEnabled(bool value)
    {
        if (entity == null)
            entity = GetComponent<NXREntity>();
        
        entity.EnableTrack(value);
        entity.SetInteractLayer(value ? "Default" : "Nothing");

        if (Hand_Mesh && Hand)
        {
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
            Hand_Mesh.transform.GetChild(0).gameObject.SetActive(true);
            Hand_Mesh.transform.GetChild(2).gameObject.SetActive(false);
            Hand_Mesh.transform.parent = Hand.transform.GetChild(4);
            Hand_Mesh = null;
        }
        
        App.Instance.UngrabAll(App.Instance.xrLeftHandDirectInteractor);
        App.Instance.UngrabAll(App.Instance.xrRightHandDirectInteractor);
    }

    public void SetGrab(GameObject rightHand)
    {
        Hand = rightHand;
        transform.GetChild(0).localRotation = Quaternion.Euler(90, 0, -90);
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
        if (Hand_Mesh)
        {
            Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;
            Hand_Mesh.transform.GetChild(0).gameObject.SetActive(false);
            Hand_Mesh.transform.GetChild(2).gameObject.SetActive(true);
            Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Grab");
        }
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

            if (Hand && !Hand_Mesh)
            {
                Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
                if (Hand_Mesh)
                {
                    Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;
                    Hand_Mesh.transform.GetChild(0).gameObject.SetActive(false);
                    Hand_Mesh.transform.GetChild(2).gameObject.SetActive(true);
                    Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Grab");
                }
            }
            
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
        }
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (Hand_Mesh)
        {
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
            Hand_Mesh.transform.GetChild(0).gameObject.SetActive(true);
            Hand_Mesh.transform.GetChild(2).gameObject.SetActive(false);
            Hand_Mesh.transform.parent = Hand.transform.GetChild(4);
            Hand_Mesh = null;
        }
    }

    public void OnActivated(NXREntity.Hand hand)
    {
        if (Hand_Mesh)
        {
            Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Action");
            StartCoroutine(Forceps_Action());
        }
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
        if (Hand_Mesh)
        {
            Hand_Mesh.transform.GetChild(2).GetComponent<Animation>().Play("Hand_Forceps_Release");
            StartCoroutine(Forceps_Release());
        }
    }

    private IEnumerator Forceps_Action()
    {
        float timer = 0;
        while (timer < 0.5)
        {
            timer += Time.deltaTime;
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.up * -18), Quaternion.Euler(Vector3.zero), timer * 2);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.up * 18), Quaternion.Euler(Vector3.zero), timer * 2);
            yield return null;
        }
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.zero);
    }

    private IEnumerator Forceps_Release()
    {
        float timer = 0;
        while (timer < 0.5)
        {
            timer += Time.deltaTime;
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(Vector3.up * -18), timer * 2);
            transform.GetChild(0).GetChild(1).localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(Vector3.up * 18), timer * 2);
            yield return null;
        }
        transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -18);
        transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 18);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsSceneObj && !isBlocking)
            AttachBlockPoint(other);
        
        if(state == eClampState.BlockAgain)
            AttachBlockPoint(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (isBlocking)
        {
            if (other.name == "Dummy001")
            {
                isBlocking = false;
                other.gameObject.SetActive(false);
            }
            else if (other.name == "Right_Graft_Attach_Point" || other.name == "Left_Graft_Attach_Point")
            {
                isBlocking = false;
                if(state == eClampState.Complete)
                    other.gameObject.SetActive(false);
            }
        }
    }
}
