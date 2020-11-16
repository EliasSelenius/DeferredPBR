using Nums;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

class Camera {
    public readonly Transform transform = new Transform();
    public float fieldOfView = 70;
    public float nearPlane = 0.1f;
    public float farPlane = 10000;

    public mat4 projectionMatrix = mat4.identity;
    public mat4 viewMatrix = mat4.identity;

    public static UBO ubo;

    static Camera() {
        ubo = new UBO("Camera", 2 * mat4.bytesize);
        Renderer.geomPass.bindUBO(ubo);
        Renderer.lightPass_dirlight.bindUBO(ubo);
        Renderer.lightPass_pointlight.bindUBO(ubo);
    }

    public void move() {
        

        var t = vec3.zero;
        var speed = 0.07f;

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
        
        if (app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift)) {
            speed += 0.3f;
        }


        transform.position += t * speed;
                
        (float mdx, float mdy) = app.window.MouseState.Delta / 100f;
        //System.Console.WriteLine(transform.rotation);
        transform.rotate(vec3.unity, mdx);
        transform.rotate(transform.left, -mdy);

    
        

    }

    public void updateUniforms() {

        viewMatrix = math.lookAt(transform.position, transform.position + transform.forward, transform.up);
        //viewMatrix = Matrix4.LookAt(transform.position.toOpenTK(), (transform.position + transform.forward).toOpenTK(), transform.up.toOpenTK()).toNums();

        Matrix4.CreatePerspectiveFieldOfView(fieldOfView * math.deg2rad, (float)app.window.Size.X / app.window.Size.Y, nearPlane, farPlane, out Matrix4 res);
        projectionMatrix = res.toNums(); 

        GLUtils.buffersubdata(ubo.id, 0, ref viewMatrix);
        GLUtils.buffersubdata(ubo.id, mat4.bytesize, ref projectionMatrix);

        //GLUtils.setUniformMatrix4("projection", ref projectionMatrix);
        //GLUtils.setUniformMatrix4("view", ref viewMatrix);
    }

}