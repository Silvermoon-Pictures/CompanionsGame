using UnityEngine;

namespace Silvermoon.Utils
{
    public static class Vector3Extensions
    {
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }
    }

}
