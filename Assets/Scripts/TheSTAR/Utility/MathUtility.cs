using System;
using System.Linq;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class MathUtility
    {
        public static int Round(float value)
        {
            var result = -1;
            var residue = value % 1;
            result = (int)value;
            if (residue >= 0.5f) result++;

            return result;
        }

        public static int Limit(int value, int min, int max) => (int)Limit((float)value, min, max);
        public static float Limit(float value, float min, float max) => MathF.Max(MathF.Min(value, max), min);

        public static Vector2 Limit(Vector2 value, Vector2 min, Vector2 max) => new Vector2(Limit(value.x, min.x, max.x), Limit(value.y, min.y, max.y));

        public static Vector2 LimitForCircle(Vector2 value, float maxDistance)
        {
            var currentDistance = (float)Math.Sqrt(value.x * value.x + value.y * value.y);
            if (currentDistance > maxDistance) value *= maxDistance/currentDistance;
            return value;
        }
        
        public static bool InBounds(int value, int min, int max) => InBounds((float)value, min, max);
        public static bool InBounds(float value, float min, float max) => (min <= value && value <= max);

        public static bool InBounds(IntVector2 value, IntVector2 min, IntVector2 max) => 
            InBounds(value.X, min.X, max.X) && InBounds(value.Y, min.Y, max.Y);
        
        public static bool InBounds(Vector2 value, Vector2 min, Vector2 max) => 
            InBounds(value.x, min.x, max.x) && InBounds(value.y, min.y, max.y);

        public static bool IsIntValue(string s, out int value)
        {
            s = s.Replace(" ", "");
            value = -1;

            var isMinus = false;
            if (s[0] == '-')
            {
                isMinus = true;
                s = s.Remove(0, 1);
            }

            if (s.Any(symbol => !char.IsNumber(symbol))) return false;

            if (string.IsNullOrEmpty(s))
            {
                value = 0;
                return true;
            }

            value = Convert.ToInt32(s) * ((isMinus ? -1 : 1));

            return true;
        }
    }

    public struct IntVector2
    {
        private int x, y;

        public int X => x;
        public int Y => y;

        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public IntVector2(Vector2 value)
        {
            x = (int)value.x;
            y = (int)value.y;
        }

        public static explicit operator IntVector2(Vector2 value)
        {
            return new IntVector2(value);
        }
        
        public static IntVector2 operator + (IntVector2 a, IntVector2 b)
        {
            return new IntVector2(a.x + b.x, a.y + b.y);
        }
    }
}