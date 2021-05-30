using OpenTK.Graphics.OpenGL4;

namespace Engine {
    public class Texture2D {
        public readonly int id;

        public int width { get; private set; }
        public int height { get; private set; }
        public color[,] pixels { get; private set; }
        public PixelInternalFormat internalFormat = PixelInternalFormat.Rgba8;
        public readonly WrapMode wrapMode = WrapMode.Repeat;
        public readonly Filter filter = Filter.Linear;
        public bool genMipmap = true;

        public Texture2D(WrapMode wmode, Filter f, int w, int h) : this(wmode, f, new color[w,h]) { }
        public Texture2D(WrapMode wmode, Filter f, color[,] pixels) {
            (width, height) = (pixels.GetLength(0), pixels.GetLength(1));         
            this.pixels = pixels;
            wrapMode = wmode;
            filter = f;
            id = GLUtils.createTexture2D(this.pixels, internalFormat, wrapMode, filter, genMipmap);
        }

        public void bind(TextureUnit unit) => GLUtils.bindTex2D(unit, id);

        public void resize(int w, int h) {
            (width, height) = (w, h);
            pixels = new color[width, height];
        }

        public static Texture2D fromFile(string filename) {
            return new Texture2D(WrapMode.Repeat, Filter.Nearest, Utils.bitmapToColorArray(new System.Drawing.Bitmap(System.Drawing.Image.FromFile(filename))));
        }

        

        public void applyChanges() {
            GL.BindTexture(TextureTarget.Texture2D, id);
            GLUtils.texImage2D(internalFormat, pixels);
            if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}