using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace CornellBox.Models.Objects
{
    public class Mesh
    {
        public readonly Triangle[] Triangles;

        public Mesh(Vector3[] verts, Vector3[] normals, Face[] faces, Material mat)
        {
            Triangles = new Triangle[faces.Length];
            Init(verts, normals, faces, mat);
        }

        public Mesh(string objFile, Material mat)
        {
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var faces = new List<Face>();

            foreach (var line in File.ReadLines(objFile))
            {
                var components = line.Split(' ');
                if (components[0] == "v")
                {
                    verts.Add(ParseVector(components.Skip(1).ToArray()));
                }
                else if (components[0] == "vn")
                {
                    norms.Add(ParseVector(components.Skip(1).ToArray()));
                }
                else if (components[0] == "f")
                {
                    faces.Add(ParseFace(components.Skip(1).ToArray()));
                }
            }

            Triangles = new Triangle[faces.Count];
            Init(verts.ToArray(), norms.ToArray(), faces.ToArray(), mat);
        }

        private void Init(Vector3[] verts, Vector3[] normals, Face[] faces, Material mat)
        {
            for (var i = 0; i < faces.Length; i++)
            {
                var vs = new[]
                {
                    verts[faces[i].VertsId[0] - 1],
                    verts[faces[i].VertsId[1] - 1],
                    verts[faces[i].VertsId[2] - 1]
                };

                var ns = new[]
                {
                    normals[faces[i].NormsId[0] - 1],
                    normals[faces[i].NormsId[1] - 1],
                    normals[faces[i].NormsId[2] - 1]
                };

                Triangles[i] = new Triangle(vs, ns, mat);
            }
        }

        private Vector3 ParseVector(string[] comps)
        {
            return new Vector3(float.Parse(comps[0]), float.Parse(comps[1]), float.Parse(comps[2]));
        }

        private Face ParseFace(string[] comps)
        {
            var vIds = new int[3];
            var nIds = new int[3];

            for (var i = 0; i < comps.Length; i++)
            {
                var comp = comps[i];
                var c = comp.Split(new[] {"//"}, StringSplitOptions.None);
                vIds[i] = int.Parse(c[0]);
                nIds[i] = int.Parse(c[1]);
            }

            return new Face(vIds, nIds);
        }
    }
}
