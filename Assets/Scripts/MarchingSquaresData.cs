

using UnityEngine;

public class MarchingSquaresData : MonoBehaviour
{
    public bool recover = true;

    public float height;
    public bool changedLastFrame;

    private const float RecoveryTime = 1f;
    private const float RecoveryDelayTime = 0.8f;
    private float RecoveryTimer = 0f;
    private float RecoveryDelayTimer = 0f;

    public void Recover()
    {
        if (recover && !changedLastFrame)
        {
            if (RecoveryDelayTimer < RecoveryDelayTime)
            {
                RecoveryDelayTimer += Time.deltaTime;
                return;
            }
            else if (RecoveryTimer < RecoveryTime)
            {
                RecoveryTimer += Time.deltaTime;
                height = Mathf.Lerp(height, 1, (height + RecoveryTimer) / RecoveryTime);
            }
            else
            { 
            }

        }
    }
    public void Update()
    {
        if (recover)
            Recover();
    }

}
