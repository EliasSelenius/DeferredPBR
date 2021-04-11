using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;

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
        

        Gui.Canvas canvas = new(Renderer.windowWidth, Renderer.windowHeight);
        Scene editorScene = new();
        bool isMultiselecting;

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


            // handle selections
            if (Mouse.state == MouseState.free && Mouse.isPressed(MouseButton.left)) {
                var r = Mousepicking.select(Scene.active, (ivec2)Mouse.position);
                
                if (!Keyboard.isDown(key.LeftShift)) selection.Clear();
                if (r is null) selection.Clear();
                else if (selection.Contains(r.gameobject)) selection.Remove(r.gameobject);
                else selection.AddLast(r.gameobject);
            }

            // handle multi-selection
            if (Mouse.state == MouseState.free) {
                if (Mouse.isDown(MouseButton.left)) {
                    isMultiselecting = true;

                } else if (Mouse.isReleased(MouseButton.left)) {
                    isMultiselecting = false;
                }
            }
            

            if (isMultiselecting) {
                canvas.rect(400, 200, color.black);
            }



            foreach (var g in selection) {
                g.transform.rotate(vec3.unity, 0.05f);
            
                for (int i = 0; i < 10; i++) {
                    g.calcWorldPosition(out vec3 wpos);
                    Gizmo.point(new vec3(math.rand(), math.rand(), math.rand()) + wpos);
                }
            }

            Gizmo.line(vec3.zero, vec3.one * 10);
            Gizmo.line(vec3.zero, (10, 10, 0));
            Gizmo.line((0, 0, 20), (10, 10, 10));

            Gizmo.bezier(vec3.zero, (20, 5, 7), 10);

            //GL.PointParameter(PointParameterName.)
            GL.PointSize(10);

            

        }

        internal override void renderLights() {
            Scene.active.renderLights();
        }

        internal override void renderFrame() {
            Gizmo.dispatchFrame();
        }

        internal override void renderGui() {
            canvas.render();
        }


        public static void open() => Application.scene = instance;
        public static void close() => Application.scene = Scene.active;
        
    }

    public class EditorCamera : Component {

        vec3 velocity;
        float speedMult = 1000f;

        protected override void onUpdate() {

            transform.position += velocity * Application.deltaTime;
            velocity *= 0.1f * Application.deltaTime;

            if (Mouse.isDown(MouseButton.right)) {
                Mouse.state = MouseState.disabled;
                var d = Mouse.delta / 100f;
                transform.rotate(vec3.unity, d.x);
                transform.rotate(transform.left, -d.y);

                velocity += (transform.forward * Keyboard.getAxis(key.S, key.W) + transform.left * Keyboard.getAxis(key.D, key.A)) * speedMult * Application.deltaTime;
                

            } else {
                Mouse.state = MouseState.free;
            }
        }

        public void focus(in vec3 point) {
            transform.lookat(point, vec3.unity);

        }
    }

    public static class Gizmo {
        static Shader shader;

        static Batch points = new(PrimitiveType.Points);
        static Batch lines = new(PrimitiveType.Lines);

        static Gizmo() {
            shader = Assets.getShader("gizmo");
        }

        public static void point(vec3 pos) {
            points.vertices.Add(new posVertex { position = pos });
            points.indices.Add((uint)(points.vertices.Count - 1));
        }
        public static void line(vec3 start, vec3 end) {
            lines.vertices.Add(new posVertex { position = start });
            lines.vertices.Add(new posVertex { position = end });

            lines.indices.Add((uint)(lines.vertices.Count - 2));
            lines.indices.Add((uint)(lines.vertices.Count - 1));
        }


        public static void bezier(vec3 p0, vec3 p1, vec3 p2) {
            float t = 0;
            
            for (int i = 0; i < 10; i++) {
                vec3 s = p0.lerp(p1, t).lerp(p1.lerp(p2, t), t);
                t += 1f / 10f;
                line(s, p0.lerp(p1, t).lerp(p1.lerp(p2, t), t));
            }
            

        }


        internal static void dispatchFrame() {
            shader.use();

            points.render();
            lines.render();
        }

        class Batch {
            public PrimitiveType primitiveType;
            public List<posVertex> vertices = new();
            public List<uint> indices = new();

            int vao, vbo, ebo;

            public Batch(PrimitiveType pType) {
                primitiveType = pType;

                vbo = GLUtils.createBuffer();
                ebo = GLUtils.createBuffer();
                vao = GLUtils.createVertexArray<posVertex>(vbo, ebo);
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


}