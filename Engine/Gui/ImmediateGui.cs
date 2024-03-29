
using System;
using System.Collections.Generic;
using Nums;

namespace Engine.Gui {

    using static ImmediateGui;


    public static unsafe class ImmediateGui {

        public static bool drawDebugLines = true;

        struct box {
            public vec2 pos, size;
        }

        // TODO: maybe determine if cursor hovers in push? mi ne pensas ke la mousecursor moves during one frame.
        public static bool hover() => Utils.insideBounds(Mouse.position - cur_pos, cur_size);
        public static bool rightclick() => hover() && Mouse.isPressed(MouseButton.right);
        public static bool leftclick() => hover() && Mouse.isPressed(MouseButton.left);

        static Canvas canvas;
        static Stack<box> box_stack = new();
        static vec2 cur_pos => box_stack.Peek().pos;
        static vec2 cur_size => box_stack.Peek().size;

        public static void beginCanvas(Canvas c) => canvas = c;

        static Font cur_font = Font.arial;
        static int cur_font_size = 12;
        static color cur_color = color.white;

        public static void push(vec2 pos, vec2 size) {
            if (drawDebugLines) canvas.rectborder(pos, size, 1, in color.red);
            
            box_stack.Push(new box {
                pos = pos,
                size = size
            });
        }

        public static void pop() {
            box_stack.Pop();
        }

        public static void fill(in color c) => canvas.rect(cur_pos, cur_size, c);
        public static void border(float thickness, in color c) => canvas.rectborder(cur_pos, cur_size, thickness, c);
        
        public static void text(string text) => canvas.text(cur_pos, cur_font, cur_font_size, text, cur_color);

        public static bool button(string label, vec2 pos) {
            var w = Text.length(label, 0, label.Length, cur_font_size, cur_font);
            w += 10; // adds a little margin
            push(pos, (w, cur_font_size));
            text(label);

            var click = leftclick();
            if (click) {
                fill(color.black);
            } else {
                if (hover()) fill(color.gray);
                else fill(color.silver);
            }

            pop();

            return click;
        }

        public static void checkbox(ref bool value, vec2 pos, float size = 10) {
            push(pos, size);
            border(2, color.gray);
            if (value) fill(color.white);
            if (leftclick()) value = !value;
            pop();
        }

        static float* s;

        public static void slider(ref float value, vec2 pos, float width) {

            fixed (float* p = &value) {
                s = p;
            }
            
            push(pos, (width, 3));
            fill(color.gray);

            float a = pos.x,
                  b = pos.x + width;

            float value_x = a + (b-a) * value;

            //value = math.lerp(a, b, value) / (b - a);

            // -5 + 1.5f to center it. 
            push((value_x - 5, pos.y - 5 + 1.5f), 10);
            fill(color.white);
            pop();

            pop();
        }

        public static void number(vec2 pos, ref float value) => throw new NotImplementedException();
        public static void number(vec2 pos, ref int value) => throw new NotImplementedException();

#region dropdown

        static bool dropdown_is_open = false;
        static Type dropdown_enum = null;
        static vec2 dropdown_pos;

        public static void dropdown<T>(vec2 pos, ref T value) where T : Enum {
            var type = typeof(T);
            var options = type.GetEnumNames();

            
            for (int i = 0; i < options.Length; i++) {
                
            }
        }

#endregion

    }


    public partial class Canvas {

        static bool test_bool;
        static float test_slider;

        public void test() {
            beginCanvas(this);

            push(100, 300);

            checkbox(ref test_bool, (800, 100));

            if (test_bool) {
                fill(color.white);
            }

            slider(ref test_slider, (800, 130), width:70);
            if (button("Hello, World!", (800, 160))) {
                test_bool = !test_bool;
            }

            pop();
        }
    }
}