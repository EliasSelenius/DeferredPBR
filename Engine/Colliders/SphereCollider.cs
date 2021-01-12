using System;

namespace Engine {
    public class SphereCollider : Collider {
        public override void intersects(Collider other, out intersection intersection) => other.intersects(this, out intersection);
        

        public override void intersects(SphereCollider other, out intersection intersection) {
            //Physics.sphere2sphere_Intersection()
            throw new NotImplementedException();
        }

        public override void intersects(AABBCollider other, out intersection intersection) {
            throw new NotImplementedException();
        }

        public override void intersects(BoxCollider other, out intersection intersection) {
            throw new NotImplementedException();
        }
    }
}