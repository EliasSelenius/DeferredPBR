
using Nums;
using OpenTK.Graphics.OpenGL4;

class PBRMaterial {
    public vec3 albedo = vec3.one;
    public float metallic = 0;
    public float roughness = 0;

    public void updateUniforms() {
        GL.Uniform3(GL.GetUniformLocation(Renderer.geomPass.id, "material.albedo"), albedo.x, albedo.y, albedo.z);
        GL.Uniform1(GL.GetUniformLocation(Renderer.geomPass.id, "material.metallic"), metallic);
        GL.Uniform1(GL.GetUniformLocation(Renderer.geomPass.id, "material.roughness"), roughness);
    }
}