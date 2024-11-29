using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ImGuiNET.Unity;

public unsafe static class ImGuiIOPtrExtensions
{
    // keep track of data allocated by the managed side
    static readonly HashSet<IntPtr> s_managedAllocations = new HashSet<IntPtr>(IntPtrEqualityComparer.Instance);

    public static void SetBackendRendererName(this ImGuiIOPtr ptr, string name)
    {
        var NativePtr = ptr.NativePtr;
        if (NativePtr->BackendRendererName != (byte*)0)
        {
            if (s_managedAllocations.Contains((IntPtr)NativePtr->BackendRendererName))
                Marshal.FreeHGlobal((IntPtr)NativePtr->BackendRendererName);
            NativePtr->BackendRendererName = (byte*)0;
        }
        if (name != null)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* nativeName = (byte*)(void*)Marshal.AllocHGlobal(byteCount+1);
            int offset = 0;
            fixed (char* chars = name)
            {
                offset = Encoding.UTF8.GetBytes(chars, name.Length, nativeName, byteCount);
            }

            nativeName[offset] = 0;
            NativePtr->BackendRendererName = nativeName;
            s_managedAllocations.Add((IntPtr)nativeName);
        }
    }

    public static void SetBackendPlatformName(this ImGuiIOPtr ptr, string name)
    {
        var NativePtr = ptr.NativePtr;
        if (NativePtr->BackendPlatformName != (byte*)0)
        {
            if (s_managedAllocations.Contains((IntPtr)NativePtr->BackendPlatformName))
                Marshal.FreeHGlobal((IntPtr)NativePtr->BackendPlatformName);
            NativePtr->BackendPlatformName = (byte*)0;
        }
        if (name != null)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* nativeName = (byte*)(void*)Marshal.AllocHGlobal(byteCount + 1);
            int offset = 0;
            fixed (char* chars = name)
            {
                offset = Encoding.UTF8.GetBytes(chars, name.Length, nativeName, byteCount);
            }

            nativeName[offset] = 0;
            NativePtr->BackendPlatformName = nativeName;
            s_managedAllocations.Add((IntPtr)nativeName);
        }
    }

    public static void SetIniFilename(this ImGuiIOPtr ptr, string name)
    {
        var NativePtr = ptr.NativePtr;
        if (NativePtr->IniFilename != (byte*)0)
        {
            if (s_managedAllocations.Contains((IntPtr)NativePtr->IniFilename))
                Marshal.FreeHGlobal((IntPtr)NativePtr->IniFilename);
            NativePtr->IniFilename = (byte*)0;
        }
        if (name != null)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* nativeName = (byte*)(void*)Marshal.AllocHGlobal(byteCount + 1);
            int offset = 0;
            fixed (char* chars = name)
            {
                offset = Encoding.UTF8.GetBytes(chars, name.Length, nativeName, byteCount);
            }

            nativeName[offset] = 0;
            NativePtr->IniFilename = nativeName;
            s_managedAllocations.Add((IntPtr)nativeName);
        }
    }

    public static void SetBackendPlatformUserData<T>(this ImGuiIOPtr ptr, T? data)
    where T : unmanaged
    {
        var NativePtr = ptr.NativePtr;
        if (NativePtr->BackendPlatformUserData != (void*)0)
        {
            if (s_managedAllocations.Contains((IntPtr)NativePtr->BackendPlatformUserData))
                Marshal.FreeHGlobal((IntPtr)NativePtr->BackendPlatformUserData);
            NativePtr->BackendPlatformUserData = (void*)0;
        }
        if (data != null)
        {
            IntPtr dataPtr = Marshal.AllocHGlobal(sizeof(T));
            Marshal.StructureToPtr(data, dataPtr, false);
            NativePtr->BackendPlatformUserData = (void*)dataPtr;
            s_managedAllocations.Add(dataPtr);
        }
    }
}

// Avoid boxing IntPtr in collections.
// IntPtr does not implement IEquatable<IntPtr> yet in 2019.3 Mono bleeding edge.
// https://github.com/Unity-Technologies/mono/blob/unity-2019.3-mbe/mcs/class/corlib/System/IntPtr.cs

public class IntPtrEqualityComparer : IEqualityComparer<IntPtr>
{
    public static IntPtrEqualityComparer Instance { get; } = new IntPtrEqualityComparer();

    IntPtrEqualityComparer() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(IntPtr p1, IntPtr p2) => p1 == p2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(IntPtr ptr) => ptr.GetHashCode();
}