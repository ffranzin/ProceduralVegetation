using UnityEngine;


namespace Utils.Math
{
    public class MathUtils
    {
        public static float DistanceBetweenRectAndPosition(Rect rect, Vector2 p)
        {
            float dx = Mathf.Max(rect.min.x - p.x, p.x - rect.max.x);
            float dy = Mathf.Max(rect.min.y - p.y, p.y - rect.max.y);

            dx = Mathf.Max(0, dx);
            dy = Mathf.Max(0, dy);

            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}