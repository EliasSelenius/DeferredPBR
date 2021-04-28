using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Engine {    
    public class Uniformblock {
        
        public int bufferId { get; private set; } = 0;

        public readonly int bindingPoint;
        public readonly string name;

        static int bindingPoint_count = 0;

        static Dictionary<string, Uniformblock> UBOs = new();
        public static Uniformblock get(string name) => UBOs[name];
        public static Uniformblock require(string name) {
            if (UBOs.ContainsKey(name)) return UBOs[name];
            else return UBOs[name] = new Uniformblock(name);
        }


        private Uniformblock(string name) {
            this.name = name;
            bindingPoint = bindingPoint_count++;
        }


        public void bindBuffer(int bufferId) {
            this.bufferId = bufferId;
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, this.bufferId);
        }
    }
}