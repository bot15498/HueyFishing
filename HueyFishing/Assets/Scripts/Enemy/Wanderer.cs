using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Enemy
{
    /// <summary>
    ///     Configures attached transform to periodically translate.
    ///     Displacement, time, and direction are configurable and can be constant or random.
    /// </summary> <remarks>
    ///     All times are in seconds.
    ///     All angles are in degrees.
    ///     2D movements are along the X and Z axes.
    /// </remarks>
    public class Wanderer : MonoBehaviour
    {
        public Preset movementPreset = Preset.CritterWander;

        List<Movement> Queue =>
            movementPreset switch
            {
                Preset.CritterWander => Presets.CritterWander,
                Preset.JitterCircle => Presets.JitterCircle,
                Preset.PingPong => Presets.PingPong,
                Preset.RampingBull => Presets.RampingBull,
                _ => Presets.CritterWander
            };


        // Movement Bounds: For Teleporting Movements
        public float minX = -11f;
        public float maxX = 11f;
        public float minZ = -8f;
        public float maxZ = 8f;

        /// <summary> If bounds are exceeded, move back from bounds by this amount, to avoid getting stuck in walls. </summary>
        public float margin = 1f;

        /// <summary> Rotate angle 180 degrees on Rigidbody collision. </summary>
        public bool bounce = true;


        // Internal Movement Tracker
        int QueuePosition { get; set; }
        Movement Current => Queue[QueuePosition];

        bool _hasMovement;
        float _phaseStopwatch;
        Phase _phase = Phase.PreWait;


        // Cached Random Values Of Current Movement
        float? _angle;
        float? _magnitude;
        float? _preWait;
        float? _duration;
        float? _postWait;


        // Unity Components
        Rigidbody _rb;


        ////////////////////////////////////////
        // Init References
        ////////////////////////////////////////
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }


        ////////////////////////////////////////
        // Main Movement Loop
        ////////////////////////////////////////
        void FixedUpdate()
        {
            if (!InitMovement()) return;
            Debug.Assert(_angle != null, nameof(_angle) + " != null");
            Debug.Assert(_magnitude != null, nameof(_magnitude) + " != null");
            Debug.Assert(_preWait != null, nameof(_preWait) + " != null");
            Debug.Assert(_duration != null, nameof(_duration) + " != null");
            Debug.Assert(_postWait != null, nameof(_postWait) + " != null");
            _phaseStopwatch += Time.deltaTime;

            switch (_phase)
            {
                default:
                case Phase.PreWait:
                    if (_phaseStopwatch < _preWait) break;
                    _phaseStopwatch = 0;
                    _phase = Phase.Move;
                    break;

                case Phase.Move:
                    var directionX = Mathf.Sin((float)_angle * Mathf.Deg2Rad);
                    var directionZ = Mathf.Cos((float)_angle * Mathf.Deg2Rad);
                    var direction = new Vector3(directionX, 0, directionZ);
                    var deltaMagnitude = Mathf.Lerp(0f, (float)_magnitude, Time.deltaTime / (float)_duration);
                    var deltaMove = direction * deltaMagnitude;
                    var position = transform.position + deltaMove;

                    if (position.x < minX)
                        position.x = minX + margin;
                    else if (position.x > maxX)
                        position.x = maxX - margin;

                    if (position.z < minZ)
                        position.z = minZ + margin;
                    else if (position.z > maxZ)
                        position.z = maxZ - margin;

                    _rb.MovePosition(position);
                    if (_phaseStopwatch < _duration) break;
                    _phaseStopwatch = 0;
                    _phase = Phase.PostWait;
                    break;

                case Phase.PostWait:
                    if (_phaseStopwatch < _postWait) break;
                    QueuePosition = (QueuePosition + 1) % Queue.Count;
                    CacheMovement();
                    break;
            }
        }


        ////////////////////////////////////////
        // Bounce on Rigidbody Collision
        ////////////////////////////////////////
        void OnCollisionEnter(Collision other)
        {
            if (bounce)
                _angle = (_angle + 180f) % 360f;
        }


        ////////////////////////////////////////
        // Helper Functions
        ////////////////////////////////////////
        bool InitMovement()
        {
            if (Queue.Count == 0)
            {
                _hasMovement = false;
                return false;
            }

            if (_hasMovement)
            {
                QueuePosition = Math.Min(QueuePosition, Queue.Count - 1);
                return true;
            }

            CacheMovement();
            return true;
        }


        void CacheMovement()
        {
            QueuePosition = Math.Min(QueuePosition, Queue.Count - 1);
            _hasMovement = true;
            _phaseStopwatch = 0;
            _phase = Phase.PreWait;

            if (Current is { AngleMin: not null, AngleMax: not null })
                _angle = Random.Range((float)Current.AngleMin, (float)Current.AngleMax);
            else if (_angle is null)
                throw new ArgumentNullException($"Missing previous Movement to inherit angle from.");

            if (Current is { MagnitudeMin: not null, MagnitudeMax: not null })
                _magnitude = Random.Range((float)Current.MagnitudeMin, (float)Current.MagnitudeMax);
            else if (_magnitude is null)
                throw new ArgumentNullException($"Missing previous Movement to inherit magnitude from.");

            if (Current is { DurationMin: not null, DurationMax: not null })
                _duration = Random.Range((float)Current.DurationMin, (float)Current.DurationMax);
            else if (_duration is null)
                throw new ArgumentNullException($"Missing previous Movement to inherit duration from.");

            if (Current is { PreWaitMin: not null, PreWaitMax: not null })
                _preWait = Random.Range((float)Current.PreWaitMin, (float)Current.PreWaitMax);
            else if (_preWait is null)
                throw new ArgumentNullException($"Missing previous Movement to inherit preWait from.");

            if (Current is { PostWaitMin: not null, PostWaitMax: not null })
                _postWait = Random.Range((float)Current.PostWaitMin, (float)Current.PostWaitMax);
            else if (_postWait is null)
                throw new ArgumentNullException($"Missing previous Movement to inherit postWait from.");
        }
    }


    ////////////////////////////////////////
    // Data Structures
    ////////////////////////////////////////
    /// <remarks> If a parameter is null, it keeps the parameter from the previous Movement in the queue. </remarks>
    public struct Movement
    {
        public Movement(float? angleMin = 0f, float? angleMax = 360f,
            float? magnitudeMin = 0f, float? magnitudeMax = 1f,
            float? preWaitMin = 0f, float? preWaitMax = 0f,
            float? durationMin = 1f, float? durationMax = 1f,
            float? postWaitMin = 0f, float? postWaitMax = 0f)
        {
            AngleMin = angleMin;
            AngleMax = angleMax;
            MagnitudeMin = magnitudeMin;
            MagnitudeMax = magnitudeMax;
            PreWaitMin = preWaitMin;
            PreWaitMax = preWaitMax;
            DurationMin = durationMin;
            DurationMax = durationMax;
            PostWaitMin = postWaitMin;
            PostWaitMax = postWaitMax;
        }

        public readonly float? AngleMin;
        public readonly float? AngleMax;
        public readonly float? MagnitudeMin;
        public readonly float? MagnitudeMax;
        public readonly float? PreWaitMin;
        public readonly float? PreWaitMax;
        public readonly float? DurationMin;
        public readonly float? DurationMax;
        public readonly float? PostWaitMin;
        public readonly float? PostWaitMax;
    }


    ////////////////////////////////////////
    // Internal Movement States
    ////////////////////////////////////////
    internal enum Phase
    {
        PreWait,
        Move,
        PostWait
    }


    ////////////////////////////////////////
    // Default Data Sets
    ////////////////////////////////////////
    public enum Preset
    {
        CritterWander,
        JitterCircle,
        PingPong,
        RampingBull
    }


    public static class Presets
    {
        public static readonly List<Movement> CritterWander = new()
        {
            new Movement(
                angleMin: 0f,
                angleMax: 360f,
                magnitudeMin: 1f,
                magnitudeMax: 1f,
                preWaitMin: 0f,
                preWaitMax: 0f,
                durationMin: 0.5f,
                durationMax: 0.5f,
                postWaitMin: 0.5f,
                postWaitMax: 0.5f
            )
        };


        public static readonly List<Movement> JitterCircle = new()
        {
            new Movement(angleMin: 0f, angleMax: 20f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 20f, angleMax: 40f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 40f, angleMax: 60f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 60f, angleMax: 80f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 80f, angleMax: 100f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 100f, angleMax: 120f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 120f, angleMax: 140f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 140f, angleMax: 160f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 160f, angleMax: 180f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 180f, angleMax: 200f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 200f, angleMax: 220f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 220f, angleMax: 240f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 240f, angleMax: 260f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 260f, angleMax: 280f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 280f, angleMax: 300f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 300f, angleMax: 320f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 320f, angleMax: 340f, durationMin: 0.25f, durationMax: 0.25f),
            new Movement(angleMin: 340f, angleMax: 360f, durationMin: 0.25f, durationMax: 0.25f)
        };


        public static readonly List<Movement> PingPong = new()
        {
            new Movement(angleMin: 0f, angleMax: 90f,
                magnitudeMin: 3f, magnitudeMax: 3f,
                durationMin: 0.5f, durationMax: 0.5f),
            new Movement(angleMin: 180f, angleMax: 270f,
                magnitudeMin: 3f, magnitudeMax: 3f,
                durationMin: 0.5f, durationMax: 0.5f),
            new Movement(angleMin: 90f, angleMax: 180f,
                magnitudeMin: 3f, magnitudeMax: 3f,
                durationMin: 0.5f, durationMax: 0.5f),
            new Movement(angleMin: 270f, angleMax: 360f,
                magnitudeMin: 3f, magnitudeMax: 3f,
                durationMin: 0.5f, durationMax: 0.5f)
        };


        public static readonly List<Movement> RampingBull = new()
        {
            new Movement(magnitudeMin: 0.1f, magnitudeMax: 0.1f, durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.12f, magnitudeMax: 0.12f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.14f, magnitudeMax: 0.14f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.16f, magnitudeMax: 0.16f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.18f, magnitudeMax: 0.18f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.2f, magnitudeMax: 0.2f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.24f, magnitudeMax: 0.24f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.28f, magnitudeMax: 0.28f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.32f, magnitudeMax: 0.32f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.36f, magnitudeMax: 0.36f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.40f, magnitudeMax: 0.40f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.50f, magnitudeMax: 0.50f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.60f, magnitudeMax: 0.60f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.70f, magnitudeMax: 0.70f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.80f, magnitudeMax: 0.80f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.70f, magnitudeMax: 0.70f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.60f, magnitudeMax: 0.60f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.50f, magnitudeMax: 0.50f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.40f, magnitudeMax: 0.40f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.30f, magnitudeMax: 0.30f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.20f, magnitudeMax: 0.20f,
                durationMin: 0.1f, durationMax: 0.1f),
            new Movement(angleMin: null, angleMax: null,
                magnitudeMin: 0.10f, magnitudeMax: 0.10f,
                durationMin: 0.1f, durationMax: 0.1f)
        };
    }
}
