using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System.Threading;

public class NXR_JamshidiNeedle : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;

    public SpinePoint SpinePoint { get; private set; }

    /// <summary>
    /// 해머질을 할 수 있는지. false라면 해머질 완료
    /// </summary>
    public bool CanHammering { get; private set; } = true;

    /// <summary>
    /// 오브젝트가 Needle에 붙을 위치 
    /// </summary>
    [SerializeField] private Transform attachTs;
    public Transform AttachTs => attachTs;

    [SerializeField] public GameObject pin;
    private NXREntity pinEntity;

    /// <summary>
    /// pin을 제거할 수 있는지
    /// </summary>
    private bool isRemovablePin = false;

    /// <summary>
    /// 트리거를 당겨 pin을 제거하고 있는지
    /// </summary>
    private bool isRemovingPin = false;

    /// <summary>
    /// pin을 제거했는지
    /// </summary>
    public bool IsRemovePin = false;

    /// <summary>
    /// Jamshidi Needle을 제거할 수 있는지
    public bool Removable_Jamshidi_Needle = false;
    bool Remove_Jamshidi_Needle_Start = false;

    /// SpinePoint에 닿아, Hammering하기 전의 처음 위치
    /// </summary>
    private Vector3 insertStartPos;
    public bool First = true;
    public bool Is_Done = false;
    NXR_Kwire kwire;

    private void Awake()
    {
        entity = GetComponent<NXREntity>();
        CanHammering = false;
        StartCoroutine(Hand_Decision());
    }

    IEnumerator Hand_Decision()
    {
        while (GetComponent<NetworkBehaviour>().Hand == null)
            yield return null;
        Hand = GetComponent<NetworkBehaviour>().Hand;
    }

    public void Set_Pin()
    {
        pin.GetComponentInChildren<BoxCollider>().enabled = false;
        pin.transform.SetParent(transform);
        pinEntity = pin.GetComponent<NXREntity>();
        pinEntity.SetInteractLayer("Nothing");
        pinEntity.EnableTrack(false);
    }

    void Update()
    {
        if (Remove_Jamshidi_Needle_Start)
            Remove_Jamshidi_Needle();
    }


    private void FixedUpdate()
    {
        if (pin && !IsRemovePin)
        {
            if (isRemovablePin && isRemovingPin)
            {
                pin.transform.position -= pin.transform.forward * 0.1f * Time.deltaTime;
                var vec = (pin.transform.position - attachTs.position).normalized;
                float distance = Vector3.Distance(attachTs.position, pin.transform.position);
                if (distance <= 0.01f && vec.y >= 0)
                {
                    IsRemovePin = true;
                    pin.transform.GetChild(2).parent = transform;
                    pin.transform.SetParent(null);
                    entity.ShowTooltip("Jamshidi Needle Pin 제거 완료");
                    Debug.Log("Jamshidi Needle Pin 제거 완료");
                    Removable_Jamshidi_Needle = true;
                    StartCoroutine(Destroy(pin.gameObject));
                    pin = null;
                }
            }
            else
            {
                pin.transform.localPosition = Vector3.zero;
                pin.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
        else if (transform.GetChild(3).transform.localPosition != Vector3.forward * -0.214f)
        {
            transform.GetChild(3).transform.localPosition = Vector3.forward * -0.214f;
        }
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        entity.ShowTooltip($"Grab {gameObject.name}");
        if (pin && isRemovablePin && First)
        {
            First = false;
            transform.GetChild(3).localPosition = Vector3.forward * -0.24f;
            transform.GetChild(3).parent = pin.transform;
        }
        if (!Removable_Jamshidi_Needle)
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
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (Removable_Jamshidi_Needle)
            Remove_Jamshidi_Needle_Start = false;
    }

    public void OnActivated(NXREntity.Hand hand)
    {
        if (entity.IsGrabbedByMe() && pin && isRemovablePin && !IsRemovePin)
        {
            isRemovingPin = true;
        }
        kwire = SpinePoint.Kwire;
        if (Removable_Jamshidi_Needle)
            Remove_Jamshidi_Needle_Start = true;
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
        if (isRemovingPin)
        {
            isRemovingPin = false;
        }
        if (Removable_Jamshidi_Needle)
            Remove_Jamshidi_Needle_Start = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnter(other);
    }

    public void TriggerEnter(Collider other)
    {
        // JamshidiNeedle Pin이 삽입된 상태가 아니라면 종료
        if (!pin)
            return;

        if (!SpinePoint && other.TryGetComponent(out SpinePoint spinePoint))
        {
            if (spinePoint.SpineLevel == 1)
            {
                entity.EnableTrack(false);
                entity.grabMode = NXREntity.GrabMode.Direct;
                SpinePoint = spinePoint;
                transform.position = SpinePoint.transform.position;
                transform.rotation = Quaternion.Euler(74.833f, 142.879f, -33.919f);

                insertStartPos = transform.position;

                entity.ShowTooltip($"Level_{SpinePoint.SpineLevel}에 Jamshidi Needle 삽입 시작");

                if (Hand != null)
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
                CanHammering = true;
            }
        }

        if (CanHammering && other.CompareTag("Hammer") && SpinePoint)
        {
            Co_InsertByHammer();
        }
    }

    /// <summary>
    /// Hammer로 내리쳐서 박히는 움직임 코루틴
    /// </summary>
    void Co_InsertByHammer()
    {
            transform.Translate(Vector3.forward * 1.05f * Time.deltaTime);

            float distance = Vector3.Distance(transform.position, SpinePoint.EndTs.position);
            float gap = Vector3.Distance(transform.position, insertStartPos);
            if (distance <= 0.005f || gap >= 0.03f)
            {
                isRemovablePin = true;
                CanHammering = false;

                //조건 충족시 강제 이동 함수
                Debug.Log("JamshidiNeedle");
                MoveToObject(transform.position, SpinePoint.EndTs.position);

                entity.ShowTooltip($"Level_{SpinePoint.SpineLevel}에 Jamshidi Needle 삽입 완료");
            }
    }

    void Remove_Jamshidi_Needle()
    {
        if (transform.position.y < kwire.transform.GetChild(3).position.y)
        {
            transform.position -= transform.forward * Time.deltaTime * 0.2f;
            return;
        }
        entity.EnableTrack(true);
        Is_Done = true;
        GameObject.Find("K-wire").GetComponent<NXR_Kwire>().SetEnabledColliders(true);
        StartCoroutine(Destroy(gameObject));
    }

    IEnumerator Destroy(GameObject gameObject)
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
        float Timer = 0;
        while (Timer < 0.1)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private void MoveToObject(Vector3 currentPos, Vector3 targetPos)
    {
        float distance = Vector3.Distance(currentPos, targetPos);
        //0.005 == 오차허용값
        if (!(distance < 0.005f))
        {
            Vector3 direction = (targetPos - currentPos).normalized;
            Vector3 newPosition = targetPos - direction * 0.005f;
            currentPos = newPosition;
        }
    }
}
