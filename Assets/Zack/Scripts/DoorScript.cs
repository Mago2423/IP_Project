using UnityEngine;

public class DoorScript : MonoBehaviour
{
	[SerializeField] private Animator doorAnimator;
	[SerializeField] private string openBoolName = "IsOpen";
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private string npcTag = "NPC";



/// <summary>
/// Keeps track of the number of characters currently in the trigger area. The door will remain open
/// as long as there is at least one character in the area.
/// </summary>
	private int CharacterCount;

	private void Awake()
	{
		if (doorAnimator == null)
		{
			doorAnimator = GetComponent<Animator>();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsValidOpener(other))
		{
			return;
		}

		CharacterCount++;
		SetDoorOpen(true);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsValidOpener(other))
		{
			return;
		}

		CharacterCount = Mathf.Max(0, CharacterCount - 1);
		if (CharacterCount == 0)
		{
			SetDoorOpen(false);
		}
	}

	private bool IsValidOpener(Collider other)
	{
		return other.CompareTag(playerTag) || other.CompareTag(npcTag);
	}

	private void SetDoorOpen(bool isOpen)
	{
		if (doorAnimator == null)
		{
			return;
		}

		doorAnimator.SetBool(openBoolName, isOpen);
	}
}
