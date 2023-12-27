using Godot;
using Godot.Collections;
using System.Collections.Concurrent;

namespace GCC
{

	///<summary>
	///This class manages the data for clothes and their corresponding blend shapes
	///</summary>
	public partial class CharacterData : Node
	{

		///<summary>
		///Override this before using the character creator if you are using the project as a git submodule for easy updates
		///</summary>
		public static string resourceBasePath = "res://godot_character_creator";

		public readonly static ConcurrentDictionary<string, Array<Vector3[]>> MeshShapes = new();
		public readonly static ConcurrentDictionary<string, Array<int[]>> MeshShapesIndexes = new();
		public readonly static ConcurrentDictionary<string, Dictionary<string, int>> ShapeNameIndexes = new();
		public readonly static ConcurrentDictionary<string, int> clothCount = new();

		/// <summary>
		/// Get data for a blend shape of the given <paramref name="cloth"/>. Load the blend shape data from disk if necessary.
		/// </summary>
		/// <param name="cloth"></param>t
		/// <param name="shapeIndex"></param>
		/// <param name="indexes"></param>
		/// <param name="data"></param>
		public static void GetBlendshapeData(string cloth, int shapeIndex, out int[] indexes, out Vector3[] data)
		{
			indexes = MeshShapesIndexes[cloth][shapeIndex];
			data = MeshShapes[cloth][shapeIndex];
		}

		/// <summary>
		/// Decrease the reference count of a <paramref name="cloth"/>, unload it from memory if necessary and collect the garbage
		/// </summary>
		/// <param name="cloth"></param>
		public static void UnloadCloth(string cloth)
		{
			var count = --clothCount[cloth];

			if (count == 0)
			{
				MeshShapes.TryRemove(cloth, out var _);
				MeshShapesIndexes.TryRemove(cloth, out var _);
				ShapeNameIndexes.TryRemove(cloth, out var _);
				clothCount.TryRemove(cloth, out var _);
				System.GC.Collect(System.GC.MaxGeneration);
			}
		}

		/// <summary>
		/// Load the <paramref name="cloth"/> from disk if necessary, otherwise increase the reference count
		/// </summary>
		/// <param name="cloth"></param>
		/// <param name="debug"></param>
		public static void LoadCloth(string cloth, bool debug = false)
		{
			if (!clothCount.ContainsKey(cloth))
			{
				var clothNameEscaped = cloth.Replace("/", "_");
				var debugStopwatch = System.Diagnostics.Stopwatch.StartNew();
				var file = FileAccess.OpenCompressed(CharacterData.resourceBasePath+"/blend_shape_data/" + clothNameEscaped + ".txt", FileAccess.ModeFlags.Read, FileAccess.CompressionMode.Zstd);
				var dataFromFile = (Array)file.GetVar();

				MeshShapes[cloth] = (Array<Vector3[]>)dataFromFile[ShapeFileKeys.BLEND_SHAPES];
				MeshShapesIndexes[cloth] = (Array<int[]>)dataFromFile[ShapeFileKeys.BLEND_SHAPE_INDEXES];
				ShapeNameIndexes[cloth] = (Dictionary<string, int>)dataFromFile[ShapeFileKeys.SHAPE_NAME_INDEX];

				file.Close();

				clothCount[cloth] = 1;

				if (debug) GD.Print("Loaded blend shape data; took " + debugStopwatch.Elapsed.TotalMilliseconds);
			}
			else
			{
				clothCount[cloth]++;
			}
		}
	}
}
