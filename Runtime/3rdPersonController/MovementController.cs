using UnityEngine;
using UnityEngine.InputSystem;

namespace Zayene.Character_Controller.Third_Person
{
    public class MovementController : MonoBehaviour
    {
        private ActionMaps actionMaps = null;

        [Header("Movement Settings")]
        [Range(1f, 20f)]
        public float movementSpeed = 10f;

        private bool isMoving = false;

        [Header("Animation Settings")]
        [SerializeField]
        private Animator animator = null;

        [SerializeField]
        private GameObject cameraPositioner;

        private void Awake() {
            if (actionMaps == null) {
                actionMaps = new ActionMaps();
            }

            if (animator == null) {
                animator = gameObject.GetComponent<Animator>();
            }
        }

        private void OnEnable() {
            actionMaps.Enable();

            actionMaps.gameplay.Movement.started += ReadMovement;
            actionMaps.gameplay.Movement.canceled += StopReadMovement;
        }

        private void OnDisable() {
            actionMaps.Disable();

            actionMaps.gameplay.Movement.started -= ReadMovement;
            actionMaps.gameplay.Movement.canceled -= StopReadMovement;
        }

        private void Update() {
            if (isMoving) {
                Debug.Log("Moving");

                Vector2 inputDirection = actionMaps.gameplay.Movement.ReadValue<Vector2>();
                Vector3 moveDirection = cameraPositioner.transform.rotation * new Vector3(inputDirection.x, 0f,inputDirection.y);
                gameObject.GetComponent<CharacterController>().Move(moveDirection * movementSpeed * Time.deltaTime);
            }
        
        }
#region Bound Methods
        private void ReadMovement(InputAction.CallbackContext context){
            Debug.Log("Move");
            isMoving = true;
        }

        private void StopReadMovement(InputAction.CallbackContext context){
            Debug.Log("Stop");
            isMoving = false;
        }
#endregion
    }
}