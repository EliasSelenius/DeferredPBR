using Nums;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Gui {


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

    public struct rectTransform {
        public vec2 pos;
        public vec2 size;
        public vec2 displacement;
        public float max_displacement_y;
    }


    public abstract class View {

        static readonly Dictionary<string, Textbox> cachedTextMeshes = new Dictionary<string, Textbox>();
        static Mesh<posUvVertex> rectMesh;
        static View() {
            //activeCanvas = new MenuCanvas();
            rectMesh = new Mesh<posUvVertex>(MeshFactory<posUvVertex>.genQuad());
        }


        public View() {
            
        }


        protected vec2 canvasSize => new vec2(Renderer.windowWidth, Renderer.windowHeight);
        protected int canvasWidth => Renderer.windowWidth;
        protected int canvasHeight => Renderer.windowHeight;
        protected vec2 size => currentBox.size;
        protected float width => size.x;
        protected float height => size.y;

        
        rectTransform rootBox = new rectTransform { pos = vec2.zero, size = new vec2(Renderer.windowWidth, Renderer.windowHeight) };
        protected rectTransform currentBox => boxStack.TryPeek(out rectTransform b) ? b : rootBox;
        readonly Stack<rectTransform> boxStack = new Stack<rectTransform>(); 

        #region start ... end 
        
        public void start(vec2 pos, vec2 size) {

            boxStack.Push(new rectTransform {
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

        
        public void end() {
            boxStack.Pop();
        }

        public void fill(in color c) {
            var b = currentBox;  
            rect(b.pos, b.size, c);
        }

        #region text

        public void text(string text, int fontSize, in color c) {
            var b = currentBox;
            this.text(text, b.pos, fontSize, c);
        }
        public void text(Textbox tb, int fontSize, in color c) {
            var b = currentBox;
            this.text(tb, b.pos, fontSize, c);
        }
        public void text(string text, vec2 pos, int fontSize, in color color) {
            if (!cachedTextMeshes.ContainsKey(text)) cachedTextMeshes.Add(text, new Textbox(text));
            this.text(cachedTextMeshes[text], pos, fontSize, color);
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


        // TODO: there needs to be a mesh per font
        Mesh<posUvVertex> textmesh = new Mesh<posUvVertex>();

        public void newtext(string text, int fontSize) {
            var pos = currentBox.pos;

            pos -= canvasSize / 2f;
            pos *= (1, -1);

            Text.genText(text, pos, fontSize, Font.arial, textmesh.data);
        }

        #endregion


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



        public bool clicking() {
            return hovering() && Mouse.isPressed(MouseButton.left);
        }

        public bool hovering() {
            return Utils.insideBounds(Mouse.position - currentBox.pos, size) && Mouse.state == MouseState.free;
        }

        
        protected abstract void renderBody();

        internal void render() {

            textmesh.data.clear();

            renderBody();

            textmesh.updateBuffers();
            Font.arial.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            var one = vec2.one;
            var zero = vec2.zero;
            var c = color.white;
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref one.x);
            GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref zero.x);
            GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref c.red);
            textmesh.render();

        }




    } 
}