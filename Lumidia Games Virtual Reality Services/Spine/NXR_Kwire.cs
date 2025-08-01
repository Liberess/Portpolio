using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_Kwire : NXR_Insertable
{
    public SpinePoint SpinePoint;
    
    /// <summary>
    /// 오브젝트가 k-wire에 붙을 위치 
    /// </summary>
    [SerializeField]
    private Transform attachTs;
    public Transform AttachTs => attachTs;

    private List<BoxCollider> boxCols;
    private SphereCollider sphereCol;

    protected override void Initialized()
    {
        boxCols = GetComponentsInChildren<BoxCollider>().ToList();
        sphereCol = GetComponentInChildren<SphereCollider>();
    }

    protected override void InsertFlow()
    {
        transform.position += transform.forward * Time.deltaTime * moveVelocity;
            
        float distance = Vector3.Distance(transform.position, targetEndPos);
        if (distance <= allowDistance)
        {
            isMovable = false;
            isInsertComplete = true;
            SpinePoint.isInsertKwire = true;
            Hand_Out();
            //조건 충족시 강제 이동 함수
            Debug.Log("K-Wire");
            MoveToObject(transform.position, SpinePoint.EndTs.position);
            SetEnabledColliders(false);
            entity.ShowTooltip($"K-wire 삽입 완료");
        }
    }

    protected override void RemoveFlow()
    {
        transform.position += -transform.forward * Time.deltaTime * moveVelocity;
            
        float distance = Vector3.Distance(transform.position, targetStartPos);
        if (distance <= allowDistance)
        {
            isUsable = false;
            entity.EnableTrack(true);
            SpinePoint.isInsertKwire = false;            
            entity.ShowTooltip($"K-wire 제거 완료");
            StartCoroutine(Destroy());
        }
    }
    
    IEnumerator Destroy()
    {
        Hand_Out();
        float Timer = 0;
        while (Timer < 0.1)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    
    public void SetRemovable()
    {
        isMovable = true;
        isInsert = false;
    }

    public void SetEnabledColliders(bool enabled)
    {
        Debug.Log($"SetEnabledColliders : {enabled}");
        foreach (var boxCol in boxCols)
            boxCol.enabled = enabled;
        sphereCol.enabled = enabled;
    }

    public void MoveToObject(Vector3 currentPos, Vector3 targetPos)
    {
        float distance = Vector3.Distance(currentPos, targetPos);
        Debug.Log("두 오브제 사이값 : " + distance);
        //0.005 == 오차허용값
        if (!(distance < 0.005f))
        {
            Debug.Log("위치이동 전 : " + currentPos);

            Vector3 direction = (targetPos - currentPos).normalized;
            Vector3 newPosition = targetPos - direction * 0.005f;
            currentPos = newPosition;
            Debug.Log("두 오브제 사이값 : " + Vector3.Distance(currentPos, targetPos));
            Debug.Log("위치이동 후 : " + currentPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!isUsable)
            return;
        
        if (!SpinePoint && other.CompareTag("JamshidiNeedle"))
        {
            var needle = Util.FindParent<NXR_JamshidiNeedle>(other.gameObject);

            if (needle && needle.IsRemovePin && needle.SpinePoint)
            {
                SpinePoint = needle.SpinePoint;

                isInsert = true;
                
                targetStartPos = SpinePoint.transform.position;
                targetEndPos = SpinePoint.EndTs.position;
            
                isMovable = true;
                entity.EnableTrack(false);
                entity.grabMode = NXREntity.GrabMode.Direct;
                transform.position = needle.AttachTs.transform.position;
                transform.rotation = needle.transform.rotation;
                TriggerEnter();
            }
        }
    }

    
}
