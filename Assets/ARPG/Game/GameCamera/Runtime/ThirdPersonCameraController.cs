using System;
using UnityEngine;

namespace ARPG.Game.GameCamera
{
    public sealed class ThirdPersonCameraController
        : MonoBehaviour
    {
        [Header("Target")]

        [SerializeField]
        private Transform _target;

        [Header("Orbit")]

        [SerializeField]
        private float _distance = 6f;

        [SerializeField]
        private float _height = 1.5f;

        [SerializeField]
        private float _sensitivity = 0.15f;

        [SerializeField]
        private float _minPitch = -30f;

        [SerializeField]
        private float _maxPitch = 70f;

        private ThirdPersonCameraInputReader
            _inputReader;

        private float _yaw;
        private float _pitch;

        public void Bind(
            Transform target)
        {
            _target =
                target
                ? target
                : throw new ArgumentNullException(
                    nameof(target));
        }

        public void Unbind()
        {
            _target = null;
        }

        private void Awake()
        {
            _inputReader =
                new ThirdPersonCameraInputReader();

            Vector3 euler =
                transform.eulerAngles;

            _yaw = euler.y;
            _pitch = NormalizePitch(
                euler.x);
        }

        private void OnEnable()
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            UpdateOrbit();
            UpdateCameraTransform();
        }

        private void UpdateOrbit()
        {
            Vector2 lookDelta =
                _inputReader.ReadLookDelta();

            _yaw +=
                lookDelta.x *
                _sensitivity;

            _pitch -=
                lookDelta.y *
                _sensitivity;

            _pitch =
                Mathf.Clamp(
                    _pitch,
                    _minPitch,
                    _maxPitch);
        }

        private void UpdateCameraTransform()
        {
            Quaternion rotation =
                Quaternion.Euler(
                    _pitch,
                    _yaw,
                    0f);

            Vector3 focusPoint =
                _target.position +
                Vector3.up * _height;

            Vector3 offset =
                rotation *
                new Vector3(
                    0f,
                    0f,
                    -_distance);

            transform.position =
                focusPoint +
                offset;

            transform.rotation =
                rotation;
        }

        private static float NormalizePitch(
            float pitch)
        {
            if (pitch > 180f)
            {
                pitch -= 360f;
            }

            return pitch;
        }
    }
}