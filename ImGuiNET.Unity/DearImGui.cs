using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;
using System;

namespace ImGuiNET.Unity
{

    [Serializable]
    public struct CursorShape
    {
        public Texture2D texture;
        public Vector2 hotspot;
    }

    // This component is responsible for setting up ImGui for use in Unity.
    // It holds the necessary context and sets it up before any operation is done to ImGui.
    // (e.g. set the context, texture and font managers before calling Layout)

    /// <summary>
    /// Dear ImGui integration into Unity
    /// </summary>
    public class DearImGui : MonoBehaviour
    {
        public ref CursorShape this[ImGuiMouseCursor cursor]
        {
            get
            {
                switch (cursor)
                {
                    case ImGuiMouseCursor.Arrow: return ref Arrow;
                    case ImGuiMouseCursor.TextInput: return ref TextInput;
                    case ImGuiMouseCursor.ResizeAll: return ref ResizeAll;
                    case ImGuiMouseCursor.ResizeEW: return ref ResizeEW;
                    case ImGuiMouseCursor.ResizeNS: return ref ResizeNS;
                    case ImGuiMouseCursor.ResizeNESW: return ref ResizeNESW;
                    case ImGuiMouseCursor.ResizeNWSE: return ref ResizeNWSE;
                    case ImGuiMouseCursor.Hand: return ref Hand;
                    case ImGuiMouseCursor.NotAllowed: return ref NotAllowed;
                    default: return ref Arrow;
                }
            }
        }

        ImGuiUnityContext _context;
        IImGuiRenderer _renderer;
        IImGuiPlatform _platform;
        CommandBuffer _cmd;
        bool _usingURP;

        public event System.Action Layout;  // Layout event for *this* ImGui instance
        [SerializeField] bool _doGlobalLayout = true; // do global/default Layout event too

        [SerializeField] Camera _camera = null;
        [SerializeField] RenderImGuiFeature _renderFeature = null;

        [SerializeField] RenderUtils.RenderType _rendererType = RenderUtils.RenderType.Mesh;
        [SerializeField] Platform.Type _platformType = Platform.Type.InputManager;

        [Header("Configuration")]
        [SerializeField] IOConfig _initialConfiguration = default;
        [SerializeField] FontAtlasConfigAsset _fontAtlasConfiguration = null;
        [SerializeField] IniSettingsAsset _iniSettings = null;  // null: uses default imgui.ini file

        [Header("Customization")]
        public Shader mesh;
        public Shader procedural;
        public string tex;
        public string vertices;
        public string baseVertex;

        [SerializeField] StyleAsset _style = null; [Tooltip("Default.")]

        public CursorShape Arrow;
        [Tooltip("When hovering over InputText, etc.")]
        public CursorShape TextInput;
        [Tooltip("(Unused by ImGui functions)")]
        public CursorShape ResizeAll;
        [Tooltip("When hovering over an horizontal border")]
        public CursorShape ResizeEW;
        [Tooltip("When hovering over a vertical border or column")]
        public CursorShape ResizeNS;
        [Tooltip("When hovering over the bottom-left corner of a window")]
        public CursorShape ResizeNESW;
        [Tooltip("When hovering over the bottom-right corner of a window")]
        public CursorShape ResizeNWSE;
        [Tooltip("(Unused by ImGui functions. Use for e.g. hyperlinks)")]
        public CursorShape Hand;
        [Tooltip("When hovering something with disabled interaction. Usually a crossed circle.")]
        public CursorShape NotAllowed;

        const string CommandBufferTag = "DearImGui";
        static readonly ProfilerMarker s_prepareFramePerfMarker = new ProfilerMarker("DearImGui.PrepareFrame");
        static readonly ProfilerMarker s_layoutPerfMarker = new ProfilerMarker("DearImGui.Layout");
        static readonly ProfilerMarker s_drawListPerfMarker = new ProfilerMarker("DearImGui.RenderDrawLists");

        void Awake()
        {
            _context = ImGuiUn.CreateUnityContext();

            // Arti addon
            _camera = Camera.main;
        }

        void OnDestroy()
        {
            ImGuiUn.DestroyUnityContext(_context);
        }

        void OnEnable()
        {
            _usingURP = RenderUtils.IsUsingURP();
            if (_camera == null) Fail(nameof(_camera));
            if (_renderFeature == null && _usingURP) Fail(nameof(_renderFeature));

            _cmd = RenderUtils.GetCommandBuffer(CommandBufferTag);
            if (_usingURP)
                _renderFeature.commandBuffer = _cmd;
            else
                _camera.AddCommandBuffer(CameraEvent.AfterEverything, _cmd);

            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            _initialConfiguration.ApplyTo(io);
            _style?.ApplyTo(ImGui.GetStyle());

            _context.textures.BuildFontAtlas(io, _fontAtlasConfiguration);
            _context.textures.Initialize(io);

            SetPlatform(Platform.Create(_platformType, this, _iniSettings), io);
            SetRenderer(RenderUtils.Create(_rendererType, this, _context.textures), io);
            if (_platform == null) Fail(nameof(_platform));
            if (_renderer == null) Fail(nameof(_renderer));

            void Fail(string reason)
            {
                OnDisable();
                enabled = false;
                throw new System.Exception($"Failed to start: {reason}");
            }
        }

        void OnDisable()
        {
            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            SetRenderer(null, io);
            SetPlatform(null, io);

            ImGuiUn.SetUnityContext(null);

            _context.textures.Shutdown();
            _context.textures.DestroyFontAtlas(io);

            if (_usingURP)
            {
                if (_renderFeature != null)
                    _renderFeature.commandBuffer = null;
            }
            else
            {
                if (_camera != null)
                    _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _cmd);
            }

            if (_cmd != null)
                RenderUtils.ReleaseCommandBuffer(_cmd);
            _cmd = null;
        }

        void Reset()
        {
            _camera = Camera.main;
            _initialConfiguration.SetDefaults();
        }

        public void Reload()
        {
            OnDisable();
            OnEnable();
        }

        void Update()
        {
            ImGuiUn.SetUnityContext(_context);
            ImGuiIOPtr io = ImGui.GetIO();

            s_prepareFramePerfMarker.Begin(this);
            _context.textures.PrepareFrame(io);
            _platform.PrepareFrame(io, _camera.pixelRect);
            ImGui.NewFrame();
            s_prepareFramePerfMarker.End();

            s_layoutPerfMarker.Begin(this);
            try
            {
                if (_doGlobalLayout)
                    ImGuiUn.DoLayout();   // ImGuiUn.Layout: global handlers
                Layout?.Invoke();     // this.Layout: handlers specific to this instance
            }
            finally
            {
                ImGui.Render();
                s_layoutPerfMarker.End();
            }

            s_drawListPerfMarker.Begin(this);
            _cmd.Clear();
            _renderer.RenderDrawLists(_cmd, ImGui.GetDrawData());
            s_drawListPerfMarker.End();
        }

        void SetRenderer(IImGuiRenderer renderer, ImGuiIOPtr io)
        {
            _renderer?.Shutdown(io);
            _renderer = renderer;
            _renderer?.Initialize(io);
        }

        void SetPlatform(IImGuiPlatform platform, ImGuiIOPtr io)
        {
            _platform?.Shutdown(io);
            _platform = platform;
            _platform?.Initialize(io);
        }
    }
}
