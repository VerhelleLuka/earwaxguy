using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    private ParticleSystem _ParticleSystem;
    private PlayerMovement _PlayerMovement;

    public float velocityThreshold = 5f;
    void Start()
    {
        _ParticleSystem = GetComponent<ParticleSystem>();
        _PlayerMovement = transform.parent.GetComponentInChildren<PlayerMovement>();
    }

    void Update()
    {
        this.transform.localRotation = Quaternion.Inverse(transform.parent.localRotation); //Keep particles from rotating with parent
           
        if((Mathf.Abs(_PlayerMovement.body.velocity.x) < velocityThreshold || !_PlayerMovement.grounded ||_PlayerMovement.swinging) && !_ParticleSystem.isStopped)
        {
            _ParticleSystem.Stop();
        }
        else if (Mathf.Abs(_PlayerMovement.body.velocity.x) >velocityThreshold && _PlayerMovement.grounded && _ParticleSystem.isStopped)
        {
            _ParticleSystem.Play();
        }
       

    }
}
