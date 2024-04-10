using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D body;
    public float moveSpeed = 5f;
    public float brakeSpeed = 0.5f;
    public float jumpPower = 50f;

    private bool Grounded = true;
    private float HorizontalMovement;

    //Swinging
    private DistanceJoint2D SpringJoint;
    private bool CanSwing = false;
    private bool Swinging = false;
    private Vector2 SwingPosition = new Vector2();
    
    void Start()
    {

    }

    private void Update()
    {

    }
    private void FixedUpdate()
    {
        //To check if player input is inverse of player velocity
        if (((HorizontalMovement < 0 && body.velocity.x > 0) || (HorizontalMovement > 0 && body.velocity.x < 0)))
            body.AddForce(new Vector2(HorizontalMovement * moveSpeed * brakeSpeed * Time.fixedDeltaTime, 0));

        else
            body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));

    }
    public void Move(InputAction.CallbackContext context)
    {
        HorizontalMovement = context.ReadValue<Vector2>().x;

    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (Grounded)
        {
            body.velocity = new Vector2(body.velocity.x, 0);
            body.AddForce(new Vector2(0, jumpPower));
            Grounded = false;
        }

    }
    public void Swing(InputAction.CallbackContext context)
    {
        if (CanSwing && context.ReadValue<float>() > 0f)
        {
            Swinging = true;

            if (SpringJoint == null)
            {
                SpringJoint = gameObject.AddComponent<DistanceJoint2D>();
                SpringJoint.connectedAnchor = SwingPosition;
                SpringJoint.distance = Vector2.Distance(SwingPosition, gameObject.transform.position);
               
                
                SpringJoint.enableCollision = false;
                
            }


        }
        else if (context.ReadValue<float>() == 0f)
        {
            Swinging = false;
            Destroy(SpringJoint);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            Grounded = true;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == ("Swing"))
        {
            CanSwing = true;
            SwingPosition = collision.transform.position;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == ("Swing"))
        {
            CanSwing = false;
        }
    }
}
