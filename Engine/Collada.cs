using System.Collections.Generic;
using System.Linq;
using Nums;
using System.Xml;
using System;

namespace Engine {


    public class Collada {

        public readonly Dictionary<string, Prefab> prefabs;
        public readonly Dictionary<string, PBRMaterial> materials;
        public readonly Dictionary<string, (Mesh<Vertex> mesh, string[] materialNames)> meshes;

        private readonly List<Geometry> colladaGeometries = new List<Geometry>();
        private readonly List<Material> colladaMaterials = new List<Material>();
        private readonly XmlElement scene;

        private static readonly mat3 m = new mat3(
                    1, 0, 0,
                    0, 0, 1,
                    0, -1, 0
                    );

        private static vec3 correct_axis(vec3 v) {
            // TODO: only do this when z-axis is up
            //return m * v;
            //return v.xzy * new vec3(1, 1, -1);
            return v;
        }

        public static Collada fromFile(string filepath) {
            var doc = new XmlDocument();
            doc.Load(filepath);
            return new Collada(doc);
        }

        public Collada(XmlDocument doc) {
            var root = doc["COLLADA"];

            // asset
            var asset_xml = root["asset"];
            var up_axis = asset_xml["up_axis"].InnerText;

            // scenes
            scene = root["library_visual_scenes"]["visual_scene"];

            // geometry
            var lib_geom = root["library_geometries"];
            foreach (var item in lib_geom.GetElementsByTagName("geometry")) {
                colladaGeometries.Add(new Geometry(this, (XmlElement)item));
            }

            // materials
            var lib_materials = root["library_materials"];
            foreach (var item in lib_materials.GetElementsByTagName("material")) {
                colladaMaterials.Add(new Material((XmlElement)item));
            }



            // meshes
            meshes = new();
            foreach (var g in colladaGeometries) {
                meshes.Add(g.id, (g.mesh, g.materialNames));
            }

            // materials
            materials = new();
            foreach (var cm in colladaMaterials) {
                materials.Add(cm.name, cm.pbrMaterial);
            }
            
            // prefabs
            prefabs = toPrefabs();
        }

        Geometry get_geometry(string id) => colladaGeometries.Find(x => x.id.Equals(id));
        Material get_material(string id) => colladaMaterials.Find(x => x.id.Equals(id));

        Dictionary<string, Prefab> toPrefabs() {
            Prefab node(XmlElement xml) {
                var g = new Prefab();

                var fs = xml["matrix"].InnerText.Split(' ').Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                var m = new mat4();
                m.m11 = fs[0];
                m.m12 = fs[1];
                m.m13 = fs[2];
                m.m14 = fs[3];

                m.m21 = fs[4];
                m.m22 = fs[5];
                m.m23 = fs[6];
                m.m24 = fs[7];

                m.m31 = fs[8];
                m.m32 = fs[9];
                m.m33 = fs[10];
                m.m34 = fs[11];

                m.m41 = fs[12];
                m.m42 = fs[13];
                m.m43 = fs[14];
                m.m44 = fs[15];
                //m.transpose();

                g.transform.setMatrix(m.transpose);

                g.transform.position = correct_axis(g.transform.position);

                //g.AddComp(new AdvMeshRenderer { mesh = geoms[xml["instance_geometry"].GetAttribute("url").TrimStart('#')] });


                var inst_geom = xml["instance_geometry"];
                if (inst_geom != null) {
                    var geom = meshes[inst_geom.GetAttribute("url").TrimStart('#')];

                    var inst_materials = inst_geom["bind_material"]?["technique_common"]?.GetElementsByTagName("instance_material");

                    var finalMaterials = new PBRMaterial[geom.materialNames.Length];
                    if (inst_materials != null) {
                        foreach (var material in inst_materials) {
                            var mat = material as XmlElement;
                            var symbol = mat.GetAttribute("symbol");
                            var target = mat.GetAttribute("target").TrimStart('#');
                            // target is material name
                            // symbol is group name                                            
                            var matIndex = Array.FindIndex(geom.materialNames, s => s.Equals(symbol));
                            if (matIndex > -1) {
                                finalMaterials[matIndex] = get_material(target).pbrMaterial;
                            }
                        }
                    }

                    for (int i = 0; i < finalMaterials.Length; i++) finalMaterials[i] ??= new PBRMaterial { albedo = (1, 0, 1) }; //PBRMaterial.defaultMaterial;

                    g.addComponent<MeshRenderer>(new Dictionary<string, object> {
                        { "mesh", geom.mesh },
                        { "materials", finalMaterials }
                    });
                }


                foreach (var child in xml.SelectNodes("*[@type='NODE']")) {
                    //g.AddChild(node(child as XmlElement));
                    g.addChild(node(child as XmlElement));
                }
                return g;
            }

            var res = new Dictionary<string, Prefab>();
            var nodes = scene.SelectNodes("*[@type='NODE']");
            foreach (var item in nodes) {
                var x = (XmlElement)item;
                res.Add(x.GetAttribute("id"), node(x));
            }

            return res;
        }

