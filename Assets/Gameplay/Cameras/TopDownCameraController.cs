using System;
using Cinemachine;
using UnityEngine;

namespace Cameras
{
    public class TopDownCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera cinemachineCamera;
        [SerializeField] private Transform follow;
        [SerializeField] private Transform lookAt;

        [SerializeField] private bool isBoundToGround;
        [SerializeField] private float samplingWidth;
        [SerializeField] private float cameraHeight;

        [SerializeField] private float orbitSpeed;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float scrollSpeed;

        private float _scrollFactor; // 0-1 percentage
        private const float MinDistance = 5f;

        [SerializeField] [Min(MinDistance + 1)]
        private float maxDistance;

        [SerializeField] private AnimationCurve heightProfile;

        private void FixedUpdate()
        {
            bool shift = Input.GetButton("Fire3");
            bool mouseR = Input.GetButton("Fire2");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseScroll = -Input.GetAxis("Mouse ScrollWheel");
            Vector2 inputDirection = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (mouseR) Orbit(mouseX);

            if (mouseScroll != 0f) Zoom(mouseScroll, shift);

            if (inputDirection != Vector2.zero) Translate(inputDirection, shift);
        }

        private void Orbit(float angle)
        {
            float rotationFactor = angle * orbitSpeed;

            lookAt.Rotate(Vector3.up, rotationFactor);
        }

        private void Zoom(float value, bool shift)
        {
            if (shift) value *= 2f;
            _scrollFactor += value * scrollSpeed / maxDistance; // dividing by for consistent steps
            _scrollFactor = Mathf.Clamp01(_scrollFactor);

            float distance = Mathf.Lerp(MinDistance, maxDistance, _scrollFactor);
            float height = heightProfile.Evaluate(_scrollFactor) * distance;
            Vector3 translateFactor = new(0f, height, -distance);

            follow.localPosition = translateFactor;
        }

        private void Translate(Vector2 direction, bool shift)
        {
            if (shift) direction *= 2;

            Vector3 position = lookAt.position; // Starting from the Look At target
            position += lookAt.forward * (direction.y * movementSpeed);
            position += lookAt.right * (direction.x * movementSpeed);


            if (!isBoundToGround) // If snapping isn't used set Y to cameraHeight
            {
                position.y = cameraHeight;
                lookAt.position = position;
                return;
            }

            // Snapping to surface
            Vector3 origin = position; // Originate rays from target position
            origin.y += 100f;          // And shoot them from 100 meters above
            
            Physics.Raycast(origin, Vector3.down, out RaycastHit sample);

            // Aborting translation if sample went out of bounds
            if (!sample.collider) return;

            // Setting position to average of three samples
            lookAt.position = sample.point;
        }
    }
}