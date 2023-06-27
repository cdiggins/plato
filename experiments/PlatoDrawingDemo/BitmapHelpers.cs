using System.Drawing.Imaging;

namespace PlatoDrawingDemo;

public static unsafe class BitmapHelpers
{
    public const int BytesPerPixel = 4;

    public static byte[] GetBytes(Bitmap bmp)
    {
        var r = new byte[bmp.Width * bmp.Height * BytesPerPixel];
        fixed (byte* ptr = r)
            CopyDataFromBitmap(bmp,ptr);
        return r;
    }

    public static int GetNumBytes(Bitmap bmp)
        => bmp.Width * bmp.Height * BytesPerPixel;

    public static void CopyData(byte* srcPointer, byte* dstPointer, int length)
        => Buffer.MemoryCopy(srcPointer, dstPointer, length, length);

    public static void CopyDataFromBitmap(Bitmap bitmap, byte* dstPointer)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var srcData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            CopyData((byte*)srcData.Scan0, dstPointer, GetNumBytes(bitmap));
        }
        finally
        {
            bitmap.UnlockBits(srcData);
        }
    }

    public static void CopyDataIntoBitmap(Bitmap bitmap, byte[] bytes)
    {
        fixed (byte* ptr = bytes)
        {
            CopyDataIntoBitmap(bitmap, ptr);
        }
    }

    public static void CopyDataIntoBitmap(Bitmap bitmap, byte* srcPointer)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var dstData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            CopyData(srcPointer, (byte*)dstData.Scan0, GetNumBytes(bitmap));
        }
        finally
        {
            bitmap.UnlockBits(dstData);
        }
    }
}