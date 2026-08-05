using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	[RequireComponent(typeof(PlayerInput))]
	public class CameraLookOnlyController : MonoBehaviour
	{
		[Header("Camera")]
		[Tooltip("The follow target used by Cinemachine")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("The actual camera to apply pitch to (e.g. MainCamera child)")]
		public Camera PitchCamera;
		[Tooltip("World Y angle the camera faces at start, independent of object placement")]
		public float StartingYaw = 0.0f;
		[Tooltip("Rotation speed of the look input")]
		public float RotationSpeed = 1.0f;

		[Header("Vertical Clamp")]
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		[Header("Horizontal Clamp (180 Total)")]
		[Tooltip("Left limit from the starting yaw")]
		public float LeftClamp = -90.0f;
		[Tooltip("Right limit from the starting yaw")]
		public float RightClamp = 90.0f;

		private StarterAssetsInputs _input;
		private PlayerInput _playerInput;

		// skip look input for a couple frames after cursor re-locks to absorb the snap delta
		private int _skipLookFrames;
		private bool _wasLookEnabled;

		private float _cinemachineTargetPitch;
		private float _currentYaw;
		private float _startYaw;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse => _playerInput.currentControlScheme == "KeyboardMouse";

		private void Start()
		{
			_input = GetComponent<StarterAssetsInputs>();
			_playerInput = GetComponent<PlayerInput>();

			if (PitchCamera == null)
				PitchCamera = GetComponentInChildren<Camera>();

			_startYaw = StartingYaw;
			_currentYaw = _startYaw;
			_cinemachineTargetPitch = 0.0f;

			// snap transforms to starting orientation immediately
			transform.rotation = Quaternion.Euler(0.0f, _currentYaw, 0.0f);
		}

		private void LateUpdate()
		{
			if (_input == null)
			{
				return;
			}

			// Keep movement/jump/sprint disabled for this camera-only controller.
			_input.move = Vector2.zero;
			_input.jump = false;
			_input.sprint = false;

			CameraRotation();
		}

		private void CameraRotation()
		{
			bool lookEnabled = _input.cursorInputForLook;
			if (lookEnabled && !_wasLookEnabled)
				_skipLookFrames = 2;
			_wasLookEnabled = lookEnabled;

			if (_skipLookFrames > 0)
			{
				_skipLookFrames--;
				_input.look = Vector2.zero;
				return;
			}

			if (_input.look.sqrMagnitude < _threshold)
			{
				return;
			}

			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

			_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
			_currentYaw += _input.look.x * RotationSpeed * deltaTimeMultiplier;

			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
			_currentYaw = Mathf.Clamp(_currentYaw, _startYaw + LeftClamp, _startYaw + RightClamp);

			var pitchRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
			if (CinemachineCameraTarget != null)
				CinemachineCameraTarget.transform.localRotation = pitchRotation;
			if (PitchCamera != null)
				PitchCamera.transform.localRotation = pitchRotation;

			transform.rotation = Quaternion.Euler(0.0f, _currentYaw, 0.0f);
		}

		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return Mathf.Clamp(angle, min, max);
		}

		private static float NormalizeAngle(float angle)
		{
			while (angle > 180f) angle -= 360f;
			while (angle < -180f) angle += 360f;
			return angle;
		}
	}
}
