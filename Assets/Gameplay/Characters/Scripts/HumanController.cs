using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Gameplay.Characters
{
    /// <summary>
    /// Movement-related operations with HumanCharacter
    /// </summary>
    internal class HumanController
    {
        private const float MinTravelDistance = 2f;

        private static readonly int Velocity = Animator.StringToHash("velocity");
        private static readonly int CycleOffset = Animator.StringToHash("cycleOffset");

        private readonly NavMeshAgent _agent;
        private readonly Animator _animator;

        public HumanController(NavMeshAgent agent, Animator animator)
        {
            _agent = agent;
            _animator = animator;
            _animator.SetFloat(CycleOffset, Random.Range(0f, 1f));
        }

        internal void SetTargetPosition(Vector3 startPosition, Vector3 targetPosition)
        {
            // Check if target is too close
            if (Vector3.Distance(targetPosition, startPosition) < MinTravelDistance) return;

            NavMeshPath potentialPath = new();
            _agent.CalculatePath(targetPosition, potentialPath);

            // Check if path is reachable
            if (_agent.pathStatus != NavMeshPathStatus.PathComplete) return;

            _agent.SetPath(potentialPath);
        }

        internal void SyncVelocity()
        {
            _animator.SetFloat(Velocity, _agent.velocity.magnitude / _agent.speed);
        }
    }
}