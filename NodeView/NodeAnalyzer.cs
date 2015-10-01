using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Max;
using MeshInspector.Utils;

namespace MeshInspector.NodeView
{
    /// <summary>
    /// small helper struct for better using the analyze methods
    /// that way you don't need to call methods with lots of parameters
    /// </summary>
    internal struct sNodeData
    {
        public NodeView ViewNode;
        public IList<IFace> Faces;
        public IList<IPoint3> Verts;

        /// <summary>
        /// map to get all connected faces to a specific vertex. 
        /// like maxscript VertUsingFaces
        /// </summary>
        public Dictionary <uint, HashSet <int>> VertUsingFaces;

        public int NumUVChannels;

        public bool IsValid;

        public void SetData(NodeView nodeview)
        {
            if (nodeview == null || !nodeview.IsValid)
                return;

            this.ViewNode = nodeview;

            // collect all verts and faces
            // this will be used in every check
            // no need to retrieve again and again and again
            this.Faces = nodeview.Mesh.Faces;
            this.Verts = nodeview.Mesh.Verts;

            this.VertUsingFaces = new Dictionary<uint, HashSet<int>>(this.Verts.Count);

            // cycle through the available maps and check for if maps 1 to n is supported
            // vertex colors are inside the mesh.maps collection as well but you can't see if its a real uv map or
            // just vertex color maps
            this.NumUVChannels = 0;
            for (int i = 1; i <= nodeview.Mesh.Maps.Count; i++)
            {
                if (nodeview.Mesh.MapSupport(i))
                    this.NumUVChannels++;
            }

            this.BuildVertFaceMap();

            this.IsValid = true;
        }

        /// <summary>
        /// cycle through all faces and build a map with vertices and their connected faces
        /// </summary>
        private void BuildVertFaceMap()
        {
            // HashSets<T> are like BitArrays in maxsdk. very usefull
            HashSet <int> hashFaces;
            uint vIndex;

            // cyacle through all faces
            for (int i = 0; i < this.Faces.Count; i++)
            {
                // cycle through the vertx indices of the face
                // since its a tri mesh the faces only got 3 vertices
                for (int j = 0; j < 3; j++)
                {
                    // get the vertex index
                    vIndex = this.Faces[i].GetVert(j);

                    // check if the vertex index is available in our vertex map
                    if (!this.VertUsingFaces.TryGetValue(vIndex, out hashFaces))
                    {
                        // create a new hashset and add it to our map
                        hashFaces = new HashSet <int>();
                        this.VertUsingFaces[vIndex] = hashFaces;
                    }

                    // had the face to 
                    hashFaces.Add(i);
                }
            }
        }
    }

    /// <summary>
    /// static class for analyzing the mesh 
    /// </summary>
    internal static class NodeAnalyzer
    {
        /// <summary>
        /// analyze a nodeview if it contains errors
        /// </summary>
        /// <param name="nodeView"></param>
        /// <returns></returns>
        internal static async Task Analyze(NodeView nodeView)
        {
            if (!nodeView.IsValidWorkingNode)
                return;

            sNodeData nodeData = new sNodeData();

            await Task.Run(() => nodeData.SetData(nodeView));

            if (!nodeData.IsValid)
                return;

            await Task.WhenAll
                (
                    NodeAnalyzer.CheckMisc(nodeData),
                    NodeAnalyzer.CheckDegTris(nodeData),
                    NodeAnalyzer.CheckBadSMGTris(nodeData),
                    NodeAnalyzer.CheckDegUVTris(nodeData)
                );
        }
        
        /// <summary>
        /// clean a node of isolated vertices and map id errors 
        /// </summary>
        /// <param name="nodeView"></param>
        public static void CleanMesh(NodeView nodeView)
        {
            if (!nodeView.IsValidWorkingNode)
                return;

            NodeAnalyzer.RemoveIsoVerts(nodeView);
            NodeAnalyzer.FixMaterialIDs(nodeView);

            nodeView.ResetMiscErrors();
        }

        /// <summary>
        /// delete isolated mesh and isolated uv vertices 
        /// </summary>
        /// <param name="nodeView"></param>
        private static void RemoveIsoVerts(NodeView nodeView)
        {
            //delete all isolated not used vertices
            nodeView.Mesh.DeleteIsoVerts();
            nodeView.Mesh.DeleteIsoMapVerts();

            // very important. call this to notify max that this node has changed
            // this is similar to events in c#
            nodeView.Node.NotifyDependents(Globals.FOREVER, 0, RefMessage.SubanimStructureChanged);
            nodeView.Node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);
        }

