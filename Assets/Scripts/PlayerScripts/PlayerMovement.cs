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
    public float momentumShiftPower = 10f;

    private bool Grounded = true;
    private float HorizontalMovement;
    private float VerticalMovement;

    //Swinging
    private DistanceJoint2D SpringJoint;
    private bool CanSwing = false;
    private bool Swinging = false;
    private Vector2 SwingPosition = new Vector2();
    private const float SwingRotationOffset = -90f;//In degrees
    private float AngularVelocityBeforeSwing = 0f;

    //arms
    public LineRenderer lineRenderer;

    //Stuck in wax
    private bool Stuck = false;
    public float moveSpeedStuck = 1.9f;

    //momentumshift
    private bool CanMomentumShift = false;


    void Start()
    {

    }

    private void Update()
    {
        if (Swinging)
        {
            Vector3 relativePos = new Vector3(SwingPosition.x, SwingPosition.y, 0) - transform.position;
            float angle = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
            angle += SwingRotationOffset;
            Debug.Log(angle);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            lineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y, -1f));
        }
    }
    private void FixedUpdate()
    {

        //To check if player input is inverse of player velocity
        if(Stuck)
        {
            body.velocity = new Vector2(HorizontalMovement * moveSpeedStuck * Time.fixedDeltaTime, VerticalMovement * moveSpeedStuck * Time.fixedDeltaTime);
            return;
        }
        if (((HorizontalMovement < 0 && body.velocity.x > 0) || (HorizontalMovement > 0 && body.velocity.x < 0)))
            body.AddForce(new Vector2(HorizontalMovement * moveSpeed * brakeSpeed * Time.fixedDeltaTime, 0));

        else
            body.AddForce(new Vector2(HorizontalMovement * moveSpeed * Time.fixedDeltaTime, 0));

    }
    public void Move(InputAction.CallbackContext context)
    {
        HorizontalMovement = context.ReadValue<Vector2>().x;
        VerticalMovement = context.ReadValue<Vector2>().y;

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

    public void MomentumShift(InputAction.CallbackContext context)
    {
        if(CanMomentumShift || GameManager.Instance.GodMode)
        {
            body.velocity = new Vector2(0, 0);
            body.velocity = new Vector2(HorizontalMovement * momentumShiftPower, VerticalMovement * momentumShiftPower);
            CanMomentumShift = false;

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

                //Stop character from spinning
                AngularVelocityBeforeSwing = body.angularVelocity;
                body.angularVelocity = 0;

                
                lineRenderer.SetPosition(0, new Vector3(SwingPosition.x, SwingPosition.y, -1f));
                lineRenderer.enabled = true;
            }


        }
        else if (context.ReadValue<float>() == 0f)
        {
            Swinging = false;
            Destroy(SpringJoint);
            body.angularVelocity = AngularVelocityBeforeSwing;
            lineRenderer.enabled = false;

        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == ("Ground"))
        {
            Grounded = true;
            CanMomentumShift = true;

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "Swing")
        {
            CanSwing = true;
            SwingPosition = collision.transform.position;
            CanMomentumShift = true;
        }
        if (collision.gameObject.tag == "WaxObject")
        {
            body.gravityScale = 0f;
            body.velocity = body.velocity * 0.1f ;
            body.angularVelocity = 0f;
            Stuck = true;
            Grounded = true;
            CanMomentumShift = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == ("Swing"))
        {
            CanSwing = false;
        }
        if (collision.gameObject.tag == "WaxObject")
        {
            body.velocity = body.velocity.normalized;
            body.gravityScale = 1f;
            Stuck = false;
        }
    }
}
