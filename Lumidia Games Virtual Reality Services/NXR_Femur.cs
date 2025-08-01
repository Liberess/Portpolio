using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class NXR_Femur : NetworkBehaviour, IEntityInteractable
{
    private NXR_GuidePin curGuidePin;
    private NXR_ReductionFinger curFinger;
    private NXREntity entity;

    //[SerializeField]
    //private Transform[] arrFemur;

    [SerializeField, Range(0.1f, 3.0f)]
    private float distributedAmount = 2.0f;
    public float DistributedAmount => distributedAmount;

    /// <summary>
    /// 목표 드릴 비트 사이즈
    /// </summary>
    [SerializeField]
    private float targetBitSize = 8.5f;
    public float TargetBitSize => targetBitSize;

    /// <summary>
    /// femur의 내부 직경
    /// </summary>
    [SerializeField]
    private float diameter = 9.0f;
    public float Diameter => diameter;

    // 리밍 로드가 리밍 중인가
    public bool IsReaming { get; private set; }
    
    // finger를 이용하여 골편을 매칭시키고 있나
    public bool IsFingering { get; private set; }
    public void SetFingering(bool fingering) => IsFingering = fingering;

    // 골절되어 절단된 골편을 매칭 시켰나
    public bool IsMatching { get; private set; }

   // public Transform DividedFemurTs => arrFemur[2].transform; 

    public Quaternion InitialRot { get; private set; }

    /// <summary>
    /// 가이드 핀 삽입을 완료했는가
    /// </summary>
    [HideInInspector] public bool isInsertGuidePinComplete = false;

    /// <summary>
    /// femur의 삽입 시작 위치
    /// </summary>
    [SerializeField] private Transform insertStartTs;
    public Transform InsertStartTs => insertStartTs;

    /// <summary>
    /// femur의 삽입 종료 위치
    /// </summary>
    [SerializeField] private Transform insertEndTs;
    public Transform InsertEndTs => insertEndTs;

    /// <summary>
    /// 가이드 핀이 삽입된 위치값
    /// </summary>
    public Vector3 GuidePinInsertPos { get; private set; }

    /// <summary>
    /// 가이드 핀이 삽입된 회전값
    /// </summary>
    public Quaternion GuidePinInsertRot { get; private set; }


    private void Start()
    {
        entity = GetComponent<NXREntity>();

        IsReaming = false;
        IsMatching = false;
        IsFingering = false;
        
        // 골절되어 절단된 골편을 랜덤한 값으로 회전시킴
        //InitialRot = arrFemur[2].transform.rotation;
        float xRotation = Random.Range(-distributedAmount, distributedAmount);
        float yRotation = Random.Range(-distributedAmount, distributedAmount);
        float zRotation = Random.Range(-distributedAmount, distributedAmount);
       // arrFemur[2].transform.Rotate(new Vector3(xRotation, yRotation, zRotation));
    }

    private void FixedUpdate()
    {
        if (!IsReaming || IsMatching || !IsFingering)
            return;

        //float gap = Quaternion.Angle(arrFemur[2].transform.rotation, InitialRot);
        //if (gap <= 0.2f)
        //{
        //    entity.ShowTooltip($"절단된 골편을 올바르게 회전 완료");
        //
        //    arrFemur[2].transform.rotation = InitialRot;
        //    
        //    IsMatching = true;
        //    IsFingering = false;
        //
        //    //if(curGuidePin)
        //    //{
        //    //    curGuidePin.SetInsertAble(true);
        //    //}
        //
        //    curFinger.SetEnabledGrab(true);
        //    curFinger.SetComplete(true);
        //}
        //else
        //{
        //    entity.ShowTooltip($"골편의 회전값 오차 : {gap}");
        //}
    }
   
    
    public void SetReaming(bool reaming, NXR_GuidePin guidePin)
    {
        IsReaming = reaming;
        curGuidePin = guidePin;
    }

    public void SetFinger(NXR_ReductionFinger finger) => curFinger = finger;

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        
    }

    public void OnActivated(NXREntity.Hand hand)
    {
        
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Collider")
        {
            collision.transform.parent.GetComponent<NXR_GuidePin>().speed = 0.1f;
        }
        else
        {
            collision.transform.parent.GetComponent<NXR_GuidePin>().speed = -0.1f;
        }
    }

    /// <summary>
    /// 삽입된 가이드 핀의 처음 위치 및 회전값
    /// </summary>
    public void SetGuidePin(NXR_GuidePin guidePin)
    {
        curGuidePin = guidePin;
        GuidePinInsertPos = curGuidePin.transform.position;
        GuidePinInsertRot = curGuidePin.transform.rotation;
    }
}
