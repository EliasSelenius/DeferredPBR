using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.IO;
using Nums;

struct posVertex {
    public vec3 position;
    public posVertex(float x, float y, float z) => position = (x, y, z);
}

static class Renderer {

    public static int windowWidth => app.window.Size.X;
    public static int windowHeight => app.window.Size.Y;

    public static Shader geomPass { get; private set; }
    public static Shader lightPass { get; private set; }

    //private static GBuffer gBuffer;
    private static Framebuffer gBuffer;

    public static int dirlightVAO;
    public static int pointlightVAO;

    public static void load() {

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);

        //gBuffer = new GBuffer();
        gBuffer = new Framebuffer(windowWidth, windowHeight, new[] {
            (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
        }, new[] {
            PixelInternalFormat.Rgba8,
            PixelInternalFormat.Rgba16f,
            PixelInternalFormat.Rgb16f
        });

        geomPass = Assets.getShader("geomPass");
        geomPass.use();
        GL.Uniform1(GL.GetUniformLocation(geomPass.id, "albedoMap"), 0);

        lightPass = Assets.getShader("lightPass");
        lightPass.use();
        int amLoc = GL.GetUniformLocation(lightPass.id, "g_Albedo_Metallic");
        int nrLoc = GL.GetUniformLocation(lightPass.id, "g_Normal_Roughness");
        int fLoc = GL.GetUniformLocation(lightPass.id, "g_Fragpos");
        GL.Uniform1(amLoc, 0);
        GL.Uniform1(nrLoc, 1);
        GL.Uniform1(fLoc, 2);
        

        {
            dirlightVAO = GLUtils.createVertexArray<posVertex>(GLUtils.createBuffer(new[] {
                new posVertex(-1, -1, 0),
                new posVertex(1, -1, 0),
                new posVertex(-1, 1, 0),
                new posVertex(1, 1, 0)
            }), GLUtils.createBuffer(new uint[] {
                0, 1, 2,
                2, 1, 3
            }));

            
        }

        whiteTexture = new Texture2D(WrapMode.Repeat, Filter.Nearest, new[,] { {new color(1f) }});


        //shader = new Shader(File.ReadAllText("data/shaders/frag.glsl"), File.ReadAllText("data/shaders/vert.glsl"));
    }

    public static Texture2D whiteTexture;

    public static void drawframe(FrameEventArgs e) {
        //System.Console.WriteLine("frame: " + e.Time);
        GL.ClearColor(0, 0, 0, 1);

        { // geom pass
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            geomPass.use();
            gBuffer.writeMode();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            Scene.active.renderGeometry();
        }

        { // light pass
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
            GL.BlendEquation(BlendEquationMode.FuncAdd);

            lightPass.use();
            GLUtils.setUniformMatrix4(GL.GetUniformLocation(lightPass.id, "view"), ref Scene.active.camera.viewMatrix);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gBuffer.readMode();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //gBuffer.blit(0, windowWidth, windowHeight, ClearBufferMask.DepthBufferBit, Filter.Nearest);

            Scene.active.renderLights();
        }
        

        GL.Flush();
        app.window.SwapBuffers();

        GLUtils.assertNoError();

    }

    public static void windowResize(ResizeEventArgs e) {
        GL.Viewport(0,0, e.Width, e.Height);
        gBuffer.resize(e.Width, e.Height);
    }
}
/*

RT0    DiffuseColor.R   DiffuseColor.G   DiffuseColor.B   Metallic
RT1    Normal.x         Normal.y         Normal.z         Roughness

*/
class GBuffer {
    int fbo, rt0, rt1, rbo;

    int width, height;

    public GBuffer() {

        (width, height) = (app.window.Size.X,app.window.Size.Y);

        fbo = GLUtils.createFramebuffer(new[] { 
            rt0 = GLUtils.createTexture2D(width, height, PixelInternalFormat.Rgba8, WrapMode.ClampToEdge, Filter.Nearest, false),
            rt1 = GLUtils.createTexture2D(width, height, PixelInternalFormat.Rgba16f, WrapMode.ClampToEdge, Filter.Nearest, false)
        }, rbo = GLUtils.createRenderbuffer(RenderbufferStorage.DepthComponent, width, height));

        
    }

    public void writeMode() {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        //GL.Viewport(0,0, app.window.Size.X / w,h);
    }
    public void readMode() {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GLUtils.bindTex2D(TextureUnit.Texture0, rt0);
        GLUtils.bindTex2D(TextureUnit.Texture1, rt1);
    }

    public void resize(int w, int h) {
        (width, height) = (w, h);

        GLUtils.reinitRenderbuffer(rbo, RenderbufferStorage.DepthComponent, width, height);
        GL.BindTexture(TextureTarget.Texture2D, rt0);
        GLUtils.applyTextureData(PixelInternalFormat.Rgba8, width, height);
        GL.BindTexture(TextureTarget.Texture2D, rt1);
        GLUtils.applyTextureData(PixelInternalFormat.Rgba16f, width, height);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void delete() {
        
        if (fbo == 0) return;

        GL.DeleteFramebuffer(fbo);
        GL.DeleteTexture(rt0);
        GL.DeleteTexture(rt1);
        GL.DeleteRenderbuffer(rbo);

        fbo = 0;
    }

    ~GBuffer() {
        if (fbo != 0) System.Console.WriteLine("Memory leak detected! g-Buffer: " + fbo);
    }
}