using Engine;
using Engine.Gui;


namespace Engine.Toolset {
    public static class Editor {
        static readonly SceneEditor sceneEditor = new();
        public static bool isOpen => Application.scene == sceneEditor;
        public static void open() => Application.scene = sceneEditor;
        public static void close() => Application.scene = Scene.active;


        public static Theme theme; 

        public static readonly Canvas canvas = new(Renderer.windowWidth, Renderer.windowHeight);

        static Editor() {
            Application.window.Resize += onWindowResize;

        }

        static void onWindowResize(OpenTK.Windowing.Common.ResizeEventArgs args) {
            canvas.resize(args.Width, args.Height);
        }

    }

    public class Theme {

        // var c = color.hex(0x004156AF);

        public static readonly Theme darkGreenish = new Theme {
            primaryColor = color.hex(0x84A98CFF),
            backgroundColor = color.hex(0x52796FDF),
            borderColor = color.hex(0x84C98CFF),
            textColor = color.hex(0xCAD2C5FF)
        };


        public color primaryColor { get; init; }
        public color backgroundColor { get; init; }
        public color borderColor { get; init; }
        public color textColor { get; init; }
        
    }
}