using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Max;
using MeshInspector.Utils;

namespace MeshInspector.NodeView
{
    /// <summary>
    /// extensions and helper for INodes, IMesh etc
    /// </summary>
    internal static class MeshExtenstions
    {
        /// <summary>
        /// get the face area for a face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="verts"></param>
        /// <returns></returns>
        public static IPoint3 GetFaceNormal(this IFace face, IList <IPoint3> verts)
        {
            // get all vert indices from this face
            int[] arrVertIndices = face.GetAllVerts();

            // get the IPoints from the vertlist
            // since all max sdk interface are using the IDisposable interface
            // you have to call dispose, they will be not handled by the garbage collection
            using (IPoint3 v0 = verts[arrVertIndices[0]])
            using (IPoint3 v1 = verts[arrVertIndices[1]])
            using (IPoint3 v2 = verts[arrVertIndices[2]])
            {
                // build the edge vestors
                using (IPoint3 v1v0 = v1.Subtract(v0))
                using (IPoint3 v2v1 = v2.Subtract(v1))
                {
                    // the cross product of the edge vector is the face normal ( unnormalized )
                    // the length of the face normal is twice the face area
                    return v1v0.CrossProduct(v2v1);
                }
            }
        }

        /// <summary>
        /// get the area of a face via its normal
        /// </summary>
        /// <param name="faceNormal"></param>
        /// <returns></returns>
        public static float GetFaceArea(this IPoint3 faceNormal)
        {
            // the length of the face normal is twice the face area
            return Math.Abs(faceNormal.Length * 0.5f);
        }

        public static float GetFaceArea(this IFace face, IList <IPoint3> verts)
        {
            // get all vert indices from this face
            int[] arrVertIndices = face.GetAllVerts();

            // get the IPoints from the vertlist
            // since all max sdk interface are using the IDisposable interface
            // you have to call dispose, they will be not handled by the garbage collection
            using (IPoint3 v0 = verts[arrVertIndices[0]])
            using (IPoint3 v1 = verts[arrVertIndices[1]])
            using (IPoint3 v2 = verts[arrVertIndices[2]])
            {
                // build the edge vestors and return the result
                using (IPoint3 v1v0 = v1.Subtract(v0))
                using (IPoint3 v2v1 = v2.Subtract(v1))
                    return v1v0.CrossProduct(v2v1).GetFaceArea();
            }
        }

        public static float GetFaceArea(this ITVFace face, IList <IPoint3> verts)
        {
            // get all vert indices from this face
            int[] arrVertIndices = face.GetAllVerts();

            // get the IPoints from the vertlist
            // since all max sdk interface are using the IDisposable interface
            // you have to call dispose, they will be not handled by the garbage collection
            using (IPoint3 v0 = verts[arrVertIndices[0]])
            using (IPoint3 v1 = verts[arrVertIndices[1]])
            using (IPoint3 v2 = verts[arrVertIndices[2]])
            {
                // build the edge vestors and return the result
                using (IPoint3 v1v0 = v1.Subtract(v0))
                using (IPoint3 v2v1 = v2.Subtract(v1))
                    return v1v0.CrossProduct(v2v1).GetFaceArea();
            }
        }

        /// <summary>
        /// create the cross product of 2 vectors
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static IPoint3 CrossProduct(this IPoint3 lhs, IPoint3 rhs)
        {
            // create a point3 via global interface from max sdk
            return AssemblyFunctions.GlobalInterface.Point3.Create
                (lhs.Y * rhs.Z - lhs.Z * rhs.Y
                    , lhs.Z * rhs.X - lhs.X * rhs.Z
                    , lhs.X * rhs.Y - lhs.Y * rhs.X
                );
        }


