using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    private ParticleSystem _ParticleSystem;
    private BetterPlayerMovement _PlayerMovement;

    public float velocityThreshold = 5f;
    void Start()
    {
        _ParticleSystem = GetComponent<ParticleSystem>();
        _PlayerMovement = transform.parent.GetComponent<BetterPlayerMovement>();
    }

    void Update()
    {
        this.transform.localRotation = Quaternion.Inverse(transform.parent.localRotation); //Keep particles from rotating with parent
         
        if((Mathf.Abs(_PlayerMovement.body.velocity.x) < velocityThreshold || Mathf.Abs(_PlayerMovement.body.velocity.y) < velocityThreshold) && _PlayerMovement.GetCurrentStateName() == "WallRunState" && _ParticleSystem.isStopped)
        {
            _ParticleSystem.Play();
        }
        if((Mathf.Abs(_PlayerMovement.body.velocity.x) < velocityThreshold || _PlayerMovement.groundCount == 0) && !_ParticleSystem.isStopped)
        {
            _ParticleSystem.Stop();
        }
        else if (Mathf.Abs(_PlayerMovement.body.velocity.x) >velocityThreshold &&_PlayerMovement.groundCount > 0 && _ParticleSystem.isStopped)
        {
            _ParticleSystem.Play();
        }

    }
}
