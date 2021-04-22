using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using Engine.Gui;

namespace Engine.Editor {

    public enum EditorRenderMode {
        lights,
        wireframe,
        solid
    }

    public class SceneViewEditor : SceneBase {

        public static bool isOpen => Application.scene == instance;
        public static EditorRenderMode renderMode;
        public static LinkedList<Gameobject> selection = new();

        static SceneViewEditor instance = new(); 
        static SceneViewEditor() { }
        

        public static readonly Gui.Canvas canvas = new(Renderer.windowWidth, Renderer.windowHeight);
        Scene editorScene = new();

        ivec2 lastLeftclickPos;

        TextEditor textEditor = new();

        ContexMenu deleteMenu = new("Delete", 
            ("parent & chilldren", null), 
            ("parent only", null),
            ("children only", null)
        );

        private SceneViewEditor() {
            Application.window.Resize += onWindowResize;

            var cam = new Gameobject(
                new Camera(),
                new EditorCamera()
            );
            cam.enterScene(editorScene);



            /*var transformGizmo = new Gameobject(
                new MeshRenderer {
                    mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genSphere(10, 0.5f)),
                    materials = new[] {
                        PBRMaterial.defaultMaterial
                    }
                }
            );*/

            var transformGizmoPrefab = Assets.getPrefab("Engine.data.models.TransformGizmo.pivot");
            var transformGizmo = transformGizmoPrefab.createInstance();
            transformGizmo.enterScene(editorScene);

            int i = 0;
            foreach (var c in transformGizmo.children) {
                i += 3;
                c.transform.position += i; 
            }


        }

        private void onWindowResize(OpenTK.Windowing.Common.ResizeEventArgs args) {
            canvas.resize(args.Width, args.Height);
        }


        internal override void update() {
            editorScene.update();

            //if (Keyboard.isPressed(key.M)) renderMode = EditorRenderMode.wireframe;

            if (Keyboard.isPressed(key.Delete)) {
                foreach (var s in selection) s.destroy();
                selection.Clear();
            }

            if (Keyboard.isPressed(key.X)) {
                deleteMenu.open();
            }

        }



        internal override void updateCamera() {            
            editorScene.camera.updateUniformBuffer();
        }

        internal override void renderGeometry() {
            //GL.LineWidth(10);
            if (renderMode == EditorRenderMode.wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            Scene.active.renderGeometry();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);


            GL.DepthFunc(DepthFunction.Always);
            editorScene.renderGeometry();
            GL.DepthFunc(DepthFunction.Lequal);



            { // selections

                // handle selections
                if (Mouse.state == MouseState.free) {
                    if (Mouse.isPressed(MouseButton.left)) {
                        lastLeftclickPos = (ivec2)Mouse.position;
                    } else if(Mouse.isDown(MouseButton.left)) {
                        canvas.rect(lastLeftclickPos, (Mouse.position - lastLeftclickPos), color.rgba(1,1,1,0.5f));
                    } else if (Mouse.isReleased(MouseButton.left)) {
                        var mpos = (ivec2)Mouse.position;

                        if (lastLeftclickPos.x == mpos.x && lastLeftclickPos.y == mpos.y) { // single selection:
                            var obj = Mousepicking.select(Scene.active, (ivec2)Mouse.position)?.gameobject;
                            
                            if (Keyboard.isDown(key.LeftShift)) {
                                if (selection.Contains(obj)) selection.Remove(obj);
                                else if (obj is not null) selection.AddLast(obj);
                            } else {
                                selection.Clear();
                                if (obj is not null) selection.AddLast(obj);
                            }
                        } else { // box selection
                            var sel = Mousepicking.select(Scene.active, lastLeftclickPos, (ivec2)Mouse.position);
                        
                            if (Keyboard.isDown(key.LeftShift)) {
                                foreach (var s in sel) if (s.gameobject is not null && !selection.Contains(s.gameobject)) selection.AddLast(s.gameobject);
                            } else {
                                selection.Clear();
                                foreach (var s in sel) if (s.gameobject is not null) selection.AddLast(s.gameobject);
                            }
                        }
                    }
                }


                // highlight selection
                foreach (var g in selection) {
                    //g.transform.rotate(vec3.unity, 0.05f);
                    for (int i = 0; i < 10; i++) {
                        g.calcWorldPosition(out vec3 wpos);

                        foreach (var c in g.children) {
                            c.calcWorldPosition(out vec3 cpos);
                            Gizmo.line(wpos, cpos);
                        }

                        if (g.parent != null) {
                            g.parent.calcWorldPosition(out vec3 ppos);
                            Gizmo.line(wpos, ppos);
                        }

                        Gizmo.color(Utils.randColor());
                        Gizmo.point(new vec3(math.rand(), math.rand(), math.rand()) + wpos);
                    }
                }
            }





            Gizmo.bezier(vec3.zero, (20, 5, 7), 10);

            //GL.PointParameter(PointParameterName.)
            GL.PointSize(10);

            
        }

        internal override void renderLights() {
            Scene.active.renderLights();
        }

        internal override void renderFrame() {

            foreach (var g in Scene.active.gameobjects) {
                foreach (var c in g.components) {
                    c.editorRender();
                }
            }

            Gizmo.dispatchFrame();

            if (selection.First != null) {
                selection.First.Value.calcWorldPosition(out vec3 wpos);
                editorScene.camera.world2screen(in wpos, out vec2 coords);
                coords.y = -coords.y;
                coords = (coords + vec2.one) / 2f;
                coords *= canvas.size;
                //coords = (coords + canvas.size / 2);

                
                canvas.text(coords, Font.arial, 30, "Hello World", in color.white);


            }

            canvas.text(vec2.zero, Font.arial, 16, "fps: " + Renderer.fps, in color.white);
            //canvas.rect(canvas.size/2, canvas.size/2 - 10, in color.white);

            Console.render(canvas);
            TextEditor.selected?.render(canvas);
            if (Keyboard.isPressed(key.Escape)) {
                if (TextEditor.selected == null) TextEditor.selected = textEditor;
                else TextEditor.selected = null;
            }

            ContexMenu.render(canvas);

            canvas.dispatchFrame();
        }


