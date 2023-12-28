using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zayene.Character_Controller.Third_Person
{
	public class Gravity : MonoBehaviour
	{
		private ActionMaps actionMaps = null;
		
		[SerializeField]
		private float gravity = -20.0f;
		[SerializeField]
		private float falldelay = 0.25f;
		private float fallTimer = 0.0f;
		[SerializeField]
		private float groundOffset = 0.01f;
		private bool isGrounded = true;
		private bool canJump = true;
		
		[SerializeField]
		private float jumpHeight = 2f;

		private Vector3 fallVelocity = Vector3.zero;
		private CharacterController characterController = null;
		
		private bool wasFalling = false;
		public event Action OnLanding;
		public event Action OnFallStart;
		
		//Animation Settings
		private Animator animator = null;
		private bool isLanding = false;
		private bool jump = false;

		private void Awake()
		{
			if (actionMaps == null) 
			{
				actionMaps = new ActionMaps();
			}
			
			if (characterController == null)
			{
				characterController = gameObject.GetComponent<CharacterController>();
			}
			
			if (animator == null)
			{
				animator = gameObject.GetComponent<Animator>();
			}
		}
		
		private void OnEnable() 
		{
			actionMaps.Enable();
			
			actionMaps.gameplay.Jump.performed += ReadJump;
		}
		
		private void OnDisable()
		{
			actionMaps.Disable();
			
			actionMaps.gameplay.Jump.performed -= ReadJump;
		}

		private void FixedUpdate()
		{
			isGrounded = IsGrounded(groundOffset);
			isLanding = IsGrounded(groundOffset * 15f);
			Animate();
			
			// Reset fall velocity if grounded
			// TODO: Use raycast instead of characterController.isGrounded
			if (isGrounded)
			{
				if (wasFalling && isLanding)
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
		
		private void ReadJump(InputAction.CallbackContext context)
		{
			if (canJump && isGrounded)
			{
				StartCoroutine(Jump());
			}
		}

		private bool IsGrounded(float offset)
		{
			// Perform a sphere check to check if the character is grounded
			Vector3 sphereCenter = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
			
			if (Physics.CheckSphere(sphereCenter, offset, LayerMask.GetMask("Ground")))
			{
				return true;
			}
			
			return false;
		}
		
		private void Fall()
		{
			//detect fall with delay
			if(!wasFalling)
				fallTimer += Time.deltaTime;
				
			if(fallTimer >= falldelay && !wasFalling)
			{
				OnFallStart?.Invoke();
				wasFalling = true;
			}
			
			//smooth fall speed
			if(fallVelocity.y < gravity)
			{
				fallVelocity.y = gravity;
			}
			else
			{
				fallVelocity.y = Mathf.Lerp(fallVelocity.y, gravity, Time.deltaTime * 2f);
			}
			
			//apply fall velocity
			characterController.Move(fallVelocity * Time.deltaTime);
		}
		
		private IEnumerator Jump()
		{
			float jumpTargetVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
			
			float jumpTimer = 0f;
			
			while(jumpTimer < 1f)
			{
				jumpTimer += Time.deltaTime;
				characterController.Move(Vector3.up * jumpTargetVelocity * Time.deltaTime);
				yield return new WaitForEndOfFrame();
			}
			yield return null;
		}
		
		private void Animate()
		{
			animator.SetBool("isGrounded", isLanding);
			animator.SetBool("jump", jump);
		}
		
		private void OnDrawGizmos() {
			Vector3 sphereCenter = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
			Gizmos.DrawSphere(sphereCenter, groundOffset);
			Gizmos.DrawSphere(sphereCenter, groundOffset * 10f);
		}
	}
}