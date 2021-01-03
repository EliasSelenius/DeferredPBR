using System.IO;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Xml;

namespace Engine {

    unsafe struct ptr<T> where T : unmanaged {
        private T* p;

        public T value {
            get => *p;
            set => *p = value;
        }

        public T this[int i] {
            get => p[i];
            set => p[i] = value;
        }

        public ptr(in T t) {
            fixed (T* p = &t) {
                this.p = p;
            }
        }

        public static implicit operator ptr<T> (in T t) => new ptr<T>(in t);


        public static void test() {
            vec2 v = (1,2);
            ptr<vec2> p = v;

            System.Console.WriteLine("value:   " + v);
            System.Console.WriteLine("Pointer: " + p.value);

            p.value = (3,4);

            System.Console.WriteLine("value:   " + v);
            System.Console.WriteLine("Pointer: " + p.value);


            v = (6,6);

            System.Console.WriteLine("value:   " + v);
            System.Console.WriteLine("Pointer: " + p.value);

            

        }

    }

    public static class app {
        public static GameWindow window { get; private set; }

        public static float deltaTime;

        public static void Main() {

            window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            window.Size = (1600, 900);

            window.Load += Assets.load;
            window.Load += Renderer.load;
            window.Load += load;
            window.UpdateFrame += update;
            window.RenderFrame += Renderer.drawframe;
            window.Resize += Renderer.windowResize;
            window.Run();
        }

        static void load() {
            window.VSync = VSyncMode.Off;
            window.CursorGrabbed = true;
        }


        static void update(FrameEventArgs e) {
            deltaTime = (float)e.Time;
            Scene.active.update();
        }
    }
}
