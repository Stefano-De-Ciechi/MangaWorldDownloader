using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace mangaWorld.ui;

public static class Program
{
    private static IWindow? _window;
    private static GL? _gl;
    private static ImGuiController? _controller;

    private static Renderer? _appRenderer;
    
    static void Main(string[] args)
    {
        // create the main rendering window
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(1280, 720);
        options.Title = "MangaWorld Scraper / Downloader";
        
        _window = Window.Create(options);

        // append rendering window Event Handlers
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClose;

        _window.Run();
    }

    private static void OnLoad()
    {
        // get OpenGL context and window inputs
        _gl = _window.CreateOpenGL();
        var inputContext = _window?.CreateInput();

        // initialize ImGuiController
        _controller = new ImGuiController(_gl, _window, inputContext);
        
        // initialize my custom render logic
        _appRenderer = new Renderer();
    }

    // executed every frame
    private static void OnRender(double deltaTime)
    {
        // tell ImGui that a new frame is being rendered
        _controller?.Update((float)deltaTime);

        // draw my custom render logic here
        _appRenderer?.Render();

        // clear screen and render ImGui
        _gl?.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        _gl?.Clear((uint) ClearBufferMask.ColorBufferBit);

        // draw ImGui frame to the screen
        _controller?.Render();
    }

    // resource cleanup
    private static void OnClose()
    {
        _controller?.Dispose();
        _gl?.Dispose();
    }
}