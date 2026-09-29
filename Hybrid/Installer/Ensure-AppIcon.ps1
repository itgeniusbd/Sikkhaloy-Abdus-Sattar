# Builds a Windows-friendly .ico (BMP 16/32/48 + PNG 256) from app-logo.png.
# PNG-only .ico files look like the generic exe icon in Explorer and Inno Setup.
$ErrorActionPreference = "Stop"

$assets = Join-Path (Split-Path -Parent $PSScriptRoot) "src\Sikkhaloy.Client\Assets"
$pngPath = Join-Path $assets "app-logo.png"
$icoPath = Join-Path $assets "app.ico"
if (-not (Test-Path $pngPath)) { throw "Logo not found: $pngPath" }

Add-Type -AssemblyName System.Drawing
$code = @"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public static class HybridIcoWriter
{
    public static void Write(string pngPath, string icoPath)
    {
        using (var src = Image.FromFile(pngPath))
        {
            int[] sizes = { 16, 32, 48, 256 };
            var parts = new List<byte[]>();
            foreach (var size in sizes)
            {
                using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(src, 0, 0, size, size);
                    parts.Add(size >= 256 ? ToPng(bmp) : ToBmpDib(bmp));
                }
            }
            WriteIco(icoPath, sizes, parts);
        }
    }

    static byte[] ToPng(Bitmap bmp)
    {
        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
    }

    static byte[] ToBmpDib(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        var rect = new Rectangle(0, 0, w, h);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int stride = data.Stride;
        var px = new byte[stride * h];
        Marshal.Copy(data.Scan0, px, 0, px.Length);
        bmp.UnlockBits(data);

        int maskStride = ((w + 31) / 32) * 4;
        var xor = new byte[w * h * 4];
        var mask = new byte[maskStride * h];
        int di = 0;
        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = 0; x < w; x++)
            {
                int si = (y * stride) + (x * 4);
                xor[di++] = px[si];
                xor[di++] = px[si + 1];
                xor[di++] = px[si + 2];
                xor[di++] = px[si + 3];
                if (px[si + 3] < 128)
                {
                    int my = h - 1 - y;
                    mask[(my * maskStride) + (x / 8)] |= (byte)(0x80 >> (x % 8));
                }
            }
        }

        var header = new byte[40];
        BitConverter.GetBytes(40).CopyTo(header, 0);
        BitConverter.GetBytes(w).CopyTo(header, 4);
        BitConverter.GetBytes(h * 2).CopyTo(header, 8);
        BitConverter.GetBytes((short)1).CopyTo(header, 12);
        BitConverter.GetBytes((short)32).CopyTo(header, 14);
        BitConverter.GetBytes(xor.Length).CopyTo(header, 20);

        var result = new byte[header.Length + xor.Length + mask.Length];
        Buffer.BlockCopy(header, 0, result, 0, header.Length);
        Buffer.BlockCopy(xor, 0, result, header.Length, xor.Length);
        Buffer.BlockCopy(mask, 0, result, header.Length + xor.Length, mask.Length);
        return result;
    }

    static void WriteIco(string path, int[] sizes, List<byte[]> parts)
    {
        using (var fs = File.Create(path))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write((short)0);
            bw.Write((short)1);
            bw.Write((short)parts.Count);
            int offset = 6 + (16 * parts.Count);
            for (int i = 0; i < parts.Count; i++)
            {
                int s = sizes[i];
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((short)1);
                bw.Write((short)32);
                bw.Write(parts[i].Length);
                bw.Write(offset);
                offset += parts[i].Length;
            }
            foreach (var part in parts)
                bw.Write(part);
        }
    }
}
"@
Add-Type -TypeDefinition $code -ReferencedAssemblies System.Drawing

[HybridIcoWriter]::Write($pngPath, $icoPath)
Write-Host "Wrote Windows icon: $icoPath"
