using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;

public enum eDilatorType { Small = 0, Middle = 1, Large = 2 }

public class NXR_Dilator : NXR_Insertable
{
    public SpinePoint SpinePoint;
    
    [SerializeField]
    private eDilatorType dilatorType;
    public eDilatorType DilatorType => dilatorType;

    public NXR_Kwire curKwire;

    bool Complete = false;
    bool Insertable;

    protected override void Initialized()
    {
        
    }

    public override void OnUngrabbed(NXREntity.Hand hand)
    {
        if(!isUsable)
            return;
        
        base.OnUngrabbed(hand);
        
        // 삽입을 완료했고 grab을 해제했으며 Large라면
        if (SpinePoint && isInsertComplete && dilatorType == eDilatorType.Large)
        {
            var small = SpinePoint.listDilator.Find( x => x.dilatorType == eDilatorType.Small);
            if (small)
            {
                small.SetInteractable(true);
                entity.SetInteractLayer("Nothing");
            }
        }
    }

    protected override void InsertFlow()
    {
        transform.position += transform.forward * Time.deltaTime * moveVelocity;
            
        float distance = Vector3.Distance(transform.position, targetEndPos);
        if (distance <= allowDistance)
        {
            isMovable = false;
            isInsertComplete = true;
            SpinePoint.listDilator.Add(this);

            //조건 충족시 강제 이동 함수
            Debug.Log("Dilator");
            curKwire.MoveToObject(transform.position, targetEndPos);

            entity.ShowTooltip($"{gameObject.name} 삽입 완료");
            Hand_Out();
            isActivated = false;
            // 큰 Dilator가 삽입 완료됐으면, 작은 것을 제거해야 하기에 kwire 콜라이더 비활성화 
            if (dilatorType == eDilatorType.Large)
                curKwire.SetEnabledColliders(false);
            Enable_Dilator_Collider(false);
            if (dilatorType == eDilatorType.Large)
            {
                SpinePoint.listDilator[0].Enable_Dilator_Collider(true);
            }
            Insertable = false;
        }
    }

    protected override void RemoveFlow()
    {
        transform.position += -transform.forward * Time.deltaTime * moveVelocity;
        float distance = Vector3.Distance(transform.position, targetStartPos);
        if (!GetComponent<XRGrabInteractable>().trackPosition && transform.position.y > curKwire.transform.GetChild(3).position.y)
        {
            entity.EnableTrack(true);
            SpinePoint.listDilator.Remove(this);
            entity.ShowTooltip($"{gameObject.name} 제거 완료");

            switch (dilatorType)
            {
                case eDilatorType.Small:
                    SpinePoint.listDilator[0].SetInteractable(true);
                    SpinePoint.listDilator[0].Enable_Dilator_Collider(true);
                    break;
                case eDilatorType.Middle:
                    SpinePoint.isRemoveSmallDilator = true;

                    // 중간 Dilator를 제거하면 kwire에 Tappering을 해야 하기에 콜라이더 활성화
                    curKwire.SetEnabledColliders(true);

                    var large = SpinePoint.listDilator.Find(x => x.dilatorType == eDilatorType.Large);
                    if (large)
                        large.entity.SetInteractLayer("Default");
                    break;
                case eDilatorType.Large:
                    // 큰 Dilator를 제거하면 kwire에 Tappering을 해야 하기에 콜라이더 활성화
                    curKwire.SetEnabledColliders(true);
                    Destroy(GameObject.Find("Tapper_Pin"));
                    SpinePoint.isDilatorComplete = true;
                    break;
            }
            Complete = true;
            SpinePoint = null;
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

    private void OnTriggerEnter(Collider other)
    {
        if(!isUsable)
            return;
        
        if (!SpinePoint && other.CompareTag("K-wire"))
        {
            curKwire = Util.FindParent<NXR_Kwire>(other.gameObject);

            // spinePoint에 jamshidi를 이용하여 k-wire를 삽입하고 jamshidi 제거까지 완료했다면 
            if (curKwire && curKwire.SpinePoint && curKwire.SpinePoint.isJamshidiComplete)
            {
                Insertable = false;
                int dilatorCnt = curKwire.SpinePoint.listDilator.Count;

                // 작은 Dilator부터 삽입돼야 하기 때문에, 큰 것이 삽입된 상태라면 삽입되지 않음
                // 반대로 자신이 큰 것이라면, 이미 삽입된 것이 있어야 삽입
                switch (dilatorType)
                {
                    case eDilatorType.Small:
                        if (dilatorCnt == 0)
                            Insertable = true;
                        break;
                    case eDilatorType.Middle:
                        if (dilatorCnt == 1)
                            Insertable = true;
                        break;
                    case eDilatorType.Large:
                        if (dilatorCnt == 2)
                        {
                            Insertable = true;
                            curKwire.SetEnabledColliders(false);
                        }
                        break;
                }
                if (!Complete && Insertable)
                {
                    SpinePoint = curKwire.SpinePoint;
                    TriggerEnter();
                    targetStartPos = SpinePoint.transform.position;
                    targetEndPos = SpinePoint.EndTs.position;
            
                    isMovable = true;
                    entity.EnableTrack(false);
                    entity.grabMode = NXREntity.GrabMode.Direct;

                    transform.position = curKwire.AttachTs.transform.position;
                    transform.rotation = curKwire.transform.rotation;
                    transform.LookAt(SpinePoint.EndTs.position);
                }
            }
        }
    }

    public void Enable_Dilator_Collider(bool TF)
    {
        transform.GetChild(0).GetComponent<CapsuleCollider>().enabled = TF;
        transform.GetChild(2).GetComponent<SphereCollider>().enabled = TF;
    }

    public void SetInteractable(bool isInteractable)
    {
        isMovable = isInteractable;
        isInsert = !isInteractable;
        
        entity.ShowTooltip($"{gameObject.name} is inseractable {isInteractable}");
        entity.SetInteractLayer(isInteractable ? "Default" : "Nothing");
    }
}
