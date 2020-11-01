
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Nums;

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

static class GLUtils {

    public static void assertNoError() {
        var error = GL.GetError();
        if (error != ErrorCode.NoError) throw new System.Exception("GLERROR: " + error.ToString());
    }

#region buffers
    public static int createBuffer() => GL.GenBuffer();
    public static int createBuffer<T>(T[] data) where T : struct {
        GL.CreateBuffers(1, out int b);
        bufferdata(b, data);
        return b;
    }

    public static void bufferdata<T>(int buffer, T[] data) where T : struct {
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * System.Runtime.InteropServices.Marshal.SizeOf<T>(), data, BufferUsageHint.StaticDraw);
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
        GL.CreateVertexArrays(1, out int vao);
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        setupAttribPointers<VertType>();
        GL.BindVertexArray(0);
        return vao;
    }

#endregion

#region uniforms 

    public static void setUniformMatrix4(int loc, ref mat4 m) => GL.UniformMatrix4(loc, 1, false, ref m.row1.x);
    public static void setUniformMatrix4(string name, ref mat4 m) => GL.UniformMatrix4(GL.GetUniformLocation(Renderer.geomPass.id, name), 1, false, ref m.row1.x);

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
        applyTextureData(internalFormat, pixels);
        if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        return t;
    }
    public static int createTexture2D(int width, int height, PixelInternalFormat internalFormat, WrapMode wrap, Filter filter, bool genMipmap) {
        int t = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, t);
        tex2DWrap(wrap);
        tex2DFilter(filter);
        applyTextureData(internalFormat, width, height);
        if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        return t;
    }

    public static void applyTextureData(PixelInternalFormat internalformat, color[,] pixels) {
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, pixels.GetLength(0), pixels.GetLength(1), 0, PixelFormat.Rgba, PixelType.Float, pixels);
    }
    public static void applyTextureData(PixelInternalFormat internalformat, int width, int height) {
        GL.TexImage2D(TextureTarget.Texture2D, 0, internalformat, width, height, 0, PixelFormat.Rgba, PixelType.Float, System.IntPtr.Zero);
    }
    public static void applyTextureData(PixelInternalFormat internalformat, System.Drawing.Bitmap bitmap) {
        applyTextureData(internalformat, Utils.bitmapToColorArray(bitmap));
    }

#endregion

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
