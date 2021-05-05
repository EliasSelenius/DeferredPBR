using System.IO;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Xml;
using System;
using Engine.Toolset;

namespace Engine {
    public static class Application {
        public static GameWindow window { get; private set; }
        public static SceneBase scene;


        public static float deltaTime;
        public static double time;

        public static void run(Action userload) {

            window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            window.Size = (1600, 900);
            window.VSync = VSyncMode.Off;


            //window.Load += Assets.load;
            window.Load += load;
            window.Load += Renderer.load;
            window.Load += userload;
            window.UpdateFrame += update;
            window.RenderFrame += Renderer.drawframe;
            window.Resize += Renderer.windowResize;
            window.Run();
        }

        

        static void load() {
            Assets.load(new EmbeddedResourceProvider(typeof(Application).Assembly));
        }


        static void update(FrameEventArgs e) {
            time += e.Time;
            deltaTime = (float)e.Time;

            
            scene.update();

            if (Keyboard.isPressed(key.F1)) {
                if (Editor.isOpen) Editor.close();
                else Editor.open();
            }
        }
    }
}
