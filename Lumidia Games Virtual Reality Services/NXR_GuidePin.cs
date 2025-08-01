using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_GuidePin : NetworkBehaviour
{
    private NXREntity entity;
    private Coroutine CurMoveCoroutine;
    private Rigidbody rb;

    public float speed = 0.1f;

    private Transform origin;
    private Quaternion originRotationDiff = Quaternion.identity;

    private float rotationSnapRange = 60f;
    bool Pin_Attached = false;
    private bool Attached = false;
    private bool IsMove = false;
    public bool Insert_End = false;    
    public Renderer objectRenderer; // ÇØ´ç °´Ã¼ÀÇ Renderer ÄÄÆ÷³ÍÆ®
    Coroutine coroutine;

    /// <summary>
    /// °ñÆí°ú Ã³À½¿¡ ¸Â´êÀ» À§Ä¡(Collision)
    /// </summary>
    [SerializeField] private Transform interactTs;
    
    /// <summary>
    /// Femur¿¡ »ðÀÔ ¿Ï·á?
    /// </summary>
    public bool IsInsertComplete { get; private set; } = false;

    void Awake()
    {
        entity = GetComponent<NXREntity>();
        rb = GetComponent<Rigidbody>();
        if(origin == null)
        {
            origin = transform;
        }
        
        originRotationDiff = Util.Math.Diff(origin.rotation, transform.rotation);
        StartCoroutine(Hand_Decision());
    }

    IEnumerator Hand_Decision()
    {
        while (GetComponent<NetworkBehaviour>().Hand == null)
            yield return null;
        Hand = GetComponent<NetworkBehaviour>().Hand;
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand)
    {
        if (entity.grabMode == NXREntity.GrabMode.Direct)
        {
            coroutine = StartCoroutine(Check());
        } 
    }

    public void OnUngrabbed(NXREntity.Hand hand)
    {
        if (entity.grabMode == NXREntity.GrabMode.Direct)
        {
            StopCoroutine(coroutine);
        }
    }

    public void OnActivated(NXREntity.Hand hand)
    {
    }

    public void OnDeactivated(NXREntity.Hand hand)
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        var collider = other.GetComponent<NXRGuidePinCollider>();

        if(collider == null)
        {
            return;
        }
        Pin_Attached = true;
        entity.EnableTrack(false);
        transform.rotation = Quaternion.LookRotation((GameObject.Find("InsertEndPoint").transform.position - other.transform.position));
        transform.position = other.transform.position - (transform.GetChild(1).position - transform.position);
        Destroy(transform.GetChild(0).GetComponent<CapsuleCollider>());
        coroutine = StartCoroutine(Check());
        if (Attached == false)
        {
            entity.PlayVibration(GetComponent<NXR_Hand_Input>().Hand.name);
            Attached = true;
        }
    }

    IEnumerator Check()
    {
        while (!Insert_End)
        {
            if (transform.position.z > -0.9884064)
            {
                Insert_End = true;
                GetComponent<NXR_Hand_Input>().End_Insert = true;
                transform.position = new Vector3(-1.966616f, 0.9921831f, -0.9884064f);
                entity.PlayVibration(GetComponent<NXR_Hand_Input>().Hand.name);
            }
            yield return null;
        }
    }

    public IEnumerator Set_GuidePin()
    {
        Pin_Attached = true;
        while (Vector3.Distance(transform.position, new Vector3(-1.966616f, 0.9921831f, -0.9884064f)) > 0.01)
        {
            transform.position = new Vector3(-1.966616f, 0.9921831f, -0.9884064f);
            transform.rotation = Quaternion.Euler(-9.29f, 16.916f, 0);
            yield return null;
        }
    }
}
