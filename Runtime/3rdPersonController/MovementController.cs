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
		[Range(2f, 7f)]
		private float walkSpeed = 2f;
		
		[Range(4f, 14f)]
		private float runSpeed = 4f;

		[Range(6f, 21f)]
		private float sprintSpeed = 6f;
		public float turnSpeed = 15f;

		//
		private float currentSpeed = 0f;
		private float targetSpeed = 0f;
		private bool isMoving = false;

		[Header("Animation Settings")]
		private Animator animator = null;
		private float speed = 0f;

		[SerializeField]
		private GameObject cameraPositioner;

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
		}

		private void OnDisable()
		{
			actionMaps.Disable();

			actionMaps.gameplay.Movement.started -= ReadMovement;
			actionMaps.gameplay.Movement.canceled -= StopReadMovement;
		}

		private void Update()
		{
			Animate();
		}
		
#region Bound Methods
		
		private void ReadMovement(InputAction.CallbackContext context)
		{
			isMoving = true;
			StartCoroutine(Move());
		}

		private void StopReadMovement(InputAction.CallbackContext context)
		{
			isMoving = false;
			StopCoroutine(Move());
		}
		
#endregion
		
		private IEnumerator Move()
		{
			while(isMoving)
			{
				Vector2 inputDirection = actionMaps.gameplay.Movement.ReadValue<Vector2>();
				Vector3 cameraRotation = cameraPositioner.transform.forward;
				cameraRotation.y = 0f; // ignore vertical rotation

				Vector3 moveDirection = cameraPositioner.transform.rotation * new Vector3(inputDirection.x, 0f, inputDirection.y);
				moveDirection.y = 0f; // ignore vertical movement
				
				//rotate player to movedirection and dont rotate camera
				Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
				transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
			
				//Todo: add different speeds
				characterController.Move(moveDirection * runSpeed * Time.deltaTime);
				
				yield return new WaitForEndOfFrame();
			}
			yield return null;
		}

		private void Animate()
		{
			speed = isMoving ? 0.66f : 0f;
			
			animator.SetFloat("Speed", speed);
		}
	}
}