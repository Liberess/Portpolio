using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class NXR_Insertable : NetworkBehaviour, IEntityInteractable
{
    protected NXREntity entity;
    
    /// <summary>
    /// 실제 상호작용이 일어날 트리거 트랜스폼
    /// </summary>
    [SerializeField]
    protected Transform interactTs;

    /// <summary>
    /// 해당 오브젝트를 사용할 수 있는지
    /// </summary>
    protected bool isUsable = false;
    
    /// <summary>
    /// 컨트롤러의 트리거를 당겨 작동시키고 있는지
    /// </summary>
    protected bool isActivated = false;

    /// <summary>
    /// 움직일 수 있는지
    /// </summary>
    public bool isMovable = false;
    
    /// <summary>
    /// 삽입? 삭제?
    /// </summary>
    public bool isInsert = true;
    
    /// <summary>
    /// 삽입이 완료됐는지
    /// </summary>
    public bool isInsertComplete = true;
    
    /// <summary>
    /// 삽입 또는 삭제될 때 움직이는 속도
    /// </summary>
    [SerializeField, Range(0.01f, 2.0f)]
    protected float moveVelocity = 0.05f;

    /// <summary>
    /// 삽입 시작 위치
    /// </summary>
    public Vector3 targetStartPos;
    
    /// <summary>
    /// 삽입 끝 위치
    /// </summary>
    protected Vector3 targetEndPos;

    /// <summary>
    /// target과의 차이에서 허용할 오차값
    /// </summary>
    [SerializeField, Range(0.001f, 0.1f)]
    protected float allowDistance = 0.005f;

    public bool First = true;
    public bool Attached = false;

    protected void Awake()
    {
        entity = GetComponentInChildren<NXREntity>();

        isUsable = true;
        isActivated = false;
        isMovable = false;
        isInsert = true;
        isInsertComplete = false;
        
        Initialized();
        StartCoroutine(Hand_Decision());
    }

    IEnumerator Hand_Decision()
    {
        while (GetComponent<NetworkBehaviour>().Hand == null)
            yield return null;
        Hand = GetComponent<NetworkBehaviour>().Hand;
        Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
    }
    /// <summary>
    /// 해당 Insertable 오브젝트 초기화
    /// </summary>
    protected abstract void Initialized();

    protected void FixedUpdate()
    {
        if(!isUsable)
            return;
        if (Attached && First)
        {
            Grabbing();
        }
        // 트리거를 당기고 있으며, 움직일 수 있는 상태라면
        if (isActivated && isMovable)
        {
            if (isInsert)
            {
                InsertFlow();
            }
            else
                RemoveFlow();
        }
    }

    void Grabbing()
    {
        if (Hand_Mesh == null || GetComponent<XRGrabInteractable>().attachTransform == null)
        {
            return;
        }
        Hand_Mesh.transform.localPosition = GetComponent<XRGrabInteractable>().attachTransform.localPosition;
        Hand_Mesh.transform.localRotation = GetComponent<XRGrabInteractable>().attachTransform.localRotation;
    }

    /// <summary>
    /// 삽입 시 수행할 로직
    /// </summary>
    protected abstract void InsertFlow();
    
    /// <summary>
    /// 제거 시 수행할 로직
    /// </summary>
    protected abstract void RemoveFlow();

    public void TriggerEnter()
    {
        Attached = true;
        entity.PlayVibration(Hand.name);
        Hand_Mesh.transform.parent = GetComponent<XRGrabInteractable>().attachTransform;
    }

    public IEnumerator Set_Transform()
    {
        entity.EnableTrack(false);
        entity.grabMode = NXREntity.GrabMode.Direct;
        Attached = true;
        float Timer = 0;
        while(Timer < 0.5)
        {
            Timer += Time.deltaTime;
            transform.position = GameObject.Find("Level_1").transform.GetChild(0).position;
            transform.rotation = Quaternion.Euler(74.833f, 142.879f, -33.919f);
            yield return null;
        }
    }

    public virtual void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        if (!First || !Attached)
            return;
        entity.ShowTooltip($"Grab {gameObject.name}");
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
            Hand_Mesh = Hand.transform.GetChild(4).GetChild(0).gameObject;
        }
    }

    public virtual void OnUngrabbed(NXREntity.Hand hand)
    {
        if (First && Attached &&Hand_Mesh != null)
        {
            First = false;
            Hand_Mesh.transform.parent = Hand.transform.GetChild(4);
            Hand_Mesh.transform.localPosition = Vector3.zero;
            Hand_Mesh.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public virtual void OnActivated(NXREntity.Hand hand)
    {
        if (entity.IsGrabbedByMe())
        {
            isActivated = true;
        }
    }

    public virtual void OnDeactivated(NXREntity.Hand hand)
    {
        if(entity.IsGrabbedByMe())
            isActivated = false;
    }

    public void Hand_Out()
    {
        switch (Hand.name)
        {
            case "LeftHand":
                App.Instance.UngrabAll(App.Instance.xrLeftHandDirectInteractor);
                break;
            case "RightHand":
                App.Instance.UngrabAll(App.Instance.xrRightHandDirectInteractor);
                break;
        }
    }
}
