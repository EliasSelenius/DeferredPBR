using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.IO;
using Nums;

namespace Engine {
    public static class Renderer {

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

        private static UBO windowInfoUBO;

        public static Gui.View userInterfaceView;

        public static void load() {

            GLUtils.enableDebug();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);



            { // init framebuffers
                gBuffer = new Framebuffer(windowWidth, windowHeight, new[] {
                    (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
                }, new[] {
                    FramebufferFormat.rgba8, // albedo metallic
                    FramebufferFormat.rgba16f, // normal roughness
                    FramebufferFormat.rgb16f // fragpos
                });

                hdrBuffer = new Framebuffer(windowWidth, windowHeight, new (FramebufferAttachment, RenderbufferStorage)[] {
                    (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
                }, new[] {
                    FramebufferFormat.rgba16f
                });
            }


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

            Gui.WindowingSystem.test();

        }

        public static Texture2D whiteTexture;

        public static void drawframe(FrameEventArgs e) {

            time += deltaTime = e.Time;

            //System.Console.WriteLine("frame: " + e.Time);
            GL.ClearColor(0, 0, 0, 1);

            { // geom pass
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);


                geomPass.use();
                gBuffer.writeMode();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                Scene.active.renderGeometry();
                Mousepicking.hovering?.render(PBRMaterial.defaultMaterial);
                Mousepicking.selected?.render(PBRMaterial.redPlastic);
            }


            Mousepicking.render();


            { // light pass
                GL.Disable(EnableCap.DepthTest);
                //GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                GL.BlendEquation(BlendEquationMode.FuncAdd);

                // copy depthbuffer from the gbuffer to the hdr buffer
                gBuffer.blit(hdrBuffer, ClearBufferMask.DepthBufferBit, Filter.Nearest);

                hdrBuffer.writeMode();
                gBuffer.readMode();
                GL.Clear(ClearBufferMask.ColorBufferBit);

                Scene.active.renderLights();
                
                GL.Enable(EnableCap.DepthTest);
                Scene.active.skybox.render();
            }

            { // image pass
                imagePass.use();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                hdrBuffer.readMode();
                GLUtils.renderScreenQuad();
            }
            
            { // Gui pass
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                textShader.use();

                OpenTK.Mathematics.Matrix4.CreateOrthographic(windowWidth, windowHeight, 0, 10, out OpenTK.Mathematics.Matrix4 res);
                var p = res.toNums();
                Camera.updateProjection(ref p);
                whiteTexture.bind(TextureUnit.Texture0);

                userInterfaceView?.render();
            }


            GL.Flush();
            app.window.SwapBuffers();

            GLUtils.assertNoError();

        }

        public static void windowResize(ResizeEventArgs e) {
            
            System.Console.WriteLine("window resize: " + e.Width + " * " + e.Height);

            if (e.Width < 1 || e.Height < 1) return;

            GL.Viewport(0,0, e.Width, e.Height);
            
            // update framebuffers
            gBuffer.resize(e.Width, e.Height);
            hdrBuffer.resize(e.Width, e.Height);
            Mousepicking.resize(e.Width, e.Height);

            // update window info ubo:
            vec2 s = new vec2(e.Width, e.Height);
            GLUtils.buffersubdata(windowInfoUBO.id, 0, ref s);


        }
    }

    /*

    GBuffer:
    RT0    DiffuseColor.R   DiffuseColor.G   DiffuseColor.B   Metallic
    RT1    Normal.x         Normal.y         Normal.z         Roughness

    */
    
    public static class Mousepicking {
        static Framebuffer framebuffer;
        public static Shader shader;


        public static IRenderer selected;
        public static IRenderer hovering;

        static Mousepicking() {
            framebuffer = new Framebuffer(Renderer.windowWidth, Renderer.windowHeight, 
            new (FramebufferAttachment, RenderbufferStorage)[] {
                (FramebufferAttachment.DepthAttachment, RenderbufferStorage.DepthComponent)
            },
            
            new[] {
                FramebufferFormat.int32
            });

            shader = Assets.getShader("mousePicking");
        }

        public static void render() {
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            shader.use();
            framebuffer.writeMode();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 1; i <= Scene.active.renderers.Count; i++) {
                var loc = GL.GetUniformLocation(shader.id, "ObjectID");
                GL.Uniform1(loc, 1, ref i);
                Scene.active.renderers[i-1].renderId();
            }

            var mousePos = Mouse.position;
            var p = read((int)mousePos.x, framebuffer.height - (int)mousePos.y, 1, 1)[0,0]-1;
            
            if (p == -1) hovering = null;    
            else hovering = Scene.active.renderers[p];
            if (Mouse.isPressed(MouseButton.left)) selected = hovering;


        }
        

        static int[,] read(int x, int y, int w, int h) {
            int[,] pixels = new int[w, h];
            GL.ReadPixels<int>(x, y, w, h, PixelFormat.RedInteger, PixelType.Int, pixels);
            return pixels;
        }

        public static void resize(int w, int h) => framebuffer.resize(w, h);
    }
}
