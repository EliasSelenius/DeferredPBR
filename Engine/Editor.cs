using OpenTK.Graphics.OpenGL4;

namespace Engine {
    public class Editor : SceneBase {

        public static bool isOpen => Application.scene == instance;

        static Editor instance = new(); 
        static Editor() { }
        

        Gui.View gui;
        Scene editorScene = new();

        private Editor() {
            var sys = new Gui.WindowingSystem();
            gui = sys;


            sys.addWindow(new Gui.DebugWindow());

            var cam = new Gameobject(
                new Camera(),
                new CameraFlyController()
            );
            cam.enterScene(editorScene);



            var transformGizmo = new Gameobject(
                new MeshRenderer {
                    mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genSphere(10, 0.5f)),
                    materials = new[] {
                        PBRMaterial.defaultMaterial
                    }
                }
            );
            transformGizmo.enterScene(editorScene);


        }


        internal override void update() {
            editorScene.update();
        }

        internal override void updateCamera() {
            editorScene.camera.updateUniformBuffer();
        }

        internal override void renderGeometry() {
            Scene.active.renderGeometry();

            //GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            editorScene.renderGeometry();
        }

        internal override void renderLights() {
            Scene.active.renderLights();
        }

        internal override void renderGui() {
            gui.render();
        }


        public static void open() => Application.scene = instance;
        public static void close() => Application.scene = Scene.active;
        
    }
}