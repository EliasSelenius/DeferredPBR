using Nums;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace Engine {
    public class Camera : Component {

        public float fieldOfView = 70;
        public float nearPlane = 0.1f;
        public float farPlane = 10000;

        public float nearPlaneHeight => 2 * nearPlane * math.tan(fieldOfView * math.deg2rad / 2f);

        public mat4 projectionMatrix = mat4.identity;
        public mat4 viewMatrix = mat4.identity;

        protected override void onEnter() {
            scene.camera = this;
        }

        public void screenToRay(vec2 ndc, out vec3 raydir) {
            var screenPoint = new Vector4(ndc.x, ndc.y, -1, 1);
            screenPoint = screenPoint * Utils.toOpenTK(projectionMatrix).Inverted();
            screenPoint.Z = -1;
            screenPoint.W = 0;

            var point = (screenPoint * Utils.toOpenTK(viewMatrix).Inverted()).Xyz;
            point.Normalize();
            raydir = Utils.toNums(point);
        }



        public void updateUniformBuffer() {

            viewMatrix = math.lookAt(transform.position, transform.position + transform.forward, transform.up);
            //viewMatrix = Matrix4.LookAt(transform.position.toOpenTK(), (transform.position + transform.forward).toOpenTK(), transform.up.toOpenTK()).toNums();

            Matrix4.CreatePerspectiveFieldOfView(fieldOfView * math.deg2rad, (float)Application.window.Size.X / Application.window.Size.Y, nearPlane, farPlane, out Matrix4 res);
            projectionMatrix = res.toNums(); 

            Renderer.updateCamera(ref viewMatrix, ref projectionMatrix);
        }


        protected override void onEditorRender() {
            //screenToRay((0, 0), out vec3 ray);

        }

    }

    public class CameraFlyController : Component {
        
        Camera camera;

        protected override void onStart() {
            camera = gameobject.requireComponent<Camera>();
        }


        Collider lastInteractedWith;
        vec2 lastMousePos;
        
        protected override void onUpdate() {    

            var t = vec3.zero;
            var speed = 2.5f;

            if (Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A)) {
                t += transform.left;
            } 

            if (Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D)) {
                t += transform.right;
            }

            if (Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W)) {
                t += transform.forward;
            }
            
            if (Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S)) {
                t += transform.backward;
            }
            
            if (Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift)) {
                speed *= 4.0f;
            }


            transform.position += t * speed * Application.deltaTime;


            var alt = Application.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt);        
            Mouse.state = alt ? MouseState.free : MouseState.disabled;
                    
            if (Mouse.state == MouseState.disabled) {
                (float mdx, float mdy) = Application.window.MouseState.Delta / 100f;
                //System.Console.WriteLine(transform.rotation);
                transform.rotate(vec3.unity, mdx);
                transform.rotate(transform.left, -mdy);
            }
            

            
            if (Mouse.isPressed(MouseButton.left)) {
                camera.screenToRay(Mouse.ndcPosition, out vec3 raydir);
                var col = scene.colliders.raycast(in transform.position, raydir);
                if (col is not null) {
                    //col.gameobject.getComponent<Rigidbody>()?.addForce(raydir * 10f);
                    lastInteractedWith = col;
                    lastMousePos = Mouse.ndcPosition;   
                }
            }

            if (Mouse.isReleased(MouseButton.left)) {
                if (lastInteractedWith != null) {
                    var v = Mouse.ndcPosition - lastMousePos;
                    lastInteractedWith.gameobject.getComponent<Rigidbody>()?.addForce(10 * (transform.up * v.y + transform.right * v.x));
                }
            }

            

        }
    }
}