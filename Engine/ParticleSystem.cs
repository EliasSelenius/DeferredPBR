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
            
            public vec4 _pos, _vel_size, _color;
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
                vec3 vel = new vec3(math.rand(), math.rand(), math.rand()) / 10f;
                float size = math.range(0.1f, 2f);
                ps[i] = new particle {
                    _pos = new vec4(math.rand(), math.rand(), math.rand(), 0) * 600f,
                    _vel_size = new vec4(vel, size),
                    _color = new vec4(math.rand(), math.rand(), math.rand(), 1)
                };
            }
            vbo = GLUtils.createBuffer(ps);

            vao = GLUtils.createVertexArray<particle>(vbo);
        }

        public void render() {
            renderShader.use();
            //spritesheet.bind(TextureUnit.Texture0);

            //GL.Enable(EnableCap.PointSprite);
            GL.Enable(EnableCap.ProgramPointSize);
            //GL.PointSize(10f);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Points, 0, numParticles);

            computeShader.use();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, vbo);
            Shader.dsipatchCompute(numParticles);

        }

    }
}