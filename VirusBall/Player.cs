using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    #region 기본 선언
    public static Player Instance;

    [Header ("스킬") ]
    public Text[] CoolTxt;
    public Image[] CoolImg;
    public GameObject[] LockImg;

    [SerializeField]
    private bool[] skillCanUse = new bool[3];
    [SerializeField]
    private float[] currentCoolTime = new float[3];

    private bool isMove;
    public bool isHide;

    private float moveSpeed = 6f;
    private int jumpCount = 0;
    private float jumpPower = 9f;

    private float maxSpeedX = 8f;
    private float maxSpeedY = 14f;

    [Header("플레이어 이펙트")]
    private Material originMate;
    public Material burnMate;
    public Material holoMate;
    public Material flickerMate;
    public Material waveMate;

    Animator anim;
    Rigidbody2D rigid;
    SpriteRenderer sprite;
    #endregion

    private void Awake() => Instance = this;

    private void Start()
    {
        isMove = true;
        isHide = false;

        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        originMate = GetComponent<SpriteRenderer>().material;

        for (int i = 0; i < skillCanUse.Length; i++)
        {
            skillCanUse[i] = true;
        }

        for (int i = 0; i < DataManager.Instance.gameData.skillUnlock.Length; i++)
        {
            if(DataManager.Instance.gameData.skillUnlock[i])
            {
                LockImg[i].SetActive(false);
            }
            else
            {
                LockImg[i].SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance.isPlay)
        {
            Jump();

            FlySkill();
            HideSkill();
            FlickerSkill();
        }
    }

    private void FixedUpdate() => Move();

    #region 움직임
    private void Move()
    {
        if(isMove && GameManager.Instance.isPlay && !GameManager.Instance.isMission)
        {
            //float inputX = Input.GetAxis("Horizontal");
            float inputX;

            if(Input.GetKey(KeyCode.LeftArrow))
            {
                //inputX = -1f;
                inputX = Input.GetAxis("Horizontal");
                sprite.flipX = true;
            }
            else if(Input.GetKey(KeyCode.RightArrow))
            {
                //inputX = 1f;
                inputX = Input.GetAxis("Horizontal");
                sprite.flipX = false;
            }
            else
            {
                inputX = 0;
            }

            rigid.velocity = new Vector2(inputX * moveSpeed, rigid.velocity.y);

            //X MaxSpeed
            if (rigid.velocity.x > maxSpeedX)  //Right
            {
                rigid.velocity = new Vector2(maxSpeedX, rigid.velocity.y);
            }
            else if (rigid.velocity.x < maxSpeedX * (-1))  //Left
            {
                rigid.velocity = new Vector2(maxSpeedX * (-1), rigid.velocity.y);
            }

            //Y MaxSpeed
            if (rigid.velocity.y > maxSpeedY)  //Up
            {
                rigid.velocity = new Vector2(rigid.velocity.x, maxSpeedY);
            }
            else if (rigid.velocity.y < maxSpeedY * (-1))  //Down
            {
                rigid.velocity = new Vector2(rigid.velocity.x, maxSpeedY * (-1));
            }
        }
    }

    private void Jump()
    {
        //if (!GameManager.Instance.isMission && GameManager.Instance.isPlay)
        if (GameManager.Instance.isPlay && !GameManager.Instance.isMission)
        {
            if (Input.GetButtonDown("Jump") && jumpCount < 1)
            {
                ++jumpCount;

                rigid.velocity = Vector2.up * jumpPower;

                SoundManager.Instance.PlaySFX("Jump");

                //Instantiate(Resources.Load("Particles/Star"), new Vector2(transform.position.x, transform.position.y - 0.5f), Quaternion.identity);
                Instantiate(Resources.Load("Particles/Smoke"), new Vector2(transform.position.x, transform.position.y - 0.5f), Quaternion.identity);
            }
        }
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
    #endregion

    #region 스킬
    private void HideSkill()
    {
        if (skillCanUse[(int)SkillType.Hide] && DataManager.Instance.gameData.skillUnlock[(int)SkillType.Hide])
        {
            CoolImg[(int)SkillType.Hide].enabled = false;
            CoolTxt[(int)SkillType.Hide].enabled = false;

            if (Input.GetKeyDown(KeyCode.S))
            {
                isHide = true;
                sprite.material = holoMate;
                anim.SetLayerWeight(0, 0);
                anim.SetLayerWeight(1, 1);
                anim.SetTrigger("doHide");
                Invoke("HideReset", 2f);

                SoundManager.Instance.PlaySFX("Hide");

                CoolTxt[(int)SkillType.Hide].enabled = true;
                CoolImg[(int)SkillType.Hide].enabled = true;
                CoolImg[(int)SkillType.Hide].fillAmount = 1; //쿨타임 이미지 비활성
                StartCoroutine(CoolTimer((int)SkillType.Hide, DataManager.Instance.gameData.cool[(int)SkillType.Hide]));

                currentCoolTime[(int)SkillType.Hide] = DataManager.Instance.gameData.cool[(int)SkillType.Hide];
                CoolTxt[(int)SkillType.Hide].text = "" + currentCoolTime[(int)SkillType.Hide];

                StartCoroutine(CoolTimeCounter((int)SkillType.Hide));

                skillCanUse[(int)SkillType.Hide] = false;

                //SoundManager.Instance.PlaySFX("PlayerShot");
            }
        }
    }

    private void FlySkill()
    {
        if (skillCanUse[(int)SkillType.Fly] && DataManager.Instance.gameData.skillUnlock[(int)SkillType.Fly])
        {
            CoolImg[(int)SkillType.Fly].enabled = false;
            CoolTxt[(int)SkillType.Fly].enabled = false;

            if (Input.GetKeyDown(KeyCode.A))
            {
                rigid.gravityScale = 0.5f;
                Invoke("FlyReset", 2f);
                transform.Find("PurpleLight").gameObject.SetActive(true);

                SoundManager.Instance.PlaySFX("Fly");

                sprite.material = waveMate;

                CoolTxt[(int)SkillType.Fly].enabled = true;
                CoolImg[(int)SkillType.Fly].enabled = true;
                CoolImg[(int)SkillType.Fly].fillAmount = 1; //쿨타임 이미지 비활성

                StartCoroutine(CoolTimer((int)SkillType.Fly, DataManager.Instance.gameData.cool[(int)SkillType.Fly]));

                currentCoolTime[(int)SkillType.Fly] = DataManager.Instance.gameData.cool[(int)SkillType.Fly];
                CoolTxt[(int)SkillType.Fly].text = "" + currentCoolTime[(int)SkillType.Fly];

                StartCoroutine(CoolTimeCounter((int)SkillType.Fly));

                skillCanUse[(int)SkillType.Fly] = false;

                StartCoroutine(Wings());

                //SoundManager.Instance.PlaySFX("PlayerShot");
            }
        }
    }

    IEnumerator Wings()
    {
        GameObject wings = Instantiate(Resources.Load<GameObject>("Particles/Wings"), transform.position, Quaternion.identity);
        Destroy(wings, 1f);

        yield return new WaitForSeconds(0.2f);

        if(skillCanUse[(int)SkillType.Fly] == false && rigid.gravityScale == 0.5f)
        {
            StartCoroutine(Wings());
        }
    }
    
    private void FlickerSkill()
    {
        if (skillCanUse[(int)SkillType.Flicker] && DataManager.Instance.gameData.skillUnlock[(int)SkillType.Flicker])
        {
            CoolImg[(int)SkillType.Flicker].enabled = false;
            CoolTxt[(int)SkillType.Flicker].enabled = false;

            if (Input.GetKeyDown(KeyCode.D))
            {
                int direction = sprite.flipX == true ? -1 : 1;

                RaycastHit2D rayHit = Physics2D.Raycast(transform.position, Vector2.right * direction, 2.2f, LayerMask.GetMask("Platform"));

                if (rayHit.collider != null) return;

                anim.SetLayerWeight(0, 0);
                anim.SetLayerWeight(1, 1);
                anim.SetTrigger("doFlicker");
                sprite.material = flickerMate;
                Invoke("FlickerReset", 2f);

                SoundManager.Instance.PlaySFX("Flicker");

                transform.Translate(Vector2.right * direction * 2f);

                CoolTxt[(int)SkillType.Flicker].enabled = true;
                CoolImg[(int)SkillType.Flicker].enabled = true;
                CoolImg[(int)SkillType.Flicker].fillAmount = 1; //쿨타임 이미지 비활성
                StartCoroutine(CoolTimer((int)SkillType.Flicker, DataManager.Instance.gameData.cool[(int)SkillType.Flicker]));

                currentCoolTime[(int)SkillType.Flicker] = DataManager.Instance.gameData.cool[(int)SkillType.Flicker];
                CoolTxt[(int)SkillType.Flicker].text = "" + currentCoolTime[(int)SkillType.Flicker];

                StartCoroutine(CoolTimeCounter((int)SkillType.Flicker));

                skillCanUse[(int)SkillType.Flicker] = false;

                //SoundManager.Instance.PlaySFX("PlayerShot");
            }
        }
    }

    IEnumerator CoolTimer(int _id, float _cool)
    {
        while (CoolImg[_id].fillAmount > 0)
        {
            CoolImg[_id].fillAmount -= 1 * Time.smoothDeltaTime / _cool;

            yield return null;
        }
    }

    IEnumerator CoolTimeCounter(int _id)
    {
        while (currentCoolTime[_id] > 0)
        {
            currentCoolTime[_id] -= Time.deltaTime;
            CoolTxt[_id].text = string.Format("{0:0}", currentCoolTime[_id]);

            yield return null;
        }

        skillCanUse[_id] = true;
    }

    private void HideReset()
    {
        isHide = false;
        anim.SetLayerWeight(0, 1);
        anim.SetLayerWeight(1, 0);
        sprite.material = originMate;
    }

    private void FlyReset()
    {
        StopCoroutine(Wings());
        rigid.gravityScale = 3f;
        sprite.material = originMate;
        transform.Find("PurpleLight").gameObject.SetActive(false);
    }

    private void FlickerReset()
    {
        anim.SetLayerWeight(0, 1);
        anim.SetLayerWeight(1, 0);
        sprite.material = originMate;
    }

    public void SkillGet(SkillType type)
    {
        DataManager.Instance.gameData.skillUnlock[(int)type] = true;
        LockImg[(int)type].SetActive(false);
    }
    #endregion

    public void Die()
    {
        isMove = false;
        moveSpeed = 0f;
        jumpPower = 0f;

        SoundManager.Instance.PlaySFX("Die");
    }

    #region 물리 충돌
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Enemy"))
        {
            collision.gameObject.tag = "Untagged";
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("MovePlatform"))
        {
            rigid.gravityScale = 3f;
            jumpCount = 0;
            rigid.velocity = Vector2.up * jumpPower;
        }

        if(collision.CompareTag("Platform"))
        {
            rigid.gravityScale = 3f;
            jumpCount = 0;
            
            if(!GameManager.Instance.isMission)
            {
                rigid.velocity = Vector2.up * jumpPower;
            }

            Instantiate(Resources.Load("Particles/Smoke"), new Vector2(transform.position.x, transform.position.y - 0.5f), Quaternion.identity);
        }

        if (collision.CompareTag("Super"))
        {
            jumpCount = -1;
            rigid.velocity = Vector2.up * jumpPower * 1.5f;
            SoundManager.Instance.PlaySFX("Super");

            Instantiate(Resources.Load("Particles/Smoke"), new Vector2(transform.position.x, transform.position.y - 0.5f), Quaternion.identity);
        }

        if (collision.CompareTag("File"))
        {
            SoundManager.Instance.PlaySFX("GetFile");

            Destroy(collision.transform.Find("Point Light 2D").gameObject);
            Instantiate(Resources.Load("Lights/BlueLight"), collision.transform.position, Quaternion.identity);
            collision.GetComponent<Transform>().localScale = new Vector2(1f, 1f);
            collision.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Ctrls/File");
            collision.GetComponent<Animator>().SetTrigger("doGet");
            collision.tag = "Untagged";

            GameManager.Instance.AddFile();
        }

        if(collision.CompareTag("ClearFile"))
        {
            collision.tag = "Untagged";
            GameManager.Instance.StageClear();
        }
    }
    #endregion
}