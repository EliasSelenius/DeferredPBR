using System;
using Nums;
using OpenTK.Graphics.OpenGL4;

/*
    TODO:
        <done> particle projection
        <done> particle sizes
        sprites
        velocity
        blending
*/

namespace Engine {

    public class ParticleEmitter {

    }

    public class ParticleSystem {

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct particle {
            public const int bytesize = sizeof(float) + vec3.bytesize * 2;
            
            public vec3 pos, vel;
            public float size;
        }

        static Shader computeShader;
        static Shader renderShader;

        static Texture2D spritesheet;

        static ParticleSystem() {
            computeShader = Assets.getShader("particle_compute");
            renderShader = Assets.getShader("particle");
            spritesheet = Assets.getTexture2D("Engine.data.textures.lowres_particle_spritesheet.png");
        }

        int vao, vbo;
        const int numParticles = 1_000_000;

        public ParticleSystem() {
            //vbo = GLUtils.createBuffer(particle.bytesize * numParticles);
            
            var ps = new particle[numParticles];
            for (int i = 0; i < numParticles; i++) {
                ps[i] = new particle {
                    pos = new vec3(math.rand(), math.rand(), math.rand()) * 600f,
                    vel = new vec3(math.rand(), math.rand(), math.rand()),
                    size = math.range(0.1f, 1f)
                };
            }
            vbo = GLUtils.createBuffer(ps);

            vao = GLUtils.createVertexArray<particle>(vbo);
        }

        public void render() {
            renderShader.use();
            PBRMaterial.redPlastic.use();
            //spritesheet.bind(TextureUnit.Texture0);

            //GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.ProgramPointSize);
            //GL.PointSize(10f);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Points, 0, numParticles);
        }

    }
}