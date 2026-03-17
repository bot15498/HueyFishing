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
                _ => Presets.CritterWander
            };


        // Movement Bounds: For Teleporting Movements
        public float minX = -11f;
        public float maxX = 11f;
        public float minZ = -8f;
        public float maxZ = 8f;


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
                    var finalX = Mathf.Clamp(transform.position.x + deltaMove.x, minX, maxX);
                    var finalZ = Mathf.Clamp(transform.position.z + deltaMove.z, minZ, maxZ);
                    var finalPosition = new Vector3(finalX, transform.position.y, finalZ);
                    _rb.MovePosition(finalPosition);

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
            float? durationMin = 0.25f, float? durationMax = 0.25f,
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
        CritterWander
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
    }
}
