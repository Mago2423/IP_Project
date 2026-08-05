using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class CameraLookOnlyController : MonoBehaviour
	{
		[Header("Camera")]
		[Tooltip("The follow target used by Cinemachine")]
		public GameObject CinemachineCameraTarget;
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
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif

		private float _cinemachineTargetPitch;
		private float _currentYaw;
		private float _startYaw;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		private void Start()
		{
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#endif

			_startYaw = NormalizeAngle(transform.eulerAngles.y);
			_currentYaw = _startYaw;

			if (CinemachineCameraTarget != null)
			{
				_cinemachineTargetPitch = NormalizeAngle(CinemachineCameraTarget.transform.localEulerAngles.x);
			}
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
			if (_input.look.sqrMagnitude < _threshold)
			{
				return;
			}

			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

			_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
			_currentYaw += _input.look.x * RotationSpeed * deltaTimeMultiplier;

			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
			_currentYaw = Mathf.Clamp(_currentYaw, _startYaw + LeftClamp, _startYaw + RightClamp);

			if (CinemachineCameraTarget != null)
			{
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
			}

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
