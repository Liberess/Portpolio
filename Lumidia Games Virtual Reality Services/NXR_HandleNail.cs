using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class NXR_HandleNail : MonoBehaviour
{
    //NailHandle과 충돌되어있는지 확인
    //충돌 중이라면 NailHandle을 부모로 지정 
    //충돌에서 벗어나면 NailHandle에서 벗어남

    public bool IsAttached = false;
    public GameObject M_obj;
    public bool isNailHandleCol = false; // 충돌 상태를 저장할 변수
    public bool isCol = true;

    private void Update()
    {
        if (isNailHandleCol)
        {
            transform.SetParent(M_obj.transform);
            transform.localScale = Vector3.one;
        }
        else if(!isNailHandleCol)
        {
            transform.SetParent(null);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Femur") && other.transform.name == "InsertEndPoint")
        {
            isCol = false;
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("NailHandle"))
        {
            isNailHandleCol = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NailHandle"))
        {
            isNailHandleCol = false;   
        }
    }
}
