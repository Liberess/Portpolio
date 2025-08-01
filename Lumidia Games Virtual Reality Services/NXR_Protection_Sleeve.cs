using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NXR_Protection_Sleeve : NetworkBehaviour
{
    void Start()
    {
        if(name == "Protection_sleeve")
        {
            return;
        }
        else if (GameObject.Find("Protectsleeve_A"))
        {
            name = "Protectsleeve_B";
            transform.GetChild(0).name = "Protectsleeve_B";
            GameObject.Find("NailHandle_Up").GetComponent<NXR_NailHandle>().PTSB = gameObject;
            transform.GetChild(0).GetChild(4).GetComponent<SphereCollider>().enabled = false;
        }
        else
        {
            name = "Protectsleeve_A";
            transform.GetChild(0).name = "Protectsleeve_A";
            GameObject.Find("NailHandle_Up").GetComponent<NXR_NailHandle>().PTSA = gameObject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "NHD_Hole_C")
        {
            Set2(other);
        }
        if (other.name != "NHD_Hole_A" && other.name != "NHD_Hole_B")
        {
            return;
        }
        Set(other);
    }

    public void Set(Collider other)
    {
        other.GetComponent<SphereCollider>().enabled = false;
        GameObject.Find("NailHandle_Up").GetComponent<NXR_NailHandle>().PTS_Insert(name == "Protectsleeve_A", other.name == "NHD_Hole_A");
        transform.GetChild(0).parent = other.transform;
        other.transform.GetChild(0).localRotation = Quaternion.Euler(Vector3.zero);
        other.transform.GetChild(0).localPosition = Vector3.zero;
        Destroy(gameObject);
    }

    public void Set2(Collider other)
    {
        other.GetComponent<SphereCollider>().enabled = false;
        transform.GetChild(0).parent = other.transform;
        other.transform.GetChild(0).localRotation = Quaternion.Euler(180, 0, 0);
        other.transform.GetChild(0).localPosition = Vector3.zero;
        other.transform.GetChild(0).GetChild(4).GetComponent<SphereCollider>().enabled = true;
        other.transform.root.GetComponent<NXR_NailHandle>().StartCoroutine("Insert_Protection_Sleeve_C",other.transform.GetChild(0).gameObject);
        ScenarioData.tools.Clear();
        ScenarioData.scenarios[2].stepMap.Clear();
        ScenarioData.scenarios[2].stepMap.Add(0, new ScenarioManager.Info.Step() { index = 12, name = "Step 13", tools = new List<string>() { "Drill_Bit_H", "ReamingDrill" } });
        ScenarioData.tools.Add("Drill_Bit_H", new ScenarioData.Tool() { id = "Drill_Bit_H", name = "Drill_Bit_H", returnMode = NXREntity.ReturnMode.Destroy });
        ScenarioData.tools.Add("ReamingDrill", new ScenarioData.Tool() { id = "ReamingDrill", name = "ReamingDrill", returnMode = NXREntity.ReturnMode.None });
        GameObject.Find("XR Origin").GetComponent<Save_Step>().Inventory.Refresh();
        Destroy(gameObject);
    }
}