        /// <summary>
        /// fix the material ids
        /// sometimes inside a node more material ids are set on faces
        /// than materials are actually used
        /// </summary>
        /// <param name="nodeView"></param>
        private static void FixMaterialIDs(NodeView nodeView)
        {
            // get the used material from the node
            int iNumMaterials = 0;
            IMtl nodeMaterial = nodeView.Node.Mtl;

            // first check if a material is on the node 
            // check as well if its a multimaterial
            if (nodeMaterial != null)
                iNumMaterials = nodeMaterial.NumSubMtls > 0 ? nodeMaterial.NumSubMtls : 1;


            // cycle through all face and check the MatID
            IFace[] arrFaces = nodeView.Mesh.Faces.ToArray();
            for (int i = 0; i < arrFaces.Length; i++)
            {
                if (arrFaces[i].MatID < iNumMaterials)
                    continue;

                arrFaces[i].MatID = iNumMaterials > 0 ? (ushort) (arrFaces[i].MatID % iNumMaterials) : (ushort)0;
            }

            // very important. call this to notify max that this node has changed
            // this is similar to events in c#
            nodeView.Node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.NodeMaterialChanged);
            nodeView.Node.NotifyDependents(Globals.FOREVER, Globals.PART_ALL, RefMessage.Change);
        }

        private static async Task CheckMisc(sNodeData nodeData)
        {
            await Task.WhenAll(NodeAnalyzer.CheckIsoUVVerts(nodeData), NodeAnalyzer.CheckMatIds(nodeData));
        }

        /// <summary>
        /// check for isolated map verts
        /// the max sdk property IsoVerts inside IMeshMap is broken so do it by hand
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        private static async Task CheckIsoUVVerts(sNodeData nodeData)
        {
            await Task.Run(() =>
            {
                IMesh mesh = nodeData.ViewNode.Mesh;

                HashSet <int> vertIndices;
                IList <ITVFace> tvFaces;

                // cycle through all vertex channels
                // -2 vertex illumination
                // -1 vertx alpha
                // 0 vertex color
                // NumUVChannels is the upper limit
                for (int i = -2; i <= nodeData.NumUVChannels; i++)
                {
                    if (!mesh.MapSupport(i))
                        continue;

                    // create a hashset with all vertice indices
                    vertIndices = mesh.MapVerts(i).Count.CreateHashSet();

                    // get the tv faces from the specific mesh map channel
                    tvFaces = mesh.MapFaces(i);

                    // cycle through all the tv faces
                    for (int j = 0; j < tvFaces.Count; j++)
                    {
                        // get all the tvvertices from the current face and remove it from the hashset
                        vertIndices.ExceptWith(tvFaces[j].GetAllVerts());
                    }

                    // if there are still vertices inside the hashset we found our not used isolated vertices
                    if (vertIndices.Count > 0)
                        nodeData.ViewNode.IsoUVVertCount += vertIndices.Count;
                }
            });
        }

        /// <summary>
        /// fix the material ids
        /// sometimes inside a node more material ids are set on faces
        /// than materials are actually used
        /// </summary>
        /// <param name="nodeData"></param>
        private static async Task CheckMatIds(sNodeData nodeData)
        {
            await Task.Run(() =>
            {
                IMesh mesh = nodeData.ViewNode.Mesh;

                // get the used material from the node
                int iNumMaterials = 0;
                IMtl nodeMaterial = nodeData.ViewNode.Node.Mtl;

                // first check if a material is on the node 
                // check as well if its a multimaterial
                if (nodeMaterial != null)
                    iNumMaterials = nodeMaterial.NumSubMtls > 0 ? nodeMaterial.NumSubMtls : 1;

                // cycle through all face and check the MatID
                IFace[] arrFaces = mesh.Faces.ToArray();
                for (int i = 0; i < arrFaces.Length; i++)
                {
                    if (arrFaces[i].MatID > iNumMaterials)
                        nodeData.ViewNode.WrongMatIDCount++;
                }
            });
        }


        /// <summary>
        /// check for degenerated faces
        /// those faces can break up all connected face normals
        /// when a vertex normal is created, the degenerated faces will result in a NaN normal.
        /// this will kill all connected normals for faces that are using the same vertices as well
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        private static async Task CheckDegTris(sNodeData nodeData)
        {
            await Task.Run(() =>
            {
                // cycle through all faces
                for (int i = 0; i < nodeData.Faces.Count; i++)
                {
                    // check if the face area of the current face is smaller than float epsilon
                    // this is the fastest and easiest test to check for degenerated faces
                    if (nodeData.Faces[i].GetFaceArea(nodeData.Verts) > float.Epsilon)
                        continue;

                    nodeData.ViewNode.ContainsDegTriErrors = true;
                    nodeData.ViewNode.HashDegTris.Add(i);
                }
            });
        }

        /// <summary>
        /// check for bad smoothing groups where a vertex normla would result in a NaN normal
        /// this is bad for normal map presentation
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        private static async Task CheckBadSMGTris(sNodeData nodeData)
        {
            await Task.Run(() =>
            {
                int[] faceIndices;

                Dictionary <uint, IPoint3> mapNormals;
                IPoint3 vecNormal;
                IPoint3[] sumNormals;

                HashSet <int> hashFaceIndices;
                HashSet <int> hashVertFaces;

                IFace face;

                // cycle trough all verts
                for (uint i = 0; i < nodeData.Verts.Count; i++)
                {
                    // if the vert is not found inside our map we found an isolated vertex
                    if (!nodeData.VertUsingFaces.TryGetValue(i, out hashVertFaces))
                    {
                        nodeData.ViewNode.IsoVertCount++;
                        continue;
                    }

                    faceIndices = hashVertFaces.ToArray();

                    // create a new map for vertex normals per smoothing group
                    mapNormals = new Dictionary <uint, IPoint3>(faceIndices.Length);

                    // create a hash for faces which result in NaN vert normals
                    hashFaceIndices = new HashSet <int>();

                    // cycle through all face from the current vertex
                    for (int j = 0; j < faceIndices.Length; j++)
                    {
                        // get the current face
                        face = nodeData.Faces[faceIndices[j]];

                        // check if a vertex normal for a smoothing group is available
                        // create a new vertex normal if not
                        if (!mapNormals.TryGetValue(face.SmGroup, out vecNormal))
                            vecNormal = AssemblyFunctions.GlobalInterface.Point3.Create(0, 0, 0);

                        // add the face normal to vertex normal inside our vertex normal map per smoothing group
                        // don't normalize, that way the face area which is double of the normal length will weight the result
                        mapNormals[face.SmGroup] = vecNormal.AddPoint3(face.GetFaceNormal(nodeData.Verts));

                        // remember the current face index
                        hashFaceIndices.Add(faceIndices[j]);
                    }

                    sumNormals = mapNormals.Values.ToArray();

                    // cycle through all vertx normals per smoothing group
                    for (int j = 0; j < sumNormals.Length; j++)
                    {
                        // normalize and check for NaN
                        if (sumNormals[j].Normalize().ValidVector())
                            continue;

                        nodeData.ViewNode.HashBadSMGTris.UnionWith(hashFaceIndices);
                        nodeData.ViewNode.ContainsBadSMGErrors = true;
                    }
                }
            });
        }

        /// <summary>
        /// check for degenerated uv faces
        /// this can happen when a quad is triangulated the wrong way on the mesh
        /// image an a quad where one triangle pointing up but its mapped planar top
        /// the uv triangles for this face will produce a degenerated triangle depending on how the 
        /// edge of the mesh quad is created. turning the edge can solve this problem
        /// degenerated uv triangles will break up normal presentation as well.
        /// if you are using mikktspace those uv faces will produce NaN tangents and binormals
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        private static async Task CheckDegUVTris(sNodeData nodeData)
        {
            await Task.Run(() =>
            {
                IMesh mesh = nodeData.ViewNode.Mesh;

                IList <IPoint3> tvVerts;
                IList <ITVFace> tvFaces;

                // cycle through real UVChannels where NumUVChannels is the upper limit
                for (int i = 1; i <= nodeData.NumUVChannels; i++)
                {
                    // check if this is supported
                    if (!mesh.MapSupport(i))
                        continue;

                    // get the tv verts and faces
                    tvVerts = mesh.MapVerts(i);
                    tvFaces = mesh.MapFaces(i);

                    // cycle through all tv faces
                    for (int j = 0; j < tvFaces.Count; j++)
                    {
                        // check if the face area is smaller than float epsilon
                        if (tvFaces[j].GetFaceArea(tvVerts) > float.Epsilon)
                            continue;

                        nodeData.ViewNode.ContainsDegUvErrors = true;
                        nodeData.ViewNode.HashDegUVTris.Add(j);
                    }
                }
            });
        }
    }
}
