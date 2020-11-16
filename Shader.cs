
using OpenTK.Graphics.OpenGL4;

class Shader {
    public readonly int id;

    public Shader(string frag, string vert) {
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

    }

    public void use() => GL.UseProgram(id);

    public void bindUBO(UBO ubo) {
        int i = GL.GetUniformBlockIndex(id, ubo.name);
        GL.UniformBlockBinding(id, i, ubo.bindingPoint);
    }
}