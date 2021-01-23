using System;
using Nums;


namespace Engine {
    public class AABBCollider : Collider {
        public override void intersects(Collider other, out intersection intersection) => other.intersects(this, out intersection);

        public vec3 size;
        
        public override bool intersectsRay(in vec3 pos, in vec3 dir) {
            return false;// new NotImplementedException();
        }
        

        public override void intersects(SphereCollider other, out intersection intersection) {
            Physics.sphere2AABB_Intersection(in other.transform.position, other.radius, in transform.position, in size, out intersection);
        }

        public override void intersects(AABBCollider other, out intersection intersection) {
            Physics.AABB2AABB_Intersection(in transform.position, in size, in other.transform.position, in other.size, out intersection);
        }

        public override void intersects(BoxCollider other, out intersection intersection) {
            throw new NotImplementedException();
        }
    }
}