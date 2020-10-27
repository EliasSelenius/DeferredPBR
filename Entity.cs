using System.Collections.Generic;
using Nums;

class Entity {
    public readonly Transform transform = new Transform();
    public Mesh<Vertex> mesh = null;
    //public PBRMaterial material = new PBRMaterial { albedo = (math.range(0, 1), math.range(0, 1),math.range(0, 1)), metallic = math.range(0, 1), roughness = math.range(0, 1) };

    public Entity parent { get; private set; }
    public List<Entity> children = new List<Entity>();

    private mat4 calcModelMatrix() {
        var m = transform.getMatrix();
        if (parent != null) m *= parent.calcModelMatrix();
        return m;
    }

    public void addChild(Entity c) {
        if (c.parent != null) throw new System.Exception("This Entity is already a child");
        c.parent = this;
        children.Add(c);
    }
    public void removeChild(Entity c) {
        if (c.parent != this) throw new System.Exception("This entity is not a child");
        c.parent = null;
        children.Remove(c);
    }

    public void render() {
        /*Utils.pushMatrix();
        Utils.rotate(transform.rotation.x, transform.rotation.y, transform.rotation.z);
        Utils.translate(transform.position);
        Utils.scale(transform.scale);
        /*Utils.rotateX(transform.rotation.x);
        Utils.rotateY(transform.rotation.y);
        Utils.rotateZ(transform.rotation.z);*/

        //var mat = Utils.currentMatrix;
        var mat = calcModelMatrix();
        GLUtils.setUniformMatrix4("model", ref mat);
        //material.updateUniforms();
        mesh?.render();

        foreach (var child in children) child.render();

        //Utils.popMatrix();
    }
}