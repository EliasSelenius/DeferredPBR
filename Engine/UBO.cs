using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Engine {    
    public class UBO {
        
        public readonly int id;
        public readonly int bindingPoint;
        public readonly string name;

        static int bindingPoint_count = 0;

        static Dictionary<string, UBO> UBOs = new();
        public static UBO get(string name) => UBOs[name];

        public UBO(string name, int bytesize) {
            this.name = name;
            id = GLUtils.createBuffer(bytesize);
            bindingPoint = bindingPoint_count++;

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, id);
        }

    }
}