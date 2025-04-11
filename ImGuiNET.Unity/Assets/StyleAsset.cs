using UnityEngine;

namespace ImGuiNET.Unity
{
    // TODO: this dll should never end up in unity editor
    // so I should really ditch the scriptable objects stuff and limit the asset bundle to just textures and shaders
    // because having all these params in there only caused me so many errors which are incredibly annoying to debug...
    [CreateAssetMenu(menuName = "Dear ImGui/Style")]
    sealed class StyleAsset : ScriptableObject
    {
        // TODO: double check if we have everything https://github.com/ocornut/imgui/blob/master/imgui.h#L2120
        [Tooltip("Global alpha applies to everything in ImGui.")]
        public float Alpha = 1;

        [Tooltip("Padding within a window.")]
        public Vector2 WindowPadding = new(8,8);

        [Tooltip("Radius of window corners rounding. Set to 0.0f to have rectangular windows.")]
        public float WindowRounding = 7;

        [Tooltip("Thickness of border around windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).")]
        public float WindowBorderSize = 1;

        [Tooltip("Minimum window size. This is a global setting. If you want to constraint individual windows, use SetNextWindowSizeConstraints().")]
        public Vector2 WindowMinSize = new(32,32);

        [Tooltip("Alignment for title bar text. Defaults to (0.0f,0.5f) for left-aligned,vertically centered.")]
        public Vector2 WindowTitleAlign = new(0, 0.5f);

        [Tooltip("Side of the collapsing/docking button in the title bar (left/right). Defaults to ImGuiDir_Left.")]
        public ImGuiDir WindowMenuButtonPosition = ImGuiDir.Left;

        [Tooltip("Radius of child window corners rounding. Set to 0.0f to have rectangular windows.")]
        public float ChildRounding = 0;

        [Tooltip("Thickness of border around child windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).")]
        public float ChildBorderSize = 1;

        [Tooltip("Radius of popup window corners rounding. (Note that tooltip windows use WindowRounding)")]
        public float PopupRounding = 0;

        [Tooltip("Thickness of border around popup/tooltip windows. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).")]
        public float PopupBorderSize = 1;

        [Tooltip("Padding within a framed rectangle (used by most widgets).")]
        public Vector2 FramePadding = new(4,3);

        [Tooltip("Radius of frame corners rounding. Set to 0.0f to have rectangular frame (used by most widgets).")]
        public float FrameRounding = 0;

        [Tooltip("Thickness of border around frames. Generally set to 0.0f or 1.0f. (Other values are not well tested and more CPU/GPU costly).")]
        public float FrameBorderSize = 0;

        [Tooltip("Horizontal and vertical spacing between widgets/lines.")]
        public Vector2 ItemSpacing = new(8,4);

        [Tooltip("Horizontal and vertical spacing between within elements of a composed widget (e.g. a slider and its label).")]
        public Vector2 ItemInnerSpacing = new(4,4);

        [Tooltip("Expand reactive bounding box for touch-based system where touch position is not accurate enough. Unfortunately we don't sort widgets so priority on overlap will always be given to the first widget. So don't grow this too much!")]
        public Vector2 TouchExtraPadding = new(0,0);

        [Tooltip("Horizontal indentation when e.g. entering a tree node. Generally == (FontSize + FramePadding.x*2).")]
        public float IndentSpacing = 21;

        [Tooltip("Minimum horizontal spacing between two columns. Preferably > (FramePadding.x + 1).")]
        public float ColumnsMinSpacing = 6;

        [Tooltip("Width of the vertical scrollbar, Height of the horizontal scrollbar.")]
        public float ScrollbarSize = 14;

        [Tooltip("Radius of grab corners for scrollbar.")]
        public float ScrollbarRounding = 9;

        [Tooltip("Minimum width/height of a grab box for slider/scrollbar.")]
        public float GrabMinSize = 10;

        [Tooltip("Radius of grabs corners rounding. Set to 0.0f to have rectangular slider grabs.")]
        public float GrabRounding = 0;

        [Tooltip("Radius of upper corners of a tab. Set to 0.0f to have rectangular tabs.")]
        public float TabRounding = 4;

        [Tooltip("Thickness of border around tabs.")]
        public float TabBorderSize = 0;

        [Tooltip("Side of the color button in the ColorEdit4 widget (left/right). Defaults to ImGuiDir_Right.")]
        public ImGuiDir ColorButtonPosition = ImGuiDir.Right;

        [Tooltip("Alignment of button text when button is larger than text. Defaults to (0.5f, 0.5f) (centered).")]
        public Vector2 ButtonTextAlign = new(0.5f,0.5f);

        [Tooltip("Alignment of selectable text when selectable is larger than text. Defaults to (0.0f, 0.0f) (top-left aligned).")]
        public Vector2 SelectableTextAlign = new(0,0);

        [Tooltip("Window position are clamped to be visible within the display area by at least this amount. Only applies to regular windows.")]
        public Vector2 DisplayWindowPadding = new(19,19);

        [Tooltip("If you cannot see the edges of your screen (e.g. on a TV) increase the safe area padding. Apply to popups/tooltips as well regular windows. NB: Prefer configuring your TV sets correctly!")]
        public Vector2 DisplaySafeAreaPadding = new(3,3);

        [Tooltip("Scale software rendered mouse cursor (when io.MouseDrawCursor is enabled). May be removed later.")]
        public float MouseCursorScale = 1;

        [Tooltip("Enable anti-aliasing on lines/borders. Disable if you are really tight on CPU/GPU.")]
        public bool AntiAliasedLines = true;

        [Tooltip("Enable anti-aliasing on filled shapes (rounded rectangles, circles, etc.)")]
        public bool AntiAliasedFill = true;