        public class Geometry {
            readonly Collada collada;
            public readonly string id, name;

            public readonly Mesh<Vertex> mesh;
            public readonly string[] materialNames;

            Dictionary<string, Source> sources = new Dictionary<string, Source>();
            TriangleCollection[] triangles;

            class Source {
                vec[] cache = null;
                float[] float_array;
                int stride;
                public Source(XmlElement xml) {
                    float_array = xml["float_array"].InnerText.Split(' ').Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                    stride = int.Parse(xml["technique_common"]["accessor"].GetAttribute("stride"));
                }
                public T[] as_vector_array<T>() where T : vec, new() {
                    if (cache != null) return cache as T[];
                    
                    T[] res = new T[float_array.Length / stride];
                    for (int i = 0; i < res.Length; i++) {
                        res[i] = new T();
                        for (int j = 0; j < stride; j++) {
                            res[i][j] = float_array[i * stride + j];
                        }
                    }

                    cache = res as vec[];
                    return res;
                }
            }
            class TriangleCollection {

                private const string VERTEX = "VERTEX";
                private const string NORMAL = "NORMAL";
                private const string TEXCOORD = "TEXCOORD";
                private const string COLOR = "COLOR";

                Geometry geometry;

                int num_inputs;
                public readonly (Source source, int offset) pos_input;
                public readonly (Source source, int offset) normal_input;
                public readonly (Source source, int offset) texcoord_input;
                
                public string material_name;

                public vertexindices[] indices;
                public class vertexindices {
                    public int pos_index;
                    public int normal_index;
                    public int texcoord_index;
                }

                public TriangleCollection(Geometry gom, XmlElement xml) {
                    geometry = gom;
                    material_name = xml.GetAttribute("material");

                    var input_nodes = xml.GetElementsByTagName("input");
                    num_inputs = input_nodes.Count;
                    foreach (var input in input_nodes) {
                        var i = input as XmlElement;
                        var src = geometry.sources.TryGetValue(i.GetAttribute("source").TrimStart('#'), out Source s) ? s : null;
                        var ofs = int.Parse(i.GetAttribute("offset"));
                        var semantic = i.GetAttribute("semantic");
                        if (semantic.Equals(VERTEX)) {
                            pos_input.source = src;
                            pos_input.offset = ofs;
                        } else if (semantic.Equals(NORMAL)) {
                            normal_input.source = src;
                            normal_input.offset = ofs;
                        } else if (semantic.Equals(TEXCOORD)) {
                            texcoord_input.source = src;
                            texcoord_input.offset = ofs;
                        }

                    }


                    var ints = xml["p"].InnerText.Split(' ').Select(x => int.Parse(x)).ToArray();
                    var indices = new List<vertexindices>();
                    for (int i = 0; i < ints.Length / num_inputs; i++) {
                        var index = i * num_inputs;
                        int get_input_index((Source source, int offset) input) {
                            if (input.source == null) return -1;
                            else return ints[index + input.offset];
                        }

                        indices.Add(new vertexindices {
                            pos_index = get_input_index(pos_input),
                            normal_index = get_input_index(normal_input),
                            texcoord_index = get_input_index(texcoord_input)
                        });

                    }


                    this.indices = indices.ToArray();
                }
            }

