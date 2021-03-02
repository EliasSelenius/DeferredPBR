using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using Nums;

namespace Engine {

    public abstract class SceneBase {
        internal abstract void update();
        internal abstract void updateCamera();
        internal abstract void renderGeometry();
        internal abstract void renderLights();
        internal abstract void renderGui();
    }

    public interface IRenderer {
        void render();
        void render(PBRMaterial material) {}
        void renderId();
    }

    public class Scene : SceneBase {

        public static Scene active { get; private set; }
        public static void load(Scene scene) {
            Application.scene = Scene.active = scene;
        }
        static Scene() {
            load(new Scene());
        }

        public Skybox skybox;
        public Camera camera { get; internal set; }

        List<Gameobject> _gameobjects = new List<Gameobject>();
        public readonly ReadOnlyCollection<Gameobject> gameobjects;
        
        public readonly List<Pointlight> pointlights = new List<Pointlight>();
        public readonly List<Dirlight> dirlights = new List<Dirlight>();

        internal readonly List<IRenderer> renderers = new List<IRenderer>();

        internal event System.Action update_event;

        internal ColliderCollection colliders = new ColliderCollection();

        public Scene() {
            gameobjects = _gameobjects.AsReadOnly();
        }

        internal void _addGameobject(Gameobject obj) => _gameobjects.Add(obj);
        internal void _removeGameobject(Gameobject obj) => _gameobjects.Remove(obj);

        internal override void update() {
            update_event?.Invoke();
            colliders.testCollisions(colliders);
        }

        internal override void updateCamera() {
            camera.updateUniformBuffer();
        }
        
        internal override void renderGeometry() {
            foreach (var r in renderers) r.render();
        }

        internal override void renderLights() {
            Renderer.lightPass_dirlight.use();
            foreach (var light in dirlights) {
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightDir"), light.dir.x, light.dir.y, light.dir.z);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                Lights.dirlightMesh.render();
            }
            

            Renderer.lightPass_pointlight.use();
            foreach (var light in pointlights) {

                //vec3 v = (camera.viewMatrix.transpose * new vec4(light.position.x, light.position.y, light.position.z, 1.0f)).xyz;
                vec3 v = (Renderer.viewMatrix.transpose * new vec4(light.position.x, light.position.y, light.position.z, 1.0f)).xyz;
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightPosition"), 1, ref v.x);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                var m = light.calcModelMatrix();
                GLUtils.setUniformMatrix4(Renderer.lightPass_pointlight.id, "model", ref m);
                Lights.pointlightMesh.render();
            }

            
            GL.Enable(EnableCap.DepthTest);
            skybox?.render();
        }

        internal override void renderGui() {

        }

    }
}