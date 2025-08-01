using Unity.Netcode;
using UnityEngine;

public class NXR_Y_Graft : NetworkBehaviour
{
    private NXREntity entity;
    public GameObject[] Handles;
    public Vector3 Left_Grab_Distance;
    public Vector3 Right_Grab_Distance;
    public bool Is_Left;
    bool Moving = false;
    Vector3 Initial_Position;
    public int Graft_Cutting_Counter = 0;

    /// <summary>
    /// 원래 씬에 배치된 오브젝트인지
    /// </summary>
    public bool IsSceneObj { get; private set; }

    private void Awake()
    {
        if (name == "Y_Graft")
            IsSceneObj = true;
    }

    private void Start()
    {
        entity = GetComponent<NXREntity>();
        
        if (transform.childCount > 0)
        {
            Handles = new GameObject[6];
            for (int i = 0; i < 6; i++)
            {
                Handles[i] = transform.GetChild(i).gameObject;
                Handles[i].GetComponent<NXR_Y_Graft_Handle>().Distance = Vector3.Distance(transform.position, Handles[i].transform.position);
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Left_Grab_Distance != Vector3.zero || Right_Grab_Distance != Vector3.zero)
            {
                Graft_Rotating(i);
            }
        }
        if (Graft_Cutting_Counter < 3 && Left_Grab_Distance != Vector3.zero && Right_Grab_Distance != Vector3.zero)
        {
            if (!Moving)
            {
                Initial_Position = transform.position;
                Handles[Prime(2)].GetComponent<NXR_Y_Graft_Handle>().Set_Initial_Grab_Position();
                if (Is_Left)
                {
                    Handles[Prime(1)].GetComponent<NXR_Y_Graft_Handle>().Set_Initial_Grab_Position();
                }
                else
                {
                    Handles[Prime(0)].GetComponent<NXR_Y_Graft_Handle>().Set_Initial_Grab_Position();
                }
                Moving = true;
            }
            Graft_Moving();
        }
        else if(Moving)
        {
            Moving = false;
        }
    }

    int Prime(int i)
    {
        if (Handles[i + 3].GetComponent<NXR_Y_Graft_Handle>().Is_Prime)
        {
            return i + 3;
        }
        else
        {
            return i;
        }
    }

    void Graft_Rotating(int i)
    {
        if (Handles[i] != null && Handles[i].GetComponent<NXR_Y_Graft_Handle>().Grabbing)
        {
            Handles[i].transform.rotation = Quaternion.LookRotation(transform.position - (Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name))) * Quaternion.Euler(Vector3.left * 90);
            if (Graft_Cutting_Counter == 3)
            {
                if(Vector3.Distance(transform.position, Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name)) < Handles[i].GetComponent<NXR_Y_Graft_Handle>().Distance + 0.1f)
                    Handles[i].transform.position = Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name);
            }
            else
            {
                Handles[i].transform.position = transform.position + ((Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name)) - transform.position).normalized * Handles[i].GetComponent<NXR_Y_Graft_Handle>().Distance;
            }
            if (Handles[i].name.Substring(0, 5) == "Prime")
            {
                Handles[i + 3].transform.rotation = Quaternion.LookRotation(transform.position - (Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name))) * Quaternion.Euler(Vector3.left * 90);
                Handles[i + 3].transform.position = transform.position + ((Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position + LR(Handles[i].name)) - transform.position).normalized * Handles[i + 3].GetComponent<NXR_Y_Graft_Handle>().Distance;
            }
            Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand_Mesh.transform.localPosition = Vector3.zero;
            Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand_Mesh.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    void Graft_Moving()
    {
        if (Is_Left)
        {
            Compare_Distance(Moving_Check(Prime(2)), Moving_Check(Prime(1)));
        }
        else
        {
            Compare_Distance(Moving_Check(Prime(2)), Moving_Check(Prime(0)));
        }
    }

    void Compare_Distance(Vector3 A, Vector3 B)
    {
        transform.position = Initial_Position + new Vector3((A.x + B.x) / 2, (A.y + B.y) / 2, (A.z + B.z) / 2);
    }

    Vector3 Moving_Check(int i)
    {
        return Handles[i].GetComponent<NXR_Y_Graft_Handle>().Hand.transform.position - Handles[i].GetComponent<NXR_Y_Graft_Handle>().Initial_Grab_Posision;
    }

    public void Cutting_Anim_End()
    {
        Graft_Cutting_Counter++;
        if (Graft_Cutting_Counter == 3)
        {
            /*
            App.Instance.UngrabAll(App.Instance.xrLeftHandDirectInteractor);
            App.Instance.UngrabAll(App.Instance.xrRightHandDirectInteractor);
            */
        
            transform.position = new Vector3(1.857f, 0.801f, -0.074f);
            transform.rotation = Quaternion.Euler(-82.64f, -31.737f, 31.761f);
        }
    }

    Vector3 LR(string Handle_Name)
    {
        if (Handle_Name.Substring(Handle_Name.Length - 2, 2) == "Up")
        {
            return Left_Grab_Distance;
        }
        else
        {
            return Right_Grab_Distance;
        }
    }

    public void SetChildEnableTrack(bool enable)
    {
        var entities = GetComponentsInChildren<NXREntity>();
        for (int i = 0; i < entities.Length; i++)
        {
            entities[i].EnableTrack(enable);
            entities[i].SetInteractLayer("Nothing");
        }
    }

    public void RemoveHandle(bool isImmediately)
    {
        var handles = GetComponentsInChildren<NXR_Y_Graft_Handle>();
        foreach (var handle in handles)
        {
            if(handle.Is_Prime)
                continue;
            
            if(isImmediately)
                handle.SetRemoveHandle();
            else
                handle.AutoRemoveHandle();
            
            handle.entity.EnableTrack(false);
            handle.entity.SetInteractLayer("Nothing");
        }
    }

    public void OnGrabbed(int grabberId, NXREntity.Hand hand) { }
    public void OnUngrabbed(NXREntity.Hand hand) { }
    public void OnActivated(NXREntity.Hand hand) { }
    public void OnDeactivated(NXREntity.Hand hand) { }
}
