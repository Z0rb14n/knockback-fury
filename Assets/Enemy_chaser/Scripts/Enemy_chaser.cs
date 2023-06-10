using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_chaser : MonoBehaviour
{
    GameObject player;
    public float explodeDistance;
    public float explodeDelayTime;

    private void Start()
    {
        player = GameObject.FindWithTag("Enemy_chaser_follow");
    }

    private void Update()
    {
        Vector3 playerPOS = player.transform.position;
        Vector3 enemyPOS = transform.position;
        float distance = (playerPOS-enemyPOS).magnitude;

        //find distance between player and enemy

        if (distance < explodeDistance) 
        {
            StartCoroutine(explode());   
        }

    }

    IEnumerator explode() 
    {  //do something before explosion delay
      yield return new WaitForSeconds(explodeDelayTime);
       //do something after explosion delay
        Destroy(gameObject);
    }
}
