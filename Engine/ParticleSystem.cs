using System;
using Nums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

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

    use cases (birth, death) conditions
        waterfall (ondeath, cls)
        water splash (prog, cls)
        engine flame (ondeath, eol)
        fire (ondeath, eol)
        explosion (prog, eol)
        ambient dust (ondeath, oob)

    different particle death conditions
        out of bounds (oob)
        end of lifetime (eol)
        collides (cls)
    different particle birth conditions
        immediately takes the place of a dead particle (ondeath)
        emission rate 
        programaticly (prog)


    different particle emissions
        point
        insideSphere
        model surface


    prate * ptime = maxp
    prate = maxp / ptime
*/

namespace Engine {

    public enum ParticleDeathConditions {
        outOfBounds
    }

    public enum ParticleBirthConditions {
        immediate,
        emissionRate,
        programatic
    }

    public class ParticleEmitter {

    }

    public partial class Scene {
        internal readonly List<ParticleSystem> particleSystems = new();
    }

    public class ParticleSystem : Component {

        protected override void onEnter() => scene.particleSystems.Add(this);
        protected override void onLeave() => scene.particleSystems.Remove(this);

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
        const int numParticles = 1_000;

        public ParticleSystem() {
            //vbo = GLUtils.createBuffer(particle.bytesize * numParticles);
            
            var ps = new particle[numParticles];
            for (int i = 0; i < numParticles; i++) {
                vec3 vel = new vec3(math.rand(), math.rand(), math.rand());
                float size = 0.1f;// math.range(0.1f, 2f);
                ps[i] = new particle {
                    _pos = new vec4(math.rand(), math.rand(), math.rand(), 0) * 50,
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
            Renderer.whiteTexture.bind(TextureUnit.Texture0);

            GL.Enable(EnableCap.ProgramPointSize);
            //GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Points, 0, numParticles);

            computeShader.use();
            GL.Uniform3(GL.GetUniformLocation(computeShader.id, "origin"), 1, ref transform.position.x);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, vbo);
            Shader.dispatchCompute(numParticles);

        }

    }











}