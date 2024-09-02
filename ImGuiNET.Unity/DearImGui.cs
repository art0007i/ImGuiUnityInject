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
        internal ref CursorShape this[ImGuiMouseCursor cursor]
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
        public bool _doGlobalLayout = true; // do global/default Layout event too

        public Camera _camera = null;
        public RenderImGuiFeature _renderFeature = null;

        public RenderType _rendererType = RenderType.Mesh;
        public PlatformType _platformType = PlatformType.InputManager;

        [Header("Configuration")]
        public IOConfig _initialConfiguration = default;
        [SerializeField] FontAtlasConfigAsset _fontAtlasConfiguration = null;
        [SerializeField] IniSettingsAsset _iniSettings = null;  // null: uses default imgui.ini file

        [Header("Customization")]
        [SerializeField] internal Shader mesh;
        [SerializeField] internal Shader procedural;
        [SerializeField] internal string tex;
        [SerializeField] internal string vertices;
        [SerializeField] internal string baseVertex;

        [SerializeField] StyleAsset _style = null;

        private CursorShape Arrow;
        private CursorShape TextInput;
        private CursorShape ResizeAll;
        private CursorShape ResizeEW;
        private CursorShape ResizeNS;
        private CursorShape ResizeNESW;
        private CursorShape ResizeNWSE;
        private CursorShape Hand;
        private CursorShape NotAllowed;
        
        public Texture2D Arrow_texture;
        public Vector2 Arrow_hotspot;

        public Texture2D TextInput_texture;
        public Vector2 TextInput_hotspot;

        public Texture2D ResizeAll_texture;
        public Vector2 ResizeAll_hotspot;

        public Texture2D ResizeEW_texture;
        public Vector2 ResizeEW_hotspot;

        public Texture2D ResizeNS_texture;
        public Vector2 ResizeNS_hotspot;

        public Texture2D ResizeNESW_texture;
        public Vector2 ResizeNESW_hotspot;

        public Texture2D ResizeNWSE_texture;
        public Vector2 ResizeNWSE_hotspot;

        public Texture2D Hand_texture;
        public Vector2 Hand_hotspot;

        public Texture2D NotAllowed_texture;
        public Vector2 NotAllowed_hotspot;

        const string CommandBufferTag = "DearImGui";
        static readonly ProfilerMarker s_prepareFramePerfMarker = new ProfilerMarker("DearImGui.PrepareFrame");
        static readonly ProfilerMarker s_layoutPerfMarker = new ProfilerMarker("DearImGui.Layout");
        static readonly ProfilerMarker s_drawListPerfMarker = new ProfilerMarker("DearImGui.RenderDrawLists");

        void Awake()
        {
            _context = ImGuiUn.CreateUnityContext();
            Reset();
        }

        void OnDestroy()
        {
            ImGuiUn.DestroyUnityContext(_context);
        }

        void OnEnable()
        {
            Arrow = new CursorShape { texture = Arrow_texture, hotspot = Arrow_hotspot };
            TextInput = new CursorShape { texture = TextInput_texture, hotspot = TextInput_hotspot };
            ResizeAll = new CursorShape { texture = ResizeAll_texture, hotspot = ResizeAll_hotspot };
            ResizeEW = new CursorShape { texture = ResizeEW_texture, hotspot = ResizeEW_hotspot };
            ResizeNS = new CursorShape { texture = ResizeNS_texture, hotspot = ResizeNS_hotspot };
            ResizeNESW = new CursorShape { texture = ResizeNESW_texture, hotspot = ResizeNESW_hotspot };
            ResizeNWSE = new CursorShape { texture = ResizeNWSE_texture, hotspot = ResizeNWSE_hotspot };
            Hand = new CursorShape { texture = Hand_texture, hotspot = Hand_hotspot };
            NotAllowed = new CursorShape { texture = NotAllowed_texture, hotspot = NotAllowed_hotspot };

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
