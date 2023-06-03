using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshTrail : MonoBehaviour
{

    public Sprite playerSprite;
    public bool SetDashVFX = true;
    public float fadeSpeed;
    public float MaxtimeInterval;
    float timeInterval;
 

    private void Start()
    {
       
        timeInterval = MaxtimeInterval;
    }
    private void Update()
    {
        if (SetDashVFX == true)
        {

            timeInterval -= Time.deltaTime;
            if (timeInterval <= 0)
            {
                StartCoroutine(GenerateOneVFX());
                timeInterval = MaxtimeInterval;
            }
        }
    }

    IEnumerator GenerateOneVFX() 
    {
        GameObject obj = Instantiate(new GameObject(), transform.position, transform.rotation);
        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = playerSprite;
     

      while (spriteRenderer.color.a >= 0f) 
        {
            Color spriteColor = spriteRenderer.color;
            spriteColor.a -= 0.01f;
            spriteRenderer.color = spriteColor;
            yield return new WaitForSeconds(Time.deltaTime/fadeSpeed);
        }
      
        Destroy(obj);

    }

}


