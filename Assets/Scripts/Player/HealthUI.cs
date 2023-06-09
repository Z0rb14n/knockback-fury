using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TextMeshProUGUI textObject;


    void Update()
    {
        textObject.text = $"HP: {playerHealth.health}/{playerHealth.maxHealth}";
    }
}
