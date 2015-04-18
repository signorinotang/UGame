using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class BmpParser
{

    public class BITMAPFILEHEADER
    {
        public short bfType = 0;
        public UInt32 bfSize = 0;
        public UInt32 bfReserved1 = 0;
        public UInt32 bfOffBits = 0;
    };

    public class BITMAPINFOHEADER
    {
        public UInt32 biSize = 0;
        public Int32 biWidth = 0;
        public Int32 biHeight = 0;
        public short biPlanes = 0;
        public short biBitCount = 0;
        public Int32 biCompression = 0;
        public UInt32 biSizeImage = 0;
        public Int32 biXPelsPerMeter = 0;
        public Int32 biYPelsPerMeter = 0;
        public UInt32 biClrUsed = 0;
        public UInt32 biClrImportant = 0;
    }

    static Int32 CalculateUsedPaletteEntries(Int32 bit_count)
    {
	    if ((bit_count >= 1) && (bit_count <= 8))
		    return 1 << bit_count;
	    return 0;
    }

    public static UnityEngine.Texture2D ParseBmpDataToTexture2D(byte[] bmpBytes)
    {

        tsf4g_tdr_csharp.TdrReadBuf buff = new tsf4g_tdr_csharp.TdrReadBuf(ref bmpBytes, bmpBytes.Length);
        buff.disableEndian();
        BITMAPFILEHEADER FileHeader = new BITMAPFILEHEADER();

        buff.readInt16(ref FileHeader.bfType);
        buff.readUInt32(ref FileHeader.bfSize);
        buff.readUInt32(ref FileHeader.bfReserved1);
        buff.readUInt32(ref FileHeader.bfOffBits);


        BITMAPINFOHEADER InfoHeader = new BITMAPINFOHEADER();

        buff.readUInt32(ref InfoHeader.biSize);
        buff.readInt32(ref InfoHeader.biWidth);
        buff.readInt32(ref InfoHeader.biHeight);
        buff.readInt16(ref InfoHeader.biPlanes);
        buff.readInt16(ref InfoHeader.biBitCount);
        buff.readInt32(ref InfoHeader.biCompression);
        buff.readUInt32(ref InfoHeader.biSizeImage);
        buff.readInt32(ref InfoHeader.biXPelsPerMeter);
        buff.readInt32(ref InfoHeader.biYPelsPerMeter);
        buff.readUInt32(ref InfoHeader.biClrUsed);
        buff.readUInt32(ref InfoHeader.biClrImportant);

        Texture2D texture = new Texture2D(InfoHeader.biWidth, InfoHeader.biHeight, TextureFormat.RGB24, false);


        if (InfoHeader.biWidth <= 0 || InfoHeader.biHeight <= 0)
        {
            //use unity default image
            texture.LoadImage(bmpBytes);
            return texture;
        }

        Color32[] cols = new Color32[InfoHeader.biWidth * InfoHeader.biHeight];

        switch (InfoHeader.biBitCount)
        {
            case 1:
            case 4:
            case 8:
                {
                    if ((InfoHeader.biClrUsed == 0) || (InfoHeader.biClrUsed > CalculateUsedPaletteEntries(InfoHeader.biBitCount)))
                        InfoHeader.biClrUsed = (uint)CalculateUsedPaletteEntries(InfoHeader.biBitCount);

                    // allocate enough memory to hold the bitmap (header, palette, pixels) and read the palette

                    Color32[] pal = new Color32[InfoHeader.biClrUsed];

                    for (int i = 0; i < InfoHeader.biClrUsed; i ++)
                    {
						int index = 54 + i * 4;
                        pal[i].b = bmpBytes[index];
                        pal[i].g = bmpBytes[index + 1];
                        pal[i].r = bmpBytes[index + 2];
                        pal[i].a = 255;
                    }


                    int fileEnd = Mathf.Min((int)(FileHeader.bfOffBits + cols.Length), (int)FileHeader.bfSize);
                    int j = 0;

                    if (InfoHeader.biBitCount == 1)
                    {
                        //TODO
                        texture.LoadImage(bmpBytes);
                        return texture;
                    }
                    else if (InfoHeader.biBitCount == 4)
                    {
                       
                        for (int i = (int)FileHeader.bfOffBits; i < fileEnd; i++)
                        {
							byte palIndex = bmpBytes[i];
                            cols[j++] = pal[(palIndex >> 4) & 0xF];					
                            cols[j++] = pal[palIndex & 0xF];
                        }
                    }
                    else
                    {
                        for (int i = (int)FileHeader.bfOffBits; i < fileEnd; )
                        {
                            cols[j++] = pal[bmpBytes[i++]];					
                        }				
                    }
                }
                break;
            case 16:
                {
                    int j = 0;
                    int fileEnd = Mathf.Min((int)(FileHeader.bfOffBits + cols.Length * 2), (int)FileHeader.bfSize);
                    for (int i = (int)FileHeader.bfOffBits; i < fileEnd; i += 2)
                    {
                        short rgb0555 = (short)(bmpBytes[i] | (bmpBytes[i + 1] << 8));
                        cols[j].r = (byte)(((rgb0555 >> 10) & 0x1F) << 3);
                        cols[j].g = (byte)(((rgb0555 >> 5) & 0x1F) << 3);
                        cols[j].b = (byte)(((rgb0555) & 0x1F) << 3);
                        cols[j++].a = 255;						
                    }
                }
                break;
            case 24:
                {
                    int j = 0;
                    int fileEnd = Mathf.Min((int)(FileHeader.bfOffBits + cols.Length * 3), (int)FileHeader.bfSize);
                    for (int i = (int)FileHeader.bfOffBits; i < fileEnd; )
                    {
                        cols[j].b = bmpBytes[i++];
                        cols[j].g = bmpBytes[i++];
                        cols[j].r = bmpBytes[i++];
                        cols[j++].a = 255;
                    }
                    break;
                }
			case 32:
                {
                    int j = 0;
                    int fileEnd = Mathf.Min((int)(FileHeader.bfOffBits + cols.Length * 4), (int)FileHeader.bfSize);
                    for (int i = (int)FileHeader.bfOffBits; i < fileEnd; )
                    {
                        cols[j].b = bmpBytes[i++];
                        cols[j].g = bmpBytes[i++];
                        cols[j].r = bmpBytes[i++];
                        cols[j++].a = bmpBytes[i++];
                    }
                    break;
                }
            default:
                texture.LoadImage(bmpBytes);
                return texture;
        }

        texture.SetPixels32(cols);
        texture.Apply(false, true);
        return texture;

    }
}
