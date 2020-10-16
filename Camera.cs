using Nums;
using OpenTK.Mathematics;

class Camera {
    public readonly Transform transform = new Transform();
    public float fieldOfView = 70;
    public float nearPlane = 0.1f;
    public float farPlane = 10000;

    public mat4 projectionMatrix = mat4.identity;
    public mat4 viewMatrix = mat4.identity;

    public void move() {
        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
            transform.position.x += 0.03f;
        } 

        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) {
            transform.position.x -= 0.03f;
        }

        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
            transform.position.z += 0.03f;
        }
        
        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
            transform.position.z -= 0.03f;
        }
    }

    public void updateUniforms() {

        viewMatrix = math.lookAt(transform.position, transform.position + vec3.unitz, vec3.unity);
        //viewMatrix = Matrix4.LookAt(transform.position.toOpenTK(), (transform.position + vec3.unitz).toOpenTK(), vec3.unity.toOpenTK()).toNums();

        Matrix4.CreatePerspectiveFieldOfView(fieldOfView * math.deg2rad, (float)app.window.Size.X / app.window.Size.Y, nearPlane, farPlane, out Matrix4 res);
        projectionMatrix = res.toNums(); 

        GLUtils.setUniformMatrix4("projection", ref projectionMatrix);
        GLUtils.setUniformMatrix4("view", ref viewMatrix);
    }

}