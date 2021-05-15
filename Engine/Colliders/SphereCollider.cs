using System;
using Nums;

namespace Engine {
    unsafe public class SphereCollider : Collider {
        public override void intersects(Collider other, out intersection intersection) => other.intersects(this, out intersection);
        
        public float radius = 1.0f;

        public override bool intersectsRay(in vec3 pos, in vec3 dir) {
            return Physics.rayIntersectsSphere(in pos, in dir, in transform.position, radius);
        }

        public override void intersects(SphereCollider other, out intersection intersection) {            
            Physics.sphere2sphere_Intersection(in transform.position, radius, in other.transform.position, other.radius, out intersection);
        }

        public override void intersects(AABBCollider other, out intersection intersection) {
            Physics.sphere2AABB_Intersection(in transform.position, radius, in other.transform.position, in other.size, out intersection);
        }

        public override void intersects(BoxCollider other, out intersection intersection) {
            throw new NotImplementedException();
        }

        protected override void onEditorRender() {
            gameobject.calcWorldPosition(out vec3 wpos);
            Toolset.Gizmo.color(in color.green);
            Toolset.Gizmo.sphere(in wpos, radius);
        }

    }
}