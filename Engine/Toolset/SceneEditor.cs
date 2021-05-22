using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System;
using Engine.Gui;

namespace Engine.Toolset {

    public enum EditorRenderMode {
        lights,
        wireframe,
        solid
    }

    public class SceneEditor : SceneBase {

        public static EditorRenderMode renderMode;
        public static LinkedList<Gameobject> selection = new();
        public static Camera camera { get; private set; }
        

        Scene editorScene = new();

        ivec2 lastLeftclickPos;

        internal SceneEditor() {

            var cam = new Gameobject(
                camera = new Camera(),
                new EditorCamera()
            );
            cam.enterScene(editorScene);



            var transformGizmoPrefab = Assets.getPrefab("Engine.data.models.TransformGizmo.pivot");
            var transformGizmo = transformGizmoPrefab.createInstance();
            transformGizmo.enterScene(editorScene);

        }


        internal override void update() {
            editorScene.update();

            //if (Keyboard.isPressed(key.M)) renderMode = EditorRenderMode.wireframe;

            if (Keyboard.isPressed(key.Delete)) {
                foreach (var s in selection) s.destroy();
                selection.Clear();
            }
        }



        internal override void updateCamera() {            
            editorScene.camera.use();
        }

        internal override void geometryPass() {
            //GL.LineWidth(10);
            if (renderMode == EditorRenderMode.wireframe) GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            Scene.active.geometryPass();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);


            GL.DepthFunc(DepthFunction.Always);
            editorScene.geometryPass();
            GL.DepthFunc(DepthFunction.Lequal);



            { // selections

                // handle selections
                if (Mouse.state == MouseState.free) {
                    if (Mouse.isPressed(MouseButton.left)) {
                        lastLeftclickPos = (ivec2)Mouse.position;
                    } else if(Mouse.isDown(MouseButton.left)) {
                        Editor.canvas.rect(lastLeftclickPos, (Mouse.position - lastLeftclickPos), color.rgba(1,1,1,0.5f));
                    } else if (Mouse.isReleased(MouseButton.left)) {
                        var mpos = (ivec2)Mouse.position;

                        if (lastLeftclickPos.x == mpos.x && lastLeftclickPos.y == mpos.y) { // single selection:
                            var obj = ScreenRaycast.select(Scene.active, (ivec2)Mouse.position)?.gameobject;

                            if (Keyboard.isDown(key.LeftShift)) {
                                if (selection.Contains(obj)) selection.Remove(obj);
                                else if (obj is not null) selection.AddLast(obj);
                            } else {
                                selection.Clear();
                                if (obj is not null) selection.AddLast(obj);
                            }
                        } else { // box selection
                            var sel = ScreenRaycast.select(Scene.active, lastLeftclickPos, (ivec2)Mouse.position);
                        
                            if (Keyboard.isDown(key.LeftShift)) {
                                foreach (var s in sel) if (s.gameobject is not null && !selection.Contains(s.gameobject)) selection.AddLast(s.gameobject);
                            } else {
                                selection.Clear();
                                foreach (var s in sel) if (s.gameobject is not null) selection.AddLast(s.gameobject);
                            }
                        }
                    }
                }


                // highlight selection
                foreach (var g in selection) {
                    //g.transform.rotate(vec3.unity, 0.05f);
                    for (int i = 0; i < 10; i++) {
                        g.calcWorldPosition(out vec3 wpos);

                        foreach (var c in g.children) {
                            c.calcWorldPosition(out vec3 cpos);
                            Gizmo.line(wpos, cpos);
                        }

                        if (g.parent != null) {
                            g.parent.calcWorldPosition(out vec3 ppos);
                            Gizmo.line(wpos, ppos);
                        }

                        Gizmo.color(Utils.randColor());
                        Gizmo.point(new vec3(math.rand(), math.rand(), math.rand()) + wpos);
                    }
                }
            }


        }

        internal override void lightPass() {
            Scene.active.lightPass();
        }

        internal override void forwardPass() {
            Scene.active.forwardPass();
        }


        internal override void renderFrame() {
            if (selection.First != null) {
                selection.First.Value.calcWorldPosition(out vec3 wpos);
                editorScene.camera.world2ndc(in wpos, out vec2 coords);
                Editor.canvas.ndc2canvasCoord(ref coords);
                Editor.canvas.text(coords, Font.arial, 30, "Hello World", in color.white);
            }
        }     
    }

    public class ContexMenu {
        static ContexMenu current;
        static ivec2 pos;

        string title;
        (string text, Action action)[] options;

        public ContexMenu(string title, params (string text, Action action)[] options) {
            this.title = title;
            this.options = options;
        }

        public void open() {
            current = this;
            pos = (ivec2)Mouse.position;
        }
        public static void close() {
            current = null;
        }

        public static void render(Canvas canvas) {
            if (current is null) return;

            current.draw(canvas);
        }

        void draw(Canvas canvas) {
            
            var textcolor = color.hex(0xd4d4d4ff);

            var p = pos;

            canvas.text(p, Font.arial, 20, title, in textcolor);
            canvas.rect(p, (130, 20 + options.Length * 16), color.hex(0x1e1e1eff));
            p.y += 20;
            p.x += 15;

            for (int i = 0; i < options.Length; i++) {
                var o = options[i];
                canvas.text(p, Gui.Font.arial, 16, o.text, textcolor);
                p.y += 16;
            }

        }
    }

}

