using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Engine {
    public class Shader {
        public readonly string name;
        public readonly int id;

        readonly Dictionary<string, int> uniformBlocks = new();


        public void use() => GL.UseProgram(id);


        public Shader(string name, string frag, string vert) {
            this.name = name;
            id = GL.CreateProgram();

            int createShader(ShaderType type, string src) {
                int r = GL.CreateShader(type);
                GL.ShaderSource(r, src);
                GL.CompileShader(r);
                var info = GL.GetShaderInfoLog(r);
                if (!string.IsNullOrWhiteSpace(info)) System.Console.WriteLine(info);
                GL.AttachShader(id, r);
                return r;
            }
            void delShader(int r) {
                GL.DetachShader(id, r);
                GL.DeleteShader(r);
            }

            int f = createShader(ShaderType.FragmentShader, frag);
            int v = createShader(ShaderType.VertexShader, vert);
            GL.LinkProgram(id);
            var info = GL.GetProgramInfoLog(id);
            if (!string.IsNullOrWhiteSpace(info)) {
                System.Console.WriteLine(info);
            }

            delShader(f); delShader(v);

            initUniformbuffers();

        }

        void initUniformbuffers() {
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

