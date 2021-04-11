using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;

namespace Engine {
    public static class Mousepicking {
        static Framebuffer framebuffer;
        public static Shader shader;


        static Mousepicking() {
            framebuffer = new Framebuffer(Renderer.windowWidth, Renderer.windowHeight, 
            new (FramebufferAttachment, RenderbufferStorage)[] {
                (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
            },
            
            new[] {
                FramebufferFormat.int32
            });

            shader = Assets.getShader("mousePicking");
        }

        public static void render(Scene scene) {
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            if (framebuffer.width != Renderer.windowWidth || framebuffer.height != Renderer.windowHeight) {
                framebuffer.resize(Renderer.windowWidth, Renderer.windowHeight);
            }

            shader.use();
            framebuffer.writeMode();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 1; i <= scene.renderers.Count; i++) {
                var loc = GL.GetUniformLocation(shader.id, "ObjectID");
                GL.Uniform1(loc, 1, ref i);

                scene.renderers[i-1].render(shader.id);
            }
        }

        public static IRenderer select(Scene scene, ivec2 coord) {
            render(scene);

            var p = read(coord.x, framebuffer.height - coord.y, 1, 1)[0,0] - 1;
            return p < 0 ? null : scene.renderers[p];
        }

        public static List<IRenderer> select(Scene scene, ivec2 fromCoord, ivec2 toCoord) {

            throw new System.NotImplementedException();

            render(scene);

            //fromCoord.y = framebuffer.height - fromCoord.y;
            //toCoord.y = framebuffer.height - toCoord.y;


            var size = math.abs(fromCoord - toCoord);
            var ps = read(fromCoord.x, fromCoord.y, size.x, size.y);

            var res = new List<IRenderer>();

            foreach(int i in ps) if (i >= 0 && !res.Contains(scene.renderers[i])) res.Add(scene.renderers[i]);
            return res;
        }
        

        static int[,] read(int x, int y, int w, int h) {
            int[,] pixels = new int[w, h];
            GL.ReadPixels<int>(x, y, w, h, PixelFormat.RedInteger, PixelType.Int, pixels);
            return pixels;
        }

    }
}
