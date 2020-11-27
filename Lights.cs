
using Nums;

static class Lights {
    public static readonly Mesh<posVertex> dirlightMesh;
    public static readonly Mesh<posVertex> pointlightMesh;

    static Lights() {
        
        dirlightMesh = new Mesh<posVertex>();
        dirlightMesh.vertices.AddRange(new[] {
            new posVertex(-1, -1, 0),
            new posVertex(1, -1, 0),
            new posVertex(-1, 1, 0),
            new posVertex(1, 1, 0)
        });

        dirlightMesh.addTriangles(0, new uint[] {
            0, 1, 2,
            2, 1, 3
        });

        dirlightMesh.bufferdata();

        pointlightMesh = MeshFactory<posVertex>.genSphere(20, 1f);
        pointlightMesh.flipIndices();
        pointlightMesh.bufferdata();

/*
        dirlightVAO = GLUtils.createVertexArray<posVertex>(GLUtils.createBuffer(new[] {
            new posVertex(-1, -1, 0),
            new posVertex(1, -1, 0),
            new posVertex(-1, 1, 0),
            new posVertex(1, 1, 0)
        }), GLUtils.createBuffer(new uint[] {
            0, 1, 2,
            2, 1, 3
        }));
*/
        
    
    }
}

/*
r = c / d^2

r = 1 / 256

r * d^2 / c = 1

d^2 / c = 1 / r
d^2 = c / r
d = sqrt(c / r)
*/

class Pointlight {
    public vec3 position;
    public vec3 color = vec3.one;

    const float minRadiance = 1f / 1000f;


    public mat4 calcModelMatrix() {
        var m = mat4.identity;
    
        float b = math.max(color.x, math.max(color.y, color.z));
        float radius = math.sqrt(b / minRadiance); 
    
        m.diagonal = (radius, radius, radius, 1);
        m.row4.xyz = position;
        return m;
    }
}

class Dirlight {
    public vec3 dir = vec3.unity;
    public vec3 color = vec3.one; 
}