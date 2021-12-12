using Engine;
using Engine.Gui;
using Nums;

namespace Engine.Toolset {


    public class TextEditor {

        public static TextEditor currentFocus; 

        public static void setFocus(TextEditor editor) {
            if (currentFocus != null) currentFocus.textbox.editing = false;
            
            currentFocus = editor;
            if (editor != null) editor.textbox.editing = true;
        }

        public string filename;
        Textbox textbox = new();

        int headerSize = 40;
        int footerSize = 20;
        int marginSize = 100;

        public TextEditor() {
            textbox.setText(Renderer.computeShader.sources[OpenTK.Graphics.OpenGL4.ShaderType.ComputeShader].Replace("\t", "    "));
            //textbox.setText(System.IO.File.ReadAllText("Program.cs"));

            textbox.editing = true;
        }

        public void render(Canvas canvas) {
            vec2 pos = (marginSize, headerSize);
            var w = canvas.width - marginSize * 2;
            var h = canvas.height - headerSize - footerSize;
            vec2 size = (w, h);

            canvas.text((marginSize, 0), Font.arial, headerSize, "File: " + filename, in color.white);


            textbox.render(canvas, pos);
            
            canvas.rectborder(pos, size, 2, in color.blue);
            canvas.rect(pos, size, color.rgba(0.1f, 0.1f, 0.1f, 0.8f));
        }
    }


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

        public static TextEditor textEditor;

        static Editor() {
            Application.window.Resize += onWindowResize;

            textEditor = new();
            //textbox.font = Font.arial;
        }

        static void onWindowResize(OpenTK.Windowing.Common.ResizeEventArgs args) {
            canvas.resize(args.Width, args.Height);
        }

        static bool test_checkbox;
        static Common.TestObject test_instance = new();

        static void renderer_drawframe() {

            foreach (var g in Scene.active.gameobjects) {
                foreach (var c in g.components) {
                    c.editorRender();
                }
            }



            // FPS:
            canvas.text((1000, 0), Font.arial, 16, "fps: " + Renderer.fps, in color.white);

            renderConsole();


            textEditor.render(canvas);


            /*{ // testing GUI features here:
                //canvas.checkbox(3, ref test_checkbox);

                if (test_checkbox) {
                    Assets.drawGui(canvas);
                }


                Common.displayObject(canvas, 400, test_instance);


                canvas.hex(370, 40, in color.magenta);

                canvas.test();
            }*/

            canvas.dispatchFrame();


            //renderToConsole();
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