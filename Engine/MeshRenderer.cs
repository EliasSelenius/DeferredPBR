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

            if (!materials.Select(x => x.GetType()).All(x => x == type)) throw new System.Exception();
            
            return materials[0] is Material;
        }

        public void render() {
            gameobject.calcModelMatrix(out mat4 mat);

            
            //mat.row1.xyz = Renderer.viewMatrix.col1.xyz;
            //mat.row2.xyz = Renderer.viewMatrix.col2.xyz;
            //mat.row3.xyz = Renderer.viewMatrix.col3.xyz;


            ModelUBO.setModelMatrix(ref mat);
            //GLUtils.setUniformMatrix4(Renderer.geomPass.id, "model", ref mat);


            mesh.render(materials);
        }

        /*public void render(PBRMaterial material) {
            gameobject.calcModelMatrix(out mat4 mat);
            GLUtils.setUniformMatrix4(Renderer.geomPass.id, "model", ref mat);
            material.updateUniforms();
            mesh.render();
        }*/

        public void render(int shaderID) {
            gameobject.calcModelMatrix(out mat4 mat);
            //GLUtils.setUniformMatrix4(shaderID, "model", ref mat);
            ModelUBO.setModelMatrix(ref mat);
            mesh.render();
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
