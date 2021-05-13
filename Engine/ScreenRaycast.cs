using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System;

namespace Engine {
    public static class ScreenRaycast {
        static Framebuffer framebuffer;
        public static Shader shader;


        static ScreenRaycast() {
            framebuffer = new Framebuffer(Renderer.windowWidth, Renderer.windowHeight, 
            new (FramebufferAttachment, RenderbufferStorage)[] {
                (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
            },
            
            new[] {
                FramebufferFormat.int32, // object ID
                FramebufferFormat.int32, // primitive ID
                FramebufferFormat.rgb16f, // normal
                FramebufferFormat.rgb16f // position
            });

            shader = Assets.getShader("mousePicking");
        }

        static void convertCoord(ref ivec2 coord) => coord.y = framebuffer.height - coord.y;

        public static void render(IEnumerable<IRenderer> renderers) {
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            if (framebuffer.width != Renderer.windowWidth || framebuffer.height != Renderer.windowHeight) {
                framebuffer.resize(Renderer.windowWidth, Renderer.windowHeight);
            }

            shader.use();
            framebuffer.bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var uniformloc = GL.GetUniformLocation(shader.id, "ObjectID");
            int i = 1;
            foreach (var r in renderers) {
                GL.Uniform1(uniformloc, 1, ref i);
                r.render(shader.id);
                i++;
            }
        }

        public static void get(ivec2 coord, List<IRenderer> renderers, out IRenderer renderer, out int primitiveID, out vec3 normal, out vec3 position) {
            render(renderers);
            convertCoord(ref coord);

            var i = readObjIDs(coord, 1 , 1)[0, 0] - 1;
            renderer = i < 0 ? null : renderers[i];
            
            primitiveID = readPrimitiveIDs(coord, 1, 1)[0, 0];

            normal = readNormals(coord, 1, 1)[0, 0];
            position = readPositions(coord, 1, 1)[0, 0];
        }



        public struct rayhitdata {
            public bool hit => renderer != null;
            public IRenderer renderer;
            public int primitiveID;
            public vec3 normal, position;
        }
        public delegate void requestCallback(in rayhitdata data);
        static readonly Dictionary<ivec2, Stack<requestCallback>> callbacks = new();
        static Stack<requestCallback> getCallbackStack(ivec2 coord) => callbacks.ContainsKey(coord) ? callbacks[coord] : callbacks[coord] = new();
        public static void request(ivec2 coord, requestCallback callback) => getCallbackStack(coord).Push(callback);

        internal static void dispatchCallbacks() {
            render(Scene.active.renderers);

            foreach (var kv in callbacks) {
                var i = readObjIDs(kv.Key, 1 , 1)[0, 0] - 1;
                var renderer = i < 0 ? null : Scene.active.renderers[i];
                
                while(kv.Value.TryPop(out requestCallback res)) {
                    //res(renderer);
                }
            }
            
        }

        public static IRenderer select(Scene scene, ivec2 coord) {
            render(scene.renderers);
            convertCoord(ref coord);
            var p = readObjIDs(coord, 1, 1)[0,0] - 1;
            return p < 0 ? null : scene.renderers[p];
        }

        public static List<IRenderer> select(Scene scene, ivec2 fromCoord, ivec2 toCoord) {
            render(scene.renderers);

            fromCoord.y = framebuffer.height - fromCoord.y;
            toCoord.y = framebuffer.height - toCoord.y;
            var min = math.min(fromCoord, toCoord);
            var max = math.max(fromCoord, toCoord);
            var size = max - min;
            size = math.max(size, ivec2.one);
            var ps = readObjIDs(min, size.x, size.y);

            var res = new List<IRenderer>();
            foreach(int i in ps) if (i-1 >= 0 && !res.Contains(scene.renderers[i-1])) res.Add(scene.renderers[i-1]);
            return res;
        }
        

        static int[,] readObjIDs(ivec2 coord, int w, int h) {
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            int[,] pixels = new int[w, h];
            GL.ReadPixels<int>(coord.x, coord.y, w, h, PixelFormat.RedInteger, PixelType.Int, pixels);
            return pixels;
        }

        static int[,] readPrimitiveIDs(ivec2 coord, int w, int h) {
            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            int[,] pixels = new int[w, h];
            GL.ReadPixels<int>(coord.x, coord.y, w, h, PixelFormat.RedInteger, PixelType.Int, pixels);
            return pixels;
        }

        static vec3[,] readNormals(ivec2 coord, int w, int h) {
            GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
            vec3[,] pixels = new vec3[w, h];
            GL.ReadPixels<vec3>(coord.x, coord.y, w, h, PixelFormat.Rgb, PixelType.Float, pixels);
            return pixels;
        }

        static vec3[,] readPositions(ivec2 coord, int w, int h) {
            GL.ReadBuffer(ReadBufferMode.ColorAttachment3);
            vec3[,] pixels = new vec3[w, h];
            GL.ReadPixels<vec3>(coord.x, coord.y, w, h, PixelFormat.Rgb, PixelType.Float, pixels);
            return pixels;
        }

    }
}