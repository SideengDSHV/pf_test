using UnityEngine;

namespace Gameplay.Cameras
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform follow;
        [SerializeField] private Transform lookAt;

        [SerializeField] private bool isBoundToTargets;
        [SerializeField] private float maxDistanceFromTarget;

        [SerializeField] private bool isBoundToGround;
        [SerializeField] private float minCameraHeight;

        [SerializeField] private float orbitSpeed;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float scrollSpeed;


        private float _scrollFactor; // 0-1 percentage
        private const float MinScrollDistance = 5f;

        [SerializeField] [Min(MinScrollDistance + 1)]
        private float maxScrollDistance;

        [SerializeField] private AnimationCurve heightProfile;

        private void OnEnable()
        {
            // Dropping rig into place, since it only updates on input 
            Orbit(0);
            Zoom(0);
            Translate(Vector2.zero);
        }

        private void Update()
        {
            bool shift = Input.GetButton("Fire3");
            bool mouseR = Input.GetButton("Fire2");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseScroll = -Input.GetAxis("Mouse ScrollWheel");
            Vector2 inputDirection = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (mouseR) Orbit(mouseX);

            if (mouseScroll != 0f) Zoom(mouseScroll, shift);

            // Always updating if bound to targets, to get pulled towards them when they move away
            if (isBoundToTargets || inputDirection != Vector2.zero) Translate(inputDirection, shift);
        }


        private void Orbit(float angle)
        {
            float rotationFactor = angle * orbitSpeed;

            // lookAt is a parent of follow, so rotating lookAt makes follow orbit around it
            lookAt.Rotate(Vector3.up, rotationFactor);
        }


        private void Zoom(float value, bool shift = false)
        {
            if (shift) value *= 2f;
            _scrollFactor += value * scrollSpeed / maxScrollDistance; // dividing for consistent step distance
            _scrollFactor = Mathf.Clamp01(_scrollFactor);

            // Converting 0-1 Scroll Factor to an actual distance from target
            float distance = Mathf.Lerp(MinScrollDistance, maxScrollDistance, _scrollFactor);
            float height = heightProfile.Evaluate(_scrollFactor) * distance;
            Vector3 translateFactor = new(0f, height, -distance);

            follow.localPosition = translateFactor;
        }


        private void Translate(Vector2 direction, bool shift = false)
        {
            if (shift) direction *= 2;

            Vector3 targetPosition = lookAt.position; // Starting from the Look At target
            targetPosition += lookAt.forward * (direction.y * movementSpeed);
            targetPosition += lookAt.right * (direction.x * movementSpeed);

            if (isBoundToTargets) targetPosition = SnapToCameraTargets(targetPosition);

            switch (isBoundToGround)
            {
                case true:
                    lookAt.position = SnapToSurface(targetPosition);
                    return;

                case false:
                    targetPosition.y = minCameraHeight;
                    lookAt.position = targetPosition;
                    return;
            }
        }

        private Vector3 SnapToCameraTargets(Vector3 target)
        {
            // Find closest target
            float closestDistance = float.MaxValue;
            Vector3 closestTarget = target;
            foreach (ICameraTarget cameraTarget in ICameraTarget.targets)
            {
                float currentDistance = Vector3.Distance(lookAt.position, cameraTarget.position);
                if (currentDistance > closestDistance) continue;
                closestDistance = currentDistance;
                closestTarget = cameraTarget.position;
            }
            
            // If closest target is in the range, or doesn't exist
            if (closestDistance < maxDistanceFromTarget) return target;

            Vector3 toTarget = (target - closestTarget).normalized;
            return closestTarget + toTarget * maxDistanceFromTarget;
        }

        private Vector3 SnapToSurface(Vector3 position)
        {
            // Originate rays from 100m above the target position
            Vector3 origin = position;
            origin.y += 100f;

            // A third of a turn
            const float tauOver3 = 2.09439510239f;

            // Equilateral triangle
            Vector3 triangleVertex1 = new(Mathf.Sin(tauOver3 * 1), 0, Mathf.Cos(tauOver3 * 1));
            Vector3 triangleVertex2 = new(Mathf.Sin(tauOver3 * 2), 0, Mathf.Cos(tauOver3 * 2));
            Vector3 triangleVertex3 = new(Mathf.Sin(tauOver3 * 3), 0, Mathf.Cos(tauOver3 * 3));

            // Scaling it by MinScrollDistance to avoid terrain clipping
            triangleVertex1 *= MinScrollDistance;
            triangleVertex2 *= MinScrollDistance;
            triangleVertex3 *= MinScrollDistance;

            Physics.Raycast(origin, Vector3.down, out RaycastHit sample0); // Sample straight down as well
            Physics.Raycast(origin + triangleVertex1, Vector3.down, out RaycastHit sample1);
            Physics.Raycast(origin + triangleVertex2, Vector3.down, out RaycastHit sample2);
            Physics.Raycast(origin + triangleVertex3, Vector3.down, out RaycastHit sample3);

            // Setting position height to the highest of four samples
            float highestY = Mathf.Max(
                sample0.point.y,
                sample1.point.y,
                sample2.point.y,
                sample3.point.y,
                minCameraHeight // Or leaving it at minCameraHeight if samples failed
            );
            position.y = highestY;
            return position;
        }
    }
}