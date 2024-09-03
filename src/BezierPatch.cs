/*
Source: https://github.com/mrvux/dx11-vvvv/blob/master/Nodes/VVVV.DX11.Nodes/Nodes/Geometry/Primitives/DX11BezierPatchNode.cs

License:

dx11-vvvv

BSD 3-Clause License

Copyright (c) 2016, Julien Vulliet (mrvux)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; 
OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.ProceduralModels;
using VL.Core;
using VL.Lib.Collections;

namespace Vl.Stride.BezierPatch
{
    public record BezierPatch
    {
        public Mesh BezierMesh { get; private set; }
        public Spread<Vector2> HelperPoints { get; private set; }

        public BezierPatch(IServiceRegistry serviceRegistry, Spread<Vector2> controlPoints, Int2 controlPointsResolution, Int2 gridResolution)
        {
            var bezierPatchGeometry = new BezierPatchGeometry(controlPointsResolution, gridResolution, controlPoints);

            var model = new Model();
            bezierPatchGeometry.Generate(serviceRegistry, model);

            BezierMesh = model.Meshes[0];
            HelperPoints = bezierPatchGeometry.HelperPoints.ToSpread();
        }
    }

    public class BezierPatchGeometry : PrimitiveProceduralModelBase
    {
        private Int2 _controlPointsResolution;
        private Int2 _gridResolution;
        private Spread<Vector2> _controlPoints;

        public List<Vector2> HelperPoints { get; private set; }

        public BezierPatchGeometry(Int2 controlPointsResolution, Int2 gridResolution, Spread<Vector2> controlPoints) : base()
        {
            _controlPointsResolution = controlPointsResolution;
            _gridResolution = gridResolution;
            _controlPoints = controlPoints;
        }

        protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData()
        {
            int resX = _gridResolution.X;
            int resY = _gridResolution.Y;

            int cresX = _controlPointsResolution.X;
            int cresY = _controlPointsResolution.Y;


            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<int>();
            HelperPoints = new List<Vector2>();


            float sx = 0.5f;
            float sy = 0.5f;

            float ix = (sx / Convert.ToSingle(resX - 1)) * 2.0f;
            float iy = (sy / Convert.ToSingle(resY - 1)) * 2.0f;

            float y = -sy;

            List<Vector3> ctrls = new List<Vector3>();

            float mx = -0.5f;
            float my = 0.5f;

            float incX = 1.0f / ((float)cresX - 1.0f);
            float incY = 1.0f / ((float)cresY - 1.0f);

            int inch = 0;

            for (int ct = 0; ct < cresX * cresY; ct++)
            {
                var controlPoint = _controlPoints[ct];
                ctrls.Add(new Vector3(controlPoint.X, controlPoint.Y, 0.0f));
            }

            for (int ct = 0; ct < cresX * cresY; ct++)
            {
                var controlPoint = _controlPoints[ct];
                ctrls.Add(new Vector3(controlPoint.X, controlPoint.Y, 0.0f));

                HelperPoints.Add(new Vector2(controlPoint.X + mx, controlPoint.Y + my));

                mx += incX;

                inch++;

                if (inch == cresX)
                {
                    inch = 0;
                    mx = -0.5f;
                    my -= incY;
                }
            }

            Vector3[] carr = new Vector3[ctrls.Count];


            for (int i = 0; i < resY; i++)
            {
                float x = -sx;
                for (int j = 0; j < resX; j++)
                {
                    var vertex = new VertexPositionNormalTexture();

                    float tu = VLMath.Map(j, 0, resX - 1, 0.0f, 1.0f, VL.Core.MapMode.Clamp);
                    float tb = VLMath.Map(i, 0, resY - 1, 1.0f, 0.0f, VL.Core.MapMode.Clamp);

                    vertex.Normal = new Vector3(0.0f, 0.0f, 1.0f);
                    vertex.TextureCoordinate = new Vector2(tu, tb);

                    float[] bu = BernsteinBasis.ComputeBasis(cresX - 1, tu);
                    float[] bv = BernsteinBasis.ComputeBasis(cresY - 1, tb);

                    for (int ck = 0; ck < ctrls.Count; ck++)
                    {
                        carr[ck].X = x + ctrls[ck].X;
                        carr[ck].Y = y + ctrls[ck].Y;
                    }

                    Vector3 vp = EvaluateBezier(carr, bu, bv, cresX, cresY);

                    vertex.Position = vp;

                    vertices.Add(vertex);

                    x += ix;
                }

                y += iy;
            }

            for (int j = 0; j < resY - 1; j++)
            {
                int rowlow = (j * resX);
                int rowup = ((j + 1) * resX);

                for (int i = 0; i < resX - 1; i++)
                {
                    int col = i * (resX - 1);

                    indices.Add(0 + rowlow + i);
                    indices.Add(0 + rowup + i);
                    indices.Add(1 + rowlow + i);

                    indices.Add(1 + rowup + i);
                    indices.Add(1 + rowlow + i);
                    indices.Add(0 + rowup + i);
                }
            }

            return new GeometricMeshData<VertexPositionNormalTexture>(vertices.ToArray(), indices.ToArray(), isLeftHanded: false) { Name = "BezierPatch" };
        }
        private Vector3 EvaluateBezier(Vector3[] verts, float[] BasisU, float[] BasisV, int cx, int cy)
        {
            Vector3 Value = Vector3.Zero;
            int cnt = 0;
            for (int i = 0; i < cy; i++)
            {
                Vector3 vl = Vector3.Zero;
                for (int j = 0; j < cx; j++)
                {
                    vl += verts[cnt] * BasisU[j];
                    cnt++;
                }
                vl *= BasisV[i];

                Value += vl;
            }
            return Value;
        }
    }
}