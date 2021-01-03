
using Nums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

using System.Reflection;

namespace Engine {

    public abstract class Canvas {

        public static Canvas activeCanvas;

        static readonly Dictionary<string, Textbox> cachedTextMeshes = new Dictionary<string, Textbox>();
        static Mesh<posUvVertex> rectMesh;
        static Canvas() {
            activeCanvas = new MenuCanvas();
            rectMesh = MeshFactory<posUvVertex>.genQuad();
        }


        protected vec2 canvasSize => new vec2(Renderer.windowWidth, Renderer.windowHeight);
        protected int canvasWidth => Renderer.windowWidth;
        protected int canvasHeight => Renderer.windowHeight;
        protected vec2 size => currentBox.size;
        protected float width => size.x;
        protected float height => size.y;

        
        BoxModel rootBox = new BoxModel { pos = vec2.zero, size = new vec2(Renderer.windowWidth, Renderer.windowHeight) };
        protected BoxModel currentBox => boxStack.TryPeek(out BoxModel b) ? b : rootBox;
        readonly Stack<BoxModel> boxStack = new Stack<BoxModel>(); 
        public class BoxModel {
            public vec2 pos;
            public vec2 size;
            public vec2 displacement;
            public float max_displacement_y;
        }

        #region start ... end 
        
        public void start(vec2 pos, vec2 size) {

            boxStack.Push(new BoxModel {
                pos = currentBox.pos + pos,
                size = size
            });
        }

        public void start(vec2 size) {
            var box = currentBox;


            if (box.displacement.x + size.x > box.size.x) {
                box.displacement.x = 0;
                box.displacement.y += box.max_displacement_y;
                box.max_displacement_y = 0;
            } 
            start(box.displacement, size);
            box.displacement.x += size.x;
            box.max_displacement_y = math.max(box.max_displacement_y, size.y);
        }

        public void fill(color c) {
            var b = currentBox;  
            rect(b.pos, b.size, c);
        }

        public void text(string text, int fontSize, color c) {
            var b = currentBox;
            this.text(text, b.pos, fontSize, c);
        }

        public void checkbox(vec2 size, ref bool state) {
            start(size);
            if (state) fill(color.white);
            else fill(color.gray);
            
            if (clicking()) state = !state;
            end();
        }

        public bool button(string t, vec2 size) {
            start(size);
            var res = clicking();
            if (res) fill(color.gray);
            else fill(color.white);
            text(t, (int)size.y + 1, color.black);
            end();
            return res;
        }

        public void displayMembers(object obj) {
            var type = obj.GetType();
            var members = type.FindMembers(MemberTypes.Field | MemberTypes.Property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null);
            
            var colors = new[] {color.silver, color.black};

            foreach (var member in members) {
                start((width, 18));
                if (member is FieldInfo f) {
                    text(member.Name + ": " + f.GetValue(obj), 18, color.white);
                } else if (member is PropertyInfo p) {
                    text(member.Name + ": " + p.GetValue(obj), 18, color.white);
                }
                end();
            }
        }

        public void end() {
            boxStack.Pop();
        }

        #endregion


