using Codice.Client.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Time = UnityEngine.Time;

namespace Zayene.CharacterController
{
    public class CameraController : MonoBehaviour
    {
        private ActionMaps actionMaps = null;
        private Transform followTarget = null;

#region Camera Settings
        [Header("Camera Settings")]
        [Range(1f, 20f)]
        public float rotationSpeed = 10f;

        [Range(0f,80f)]
        public float maxPitch = 60f;
        
        [Range(-80f,0f)]
        public float minPitch = -60f;
#endregion

        private void Awake() {
            if (actionMaps == null) {
                actionMaps = new ActionMaps();
            }
            if (followTarget == null) {
                followTarget = gameObject.GetComponent<CinemachineVirtualCamera>().Follow;
            }
        }

        private void OnEnable() {
            actionMaps.Enable();

            actionMaps.gameplay.Camera.performed += MoveCamera;
        }

        private void OnDisable() {
            actionMaps.Disable();

            actionMaps.gameplay.Camera.performed -= MoveCamera;
        }

        private void MoveCamera(InputAction.CallbackContext context)
        {
            Vector2 moveDirection = context.ReadValue<Vector2>();
            
            //calculate rotation
            float xRot = moveDirection.y * rotationSpeed/100f * (-1f);
            float yRot = moveDirection.x * rotationSpeed/100f;

            float rotatedX = followTarget.localRotation.eulerAngles.x + xRot;
            float rotatedY = followTarget.localRotation.eulerAngles.y + yRot;

            //limit vertical rotation
            if(rotatedX > 180f) {
                rotatedX -= 360f;
            }
            float clampedX = Mathf.Clamp(rotatedX, minPitch, maxPitch);
            
            //apply rotation
            followTarget.localRotation = Quaternion.Euler(clampedX , rotatedY, 0);
        }
    }
}