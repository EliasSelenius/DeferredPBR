
using Nums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

using System.Reflection;

namespace Engine {
    
    enum Flow {
        left2right,
        right2left

    }

    /*enum Flow {
        block,
        inline
    }*/

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


}
