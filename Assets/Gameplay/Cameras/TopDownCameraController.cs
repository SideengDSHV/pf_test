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

        [SerializeField] private float orbitSpeed;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float scrollSpeed;

        private float _scrollFactor; // 0-1 percentage
        private const float MinDistance = 5f;

        [SerializeField] [Min(MinDistance + 1)]
        private float maxDistance;

        [SerializeField] private AnimationCurve heightProfile;

        private void Update()
        {
            bool shift = Input.GetButton("Shift");

            // Orbit
            float mouseX = Input.GetAxis("Mouse X");
            if (mouseX != 0f)
            {
                float rotationFactor = mouseX * orbitSpeed;

                lookAt.Rotate(Vector3.up, rotationFactor);
            }


            // Scroll
            float mouseScroll = -Input.GetAxis("Mouse ScrollWheel");
            if (mouseScroll != 0f)
            {
                if (shift) mouseScroll *= 2f;
                _scrollFactor += mouseScroll * scrollSpeed / maxDistance; // dividing by for consistent steps
                _scrollFactor = Mathf.Clamp01(_scrollFactor);

                float distance = Mathf.Lerp(MinDistance, maxDistance, _scrollFactor);
                float height = heightProfile.Evaluate(_scrollFactor) * distance;
                Vector3 translateFactor = new(0f, height, -distance);

                follow.localPosition = translateFactor;
            }

            // Moving
            Vector2 inputDirection = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (inputDirection != Vector2.zero)
            {
                Vector3 translationFactor = new Vector3(inputDirection.x, 0f, inputDirection.y);
                translationFactor *= movementSpeed;
                if (shift) translationFactor *= 2;

                lookAt.Translate(translationFactor);
            }
        }
    }
}