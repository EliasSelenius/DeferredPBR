using Nums;
using System.Runtime.InteropServices;

namespace Engine {
    public interface VertexData {

        vec3 getPosition();
        void setPosition(vec3 value);

        vec3 getNormal() => vec3.zero;
        void setNormal(vec3 value) { }

        vec2 getTexcoord() => vec2.zero;
        void setTexcoord(vec2 value) { }

        vec4 getColor() => vec4.zero;
        void setColor(vec4 value) { } 
    } 


    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : VertexData {
        public vec3 position;
        public vec3 normal;
        public vec2 uv;
        //public vec3 color;

        vec3 VertexData.getPosition() => position;
        void VertexData.setPosition(vec3 value) => position = value;

        vec3 VertexData.getNormal() => normal;
        void VertexData.setNormal(vec3 value) => normal = value;

        vec2 VertexData.getTexcoord() => uv;
        void VertexData.setTexcoord(vec2 value) => uv = value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct posVertex : VertexData {
        public vec3 position;
        public posVertex(float x, float y, float z) => position = (x, y, z);

        vec3 VertexData.getPosition() => position;
        void VertexData.setPosition(vec3 value) => position = value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct posUvVertex : VertexData {
        public vec3 position;
        public vec2 uv;


        vec3 VertexData.getPosition() => position;
        void VertexData.setPosition(vec3 value) => position = value;

        vec2 VertexData.getTexcoord() => uv;
        void VertexData.setTexcoord(vec2 value) => uv = value;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct posColorVertex : VertexData {
        public vec3 position;
        public vec4 color;

        vec3 VertexData.getPosition() => position;
        void VertexData.setPosition(vec3 value) => position = value;

        vec4 getColor() => color;
        void setColor(vec4 value) => color = value;
    }
}