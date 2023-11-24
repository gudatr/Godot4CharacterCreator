using Godot;
using System;
using System.Threading.Tasks;

namespace GCC
{

    public partial class CharacterBase
    {
        /// <summary>
        /// This class encapsulates clothes applied to a character.
        /// It also takes care of generating the necessary atlas textures.
        /// </summary>
        private class MeshContainer
        {
            public CharacterBase character;

            public string name;
            public Vector3[] vertices;
            public MeshInstance3D instance;
            public Godot.Collections.Array meshData;
            public bool wasAltered;
            public bool isGenerating;
            public bool atlasIsGenerating;
            public StandardMaterial3D material;

            private StandardMaterial3D atlasMaterial;
            private ImageTexture atlasAlbedo;
            private ImageTexture atlasNormal;
            private Image atlasNormalImage;
            private Image atlasAlbedoImage;
            public readonly System.Collections.Generic.Dictionary<string, Overlay> overlays = new();

            /// <summary>
            /// The size of the texture atlases generated for this container
            /// </summary>
            public int AtlasSize { get; private set; } = 2048;

            /// <summary>
            /// Adds a new overlay texture from a <paramref name="material"/>. It will be given the alias <paramref name="overlayName"/> to adress it.
            /// </summary>
            /// <param name="overlayName"></param>
            /// <param name="material"></param>
            /// <param name="regenerateAtlas"></param>
            /// <returns></returns>
            public async Task AddOrReplaceOverlay(string overlayName, StandardMaterial3D material, bool regenerateAtlas = true)
            {
                overlays[overlayName] = new Overlay()
                {
                    material = material,
                    color = material.AlbedoColor,
                    albedo = material.AlbedoTexture,
                    normal = material.NormalTexture,
                };

                if (regenerateAtlas) await RegenerateAtlas();
            }

            /// <summary>
            /// Adds a new overlay texture from <paramref name="resPath"/>. It will be given the alias <paramref name="overlayName"/> to adress it.
            /// </summary>
            /// <param name="overlayName"></param>
            /// <param name="resPath"></param>
            /// <param name="regenerateAtlas"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public async Task AddOrReplaceOverlay(string overlayName, string resPath, bool regenerateAtlas = true)
            {
                var material = ResourceLoader.Load(resPath) as StandardMaterial3D ?? throw new Exception("Could not find overlay at path " + resPath);

                overlays[overlayName] = new Overlay()
                {
                    material = material,
                    color = material.AlbedoColor,
                    albedo = material.AlbedoTexture,
                    normal = material.NormalTexture,
                };

                if (regenerateAtlas) await RegenerateAtlas();
            }

            /// <summary>
            /// Changes the base material to the one from <paramref name="resPath"/>
            /// </summary>
            public async Task<bool> ChangeBaseMaterial(string resPath, bool regenerateAtlas = true)
            {
                var material = ResourceLoader.Load<StandardMaterial3D>(resPath);

                if (material == null) return false;

                if (material == this.material) return true;

                this.material = material;

                instance.MaterialOverride = material;

                if (regenerateAtlas) await RegenerateAtlas();

                return true;
            }

            /// <summary>
            /// Remove an overlay that uses the alias <paramref name="overlayName"/>.
            /// </summary>
            public async Task<bool> RemoveOverlay(string overlayName, bool regenerateAtlas = true)
            {
                var removed = overlays.Remove(overlayName);

                if (removed && regenerateAtlas) await RegenerateAtlas();

                return removed;
            }

            /// <summary>
            /// Removes parts of the <paramref name="albedo"/> and <paramref name="normal"/> images based on the <paramref name="bodyPartsHidden"/> configuration.
            /// The mapping for this is based on the body_parts texture.
            /// </summary>
            private void CutOutBodyPartsHidden(Image albedo, Image normal, BodyPartsHidden bodyPartsHidden)
            {
                if (bodyPartsHidden == BodyPartsHidden.None || albedo == null && normal == null) return;

                var scaleFactor = AtlasSize / 256;

                var noColor = new Color(0f, 0f, 0f, 0f);

                var removeImageAlbedo = Image.Create(scaleFactor, scaleFactor, false, albedo?.GetFormat() ?? Image.Format.Rgba8);
                var removeImageNormal = Image.Create(scaleFactor, scaleFactor, false, normal?.GetFormat() ?? Image.Format.Rgba8);

                for (var x = 0; x < scaleFactor; x++)
                {
                    for (var y = 0; y < scaleFactor; y++)
                    {
                        removeImageAlbedo.SetPixel(x, y, noColor);
                        removeImageNormal.SetPixel(x, y, noColor);
                    }
                }

                var sourceRect = new Rect2I(new Vector2I(0, 0), new Vector2I(scaleFactor, scaleFactor));

                for (var x = 0; x < 256; x++)
                {
                    for (var y = 0; y < 256; y++)
                    {

                        bool headHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Head) && hiddenPixelsHead[x, y];
                        bool torsoHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Torso) && hiddenPixelsTorso[x, y];
                        bool armsHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Arms) && hiddenPixelsArms[x, y];
                        bool handsHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Hands) && hiddenPixelsHands[x, y];
                        bool legsHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Legs) && hiddenPixelsLegs[x, y];
                        bool feetHidden = bodyPartsHidden.HasFlag(BodyPartsHidden.Feet) && hiddenPixelsFeet[x, y];

