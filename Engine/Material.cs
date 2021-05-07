
using Nums;
using OpenTK.Graphics.OpenGL4;

namespace Engine {

    public interface IMaterial {
        void use();
    }

    public class Material : IMaterial {
        static Uniformblock uBlock = Uniformblock.require("Material");

        public readonly Shader shader;
        int buffer;

        public Material(Shader shader) {
            this.shader = shader;
        }

        public void setdata<T>(T data) where T : unmanaged {
            buffer = GLUtils.createBuffer(ref data);
        }

        public void use() {
            shader.use();
            uBlock.bindBuffer(buffer);
        }

        
    }


    public class PBRMaterial : IMaterial {
        public vec3 albedo = vec3.one;
        public float metallic = 0;
        public float roughness = 0;

        public Texture2D albedoMap = null;

        public void use() {
            (albedoMap ?? Renderer.whiteTexture).bind(TextureUnit.Texture0);

            GL.Uniform3(GL.GetUniformLocation(Renderer.geomPass.id, "material.albedo"), albedo.x, albedo.y, albedo.z);
            GL.Uniform1(GL.GetUniformLocation(Renderer.geomPass.id, "material.metallic"), metallic);
            GL.Uniform1(GL.GetUniformLocation(Renderer.geomPass.id, "material.roughness"), roughness);
        }

        public static readonly PBRMaterial defaultMaterial = new PBRMaterial {
            roughness = 0.2f
        };

        public static readonly PBRMaterial redPlastic = new PBRMaterial {
            albedo = (1.0f, 0, 0),
            metallic = 0,
            roughness = 0.05f
        };
    }
}
