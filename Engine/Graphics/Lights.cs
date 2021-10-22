
using Nums;

namespace Engine {

    static class Lights {
        public static readonly Mesh<posVertex> dirlightMesh;
        public static readonly Mesh<posVertex> pointlightMesh;

        static Lights() {
            
            dirlightMesh = new Mesh<posVertex>();
            dirlightMesh.data.vertices.AddRange(new[] {
                new posVertex(-1, -1, 0),
                new posVertex(1, -1, 0),
                new posVertex(-1, 1, 0),
                new posVertex(1, 1, 0)
            });

            dirlightMesh.data.addTriangles(0, new uint[] {
                0, 1, 2,
                2, 1, 3
            });

            dirlightMesh.updateBuffers();

            pointlightMesh = new Mesh<posVertex>(MeshFactory<posVertex>.genSphere(20, 1f));
            pointlightMesh.data.flipIndices();
            pointlightMesh.updateBuffers();
        }
    }


    /*
    r = c / d^2
    r = 1 / 256
    r * d^2 / c = 1
    d^2 / c = 1 / r
    d^2 = c / r
    d = sqrt(c / r)

    where:
        r is minRadiance
        c is color
        d is distance or radius
    */

    public class Pointlight : Component {
        public vec3 color = vec3.one;

        const float minRadiance = 1f / 1000f;

        public void calcLightVolumeModelMatrix(out mat4 m) {
            gameobject.calcModelMatrix(out m);

            float b = math.max(color.x, math.max(color.y, color.z));
            float radius = math.sqrt(b / minRadiance);         
            
            // overwriting gameobjects rotation and scale, as its not used for the lightvolume
            m.row1.xyz = new vec3(radius, 0, 0);
            m.row2.xyz = new vec3(0, radius, 0);
            m.row3.xyz = new vec3(0, 0, radius);
        }

        protected override void onEnter() {
            scene._pointlights.Add(this);
        }

        protected override void onLeave() {
            scene._pointlights.Remove(this);
        }
    }

    public class Dirlight {
        public vec3 dir = vec3.unity;
        public vec3 color = vec3.one;

        // TODO: may remove this feature in favor of better ambientlight/global illumination/ambientoclusion algos
        public float ambientScale = 0.04f;
    }
}
