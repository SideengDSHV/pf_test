using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Cameras
{
    /// <summary>
    /// Object for camera to limit distance from
    /// </summary>
    public interface ICameraTarget
    {
        public static List<ICameraTarget> targets { get; } = new();

        Vector3 position { get; }
    }
}