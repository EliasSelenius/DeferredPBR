using Nums;
using Engine;

class WaterChunk : Component {

    protected override void onStart() {
        var mr = gameobject.requireComponent<MeshRenderer>();
        mr.mesh = new Mesh<Vertex>(MeshFactory<Vertex>.genPlane(30, 30));
        mr.materials = new[] { PBRMaterial.defaultMaterial };


    }

    protected override void onUpdate() {
        transform.position.xz = scene.camera.transform.position.xz;
    }
}