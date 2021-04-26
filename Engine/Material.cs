
using Nums;
using OpenTK.Graphics.OpenGL4;

namespace Engine {

    /*public class Material {
        public Shader shader;

        IProperty[] properties;

        public void updateUniforms() {

        }

        interface IProperty {
            void bind(int loc);
        }

        struct vec3_prop : IProperty {
            public vec3 value;
            public void bind(int loc) => GL.Uniform3(loc, value.x, value.y, value.z);
        }
        struct vec2_prop : IProperty {

        }
        
    }*/


    public class PBRMaterial {
        public vec3 albedo = vec3.one;
        public float metallic = 0;
        public float roughness = 0;

        public Texture2D albedoMap = null;

        public void updateUniforms() {
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
