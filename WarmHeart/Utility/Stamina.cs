using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    public GameObject Player;
    Image image;
    public Image image1;
    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        float c_fill = Player.GetComponent<PlayerFly>().FlyPower;
        if (c_fill != 100)
        {
            image.enabled = true;
            image1.enabled = true;
            image.fillAmount = c_fill / 100;
        }
        else
        {
            image.enabled = false;
            image1.enabled = false;
        }
        
    }
}
