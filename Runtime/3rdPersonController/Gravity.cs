using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
	[SerializeField]
	private float mass = 2f;

	[SerializeField]
	private float gravity = -9.81f;

	private Vector3 fallVelocity = Vector3.zero;
	private CharacterController characterController = null;

	private void Awake()
	{
		if (characterController == null)
		{
			characterController = gameObject.GetComponent<CharacterController>();
		}
	}

	private void Update(){
	
		//reset fall velocity if grounded
		if (characterController.isGrounded)
		{
			fallVelocity = Vector3.zero;
			return;
		}

		Fall();
	}
	
	private void Fall()
	{
		//increase fall velocity if max velocity is not reached
		if (fallVelocity.y > gravity * mass)
		{
			fallVelocity += new Vector3(0f, gravity * mass * Time.deltaTime * 2f, 0f);
		}
		
		//apply fall velocity
		characterController.Move(fallVelocity * Time.deltaTime);
	}
}
