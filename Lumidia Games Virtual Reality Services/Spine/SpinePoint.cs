using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinePoint : MonoBehaviour
{
    /// <summary>
    /// 해당 포인트가 Spine에서 위치한 레벨
    /// </summary>
    [SerializeField] private int spineLevel = 1;
    public int SpineLevel => spineLevel;

    [SerializeField] private Transform endTs;
    public Transform EndTs => endTs;

    [SerializeField] private SphereCollider[] spCols = new SphereCollider[2];

    public bool isJamshidiComplete = false;
    public bool isInsertKwire = false;
    public bool isDilatorComplete = false;
    public bool isRemoveSmallDilator = false;
    public bool isInsertRod = false;
    public bool isTappingComplete = false;
    public bool isScrew = false;
    
    /// <summary>
    /// 1차 capping 공정이 완료되었는지
    /// </summary>
    public bool isCapping1 = false;
        
    /// <summary>
    /// Tap Breaker 공정이 끝났는지
    /// </summary>
    public bool isComplete_TapBreaker = false;

    /// <summary>
    /// 모든 공정을 완료했는지
    /// </summary>
    public bool isCompleteAll = false;
    
    public List<NXR_Dilator> listDilator = new List<NXR_Dilator>();
    
    public NXR_Kwire Kwire { get; private set; }
    
    private void Start()
    {
        if (!endTs)
            endTs = Util.Find(this.gameObject, "EndPoint").transform;
    }

    public void CompleteScrew()
    {
        isScrew = true;

        if (Kwire)
        {
            Kwire.SetRemovable();
            //kwire 콜라이더 활성화
            //kwire.SetEnabledColliders(true);
        }
    }

    public void CompleteCapping_First()
    {
        isCapping1 = true;  
    }

    /// <summary>
    /// TapBreaker를 이용한 공정 완료
    /// </summary>
    public void Complete_Breaking()
    {
        isComplete_TapBreaker = true;
        isCompleteAll = true;

        foreach (var col in spCols)
            col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCompleteAll)
            return;
        
        if (other.CompareTag("K-wire"))
            Kwire = Util.FindParent<NXR_Kwire>(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (isCompleteAll)
            return;
        
        if (isInsertKwire && other.CompareTag("JamshidiNeedle"))
            isJamshidiComplete = true;

        if (!isTappingComplete && other.CompareTag("TapperPin"))
        {
            isTappingComplete = true;

            var largeDilator = listDilator.Find(
                x => x.DilatorType == eDilatorType.Large);
            if (largeDilator)
                largeDilator.SetInteractable(true);
        }
    }
}
