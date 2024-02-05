using System;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(NavMeshAgent))] [RequireComponent(typeof(Animator))]
    public class HumanCharacterController : MonoBehaviour
    {
        private static readonly int Velocity = Animator.StringToHash("velocity");

        private NavMeshAgent _agent;
        private Animator _animator;
        private Camera _camera;

        [SerializeField] private float minTravelDistance;

        private void OnEnable()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire1")) SetDestinationToMouse();
            
            _animator.SetFloat(Velocity, _agent.velocity.magnitude / _agent.speed);
        }

        private void SetDestinationToMouse()
        {
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

            // Check if target is too close
            if (Vector3.Distance(hit.point, transform.position) < minTravelDistance) return;

            NavMeshPath potentialPath = new();
            _agent.CalculatePath(hit.point, potentialPath);

            // Check if path is reachable
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete) return;

            _agent.SetPath(potentialPath);
        }
    }
}