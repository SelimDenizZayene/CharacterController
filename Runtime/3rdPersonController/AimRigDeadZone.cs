using UnityEngine;

namespace Zayene.Character_Controller.Third_Person
{
	public class AimRigDeadZone : MonoBehaviour
	{
		[SerializeField]
		private Transform lookAtTarget = null;
		
		private void Update() {
			if(lookAtTarget != null) {
				float relativeYRot = transform.rotation.z - lookAtTarget.rotation.z;
				if (( relativeYRot > 120f) || (relativeYRot < 240f)) {
					Debug.Log("dead zone");
					gameObject.SetActive(false);
				} else {
					Debug.Log("not dead zone");
					gameObject.SetActive(true);
				}
			}
		}
		
	}
}

