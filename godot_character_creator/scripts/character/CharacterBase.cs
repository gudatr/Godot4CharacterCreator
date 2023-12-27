using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GCC
{

    public partial class CharacterBase : CharacterBody3D
    {
        private Godot.Collections.Dictionary<string, float> appearance = new();

        public Godot.Collections.Dictionary<string, float> Appearance
        {
            get
            {
                return appearance.Duplicate();
            }
            private set
            {
                appearance = value;
            }
        }

        private Skeleton3D Skeleton;

        private MeshContainer Body;

        public AnimationPlayer AnimationPlayer { get; private set; }

        /// <summary>
        /// The <c>CharacterExpressionPlayer</c> attached to the character.
        /// It manages expressions, head movement and speech.
        /// </summary>
        public CharacterExpressionPlayer ExpressionPlayer { get; private set; }

        private readonly List<MeshContainer> meshContainers = new();

        private bool meshContainersLocked = false;

        /// <summary>
        /// Specifies if debug output for transformations should be printed to Godot's console.
        /// </summary>
        public bool debug = false;

        /// <summary>
        /// Changes how many frames have to pass for the morphed mesh to be updated.
        /// Set this to 2 or higher to improve performance when morphing.
        /// Use 1 as a value if you want to realtime-morph the mesh, e.g. a character transforms or facial morphs, and the game's focus is on that.
        /// </summary>
        public int framesToUpdate = 2;

        private int updateIteration = 0;

        /// <summary>
        /// Specifies if the normals should be recalculated.
        /// Set this to false outside of character creation or focused morphing as it slows down mesh regeneration drastically.
        /// </summary>
        public bool updateNormals = true;

        public Sex Sex { get; private set; }
        public Ethnicity Ethnicity { get; private set; }
        public Age Age { get; private set; }
        public BodyPartsHidden BodyPartsHidden { get; private set; }

        private int atlasSize = 1024;
        public int AtlasSize
        {
            get
            {
                return atlasSize;
            }
            set
            {
                atlasSize = Mathf.Max(256, value);
            }
        }

        public bool autoGenerateAtlas = true;

        private static readonly System.Collections.Generic.Dictionary<Color24, BodyPartsHidden> bodyPartsMapping = new()
        {
            { new(1, 0, 0), BodyPartsHidden.Head },
            { new(0, 1, 0), BodyPartsHidden.Arms },
            { new(0, 0, 1), BodyPartsHidden.Hands },
            { new(1, 0, 1), BodyPartsHidden.Torso },
            { new(0, 1, 1), BodyPartsHidden.Legs },
            { new(1, 1, 0), BodyPartsHidden.Feet }
        };

        private static readonly Godot.Collections.Dictionary<string, StandardMaterial3D> bodyBaseMaterials = new();

        private static readonly bool[,] hiddenPixelsFeet = new bool[256, 256];
        private static readonly bool[,] hiddenPixelsLegs = new bool[256, 256];
        private static readonly bool[,] hiddenPixelsTorso = new bool[256, 256];
        private static readonly bool[,] hiddenPixelsHands = new bool[256, 256];
        private static readonly bool[,] hiddenPixelsArms = new bool[256, 256];
        private static readonly bool[,] hiddenPixelsHead = new bool[256, 256];

        /// <summary>
        /// This will create the hiddenPixels[...] boolean maps for cutting off body parts based on the meshes_for_generation/body_parts.png texture
        /// </summary>
        private void LoadHiddenPixelsForBodyParts()
        {
            var image = ResourceLoader.Load(CharacterData.resourceBasePath+"/meshes_for_generation/body_parts.png") as Texture2D;
            var imageData = image.GetImage();

            for (var x = 0; x < 256; x++)
            {
                for (var y = 0; y < 256; y++)
                {
                    var pixel = imageData.GetPixel(x, y);
                    var col = new Color24(pixel.R, pixel.G, pixel.B);

                    if (bodyPartsMapping.TryGetValue(col, out var val))
                    {
                        switch (val)
                        {
                            case BodyPartsHidden.Head:
                                hiddenPixelsHead[x, y] = true;
                                break;
                            case BodyPartsHidden.Torso:
                                hiddenPixelsTorso[x, y] = true;
                                break;
                            case BodyPartsHidden.Hands:
                                hiddenPixelsHands[x, y] = true;
                                break;
                            case BodyPartsHidden.Arms:
                                hiddenPixelsArms[x, y] = true;
                                break;
                            case BodyPartsHidden.Legs:
                                hiddenPixelsLegs[x, y] = true;
                                break;
                            case BodyPartsHidden.Feet:
                                hiddenPixelsFeet[x, y] = true;
                                break;
                        }
                    }
                }
            }
        }

        private class Overlay
        {
            public StandardMaterial3D material;
            public Color color;
            public Texture2D albedo;
            public Texture2D normal;
        }

        /// <summary>
        /// A wrapper function for enabling the character
        /// </summary>
        public void Enable()
        {
            SetProcess(true);
        }

        /// <summary>
        /// A wrapper function for disabling the character.
        /// Stops speaking immediately
        /// </summary>
        public void Disable()
        {
            ExpressionPlayer.StopSpeaking();
            SetProcess(false);
        }

        public async override void _Ready()
        {
            await SetUpBodyBaseMaterials();

            Skeleton = FindChild("skeleton") as Skeleton3D;

            ExpressionPlayer = Skeleton as CharacterExpressionPlayer;

            AnimationPlayer = FindChild("animation") as AnimationPlayer;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            await TakeOnClothes("body/body");
            await TakeOnClothes("body/eyes");
            await TakeOnClothes("body/tongue");
            await TakeOnClothes("body/teeth");
            await TakeOnClothes("body/eyelashes");

            ProcessPriority = 100;
        }

        public override void _Process(double delta)
        {
            if (++updateIteration < framesToUpdate) return;

            updateIteration = 0;

            try
            {
                foreach (var container in meshContainers)
                {
                    if (container.wasAltered)
                    {
                        SaveMesh(container);
                    }
                }
            }
            catch (Exception e)
            {
                //locking meshContainers unfortunately can lead to a deadlock on the main thread
                GD.Print(e);
            }

        }

        /// <summary>
        /// Changes the base material of a <paramref name="cloth"></paramref> to the one from <paramref name="resPath"></paramref>
        /// </summary>
        /// <param name="cloth"></param>
        /// <param name="resPath"></param>
        /// <returns></returns>
        public async Task<bool> ChangeBaseMaterial(string cloth, string resPath)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                foreach (var container in meshContainers)
                {
                    if (container.name == cloth)
                    {
                        await container.ChangeBaseMaterial(resPath, autoGenerateAtlas);
                        return true;
                    }
                }

                return false;

            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Adds a new overlay from a <paramref name="material"/> to a <paramref name="cloth"/>. It will be given the alias <paramref name="overlayName"/> to adress it.
        /// </summary>
        /// <param name="cloth"></param>
        /// <param name="overlayName"></param>
        /// <param name="material"></param>
        /// <returns></returns>
        public async Task<bool> AddOrReplaceOverlay(string cloth, string overlayName, StandardMaterial3D material)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                meshContainersLocked = true;

                foreach (var container in meshContainers)
                {
                    if (container.name == cloth)
                    {
                        await container.AddOrReplaceOverlay(overlayName, material, autoGenerateAtlas);
                        return true;
                    }
                }

                return false;

            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Adds a new overlay texture to a <paramref name="cloth"/> from <paramref name="resPath"/>. It will be given the alias <paramref name="overlayName"/> to adress it.
        /// </summary>
        public async Task<bool> AddOrReplaceOverlay(string cloth, string overlayName, string resPath)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                foreach (var container in meshContainers)
                {
                    if (container.name == cloth)
                    {
                        await container.AddOrReplaceOverlay(overlayName, resPath, autoGenerateAtlas);
                        return true;
                    }
                }

                return false;

            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Remove an overlay from a <paramref name="cloth"/> that uses the alias provided in <paramref name="overlayName"/>.
        /// </summary>
        public async Task<bool> RemoveOverlay(string cloth, string overlayName)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                foreach (var container in meshContainers)
                {
                    if (container.name == cloth)
                    {
                        return await container.RemoveOverlay(overlayName);
                    }
                }

                return false;
            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Applies a new <paramref name="appearance"/>. If <paramref name="reset"/> is set to true, the characters appearance will be reset before applying the new one.
        /// </summary>
        /// <param name="appearance"></param>
        /// <param name="reset"></param>
        /// <returns></returns>
        public async Task ApplyAppearance(Godot.Collections.Dictionary<string, float> appearance, bool reset = false)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                if (reset) await ResetAppearance(autoGenerateAtlas);

                var shapeNames = new string[appearance.Count];
                var values = new float[appearance.Count];

                int index = 0;

                foreach (var shapeNameAndValue in appearance)
                {
                    values[index] = shapeNameAndValue.Value;
                    shapeNames[index++] = shapeNameAndValue.Key;
                }

                UpdateMorphs(shapeNames, values);
            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Performs the transformation of the vertices in a <paramref name="meshContainer"/> using a blend shape at the specified <paramref name="shapeIndex"/> with the given <paramref name="value"/>
        /// </summary>
        /// <param name="meshContainer"></param>
        /// <param name="shapeIndex"></param>
        /// <param name="value"></param>
        private void UpdateVertex(MeshContainer meshContainer, int shapeIndex, float value)
        {
            var debugStopwatch = Stopwatch.StartNew();

            CharacterData.GetBlendshapeData(meshContainer.name, shapeIndex, out var indexes, out var data);

            if (indexes.Length == 0)
            {
                if (debug)
                {
                    GD.Print($"Skipped UpdateVertex on {meshContainer.name} because blend shape length is 0.");
                }
                return;
            }

            var length = data.Length;

            for (var i = 0; i < length; i++)
            {
                meshContainer.vertices[indexes[i]] += data[i] * value;
            }

            meshContainer.wasAltered = true;

            if (debug)
            {
                GD.Print($"Performed UpdateVertex on {meshContainer.name}, took {debugStopwatch.Elapsed.TotalMilliseconds} ms.");
            }
        }

        /// <summary>
        /// Creates a new mesh from the data in the <paramref name="meshContainer"/> on a thread.
        /// If there is already a thread generating the mesh, the function will immediately return.
        /// </summary>
        /// <param name="meshContainer"></param>
        private void SaveMesh(MeshContainer meshContainer)
        {
            if (meshContainer.isGenerating) return;

            meshContainer.isGenerating = true;

            System.Threading.ThreadPool.QueueUserWorkItem((_) =>
            {
                var debugStopwatch = Stopwatch.StartNew();

                ArrayMesh arrayMesh = new();
                meshContainer.meshData[(int)Mesh.ArrayType.Vertex] = meshContainer.vertices;
                if(updateNormals) meshContainer.meshData[(int)Mesh.ArrayType.Normal] = CalculateNormals(meshContainer.indexes, meshContainer.vertices);
                arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshContainer.meshData);
                if (updateNormals) arrayMesh.RegenNormalMaps();
                meshContainer.instance.SetDeferred("mesh", arrayMesh);

                meshContainer.wasAltered = false;
                meshContainer.isGenerating = false;

                if (debug)
                {
                    GD.Print($"Performed SaveMesh on {meshContainer.name} in thread, took {debugStopwatch.Elapsed.TotalMilliseconds} ms.");
                }
            });
        }

        private Vector3[] CalculateNormals(int[] indexes, Vector3[] vertices)
        {
            var length = vertices.Length;
            var indexLength = indexes.Length;
            var normals = new Vector3[length];

            for (int i = 0; i < indexLength; i += 3)
            {
                int vertexIndexA = indexes[i];
                int vertexIndexB = indexes[i + 1];
                int vertexIndexC = indexes[i + 2];

                Vector3 triangleNormal = SurfaceNormalFormIndices(vertexIndexA, vertexIndexB, vertexIndexC, vertices);

                normals[vertexIndexA] += triangleNormal;
                normals[vertexIndexB] += triangleNormal;
                normals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < length; i++)
            {
                normals[i] = normals[i].Normalized();
            }

            return normals;
        }

        private Vector3 SurfaceNormalFormIndices(int indexA, int indexB, int indexC, Vector3[] vertices)
        {
            var pointA = vertices[indexA];
            var pointB = vertices[indexB];
            var pointC = vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return sideAC.Cross(sideAB).Normalized();
        }


        /// <summary>
        /// Resets a character and loads a new appearance from a given <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task Load(string filePath)
        {
            var file = FileAccess.OpenCompressed(filePath, FileAccess.ModeFlags.Read);

            var dataArray = (Godot.Collections.Array)file.GetVar();

            var appearance = dataArray[0];
            var sex = dataArray[1];
            var age = dataArray[2];
            var ethnicity = dataArray[3];
            var bodyPartsHidden = dataArray[4];
            var clothes = (Godot.Collections.Dictionary<string, Array<string>>)dataArray[5];
            var bodyParts = (BodyPartsHidden)Enum.Parse(typeof(BodyPartsHidden), (string)bodyPartsHidden);

            var atlasGenerationSetting = autoGenerateAtlas;

            autoGenerateAtlas = false;

            foreach (var cloth in clothes)
            {
                var overlays = cloth.Value;
                var overlayCount = overlays.Count;

                await TakeOnClothes(cloth.Key, false);
                await ChangeBaseMaterial(cloth.Key, overlays[0]);

                for (var i = 1; i < overlayCount; i += 2)
                {
                    await AddOrReplaceOverlay(cloth.Key, overlays[i], overlays[i + 1]);
                }
            }

            await ApplyAppearance((Godot.Collections.Dictionary<string, float>)appearance, true);
            await SetSex((Sex)Enum.Parse(typeof(Sex), (string)sex), false);
            await SetAge((Age)Enum.Parse(typeof(Age), (string)age), false);
            await SetEthnicity((Ethnicity)Enum.Parse(typeof(Ethnicity), (string)ethnicity), false);
            HideBodyPartsAbsolute(bodyParts);

            autoGenerateAtlas = atlasGenerationSetting;

            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            Task[] regenerateAtlasTasks = new Task[meshContainers.Count];

            for (var i = 0; i < meshContainers.Count; i++)
            {
                regenerateAtlasTasks[i] = meshContainers[i].RegenerateAtlas();
            }

            await Task.WhenAll(regenerateAtlasTasks);
        }

        /// <summary>
        /// Saves the current appearance to a <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task Save(string filePath)
        {
            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                var clothes = new Godot.Collections.Dictionary<string, Array<string>>();

                foreach (var meshContainer in meshContainers)
                {
                    var overlays = new Array<string>
                {
                    meshContainer.material.ResourcePath
                };

                    foreach (var overlay in meshContainer.overlays)
                    {
                        overlays.Add(overlay.Key);
                        overlays.Add(overlay.Value.material.ResourcePath);
                    }

                    clothes[meshContainer.name] = overlays;
                }

                var dataArray = new Godot.Collections.Array
            {
                Appearance,
                Sex.ToString(),
                Age.ToString(),
                Ethnicity.ToString(),
                BodyPartsHidden.ToString(),
                clothes
            };

                var file = FileAccess.OpenCompressed(filePath, FileAccess.ModeFlags.Write, FileAccess.CompressionMode.Zstd);
                file.StoreVar(dataArray);
                file.Close();
            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Same as UpdateMorphs, with the difference being that it uses a single <paramref name="blendShape"/> as input.
        /// </summary>
        /// <param name="blendShape"></param>
        /// <param name="value"></param>
        public void UpdateMorph(string blendShape, float value)
        {
            UpdateMorphs(new[] { blendShape }, new[] { value });
        }

        /// <summary>
        /// Changes the appearance values of a character and applies transformations using the specified <paramref name="blendShapes"/> with the corresponding <paramref name="values"/>.
        /// </summary>
        /// <param name="blendShapes"></param>
        /// <param name="values"></param>
        public void UpdateMorphs(string[] blendShapes, float[] values)
        {
            foreach (MeshContainer meshContainer in meshContainers)
            {
                UpdateMeshContainer(blendShapes, values, meshContainer);
            }

            for (var i = 0; i < blendShapes.Length; i++)
            {
                var _appearance = appearance[blendShapes[i]] = values[i];
                if (_appearance == 0) appearance.Remove(blendShapes[i]);
            }
        }

        /// <summary>
        /// Update a MeshContainer using the specified <paramref name="blendShapes"/> and corresponding <paramref name="values"/>.
        /// <c>skipappearanceCheck</c> can be used for newly added meshes since their form they does not
        /// match the characters current appearance values
        /// </summary>
        /// <param name="blendShapes"></param>
        /// <param name="values"></param>
        /// <param name="meshContainer"></param>
        /// <param name="skipappearanceCheck"></param>
        private void UpdateMeshContainer(string[] blendShapes, float[] values, MeshContainer meshContainer, bool skipappearanceCheck = false)
        {
            var shapeIndexDict = CharacterData.ShapeNameIndexes[meshContainer.name];

            for (var i = 0; i < blendShapes.Length; i++)
            {
                var shapeName = blendShapes[i];
                var val = values[i];

                if (!shapeIndexDict.TryGetValue(shapeName, out var shapeIndex))
                {
                    continue;
                }

                if (!skipappearanceCheck && appearance.TryGetValue(shapeName, out var appearanceValue))
                {
                    val -= appearanceValue;
                }

                UpdateVertex(meshContainer, shapeIndex, val);
            }
        }

        /// <summary>
        /// Sets the blend shapes starting with "eye_", "cheek_", "head_", "jaw_", "nose_", "lips_" to random values from 0 to <paramref name="intensity"/>.
        /// The blend shapes <c>BlendShapes.HEAD_CIRCLE</c>, <c>BlendShapes.HEAD_OVAL</c>, <c>BlendShapes.HEAD_QUADRATIC</c>,
        /// <c>BlendShapes.HEAD_INVERSE_TRIANGLE</c>, <c>BlendShapes.HEAD_TRIANGLE</c> are skipped.
        /// </summary>
        /// <param name="intensity"></param>
        public void RandomFaceGen(float intensity)
        {
            var randomGenerator = new Random();
            var faceBlendShapeKeys = new Array<string>() { "eye_", "cheek_", "head_", "jaw_", "nose_", "lips_" };
            var blockedBlendShapeKeys = new Array<string>() { BlendShapes.HEAD_CIRCLE, BlendShapes.HEAD_OVAL, BlendShapes.HEAD_QUADRATIC, BlendShapes.HEAD_INVERSE_TRIANGLE, BlendShapes.HEAD_TRIANGLE };

            List<string> acceptableBlendShapeKeys = new();

            foreach (var blendShapeKey in MeshGeneration.BLEND_SHAPE_KEYS)
            {
                if (faceBlendShapeKeys.Any(blendShapeKey.StartsWith) && !blockedBlendShapeKeys.Contains(blendShapeKey))
                {
                    acceptableBlendShapeKeys.Add(blendShapeKey);
                }
            }

            var values = new float[acceptableBlendShapeKeys.Count];

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = randomGenerator.NextSingle() * intensity;
            }

            UpdateMorphs(acceptableBlendShapeKeys.ToArray(), values);
        }

        /// <summary>
        /// Returns an array containing the used cloth names, e.g. "body/body"
        /// </summary>
        public string[] GetClothes()
        {
            var clothes = new string[meshContainers.Count];
            for (var i = 0; i < clothes.Length; i++)
            {
                clothes[i] = meshContainers[i].name;
            }

            return clothes;
        }

        /// <summary>
        /// Take a <paramref name="cloth"/> off. Associated data will be removed.
        /// </summary>
        /// <param name="cloth"></param>
        /// <returns></returns>
        public async Task TakeOffClothes(string cloth)
        {

            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            try
            {
                foreach (var clothItem in meshContainers)
                {
                    if (clothItem.name == cloth)
                    {
                        clothItem.meshData = null;
                        clothItem.instance.QueueFree();
                        clothItem.instance = null;
                        meshContainers.Remove(clothItem);
                        CharacterData.UnloadCloth(cloth);
                        break;
                    }
                }
            }
            finally
            {
                meshContainersLocked = false;
            }
        }

        /// <summary>
        /// Take on a new <paramref name="cloth"/> and use a <paramref name="clothAlias"/> if one is specified. If it is already on the character it will be reset. This removes all overlays and custom materials. 
        /// </summary>
        /// <param name="cloth"></param>
        /// <param name="regenerateAtlas"></param>
        /// <param name="clothAlias"></param>
        /// <returns></returns>
        public async Task TakeOnClothes(string cloth, bool regenerateAtlas = true, string clothAlias = "")
        {
            await TakeOffClothes(cloth);

            while (meshContainersLocked)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            meshContainersLocked = true;

            var finished = false;

            System.Threading.ThreadPool.QueueUserWorkItem((_) =>
            {
                try
                {
                    var clothTaken = (PackedScene)ResourceLoader.Load(CharacterData.resourceBasePath+"/assets/meshes/" + (clothAlias == "" ? cloth : clothAlias) + ".tscn");

                    CharacterData.LoadCloth(cloth, debug);

                    var instance = clothTaken.Instantiate() as MeshInstance3D;
                    instance.Name = cloth.Replace("/", "_") + "@" + instance.GetInstanceId();
                    Skeleton.CallDeferred("add_child", instance);

                    var indexVertices = (int)Mesh.ArrayType.Vertex;
                    var indexIndexes = (int)Mesh.ArrayType.Index;
                    var meshData = instance.Mesh.SurfaceGetArrays(0);
                    var vertices = ((Array<Vector3>)meshData[indexVertices]).ToArray();
                    var indexes = ((Array<int>)meshData[indexIndexes]).ToArray();
                    var material = instance.GetActiveMaterial(0) as StandardMaterial3D;
                    meshData[indexVertices] = vertices;
                    meshData[indexIndexes] = indexes;

                    var container = new MeshContainer()
                    {
                        name = cloth,
                        instance = instance,
                        meshData = meshData,
                        vertices = vertices,
                        indexes = indexes,
                        material = material,
                        character = this
                    };

                    if (cloth == "body/body") Body = container;

                    _ = UpdateMaterial(regenerateAtlas);

                    foreach (var apperance in appearance)
                    {
                        UpdateMeshContainer(new[] { apperance.Key }, new[] { apperance.Value }, container, true);
                    }

                    meshContainers.Add(container);
                }
                finally
                {
                    meshContainersLocked = false;
                    finished = true;
                }
            });

            while (finished)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
        }

        /// <summary>
        /// Preloads the base materials for the "body/body" cloth and stores them in <c>bodyBaseMaterials</c> for direct access 
        /// </summary>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        private async Task SetUpBodyBaseMaterials(bool regenerateAtlas = true)
        {
            lock (bodyBaseMaterials)
            {
                if (bodyBaseMaterials.Count > 0) return;

                LoadHiddenPixelsForBodyParts();

                foreach (Sex iSex in Enum.GetValues(typeof(Sex)))
                {
                    foreach (Ethnicity iEthnicity in Enum.GetValues(typeof(Ethnicity)))
                    {
                        foreach (Age iAge in Enum.GetValues(typeof(Age)))
                        {
                            Age = iAge;
                            Sex = iSex;
                            Ethnicity = iEthnicity;

                            GetMaterialBaseStrings(out var ageString, out var ethnicityString, out var sexString);

                            var materialString = GetMaterialString();

                            if (!bodyBaseMaterials.TryGetValue(materialString, out StandardMaterial3D material))
                            {
                                var path = CharacterData.resourceBasePath+"/assets/materials/skin/" + sexString + "/" + ethnicityString + "_" + sexString + "_" + ageString + ".tres";
                                material = ResourceLoader.Load(path) as StandardMaterial3D;
                            }

                            bodyBaseMaterials[materialString] = material;
                        }
                    }
                }
            }

            Age = Age.Young;
            Sex = Sex.Female;
            Ethnicity = Ethnicity.Caucasian;

            await UpdateMaterial(regenerateAtlas);
        }

        /// <summary>
        /// Set the <paramref name="sex"/> of the character. This updates the material and sets the blend shapes
        /// <c>BlendShapes.SEX_WOMAN_1</c>, <c>BlendShapes.SEX_MAN</c> to the corresponding value.
        /// Set regenerateAtlas to false if you want to also set ethnicity or age after this.
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        public async Task SetSex(Sex sex, bool regenerateAtlas = true)
        {
            Sex = sex;

            UpdateMorphs(
                new[] { BlendShapes.SEX_WOMAN_1, BlendShapes.SEX_MAN },
                new[] {
                    sex == Sex.Female ? 0.75f : 0,
                    sex == Sex.Male ? 1f : 0
                });

            await UpdateMaterial(regenerateAtlas);
        }

        /// <summary>
        /// Resets all blend shapes. Sets Age to 'Young', Sex to 'Ambigous', Ethnicity to 'Caucasian'.
        /// </summary>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        public async Task ResetAppearance(bool regenerateAtlas = true)
        {
            var shapeNames = new string[appearance.Count];
            var values = new float[appearance.Count];

            int index = 0;

            foreach (var shape in appearance.Keys)
            {
                values[index] = 0;
                shapeNames[index++] = shape;
            }

            UpdateMorphs(shapeNames, values);

            await SetAge(Age.Young, false);
            await SetSex(Sex.Ambiguous, false);
            await SetEthnicity(Ethnicity.Caucasian, regenerateAtlas);
        }

        /// <summary>
        /// Set the <paramref name="ethnicity"/> of the character. This updates the material and sets the blend shapes
        /// BlendShapes.ETHNICITY_CAUCASIAN, BlendShapes.ETHNICITY_ASIAN, BlendShapes.ETHNICITY_AFRICAN to the corresponding value.
        /// Set regenerateAtlas to false if you want to also set age or sex after this.
        /// </summary>
        /// <param name="ethnicity"></param>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        public async Task SetEthnicity(Ethnicity ethnicity, bool regenerateAtlas = true)
        {
            Ethnicity = ethnicity;

            var values = new[] {
                ethnicity == Ethnicity.Caucasian ? 0.75f : 0.00f,
                ethnicity == Ethnicity.Asian ? 0.66f : 0.00f,
                ethnicity == Ethnicity.African ? 0.66f : 0.00f
            };

            UpdateMorphs(new[] { BlendShapes.ETHNICITY_CAUCASIAN, BlendShapes.ETHNICITY_ASIAN, BlendShapes.ETHNICITY_AFRICAN }, values);

            await UpdateMaterial(regenerateAtlas);
        }

        /// <summary>
        /// Removes the flags supplied in <paramref name="parts"/> from the BodyPartsHidden configuration of the character and forces atlas regeneration of the body
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public async Task ShowBodyParts(BodyPartsHidden parts)
        {
            foreach (BodyPartsHidden flag in Enum.GetValues(typeof(BodyPartsHidden)))
            {
                if (parts.HasFlag(flag))
                {
                    BodyPartsHidden &= ~flag;
                }
            }

            if (autoGenerateAtlas) await Body.RegenerateAtlas();
        }

        /// <summary>
        /// Adds the flags supplied in <paramref name="parts"/> to the BodyPartsHidden configuration of the character and forces atlas regeneration of the body
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public async Task HideBodyParts(BodyPartsHidden parts)
        {
            foreach (BodyPartsHidden flag in Enum.GetValues(typeof(BodyPartsHidden)))
            {
                if (parts.HasFlag(flag))
                {
                    BodyPartsHidden |= flag;
                }
            }

            if (autoGenerateAtlas) await Body.RegenerateAtlas();
        }

        /// <summary>
        /// Directly changes the BodyPartsHidden configuration of the character to the value supplied in <paramref name="parts"/> and forces atlas regeneration of the body
        /// </summary>
        /// <param name="parts"></param>
        public async void HideBodyPartsAbsolute(BodyPartsHidden parts)
        {
            BodyPartsHidden = parts;

            if (autoGenerateAtlas) await Body.RegenerateAtlas();
        }

        /// <summary>
        /// Set the <paramref name="age"/> of the character. This updates the material and sets the blend shapes
        /// BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD to the corresponding value.
        /// Set regenerateAtlas to false if you want to also set ethnicity or sex after this.
        /// </summary>
        /// <param name="age"></param>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        public async Task SetAge(Age age, bool regenerateAtlas = true)
        {
            Age = age;

            switch (age)
            {
                case Age.Child:
                    UpdateMorphs(new[] { BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD }, new[] { 0.3f, 0.3f, 0.15f, 1.4f, 0.0f });
                    break;
                case Age.Teen:
                    UpdateMorphs(new[] { BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD }, new[] { 0.15f, 0.15f, 0.075f, 0.7f, 0.0f });
                    break;
                case Age.Young:
                    UpdateMorphs(new[] { BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD }, new[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f });
                    break;
                case Age.MiddleAge:
                    UpdateMorphs(new[] { BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD }, new[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.25f });
                    break;
                case Age.Old:
                    UpdateMorphs(new[] { BlendShapes.HEAD_SCALE_X, BlendShapes.HEAD_SCALE_Y, BlendShapes.HEAD_SCALE_Z, BlendShapes.HEAD_YOUNG, BlendShapes.AGE_OLD }, new[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.5f });
                    break;
            }

            await UpdateMaterial(regenerateAtlas);
        }

        /// <summary>
        /// Converts the Age, Ethnicity and Sex enums to their string representations used in file paths
        /// </summary>
        /// <param name="ageString"></param>
        /// <param name="ethnicityString"></param>
        /// <param name="sexString"></param>
        private void GetMaterialBaseStrings(out string ageString, out string ethnicityString, out string sexString)
        {
            ageString = (Age == Age.Child || Age == Age.Teen ? Age.Young : Age).ToString().ToLower();

            ethnicityString = Ethnicity.ToString().ToLower();

            sexString = (Sex == Sex.Ambiguous ? Sex.Female : Sex).ToString().ToLower();
        }

        /// <summary>
        /// Generates the string used to access the body material in <c>bodyBaseMaterials</c>
        /// </summary>
        private string GetMaterialString()
        {
            GetMaterialBaseStrings(out var ageString, out var ethnicityString, out var sexString);

            string materialString = ethnicityString + "_" + sexString + "_" + ageString;

            return materialString;
        }

        /// <summary>
        /// Updates the model material based on the current sex, age and ethnicity. Regenerates the atlas unless specified not to.
        /// </summary>
        /// <param name="regenerateAtlas"></param>
        /// <returns></returns>
        private async Task UpdateMaterial(bool regenerateAtlas = true)
        {
            var materialString = GetMaterialString();

            GD.Print(Body);

            if (Body != null)
            {
                GD.Print(Body.name);
                GD.Print(materialString);
                Body.material = bodyBaseMaterials[materialString];
                if (regenerateAtlas) await Body.RegenerateAtlas();
            }
        }
    }

    public enum Age
    {
        Child,
        Teen,
        Young,
        MiddleAge,
        Old
    }

    public enum Ethnicity
    {
        Asian,
        African,
        Caucasian,
    }

    public enum Sex
    {
        Male,
        Female,
        Ambiguous
    }
}
