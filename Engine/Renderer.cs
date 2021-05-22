using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using System.IO;
using Nums;

namespace Engine {
    public static class Renderer {

        public static int windowWidth => Application.window.Size.X;
        public static int windowHeight => Application.window.Size.Y;
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

        private static Uniformblock windowInfoUBO;
        private static Uniformblock cameraUBO;

        internal static Texture2D whiteTexture;


        internal static mat4 viewMatrix;
        internal static mat4 projectionMatrix;
        
        public static void updateCamera(ref mat4 view, ref mat4 projection) {
            updateCameraView(ref view);
            updateCameraProjection(ref projection);
            //GLUtils.buffersubdata(cameraUBO.id, 0, ref view);
            //GLUtils.buffersubdata(cameraUBO.id, mat4.bytesize, ref projection);
        }
        public static void updateCameraView(ref mat4 view) {
            viewMatrix = view;
            GLUtils.buffersubdata(cameraUBO.bufferId, 0, ref view);
        }
        public static void updateCameraProjection(ref mat4 projection) {
            projectionMatrix = projection;
            GLUtils.buffersubdata(cameraUBO.bufferId, mat4.bytesize, ref projection);
        }

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



            { // init UBOs
                windowInfoUBO = Uniformblock.get("Window");
                windowInfoUBO.bindBuffer(GLUtils.createBuffer(vec4.bytesize));

                cameraUBO = Uniformblock.get("Camera");
                cameraUBO.bindBuffer(GLUtils.createBuffer(2 * mat4.bytesize));
            }




            whiteTexture = new Texture2D(WrapMode.Repeat, Filter.Nearest, new[,] { {new color(1f) }});

        }

        public static event System.Action onDrawFrame;

        internal static void drawframe(FrameEventArgs e) {

            time += deltaTime = e.Time;

            var scene = Application.scene;

            //System.Console.WriteLine("frame: " + e.Time);
            GL.ClearColor(0, 0, 0, 1);

            { // geom pass
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);


                geomPass.use();
                gBuffer.bind();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                scene.updateCamera();
                scene.geometryPass();
            }


            { // light pass
                GL.Disable(EnableCap.DepthTest);
                //GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);
                GL.BlendEquation(BlendEquationMode.FuncAdd);

                // copy depthbuffer from the gbuffer to the hdr buffer
                gBuffer.blit(hdrBuffer, ClearBufferMask.DepthBufferBit, Filter.Nearest);

                hdrBuffer.bind();
                GL.Clear(ClearBufferMask.ColorBufferBit);

                gBuffer.readMode();

                scene.lightPass();
            }


            { // forward pass
                GL.Enable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);

                scene.forwardPass();
            }


            { // image pass
                GL.Disable(EnableCap.DepthTest);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                imagePass.use();
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                hdrBuffer.readMode();
                GLUtils.renderScreenQuad();

            }
            
            ScreenRaycast.dispatchCallbacks();
            
            // TODO: ScreenRaycast.dispatchCallbacks() should hapen after scene.renderFrame(), 
            // but canvas.dispatchFrame() updates projection, so thats a problem. solve it by yeeting it away from canvas, (its not needed anyway)
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); 
            scene.renderFrame();

            onDrawFrame?.Invoke();

            GL.Flush();
            Application.window.SwapBuffers();

            GLUtils.assertNoError();

        }

        public static void windowResize(ResizeEventArgs e) {
            

            if (e.Width < 1 || e.Height < 1) return;
            
            System.Console.WriteLine("window resize: " + e.Width + " * " + e.Height);


            GL.Viewport(0,0, e.Width, e.Height);
            
            // update framebuffers
            gBuffer.resize(e.Width, e.Height);
            hdrBuffer.resize(e.Width, e.Height);

            // update window info ubo:
            vec2 s = new vec2(e.Width, e.Height);
            GLUtils.buffersubdata(windowInfoUBO.bufferId, 0, ref s);


        }
    }

    /*

    GBuffer:
    RT0    DiffuseColor.R   DiffuseColor.G   DiffuseColor.B   Metallic
    RT1    Normal.x         Normal.y         Normal.z         Roughness

    */
}
