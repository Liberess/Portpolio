using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NXR_Blood_Clot : MonoBehaviour
{
    public List<GameObject> Clots;
    GameObject[] Finger;
    GameObject Attaching_Finger;
    public bool isRemovable = false;
    public bool Clots_Removed = false;

    void Awake()
    {
        Clots = new List<GameObject>();
        Attaching_Finger = gameObject;
        for (int i = 0; i < transform.childCount; i++)
        {
            Clots.Add(transform.GetChild(i).gameObject);
        }
        Finger = new GameObject[2];
        foreach (GameObject L in GameObject.FindGameObjectsWithTag("Left_Hand"))
        {
            if (L.name == "IndexD_end")
            {
                Finger[0] = L;
                break;
            }
        }
        foreach (GameObject R in GameObject.FindGameObjectsWithTag("Right_Hand"))
        {
            if (R.name == "IndexD_end")
            {
                Finger[1] = R;
                break;
            }
        }
    }
    
    void FixedUpdate()
    {
        if(!isRemovable || Clots_Removed)
            return;
        
        if (Clots.Count > 0)
        {
            Hand_Decision();
            Clot_Distance_Calculate();
        }
        Counting_Clots_Amount();
    }

    public void DestroyClots()
    {
        isRemovable = false;
        
        for (int i = Clots.Count - 1; i >= 0; i--)
        {
            Destroy(Clots[i]);
            Clots.RemoveAt(i);
        }
        
        Clots_Removed = true;
    }

    private void Hand_Decision()
    {
        if (!Finger[0] || !Finger[1])
            return;
        
        if (Vector3.Distance(Finger[0].transform.position, transform.position) < Vector3.Distance(Finger[1].transform.position, transform.position))
        {
            if (!Attaching_Finger.CompareTag("Left_Hand"))
            {
                Attaching_Finger = Finger[0];
            }
        }
        else if (!Attaching_Finger.CompareTag("Right_Hand"))
        {
            Attaching_Finger = Finger[1];
        }
    }

    void Clot_Distance_Calculate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (Vector3.Distance(Clots[i].transform.position, Attaching_Finger.transform.position) < 0.015)
            {
                Clots[i].transform.parent = Attaching_Finger.transform;
                Clots.Remove(Clots[i]);
            }
        }
    }

    void Counting_Clots_Amount()
    {
        if (Clots.Count == 0 && !Clots_Removed)
        {
            Clots_Removed = true;
            StartCoroutine(Delay());
        }
    }

    IEnumerator Delay()
    {
        float Timer = 0;
        while (Timer < 1)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        Timer = 0;
        int Counter = 0;
        int L_Count = Finger[0].transform.childCount;
        int R_Count = Finger[1].transform.childCount;
        while (Finger[0].transform.childCount + Finger[1].transform.childCount > 0)
        {
            Timer += Time.deltaTime;
            if (Timer > 0.025 * Counter)
            {
                Counter++;
                if (Finger[0].transform.childCount > 0)
                {
                    Destroy(Finger[0].transform.GetChild(L_Count - Counter).gameObject);
                }
                if (Finger[1].transform.childCount > 0)
                {
                    Destroy(Finger[1].transform.GetChild(R_Count - Counter).gameObject);
                }
            }
            yield return null;
        }
    }
}
