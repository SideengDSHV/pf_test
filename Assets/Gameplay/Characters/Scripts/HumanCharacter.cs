using Gameplay.Cameras;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(Animator))] [RequireComponent(typeof(NavMeshAgent))]
    public class HumanCharacter : MonoBehaviour, ICameraTarget
    {
        public Vector3 position => transform.position;

        private HumanController _controller; // Aggregating all movement stuff here

        private void Awake()
        {
            _controller = new HumanController(GetComponent<NavMeshAgent>(), GetComponent<Animator>());
        }

        private void OnEnable() => ICameraTarget.targets.Add(this);
        private void OnDisable() => ICameraTarget.targets.Remove(this);

        private void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(screenRay, out RaycastHit hit))
                    _controller.SetTargetPosition(transform.position, hit.point);
            }

            _controller.SyncVelocity();
        }
    }
}