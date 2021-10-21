using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using Nums;

namespace Engine {

    public abstract class SceneBase {
        internal abstract void update();
        internal abstract void updateCamera();

        internal abstract void geometryPass();
        internal abstract void lightPass();
        internal abstract void forwardPass();
        
        internal abstract void renderFrame();
    }

    public interface IRenderer {
        Gameobject gameobject { get; }
        Transform transform { get; }

        void render();
    }


    public partial class Scene : SceneBase {

        static Scene _active;
        public static Scene active { 
            get => _active;
            set => Application.scene = _active = value;
        }
        static Scene() {
            active = new Scene(); 
        }

        public Skybox skybox;
        public Camera camera { get; internal set; }

        List<Gameobject> _gameobjects = new List<Gameobject>();
        public readonly ReadOnlyCollection<Gameobject> gameobjects;
        
        internal List<Pointlight> _pointlights = new List<Pointlight>();
        public readonly ReadOnlyCollection<Pointlight> pointlights;

        public readonly List<Dirlight> dirlights = new List<Dirlight>();

        internal readonly List<IRenderer> renderers = new();
        internal readonly List<IRenderer> forwardpassRenderers = new();

        internal event System.Action update_event;

        internal ColliderCollection colliders = new ColliderCollection();

        public Scene() {
            gameobjects = _gameobjects.AsReadOnly();
            pointlights = _pointlights.AsReadOnly();
        }

        internal void _addGameobject(Gameobject obj) => _gameobjects.Add(obj);
        internal void _removeGameobject(Gameobject obj) => _gameobjects.Remove(obj);

        public Gameobject createObject(params Component[] comps) {
            var g = new Gameobject(comps);
            g.enterScene(this);
            return g;
        }

        internal override void update() {
            update_event?.Invoke();
            colliders.testCollisions(colliders);
        }

        internal override void updateCamera() {
            camera.use();
        }
        
        internal override void geometryPass() {
            foreach (var r in renderers) r.render();
        }

        internal override void lightPass() {
            Renderer.lightPass_dirlight.use();
            foreach (var light in dirlights) {
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightDir"), light.dir.x, light.dir.y, light.dir.z);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                GL.Uniform1(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "ambientScale"), light.ambientScale);
                Lights.dirlightMesh.render();
            }
            

            Renderer.lightPass_pointlight.use();
            foreach (var light in _pointlights) {
                light.calcLightVolumeModelMatrix(out mat4 model);
                GLUtils.setUniformMatrix4(Renderer.lightPass_pointlight.id, "model", ref model);

                var lightWorldPos = model.row4.xyz;
                vec3 v = (Renderer.viewMatrix.transpose * new vec4(lightWorldPos.x, lightWorldPos.y, lightWorldPos.z, 1.0f)).xyz;
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightPosition"), 1, ref v.x);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                
                Lights.pointlightMesh.render();
            }

            
            GL.Enable(EnableCap.DepthTest);
            skybox?.render();

            foreach (var p in particleSystems) {
                p.render();
            }
        }

        internal override void forwardPass() {
            foreach (var r in forwardpassRenderers) r.render();
        }


        internal override void renderFrame() {
            camera.canvas?.dispatchFrame();
        }
    }
}