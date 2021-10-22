using OpenTK.Graphics.OpenGL4;

namespace Engine {
    public class Texture2D {
        public readonly int id;

        public int width { get; private set; }
        public int height { get; private set; }

        public PixelInternalFormat internalFormat { get; init; } = PixelInternalFormat.Rgba8;
        public bool genMipmap { get; init; } = true;

        private WrapMode _wrapMode;
        public WrapMode wrapMode {
            get => _wrapMode;
            set { _glBind(id); GLUtils.tex2DWrap(_wrapMode = value); _glBind(0); }
        }

        private Filter _filter;
        public Filter filter {
            get => _filter;
            set { _glBind(id); GLUtils.tex2DFilter(_filter = value); _glBind(0); }
        }

        public Texture2D() {
            id = GL.GenTexture();
        }

        public Texture2D(color[,] pixels) : this() {
            setPixels(pixels);
        }

        static void _glBind(int tex) => GL.BindTexture(TextureTarget.Texture2D, tex);
        public void bind(TextureUnit unit) => GLUtils.bindTex2D(unit, id);

        public color[,] getPixels() => throw new System.NotImplementedException();
        public void setPixels(color[,] pixels) {
            (width, height) = (pixels.GetLength(0), pixels.GetLength(1));
            _glBind(id);
                GLUtils.texImage2D(internalFormat, pixels);
                if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            _glBind(0);
        }

        public void setPixels(System.Drawing.Bitmap bitmap) => setPixels(Utils.bitmapToColorArray(bitmap));

        public static Texture2D fromFile(string filename) {
            return new Texture2D(Utils.bitmapToColorArray(new System.Drawing.Bitmap(System.Drawing.Image.FromFile(filename))));
        }

    }
}