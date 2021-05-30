using Nums;
using Engine.Gui;

namespace Engine.Toolset {
    public class EditorCamera : Component {

        vec3 velocity;
        float speedMult = 100f;

        vec3 lastClickPos;
        vec3 lastClickNormal = vec3.unity;

        protected override void onUpdate() {

            transform.position += velocity * Application.deltaTime;
            velocity *= 0.1f * Application.deltaTime;

            if (Mouse.isDown(MouseButton.right)) {
                Mouse.state = MouseState.disabled;
                var d = Mouse.delta / 100f;
                transform.rotate(vec3.unity, d.x);
                transform.rotate(transform.left, -d.y);

                velocity += (transform.forward * Keyboard.getAxis(key.S, key.W) + transform.left * Keyboard.getAxis(key.D, key.A)) * speedMult;
                
                
                float rate = 0.8f * Application.deltaTime;
                if (Keyboard.isDown(key.LeftShift)) speedMult *= (1f + rate);
                else if (Keyboard.isDown(key.LeftControl)) speedMult = math.max(1f, speedMult * (1f - rate));


            } else {
                Mouse.state = MouseState.free;
            }

            //Editor.canvas.text((0, 30), Font.arial, 16, "velocity: " + velocity.length.ToString(), color.white);
            //Editor.canvas.text((0, 46), Font.arial, 16, "speedMul: " + speedMult, color.white);


            // test screen raycast callbacks
            if (Mouse.isPressed(MouseButton.left)) {
                ScreenRaycast.onHit(hit => {
                    var mesh = (hit.renderer as MeshRenderer).mesh;
                    lastClickPos = hit.position;
                    lastClickNormal = hit.normal;
                });
            }
            Gizmo.color(in color.blue);
            Gizmo.circle(in lastClickPos, in lastClickNormal, 1);
            Gizmo.line(lastClickPos, lastClickPos + lastClickNormal);



            
            /*{ // voxel testing
                if (Mouse.isPressed(MouseButton.left)) {
                    var vpos = (ivec3)(transform.position + transform.forward * 2);
                    Editor.notify("voxel placed at " + vpos);
                    Voxels.Voxelgrid.grid.voxelAt(vpos).isSolid = true;
                    Voxels.Voxelgrid.grid.updateMesh();
                    
                }
            }*/
        }

        public void focus(in vec3 point) {
            transform.lookat(point, vec3.unity);

        }
    }

}

