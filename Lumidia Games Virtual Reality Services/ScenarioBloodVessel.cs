using System.Collections;
using System.Collections.Generic;
using Consts;
using UnityEngine;

public class ScenarioBloodVessel : Scenario
{
    public NXR_Aorta_CS Bloode_Vessel;
    bool TF = false;
    GameObject XR_Origin;
    Save_Step SS;

    class Tool
    {
        public string name;
        public bool active;
        public GameObject gameObject;
        public Vector3 position;
        public Quaternion rotation;
        bool End = false;

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void ResetTransform()
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }

        public void Reset()
        {
            ResetTransform();
        }
    }

    Dictionary<string, Tool> tools = new Dictionary<string, Tool>();

    List<string> toolNames = new List<string>()
    {
        "Clamp_1",
        "Clamp_2",
        "Clamp_3",
        "Clamp_4",
        "Clamp_5",
        "Y_Graft",
    };

    public override void OnStart(ScenarioManager.Info info)
    {
        base.OnStart(info);
        XR_Origin = GameObject.Find("XR Origin");
        SS = XR_Origin.GetComponent<Save_Step>();
        tools.Clear();
        foreach (GameObject Main_Camera in GameObject.FindGameObjectsWithTag("MainCamera"))
        {
            Main_Camera.transform.GetChild(0).gameObject.SetActive(false);
        }
        foreach (var toolName in toolNames)
        {
            var tool = GameObject.Find(toolName);
            if (tool == null)
                continue;

            tools.Add(toolName, new Tool() { name = toolName, gameObject = tool, active = tool.activeSelf, position = tool.transform.position, rotation = tool.transform.rotation });
        }
        ResetTools();
        if (SS.Step != 0)
        {
            StepJump(SS.Step);
        }
        ScenarioData.tools.Clear();
        StartCoroutine(Delay());
    }
    
    IEnumerator Delay()
    {
        float Timer = 0;
        while (Timer < 2)
        {
            Timer += Time.deltaTime;
            yield return null;
        }
        NXRSession.Instance.Press_Button = false;
    }

    public override void OnEnd(ScenarioManager.Info info)
    {
        base.OnEnd(info);
    }
    
    void ResetTools()
    {
        foreach (var tool in tools.Values)
        {
            tool.SetActive(false);
        }
    }

    void StepJump(int stepIndex)
    {
        for (int i = 0; i < stepIndex; i++)
        {
            Step_Data(i, stepIndex);
        }
        OnStepStart(stepIndex, "");
    }

    void Step_Data(int Step, int targetStep)
    {
        bool isNearbyStep = targetStep - Step == 1;
        switch (Step)
        {
            case 0: 
                break;
            
            case 1: // 절개된 복부대동맥 혈관 상/ 좌 /우 3군데에 겸자 위치시키고 Entity.EnableTrack(false);시키기
                StartCoroutine(SetBlockClamp_First());
                Bloode_Vessel.isBlockClamp_First = true;
                break;
            
            case 2: // 복부대동맥.GetComponent<Animation>().Play("First_Cutting");하여 첫번째 절개 실시
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_First_Complete");
                Bloode_Vessel.isCut_First = true;
                break;
            
            case 3: // 혈전 Array를 foreach문으로 돌려서 전부 삭제시키기
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_First_Complete");
                Bloode_Vessel.Clots[0].DestroyClots();
                break;
            
            case 4: // 복부대동맥.GetComponent<Animation>().Play("Second_Cutting");하여 두번째 절개 실시
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                Bloode_Vessel.isCut_Second = true;
                break;
            
            case 5: // 인조 혈관 배치
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                var yGraft = tools["Y_Graft"].gameObject.GetComponent<NXR_Y_Graft>();
                yGraft.gameObject.SetActive(true);
                yGraft.transform.position = new Vector3(1.857f, 0.801f, -0.074f);
                yGraft.transform.rotation = Quaternion.Euler(-82.64f, -31.737f, 31.761f);
                yGraft.SetChildEnableTrack(false);
                yGraft.RemoveHandle(true);
                break;
            
            case 6: // 인조 혈관 치환
                break;
            case 7: // 인조 혈관 문합
                break;
            
            case 8: // 인조 혈관 아래쪽 좌우 limb 겸자 차단
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                StartCoroutine(SetBlockClamp_Second());
                Bloode_Vessel.blockYGraftCount = 2;
                break;
            
            case 9: // 위쪽 대동맥 겸자 해제
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                var clamp1 = tools["Clamp_1"].gameObject.GetComponent<NXR_Clamp>();
                clamp1.gameObject.SetActive(false);
                break;
            
            case 10: // 우측 limb 문합
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                tools["Clamp_5"].gameObject.SetActive(false);
                tools["Clamp_3"].gameObject.SetActive(false);
                break;
            
            case 11: // 좌측 limb 문합
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Second_Complete");
                tools["Clamp_4"].gameObject.SetActive(false);
                tools["Clamp_2"].gameObject.SetActive(false);
                break;
            
            case 12: 
                // 기존의 대동맥으로 인조혈관 감싸기
                if(isNearbyStep)
                    Bloode_Vessel.PlayAnimation("Blood_Vessel_Combine");
                Bloode_Vessel.isCoverable = true;
                Bloode_Vessel.isCompleteCover = true;
                break;
        }
    }

    #region StepStart,Finish

    private void OnStepStart0()
    {
        if(!NXRSession.Instance.Press_Button)
            NXRSession.Instance.RequestScenarioSetpServerRpc(1);
    }

    private void OnStepStart1()
    {
        Debug.Log("OnStepStart.01");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 1, name = "Step 2", tools = new List<string>() { "Clamp" } } );
        ScenarioData.tools.Add("Clamp", new ScenarioData.Tool() { id = "Clamp", name = "Clamp", returnMode = NXREntity.ReturnMode.None });
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Dummy001 ~ Dummy003
        Bloode_Vessel.transform.GetChild(2).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(3).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(4).gameObject.SetActive(true);
        
        //Abdominal_Aorta > Dummy004
        Bloode_Vessel.transform.GetChild(5).gameObject.SetActive(false);
        
        StartCoroutine(FinishStep1());
    }
    
    private IEnumerator FinishStep1()
    {
        while (!Bloode_Vessel.isBlockClamp_First)
            yield return null;

        yield return StartCoroutine(SetBlockClamp_First());
        
        Debug.Log("FinishStep.01: Block Three Clamp");
        
        //Abdominal_Aorta > Dummy001 ~ Dummy003
        Bloode_Vessel.transform.GetChild(2).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(3).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(4).gameObject.SetActive(false);
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(2);

        yield return new WaitForSeconds(2f);
    }
    
    private void SetClampProperties(string clampName, Vector3 pos, Quaternion rot)
    {
        var tool = tools[clampName].gameObject;
        tool.transform.position = pos;
        tool.transform.rotation = rot;
        tool.SetActive(true);
    
        var entity = tool.GetComponent<NXREntity>();
        entity.EnableTrack(false);
        entity.SetInteractLayer("Nothing");
    
        tool.GetComponent<NXR_Clamp>().isBlocking = true;
    
        tool.transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(Vector3.up * -5);
        tool.transform.GetChild(0).GetChild(1).localRotation = Quaternion.Euler(Vector3.up * 5);
    }
    
    private IEnumerator SetBlockClamp_First()
    {
        var clamp = FindObjectsOfType<NXR_Clamp>();
        for (int i = clamp.Length - 1; i >= 0; i--)
        {
            if(!clamp[i].IsSceneObj)
                Destroy(clamp[i].gameObject);
        }

        SetClampProperties("Clamp_1", new Vector3(1.86090004f, 0.977999985f, -0.208299994f),
            Quaternion.Euler(359.610077f, 268.075378f, 29.1986008f));
        SetClampProperties("Clamp_2", new Vector3(1.76139998f, 0.932299972f, -0.00749999983f), 
            Quaternion.Euler(344.779694f, 45.3015518f, 30.0504704f));
        SetClampProperties("Clamp_3", new Vector3(1.8865f, 0.94630003f, 0.0151000004f), 
            Quaternion.Euler(350.58316f, 127.279144f, 15.0871725f));
        
        yield return null;
    }

    private void OnStepStart2()
    {
        Debug.Log("OnStepStart.02");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 2, name = "Step 3", tools = new List<string>() { "Mess" } });
        ScenarioData.tools.Add("Mess", new ScenarioData.Tool() { id = "Mess", name = "Mess", returnMode = NXREntity.ReturnMode.Destroy });
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Dummy004
        Bloode_Vessel.transform.GetChild(5).gameObject.SetActive(true);
        
        StartCoroutine(FinishStep2());
    }
    
    private IEnumerator FinishStep2()
    {
        while (!Bloode_Vessel.isCut_First)
            yield return null;
        
        Debug.Log("FinishStep.02: Cut_First Aorta");
        
        //Abdominal_Aorta > Dummy004
        Bloode_Vessel.transform.GetChild(5).gameObject.SetActive(false);
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(3);

        yield return new WaitForSeconds(2f);
    }
    
    private void OnStepStart3()
    {
        Debug.Log("OnStepStart.03");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 3, name = "Step 4", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        StartCoroutine(FinishStep3());
    }

    private IEnumerator FinishStep3()
    {
        Bloode_Vessel.Clots[0].isRemovable = true;

        while (!Bloode_Vessel.Clots[0].Clots_Removed)
            yield return null;
        
        Debug.Log("FinishStep.03: Remove Clots");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(4);

        yield return new WaitForSeconds(2f);
    }
    
    private void OnStepStart4()
    {
        Debug.Log("OnStepStart.04");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 4, name = "Step 5", tools = new List<string>() { "Mess" } });
        ScenarioData.tools.Add("Mess", new ScenarioData.Tool() { id = "Mess", name = "Mess", returnMode = NXREntity.ReturnMode.Destroy });
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Dummy005
        Bloode_Vessel.transform.GetChild(6).gameObject.SetActive(true);
        
        StartCoroutine(FinishStep4());
    }

    private IEnumerator FinishStep4()
    {
        while (!Bloode_Vessel.isCut_Second)
            yield return null;
        
        yield return new WaitForSeconds(5f);
        Debug.Log("FinishStep.04: Cut_Second Aorta");
        
        //Abdominal_Aorta > Dummy005
        Bloode_Vessel.transform.GetChild(6).gameObject.SetActive(false);
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(5);

        yield return new WaitForSeconds(2f);
    }
    
    private void OnStepStart5()
    {
        Debug.Log("OnStepStart.05");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 5, name = "Step 6", tools = new List<string>() { "Y_Graft"} });
        ScenarioData.tools.Add("Y_Graft", new ScenarioData.Tool() { id = "Y_Graft", name = "Y_Graft", returnMode = NXREntity.ReturnMode.None });
        SS.Inventory.Refresh();
        
        // Graft_Cutting_Point
        Bloode_Vessel.transform.GetChild(8).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(9).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(10).gameObject.SetActive(true);
        
        // Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(11).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(12).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(13).gameObject.SetActive(true);
        
        StartCoroutine(FinishStep5());
    }
    
    public bool GetActiveYGraft() => tools["Y_Graft"].gameObject.activeSelf;

    public void SetActiveYGraft(bool active)
    {
        tools["Y_Graft"].gameObject.SetActive(active);
    }
    
    private IEnumerator FinishStep5()
    {
        //Dummy_Prime_Handle_Up과 Dummy_Prime_Handle_Left를 집고 각 센서 근처에 갖다대면 통과체크용 bool이 활성화됨
        var yGraft = tools["Y_Graft"].gameObject.GetComponent<NXR_Y_Graft>();
        while (yGraft.Graft_Cutting_Counter < 3)
            yield return null;

        // Graft_Cutting_Point
        Bloode_Vessel.transform.GetChild(8).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(9).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(10).gameObject.SetActive(false);
        
        // Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(11).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(12).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(13).gameObject.SetActive(false);
        
        yGraft.transform.position = new Vector3(1.857f, 0.801f, -0.074f);
        yGraft.transform.rotation = Quaternion.Euler(-82.64f, -31.737f, 31.761f);
        yGraft.SetChildEnableTrack(false);
        
        Debug.Log("FinishStep.05: Placed Y Graft");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(6);

        yield return new WaitForSeconds(2f);
    }

    private void OnStepStart6()
    {
        Debug.Log("OnStepStart.06");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 6, name = "Step 7", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        Debug.Log("FinishStep.06: Cut Y Graft");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(7);
    }
    
    private void OnStepStart7()
    {
        Debug.Log("OnStepStart.07");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 7, name = "Step 8", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        Debug.Log("FinishStep.07: Suture Y Graft");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(8);
    }
    
    private void OnStepStart8()
    {
        Debug.Log("OnStepStart.08");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 8, name = "Step 9", tools = new List<string>() { "Clamp" } } );
        ScenarioData.tools.Add("Clamp", new ScenarioData.Tool() { id = "Clamp", name = "Clamp", returnMode = NXREntity.ReturnMode.None });
        SS.Inventory.Refresh();
        
        // Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(12).gameObject.SetActive(true);
        Bloode_Vessel.transform.GetChild(13).gameObject.SetActive(true);
        
        StartCoroutine(FinishStep8());
    }

    private IEnumerator FinishStep8()
    {
        while (Bloode_Vessel.blockYGraftCount < 2)
            yield return null;
        
        yield return StartCoroutine(SetBlockClamp_Second());
        
        Debug.Log("FinishStep.08: Block Y Graft Left/Right");
        
        // Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(12).gameObject.SetActive(false);
        Bloode_Vessel.transform.GetChild(13).gameObject.SetActive(false);
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(9);
        
        yield return new WaitForSeconds(2f);
    }
    
    private IEnumerator SetBlockClamp_Second()
    {
        var clamp = FindObjectsOfType<NXR_Clamp>();
        for (int i = clamp.Length - 1; i >= 0; i--)
        {
            if(!clamp[i].IsSceneObj)
                Destroy(clamp[i].gameObject);
        }
        
        SetClampProperties("Clamp_4", new Vector3(1.83179998f,0.954900026f,0.00039999999f),
            Quaternion.Euler(2.92591333f,65.8073349f,22.8121033f));
        SetClampProperties("Clamp_5", new Vector3(1.90269995f,0.94599998f,-0.0217000004f),
            Quaternion.Euler(7.50173855f,112.163406f,17.4481697f));
        
        yield return null;
    }
    
    private void OnStepStart9()
    {
        Debug.Log("OnStepStart.09");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 9, name = "Step 10", tools = new List<string>() } );
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Dummy001
        Bloode_Vessel.transform.GetChild(2).gameObject.SetActive(true);
        
        tools["Clamp_1"].gameObject.GetComponent<NXREntity>().EnableTrack(true);
        tools["Clamp_1"].gameObject.GetComponent<NXREntity>().SetInteractLayer("Default");
        
        StartCoroutine(FinishStep9());
    }

    private IEnumerator FinishStep9()
    {
        NXR_Clamp clamp = tools["Clamp_1"].gameObject.GetComponent<NXR_Clamp>();
        
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("FinishStep.09: Unblock Up Clamp");

        yield return new WaitForSeconds(1f);
        clamp.gameObject.SetActive(false);
        clamp.HandleEnabled(false);
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(10);
        
        yield return new WaitForSeconds(2f);
    }
    
    #endregion
    
    private void OnStepStart10()
    {
        Debug.Log("OnStepStart.10");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 10, name = "Step 11", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Right_Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(13).gameObject.SetActive(true);
        
        StartCoroutine(FinishStep10());
    }

    private IEnumerator FinishStep10()
    {
        NXR_Clamp clamp = tools["Clamp_5"].gameObject.GetComponent<NXR_Clamp>();
        clamp.gameObject.SetActive(true);
        clamp.HandleEnabled(true);
        
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("[Right] 겸자 차단 해제 완료");
        
        yield return new WaitForSeconds(1f);

        Debug.Log("[Right] 혈전 발생");
        
        Bloode_Vessel.Clots[1].gameObject.SetActive(true);
        Bloode_Vessel.Clots[1].isRemovable = true;
        while (!Bloode_Vessel.Clots[1].Clots_Removed)
            yield return null;
        
        Debug.Log("[Right] 혈전 제거 완료");
        
        yield return new WaitForSeconds(1f);

        // 인조혈관 차단을 해제했던 겸자를 차단
        clamp.state = eClampState.BlockAgain;
        while (!clamp.isBlocking)
            yield return null;

        clamp.HandleEnabled(false);
        Debug.Log("[Right] 겸자 차단 완료, 나머지 문합");
        
        yield return new WaitForSeconds(1f);
        
        // 인조혈관 차단을 해제했던 겸자를 다시 차단 해제
        clamp.HandleEnabled(true);
        while (clamp.isBlocking)
            yield return null;

        Debug.Log("[Right] 다시 겸자 차단 해제 완료");
        yield return new WaitForSeconds(1f);
        
        clamp.HandleEnabled(false);
        clamp.gameObject.SetActive(false);
        
        //Abdominal_Aorta > Dummy002
        Bloode_Vessel.transform.GetChild(3).gameObject.SetActive(true);
        
        clamp = tools["Clamp_3"].gameObject.GetComponent<NXR_Clamp>();
        clamp.HandleEnabled(true);
        clamp.state = eClampState.Complete;
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("[Right] 동맥 겸자 차단 해제 완료");
        yield return new WaitForSeconds(1f);
        
        clamp.HandleEnabled(false);
        clamp.gameObject.SetActive(false);
        
        Debug.Log("FinishStep.10: Unblock Right Clamp");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(11);
        
        yield return new WaitForSeconds(2f);
    }
    
    private void OnStepStart11()
    {
        Debug.Log("OnStepStart.10");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 11, name = "Step 12", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        //Abdominal_Aorta > Left_Graft_Attach_Point
        Bloode_Vessel.transform.GetChild(12).gameObject.SetActive(true);
        
        if (tools["Clamp_4"].gameObject.TryGetComponent<NXR_Clamp>(out var clmap))
            clmap.HandleEnabled(true);

        StartCoroutine(FinishStep11());
    }

    private IEnumerator FinishStep11()
    {
        NXR_Clamp clamp = tools["Clamp_4"].gameObject.GetComponent<NXR_Clamp>();
        
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("[Left] 겸자 차단 해제 완료");
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log("[Left] 혈전 발생");
        
        Bloode_Vessel.Clots[2].gameObject.SetActive(true);
        Bloode_Vessel.Clots[2].isRemovable = true;
        while (!Bloode_Vessel.Clots[2].Clots_Removed)
            yield return null;
        
        Debug.Log("[Left] 혈전 제거 완료");
        
        yield return new WaitForSeconds(1f);

        // 인조혈관 차단을 해제했던 겸자를 차단
        clamp.state = eClampState.BlockAgain;
        while (!clamp.isBlocking)
            yield return null;
        
        clamp.HandleEnabled(false);
        Debug.Log("[Left] 겸자 차단 완료, 나머지 문합");
        
        yield return new WaitForSeconds(1f);
        
        // 인조혈관 차단을 해제했던 겸자를 다시 차단 해제
        clamp.HandleEnabled(true);
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("[Left] 다시 겸자 차단 해제 완료");
        yield return new WaitForSeconds(1f);
        
        clamp.HandleEnabled(false);
        clamp.gameObject.SetActive(false);
        
        //Abdominal_Aorta > Dummy003
        Bloode_Vessel.transform.GetChild(4).gameObject.SetActive(true);
        
        clamp = tools["Clamp_2"].gameObject.GetComponent<NXR_Clamp>();
        clamp.HandleEnabled(true);
        clamp.state = eClampState.Complete;
        while (clamp.isBlocking)
            yield return null;
        
        Debug.Log("[Left] 동맥 겸자 차단 해제 완료");
        yield return new WaitForSeconds(1f);
        
        clamp.HandleEnabled(false);
        clamp.gameObject.SetActive(false);
        
        Debug.Log("FinishStep.11: Unblock Left Clamp");
        
        NXRSession.Instance.RequestScenarioSetpServerRpc(12);
        
        yield return new WaitForSeconds(2f);
    }
    
    private void OnStepStart12()
    {
        Debug.Log("OnStepStart.12");
        
        ScenarioData.tools.Clear();
        ScenarioData.GetScenarioStepMap().Clear();
        ScenarioData.GetScenarioStepMap().Add(0, new ScenarioManager.Info.Step() { index = 12, name = "Step 13", tools = new List<string>() });
        SS.Inventory.Refresh();
        
        StartCoroutine(FinishStep12());
    }

    private IEnumerator FinishStep12()
    {
        Bloode_Vessel.isCoverable = true;

        while (!Bloode_Vessel.isCompleteCover)
            yield return null;
        
        Debug.Log("FinishStep.12: Cover Aorta");

        yield return new WaitForSeconds(2f);
    }

    public override void OnStepStart(int stepIndex, string stepName)
    {
        switch (stepIndex)
        {
            case 0: OnStepStart0(); break;
            case 1: OnStepStart1(); break;
            case 2: OnStepStart2(); break;
            case 3: OnStepStart3(); break;
            case 4: OnStepStart4(); break;
            case 5: OnStepStart5(); break;
            case 6: OnStepStart6(); break;
            case 7: OnStepStart7(); break;
            case 8: OnStepStart8(); break;
            case 9: OnStepStart9(); break;
            case 10: OnStepStart10(); break;
            case 11: OnStepStart11(); break;
            case 12: OnStepStart12(); break;
        }
    }

    public override void OnStepEnd(int stepIndex, string stepName)
    {
        
    }
}
