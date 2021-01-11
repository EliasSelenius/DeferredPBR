using System.IO;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Xml;
using System;

namespace Engine {

    public static class app {
        public static GameWindow window { get; private set; }

        public static float deltaTime;

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


            var resProv = new EmbeddedResourceProvider(typeof(app).Assembly);
            Assets.load(resProv);


        }


        static void update(FrameEventArgs e) {
            deltaTime = (float)e.Time;
            Scene.active.update();
        }
    }
}
