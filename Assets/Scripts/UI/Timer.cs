using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI textMeshPro;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = TimeSpan.FromMinutes(GameManager.instance.timer).ToString();
        if (textMeshPro.text.Length > 11)
            textMeshPro.text = textMeshPro.text.Substring(0,11);
    }
}