        /// <summary>
        /// add 2 points together
        /// something was weird with the add method inside ipoint3 interface
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static IPoint3 AddPoint3(this IPoint3 lhs, IPoint3 rhs)
        {
            // create a point3 via global interface from max sdk
            return AssemblyFunctions.GlobalInterface.Point3.Create(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }

        /// <summary>
        /// normalize a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static IPoint3 Normalize(this IPoint3 vector)
        {
            // check if the vector is valid
            if (!vector.ValidVector())
                return vector;

            float fLength = vector.Length;

            IPoint3 vecResult = vector;
            vecResult.X /= fLength;
            vecResult.Y /= fLength;
            vecResult.Z /= fLength;

            return vecResult;
        }

        /// <summary>
        /// create a bitarray for a face selection
        /// </summary>
        /// <param name="selectionIndices"></param>
        /// <param name="numTris"></param>
        /// <returns></returns>
        public static IBitArray SelectionAllFromHashSet(this HashSet <int> selectionIndices, int numTris)
        {
            // create a bitarray via global interface from max sdk
            // on initialize all bits are false
            IBitArray faceSelection = AssemblyFunctions.GlobalInterface.BitArray.Create(numTris);

            int[] arrErrorFaces = selectionIndices.ToArray();

            // cycle trough all error face indices 
            // set the bit for the specific face index
            for (int i = 0; i < arrErrorFaces.Length; i++)
                faceSelection.Set(arrErrorFaces[i]);

            return faceSelection;
        }

        /// <summary>
        /// create a bitarray for a face selection
        /// </summary>
        /// <param name="selectionIndices"></param>
        /// <param name="numTris"></param>
        /// <returns></returns>
        public static IBitArray SelectionFirstFromHashSet(this HashSet <int> selectionIndices, int numTris)
        {
            // create a bitarray via global interface from max sdk
            // on initialize all bits are false
            IBitArray faceSelection = AssemblyFunctions.GlobalInterface.BitArray.Create(numTris);

            // set the bit for the first face index in the error list
            faceSelection.Set(selectionIndices.FirstOrDefault());

            return faceSelection;
        }

        /// <summary>
        /// create a bitarray for a face selection
        /// </summary>
        /// <param name="selectionIndices"></param>
        /// <param name="index"></param>
        /// <param name="numTris"></param>
        /// <returns></returns>
        public static IBitArray SelectionNextFromHashSet(this HashSet <int> selectionIndices, int index, int numTris)
        {
            // create a bitarray via global interface from max sdk
            // on initialize all bits are false
            IBitArray faceSelection = AssemblyFunctions.GlobalInterface.BitArray.Create(numTris);

            // set the bit for a specific face index in the error list
            faceSelection.Set(selectionIndices.ElementAt(index));

            return faceSelection;
        }

        /// <summary>
        /// helper for the notifiy dependands on node
        /// if you change anything on a node the node have to notify all dependents
        /// this is similar to events in c#
        /// </summary>
        /// <param name="node"></param>
        /// <param name="interval"></param>
        /// <param name="part"></param>
        /// <param name="message"></param>
        public static void NotifyDependents(this IINode node, IInterval interval, int part, RefMessage message)
        {
            node.NotifyDependents(interval, new UIntPtr((uint) part), message, Globals.NOTIFY_ALL, true, null);
        }

        /// <summary>
        /// validate a vector
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool ValidVector(this IPoint3 point)
        {
            // check if the vector is zero
            if (Math.Abs(point.X) < float.Epsilon
                && Math.Abs(point.Y) < float.Epsilon
                && Math.Abs(point.Z) < float.Epsilon)
                return false;

            // check if any component is NaN
            if (float.IsNaN(point.X)
                || float.IsNaN(point.Y)
                || float.IsNaN(point.Z))
                return false;

            return true;
        }


        // create a hashset with specific size and fill it
        public static HashSet <int> CreateHashSet(this int count)
        {
            int[] arrValues = new int[count];
            for (int i = 0; i < arrValues.Length; i++)
                arrValues[i] = i;

            return new HashSet <int>(arrValues);
        }


        /// <summary>
        /// get all vert indices 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int[] GetAllVerts(this ITVFace face)
        {
            return new[] {(int) face.GetTVert(0), (int) face.GetTVert(1), (int) face.GetTVert(2)};
        }

        /// <summary>
        /// get all vert indices 
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static int[] GetAllVerts(this IFace face)
        {
            return new[] {(int) face.GetVert(0), (int) face.GetVert(1), (int) face.GetVert(2)};
        }
    }
}
