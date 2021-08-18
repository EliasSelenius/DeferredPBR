
using System;
using Nums;

namespace Engine.Gui {
    public partial class Canvas {


        public void checkbox(vec2 pos, ref bool value, float size = 10) {
            var mpos = Mouse.position;
            float borderThiccnes = 2;
            rectborder(pos, size, borderThiccnes, in color.gray);
            if (value) {
                rect(pos + borderThiccnes, size - borderThiccnes*2, in color.white);
            }

            if (Utils.insideBounds(mpos - pos, size) && Mouse.isPressed(MouseButton.left)) {
                value = !value;
            }
        }

        public void slider(vec2 pos, ref float value) => throw new NotImplementedException();
        public void slider(vec2 pos, ref int value) => throw new NotImplementedException();

        public void number(vec2 pos, ref float value) => throw new NotImplementedException();
        public void number(vec2 pos, ref int value) => throw new NotImplementedException();
    }
}