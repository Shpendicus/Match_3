﻿using System.Numerics;

namespace Match_3;

public struct Int2
{
    public int X { get; set; }
    public int Y { get; set; }

    public Int2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Int2(int v)
    {
        X = v;
        Y = v;
    }

   // public static bool operator implicit Vector2(Int2 intVec) => return
    public static implicit operator Vector2(Int2 intPos) => new(intPos.X, intPos.Y);
    
    public static implicit operator Int2(Vector2 intPos) => new((int)intPos.X, (int)intPos.Y);

    public static bool operator ==(Int2 left, Int2 right)
    {
        return left.X == right.X && left.Y == right.Y;
    }

    public static bool operator !=(Int2 left, Int2 right)
    {
        return left.X != right.X || left.Y != right.Y;
    }

    public static Int2 operator +(Int2 left, Int2 right)
    {
        return new Int2(left.X + right.X, left.Y + right.Y);
    }

    public static Int2 operator -(Int2 left, Int2 right)
    {
        return new Int2(left.X - right.X, left.Y - right.Y);
    }

    public static Int2 operator *(Int2 left, int right)
    {
        return new Int2(left.X * right, left.Y * right);
    }

    /// <summary>
    /// Lets see if this conversion gets the right value
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static Int2 operator *(Int2 left, float right)
    {
        return new Int2(Convert.ToInt32(left.X * right), Convert.ToInt32(left.Y * right));
    }

    public static Int2 operator /(Int2 left, int right)
    {
        return new Int2(left.X / right, left.Y / right);
    }

    public static Int2 operator %(Int2 left, int right)
    {
        return new Int2(left.X % right, left.Y % right);
    }

    public static Int2 operator +(Int2 left, int right)
    {
        return new Int2(left.X + right, left.Y + right);
    }

    public static Int2 operator -(Int2 left, int right)
    {
        return new Int2(left.X - right, left.Y - right);
    }

    public static Int2 operator *(Int2 left, Int2 right)
    {
        return new Int2(left.X * right.X, left.Y * right.Y);
    }

    public static Int2 operator /(Int2 left, Int2 right)
    {
        return new Int2(left.X / right.X, left.Y / right.Y);
    }

    public static Int2 operator -(Int2 value)
    {
        return new Int2(-value.X, -value.Y);
    }

    public static Int2 One => new(1);
    public static Int2 Zero => new(0);
    public static Int2 UnitX => new(1, 0);
    public static Int2 UnitY => new(0, 1);
    public static Int2 MinValue => new(int.MinValue);
    public static Int2 MaxValue => new(int.MaxValue);

    public static Int2 Abs(Int2 value)
    {
        return new Int2(Math.Abs(value.X), Math.Abs(value.Y));
    }

    public static Int2 Min(Int2 value1, Int2 value2)
    {
        return new Int2(Math.Min(value1.X, value2.X), Math.Min(value1.Y, value2.Y));
    }

    public static Int2 Max(Int2 value1, Int2 value2)
    {
        return new Int2(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y));
    }

    public static Vector2 operator *(Vector2 left, Int2 right)
    {
        return new Vector2(left.X * right.X, left.Y * right.Y);
    }

    private static Dictionary<Int2, string> _stringCache = new();
    public override string ToString()
    {
        if (!_stringCache.TryGetValue(this, out var result))
        {
            result = $"<{X}, {Y}>";
            _stringCache.Add(this, result);
        }

        return result;
    }

    public bool Equals(Int2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Int2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}