using System.Collections;
using UnityEngine;

public class NXR_ReamingRod : MonoBehaviour
{
    public bool Step9_End = false;
    private void OnTriggerEnter(Collider other)
    {
        if (GetComponent<NXR_Hand_Input>().Out)
        {
            return;   
        }
        if (GetComponent<NXR_Hand_Input>().Auto_Snap && other.name == "GuidePinInsertPoint")
        {
            {
                GetComponent<NXR_Hand_Input>().Auto_Snap = false;
                transform.rotation = Quaternion.LookRotation(GameObject.Find("InsertEndPoint").transform.position - other.transform.position);
                transform.position = other.transform.position - (transform.GetChild(2).position - transform.position);
            }
        }
        if (other.name == "InsertEndPoint")
        {
            GetComponent<NXR_Hand_Input>().End_Insert = true;
            Step9_End = true;
            switch (GetComponent<NXR_Hand_Input>().Hand.name)
            {
                case "LeftHand":
                    App.Instance.UngrabAll(App.Instance.xrLeftHandDirectInteractor);
                    break;
                case "RightHand":
                    App.Instance.UngrabAll(App.Instance.xrRightHandDirectInteractor);
                    break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (GetComponent<NXR_Hand_Input>().Out && other.name == "GuidePinInsertPoint" && transform.position.z < other.transform.position.z)
        {
            GetComponent<NXR_Hand_Input>().End_Insert = true;
            GameObject.Find("NailHandle_Up").GetComponent<NXR_NailHandle>().ReamingRod_Out();
            GetComponent<NXR_Hand_Input>().Out = false;
            StartCoroutine(Delay());
        }
    }
    IEnumerator Delay()
    {
        float Timer = 0;
        while (Timer < 0.5)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

        public IEnumerator Set_ReamingRod()
    {
        GetComponent<NXR_Hand_Input>().Out = true;
        while (Vector3.Distance(transform.position, new Vector3(-2.054544f, 0.9427527f, -1.277528f)) > 0.01)
        {
            transform.position = new Vector3(-2.054544f, 0.9427527f, -1.277528f);
            transform.rotation = Quaternion.Euler(-9.29f, 16.916f, 0);
            yield return null;
        }
    }
}