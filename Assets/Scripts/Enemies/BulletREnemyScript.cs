using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletREnemyScript : MonoBehaviour
{
    private GameObject player;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
