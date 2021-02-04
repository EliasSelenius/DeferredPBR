using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Engine {
    public class Framebuffer {
        private List<(int, PixelInternalFormat)> textureAttachments = new List<(int, PixelInternalFormat)>();
        private List<(int, FramebufferAttachment, RenderbufferStorage)> renderbufferAttachments = new List<(int, FramebufferAttachment, RenderbufferStorage)>();

        private int id;

        public int width { get; private set; }
        public int height { get; private set; }

        public Framebuffer(int w, int h, (FramebufferAttachment, RenderbufferStorage)[] rbos, PixelInternalFormat[] texs) {
            (width, height) = (w, h);
            id = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);

            for (int i = 0; i < rbos.Length; i++) {
                int r = GLUtils.createRenderbuffer(rbos[i].Item2, width, height);
                renderbufferAttachments.Add((r, rbos[i].Item1, rbos[i].Item2));
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, rbos[i].Item1, RenderbufferTarget.Renderbuffer, r);
            }

            for (int i = 0; i < texs.Length; i++) {
                int t = GLUtils.createTexture2D(width, height, texs[i], WrapMode.ClampToEdge, Filter.Nearest, false);
                textureAttachments.Add((t, texs[i]));
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, TextureTarget.Texture2D, t, 0);
            }
            GL.DrawBuffers(texs.Length, texs.Select((x, i) => DrawBuffersEnum.ColorAttachment0 + i).ToArray());

            var code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete) throw new System.Exception("Framebuffer error code: " + code.ToString());

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        
        public void writeMode() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
        }
        public void readMode() {
            for (int i = 0; i < textureAttachments.Count; i++) {
                GLUtils.bindTex2D(TextureUnit.Texture0 + i, textureAttachments[i].Item1);
            }
        }
    
        public void resize(int w, int h) {
            if (w < 1) throw new System.ArgumentOutOfRangeException(nameof(w));
            if (h < 1) throw new System.ArgumentOutOfRangeException(nameof(h));

            (width, height) = (w, h);
            
            for (int i = 0; i < textureAttachments.Count; i++) {
                GL.BindTexture(TextureTarget.Texture2D, textureAttachments[i].Item1);
                GLUtils.applyTextureData(textureAttachments[i].Item2, width, height);
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);

            for (int i = 0; i < renderbufferAttachments.Count; i++) { 
                GLUtils.reinitRenderbuffer(renderbufferAttachments[i].Item1, renderbufferAttachments[i].Item3, width, height);
            }
        }

        public void delete() {
            if (id == 0) return;

            for (int i = 0; i < textureAttachments.Count; i++) GL.DeleteTexture(textureAttachments[i].Item1);
            for (int i = 0; i < renderbufferAttachments.Count; i++) GL.DeleteRenderbuffer(renderbufferAttachments[i].Item1);
            
            GL.DeleteFramebuffer(id);

            textureAttachments.Clear();
            renderbufferAttachments.Clear();

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

    }
}
