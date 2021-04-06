
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

        public static CubemapSkybox generate(Shader shader, int size = 512, Filter filter = Filter.Linear) {
            int c = GLUtils.generateCubemap(
                shader, 
                
                width: size, 
                height: size, 
                
                internalFormat: OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgba16f, 
                
                wrap: WrapMode.ClampToEdge, // NOTE: wrap is hardcoded to ClampToEdge since ClampToBorder makes border artifacts, and any Reapeat option does not make sence for a skybox
                filter: filter
                );

            return new CubemapSkybox(c);
        }

    }
}
