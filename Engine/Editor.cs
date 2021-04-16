using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Engine {

    public enum EditorRenderMode {
        lights,
        wireframe,
        solid
    }

    public class Editor : SceneBase {

        public static bool isOpen => Application.scene == instance;
        public static EditorRenderMode renderMode;
        public static LinkedList<Gameobject> selection = new();

        static Editor instance = new(); 
        static Editor() { }
        

        public static readonly Gui.Canvas canvas = new(Renderer.windowWidth, Renderer.windowHeight);
        Scene editorScene = new();

        ivec2 lastLeftclickPos;

        TextEditor textEditor = new();

        private Editor() {
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

            if (Keyboard.isPressed(key.M)) renderMode = EditorRenderMode.wireframe;

            if (Keyboard.isPressed(key.Delete)) {
                foreach (var s in selection) s.destroy();
                selection.Clear();
            }

            if (Keyboard.isDown(key.X)) {
                contexMenu(lastLeftclickPos, ("delete", () => {}), ("delete parent only", () => {}));
            }

        }

        static void contexMenu(vec2 pos, params (string text, Action action)[] options) {

            canvas.rect(pos, (130, options.Length * 16), color.hex(0x1e1e1eff));
            var textcolor = color.hex(0xd4d4d4ff);

            for (int i = 0; i < options.Length; i++) {
                var o = options[i];
                canvas.text(pos, Font.arial, 16, o.text, textcolor);
                pos.y += 16;
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


            canvas.text(vec2.zero, Font.arial, 16, "fps: " + Renderer.fps, in color.white);
            //canvas.rect(canvas.size/2, canvas.size/2 - 10, in color.white);

            //textEditor.render(canvas);

            canvas.dispatchFrame();
        }


        public static void open() => Application.scene = instance;
        public static void close() => Application.scene = Scene.active;
        
    }

    public class EditorCamera : Component {

        vec3 velocity;
        float speedMult = 100f;

        protected override void onUpdate() {

            transform.position += velocity * Application.deltaTime;
            velocity *= 0.1f * Application.deltaTime;

            if (Mouse.isDown(MouseButton.right)) {
                Mouse.state = MouseState.disabled;
                var d = Mouse.delta / 100f;
                transform.rotate(vec3.unity, d.x);
                transform.rotate(transform.left, -d.y);

                velocity += (transform.forward * Keyboard.getAxis(key.S, key.W) + transform.left * Keyboard.getAxis(key.D, key.A)) * speedMult;
                
                
                float rate = 0.8f * Application.deltaTime;
                if (Keyboard.isDown(key.LeftShift)) speedMult *= (1f + rate);
                else if (Keyboard.isDown(key.LeftControl)) speedMult = math.max(1f, speedMult * (1f - rate));


            } else {
                Mouse.state = MouseState.free;
            }

            Editor.canvas.text((0, 30), Font.arial, 16, "velocity: " + velocity.length.ToString(), color.white);
            Editor.canvas.text((0, 46), Font.arial, 16, "speedMul: " + speedMult, color.white);
        }

        public void focus(in vec3 point) {
            transform.lookat(point, vec3.unity);

        }
    }

    public static class Gizmo {
        static Shader shader;

        static Batch points = new(PrimitiveType.Points);
        static Batch lines = new(PrimitiveType.Lines);

        static vec4 currentColor = vec4.one;

        static Gizmo() {
            shader = Assets.getShader("gizmo");
        }

        public static void color(in vec4 color) => currentColor = color;
        public static void color(in color color) => color.color2vec(in color, out currentColor);

        public static void point(vec3 pos) {
            points.vertex(pos, currentColor);
            points.index(points.vertices.Count - 1);
        }
        public static void line(vec3 start, vec3 end) {
            lines.vertex(start, currentColor);
            lines.vertex(end, currentColor);

            lines.index(lines.vertices.Count - 2);
            lines.index(lines.vertices.Count - 1);
        }

        public static void bezier(vec3 p0, vec3 p1, vec3 p2) {
            const int res = 20;
            float t = 0;
            
            line(p0, p1);
            line(p1, p2);
            for (int i = 0; i < res; i++) {
                line(math.bezier(p0, p1, p2, t), math.bezier(p0, p1, p2, t += 1f / res));
            }
        }


        internal static void dispatchFrame() {
            shader.use();

            points.render();
            lines.render();
        }

        class Batch {
            public PrimitiveType primitiveType;
            public List<posColorVertex> vertices = new();
            public List<uint> indices = new();

            int vao, vbo, ebo;

            public Batch(PrimitiveType pType) {
                primitiveType = pType;

                vbo = GLUtils.createBuffer();
                ebo = GLUtils.createBuffer();
                vao = GLUtils.createVertexArray<posColorVertex>(vbo, ebo);
            }

            public void render() {
                GLUtils.bufferdata(vbo, vertices.ToArray());
                GLUtils.bufferdata(ebo, indices.ToArray());
                
                GL.BindVertexArray(vao);
                GL.DrawElements(primitiveType, indices.Count, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);

                clear();
            }

            void clear() {
                vertices.Clear();
                indices.Clear();
            }

            public void vertex(in vec3 pos, in vec4 color) => vertices.Add(new posColorVertex { position = pos, color = color });
            public void index(int i) => indices.Add((uint)i);
        }
    }


    class SceneViewWindow : Gui.Window {
        
        public SceneViewWindow() : base("Scene", (200, 400)) {

        }

        protected override void onAttached() {
        }

        protected override void renderContent() {
            
            for (int i = 0; i < Scene.active.gameobjects.Count; i++) {
                var o = Scene.active.gameobjects[i];
                if (o.isRootObject) {
                    obj(i, o);
                }
            }
        }
        

        void obj(int i, Gameobject o) {
            start((width, 16));
            text("Gameobject " + i, 16, system.theme.textColor);
            if (o.isParent) {
                start((20, 0), (width - 20, 0));
                for (int j = 0; j < o.children.Count; j++) {
                    obj(i, o.children[j]);
                }
                end();
            }
            end();
        }
    }


    class TextEditor {
        System.Text.StringBuilder builder;


        public void render(Gui.Canvas canvas) {
            var textcolor = color.hex(0xd4d4d4ff);
            canvas.text(100, Font.arial, 22, "Some file title", textcolor);

            canvas.rect((110, 140), canvas.size - (220, 250), color.hex(0x2e2e2eff));
            canvas.rect(100, canvas.size - 200, color.hex(0x1e1e1eff));
        }
    }

}