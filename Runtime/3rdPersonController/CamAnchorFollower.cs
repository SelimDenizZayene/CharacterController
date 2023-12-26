using UnityEngine;

public class CamAnchorFollower : MonoBehaviour
{
	[SerializeField]
	private Transform anchor;
	void Update()
	{
		transform.position = anchor.position;
	}
}
