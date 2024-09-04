/*
Source: https://github.com/mrvux/FeralTic/blob/master/Core/Core/Maths/BernsteinBasis.cs

License:

FeralTic

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

namespace Vl.Stride.BezierPatch
{
    public class BernsteinBasis
    {
        public static float[] ComputeBasis(int degree, float t)
        {
            float[] result = new float[degree + 1];

            int[] coeff = GetBinomial(degree);

            float invT = 1.0f - t;

            for (int i = 0; i < degree + 1; i++)
            {
                double res = coeff[i] * Math.Pow(invT, degree - i) * Math.Pow(t, i);

                result[i] = (float)res;
            }

            return result;
        }

        private static int[] GetBinomial(int degree)
        {
            int[] res = new int[1] { 1 };

            for (int j = 1; j < degree + 1; j++)
            {
                int[] curr = new int[res.Length + 1];
                for (int i = 0; i < res.Length + 1; i++)
                {
                    if (i == 0)
                    {
                        curr[i] = 1;
                    }
                    else
                    {
                        if (i == res.Length)
                        {
                            curr[i] = 1;
                        }
                        else
                        {
                            curr[i] = res[i - 1] + res[i];
                        }
                    }
                }
                res = curr;
            }
            return res;
        }
    }
}