        public static void open() => Application.scene = instance;
        public static void close() => Application.scene = Scene.active;
        
    }

    class TextEditor {
        public static TextEditor selected = null;
        
        static TextEditor() {
            Keyboard.onKeyPressed += keyboard_keypressed;
            Keyboard.onTextInput += keyboard_textinput;
        }

        static void keyboard_keypressed(key k, keymod m) {
            var s = selected;
            if (s == null) return;

            if (k == key.Up) s.moveCursor(0, -1); 
            else if (k == key.Down) s.moveCursor(0, 1); 
            else if (k == key.Left) s.moveCursor(-1, 0); 
            else if (k == key.Right) s.moveCursor(1, 0); 

            else if (k == key.Enter) {
                var sb = new StringBuilder(s.currentLine.ToString().Substring(s.cursor.x));
                s.currentLine.Remove(s.cursor.x, s.currentLine.Length - s.cursor.x);
                s.lines.AddAfter(s.lines.Find(s.currentLine), sb); 
                s.moveCursor(-s.cursor.x, 1);
            } else if (k == key.Backspace) {
                if (s.cursor.x == 0) {
                    if (s.cursor.y != 0) {
                        var o = s.lines.ElementAt(s.cursor.y - 1);
                        var l = o.Length;
                        o.Append(s.currentLine.ToString());
                        s.lines.Remove(s.currentLine);
                        s.moveCursor(l, -1);
                    }
                } else {
                    s.currentLine.Remove(s.cursor.x - 1, 1);
                    s.moveCursor(-1, 0);
                }
            } else if (k == key.Delete) {
                if (s.cursor.x == s.currentLine.Length) {
                    if (s.cursor.y != s.lines.Count-1) {
                        s.lines.ElementAt(s.cursor.y + 1).Insert(0, s.currentLine.ToString());
                        s.lines.Remove(s.currentLine);
                        s.moveCursor(0, 0);
                    }
                } else {
                    s.currentLine.Remove(s.cursor.x, 1);
                }
            } else if (k == key.Tab) {
                s.input("    ");
            }
        }


        static void keyboard_textinput(string text) {
            selected?.input(text);
        }
        
        
        LinkedList<StringBuilder> lines = new LinkedList<StringBuilder>();
        StringBuilder currentLine;

        ivec2 cursor = ivec2.zero;

        public TextEditor() {

            // add first initial line
            currentLine = new StringBuilder("Nice text rendering dude!");
            lines.AddLast(currentLine);
        }

        private void moveCursor(int x, int y) {
            currentLine = lines.ElementAt(cursor.y = math.clamp(cursor.y += y, 0, lines.Count-1));
            cursor.x = math.clamp(cursor.x += x, 0, currentLine.Length);
        }

        private void input(string text) {
            Console.notify(text + " was pressed");
            
            currentLine.Insert(cursor.x, text);
            cursor.x += text.Length;
        }

        public void render(Gui.Canvas canvas) {

            var textcolor = color.hex(0xd4d4d4ff);
            canvas.text(100, Font.arial, 22, "Some file title", textcolor);


            var textareapos = new vec2(110, 140);

            vec2 linepos = textareapos;
            int i = 0;
            foreach(var sb in lines) {
                canvas.text(linepos, Font.arial, 16, (i+1).ToString(), in textcolor);
                canvas.text((linepos.x + 20, linepos.y), Font.arial, 16, sb.ToString(), in textcolor);
                linepos.y += 16;
                i++;
            }

            // cursor
            canvas.rect(textareapos + (20 + Text.length(currentLine.ToString(), cursor.x, 16, Font.arial), cursor.y * 16), (2, 16), in color.white);

            canvas.rectborder(textareapos, (20, linepos.y - textareapos.y), 1, in textcolor);
            

            canvas.rect((canvas.width - 110, 100), 10, in color.gray);


            canvas.rect(textareapos, canvas.size - (220, 250), color.hex(0x2e2e2eff));
            canvas.rect(100, canvas.size - 200, color.hex(0x1e1e1eff));

        }
    }

    public class ContexMenu {
        static ivec2 pos;
        static ContexMenu current;

        string title;
        (string text, Action action)[] options;

        public ContexMenu(string title, params (string text, Action action)[] options) {
            this.title = title;
            this.options = options;
        }

        public void open() {
            pos = (ivec2)Mouse.position;
            current = this;
        }
        public static void close() {
            current = null;
        }

        public static void render(Canvas canvas) {
            if (current is null) return;

            current.draw(canvas);
        }

        void draw(Canvas canvas) {
            
            var textcolor = color.hex(0xd4d4d4ff);
            var p = pos;

            canvas.text(p, Font.arial, 20, title, in textcolor);
            canvas.rect(p, (130, 20 + options.Length * 16), color.hex(0x1e1e1eff));
            p.y += 20;
            p.x += 15;

            for (int i = 0; i < options.Length; i++) {
                var o = options[i];
                canvas.text(p, Gui.Font.arial, 16, o.text, textcolor);
                p.y += 16;
            }

        }
    }

}

