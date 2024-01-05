using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Zayene.Character_Controller.Third_Person
{
	//fix: when quickly changing directions, movement becomes very fast until stopping
	public class MovementController : MonoBehaviour
	{
		private ActionMaps actionMaps = null;
		private CharacterController characterController = null;

		[Header("Movement Settings")]
		[Space(5f)]
		[SerializeField]
		[Range(1f, 7f)]
		private float walkSpeed = 2f;
		
		[SerializeField]
		[Range(2f, 14f)]
		private float runSpeed = 4f;

		[SerializeField]
		[Range(3f, 21f)]
		private float sprintSpeed = 6f;
		
		[SerializeField]
		[Tooltip("the speed at which the player rotates towards the movement direction")]
		public float turnSpeed = 15f;

		//Movement Variables
		private bool isMoving = false; //for reading input
		private bool isSprinting = false; //priority over isWalking
		private bool isWalking = false;
		private float currentSpeed = 0f; //for smooth speed change
		private float targetSpeed = 4f;
		private Vector3 moveVector = Vector3.zero;
		private Vector3 currentMoveVector = Vector3.zero;
		
		[SerializeField]
		[Tooltip("Higher value = faster speed change")]
		private float SpeedChangeFactor = 10f;
		
		[Header("Gravity Settings")]
		[Space(5f)]
		[Tooltip("The gravity applied to the character in fixed update")]
		[SerializeField]
		private float gravity = -20.0f;
		[SerializeField]
		[Tooltip("Time until the player starts falling after walking off a ledge")]
		private float falldelay = 0.25f;
		
		[SerializeField]
		private float groundOffset = 0.01f;
				
		[SerializeField]
		private float jumpForce = 2f;
		
		[SerializeField]
		private float jumpDelay = 0.4f;
		
		//Gravity Variables
		private float fallTimer = 0.0f;
		private bool isGrounded = true;
		private bool canJump = true;
		private Vector3 fallVelocity = Vector3.zero;
		private bool wasFalling = false;

		[Header("Animation Settings")]
		[Space(5f)]
		private Animator animator = null;
		private float speed = 0f;
		private bool isLanding = false;
		private bool jump = false;
		
		[Header("Camera")]
		[Space(5f)]
		[SerializeField]
		private GameObject cameraPositioner;
		
		//Events
		public event Action OnLanding;
		public event Action OnFallStart;
		
#region Initialization

		private void Awake()
		{
			if (actionMaps == null) 
			{
				actionMaps = new ActionMaps();
			}

			if (animator == null)
			{
				animator = gameObject.GetComponent<Animator>();
			}

			if(characterController == null)
			{
				characterController = gameObject.GetComponent<CharacterController>();
			}
		}

		private void OnEnable()
		{
			actionMaps.Enable();

			actionMaps.gameplay.Movement.started += ReadMovement;
			actionMaps.gameplay.Movement.canceled += StopReadMovement;
			
			actionMaps.gameplay.Jump.performed += ReadJump;
			
			actionMaps.gameplay.Sprint.started += ReadSprint;
			actionMaps.gameplay.Sprint.canceled += StopReadSprint;
			
			actionMaps.gameplay.Walk.started += ReadWalk;
			actionMaps.gameplay.Walk.canceled += StopReadWalk;
			
			OnLanding += Land;
		}

		private void OnDisable()
		{
			actionMaps.Disable();

			actionMaps.gameplay.Movement.started -= ReadMovement;
			actionMaps.gameplay.Movement.canceled -= StopReadMovement;
			
			actionMaps.gameplay.Jump.performed -= ReadJump;
			
			actionMaps.gameplay.Sprint.started -= ReadSprint;
			actionMaps.gameplay.Sprint.canceled -= StopReadSprint;
			
			actionMaps.gameplay.Walk.started -= ReadWalk;
			actionMaps.gameplay.Walk.canceled -= StopReadWalk;
			
			OnLanding -= Land;
		}
		
#endregion

		private void FixedUpdate()
		{
			isGrounded = IsGrounded(groundOffset);
			isLanding = IsGrounded(groundOffset * 15f);
			
			if(isMoving)
			{
				CalculateMove();
			}
			else
			{
				// moveVector.x = 0f;
				// moveVector.z = 0f;
				// currentSpeed = 0f;
				targetSpeed = 0f;
			}
			
			if (isGrounded)
			{
				if (wasFalling)
				{
					OnLanding?.Invoke();
					wasFalling = false;
				}

				if(!jump)
				{
					fallTimer = 0.0f;
					fallVelocity = Vector3.zero;
				}
			}
			else
			{
				CalculateFall();
			}
			
			ApplyMove();
			Animate();
		}
		
#region Bound Methods
		
		private void ReadMovement(InputAction.CallbackContext context)
		{
			isMoving = true;
			CalculateMovementMode();
		}

		private void StopReadMovement(InputAction.CallbackContext context)
		{
			isMoving = false;
			CalculateMovementMode();
		}
		
		private void ReadSprint(InputAction.CallbackContext context)
		{
			isSprinting = true;
			CalculateMovementMode();
		}
		
		private void StopReadSprint(InputAction.CallbackContext context)
		{
			isSprinting = false;
			CalculateMovementMode();
		}
		
		private void ReadWalk(InputAction.CallbackContext context)
		{
			isWalking = true;
			CalculateMovementMode();
		}
		
		private void StopReadWalk(InputAction.CallbackContext context)
		{
			isWalking = false;
			CalculateMovementMode();
		}
		
		private void ReadJump(InputAction.CallbackContext context)
		{
			if (canJump && isGrounded)
			{
				StartCoroutine(CalculateJump());
			}
		}
		
		private void Land()
		{
			StartCoroutine(CalculateJumpDelay());
			StartCoroutine(CalculateMoveDelay());
		}
		
#endregion
		
		
		private void ApplyMove()
		{
			//smooth speed change
			currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * SpeedChangeFactor);
			
			//read movement from input and gravity
			currentMoveVector.x = moveVector.x * currentSpeed;
			currentMoveVector.z = moveVector.z * currentSpeed;
			currentMoveVector.y = fallVelocity.y;
			
			//apply movement
			characterController.Move(currentMoveVector * Time.deltaTime);
		}
		
#region Movement Calculations
		
		private void CalculateMove()
		{
			Vector2 inputDirection = actionMaps.gameplay.Movement.ReadValue<Vector2>();

			Vector3 moveDirection = cameraPositioner.transform.rotation * new Vector3(inputDirection.x, 0f, inputDirection.y);
			moveDirection.y = 0f;
			
			// ignore vertical movement
			moveVector.x = moveDirection.x;
			moveVector.z = moveDirection.z;
			
			//rotate player to movedirection and dont rotate camera
			Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
			transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
		}
		
		private void CalculateMovementMode()
		{
			if(isSprinting)
			{
				targetSpeed = sprintSpeed;
			}
			else if(isWalking)
			{
				targetSpeed = walkSpeed;
			}
			else
			{
				targetSpeed = runSpeed;
			}
		}
		
		private Vector3 CalculateFall()
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
				return fallVelocity;
			}
			else
			{
				fallVelocity.y = Mathf.Lerp(fallVelocity.y, gravity, Time.deltaTime * 2f);
				return fallVelocity;
			}
		}
		
		private IEnumerator CalculateJump()
		{
			float jumpTargetVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
			
			float jumptimer = 0.0f;
			
			jump = true;
			while(jumptimer < 0.1f)
			{
				jumptimer += Time.deltaTime;
				fallVelocity.y = jumpTargetVelocity;
				yield return new WaitForEndOfFrame();
			}
			jump = false;
			yield return null;
		}
		
		private IEnumerator CalculateJumpDelay()
		{
			canJump = false;
			
			float JumpDelayTimer = 0.0f;
			while(JumpDelayTimer < jumpDelay)
			{
				JumpDelayTimer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}

			canJump = true;
			yield return null;
		}
		
		private IEnumerator CalculateMoveDelay()
		{
			actionMaps.gameplay.Movement.Disable();
			float MoveDelayTimer = 0.0f;
			while(MoveDelayTimer < jumpDelay/2f)
			{
				MoveDelayTimer += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
			actionMaps.gameplay.Movement.Enable();
			yield return null;
		}
		
#endregion
		
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
		
		private void Animate()
		{
			//normalize speed for animation
			speed = currentSpeed / sprintSpeed;
			
			animator.SetFloat("Speed", speed);
			animator.SetBool("isGrounded", isLanding);
			animator.SetBool("jump", jump);
		}
	}
}