using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System;
using System.Linq;

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

            shader = Assets.getShader("ScreenRaycast");
        }

        static void convertCoord(ref ivec2 coord) => coord.y = framebuffer.height - coord.y;

        static List<IRenderer> renderers;
        public static void render(List<IRenderer> rens) {
            renderers = rens;

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


#region callbacks API
        public class rayhitdata {
            public readonly IRenderer renderer;
            public readonly int primitiveID;
            public readonly vec3 normal, position;

            public rayhitdata(IRenderer r, int primID, vec3 pos, vec3 norm) {
                renderer = r;
                primitiveID = primID;
                position = pos;
                normal = norm;
            }
            
        }
    
        public delegate void hitCallback(rayhitdata data);
        static Stack<hitCallback> callbacks = new();

        public static void onHit(hitCallback callback) => callbacks.Push(callback);

        static rayhitdata getHitdata(ivec2 coord) {
            convertCoord(ref coord);

            var i = readObjID(coord) - 1;
            
            if (i < 0) return null;
            
            return new(
                renderers[i],
                readPrimitiveID(coord),
                readPosition(coord),
                readNormal(coord)
            );
        }

        public static Func<IRenderer, bool> filter;
        internal static void dispatchCallbacks() {
            if (callbacks.Count == 0) return;

            if (filter != null) render(Scene.active.renderers.Where(filter).ToList());
            else render(Scene.active.renderers);

            var data = getHitdata((ivec2)Mouse.position);
            if (data == null) callbacks.Clear();

            while (callbacks.TryPop(out hitCallback callback)) callback(data);
        }
#endregion

        public static IRenderer select(Scene scene, ivec2 coord) {
            render(scene.renderers);
            convertCoord(ref coord);
            var p = readObjID(coord) - 1;
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
            var ps = readObjIDs(min, size);

            var res = new List<IRenderer>();
            foreach(int i in ps) if (i-1 >= 0 && !res.Contains(scene.renderers[i-1])) res.Add(scene.renderers[i-1]);
            return res;
        }
        

        static int readObjID(ivec2 coord) => readObjIDs(coord, ivec2.one)[0,0];
        static int[,] readObjIDs(ivec2 coord, ivec2 size) {
            return read<int>(
                ReadBufferMode.ColorAttachment0,
                PixelFormat.RedInteger,
                PixelType.Int,
                coord,
                size
            );
        }
        
        static int readPrimitiveID(ivec2 coord) => readPrimitiveIDs(coord, ivec2.one)[0,0];
        static int[,] readPrimitiveIDs(ivec2 coord, ivec2 size) {
            return read<int>(
                ReadBufferMode.ColorAttachment1,
                PixelFormat.RedInteger,
                PixelType.Int,
                coord,
                size
            );
        }

        static vec3 readNormal(ivec2 coord) => readNormals(coord, ivec2.one)[0,0];
        static vec3[,] readNormals(ivec2 coord, ivec2 size) {
            return read<vec3>(
                ReadBufferMode.ColorAttachment2,
                PixelFormat.Rgb,
                PixelType.Float,
                coord,
                size
            );
        }

        static vec3 readPosition(ivec2 coord) => readPositions(coord, ivec2.one)[0,0];
        static vec3[,] readPositions(ivec2 coord, ivec2 size) {
            return read<vec3>(
                ReadBufferMode.ColorAttachment3,
                PixelFormat.Rgb,
                PixelType.Float,
                coord,
                size
            );
        }

        static T[,] read<T>(ReadBufferMode rb, PixelFormat format, PixelType type, ivec2 coord, ivec2 size) where T : unmanaged {
            GL.ReadBuffer(rb);
            T[,] pixels = new T[size.x, size.y];
            GL.ReadPixels<T>(coord.x, coord.y, size.x, size.y, format, type, pixels);
            return pixels;
        }

    }
}
