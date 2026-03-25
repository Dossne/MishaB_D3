using System;
using UnityEngine;

namespace RainbowTower.GameplayField
{
    [DisallowMultipleComponent]
    public sealed class GameplayPathDefinition : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints = Array.Empty<Transform>();

        public int WaypointCount => waypoints.Length;
        public Transform[] Waypoints => waypoints;

        public Vector3 GetWaypointPosition(int index)
        {
            if (index < 0 || index >= waypoints.Length || waypoints[index] == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return waypoints[index].position;
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                return;
            }

            Gizmos.color = new Color(0.08f, 0.58f, 0.95f, 0.9f);

            for (var index = 0; index < waypoints.Length; index++)
            {
                var waypoint = waypoints[index];
                if (waypoint == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(waypoint.position, 0.08f);

                if (index >= waypoints.Length - 1 || waypoints[index + 1] == null)
                {
                    continue;
                }

                Gizmos.DrawLine(waypoint.position, waypoints[index + 1].position);
            }
        }
    }
}
