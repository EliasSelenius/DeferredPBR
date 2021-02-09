
namespace Engine {
    public class Skybox {

        protected static Mesh<posVertex> skymesh;
        static Skybox() {
            skymesh = new Mesh<posVertex>(MeshFactory<posVertex>.genCube(1, 10f));
            skymesh.data.flipIndices();
            skymesh.updateBuffers();
        }

        public Shader shader;

        public virtual void render() {
            shader.use();
            skymesh.render();
        }

    }

    /*
        step 1: render CubemapSkybox
        step 2: render with cubemap
        step 3: generate cubemap from shader 


    */

    public class CubemapSkybox : Skybox {
        
        int cubemap;

        public CubemapSkybox(int cubemap) {
            shader = Assets.getShader("CubemapSkybox");
            this.cubemap = cubemap;
        }

        public override void render() {
            shader.use();
            GLUtils.bindCubemap(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0, cubemap);
            skymesh.render();
        }

        public static CubemapSkybox generate(Shader shader) {
            int c = GLUtils.generateCubemap(shader, 1024, 1024, OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba16f, WrapMode.ClampToBorder, Filter.Linear);
            return new CubemapSkybox(c);
        }

    }
}
