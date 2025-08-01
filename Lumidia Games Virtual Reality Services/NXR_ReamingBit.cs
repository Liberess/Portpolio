using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NXR_ReamingBit : NetworkBehaviour, IEntityInteractable
{
    private NXREntity entity;
    
    /// <summary>
    /// 리밍 비트의 크기
    /// </summary>
    [SerializeField]
    private float bitSize;
    public float BitSize => bitSize;

    private void Start()
    {
        entity = GetComponent<NXREntity>();
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        entity.ShowTooltip($"Bit Size : {bitSize}");
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
}