        public void rect(vec2 pos, vec2 size, color color, vec2? pivot = null) {
            pos -= canvasSize / 2f;
            pos += (size * (pivot ?? vec2.one)) / 2f;
            pos *= (1, -1);
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref size.x);
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
            GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.red);
            Renderer.whiteTexture.bind(TextureUnit.Texture0);
            rectMesh.render();
        }
        public void text(Textbox tb, vec2 pos, int fontSize, color color) {
            
            pos -= canvasSize / 2f;
            pos *= (1, -1);

            var one = vec2.one * fontSize;
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref one.x);
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
            GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.red);
            tb.render();
        }

        public void text(string text, vec2 pos, int fontSize, color color) {
            if (!cachedTextMeshes.ContainsKey(text)) cachedTextMeshes.Add(text, new Textbox(text));
            this.text(cachedTextMeshes[text], pos, fontSize, color);
        }


        public bool clicking() {
            return hovering() && Mouse.isPressed(MouseButton.left);
        }

        public bool hovering() {
            var o = currentBox.pos;
            var c = o + size;
            return Mouse.position.x < c.x && Mouse.position.x > o.x && Mouse.position.y < c.y && Mouse.position.y > o.y && Mouse.state == MouseState.free;
        }

        
        public abstract void render();




    }


    /*
        ui elements:
            Basics:
            - buttons
            - sliders
            - checkboxes
            - dropdowns
            - textinput
            - images
            - icons
            - scrollbar
            - collapsing elements

            Advanced:
            - windows
                - tabs
                - docking
            - color picker
            - curve editor

    */



    public class GuiWindow : Canvas {

        bool isWindowSelected = false;
        vec2 window_pos = 100;
        vec2 mouse_offset;
        vec2 window_size;
        string title;

        bool render_background = true;
        int clickCount = 0;

        public GuiWindow(string title, vec2 s) {
            window_size = s;
            this.title = title;
        }

        public override void render() {
            start(window_pos, (window_size.x, 22));
            
            if (Mouse.isReleased(MouseButton.left)) isWindowSelected = false;
            if (hovering()) {
                if (Mouse.isPressed(MouseButton.left)) {
                    mouse_offset = Mouse.position - window_pos;
                    isWindowSelected = true;
                }
            } 
            if (isWindowSelected) {
                window_pos = Mouse.position - mouse_offset;
                fill(.8f);
            } else {
                fill(color.gray);
            }
            
            // window title:
            start((100, 22));
            fill(color.silver);
            text(title, 22, color.white);
            end();


            void t(string name) {
                start((width, 22));
                text(name, 22, color.white);
                end();
            }


            // window content
            start((0, height), window_size);
            var c = color.hex(0x004156AF);
            fill(c);
                displayMembers(Assets.getMaterial("default"));
            end();
            
            end();
        }

    }




    class MenuCanvas : Canvas {

        Textbox fps = new Textbox();
        Textbox mousePosText = new Textbox();

        List<GuiWindow> windows = new List<GuiWindow>();

        public MenuCanvas() {
            windows.Add(new GuiWindow("My Wind", (500, 200)));
            windows.Add(new GuiWindow("Window", (400, 400)));
            

        }

        public override void render() {

            var alt = app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt);        
            Mouse.state = alt ? MouseState.free : MouseState.disabled;
            
            fps.setText("Time: " + (Renderer.deltaTime * 1000.0).ToString("##.#") + "ms, fps: " + Renderer.fps.ToString());
            text(fps, 0, 16, color.white);
            mousePosText.setText(Mouse.position.ToString());
            text(mousePosText, (0, 16), 16, color.white);
            text("cursorgrab: " + app.window.CursorGrabbed, (0, 32), 16, color.white);
            

            foreach (var window in windows) {
                window.render();
            }        



        }



        
    }



    enum Flow {
        block,
        inline
    }

    class BoxTransform {

        public vec2 pixelSize = vec2.one;
        public float pixelWidth => pixelSize.x;
        public float pixelHeight => pixelSize.y;

        public float totalWidth => marginLeft + pixelWidth + marginRight;
        public float totalHeight => marginTop + pixelHeight + marginBottom;

        public vec4 margin;
        public float marginTop => margin.x;
        public float marginBottom => margin.y;
        public float marginLeft => margin.z;
        public float marginRight => margin.w;

        public vec4 padding;
        public float paddingTop => padding.x;
        public float paddingBottom => padding.y;
        public float paddingLeft => padding.z;
        public float paddingRight => padding.w;
        

    }


    public enum Origin {
        center,
        top,
        bottom,
        left,
        right,
        topLeft,
        topRight,
        bottomLeft,
        bottomRight
    }




    class SpatialHashGrid {
        private readonly int gridSize = 10;
        private readonly Dictionary<ivec3, List<int>> grid = new Dictionary<ivec3, List<int>>();

        private ivec3 getGridCoord(vec3 pos) => (ivec3)(pos / gridSize);

        public void set(vec3 pos, int i) {
            var gp = getGridCoord(pos);
            if (!grid.ContainsKey(gp)) grid.Add(gp, new List<int>());
            grid[gp].Add(i); 
        }

    }


    /*


    todo before we start:
        make project a dll and move current test code in to another exe
        scene/gameobject/component/prefab stuff
        collada stuff in assets



    spore/spaz spacegame idea:
        level 1:
            fly simple ship around in asteroid field, and have blasters go pew pew.
            prerequisits: figure out project structure
                - from scratch
                - from library (dll)
                - from engine (dll + exe)


        level 2:
            spore-like ship editor
            prerequisits: learn uv stuff in blender
            and create kitbash of ship parts
                - cockpit
                - engine
                - hull
                - wings
                - blaster
            
        level 3: 
            dock with station to create/modify/switch ship

        level 4:
            


    blocky spacegame idea:
        level 1:
            build ship out of blocks
        level 2:
            bake all blocks into one mesh
        level 3:
            place thrusters on ship, and have them thrust
    


    game design principles:
        - premature optimalization/abstraction
        - kiss (keep it simple stupid)
        - feature creep



    what to use Source Generators for:
        - Nums
        - void update() instead of protected override void update()
        - scripting language transpiler
        - generating a parser (Pgen)
        - integrate assets into assembly
        - reflection (inside Prefab) begone



    */
}
