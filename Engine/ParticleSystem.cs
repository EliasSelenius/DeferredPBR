using System;
using Nums;
using OpenTK.Graphics.OpenGL4;

/*
    TODO:
        <done> particle projection
        <done> particle sizes
        paritcle emission
        sprites
        <done> velocity
        blending


    ideal particle system capabilities:
        transparancy
        gravity (or forces in general)
        variable amount of particles 
        textures
        collision
    use cases
        waterfall
        water splash
        engine flame
        fire
        explosion
        ambient dust

    different particle death conditions
        out of bounds
        end of lifetime
        collides
    different particle birth conditions
        imeadeatly takes the place of a dead particle
        emission rate
        programaticly
*/

namespace Engine {

    public class ParticleEmitter {

    }

    public class ParticleSystem {

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct particle {            
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
            //GL.TexEnv()
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