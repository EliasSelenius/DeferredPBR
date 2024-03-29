using Nums;
using System.Linq;

namespace Engine {

    static class ModelUBO {
        static Uniformblock uBlock;
        static ModelUBO() {
            uBlock = Uniformblock.get("Model");
            uBlock.bindBuffer(GLUtils.createBuffer(mat4.bytesize));
        }

        public static void setModelMatrix(ref mat4 model) {
            GLUtils.buffersubdata(uBlock.bufferId, 0, ref model);
        }
    }


    /*public abstract class DeferredRenderableComponent : Component {

        protected override void onEnter() {
            base.onEnter();
        }

        protected abstract void render();
    }*/

    public class MeshRenderer : Component, IRenderer {
        public Mesh<Vertex> mesh;
        public IMaterial[] materials;

        bool isUnlit() {
            var type = materials[0].GetType();

            // checks to see if all materials are of the same type, and throws if not.
            if (!materials.Select(x => x.GetType()).All(x => x == type)) throw new System.Exception();
            
            return materials[0] is Material;
        }

        public void render() {
            gameobject.calcModelMatrix(out mat4 mat);

            
            //mat.row1.xyz = Renderer.viewMatrix.col1.xyz;
            //mat.row2.xyz = Renderer.viewMatrix.col2.xyz;
            //mat.row3.xyz = Renderer.viewMatrix.col3.xyz;


            ModelUBO.setModelMatrix(ref mat);
            mesh.render(materials);
        }

        protected override void onEnter() {
            if (isUnlit()) scene.forwardpassRenderers.Add(this);
            else scene.renderers.Add(this);
        }

        protected override void onLeave() {
            if (isUnlit()) scene.forwardpassRenderers.Remove(this);
            else scene.renderers.Remove(this);
        }
    }
}
