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
        
        var t = vec3.zero;
        
        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
            t += transform.left;
        } 

        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) {
            t += transform.right;
        }

        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
            t += transform.forward;
        }
        
        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
            t += transform.backward;
        }
        

        transform.position += t * 0.07f;
        
        (float mdx, float mdy) = app.window.MouseState.Delta / 100f;
        System.Console.WriteLine(transform.rotation);
        transform.rotate(vec3.unity, mdx);
        transform.rotate(transform.left, -mdy);
        

    }

    public void updateUniforms() {

        viewMatrix = math.lookAt(transform.position, transform.position + transform.forward, transform.up);
        //viewMatrix = Matrix4.LookAt(transform.position.toOpenTK(), (transform.position + transform.forward).toOpenTK(), transform.up.toOpenTK()).toNums();

        Matrix4.CreatePerspectiveFieldOfView(fieldOfView * math.deg2rad, (float)app.window.Size.X / app.window.Size.Y, nearPlane, farPlane, out Matrix4 res);
        projectionMatrix = res.toNums(); 

        GLUtils.setUniformMatrix4("projection", ref projectionMatrix);
        GLUtils.setUniformMatrix4("view", ref viewMatrix);
    }

}