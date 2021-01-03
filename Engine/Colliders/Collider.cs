
using System.Collections.Generic;

namespace Engine {
    

    public abstract class Collider {

        
        //public abstract bool intersects(SphereCollider other);
        //public abstract bool intersects(AABBCollider other);
        //public abstract bool intersects(BoxCollider other);
    }

    public class SphereCollider : Collider {

    }

    public class AABBCollider : Collider {

    }

    public class BoxCollider : Collider {

    }

    class ColliderGroup {
        readonly List<Collider> colliders = new List<Collider>();

        
    }
}