        [Tooltip("Tessellation tolerance when using PathBezierCurveTo() without a specific number of segments. Decrease for highly tessellated curves (higher quality, more polygons), increase to reduce quality.")]
        public float CurveTessellationTol = 1.25f;

        [Tooltip("Maximum error (in pixels) allowed when using AddCircle()/AddCircleFilled() or drawing rounded corner rectangles with no explicit segment count specified. Decrease for higher quality but more geometry.")]
        public float CircleSegmentMaxError = 1.6f; // TODO: rename this param to "CircleTessellationMaxError" (not doing it now because I'd have to rebuild the assetbundle and that sounds like a lot of effort)

        [HideInInspector]
        public Color[] Colors = new Color[(int)ImGuiCol.COUNT];

        public unsafe void ApplyTo(ImGuiStylePtr s)
        {
            s.Alpha                  = Alpha;
            s.WindowPadding          = WindowPadding.ToNumerics();
            s.WindowRounding         = WindowRounding;
            s.WindowBorderSize       = WindowBorderSize;
            s.WindowMinSize          = WindowMinSize.ToNumerics();
            s.WindowTitleAlign       = WindowTitleAlign.ToNumerics();
            s.WindowMenuButtonPosition = WindowMenuButtonPosition;
            s.ChildRounding          = ChildRounding;
            s.ChildBorderSize        = ChildBorderSize;
            s.PopupRounding          = PopupRounding;
            s.PopupBorderSize        = PopupBorderSize;
            s.FramePadding           = FramePadding.ToNumerics();
            s.FrameRounding          = FrameRounding;
            s.FrameBorderSize        = FrameBorderSize;
            s.ItemSpacing            = ItemSpacing.ToNumerics();
            s.ItemInnerSpacing       = ItemInnerSpacing.ToNumerics();
            s.TouchExtraPadding      = TouchExtraPadding.ToNumerics();
            s.IndentSpacing          = IndentSpacing;
            s.ColumnsMinSpacing      = ColumnsMinSpacing;
            s.ScrollbarSize          = ScrollbarSize;
            s.ScrollbarRounding      = ScrollbarRounding;
            s.GrabMinSize            = GrabMinSize;
            s.GrabRounding           = GrabRounding;
            s.TabRounding            = TabRounding;
            s.TabBorderSize          = TabBorderSize;
            s.ColorButtonPosition    = ColorButtonPosition;
            s.ButtonTextAlign        = ButtonTextAlign.ToNumerics();
            s.SelectableTextAlign    = SelectableTextAlign.ToNumerics();
            s.DisplayWindowPadding   = DisplayWindowPadding.ToNumerics();
            s.DisplaySafeAreaPadding = DisplaySafeAreaPadding.ToNumerics();
            s.MouseCursorScale       = MouseCursorScale;
            s.AntiAliasedLines       = AntiAliasedLines;
            s.AntiAliasedFill        = AntiAliasedFill;
            s.CurveTessellationTol   = CurveTessellationTol;
            s.CircleTessellationMaxError  = CircleSegmentMaxError;
            for (var i = 0; i < Colors.Length; ++i)
                s.Colors[i] = Colors[i].ToNumerics();
        }

        public unsafe void SetFrom(ImGuiStylePtr s)
        {
            Alpha                  = s.Alpha;
            WindowPadding          = s.WindowPadding.ToUnity();
            WindowRounding         = s.WindowRounding;
            WindowBorderSize       = s.WindowBorderSize;
            WindowMinSize          = s.WindowMinSize.ToUnity();
            WindowTitleAlign       = s.WindowTitleAlign.ToUnity();
            WindowMenuButtonPosition = s.WindowMenuButtonPosition;
            ChildRounding          = s.ChildRounding;
            ChildBorderSize        = s.ChildBorderSize;
            PopupRounding          = s.PopupRounding;
            PopupBorderSize        = s.PopupBorderSize;
            FramePadding           = s.FramePadding.ToUnity();
            FrameRounding          = s.FrameRounding;
            FrameBorderSize        = s.FrameBorderSize;
            ItemSpacing            = s.ItemSpacing.ToUnity();
            ItemInnerSpacing       = s.ItemInnerSpacing.ToUnity();
            TouchExtraPadding      = s.TouchExtraPadding.ToUnity();
            IndentSpacing          = s.IndentSpacing;
            ColumnsMinSpacing      = s.ColumnsMinSpacing;
            ScrollbarSize          = s.ScrollbarSize;
            ScrollbarRounding      = s.ScrollbarRounding;
            GrabMinSize            = s.GrabMinSize;
            GrabRounding           = s.GrabRounding;
            TabRounding            = s.TabRounding;
            TabBorderSize          = s.TabBorderSize;
            ColorButtonPosition    = s.ColorButtonPosition;
            ButtonTextAlign        = s.ButtonTextAlign.ToUnity();
            SelectableTextAlign    = s.SelectableTextAlign.ToUnity();
            DisplayWindowPadding   = s.DisplayWindowPadding.ToUnity();
            DisplaySafeAreaPadding = s.DisplaySafeAreaPadding.ToUnity();
            MouseCursorScale       = s.MouseCursorScale;
            AntiAliasedLines       = s.AntiAliasedLines;
            AntiAliasedFill        = s.AntiAliasedFill;
            CurveTessellationTol   = s.CurveTessellationTol;
            CircleSegmentMaxError = s.CircleTessellationMaxError;
            for (var i = 0; i < Colors.Length; ++i)
                Colors[i] = s.Colors[i].ToUnityColor();
        }

        void Reset()
        {
            var context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            SetFrom(ImGui.GetStyle());
            ImGui.DestroyContext(context);
        }
    }
}