                        bool hidden = headHidden || torsoHidden || armsHidden || handsHidden || legsHidden || feetHidden;

                        if (!hidden) continue;

                        var realX = x * scaleFactor;
                        var realY = y * scaleFactor;

                        albedo?.BlitRect(removeImageAlbedo, sourceRect, new Vector2I(realX, realY));
                    }
                }
            }

            /// <summary>
            /// Regenerates the atlas of the MeshContainer, applying overlays and scaling the resulting textures to AtlasSize.
            /// This is done on a thread. If the atlas generation is already in progress, the function will immediately return.
            /// </summary>
            public async Task RegenerateAtlas()
            {
                if (atlasIsGenerating || overlays.Count == 0 && (this != character.Body || character.BodyPartsHidden == BodyPartsHidden.None && atlasMaterial == null))
                {
                    instance?.SetDeferred("material_override", material);
                    return;
                }

                atlasIsGenerating = true;

                var finished = false; //Necessary so we don't get race conditions and wait forever

                System.Threading.ThreadPool.QueueUserWorkItem((_) =>
                {
                    var normal = material.NormalTexture;
                    var albedo = material.AlbedoTexture;

                    atlasMaterial = material.Duplicate() as StandardMaterial3D;

                    var atlasEdge = new Vector2I(0, 0);
                    var atlasRect = new Rect2I(atlasEdge, new Vector2I(AtlasSize, AtlasSize));

                    if (normal != null)
                    {
                        var baseImage = normal.GetImage();
                        baseImage.Decompress();
                        baseImage.Resize(AtlasSize, AtlasSize, Image.Interpolation.Lanczos);

                        if (atlasNormal == null)
                        {
                            atlasNormalImage = Image.CreateFromData(AtlasSize, AtlasSize, true, baseImage.GetFormat(), baseImage.GetData());
                            atlasNormal = ImageTexture.CreateFromImage(atlasNormalImage);
                        }

                        if (atlasNormal.GetHeight() != AtlasSize)
                        {
                            atlasNormalImage.Resize(AtlasSize, AtlasSize);
                        }

                        atlasNormalImage.BlitRect(baseImage, atlasRect, atlasEdge);

                        foreach (var overlay in overlays.Values)
                        {
                            if (overlay.normal != null)
                            {
                                var overlayImage = overlay.normal.GetImage();
                                overlayImage.Decompress();
                                overlayImage.Resize(AtlasSize, AtlasSize, Image.Interpolation.Lanczos);
                                atlasNormalImage.BlendRect(overlayImage, atlasRect, atlasEdge);
                            }
                        }

                    }

                    if (albedo != null)
                    {
                        var baseImage = albedo.GetImage();
                        baseImage.Decompress();
                        baseImage.Resize(AtlasSize, AtlasSize, Image.Interpolation.Lanczos);

                        if (atlasAlbedo == null)
                        {
                            atlasAlbedoImage = Image.CreateFromData(AtlasSize, AtlasSize, true, baseImage.GetFormat(), baseImage.GetData());
                            atlasAlbedo = ImageTexture.CreateFromImage(atlasAlbedoImage);
                        }

                        if (atlasAlbedo.GetHeight() != AtlasSize)
                        {
                            atlasAlbedoImage.Resize(AtlasSize, AtlasSize);
                        }

                        atlasAlbedoImage.BlitRect(baseImage, atlasRect, atlasEdge);

                        foreach (var overlay in overlays.Values)
                        {
                            if (overlay.albedo != null)
                            {
                                var overlayImage = overlay.albedo.GetImage();
                                overlayImage.Decompress();
                                overlayImage.Resize(AtlasSize, AtlasSize, Image.Interpolation.Lanczos);
                                atlasAlbedoImage.BlendRect(overlayImage, atlasRect, atlasEdge);
                            }
                        }
                    }

                    if (this == character.Body)
                    {
                        CutOutBodyPartsHidden(atlasAlbedoImage, atlasNormalImage, character.BodyPartsHidden);
                    }

                    if (albedo != null)
                    {
                        atlasAlbedoImage.GenerateMipmaps();
                        atlasAlbedo.Update(atlasAlbedoImage);
                        atlasMaterial.AlbedoTexture = atlasAlbedo;
                    }

                    if (normal != null)
                    {
                        atlasNormalImage.GenerateMipmaps();
                        atlasNormal.Update(atlasNormalImage);
                        atlasMaterial.NormalEnabled = true;
                        atlasMaterial.NormalTexture = atlasNormal;
                    }

                    instance?.SetDeferred("material_override", atlasMaterial);

                    atlasIsGenerating = false;

                    finished = true;

                    GC.Collect(GC.MaxGeneration);
                });

                while (!finished)
                {
                    await character.ToSignal(character.GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }
        }
    }
}
