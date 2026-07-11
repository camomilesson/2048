using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace TwentyFortyEight.UI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class RandomCatWander : MonoBehaviour
    {
        [Header("Wander Area")]
        [SerializeField] private Collider wanderArea;
        [SerializeField] private float navMeshSampleDistance = 1f;
        [SerializeField] private int maximumPointAttempts = 20;

        [Header("Timing")]
        [SerializeField] private float minimumIdleTime = 1f;
        [SerializeField] private float maximumIdleTime = 3f;

        [Header("Arrival")]
        [SerializeField] private float arrivalTolerance = 0.1f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private float animationDampTime = 0.08f;
        [SerializeField] private float minimumMovingSpeed = 0.05f;

        private NavMeshAgent agent;
        private int speedParameterHash;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            ValidateSettings();

            speedParameterHash =
                Animator.StringToHash(speedParameterName);

            // The NavMeshAgent moves the wrapper object.
            // The animation should only animate the cat visually.
            animator.applyRootMotion = false;
        }

        private void Update()
        {
            UpdateAnimation();
        }

        private IEnumerator Start()
        {
            /*
             * Wait one frame so the scene's NavMesh has been
             * enabled before checking the agent.
             */
            yield return null;

            if (!EnsureAgentIsOnNavMesh())
            {
                Debug.LogError(
                    "RandomCatWander could not place the cat on a NavMesh.",
                    this
                );

                enabled = false;
                yield break;
            }

            while (enabled)
            {
                yield return WaitAtCurrentPosition();

                if (!TryChooseDestination(out Vector3 destination))
                {
                    Debug.LogWarning(
                        "RandomCatWander could not find a valid destination.",
                        this
                    );

                    yield return null;
                    continue;
                }

                bool destinationAccepted =
                    agent.SetDestination(destination);

                if (!destinationAccepted)
                {
                    yield return null;
                    continue;
                }

                while (
                    enabled &&
                    agent.isOnNavMesh &&
                    agent.pathPending
                )
                {
                    yield return null;
                }

                if (
                    !enabled ||
                    !agent.isOnNavMesh ||
                    agent.pathStatus == NavMeshPathStatus.PathInvalid
                )
                {
                    continue;
                }

                while (
                    enabled &&
                    agent.isOnNavMesh &&
                    HasNotReachedDestination()
                )
                {
                    yield return null;
                }
            }
        }

        private void UpdateAnimation()
        {
            float normalizedSpeed = 0f;

            if (
                agent != null &&
                agent.enabled &&
                agent.isOnNavMesh &&
                agent.speed > 0f
            )
            {
                float currentSpeed =
                    agent.velocity.magnitude;

                if (currentSpeed >= minimumMovingSpeed)
                {
                    normalizedSpeed = Mathf.Clamp01(
                        currentSpeed / agent.speed
                    );
                }
            }

            animator.SetFloat(
                speedParameterHash,
                normalizedSpeed,
                animationDampTime,
                Time.deltaTime
            );
        }

        private IEnumerator WaitAtCurrentPosition()
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            float waitDuration = UnityEngine.Random.Range(
                minimumIdleTime,
                maximumIdleTime
            );

            yield return new WaitForSeconds(waitDuration);
        }

        private bool TryChooseDestination(
            out Vector3 destination
        )
        {
            destination = transform.position;

            Bounds bounds = wanderArea.bounds;

            for (
                int attempt = 0;
                attempt < maximumPointAttempts;
                attempt++
            )
            {
                Vector3 candidate = new Vector3(
                    UnityEngine.Random.Range(
                        bounds.min.x,
                        bounds.max.x
                    ),
                    bounds.max.y,
                    UnityEngine.Random.Range(
                        bounds.min.z,
                        bounds.max.z
                    )
                );

                bool foundNavMeshPoint =
                    NavMesh.SamplePosition(
                        candidate,
                        out NavMeshHit hit,
                        navMeshSampleDistance,
                        agent.areaMask
                    );

                if (!foundNavMeshPoint)
                {
                    continue;
                }

                destination = hit.position;
                return true;
            }

            return false;
        }

        private bool EnsureAgentIsOnNavMesh()
        {
            if (agent.isOnNavMesh)
            {
                return true;
            }

            bool foundStartingPoint =
                NavMesh.SamplePosition(
                    transform.position,
                    out NavMeshHit hit,
                    navMeshSampleDistance * 2f,
                    agent.areaMask
                );

            if (!foundStartingPoint)
            {
                return false;
            }

            return agent.Warp(hit.position);
        }

        private bool HasNotReachedDestination()
        {
            if (agent.pathPending)
            {
                return true;
            }

            float requiredDistance =
                agent.stoppingDistance +
                arrivalTolerance;

            return
                agent.remainingDistance > requiredDistance ||
                agent.velocity.sqrMagnitude > 0.01f;
        }

        private void ValidateSettings()
        {
            if (wanderArea == null)
            {
                throw new InvalidOperationException(
                    "RandomCatWander is missing its Wander Area reference."
                );
            }

            if (animator == null)
            {
                throw new InvalidOperationException(
                    "RandomCatWander could not find an Animator."
                );
            }

            if (string.IsNullOrWhiteSpace(speedParameterName))
            {
                throw new InvalidOperationException(
                    "Animator speed parameter name cannot be empty."
                );
            }

            if (navMeshSampleDistance <= 0f)
            {
                throw new InvalidOperationException(
                    "NavMesh sample distance must be greater than zero."
                );
            }

            if (maximumPointAttempts < 1)
            {
                throw new InvalidOperationException(
                    "Maximum point attempts must be at least one."
                );
            }

            if (minimumIdleTime < 0f)
            {
                throw new InvalidOperationException(
                    "Minimum idle time cannot be negative."
                );
            }

            if (maximumIdleTime < minimumIdleTime)
            {
                throw new InvalidOperationException(
                    "Maximum idle time cannot be smaller than minimum idle time."
                );
            }
        }

        private void OnValidate()
        {
            navMeshSampleDistance =
                Mathf.Max(0.01f, navMeshSampleDistance);

            maximumPointAttempts =
                Mathf.Max(1, maximumPointAttempts);

            minimumIdleTime =
                Mathf.Max(0f, minimumIdleTime);

            maximumIdleTime =
                Mathf.Max(
                    minimumIdleTime,
                    maximumIdleTime
                );

            arrivalTolerance =
                Mathf.Max(0f, arrivalTolerance);

            animationDampTime =
                Mathf.Max(0f, animationDampTime);

            minimumMovingSpeed =
                Mathf.Max(0f, minimumMovingSpeed);
        }
    }
}