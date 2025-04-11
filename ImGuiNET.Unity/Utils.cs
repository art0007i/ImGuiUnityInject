using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ImGuiNET.Unity;

public static class Utils
{
    public static System.Numerics.Vector2 ToNumerics(this UnityEngine.Vector2 v) => new System.Numerics.Vector2(v.x, v.y);
    public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v) => new UnityEngine.Vector2(v.X, v.Y);
    public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);
    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v) => new UnityEngine.Vector3(v.X, v.Y, v.Z);
    public static System.Numerics.Vector4 ToNumerics(this UnityEngine.Vector4 v) => new System.Numerics.Vector4(v.x, v.y, v.z, v.w);
    public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 v) => new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
    public static System.Numerics.Vector4 ToNumerics(this UnityEngine.Color c) => new System.Numerics.Vector4(c.r, c.g, c.b, c.a);
    public static UnityEngine.Color ToUnityColor(this System.Numerics.Vector4 v) => new UnityEngine.Color(v.X, v.Y, v.Z, v.W);
    public static unsafe string StringFromPtr(byte* ptr)
    {
        int i;
        for (i = 0; ptr[i] != 0; i++)
        {
        }

        return Encoding.UTF8.GetString(ptr, i);
    }
}