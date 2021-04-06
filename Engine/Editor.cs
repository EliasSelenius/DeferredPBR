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

        private Editor() {
            Application.window.Resize += onWindowResize;

            var cam = new Gameobject(
                new Camera(),
                new CameraFlyController()
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
            GL.LineWidth(2);
            if (renderMode == EditorRenderMode.wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            Scene.active.renderGeometry();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);


            GL.DepthFunc(DepthFunction.Always);
            editorScene.renderGeometry();
            GL.DepthFunc(DepthFunction.Lequal);


            // handle selections
            if (Mouse.isPressed(MouseButton.left)) {
                var r = Mousepicking.select(Scene.active, (ivec2)Mouse.position);
                
                if (!Keyboard.isDown(key.LeftShift)) selection.Clear();
                if (r is null) selection.Clear();
                else if (selection.Contains(r.gameobject)) selection.Remove(r.gameobject);
                else selection.AddLast(r.gameobject);
            }

            foreach (var g in selection) {
                g.transform.rotate(vec3.unity, 0.05f);
            }
        }

        internal override void renderLights() {
            Scene.active.renderLights();
        }

        internal override void renderGui() {
            canvas.render();
        }


        public static void open() => Application.scene = instance;
        public static void close() => Application.scene = Scene.active;
        
    }

    public static class Gizmo {
        static Shader shader;

        public static void point(vec3 pos) {}
        public static void line(vec3 start, vec3 end) {}

        internal static void dispatchFrame() {
            shader.use();
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