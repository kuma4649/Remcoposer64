using System;
using System.Collections.Generic;
using System.Text;
using Vortice.Direct2D1;

namespace Remcoposer64.Common.Render
{
    public sealed class ImageResource : IDisposable
    {
        public ID2D1Bitmap Bitmap { get; }

        public Dictionary<int, Rectangle> SourceRects { get; }

        public ImageResource(
            ID2D1Bitmap bitmap,
            Dictionary<int, Rectangle> sourceRects)
        {
            Bitmap = bitmap;
            this.SourceRects = sourceRects;
        }

        public Rectangle GetRect(int characterID)
        {
            return SourceRects[characterID];
        }

        public bool TryGetRect(
            int characterID,
            out Rectangle rect)
        {
            return SourceRects.TryGetValue(
                characterID,
                out rect);
        }

        public void Dispose()
        {
            Bitmap?.Dispose();
        }
    }
}
