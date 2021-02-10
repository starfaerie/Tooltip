using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RectangleTexture
{
    public static Texture2D CreateRoundedRectangleTexture(int resolutionMultiplier, int width, int height, int borderThickness, int borderRadius, int borderShadow, List<Color32> backgroundColors, List<Color32> borderColors, float initialShadowIntensity, float finalShadowIntensity)
    {
        if (backgroundColors == null || backgroundColors.Count == 0) throw new ArgumentException("Must define at least one background color (up to four)."); 
        if (borderColors == null || borderColors.Count == 0) throw new ArgumentException("Must define at least one border color (up to three)."); 
        if (borderRadius < 1) throw new ArgumentException("Must define a border radius (rounds off edges)."); 
        if (borderThickness < 1) throw new ArgumentException("Must define border thickness."); 
        if (borderThickness + borderRadius > height / 2 || borderThickness + borderRadius > width / 2) throw new ArgumentException("Border will be too thick and/or rounded to fit on the texture."); 
        if (borderShadow > borderRadius) throw new ArgumentException("Border shadow must be lesser in magnitude than the border radius (suggested: shadow <= 0.25 * radius).");

        width *= resolutionMultiplier;
        height *= resolutionMultiplier;

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        Color32[] color = new Color32[width * height];

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                switch (backgroundColors.Count)
                {
                    case 4:
                        Color32 leftColor0 = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                        Color32 rightColor0 = Color32.Lerp(backgroundColors[2], backgroundColors[3], ((float)y / (height - 1)));
                        color[x + width * y] = Color32.Lerp(leftColor0, rightColor0, ((float)x / (width - 1)));
                        break;
                    case 3:
                        Color32 leftColor1 = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)y / (width - 1)));
                        Color32 rightColor1 = Color32.Lerp(backgroundColors[1], backgroundColors[2], ((float)y / (height - 1)));
                        color[x + width * y] = Color32.Lerp(leftColor1, rightColor1, ((float)x / (width - 1)));
                        break;
                    case 2:
                        color[x + width * y] = Color32.Lerp(backgroundColors[0], backgroundColors[1], ((float)x / (width - 1)));
                        break;
                    default:
                        color[x + width * y] = backgroundColors[0];
                        break;
                }

                color[x + width * y] = ColorBorder(x, y, width, height, borderThickness, borderRadius, borderShadow, color[x + width * y], borderColors, initialShadowIntensity, finalShadowIntensity);
            }
        }

        texture.SetPixels32(color);
        texture.Apply();
        return texture;
    }

    private static Color32 ColorBorder(int x, int y, int width, int height, int borderThickness, int borderRadius, int borderShadow, Color32 initialColor, List<Color32> borderColors, float initialShadowIntensity, float finalShadowIntensity)
    {
        Rect internalRectangle = new Rect((borderThickness + borderRadius), (borderThickness + borderRadius), width - 2 * (borderThickness + borderRadius), height - 2 * (borderThickness + borderRadius));


        Vector2 point = new Vector2(x, y);
        if (internalRectangle.Contains(point)) return initialColor;

        Vector2 origin = Vector2.zero;

        if (x < borderThickness + borderRadius)
        {
            if (y < borderRadius + borderThickness)
                origin = new Vector2(borderRadius + borderThickness, borderRadius + borderThickness);
            else if (y > height - (borderRadius + borderThickness))
                origin = new Vector2(borderRadius + borderThickness, height - (borderRadius + borderThickness));
            else
                origin = new Vector2(borderRadius + borderThickness, y);
        }
        else if (x > width - (borderRadius + borderThickness))
        {
            if (y < borderRadius + borderThickness)
                origin = new Vector2(width - (borderRadius + borderThickness), borderRadius + borderThickness);
            else if (y > height - (borderRadius + borderThickness))
                origin = new Vector2(width - (borderRadius + borderThickness), height - (borderRadius + borderThickness));
            else
                origin = new Vector2(width - (borderRadius + borderThickness), y);
        }
        else
        {
            if (y < borderRadius + borderThickness)
                origin = new Vector2(x, borderRadius + borderThickness);
            else if (y > height - (borderRadius + borderThickness))
                origin = new Vector2(x, height - (borderRadius + borderThickness));
        }

        if (origin.Equals(Vector2.zero)) return initialColor;
        float distance = Vector2.Distance(point, origin);

        if (distance > borderRadius + borderThickness + 1)
        {
            return Color.clear;
        }
        else if (distance > borderRadius + 1)
        {
            if (borderColors.Count > 2)
            {
                float modNum = distance - borderRadius;

                return modNum < borderThickness * 0.5f ? Color32.Lerp(borderColors[2], borderColors[1], (float)((modNum) / (borderThickness / 2.0))) : Color32.Lerp(borderColors[1], borderColors[0], (float)((modNum - (borderThickness / 2.0)) / (borderThickness / 2.0)));
            }


            if (borderColors.Count > 0)
                return borderColors[0];
        }
        else if (distance > borderRadius - borderShadow + 1)
        {
            float mod = (distance - (borderRadius - borderShadow)) / borderShadow;
            float shadowDiff = initialShadowIntensity - finalShadowIntensity;
            return DarkenColor(initialColor, ((shadowDiff * mod) + finalShadowIntensity));
        }

        return initialColor;
    }

    private static Color32 DarkenColor(Color32 color, float shadowIntensity)
    {
        return Color32.Lerp(color, Color.black, shadowIntensity);
    }
    
    public static void ProcessTexture(Texture2D texture)
    { 
        const string path = "Assets/Tooltip/";
        string dirDataPath = Application.dataPath + path;
        if(!Directory.Exists(dirDataPath)) {
            Directory.CreateDirectory(dirDataPath);
        }
        SaveTextureAsPNG(texture,dirDataPath);
         
         TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(dirDataPath);

         if (!(importer is null))
         {
             importer.isReadable = true;
             importer.textureType = TextureImporterType.Sprite;
             importer.spriteImportMode = SpriteImportMode.Multiple;
             importer.mipmapEnabled = false;
             importer.filterMode = FilterMode.Point;
             importer.spritePivot = Vector2.down;
             importer.textureCompression = TextureImporterCompression.Uncompressed;

             TextureImporterSettings textureSettings =
                 new TextureImporterSettings(); // need this class because spriteExtrude and spriteMeshType aren't exposed on TextureImporter
             importer.ReadTextureSettings(textureSettings);
             textureSettings.spriteMeshType = SpriteMeshType.Tight;
             textureSettings.spriteExtrude = 0;

             importer.SetTextureSettings(textureSettings);

             const int minimumSpriteSize = 16;
             const int extrudeSize = 0;

             Rect[] rects =
                 InternalSpriteUtility.GenerateAutomaticSpriteRectangles(texture, minimumSpriteSize, extrudeSize);
             List<Rect> rectsList = new List<Rect>(rects);
             rectsList = SortRects(rectsList, texture.width);

             string filenameNoExtension = Path.GetFileNameWithoutExtension(dirDataPath);
             List<SpriteMetaData> metas = new List<SpriteMetaData>();
             int rectNum = 0;

             foreach (Rect rect in rectsList)
             {
                 SpriteMetaData meta = new SpriteMetaData
                 {
                     pivot = Vector2.down,
                     alignment = (int) SpriteAlignment.BottomCenter,
                     rect = rect,
                     name = filenameNoExtension + "_" + rectNum++
                 };
                 metas.Add(meta);
             }

             importer.spritesheet = metas.ToArray();
         }

         AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
         
         void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
         {
             const string saveName = "rectangle.png";
             string fullDataPath = _fullPath + saveName;
             
             byte[] bytes =_texture.EncodeToPNG();

             using (FileStream fileStream =
                 new FileStream(fullDataPath, FileMode.Create))
             {
                 BinaryWriter writer = new BinaryWriter(fileStream);
                 writer.Write(bytes);
                 fileStream.Close(); //not necessary since we're in a using block
             } //end of using block disposes of the fileStream
             Debug.Log(bytes.Length/1024  + "Kb was saved as: " + fullDataPath);
         }
     }

    private static List<Rect> SortRects(List<Rect> rects, float textureWidth)
     {
         List<Rect> list = new List<Rect>();
         while (rects.Count > 0)
         {
             Rect rect = rects[rects.Count - 1];
             Rect sweepRect = new Rect(0f, rect.yMin, textureWidth, rect.height);
             List<Rect> list2 = RectSweep(rects, sweepRect);
             if (list2.Count <= 0)
             {
                 list.AddRange(rects);
                 break;
             }
             list.AddRange(list2);
         }
         return list;
     }

    private static List<Rect> RectSweep(List<Rect> rects, Rect sweepRect)
     {
         List<Rect> result;
         if (rects == null || rects.Count == 0)
         {
             result = new List<Rect>();
         }
         else
         {
             List<Rect> list = new List<Rect>();
             foreach (Rect current in rects)
             {
                 if (current.Overlaps(sweepRect))
                 {
                     list.Add(current);
                 }
             }
             foreach (Rect current2 in list)
             {
                 rects.Remove(current2);
             }
             list.Sort((a, b) => a.x.CompareTo(b.x));
             result = list;
         }
         return result;
     }
}