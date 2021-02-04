
namespace Engine {
    public class Skybox {

        private static Mesh<posVertex> skymesh;
        static Skybox() {
            skymesh = new Mesh<posVertex>(MeshFactory<posVertex>.genCube(1, 10f));
            skymesh.data.flipIndices();
            skymesh.updateBuffers();
        }

        public Shader shader;

        public void render() {
            shader.use();
            skymesh.render();
        }

    }

    public class CubemapSkybox : Skybox {
        
        public CubemapSkybox() {
            shader = Assets.getShader("CubemapSkybox");
        }

        public static CubemapSkybox generate(Shader shader) {
            throw new System.NotImplementedException();
        }

    }
}
