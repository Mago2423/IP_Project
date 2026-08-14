/// <summary>
/// Author: Zack
/// StudentNo: 10274404J
/// Purpose:
/// Provides the core implementation for DoorScript.
/// </summary>

using UnityEngine;

/// <summary>
/// Controls door interaction and animation state for playable scene objects.
/// </summary>
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

/// <summary>
/// Initializes the controller references and setup state.
/// </summary>
	private void Awake()
	{
		if (doorAnimator == null)
		{
			doorAnimator = GetComponent<Animator>();
		}
	}

/// <summary>
/// Performs the on trigger enter action.
/// </summary>
	private void OnTriggerEnter(Collider other)
	{
		if (!IsValidOpener(other))
		{
			return;
		}

		CharacterCount++;
		SetDoorOpen(true);
	}

/// <summary>
/// Performs the on trigger exit action.
/// </summary>
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

/// <summary>
/// Performs the is valid opener action.
/// </summary>
	private bool IsValidOpener(Collider other)
	{
		return other.CompareTag(playerTag) || other.CompareTag(npcTag);
	}

/// <summary>
/// Performs the set door open action.
/// </summary>
	private void SetDoorOpen(bool isOpen)
	{
		if (doorAnimator == null)
		{
			return;
		}

		doorAnimator.SetBool(openBoolName, isOpen);
	}
}