            internal Geometry(Collada c, XmlElement xml) {
                collada = c;
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");

                xml = xml["mesh"];

                // sources
                var srcnodes = xml.GetElementsByTagName("source");
                foreach (var item in srcnodes) {
                    var x = item as XmlElement;
                    sources.Add(x.GetAttribute("id"), new Source(x));
                }
                // vertices
                var vs = xml["vertices"];
                sources.Add(vs.GetAttribute("id"), sources[vs["input"].GetAttribute("source").TrimStart('#')]);

                // triangles
                triangles = xml.GetElementsByTagName("triangles").Cast<XmlElement>().Select(x => new TriangleCollection(this, x)).ToArray();


                (mesh, materialNames) = genMesh();
            }

            (Mesh<Vertex> mesh, string[] materialNames) genMesh() {
                var mesh = new Meshdata<Vertex>();

                static int add_vertex(Vertex v, Meshdata<Vertex> mesh) {
                    //if (mesh.vertices.Contains(v)) return mesh.vertices.IndexOf(v);
                    //var index = mesh.vertices.IndexOf(v);
                    //if (index != -1) return index;
                    

                    
                    v.normal = correct_axis(v.normal);
                    v.position = correct_axis(v.position);

                    mesh.vertices.Add(v);
                    return mesh.vertices.Count - 1;
                }


                int groupIndex = 0;
                //var materials = new PBRMaterial[triangles.Length];
                var materials = new string[triangles.Length];
                foreach (var trcollection in triangles) {
                    var indices = new uint[trcollection.indices.Length];
                    vec3[] positions = trcollection.pos_input.source.as_vector_array<vec3>();
                    vec3[] normals = trcollection.normal_input.source.as_vector_array<vec3>();
                    vec2[] texcoords = trcollection.texcoord_input.source?.as_vector_array<vec2>();

                    for (int i = 0; i < trcollection.indices.Length; i++) {
                        var vi = trcollection.indices[i];
                        indices[i] = (uint)add_vertex(new Vertex{
                            position = positions[vi.pos_index], 
                            uv = texcoords?[vi.texcoord_index] ?? vec2.zero, 
                            normal = normals[vi.normal_index],
                            //color = (math.range(0, 1), math.range(0, 1),math.range(0, 1))
                        }, mesh);
                    }

                    mesh.addTriangles(groupIndex, indices);
                    //materials[groupIndex] = collada.get_material(trcollection.material_name).pbrMaterial;
                    materials[groupIndex] = trcollection.material_name;
                    groupIndex++;
                }
                //mesh.bufferdata();

                return (new Mesh<Vertex>(mesh), materials);
            }
        }


        public class Material {
            public readonly string id, name;

            public readonly PBRMaterial pbrMaterial;

            internal Material(XmlElement xml) {
                id = xml.GetAttribute("id");
                name = xml.GetAttribute("name");

                var effectId = xml["instance_effect"].GetAttribute("url").TrimStart('#');

                var lambert_xml = xml.OwnerDocument.DocumentElement["library_effects"].SelectSingleNode($"*[@id='{effectId}']")["profile_COMMON"]["technique"]["lambert"];

                vec4 parse_color(string t) {
                    var n = t.Split(' ').Select(x => float.Parse(x, System.Globalization.CultureInfo.InvariantCulture));
                    return new vec4(n.ElementAt(0), n.ElementAt(1), n.ElementAt(2), n.ElementAt(3));
                }

                pbrMaterial = new PBRMaterial {
                    albedo = parse_color(lambert_xml["diffuse"].InnerText).xyz,
                    //emission = parse_color(lambert_xml["emission"].InnerText).xyz,
                    roughness = 1 - float.Parse(lambert_xml["reflectivity"]?.InnerText ?? "0", System.Globalization.CultureInfo.InvariantCulture)
                };

            }

        }


    }

}
