using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillFile : MonoBehaviour
{
    [SerializeField]
    private SkillType type;

    public UnityEngine.Experimental.Rendering.Universal.Light2D myLight;
    public RuntimeAnimatorController animCtrl;
    public Material effect;

    Animator anim;
    BoxCollider2D boxCol;
    SpriteRenderer sprite;

    private void Start()
    {
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    IEnumerator FadeIn()
    {
        float time = 0f;
        float fadeTime = 1.5f;

        while (myLight.intensity > 0f)
        {
            time += Time.deltaTime / fadeTime;
            myLight.intensity = Mathf.Lerp(1, 0, time);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Player.Instance.SkillGet(type);
            SoundManager.Instance.PlaySFX("GetSkilFile");

            gameObject.transform.Find("PurpleLight").GetComponent<LightFlicker>().enabled = false;
            gameObject.layer = 14;
            boxCol.enabled = false;
            sprite.material = effect;
            anim.runtimeAnimatorController = animCtrl;
            StartCoroutine(FadeIn());
        }
    }
}