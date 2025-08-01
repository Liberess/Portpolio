using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NXR_Rod : NetworkBehaviour
{
    private NXREntity entity;

    private void Awake()
    {
        entity = GetComponent<NXREntity>();
    }

    public void SetCalm()
    {
        entity.EnableTrack(false);
        entity.SetInteractLayer("Nothing");
        GetComponentInChildren<MeshCollider>().enabled = false;
        GetComponentInChildren<BoxCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RodHolder"))
        {
            var holder = Util.FindParent<NXR_RodHolder>(other.gameObject);
            if (holder)
                holder.rod = this;
        }
    }
}
