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
		[Range(5f, 15f)]
		public float runSpeed = 10f;
		
		[Range(2f, 7f)]
		public float walkSpeed = 5f;
		// public float fallingMvtMulti = 0.3f;\
		// public float backRunMulti = -1f;
		// public float backWalkMulti = -0.2f;
		// public float walkMulti = 0.2f;
		// public float runMulti = 1f;
		// public float sprintMulti = 2f;
		public float currentMoveMulti = 0f;
		public float turnSpeed = 15f;

		private bool isMoving = false;

		[Header("Animation Settings")]
		private Animator animator = null;

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
			SetMoveMultiplier();
			isMoving = true;
			StartCoroutine(Move());
		}

		private void StopReadMovement(InputAction.CallbackContext context)
		{
			isMoving = false;
			currentMoveMulti = 0f;
			StopCoroutine(Move());
		}
		
#endregion

		private void SetMoveMultiplier()
		{
			currentMoveMulti = 1f;
		}
		
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
			animator.SetFloat("Speed", currentMoveMulti);
		}
	}
}