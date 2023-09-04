﻿using TMPro;
using UnityEngine;

namespace FileSave
{
    public class CheeseUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI cheeseText;
        private void Awake()
        {
            CrossRunInfo.Instance.OnCheeseCountChange += _ => cheeseText.text = CrossRunInfo.Instance.data.cheese.ToString();
        }
    }
}