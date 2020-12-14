
namespace Engine {
    public class Skybox {

        private static Mesh<posVertex> invertedCube;

        static Skybox() {
            invertedCube = MeshFactory<posVertex>.genCube(1, 1f);
            invertedCube.flipIndices();
            invertedCube.bufferdata();
        }

        public Shader shader;

        public void render() {
            shader.use();
            invertedCube.render();       
        }

    }

    public class CubemapSkybox : Skybox {
        

        public static CubemapSkybox generate(Shader shader) {
            throw new System.NotImplementedException();
        }

    }
}
