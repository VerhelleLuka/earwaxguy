

using UnityEngine;

public class MarchingSquaresData
{
    public bool recover = true;

    public float height = 0f;
    
    public bool changedLastFrame = false;
    public bool changedThisFrame = false;
    private bool Recovering = false;
    private const float RecoveryTime = 2f;
    private const float RecoveryDelayTime = 0.8f;
    private float RecoveryTimer = 0f;
    private float RecoveryDelayTimer = 0f;

    public void Recover()
    {
        if (changedThisFrame)
        {
            changedThisFrame = false;

            RecoveryDelayTimer = 0;
            RecoveryTimer = 0;
            Recovering = true;
            return;
        }
        if (Recovering)
        {
            if (RecoveryDelayTimer < RecoveryDelayTime)
            {
                RecoveryDelayTimer += Time.deltaTime;
            }
            else if (RecoveryTimer < RecoveryTime)
            {
                RecoveryTimer += Time.deltaTime;
                height = Mathf.Lerp(height, 1,   RecoveryTimer / RecoveryTime);
            }
            else
            {
                RecoveryDelayTimer = 0;
                RecoveryTimer = 0;
                height = 1;
                Recovering=false;
            }
        }
        
    }
    public bool Update( )
    {
        if (recover && height < 1)
            Recover();

        return Recovering;
    }

}
