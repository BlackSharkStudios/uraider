﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float sprintSpeed = 5.0f;
    public float runSpeed = 3.0f;
    public float walkSpeed = 1.2f;
    public float gravity = 14.0f;
    public float jumpYVel = 5.8f;
    public float jumpZVel = 4f;
    public float sJumpYVel = 3f;
    public float sJumpZVel = 2.4f;
    public float interpolationRate = 8f;

    public CameraController camController;

    private IPlayerState currentState;
    private CharacterController charControl;
    private Transform cam;
    private Animator anim;

    private bool isGrounded = true;
    private Vector3 velocity;

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        anim = GetComponent<Animator>();
        velocity = Vector3.zero;
        currentState = Locomotion.Instance;
        currentState.OnEnter(this);
    }

    private void Update()
    {
        isGrounded = charControl.isGrounded && velocity.y <= 0.0f;
        anim.SetBool("isGrounded", isGrounded);

        currentState.Update(this);

        AnimatorStateInfo animState = anim.GetCurrentAnimatorStateInfo(0);
        float animTime = animState.normalizedTime <= 1.0f ? animState.normalizedTime
            : animState.normalizedTime % (int)animState.normalizedTime;
        anim.SetFloat("AnimTime", animTime);  // Used for determining certain transitions
        
        if (charControl.enabled)
            charControl.Move(velocity * Time.deltaTime);
    }

    public void MoveFree()
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.right;

        float moveSpeed = Input.GetKey(KeyCode.LeftControl) ? walkSpeed
            : runSpeed;

        Vector3 targetVector = camForward * Input.GetAxisRaw("Vertical")
            + camRight * Input.GetAxisRaw("Horizontal");
        if (targetVector.magnitude > 1.0f)
            targetVector = targetVector.normalized;
        targetVector.y = 0f;
        targetVector *= moveSpeed;

        velocity.y = 0f; // So slerp is correct

        velocity = Vector3.Slerp(velocity, targetVector, Time.deltaTime * interpolationRate);
        anim.SetFloat("Speed", UMath.GetHorizontalMag(velocity));
        anim.SetFloat("TargetSpeed", UMath.GetHorizontalMag(targetVector));

        velocity.y = -gravity;  // so charControl is grounded consistently
    }

    public void RotateToVelocityGround()
    {
        if (UMath.GetHorizontalMag(velocity) > 0.1f)
        {
            Quaternion target = Quaternion.Euler(0.0f, Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg, 0.0f);
            transform.rotation = target;
        }
    }

    public void RotateToTarget(Vector3 target)
    {
        Vector3 direction = Vector3.Scale((target - transform.position), new Vector3(1.0f, 0.0f, 1.0f));
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void MinimizeCollider()
    {
        charControl.radius = 0f;
    }

    public void MaximizeCollider()
    {
        charControl.radius = 0.2f;
    }

    public void DisableCharControl()
    {
        charControl.enabled = false;
    }

    public void EnableCharControl()
    {
        charControl.enabled = true;
    }

    public IPlayerState State
    {
        get { return currentState; }
        set
        {
            currentState.OnExit(this);
            currentState = value;
            currentState.OnEnter(this);
        }
    }

    public CharacterController Controller
    {
        get { return charControl; }
    }

    public Transform Cam
    {
        get { return cam; }
    }

    public Animator Anim
    {
        get { return anim; }
    }

    public bool Grounded
    {
        get { return isGrounded; }
    }

    public Vector3 Velocity
    {
        get { return velocity; }
        set { velocity = value; }
    }
}