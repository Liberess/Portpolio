using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NXR_Mess : NetworkBehaviour, IEntityInteractable
{
    private GameObject curPatient;
    
    /// <summary>
    /// 플레이어가 Scalpel를 쥐고 있는가?
    /// </summary>
    private bool isActive = false;

    /// <summary>
    /// 현재 절개를 진행하는 중인가?
    /// </summary>
    private bool isIncising = false;

    /// <summary>
    /// 절개를 완료했는가?
    /// </summary>
    public bool IsIncision { get; private set; } = false;
    public void SetIncision(bool value) => IsIncision = value;

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        isActive = true;
        curPatient = null;
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        isActive = false;
    }

    public void OnActivated(NXREntity.Hand hand)
    {
    
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {
      
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsIncision)
            return;
        
        if(other.gameObject == this.gameObject)
            return;
        
        // 활성화된(그립) 상태가 아니거나 이미 환자와 상호작용 중이라면 종료
        if(!isActive || curPatient != null)
            return;

        //if (other.CompareTag("Patient"))
        //{
        //    curPatient = other.transform.root.gameObject;
        //    StartCoroutine(ProcessIncise());
        //}
    }

    private IEnumerator ProcessIncise()
    {
        isIncising = true;
        
        float time = 0.0f;
        while (isIncising)
        {
            time += Time.deltaTime;

            if (time >= 2.0f)
            {
                IsIncision = true;
                isIncising = false;
            }
            
            yield return null;
        }
        
        yield return null;
    }

    private void OnTriggerExit(Collider other)
    {
        if(IsIncision)
            return;
        
        if(other.gameObject == this.gameObject)
            return;
        
        if(curPatient == null)
            return;

        //if (other.CompareTag("Patient"))
        //{
        //    if (curPatient == other.transform.root.gameObject)
        //    {
        //        curPatient = null;
        //        isIncising = false;
        //        StopCoroutine(ProcessIncise());
        //    }
        //}
    }
}
