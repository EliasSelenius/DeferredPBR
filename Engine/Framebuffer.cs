using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Engine {

    public enum FramebufferFormat {
        rgba8,
        rgb8,

        rgba16f,
        rgb16f,
        
        int32
    }

    interface IRendertarget {
        void bind();
        void resize(int w, int h);
        int width { get; }
        int height { get; }
    }


    public class Framebuffer {
        // TODO: maybe make thees readonly lists or something

        //public readonly struct attachment { public readonly int id; public readonly FramebufferFormat format; }
        public readonly (int id, FramebufferFormat format)[] textureAttachments;
        public readonly (int id, FramebufferAttachment attach, RenderbufferStorage storage)[] renderbufferAttachments;

        private int id;

        public int width { get; private set; }
        public int height { get; private set; }


        static (PixelInternalFormat internalFormat, PixelFormat format, PixelType type) getGLformat(FramebufferFormat format) => format switch {
            FramebufferFormat.rgba8 => (PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Byte),
            FramebufferFormat.rgb8 => (PixelInternalFormat.Rgb8, PixelFormat.Rgb, PixelType.Byte),
            FramebufferFormat.rgba16f => (PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float), 
            FramebufferFormat.rgb16f => (PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float),
            FramebufferFormat.int32 => (PixelInternalFormat.R32i, PixelFormat.RedInteger, PixelType.Int),
            _ => throw new System.Exception("Framebuffer format not supported")
        };

        public Framebuffer(int w, int h, (FramebufferAttachment, RenderbufferStorage)[] rbos, FramebufferFormat[] texs) {
            (width, height) = (w, h);
            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            if (rbos != null) {
                renderbufferAttachments = new (int id, FramebufferAttachment attach, RenderbufferStorage storage)[rbos.Length];
                for (int i = 0; i < rbos.Length; i++) {
                    int r = GLUtils.createRenderbuffer(rbos[i].Item2, width, height);
                    renderbufferAttachments[i] = (r, rbos[i].Item1, rbos[i].Item2);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, rbos[i].Item1, RenderbufferTarget.Renderbuffer, r);
                }
            }

            textureAttachments = initDrawBuffers(texs);

            var code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete) throw new System.Exception("Framebuffer error code: " + code.ToString());

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        (int id, FramebufferFormat format)[] initDrawBuffers(FramebufferFormat[] texs) {
            var res = new (int id, FramebufferFormat format)[texs.Length]; 
            for (int i = 0; i < texs.Length; i++) {
                var f = getGLformat(texs[i]);

                int t = GLUtils.createTexture2D(width, height, f.internalFormat, f.format, f.type, WrapMode.ClampToEdge, Filter.Nearest, false);
                res[i] = (t, texs[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, t, 0);
            }
            GL.DrawBuffers(texs.Length, texs.Select((x, i) => DrawBuffersEnum.ColorAttachment0 + i).ToArray());
            return res;
        }
        
        public void bind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }
        public void readMode() {
            for (int i = 0; i < textureAttachments.Length; i++) {
                GLUtils.bindTex2D(TextureUnit.Texture0 + i, textureAttachments[i].id);
            }
        }
    
        public void resize(int w, int h) {
            if (w < 1) throw new System.ArgumentOutOfRangeException(nameof(w));
            if (h < 1) throw new System.ArgumentOutOfRangeException(nameof(h));

            (width, height) = (w, h);
            
            for (int i = 0; i < textureAttachments.Length; i++) {
                GL.BindTexture(TextureTarget.Texture2D, textureAttachments[i].id);
                var f = getGLformat(textureAttachments[i].format);
                GLUtils.texImage2D(f.internalFormat, f.format, f.type, width, height);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);

            int rbs = renderbufferAttachments?.Length ?? 0;
            for (int i = 0; i < 0; i++) { 
                GLUtils.reinitRenderbuffer(renderbufferAttachments[i].id, renderbufferAttachments[i].storage, width, height);
            }
        }

        public void delete() {
            if (id == 0) return;

            for (int i = 0; i < textureAttachments.Length; i++) {
                GL.DeleteTexture(textureAttachments[i].id);
                textureAttachments[i].id = 0;
            }
            int rbs = renderbufferAttachments?.Length ?? 0;
            for (int i = 0; i < rbs; i++) {
                GL.DeleteRenderbuffer(renderbufferAttachments[i].id);
                renderbufferAttachments[i].id = 0;
            }
            
            GL.DeleteFramebuffer(id);


            id = 0;
        }

        ~Framebuffer() {
            if (id != 0) throw new System.Exception("Memory leak detected: Framebuffer was not deleted before garbage collection.");
        }

        public void blit(Framebuffer target, ClearBufferMask mask, Filter filter) {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, id);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target.id);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, target.width, target.height, mask, (BlitFramebufferFilter)filter);
        }

        public void blit(int target, int targetWidth, int targetHeight, ClearBufferMask mask, Filter filter) {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, id);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, target);
            GL.BlitFramebuffer(0, 0, width, height, 0, 0, targetWidth, targetHeight, mask, (BlitFramebufferFilter)filter);
        }

        public static void read(int x, int y, int w, int h) {
            throw new System.NotImplementedException();
        }

    }
}
