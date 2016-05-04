/*
 * Copyright 2016 Dirk Weltz
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace org.mapsforge.provider.graphics
{
    using org.mapsforge.core.graphics;
    using SkiaSharp;

    public class SkiaMatrix : Matrix
    {
        private SKMatrix nativeMatrix;

        public SkiaMatrix()
        {
            nativeMatrix = new SKMatrix();
        }

        public void Reset()
        {
            nativeMatrix = SKMatrix.MakeIdentity();
        }

        public void Rotate(float theta)
        {
            nativeMatrix = SKMatrix.MakeRotation(theta);
        }

        public void Rotate(float theta, float pivotX, float pivotY)
        {
            // TODO
            //nativeMatrix = SKMatrix.MakeRotation(theta, pivotX, pivotY);
        }

        public void Scale(float scaleX, float scaleY)
        {
            nativeMatrix = SKMatrix.MakeScale(scaleX, scaleY);
        }

        public void Scale(float scaleX, float scaleY, float pivotX, float pivotY)
        {
            nativeMatrix = SKMatrix.MakeScale(scaleX, scaleY, pivotX, pivotY);
        }

        public void Translate(float translateX, float translateY)
        {
            nativeMatrix = SKMatrix.MakeTranslation(translateX, translateY);
        }
    }
}
