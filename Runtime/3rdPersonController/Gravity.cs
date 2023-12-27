using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
	[SerializeField]
	private float gravity = -20.0f;
	[SerializeField]
	private float falldelay = 0.25f;
	private float fallTimer = 0.0f;
	[SerializeField]
	private float groundOffset = 0.01f;

	private Vector3 fallVelocity = Vector3.zero;
	private CharacterController characterController = null;
	
	private bool wasFalling = false;
	public event Action OnLanding;
	public event Action OnFallStart;
	
	//Animation Settings
	private Animator animator = null;
	private bool isGrounded = true;
	private bool jump = false;

	private void Awake()
	{
		if (characterController == null)
		{
			characterController = gameObject.GetComponent<CharacterController>();
		}
		
		if (animator == null)
		{
			animator = gameObject.GetComponent<Animator>();
		}
	}
	
	private void OnEnable() {
		OnFallStart += () => { Debug.Log("Falling"); };
		OnLanding += () => { Debug.Log("Landing"); };
	}

	private void FixedUpdate()
	{
		Animate();
		
		// Reset fall velocity if grounded
		// TODO: Use raycast instead of characterController.isGrounded
		if (IsGrounded())
		{
			if (wasFalling)
			{
				OnLanding?.Invoke();
				wasFalling = false;
			}

			fallTimer = 0.0f;
			fallVelocity = Vector3.zero;
			return;
		}

		Fall();
	}

	private bool IsGrounded()
	{
		// Perform a sphere check to check if the character is grounded
		Vector3 sphereCenter = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
		
		if (Physics.CheckSphere(sphereCenter, groundOffset, LayerMask.GetMask("Ground")))
		{
			isGrounded = true;
		}

		else
		{
			isGrounded = false;
		}
		
		return isGrounded;
	}
	
	private void Fall()
	{
		if(!wasFalling)
			fallTimer += Time.deltaTime;
			
		if(fallTimer >= falldelay && !wasFalling)
		{
			OnFallStart?.Invoke();
			wasFalling = true;
		}
		//increase fall velocity if max velocity is not reached
		if (fallVelocity.y > gravity)
		{
			fallVelocity += new Vector3(0f, gravity * Time.deltaTime * 5f, 0f);
		}
		else
		{
			fallVelocity = new Vector3(0f, gravity, 0f);
		}
		
		//apply fall velocity
		characterController.Move(fallVelocity * Time.deltaTime);
	}
	
	private void Animate()
	{
		animator.SetBool("isGrounded", isGrounded);
		animator.SetBool("jump", jump);
	}
	
	private void OnDrawGizmos() {
		Vector3 sphereCenter = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
		Gizmos.DrawSphere(sphereCenter, groundOffset);
	}
}