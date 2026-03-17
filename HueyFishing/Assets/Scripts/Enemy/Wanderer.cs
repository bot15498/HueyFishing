using System;
using System.Collections.Generic;
using UnityEngine;
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

        int QueuePosition { get; set; }
        Movement Current => Queue[QueuePosition];

        bool _hasMovement;
        float _phaseStopwatch;
        Phase _phase = Phase.PreWait;

        float _angle;
        float _magnitude;
        float _preWait;
        float _duration;
        float _postWait;

        ////////////////////////////////////////
        // Main Movement Loop
        ////////////////////////////////////////
        void FixedUpdate()
        {
            if (!InitMovement()) return;
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
                    var directionX = Mathf.Sin(_angle * Mathf.Deg2Rad);
                    var directionZ = Mathf.Cos(_angle * Mathf.Deg2Rad);
                    var direction = new Vector3(directionX, 0, directionZ);
                    var deltaMagnitude = Mathf.Lerp(0f, _magnitude, Time.deltaTime / _duration);
                    var deltaMove = direction * deltaMagnitude;
                    transform.position += deltaMove;

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

            _angle = Random.Range(Current.AngleMin, Current.AngleMax);
            _magnitude = Random.Range(Current.MagnitudeMin, Current.MagnitudeMax);
            _duration = Random.Range(Current.DurationMin, Current.DurationMax);
            _preWait = Random.Range(Current.PreWaitMin, Current.PreWaitMax);
            _postWait = Random.Range(Current.PostWaitMin, Current.PostWaitMax);
        }
    }


    ////////////////////////////////////////
    // Data Structures
    ////////////////////////////////////////
    public struct Movement
    {
        public Movement(float angleMin = 0f, float angleMax = 360f,
            float magnitudeMin = 0f, float magnitudeMax = 1f,
            float preWaitMin = 0f, float preWaitMax = 0f,
            float durationMin = 1f, float durationMax = 1f,
            float postWaitMin = 0f, float postWaitMax = 0f)
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

        public readonly float AngleMin;
        public readonly float AngleMax;
        public readonly float MagnitudeMin;
        public readonly float MagnitudeMax;
        public readonly float PreWaitMin;
        public readonly float PreWaitMax;
        public readonly float DurationMin;
        public readonly float DurationMax;
        public readonly float PostWaitMin;
        public readonly float PostWaitMax;
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
