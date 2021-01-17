using System;
using Nums;

namespace Engine {
    public class BoxCollider : Collider {
        public override void intersects(Collider other, out intersection intersection) => other.intersects(this, out intersection);
        
        
        public override bool intersectsRay(in vec3 pos, in vec3 dir) {
            throw new NotImplementedException();
        }

        public override void intersects(SphereCollider other, out intersection intersection) {
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

