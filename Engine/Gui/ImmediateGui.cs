
using System;
using System.Collections.Generic;
using Nums;

namespace Engine.Gui {

    using static ImmediateGui;


    public static class ImmediateGui {

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

        public static void push(vec2 pos, vec2 size) {
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
        

        public static void checkbox(ref bool value, vec2 pos, float size = 10) {
            push(pos, size);
            border(2, color.gray);
            if (value) fill(color.white);
            if (leftclick()) value = !value;
            pop();
        }

        public static void slider(ref float value, vec2 pos, float width) {
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

            pop();
        }

        /*public void checkbox(vec2 pos, ref bool value, float size = 10) {
            var mpos = Mouse.position;
            float borderThiccnes = 2;
            rectborder(pos, size, borderThiccnes, in color.gray);
            if (value) {
                rect(pos + borderThiccnes, size - borderThiccnes*2, in color.white);
            }

            if (Utils.insideBounds(mpos - pos, size) && Mouse.isPressed(MouseButton.left)) {
                value = !value;
            }
        }*/

#region dropdown

        bool dropdown_is_open = false;
        Type dropdown_enum = null;
        vec2 dropdown_pos;

        public void dropdown<T>(vec2 pos, ref T value) where T : Enum {
            var type = typeof(T);
            var options = type.GetEnumNames();

            
            for (int i = 0; i < options.Length; i++) {
                
            }
        }

#endregion

        //public void slider(vec2 pos, ref float value) => throw new NotImplementedException();
        //public void slider(vec2 pos, ref int value) => throw new NotImplementedException();

        public void number(vec2 pos, ref float value) => throw new NotImplementedException();
        public void number(vec2 pos, ref int value) => throw new NotImplementedException();
    }
}