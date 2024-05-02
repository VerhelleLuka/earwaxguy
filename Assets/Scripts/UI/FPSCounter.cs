using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;


    void Update()
    {
        Text.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
    }
}