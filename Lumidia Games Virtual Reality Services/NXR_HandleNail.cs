using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;

public class NXR_HandleNail : MonoBehaviour
{
    //NailHandle�� �浹�Ǿ��ִ��� Ȯ��
    //�浹 ���̶�� NailHandle�� �θ�� ���� 
    //�浹���� ����� NailHandle���� ���

    public bool IsAttached = false;
    public GameObject M_obj;
    public bool isNailHandleCol = false; // �浹 ���¸� ������ ����
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
