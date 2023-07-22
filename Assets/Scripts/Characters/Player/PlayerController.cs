using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experimental.Physics;
using Utilities;

public class PlayerController : MonoBehaviour
{
    public float cameraSensitivity = 90;
    public float climbSpeed = 1;
    public float normalMoveSpeed = 10;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;
    public bool isFlying = false;
    public bool isGrounded = true;
    public float m_StickToGroundForce = 0.5f;
    public float m_GravityMultiplier = 1f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    [SerializeField]
    private Vector3 m_Velocity = Vector3.zero;
    private float speed;

    public Vector3 Velocity { get { return m_Velocity; } }
    public  Vector3 PreviousPosition { get; private set; }

    void Start()
    {
        rotationX = transform.eulerAngles.y;
        rotationY = -transform.eulerAngles.x;
    }

    void Update()
    {
        // for other components
        PreviousPosition = transform.position;

        rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

        m_Velocity.x = Input.GetAxisRaw("Horizontal");
        m_Velocity.z = Input.GetAxisRaw("Vertical");
        speed = (Input.GetKey(KeyCode.LeftShift) ? normalMoveSpeed * fastMoveFactor : (Input.GetKey(KeyCode.LeftControl) ? normalMoveSpeed * slowMoveFactor : normalMoveSpeed));
        if (m_Velocity.sqrMagnitude > 1)
            m_Velocity.Normalize();

        // transform the vector so "forward" and "right" is relative to the player and not the world
        m_Velocity = transform.forward * m_Velocity.z + transform.right * m_Velocity.x;
        if (!isFlying)
        {
            // prevent "fly-cam" by projecting our velocity along the ground's normal
            // we're using boxels and don't have any wall-climbing or anti-grav boots so the ground's normal is always up
            m_Velocity = Vector3.ProjectOnPlane(m_Velocity, Vector3.up).normalized;
            // we can do gravity and jumping here too since they only work when you're not flying
            if (isGrounded)
            {
                // apply a constant downward force so that you stick to the ground
                // different from gravity: should be a very, very small force to prevent pushing us THROUGH the ground.
                m_Velocity.y = -m_StickToGroundForce;
            }
            else
            {
                // NOTE: multiplier so we can make some characters immune to gravity
                // also we can set Physics.gravity to push on multiple axes, which could be fun to experiment with
                m_Velocity += Physics.gravity * m_GravityMultiplier * Time.deltaTime;
            }
            m_Velocity.x *= speed;
            m_Velocity.z *= speed;
        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
                m_Velocity += Vector3.up * climbSpeed;
            if (Input.GetKey(KeyCode.C))
                m_Velocity -= Vector3.up * climbSpeed;
            m_Velocity *= speed;
        }

        // finally, move the player according to their velocity (smoothed over frame)
        transform.position += (m_Velocity * Time.deltaTime);
    }
}