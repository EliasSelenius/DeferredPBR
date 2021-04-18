using Nums;
using System.Collections.Generic;
using System;

namespace Engine.Gui {
    public class BoxModel {
        public vec2 pos, size;
        public float width => size.x;
        public float height => size.y;

        public color backgroundColor;

    }



    public class Document {

        List<BoxModel> elements = new();

        public void render(Canvas canvas) {

            vec2 pos = vec2.zero;

            foreach (var el in elements) {

            }

        }

    }
}