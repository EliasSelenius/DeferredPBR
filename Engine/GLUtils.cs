
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Nums;
using System;

/*
    GL stuff:
        createTexture:
            genTexture()
            bindTexture()
            TexParameter() ...
            TexImage2D(format, width, height, data)
            GenerateMipmap()
            unbind()
        createFramebuffer:
            genFramebuffer()
            bindFramebuffer()
            FramebufferTexture2D(attachmentPoint, texture2D)
            FramebufferRenderbuffer(attachmentPoint, renderbuffer)
            DrawBuffers(ColorAttachments[])
            CheckFramebufferStatus()
            unbind()
        createRenderbuffer:
            genRenderbuffer()
            bindRenderbuffer()
            renderbufferStorage(format, width, height)
            unbind()

*/

namespace Engine {

    struct glBuffer {
        public readonly int id;

        
    }

    public static class GLUtils {

        static GLUtils() {
            texture_generator_fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture_generator_fbo);
            GL.DrawBuffers(1, new[] { DrawBuffersEnum.ColorAttachment0 } );
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            screenQuadVao = createVertexArray<posVertex>(createBuffer(new[] {
                new posVertex(-1, -1, 0),
                new posVertex(1, -1, 0),
                new posVertex(-1, 1, 0),
                new posVertex(1, 1, 0)
            }), createBuffer(new uint[] {
                0, 1, 2,
                2, 1, 3
            }));
        }

    #region GL DEBUG

