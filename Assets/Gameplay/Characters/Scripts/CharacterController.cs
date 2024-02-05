using System;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(NavMeshAgent))] [RequireComponent(typeof(Animator))]
    public class HumanCharacterController : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Camera _camera;

        [SerializeField] private float animationStoppingDistance;

        private void OnEnable()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Fire1")) SetDestinationToMouse();

            SyncAnimator();
        }

        private void SetDestinationToMouse()
        {
            if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

            // Check if target is too close
            if (Vector3.Distance(hit.point, transform.position) < animationStoppingDistance) return;

            NavMeshPath potentialPath = new();
            _navMeshAgent.CalculatePath(hit.point, potentialPath);

            // Check if path is reachable
            if (_navMeshAgent.pathStatus != NavMeshPathStatus.PathComplete) return;

            _navMeshAgent.SetPath(potentialPath);
        }

        private void SyncAnimator()
        {
            bool _isRunning = _navMeshAgent.hasPath && _navMeshAgent.remainingDistance > animationStoppingDistance;
            _animator.SetBool(IsRunning, _isRunning);
        }
    }
}