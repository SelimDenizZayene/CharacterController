using UnityEngine;

namespace Zayene.Character_Controller.Third_Person
{
    public class LookAtTargetPositioner : MonoBehaviour
    {
        public Transform referenceTransform;
        public Vector3 Offset;
        void Update()
        {
            transform.position = referenceTransform.position + referenceTransform.rotation * Offset;
        }
    }
}