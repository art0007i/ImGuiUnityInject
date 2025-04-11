using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using ImGuiNET.Unity;

namespace ImGuiNET
{
    // ImGui extra functionality related with Images
    public static partial class ImGuiUn
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Image(Texture tex)
        {
            ImGui.Image((IntPtr)GetTextureId(tex), new System.Numerics.Vector2(tex.width, tex.height));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Image(Texture tex, Vector2 size)
        {
            ImGui.Image((IntPtr)GetTextureId(tex), size.ToNumerics());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Image(Sprite sprite)
        {
            SpriteInfo info = GetSpriteInfo(sprite);
            ImGui.Image((IntPtr)GetTextureId(info.texture), info.size.ToNumerics(), info.uv0.ToNumerics(), info.uv1.ToNumerics());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Image(Sprite sprite, Vector2 size)
        {
            SpriteInfo info = GetSpriteInfo(sprite);
            ImGui.Image((IntPtr)GetTextureId(info.texture), size.ToNumerics(), info.uv0.ToNumerics(), info.uv1.ToNumerics());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImageButton(string str_id, Texture tex)
        {
            ImGui.ImageButton(str_id, (IntPtr)GetTextureId(tex), new System.Numerics.Vector2(tex.width, tex.height));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImageButton(string str_id, Texture tex, Vector2 size)
        {
            ImGui.ImageButton(str_id, (IntPtr)GetTextureId(tex), size.ToNumerics());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImageButton(string str_id, Sprite sprite)
        {
            SpriteInfo info = GetSpriteInfo(sprite);
            ImGui.ImageButton(str_id, (IntPtr)GetTextureId(info.texture), info.size.ToNumerics(), info.uv0.ToNumerics(), info.uv1.ToNumerics());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ImageButton(string str_id, Sprite sprite, Vector2 size)
        {
            SpriteInfo info = GetSpriteInfo(sprite);
            ImGui.ImageButton(str_id, (IntPtr)GetTextureId(info.texture), size.ToNumerics(), info.uv0.ToNumerics(), info.uv1.ToNumerics());
        }
    }
}
