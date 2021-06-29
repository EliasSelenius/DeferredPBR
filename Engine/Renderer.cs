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
        public static Shader toscreen { get; private set; }
        public static Shader textShader { get; private set; }

        private static Framebuffer gBuffer;
        private static Framebuffer hdrBuffer;
        private static Framebuffer ldrBuffer;
        static int bluredTexture;

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


            //bluredTexture = GLUtils.createTexture2D(windowWidth/2, windowHeight/2, PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Byte, WrapMode.ClampToEdge, Filter.Nearest, false);


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

                ldrBuffer = new Framebuffer(windowWidth, windowHeight, rbos:null, new[] {
                    FramebufferFormat.rgba8, // color
                    FramebufferFormat.rgb8 // brightness
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
            GL.Uniform1(GL.GetUniformLocation(imagePass.id, "tex_input"), 0);

            toscreen = Assets.getShader("toscreen");
            toscreen.use();
            GL.Uniform1(GL.GetUniformLocation(toscreen.id, "colorInput"), 0);
            GL.Uniform1(GL.GetUniformLocation(toscreen.id, "brightnessInput"), 1);

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

        static ParticleSystem pSys = new();

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
                pSys.render();
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

                ldrBuffer.bind();
                imagePass.use();
                GL.Clear(ClearBufferMask.ColorBufferBit);
                hdrBuffer.readMode();
                GLUtils.renderScreenQuad();


                // render to screen:
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                // Effect.blur(ldrBuffer.textureAttachments[0].id, bluredTexture, ldrBuffer.width/2, ldrBuffer.height/2, SizedInternalFormat.Rgba8);
                toscreen.use();
                ldrBuffer.readMode();
                //GLUtils.bindTex2D(TextureUnit.Texture0, bluredTexture);
                GLUtils.renderScreenQuad();

            }
            
            ScreenRaycast.dispatchCallbacks();
            
            // TODO: ScreenRaycast.dispatchCallbacks() should hapen after scene.renderFrame(), 
            // but canvas.dispatchFrame() updates projection, so thats a problem. solve it by yeeting it away from canvas, (its not needed anyway)
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0); 
            scene.renderFrame();

            Toolset.Gizmo.dispatchFrame();

            
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
            ldrBuffer.resize(e.Width, e.Height);

            //GL.BindTexture(TextureTarget.Texture2D, bluredTexture);
            //GLUtils.texImage2D(PixelInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.Byte, e.Width/2, e.Height/2);
            //GL.BindTexture(TextureTarget.Texture2D, 0);


            // update window info ubo:
            vec2 s = new vec2(e.Width, e.Height);
            GLUtils.buffersubdata(windowInfoUBO.bufferId, 0, ref s);


        }


#region compute shader test

        internal static Shader computeShader;

        public static void startGameOfLife(Texture2D tex) {
            if (computeShader == null) {
                computeShader = new Shader("computeTest");
                computeShader.sources[ShaderType.ComputeShader] = Assets.shaderSources["Engine.data.shaders.compute.test1.glsl"];
                if (!computeShader.linkProgram())
                    System.Console.WriteLine(computeShader.getInfolog());
            }

            tex.internalFormat = PixelInternalFormat.R8;
            tex.applyChanges();

            onDrawFrame += () => {

                GL.BindImageTexture(0, tex.id, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R8);
                
                computeShader.use();
                GL.Uniform1(0, (float)Application.time);
                GL.DispatchCompute(tex.width, tex.height, 1);
            };
        }



#endregion
    }

    /*

    GBuffer:
    RT0    DiffuseColor.R   DiffuseColor.G   DiffuseColor.B   Metallic
    RT1    Normal.x         Normal.y         Normal.z         Roughness

    */



    /*

        geometry pass
            binds gBuffer
            render: lit materials

        light pass
            binds hdrBuffer
            render: lights

        forward pass
            render: unlit materials 

        image pass
            binds default
            render: screenquad

    */

    public static class Effect {

        static Shader blurShader;

        static Effect() {
            blurShader = new Shader("blur");
            blurShader.sources[ShaderType.ComputeShader] = Assets.shaderSources["Engine.data.shaders.compute.blur.glsl"];
            if (!blurShader.linkProgram()) System.Console.WriteLine(blurShader.getInfolog());
        }

        public static void blur(int srcImg, int destImg, int width, int height, SizedInternalFormat internalFormat) {
            //GL.BindImageTexture(0, srcImg, 0, false, 0, TextureAccess.ReadOnly, internalFormat);
            GLUtils.bindTex2D(TextureUnit.Texture0, srcImg);
            GL.BindImageTexture(1, destImg, 0, false, 0, TextureAccess.WriteOnly, internalFormat);
            blurShader.use();
            GL.DispatchCompute(width, height, 1);
        }
    }
}