        static DebugProc dbcallback;
        static void debug_callback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam) {
            
            var msg = "[" + type.ToString().Substring("DebugType".Length) + "]";
            
            var color = severity switch {
                DebugSeverity.DebugSeverityHigh => ConsoleColor.Red,
                DebugSeverity.DebugSeverityMedium => ConsoleColor.Yellow,
                DebugSeverity.DebugSeverityLow => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };
            var sev = severity switch {
                DebugSeverity.DebugSeverityNotification => "Notification",
                DebugSeverity.DebugSeverityHigh => "High",
                DebugSeverity.DebugSeverityMedium => "Medium",
                DebugSeverity.DebugSeverityLow => "Low",
                _ => "DontCare"
            };

            Console.ForegroundColor = color;
            msg += " severity: " + sev;
            Console.WriteLine(msg);

            var m = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(message);
            System.Console.WriteLine("    " + m);

            Console.ResetColor();
        }
        public static void enableDebug() {
            GL.Enable(EnableCap.DebugOutput);
            dbcallback = debug_callback;
            GL.DebugMessageCallback(dbcallback, System.IntPtr.Zero);
        }

        public static void assertNoError() {
            var error = GL.GetError();
            if (error != ErrorCode.NoError) throw new System.Exception("GLERROR: " + error.ToString());
        }
    #endregion

    #region buffers
        public static int createBuffer() => GL.GenBuffer();
        public static int createBuffer<T>(T[] data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct {
            int b = GL.GenBuffer();
            bufferdata(b, data, hint);
            return b;
        }
        public static int createBuffer<T>(ref T data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct {
            int b = GL.GenBuffer();
            bufferdata(b, ref data, hint);
            return b;
        }
        public static int createBuffer(int bytesize, BufferUsageHint hint = BufferUsageHint.StaticDraw) {
            int b = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, b);
            GL.BufferData(BufferTarget.ArrayBuffer, bytesize, System.IntPtr.Zero, hint);
            return b;
        }
        

        public static void bufferdata<T>(int buffer, T[] data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * System.Runtime.InteropServices.Marshal.SizeOf<T>(), data, hint);
        }
        public static void bufferdata<T>(int buffer, ref T data, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, System.Runtime.InteropServices.Marshal.SizeOf<T>(), ref data, hint );
        }

        public static void buffersubdata<T>(int buffer, int offset, ref T data) where T : struct {
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
            GL.BufferSubData(BufferTarget.ArrayBuffer, (System.IntPtr)offset, System.Runtime.InteropServices.Marshal.SizeOf<T>(), ref data);
        }

    #endregion

    #region VertexArray
        
        class Attrib {
            public readonly int index;
            public readonly int compSize;
            public readonly VertexAttribPointerType type;
            public readonly bool normalized;
            public readonly int stride;
            public readonly int offset;


            public Attrib(int i, System.Type fieldType, bool norm, int str, int ofs) {
                index = i;

                    if (fieldType == typeof(float)) (compSize, type) = (1, VertexAttribPointerType.Float);
                else if (fieldType == typeof(double)) (compSize, type) = (1, VertexAttribPointerType.Double);
                else if (fieldType == typeof(int)) (compSize, type) = (1, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec2)) (compSize, type) = (2, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec2)) (compSize, type) = (2, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec2)) (compSize, type) = (2, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec3)) (compSize, type) = (3, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec3)) (compSize, type) = (3, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec3)) (compSize, type) = (3, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec4)) (compSize, type) = (4, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec4)) (compSize, type) = (4, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec4)) (compSize, type) = (4, VertexAttribPointerType.Int);
                else throw new System.Exception("Type not supported");

                normalized = norm;
                stride = str;
                offset = ofs;

            }

            public void apply() {
                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(index, compSize, type, normalized, stride, offset);
            }

        }

        private static readonly Dictionary<System.Type, Attrib[]> attributes = new Dictionary<System.Type, Attrib[]>();

        private static void setupAttribPointers<VertType>() where VertType : struct {
            var type = typeof(VertType);
            if (!attributes.ContainsKey(type)) {
                var attribs = new List<Attrib>();
                int i = 0;
                int offset = 0;
                foreach (var field in type.GetFields()) {
                    var a = new Attrib(i, field.FieldType, false, System.Runtime.InteropServices.Marshal.SizeOf<VertType>(), offset);
                    attribs.Add(a);
                    offset += a.compSize * a.type switch {
                        VertexAttribPointerType.Float => sizeof(float),
                        VertexAttribPointerType.Double => sizeof(double),
                        VertexAttribPointerType.Int => sizeof(int),
                        _ => throw new System.Exception("invalid Type")
                    };
                    i++;
                }
                attributes.Add(type, attribs.ToArray());
            }


            foreach (var a in attributes[type]) {
                a.apply();
            }

        }


        public static int createVertexArray<VertType>(int vbo, int ebo) where VertType : struct {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            setupAttribPointers<VertType>();
            GL.BindVertexArray(0);
            return vao;
        }

        public static int createVertexArray<VertType>(int vbo) where VertType : struct {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            setupAttribPointers<VertType>();
            GL.BindVertexArray(0);
            return vao;
        }

    #endregion

    #region uniforms 

        public static void setUniformMatrix4(int loc, ref mat4 m) => GL.UniformMatrix4(loc, 1, false, ref m.row1.x);
        public static void setUniformMatrix4(int shader, string name, ref mat4 m) {
            var loc = GL.GetUniformLocation(shader, name);
            GL.UniformMatrix4(loc, 1, false, ref m.row1.x);
        }

    #endregion

    #region Framebuffer

    /*
    createFramebuffer:
                genFramebuffer()
                bindFramebuffer()
                FramebufferTexture2D(attachmentPoint, texture2D)
                FramebufferRenderbuffer(attachmentPoint, renderbuffer)
                DrawBuffers(ColorAttachments[])
                CheckFramebufferStatus()
                unbind()
    */

        //private static FramebufferAttachment getAttachmentType(RenderbufferStorage rbs) {}

        public static int createFramebuffer(int[] colorAttachments, int renderbuffer) {
            int f = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, f);
            
            // TODO: Framebuffer class. it will manage all attachments
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);

            var drawbuffers = new DrawBuffersEnum[colorAttachments.Length];
            for (int i = 0; i < colorAttachments.Length; i++) {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, colorAttachments[i], 0);
                drawbuffers[i] = DrawBuffersEnum.ColorAttachment0 + i;
            }
            GL.DrawBuffers(drawbuffers.Length, drawbuffers);

            var code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete) throw new System.Exception("Framebuffer error code: " + code.ToString());

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return f;
        }

    /*
            createRenderbuffer:
                genRenderbuffer()
                bindRenderbuffer()
                renderbufferStorage(format, width, height)
                unbind()
    */
        public static int createRenderbuffer(RenderbufferStorage internalformat, int width, int height) {
            int r = GL.GenRenderbuffer();
            reinitRenderbuffer(r, internalformat, width, height);
            return r;
        }
        public static void reinitRenderbuffer(int renderbuffer, RenderbufferStorage internalformat, int width, int height) {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalformat, width, height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }


    #endregion

    #region Textures 

    /*
    createTexture:
                genTexture()
                bindTexture()
                TexParameter() ...
                TexImage2D(format, width, height, data)
                GenerateMipmap()
                unbind()
    */

        public static void bindTex2D(TextureUnit unit, int t) {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, t);
        }

        public static void tex2DWrap(WrapMode mode) {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)mode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)mode);
        }
        public static void tex2DFilter(Filter filter) {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);
        }

        public static int createTexture2D(color[,] pixels, PixelInternalFormat internalFormat, WrapMode wrap, Filter filter, bool genMipmap) {
            int t = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, t);
            tex2DWrap(wrap);
            tex2DFilter(filter);
            texImage2D(internalFormat, pixels);
            if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return t;
        }
        public static int createTexture2D(int width, int height, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType, WrapMode wrap, Filter filter, bool genMipmap) {
            int t = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, t);
            tex2DWrap(wrap);
            tex2DFilter(filter);
            texImage2D(internalFormat, format, pixelType, width, height);
            if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return t;
        }

        public static void texImage2D(PixelInternalFormat internalformat, color[,] pixels) {
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, pixels.GetLength(0), pixels.GetLength(1), 0, PixelFormat.Rgba, PixelType.Float, pixels);
        }
        public static void texImage2D(PixelInternalFormat internalformat, PixelFormat format, PixelType pixelType, int width, int height) {
            GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, width, height, 0, format, pixelType, System.IntPtr.Zero);
        }
        public static void texImage2D(PixelInternalFormat internalformat, System.Drawing.Bitmap bitmap) {
            texImage2D(internalformat, Utils.bitmapToColorArray(bitmap));
        }

    #endregion

    #region Cubemap

        public static void bindCubemap(TextureUnit unit, int t) {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.TextureCubeMap, t);
        }

        public static int createCubemap(int width, int height, PixelInternalFormat internalFormat, WrapMode wrap, Filter filter) {
            int cubemap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap);
            for (int i = 0; i < 6; i++) {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, internalFormat, width, height, 0, PixelFormat.Rgb, PixelType.Float, System.IntPtr.Zero);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)wrap);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)filter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)filter);

            return cubemap;
        }

        public static int createCubemap(System.Drawing.Bitmap[] faces, PixelInternalFormat internalFormat, WrapMode wrap, Filter filter) {
            if (faces.Length != 6) throw new ArgumentException();

            (int width, int height) = (faces[0].Width, faces[0].Height);
            int cubemap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubemap);
            for (int i = 0; i < 6; i++) {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, internalFormat, width, height, 0, PixelFormat.Rgba, PixelType.Float, Utils.bitmapToColorArray(faces[i]));
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)wrap);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)filter);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)filter);

            return cubemap;
        }

    #endregion

    #region generators

        static int texture_generator_fbo, texture_generator_depthbuffer;

        public static int generateTexture2D(Shader shader, int width, int height, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType, WrapMode wrap, Filter filter, bool genMipmap) {
            int texture = createTexture2D(width, height, internalFormat, format, pixelType, wrap, filter, genMipmap);
            shader.use();
            GL.Viewport(0, 0, width, height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture_generator_fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            renderScreenQuad();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // HACK: set viewport back to what it probably was:
            GL.Viewport(0, 0, Renderer.windowWidth, Renderer.windowHeight);

            return texture;
        }

        public static int generateCubemap(Shader shader, int width, int height, PixelInternalFormat internalFormat, WrapMode wrap, Filter filter) {
            int cubemap = createCubemap(width, height, internalFormat, wrap, filter);
            shader.use();
            GL.Viewport(0, 0, width, height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, texture_generator_fbo);
            for (int i = 0; i < 6; i++) {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.TextureCubeMapPositiveX + i, cubemap, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Uniform1(GL.GetUniformLocation(shader.id, "cubemapSide"), i);
                renderScreenQuad();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // HACK: set viewport back to what it probably was:
            GL.Viewport(0, 0, Renderer.windowWidth, Renderer.windowHeight);

            return cubemap;
        }

    #endregion

        static int screenQuadVao;
        public static void renderScreenQuad() {
            GL.BindVertexArray(screenQuadVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }

    }

    public enum Filter {
        Nearest = 9728,
        Linear = 9729
    }

    public enum WrapMode {
        Repeat = 10497,
        ClampToBorder = 33069,
        ClampToEdge = 33071,
        MirroredRepeat = 33648
    }
}

