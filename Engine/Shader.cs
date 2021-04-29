using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine {    
    public class Shader {
        public readonly string name;
        public readonly int id;

        public bool linked { get; private set; }

        readonly Dictionary<string, int> uniformBlocks = new();
        public readonly Dictionary<ShaderType, string> sources = new();

        public void use() => GL.UseProgram(id);
        public string getInfolog() => GL.GetProgramInfoLog(id);

        public Shader(string name, string frag, string vert) {
            this.name = name;
            id = GL.CreateProgram();

            sources.Add(ShaderType.FragmentShader, frag);
            sources.Add(ShaderType.VertexShader, vert);

            if (!linkProgram()) {
                System.Console.WriteLine(getInfolog());
            }

        }

        /// <summary>returns true if linking was successful</summary>
        public bool linkProgram() {
            int create(ShaderType type, string src) {
                int r = GL.CreateShader(type);
                GL.ShaderSource(r, src);
                GL.CompileShader(r);

                /*var info = GL.GetShaderInfoLog(r);
                if (!string.IsNullOrWhiteSpace(info)) System.Console.WriteLine(info);*/

                GL.AttachShader(id, r);
                return r;
            }
            void del(int r) {
                GL.DetachShader(id, r);
                GL.DeleteShader(r);
            }

            Span<int> s = stackalloc int[sources.Count];
            int i = 0;
            foreach (var source in sources) s[i++] = create(source.Key, source.Value);
            GL.LinkProgram(id);
            for (i = 0; i < s.Length; i++) del(s[i]);
            

            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int ls);
            this.linked = ls switch { 0 => false, 1 => true, _ => throw new Exception() };

            if (!this.linked) return false;
            
            // after successful linking:

            initUniformbuffers();

            return true;
        }

        void initUniformbuffers() {
            uniformBlocks.Clear();

            GL.GetProgram(id, GetProgramParameterName.ActiveUniformBlocks, out int count);
            GL.GetProgram(id, GetProgramParameterName.ActiveUniformBlockMaxNameLength, out int maxBufSize);
            
            for (int i = 0; i < count; i++) {
                GL.GetActiveUniformBlockName(id, i, maxBufSize, out int nameLength, out string name);
                
                int uboIndex = GL.GetUniformBlockIndex(id, name);    
                uniformBlocks[name] = uboIndex;

                var ubo = Uniformblock.require(name);
                GL.UniformBlockBinding(id, uboIndex, ubo.bindingPoint);
            }

        }
    }


    internal static class GLSL_Preprocessor {
        

        
        public static void process(ref string source) {

        }
    }

}

