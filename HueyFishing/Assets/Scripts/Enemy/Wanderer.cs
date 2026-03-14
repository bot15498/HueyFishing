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
        public List<Movement> Queue = new() { Movements.CritterWander };
        int QueuePosition { get; set; }
        Movement Current => Queue[QueuePosition];

        bool _hasMovement;
        float _phaseStopwatch;
        Phase _phase = Phase.PreWait;
        Quadrant _quadrant = Quadrant.TopRight;

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
                    var direction = new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0, Mathf.Cos(_angle * Mathf.Deg2Rad));
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
                    if (Current.Direction == Direction.Quarters)
                        _quadrant = (Quadrant) ((int) _quadrant % 4 + 1);
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

            _angle = Current.Direction switch
            {
                Direction.Random => Random.Range(0f, 360f),
                Direction.Quarters => _quadrant switch
                {
                    Quadrant.TopRight => Random.Range(0f, 90f),
                    Quadrant.TopLeft => Random.Range(90f, 180f),
                    Quadrant.BottomLeft => Random.Range(180f, 270f),
                    Quadrant.BottomRight => Random.Range(270f, 360f),
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };
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
        public Movement(Direction direction, float magnitudeMin, float magnitudeMax, float durationMin,
            float durationMax, float preWaitMin, float preWaitMax, float postWaitMin, float postWaitMax)
        {
            Direction = direction;
            MagnitudeMin = magnitudeMin;
            MagnitudeMax = magnitudeMax;
            PreWaitMin = preWaitMin;
            PreWaitMax = preWaitMax;
            DurationMin = durationMin;
            DurationMax = durationMax;
            PostWaitMin = postWaitMin;
            PostWaitMax = postWaitMax;
        }

        public readonly Direction Direction;
        public readonly float MagnitudeMin;
        public readonly float MagnitudeMax;
        public readonly float PreWaitMin;
        public readonly float PreWaitMax;
        public readonly float DurationMin;
        public readonly float DurationMax;
        public readonly float PostWaitMin;
        public readonly float PostWaitMax;
    }


    public enum Direction
    {
        /// <summary> Completely random </summary>
        Random,

        /// <summary> Alternate between four quadrants in order. Within quadrant, pick a random direction </summary>
        Quarters
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


    internal enum Quadrant
    {
        TopRight = 1,
        TopLeft = 2,
        BottomLeft = 3,
        BottomRight = 4
    }


    ////////////////////////////////////////
    // Default Data Sets
    ////////////////////////////////////////
    public static class Movements
    {
        public static readonly Movement CritterWander = new(
            direction: Direction.Random,
            magnitudeMin: 1f,
            magnitudeMax: 1f,
            preWaitMin: 0.5f,
            preWaitMax: 0.5f,
            durationMin: 0.5f,
            durationMax: 0.5f,
            postWaitMin: 0.5f,
            postWaitMax: 0.5f
        );
    }
}
