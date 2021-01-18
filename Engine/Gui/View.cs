
using Nums;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Gui {

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

        /*public void displayMembers(object obj) {
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
        }*/

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
}