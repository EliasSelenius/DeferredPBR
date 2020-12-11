using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.IO;
using Nums;



static class Renderer {

    public static int windowWidth => app.window.Size.X;
    public static int windowHeight => app.window.Size.Y;
    public static float windowAspect => (float)windowWidth / windowHeight;

    public static double time;
    public static double deltaTime;
    public static double fps => 1f / deltaTime;

    public static Shader geomPass { get; private set; }
    public static Shader lightPass_dirlight { get; private set; }
    public static Shader lightPass_pointlight { get; private set; }
    public static Shader imagePass { get; private set; }
    public static Shader textShader { get; private set; }

    private static Framebuffer gBuffer;
    private static Framebuffer hdrBuffer;

    static int screenQuadVAO;

    private static UBO windowInfoUBO;


    public static void load() {

        GLUtils.enableDebug();

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.Blend);


        { // init framebuffers
            gBuffer = new Framebuffer(windowWidth, windowHeight, new[] {
                (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
            }, new[] {
                PixelInternalFormat.Rgba8, // albedo metallic
                PixelInternalFormat.Rgba16f, // normal roughness
                PixelInternalFormat.Rgb16f // fragpos
            });

            hdrBuffer = new Framebuffer(windowWidth, windowHeight, new (FramebufferAttachment, RenderbufferStorage)[] {}, new[] {
                PixelInternalFormat.Rgba16f
            });
        }

        screenQuadVAO = GLUtils.createVertexArray<posVertex>(GLUtils.createBuffer(new[] {
            new posVertex(-1, -1, 0),
            new posVertex(1, -1, 0),
            new posVertex(-1, 1, 0),
            new posVertex(1, 1, 0)
        }), GLUtils.createBuffer(new uint[] {
            0, 1, 2,
            2, 1, 3
        }));


        textShader = Assets.getShader("text");
        textShader.use();
        GL.Uniform1(GL.GetUniformLocation(textShader.id, "atlas"), 0);

        geomPass = Assets.getShader("geomPass");
        geomPass.use();
        GL.Uniform1(GL.GetUniformLocation(geomPass.id, "albedoMap"), 0);

        imagePass = Assets.getShader("imagePass");
        imagePass.use();
        GL.Uniform1(GL.GetUniformLocation(imagePass.id, "input"), 0);


        lightPass_dirlight = Assets.getShader("lightPass_dirlight");
        lightPass_pointlight = Assets.getShader("lightPass_pointlight");
        setupUniforms(lightPass_dirlight);
        setupUniforms(lightPass_pointlight);
        
        void setupUniforms(Shader shader) {
            shader.use();
            int amLoc = GL.GetUniformLocation(shader.id, "g_Albedo_Metallic");
            int nrLoc = GL.GetUniformLocation(shader.id, "g_Normal_Roughness");
            int fLoc = GL.GetUniformLocation(shader.id, "g_Fragpos");
            GL.Uniform1(amLoc, 0);
            GL.Uniform1(nrLoc, 1);
            GL.Uniform1(fLoc, 2);
        }


        windowInfoUBO = new UBO("Window", vec4.bytesize);
        lightPass_pointlight.bindUBO(windowInfoUBO);

        whiteTexture = new Texture2D(WrapMode.Repeat, Filter.Nearest, new[,] { {new color(1f) }});

    }

    public static Texture2D whiteTexture;

    public static void drawframe(FrameEventArgs e) {

        time += deltaTime = e.Time;

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

            hdrBuffer.writeMode();
            gBuffer.readMode();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //gBuffer.blit(0, windowWidth, windowHeight, ClearBufferMask.DepthBufferBit, Filter.Nearest);

            Scene.active.renderLights();
        }

        { // image pass
            imagePass.use();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            hdrBuffer.readMode();
            GL.BindVertexArray(screenQuadVAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
        }
        
        { // Gui pass
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            textShader.use();

            OpenTK.Mathematics.Matrix4.CreateOrthographic(windowWidth, windowHeight, 0, 10, out OpenTK.Mathematics.Matrix4 res);
            var p = res.toNums();
            Camera.updateProjection(ref p);
            whiteTexture.bind(TextureUnit.Texture0);

            Canvas.activeCanvas.render();
        }


        GL.Flush();
        app.window.SwapBuffers();

        GLUtils.assertNoError();

    }

    public static void windowResize(ResizeEventArgs e) {
        GL.Viewport(0,0, e.Width, e.Height);
        
        gBuffer.resize(e.Width, e.Height);
        hdrBuffer.resize(e.Width, e.Height);

        vec2 s = new vec2(e.Width, e.Height);
        GLUtils.buffersubdata(windowInfoUBO.id, 0, ref s);
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