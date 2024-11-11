using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dashbar : MonoBehaviour
{


    public Image image;
    [SerializeField] private PlayerMovement playerMovement;
    private float cooldownTimer = 1f;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Fiks mij");
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillAmount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownTimer < playerMovement.dashCooldown && image.fillAmount < 1)
        {
            cooldownTimer += Time.deltaTime;
            image.fillAmount = Mathf.Lerp(0, 1, cooldownTimer / playerMovement.dashCooldown);

        }
        else
        {
            image.color = Color.green;
            cooldownTimer = 0f;
        }
    }

    private void OnEnable()
    {
        //playerMovement.DashExecuted += OnDashExecuted;
    }
    private void OnDisable()
    {
        //playerMovement.DashExecuted -= OnDashExecuted;

    }

    void OnDashExecuted()
    {
        cooldownTimer = 0f;
        image.color = Color.red;
        image.fillAmount = 0;
    }
}
