using Engine;
using Engine.Gui;


namespace Engine.Toolset {
    public static partial class Editor {
        static readonly SceneEditor sceneEditor = new();
        
        public static bool isOpen => Application.scene == sceneEditor;
        
        public static void open() {
            Application.scene = sceneEditor;
            Renderer.onDrawFrame += renderer_drawframe;
        }
        public static void close() {
            Application.scene = Scene.active;
            Renderer.onDrawFrame -= renderer_drawframe;
        }


        public static Theme theme = Theme.darkGreenish;

        public static readonly Canvas canvas = new(Renderer.windowWidth, Renderer.windowHeight);

        static Editor() {
            Application.window.Resize += onWindowResize;

            if (Renderer.computeShader != null)
                textbox.setText(Renderer.computeShader.sources[OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader].Replace("\t", "    "));
            //textbox.font = Font.arial;
        }

        static void onWindowResize(OpenTK.Windowing.Common.ResizeEventArgs args) {
            canvas.resize(args.Width, args.Height);
        }

        static Textbox textbox = new();
        static bool test_checkbox;
        static Common.TestObject test_instance = new();

        static void renderer_drawframe() {

            if (textbox.editing) { // text box 
                textbox.render(canvas, (500, 100));
            }

            if (Keyboard.isPressed(key.Escape)) {
                if (textbox.editing) {
                    Renderer.computeShader.sources[OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader] = textbox.getText();
                    if (!Renderer.computeShader.linkProgram()) System.Console.WriteLine(Renderer.computeShader.getInfolog());
                }
                textbox.editing = !textbox.editing;
            }


            foreach (var g in Scene.active.gameobjects) {
                foreach (var c in g.components) {
                    c.editorRender();
                }
            }



            // FPS:
            canvas.text((1000, 0), Font.arial, 16, "fps: " + Renderer.fps, in color.white);

            renderConsole();


            { // testing GUI features here:
                canvas.checkbox(3, ref test_checkbox);

                if (test_checkbox) {
                    Assets.drawGui(canvas);
                }


                Common.displayObject(canvas, 400, test_instance);


                canvas.hex(370, 40, in color.purple);
            }

            canvas.dispatchFrame();
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