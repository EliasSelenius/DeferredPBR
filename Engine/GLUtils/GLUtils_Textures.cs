using OpenTK.Graphics.OpenGL4;


namespace Engine {
    /*
    createTexture:
                genTexture()
                bindTexture()
                TexParameter() ...
                TexImage2D(format, width, height, data)
                GenerateMipmap()
                unbind()
    */
    public static partial class GLUtils {
        
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
    }
}