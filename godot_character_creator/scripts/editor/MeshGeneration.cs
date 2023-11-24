using Godot;
using Godot.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GCC
{
	/// <summary>
	/// The indexes for the data arrays serialized to disk
	/// </summary>
	public class ShapeFileKeys
	{
		public const int BLEND_SHAPE_INDEXES = 0;
		public const int BLEND_SHAPES = 1;
		public const int SHAPE_NAME_INDEX = 2;
	}

	[Flags]
	public enum BodyPartsHidden
	{
		None = 0,
		Head = 1,
		Arms = 2,
		Hands = 4,
		Torso = 8,
		Legs = 16,
		Feet = 32
	}

	/// <summary>
	/// Constants for the blend shape names for better readability and maintainability
	/// </summary>
	public static class BlendShapes
	{
		public const string BODY_FAT = "fat";
		public const string BODY_ZOMBIE = "zombie";
		public const string BODY_SKINNY = "skinny";
		public const string BODY_MUSCLE = "muscle";
		public const string BODY_PREGNANY = "pregnancy";
		public const string SEX_MAN = "man";
		public const string SEX_WOMAN_1 = "woman1";
		public const string SEX_WOMAN_2 = "woman2";
		public const string ARM_MUSCLE = "armmuscle";
		public const string ARM_FAT = "armfat";
		public const string LEG_MUSCLE = "legmuscle";
		public const string LEG_FAT = "legfat";
		public const string AGE_OLD = "old";
		public const string AGE_CHILD = "child";
		public const string STOMACH_SMALLER = "stomach+";
		public const string STOMACH_BIGGER = "stomach-";
		public const string NAVEL_BIGGER = "navel+";
		public const string NAVEL_SMALLER = "navel-";
		public const string NAVEL_DOWN = "navel_move";
		public const string BUTT_BIGGER = "butt_cast";
		public const string BUTT_UP = "butt_pam1_+";
		public const string BUTT_DOWN = "butt_pam1_-";
		public const string BREAST_BIGGER_1 = "breast1";
		public const string BREAST_BIGGER_2 = "breast2";
		public const string BREAST_BIGGER_3 = "breast3";
		public const string BREAST_DOWN_1 = "breast_pam1";
		public const string BREAST_DOWN_2 = "breast_pam2";
		public const string BREAST_FORWARD = "breast_pam3";
		public const string BREAST_BROADER = "breast_pam4_+";
		public const string BREAST_NARROWER = "breast_pam4_-";
		public const string TORSO_THINNER = "torso_pam1_-";
		public const string TORSO_WIDER = "torso_pam1_+";
		public const string TORSO_SIDE_THINNER = "torso_pam2_-";
		public const string TORSO_SIDE_WIDER = "torso_pam2_+";
		public const string TORSO_LOWER_THINNER = "torso_pam3_-";
		public const string TORSO_LOWER_WIDER = "torso_pam3_+";
		public const string TORSO_LOWER_SIDE_THINNER = "torso_pam4_-";
		public const string TORSO_LOWER_SIDE_WIDER = "torso_pam4_+";
		public const string TORSO_LOWER_DOWN = "torso_pam5_-";
		public const string TORSO_LOWER_UP = "torso_pam5_+";
		public const string TORSO_UPPER_WIDER = "torso_pam6";
		public const string TORSO_UPPER_INFLATED = "torso_pam7";
		public const string TORSO_CENTER_SIDE_WIDER = "torso_pam8_+";
		public const string TORSO_CENTER_SIDE_THINNER = "torso_pam8_-";
		public const string TORSO_CENTER_WIDER = "torso_pam9_+";
		public const string TORSO_CENTER_THINNER = "torso_pam9_-";
		public const string ETHNICITY_AFRICAN = "african";
		public const string ETHNICITY_CAUCASIAN = "caucasian";
		public const string ETHNICITY_ASIAN = "asian";
		public const string NECK_DOUBLE_NECK = "double_neck";
		public const string NECK_BIGGER = "neck_scale_XY";
		public const string HEAD_YOUNG = "head_young";
		public const string HEAD_OVAL = "head_oval";
		public const string HEAD_CIRCLE = "head_circle";
		public const string HEAD_QUADRATIC = "head_quad";
		public const string HEAD_INVERSE_TRIANGLE = "head_inv_tris";
		public const string HEAD_TRIANGLE = "head_tris";
		public const string HEAD_TEMPLE_WIDER = "head_visok_+";
		public const string HEAD_SCALE_X = "head_scale_X";
		public const string HEAD_SCALE_Y = "head_scale_Y";
		public const string HEAD_SCALE_Z = "head_scale_Z";
		public const string HEAD_TOP_FRONT_SMALLER = "head_pam1_+";
		public const string HEAD_TOP_FRONT_BIGGER = "head_pam1_-";
		public const string HEAD_TOP_BIGGER = "head_pam2_+";
		public const string HEAD_TOP_SMALLER = "head_pam_2_-";
		public const string HEAD_BACK_BIGGER = "head_pam_3_+";
		public const string HEAD_BACK_SMALLER = "head_pam_3_-";
		public const string HEAD_EYES_SIDE_DOWN = "head_pam_4_+";
		public const string HEAD_FOREHEAD_UP = "head_pam_5_+";
		public const string HEAD_BROWS_DOWN = "head_pam_5_-";
		public const string CHEEKS_WIDER = "cheek_pam1_+";
		public const string CHEEKS_THINNER = "cheek_pam1_-";
		public const string CHEEKS_TOP_WIDER = "cheek_pam2_+";
		public const string CHEEKS_FRONT_BIGGER = "cheek_pam3_+";
		public const string CHEEKS_FRONT_WIDER = "cheek_pam4+";
		public const string CHEEKS_FRONT_DOWN = "cheek_pam4_-";
		public const string JAW_BOTTOM_BACK_WIDER = "jaw_pam1_+";
		public const string JAW_BOTTOM_BACK_THINNER = "jaw_pam1_-";
		public const string JAW_CHIN_PRONOUNCED = "jaw_pam2";
		public const string JAW_CHIN_SMALLER = "jaw_pam3_+";
		public const string JAW_CHIN_BIGGER = "jaw_pam3_-";
		public const string JAW_CHIN_WIDER = "jaw_pam4_+";
		public const string JAW_CHIN_THINNER = "jaw_pam4_-";
		public const string JAW_BIGGER = "jaw_pam5_+";
		public const string JAW_SMALLER = "jaw_pam5_-";
		public const string JAW_SIDE_DOWN = "jaw_pam6_+";
		public const string JAW_SIDE_BACK = "jaw_pam6_-";
		public const string JAW_UNDERBITE = "jaw_pam7_+";
		public const string JAW_OVERBITE = "jaw_pam7_-";
		public const string NOSE_SCALE_X = "nose_scale_X";
		public const string NOSE_SCALE_Y = "nose_scale_Y";
		public const string NOSE_SCALE_Z = "nose_scale_Z";
		public const string NOSE_MOVE_FORWARD = "nose_move_Y_+";
		public const string NOSE_MOVE_BACK = "nose_move_Y_-";
		public const string NOSE_MOVE_UP = "nose_move_Z_+";
		public const string NOSE_MOVE_DOWN = "nose_move_Z_-";
		public const string NOSE_ROOT_WIDER = "nose_scale_1_+";
		public const string NOSE_ROOT_THINNER = "nose_scale_1_-";
		public const string NOSE_ROOT_DOWN = "head_pam_4_-";
		public const string NOSE_BRIDGE_WIDER = "nose_scale_2_+";
		public const string NOSE_BRIDGE_THINNER = "nose_scale_2_-";
		public const string NOSE_NOSTRILS_WIDER = "nose_scale_3_+";
		public const string NOSE_NOSTRILS_THINNER = "nose_scale_3_-";
		public const string NOSE_TIP_BENT = "nose_pam1_+";
		public const string NOSE_TIP_PRONOUNCED = "nose_pam1_-";
		public const string NOSE_BRIDGE_UP_1 = "nose_pam2_+";
		public const string NOSE_BRIDGE_UP_2 = "nose_pam3_+";
		public const string NOSE_BRIDGE_DOWN_1 = "nose_pam2_-";
		public const string NOSE_BRIDGE_DOWN_2 = "nose_pam4_-";
		public const string NOSE_UPPER_BRIDGE_DOWN = "nose_pam3_-";
		public const string NOSE_BRIDGE_CRINKLED = "nose_pam4_+";
		public const string NOSE_TIP_INFLATED = "nose_pam5";
		public const string NOSE_NOSTRILS_UP = "nose_pam6_+";
		public const string NOSE_NOSTRILS_DOWN = "nose_pam6_-";
		public const string LIPS_WIDER = "lips_scale_X_+";
		public const string LIPS_THINNER = "lips_scale_X_-";
		public const string LIPS_BIGGER = "lips_scale_Y_+";
		public const string LIPS_SMALLER = "lips_scale_Y_-";
		public const string LIPS_PRONOUNCED = "lips_scale_Z_+";
		public const string LIPS_FLATTER = "lips_scale_Z_-";
		public const string LIPS_MOVE_FOREWARD = "lips_move_Y_+";
		public const string LIPS_MOVE_BACK = "lips_move_Y_-";
		public const string LIPS_MOVE_UP = "lips_move_Z_+";
		public const string LIPS_MOVE_DOWN = "lips_move_Z_-";
		public const string LIPS_BASE_BIGGER = "lips_pam1_+";
		public const string LIPS_BASE_SMALLER = "lips_pam1_-";
		public const string LIPS_BASE_WIDER = "lips_pam2_+";
		public const string LIPS_BASE_THINNER = "lips_pam2_-";
		public const string LIPS_CORNERS_UP = "lips_pam3_+";
		public const string LIPS_CORNERS_DOWN = "lips_pam3_-";
		public const string LIPS_LOWER_BIGGER = "lips_pam4_+";
		public const string LIPS_LOWER_SMALLER = "lips_pam4_-";
		public const string LIPS_UPPER_BIGGER = "lips_pam5_+";
		public const string LIPS_UPPER_SMALLER = "lips_pam5_-";
		public const string EYES_ZYGOMATICUM_BIGGER = "eye_pam1";
		public const string EYES_ZYGOMATICUM_UP = "eye_pam2_+";
		public const string EYES_ZYGOMATICUM_DOWN = "eye_pam2_-";
		public const string EYES_INNER_EYELID_BIGGER = "eye_pam3_+";
		public const string EYES_OUTER_EYELID_BIGGER = "eye_pam4_-";
		public const string EYES_INNER_CORNERS_BIGGER = "eye_pam5_+";
		public const string EYES_INNER_CORNERS_SMALLER = "eye_pam5_-";
		public const string EYES_UPPER_EYELID_FOREWARD = "eye_pam6_+";
		public const string EYES_UPPER_EYELID_BACK = "eye_pam6_-";
		public const string EYES_INNER_EYELIDS_BROADER = "eye_pam_7_+";
		public const string EYES_INNER_EYELIDS_NARROWER = "eye_pam_7_-";
		public const string EYES_EYLIDS_BROADER = "eye_pam_8_+";
		public const string EYES_EYLIDS_NARROWER = "eye_pam_8_-";
		public const string EYES_OUTER_EYELIDS_BROADER = "eye_pam_9_+";
		public const string EYES_OUTER_EYELIDS_NARROWER = "eye_pam_9_-";
		public const string EYES_OUTER_CORNERS_BIGGER = "eye_pam10_+";
		public const string EYES_OUTER_CORNERS_SMALLER = "eye_pam10_-";
		public const string EYES_SPACE_BROADER = "eye_pam11_+";
		public const string EYES_SPACE_NARROWER = "eye_pam11_-";
		public const string EYES_UP = "eye_pam12_+";
		public const string EYES_DOWN = "eye_pam12_-";
		public const string EYES_BIGGER = "eye_pam13_+";
		public const string EYES_SMALLER = "eye_pam13_-";
		public const string EYES_OUTER_CORNERS_UP = "eye_pam14_+";
		public const string EYES_OUTER_CORNERS_DOWN = "eye_pam14_-";
		public const string EYES_INNER_CORNERS_UP = "eye_pam15_+";
		public const string EYES_INNER_CORNERS_DOWN = "eye_pam15_-";
		public const string EXPRESSION_ANGRY = "exp_anger";
		public const string EXPRESSION_BORED = "exp_boring";
		public const string EXPRESSION_CRYING = "exp_cry";
		public const string EXPRESSION_DOUBTING = "exp_doubt";
		public const string EXPRESSION_DETERMINED = "exp_determination";
		public const string EXPRESSION_DISGUSTED = "exp_disgust";
		public const string EXPRESSION_STRAINED = "exp_effort1";
		public const string EXPRESSION_PAINED = "exp_effort3";
		public const string EXPRESSION_SURPRISED = "exp_fear3";
		public const string EXPRESSION_PITYING = "exp_sad1";
		public const string EXPRESSION_SAD = "exp_sad2";
		public const string EXPRESSION_SLEEPING = "exp_sleep";
		public const string EXPRESSION_STUNNED = "exp_stun";
		public const string EXPRESSION_SMIRKING = "exp_smile1";
		public const string EXPRESSION_SMILING_1 = "exp_smile2";
		public const string EXPRESSION_SMILING_2 = "exp_smile3";
		public const string EXPRESSION_SMILING_3 = "exp_smile4";
		public const string EXPRESSION_LAUGHING = "exp_smile5";
		public const string EXPRESSION_THREATENING = "exp_treaten";
		public const string EXPRESSION_AIMING = "exp_aim";
		public const string EXPRESSION_VOCAL = "exp_vocal";
		public const string EXPRESSION_GLEEFUL = "exp_fff";
		public const string EXPRESSION_CONCENTRATING = "exp_concentrating";
		public const string EXPRESSION_KISSING = "exp_airkiss";
		public const string EXPRESSION_DEVICIOUS = "exp_devicios";
		public const string EXPRESSION_SICK = "exp_sick";
	}

	/// <summary>
	/// Simple color struct since Intellisense becomes very slow when using collections initializers in combination with godot variants. Also an alpha channel is not needed
	/// </summary>
	internal struct Color24
	{
		public float r = 0;
		public float g = 0;
		public float b = 0;

		public Color24(float r, float g, float b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}
	}

	public partial class MeshGeneration : Node
	{
		[Export]
		Array<string> clothesBlendShapes;

		[Export]
		Array<string> clothesGeneration;

		[Export]
		Array<string> clothesNormal;

		static MeshGeneration()
		{
			var arr = new string[vertexGroupMapping.Keys.Count];
			vertexGroupMapping.Keys.CopyTo(arr, 0);
			BLEND_SHAPE_KEYS = System.Array.AsReadOnly(arr);
		}

		public static readonly ReadOnlyCollection<string> BLEND_SHAPE_KEYS;

		/// <summary>
		/// A mapping for the affected regions of blend shapes which is used to reduce their size. The colors must match the ones from vertex_groups.png
		/// </summary>
		private static readonly System.Collections.Generic.Dictionary<string, Color24[]> vertexGroupMapping = new()
		{
			{ BlendShapes.ETHNICITY_AFRICAN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1), new(0, 0, 0) } },
			{ BlendShapes.ETHNICITY_CAUCASIAN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1), new(0, 0, 0) } },
			{ BlendShapes.ETHNICITY_ASIAN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1), new(0, 0, 0) } },
			{ BlendShapes.AGE_CHILD, new Color24[] { new(0, 1, 0), new(0, 0, 1), new(0, 1, 1), new(1, 1, 0), new(1, 0, 0) } },
			{ BlendShapes.AGE_OLD, new Color24[] { } },
			{ BlendShapes.BODY_FAT, new Color24[] { } },
			{ BlendShapes.BODY_ZOMBIE, new Color24[] { } },
			{ BlendShapes.BODY_SKINNY, new Color24[] { } },
			{ BlendShapes.BODY_MUSCLE, new Color24[] { } },
			{ BlendShapes.BODY_PREGNANY, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.SEX_MAN, new Color24[] { } },
			{ BlendShapes.SEX_WOMAN_1, new Color24[] { } },
			{ BlendShapes.SEX_WOMAN_2, new Color24[] { } },
			{ BlendShapes.ARM_MUSCLE, new Color24[] { new(0, 1, 0) } },
			{ BlendShapes.ARM_FAT, new Color24[] { new(0, 1, 0) } },
			{ BlendShapes.LEG_FAT, new Color24[] { } },
			{ BlendShapes.LEG_MUSCLE, new Color24[] { new(0, 0, 1), new(0, 1, 1) } },
			{ BlendShapes.STOMACH_BIGGER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.STOMACH_SMALLER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.NAVEL_SMALLER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.NAVEL_BIGGER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.NAVEL_DOWN, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.BUTT_BIGGER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.BUTT_UP, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.BUTT_DOWN, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.BREAST_BIGGER_1, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_BIGGER_2, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_BIGGER_3, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_DOWN_1, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_DOWN_2, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_FORWARD, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_NARROWER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.BREAST_BROADER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_THINNER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_WIDER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_SIDE_THINNER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_SIDE_WIDER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_THINNER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_WIDER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_SIDE_THINNER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_SIDE_WIDER, new Color24[] { new(0, 1, 0), new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_DOWN, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.TORSO_LOWER_UP, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.TORSO_UPPER_WIDER, new Color24[] { new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) } },
			{ BlendShapes.TORSO_UPPER_INFLATED, new Color24[] { new(0, 0, 1), new(0, 1, 0), new(1, 1, 0) } },
			{ BlendShapes.TORSO_CENTER_SIDE_WIDER, new Color24[] { new(0, 0, 1), new(0, 1, 0) } },
			{ BlendShapes.TORSO_CENTER_SIDE_THINNER, new Color24[] { new(0, 0, 1), new(0, 1, 0) } },
			{ BlendShapes.TORSO_CENTER_WIDER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.TORSO_CENTER_THINNER, new Color24[] { new(0, 0, 1) } },
			{ BlendShapes.NECK_DOUBLE_NECK, new Color24[] { new(1, 0, 0), new(1, 1, 0), new(0, 0, 0) } },
			{ BlendShapes.NECK_BIGGER, new Color24[] { new(1, 1, 0), new(0, 0, 0), new(0, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_YOUNG, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.HEAD_OVAL, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_CIRCLE, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_QUADRATIC, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_INVERSE_TRIANGLE, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_TRIANGLE, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.HEAD_TEMPLE_WIDER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_SCALE_X, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.HEAD_SCALE_Y, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.HEAD_SCALE_Z, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.HEAD_TOP_FRONT_SMALLER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_TOP_FRONT_BIGGER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_TOP_BIGGER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_TOP_SMALLER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_BACK_BIGGER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_BACK_SMALLER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.HEAD_EYES_SIDE_DOWN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1) } },
			{ BlendShapes.HEAD_FOREHEAD_UP, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1) } },
			{ BlendShapes.HEAD_BROWS_DOWN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1) } },
			{ BlendShapes.CHEEKS_WIDER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.CHEEKS_THINNER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.CHEEKS_TOP_WIDER, new Color24[] { new(1, 0, 0) } },
			{ BlendShapes.CHEEKS_FRONT_BIGGER, new Color24[] { new(1, 0, 0), new(1, 0, 1) } },
			{ BlendShapes.CHEEKS_FRONT_WIDER, new Color24[] { new(1, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.CHEEKS_FRONT_DOWN, new Color24[] { new(1, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.JAW_BOTTOM_BACK_WIDER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_BOTTOM_BACK_THINNER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_CHIN_PRONOUNCED, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_CHIN_SMALLER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_CHIN_BIGGER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_CHIN_WIDER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_CHIN_THINNER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_BIGGER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_SMALLER, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_SIDE_DOWN, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_SIDE_BACK, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_UNDERBITE, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.JAW_OVERBITE, new Color24[] { new(1, 0, 0), new(0, 0, 0) } },
			{ BlendShapes.NOSE_ROOT_DOWN, new Color24[] { new(1, 0, 0), new(1, 1, 1), new(1, 0, 1) } },
			{ BlendShapes.NOSE_SCALE_X, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_SCALE_Y, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_SCALE_Z, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_MOVE_FORWARD, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_MOVE_BACK, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_MOVE_UP, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_MOVE_DOWN, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_ROOT_WIDER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_ROOT_THINNER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_WIDER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_THINNER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_UP_1, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_DOWN_1, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_UP_2, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_DOWN_2, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_BRIDGE_CRINKLED, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_UPPER_BRIDGE_DOWN, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_NOSTRILS_WIDER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_NOSTRILS_THINNER, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_NOSTRILS_UP, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_NOSTRILS_DOWN, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_TIP_BENT, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_TIP_PRONOUNCED, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.NOSE_TIP_INFLATED, new Color24[] { new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.LIPS_WIDER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_THINNER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_BIGGER, new Color24[] { new(0, 0, 0), new(1, 0, 1) } },
			{ BlendShapes.LIPS_SMALLER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_PRONOUNCED, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_FLATTER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_MOVE_FOREWARD, new Color24[] { new(0, 0, 0), new(1, 0, 1) } },
			{ BlendShapes.LIPS_MOVE_BACK, new Color24[] { new(0, 0, 0), new(1, 0, 1), new(1, 0, 0) } },
			{ BlendShapes.LIPS_MOVE_UP, new Color24[] { new(0, 0, 0), new(1, 0, 1) } },
			{ BlendShapes.LIPS_MOVE_DOWN, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_BASE_BIGGER, new Color24[] { new(0, 0, 0), new(1, 0, 0) } },
			{ BlendShapes.LIPS_BASE_SMALLER, new Color24[] { new(0, 0, 0), new(1, 0, 0) } },
			{ BlendShapes.LIPS_BASE_WIDER, new Color24[] { new(0, 0, 0), new(1, 0, 0) } },
			{ BlendShapes.LIPS_BASE_THINNER, new Color24[] { new(0, 0, 0), new(1, 0, 0) } },
			{ BlendShapes.LIPS_CORNERS_UP, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_CORNERS_DOWN, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_LOWER_BIGGER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_LOWER_SMALLER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_UPPER_BIGGER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.LIPS_UPPER_SMALLER, new Color24[] { new(0, 0, 0) } },
			{ BlendShapes.EYES_ZYGOMATICUM_BIGGER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_ZYGOMATICUM_UP, new Color24[] { new(1, 1, 1), new(1, 0, 1), new(1, 0, 0) } },
			{ BlendShapes.EYES_ZYGOMATICUM_DOWN, new Color24[] { new(1, 1, 1), new(1, 0, 1), new(1, 0, 0) } },
			{ BlendShapes.EYES_INNER_EYELID_BIGGER, new Color24[] { new(1, 1, 1), new(1, 0, 1) } },
			{ BlendShapes.EYES_OUTER_EYELID_BIGGER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_CORNERS_BIGGER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_CORNERS_SMALLER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_UPPER_EYELID_FOREWARD, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_UPPER_EYELID_BACK, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_EYELIDS_BROADER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_EYELIDS_NARROWER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_EYLIDS_BROADER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_EYLIDS_NARROWER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_EYELIDS_BROADER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_EYELIDS_NARROWER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_CORNERS_BIGGER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_CORNERS_SMALLER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_SPACE_BROADER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_SPACE_NARROWER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_UP, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_DOWN, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_BIGGER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_SMALLER, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_CORNERS_UP, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_OUTER_CORNERS_DOWN, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_CORNERS_UP, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EYES_INNER_CORNERS_DOWN, new Color24[] { new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_ANGRY, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_BORED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_CRYING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_DETERMINED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_DISGUSTED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_DOUBTING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_STRAINED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_PAINED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SURPRISED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_PITYING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SAD, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SLEEPING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_STUNNED, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SMIRKING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SMILING_1, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SMILING_2, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SMILING_3, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_LAUGHING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_THREATENING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_AIMING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_VOCAL, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_GLEEFUL, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_CONCENTRATING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_KISSING, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_DEVICIOUS, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } },
			{ BlendShapes.EXPRESSION_SICK, new Color24[] { new(1, 0, 0), new(0, 0, 0), new(1, 0, 1), new(1, 1, 1) } }

		};

		bool meshGenerationComplete = false;

		/// <summary>
		/// This function returns an integer array called <c>ProximityMapping</c> the size of <paramref name="vertex"/> which specifies the closest vertex index of the original <paramref name="vertexOriginal"/>.
		/// The array can then be used by the transfer functions to generate blendshapes and boneweights.
		/// The best possible 
		/// </summary>
		/// <param name="vertex"></param>
		/// <param name="vertexOriginal"></param>
		/// <returns></returns>
		public static int[] GetProximityMapping(Vector3[] vertex, Vector3[] vertexOriginal)
		{
			var sw = Stopwatch.StartNew();
			var mapping = new int[vertex.Length];

			for (var i = 0; i < vertex.Length; i++)
			{
				var closestIndex = 0;
				var distance = float.MaxValue;
				var currentVertex = vertex[i];

				for (var ii = 0; ii < vertexOriginal.Length; ii++)
				{
					var _originalVertex = vertexOriginal[ii];
					float xD = currentVertex.X - _originalVertex.X;
					float yD = currentVertex.Y - _originalVertex.Y;
					float zD = currentVertex.Z - _originalVertex.Z;
					var _distance = xD * xD + yD * yD + zD * zD;
					if (_distance < distance)
					{
						distance = _distance;
						closestIndex = ii;
						if (distance < 0.00000003f) break;
					}
				}

				mapping[i] = closestIndex;
			}

			GD.Print("Proximity Mapping Time: " + sw.Elapsed.TotalMilliseconds);

			return mapping;
		}

		/// <summary>
		/// Uses a <paramref name="proximityMapping"/> to transfer bone weights and indexes from one mesh to the other.
		/// </summary>
		/// <param name="proximityMapping"></param>
		/// <param name="boneIndexesOriginal"></param>
		/// <param name="boneWeightsOriginal"></param>
		/// <param name="boneIndexes"></param>
		/// <param name="boneWeights"></param>
		public static void TransferBoneWeights(int[] proximityMapping, int[] boneIndexesOriginal, float[] boneWeightsOriginal, int[] replaceBones, int replaceBone, out int[] boneIndexes, out float[] boneWeights)
		{
			boneIndexes = new int[proximityMapping.Length * 4];
			boneWeights = new float[proximityMapping.Length * 4];

			var offset = 0;

			for (var i = 0; i < proximityMapping.Length; i++)
			{

				var offsetOriginal = proximityMapping[i] * 4;

				for (int ii = 0; ii < 4; ii++)
				{
					var boneIndex = boneIndexes[offset + ii] = boneIndexesOriginal[offsetOriginal + ii];
					if (replaceBones.Contains(boneIndex)) boneIndexes[offset + ii] = replaceBone;

					boneWeights[offset + ii] = boneWeightsOriginal[offsetOriginal + ii]; ;
				}

				offset += 4;
			}
		}

		/// <summary>
		/// Generates a new blend shape array based on the provided <paramref name="proximityMapping"/> and the <paramref name="originalBlendShape"/>
		/// </summary>
		/// <param name="proximityMapping"></param>
		/// <param name="originalBlendShape"></param>
		public static Vector3[] TransferBlendshape(int[] proximityMapping, Vector3[] originalBlendShape)
		{
			var newBlendshape = new System.Collections.Generic.List<Vector3>();

			for (var i = 0; i < proximityMapping.Length; i++)
			{
				var mappedIndex = proximityMapping[i];
				newBlendshape.Add(originalBlendShape[mappedIndex]);
			}

			return newBlendshape.ToArray();
		}

		/// <summary>
		/// Checks if there are any files that match <paramref name="cloth"/> + a valid extension (.mesh, .obj, .fbx, .gltf) inside the meshes_for_generation folder 
		/// </summary>
		/// <param name="cloth"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string CheckOriginalMeshPath(string cloth)
		{
			var formats = new[] { ".mesh", ".obj", ".fbx", ".gltf" };

			foreach (var format in formats)
			{
				var meshPath = "res://godot_character_creator/meshes_for_generation/" + cloth + format;

				if (FileAccess.FileExists(meshPath))
				{
					return meshPath;
				}
			}

			throw new Exception("Could not find cloth file for supported formats: " + cloth);
		}

		private Label processingLabel;
		private Label timestampsLabel;
		private Stopwatch processingTimer;
		private ScrollContainer scrollContainer;
		private ScrollBar scrollBar;

		/// <summary>
		/// Sets the text under "Currently Processing" to <paramref name="info"/> and records the currently elapsed milliseconds under "Processed".
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public async Task<Variant[]> SetProcessingInfo(string info)
		{
			processingLabel ??= FindChild("processing") as Label;
			timestampsLabel ??= FindChild("timestamps") as Label;
			scrollContainer ??= FindChild("scroll_container") as ScrollContainer;
			scrollBar ??= scrollContainer.GetVScrollBar();
			processingTimer ??= Stopwatch.StartNew();

			if (processingLabel.Text != "") timestampsLabel.Text += $"[{processingTimer.Elapsed.TotalMilliseconds}] " + processingLabel.Text + System.Environment.NewLine;
			processingLabel.Text = info;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			scrollContainer.ScrollVertical = (int)Mathf.Ceil(scrollBar.MaxValue);
			return null;
		}

		public override void _Process(double delta)
		{
			if (meshGenerationComplete && Input.IsKeyPressed(Key.Enter))
			{
				GetTree().Quit();
			}
		}

		public async override void _Ready()
		{
			try
			{
				Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			}
			catch (System.ComponentModel.Win32Exception e)
			{
				GD.Print(e);
			}

			//Twice so we actually get a render of the scene
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			await SetProcessingInfo("Intialization");

			var skeletonScene = (PackedScene)ResourceLoader.Load("res://godot_character_creator/assets/skeletons/default.tscn");
			var skeleton = skeletonScene.Instantiate() as Skeleton3D;
			var excludedBones = new string[] { "tongue_base", "tongue_mod", "tongue_tip", "lolid.L", "lolid.R", "uplid.L", "uplid.R", "LeftEye", "RightEye", "Jaw" };
			var excludedBonesEyelashes = new string[] { "tongue_base", "tongue_mod", "tongue_tip", "LeftEye", "RightEye", "Jaw" };
			var excludedBoneIds = new int[excludedBones.Length];
			var excludedBoneIdsEyelashes = new int[excludedBonesEyelashes.Length];

			AddChild(skeleton);

			for (var i = 0; i < excludedBones.Length; i++)
			{
				var excludedBone = excludedBones[i];
				excludedBoneIds[i] = skeleton.FindBone(excludedBone);
			}

			for (var i = 0; i < excludedBonesEyelashes.Length; i++)
			{
				var excludedBone = excludedBonesEyelashes[i];
				excludedBoneIdsEyelashes[i] = skeleton.FindBone(excludedBone);
			}

			var excludedBonesReplacement = skeleton.FindBone("Head");

			await SetProcessingInfo("Processing body mesh");

			ProcessBodyMesh(out var meshShapes, out var processedBodyShapes, out var bodyBaseForm, out var bodyShapeNameIndex, out var bodyBones, out var bodyBoneWeights);

			await ClothesWithoutGeneration(meshShapes);

			await SetProcessingInfo("Processing Clothes with generated data");

			await ClothesWithFullGeneration(processedBodyShapes, bodyBaseForm, meshShapes, bodyShapeNameIndex, bodyBones, bodyBoneWeights, excludedBoneIdsEyelashes, excludedBoneIds, excludedBonesReplacement);

			await ClothesWithBlendshapesGeneration(processedBodyShapes, bodyBaseForm, meshShapes, bodyShapeNameIndex);

			await SetProcessingInfo("Compressing blend shapes");

			meshShapes = CompressData(meshShapes);

			await SetProcessingInfo("Stripping unused blend shapes");

			meshShapes = StripUnusedBlendshapes(meshShapes);

			await SetProcessingInfo("Writing blend shape data files");

			SaveData(meshShapes);

			await SetProcessingInfo("Finished, press Enter to quit");
			await SetProcessingInfo("Finished, press Enter to quit");

			meshGenerationComplete = true;
		}

		/// <summary>
		/// Loads the body mesh, extracts the blend shapes and reduces them using the vertex_group mapping. Then saves the smaller mesh and provides vertex, blend shape and bone data so they can be used by other generative methods
		/// </summary>
		/// <param name="meshShapes"></param>
		/// <param name="processedBodyShapes"></param>
		/// <param name="bodyBaseForm"></param>
		/// <param name="bodyShapeNameIndex"></param>
		/// <param name="bodyBones"></param>
		/// <param name="bodyBoneWeights"></param>
		private void ProcessBodyMesh(out Dictionary<string, Array<Variant>> meshShapes, out Array<Vector3[]> processedBodyShapes, out Vector3[] bodyBaseForm, out Dictionary<string, int> bodyShapeNameIndex, out int[] bodyBones, out float[] bodyBoneWeights)
		{
			DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/materials/");

			var blendShapes = new Array<Array<Vector3>>();
			var shapeNameIndex = new Dictionary<string, int>();
			var meshForArrays = ResourceLoader.Load(CheckOriginalMeshPath("body/body")) as ArrayMesh;
			var meshForArraysBlendShapeCount = meshForArrays.GetBlendShapeCount();

			for (var i = 0; i < meshForArraysBlendShapeCount; i++)
			{
				shapeNameIndex[meshForArrays.GetBlendShapeName(i)] = i;
			}

			var blendShapeArrays = meshForArrays.SurfaceGetBlendShapeArrays(0);
			var meshArrays = meshForArrays.SurfaceGetArrays(0);

			var mesh = new ArrayMesh();
			mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshArrays);
			DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/meshes/body/");
			ResourceSaver.Save(mesh, "res://godot_character_creator/assets/meshes/body/body.mesh");
			MakeScene("body/body");

			var baseForm = ((Array<Vector3>)meshArrays[(int)Mesh.ArrayType.Vertex]).ToArray();

			for (var i = 0; i < blendShapeArrays.Count; i++)
			{
				var blendShapeArray = (Array<Vector3>)blendShapeArrays[i][0];

				for (var j = 0; j < baseForm.Length; j++)
				{
					blendShapeArray[j] -= baseForm[j];
				}

				blendShapes.Add(blendShapeArray);
			}

			var vertexGroupsTexture = ResourceLoader.Load("res://godot_character_creator/meshes_for_generation/vertex_groups.png") as Texture2D;
			var vertexGroups = vertexGroupsTexture.GetImage();
			vertexGroups.Decompress();
			var vertexUV = ((Array<Vector2>)meshArrays[(int)Mesh.ArrayType.TexUV]).ToArray();
			var texSize = vertexGroups.GetSize();
			var keys = vertexGroupMapping.Keys.ToArray();

			Parallel.ForEach(keys, (blendShapeKey) =>
			{
				if (!shapeNameIndex.TryGetValue(blendShapeKey, out var index))
				{
					GD.Print("Blend shape key not found: " + blendShapeKey);
					return;
				}

				var vertexGroup = vertexGroupMapping[blendShapeKey];

				if (vertexGroup.Length == 0) return;

				var blendshape = blendShapes[index];
				var length = blendshape.Count;

				for (var j = 0; j < length; j++)
				{
					var uv = vertexUV[j];
					var color = vertexGroups.GetPixel((int)(uv.X * texSize.X), (int)(uv.Y * texSize.Y));
					var found = false;

					for (var i = 0; i < vertexGroup.Length; i++)
					{
						var vertexGroupColor = vertexGroup[i];

						if (vertexGroupColor.r == color.R && vertexGroupColor.g == color.G && vertexGroupColor.b == color.B)
						{
							found = true;
							break;
						}
					}

					if (!found) blendshape[j] = Vector3.Zero;
				}
			});


			processedBodyShapes = new();
			foreach (var blendShape in blendShapes)
			{
				processedBodyShapes.Add(blendShape.ToArray());
			}

			meshShapes = new Dictionary<string, Array<Variant>>
			{
				["body_body"] = new Array<Variant>()
				{
					new int[0],
					processedBodyShapes,
					shapeNameIndex
				}
			};

			bodyBaseForm = baseForm.ToArray();
			bodyShapeNameIndex = shapeNameIndex;
			bodyBones = ((Array<int>)meshArrays[(int)Mesh.ArrayType.Bones]).ToArray();
			bodyBoneWeights = ((Array<float>)meshArrays[(int)Mesh.ArrayType.Weights]).ToArray();
			//This is the difference between the baseForm and the blendshape of our body.
			//If our clothes fit the base character, then they SHOULD fit if we adjust them like the body.
			//Of course this is all just approximated, if you want something more accurate
			//you will have to manually set up things in blender
		}

		/// <summary>
		/// Extracts the blend shapes from meshes that are in the <c>clothesNormal</c> array and adds them to the meshShapes dictionary and saves the smaller meshes
		/// </summary>
		/// <param name="meshShapes"></param>
		/// <returns></returns>
		private async Task ClothesWithoutGeneration(Dictionary<string, Array<Variant>> meshShapes)
		{
			clothesNormal ??= new();

			foreach (var cloth in clothesNormal)
			{
				await SetProcessingInfo("Cloth: " + cloth);

				var shapeNameIndex = new Dictionary<string, int>();
				Array<Vector3[]> blendShapes = new();

				var meshForArrays = ResourceLoader.Load(CheckOriginalMeshPath(cloth)) as ArrayMesh;
				var meshForArraysBlendShapeCount = meshForArrays.GetBlendShapeCount();
				var blendShapeArrays = meshForArrays.SurfaceGetBlendShapeArrays(0);

				for (var i = 0; i < meshForArraysBlendShapeCount; i++)
				{
					shapeNameIndex[meshForArrays.GetBlendShapeName(i)] = i;
				}

				var meshArrays = meshForArrays.SurfaceGetArrays(0);
				var baseForm = ((Array<Vector3>)meshArrays[(int)Mesh.ArrayType.Vertex]).ToArray();
				var mesh = new ArrayMesh();
				mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshArrays);

				if (cloth.RFind("/") != -1) DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/meshes/" + cloth[..cloth.RFind("/")]);
				ResourceSaver.Save(mesh, "res://godot_character_creator/assets/meshes/" + cloth + ".mesh");
				MakeScene(cloth);

				for (var i = 0; i < meshForArraysBlendShapeCount; i++)
				{
					var blendShapeArray = (Array<Vector3>)blendShapeArrays[i][0];

					for (var j = 0; j < baseForm.Length; j++)
					{
						blendShapeArray[j] -= baseForm[j];
					}

					blendShapes.Add(blendShapeArray.ToArray());
				}

				meshShapes[cloth.Replace("/", "_")] = new()
				{
					new int[0],
					blendShapes,
					shapeNameIndex
				};
			};
		}

        /// <summary>
        /// Processes meshes that are in <c>clothesBlendShapes</c>. These will have blend shape data generated based on the data of the body mesh and keep their own bone weights
        /// </summary>
        /// <param name="processedBodyShapes"></param>
        /// <param name="bodyBaseForm"></param>
        /// <param name="meshShapes"></param>
        /// <param name="bodyShapeNameIndex"></param>
        /// <returns></returns>
        private async Task ClothesWithBlendshapesGeneration(Array<Vector3[]> processedBodyShapes, Vector3[] bodyBaseForm, Dictionary<string, Array<Variant>> meshShapes, Dictionary<string, int> bodyShapeNameIndex)
		{
			clothesBlendShapes ??= new();

			for (var i = 0; i < clothesBlendShapes.Count; i++)
			{
				var cloth = clothesBlendShapes[i];
				var _meshForArrays = ResourceLoader.Load(CheckOriginalMeshPath(cloth)) as ArrayMesh;
				var arr = _meshForArrays.SurfaceGetArrays(0);
				var vertexArray = ((Array<Vector3>)arr[(int)Mesh.ArrayType.Vertex]).ToArray();

				await SetProcessingInfo("Cloth with generated blend shapes: " + cloth);

				var vertexProximityMapping = GetProximityMapping(vertexArray, bodyBaseForm);
				var mesh = new ArrayMesh();
				mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);
				if (cloth.RFind("/") != -1) DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/meshes/" + cloth[..cloth.RFind("/")]);
				ResourceSaver.Save(mesh, "res://godot_character_creator/assets/meshes/" + cloth + ".mesh");
				MakeScene(cloth);

				Array<Vector3[]> blendShapes = new();

				for (var ii = 0; ii < processedBodyShapes.Count; ii++)
				{
					blendShapes.Add(TransferBlendshape(vertexProximityMapping, processedBodyShapes[ii]));
				}

				meshShapes[cloth.Replace("/", "_")] = new()
				{
					new int[0],
					blendShapes,
					bodyShapeNameIndex.Duplicate()
				};
			}
		}

        /// <summary>
        /// Processes meshes that are in <c>clothesGeneration</c>. These will have blend shape and bone data generated based on the data of the body mesh
        /// </summary>
        /// <param name="processedBodyShapes"></param>
        /// <param name="bodyBaseForm"></param>
        /// <param name="meshShapes"></param>
        /// <param name="bodyShapeNameIndex"></param>
        /// <param name="bodyBones"></param>
        /// <param name="bodyBoneWeights"></param>
        /// <param name="excludedBoneIdsEyelashes"></param>
        /// <param name="excludedBoneIds"></param>
        /// <param name="excludedBonesReplacement"></param>
        /// <returns></returns>
        private async Task ClothesWithFullGeneration(Array<Vector3[]> processedBodyShapes, Vector3[] bodyBaseForm, Dictionary<string, Array<Variant>> meshShapes, Dictionary<string, int> bodyShapeNameIndex, int[] bodyBones, float[] bodyBoneWeights, int[] excludedBoneIdsEyelashes, int[] excludedBoneIds, int excludedBonesReplacement)
		{
			clothesGeneration ??= new();
			var arrayMeshesClothGenerationArray = new Dictionary<string, Godot.Collections.Array>();
			var clothGenerationArrayVertexes = new Vector3[clothesGeneration.Count][];

			for (var i = 0; i < clothesGeneration.Count; i++)
			{
				var cloth = clothesGeneration[i];
				var _meshForArrays = ResourceLoader.Load(CheckOriginalMeshPath(cloth)) as ArrayMesh;
				var arr = arrayMeshesClothGenerationArray[cloth] = _meshForArrays.SurfaceGetArrays(0);
				clothGenerationArrayVertexes[i] = ((Array<Vector3>)arr[(int)Mesh.ArrayType.Vertex]).ToArray();
			}

			var length = clothesGeneration.Count;
			var meshArray = new Mesh[length];

			Parallel.For(0, length, (i) =>
			{
				var cloth = clothesGeneration[i];
				var vertexProximityMapping = GetProximityMapping(clothGenerationArrayVertexes[i], bodyBaseForm);

				TransferBoneWeights(vertexProximityMapping,
					bodyBones,
					bodyBoneWeights,
					cloth == "body/eyelashes" ? excludedBoneIdsEyelashes : excludedBoneIds,
					excludedBonesReplacement,
					out var boneIndexes,
					out var boneWeights);

				var arr = arrayMeshesClothGenerationArray[cloth];
				arr[(int)Mesh.ArrayType.Weights] = boneWeights;
				arr[(int)Mesh.ArrayType.Bones] = boneIndexes;
				var _mesh = new ArrayMesh();
				_mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);

				Array<Vector3[]> _blendShapeArrays = new();

				for (var ii = 0; ii < processedBodyShapes.Count; ii++)
				{
					_blendShapeArrays.Add(TransferBlendshape(vertexProximityMapping, processedBodyShapes[ii]));
				}

				meshArray[i] = _mesh;


				lock (meshShapes)
				{
					meshShapes[cloth.Replace("/", "_")] = new()
					{
						new int[0],
						_blendShapeArrays,
						bodyShapeNameIndex.Duplicate()
					};
				}
			});

			await SetProcessingInfo("Saving meshes for clothes with generated data");

			for (var i = 0; i < length; i++)
			{
				var cloth = clothesGeneration[i];
				if (cloth.RFind("/") != -1) DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/meshes/" + cloth[..cloth.RFind("/")]);
				ResourceSaver.Save(meshArray[i], "res://godot_character_creator/assets/meshes/" + cloth + ".mesh");
				MakeScene(cloth);
			}
		}

		/// <summary>
		/// Saves the blend shape <paramref name="data"/> to individual files for all meshes
		/// </summary>
		/// <param name="data"></param>
		private void SaveData(Dictionary<string, Array<Variant>> data)
		{
			DirAccess.MakeDirAbsolute("res://godot_character_creator/blend_shape_data/");

			foreach (var dataPairs in data)
			{
				var clothName = dataPairs.Key;
				var file = FileAccess.OpenCompressed("res://godot_character_creator/blend_shape_data/" + clothName + ".txt", FileAccess.ModeFlags.Write, FileAccess.CompressionMode.Zstd);
				file.StoreVar(dataPairs.Value);
				file.Close();
			}
		}

		/// <summary>
		/// Compresses blendshapes by removing transformation values that are under a certain threshold
		/// </summary>
		/// <param name="meshShapes"></param>
		/// <returns></returns>
		private Dictionary<string, Array<Variant>> CompressData(Dictionary<string, Array<Variant>> meshShapes)
		{
			var keys = meshShapes.Keys.ToArray();

			Parallel.ForEach(keys, (clothKey) =>
			{
				var originalBlendShapes = (Array<Vector3[]>)meshShapes[clothKey][ShapeFileKeys.BLEND_SHAPES];
				var newBlendShapes = new Array<Vector3[]>();
				var newBlendShapeIndexes = new Array<int[]>();

				for (var i = 0; i < originalBlendShapes.Count; i++)
				{
					var originalBlendShape = originalBlendShapes[i];
					var blendShape = new System.Collections.Generic.List<Vector3>();
					var blendShapeIndex = new System.Collections.Generic.List<int>();
					var length = originalBlendShape.Length;

					for (var ii = 0; ii < length; ii++)
					{
						var vec = originalBlendShape[ii];

						if (Mathf.Abs(vec.X) + Mathf.Abs(vec.Y) + Mathf.Abs(vec.Z) > 0.00033f)
						{
							blendShape.Add(vec);
							blendShapeIndex.Add(ii);
						}
					}

					newBlendShapes.Add(blendShape.ToArray());
					newBlendShapeIndexes.Add(blendShapeIndex.ToArray());
				}

				lock (meshShapes)
				{
					meshShapes[clothKey][ShapeFileKeys.BLEND_SHAPES] = newBlendShapes;
					meshShapes[clothKey][ShapeFileKeys.BLEND_SHAPE_INDEXES] = newBlendShapeIndexes;
				}
			});

			return meshShapes;
		}

		/// <summary>
		/// Removes blend shapes that have an effective length of 0 from the <paramref name="meshShapes"/> dictionary
		/// </summary>
		/// <param name="meshShapes"></param>
		/// <returns></returns>
		private static Dictionary<string, Array<Variant>> StripUnusedBlendshapes(Dictionary<string, Array<Variant>> meshShapes)
		{
			foreach (var clothKey in meshShapes.Keys)
			{
				var shapeData = meshShapes[clothKey];
				var blendShapeNames = (Dictionary<string, int>)shapeData[ShapeFileKeys.SHAPE_NAME_INDEX];
				var blendShapes = (Array<Vector3[]>)shapeData[ShapeFileKeys.BLEND_SHAPES];
				var blendShapeIndexes = (Array<int[]>)shapeData[ShapeFileKeys.BLEND_SHAPE_INDEXES];

				var newShapeNameIndex = new Dictionary<string, int>();
				var newBlendShapes = new Array<Vector3[]>();
				var newBlendShapeIndexes = new Array<int[]>();

				foreach (var blendShapeKV in blendShapeNames)
				{
					var index = blendShapeNames[blendShapeKV.Key];
					if (blendShapes[index].Length > 0)
					{
						newShapeNameIndex[blendShapeKV.Key] = newBlendShapes.Count;
						newBlendShapes.Add(blendShapes[blendShapeKV.Value]);
						newBlendShapeIndexes.Add(blendShapeIndexes[blendShapeKV.Value]);
					}
				}

				meshShapes[clothKey][ShapeFileKeys.BLEND_SHAPES] = newBlendShapes;
				meshShapes[clothKey][ShapeFileKeys.BLEND_SHAPE_INDEXES] = newBlendShapeIndexes;
				meshShapes[clothKey][ShapeFileKeys.SHAPE_NAME_INDEX] = newShapeNameIndex;
			}

			return meshShapes;
		}

		private Skin skin;

		/// <summary>
		/// Generates a scene that contains a single mesh instance with the <paramref name="cloth"/>'s model and a material fitting for direct application to a character.
		/// If the material already exists is it not regenerated to preserve values
		/// </summary>
		/// <param name="cloth"></param>
		private void MakeScene(string cloth)
		{
			if (cloth.RFind("/") != -1) DirAccess.MakeDirAbsolute("res://godot_character_creator/assets/materials/" + cloth[..cloth.RFind("/")]);

			skin ??= ResourceLoader.Load<Skin>("res://godot_character_creator/assets/skins/body.tres");

			var path = "res://godot_character_creator/assets/meshes/" + cloth;
			var albedoPath = "res://godot_character_creator/assets/textures/" + cloth + "_albedo.png";
			var normalsPath = "res://godot_character_creator/assets/textures/" + cloth + "_normals.png";
			var materialPath = "res://godot_character_creator/assets/materials/" + cloth + ".tres";
			var mesh = ResourceLoader.Load(path + ".mesh") as Mesh;

			var material = ResourceLoader.Exists(materialPath) ? ResourceLoader.Load(materialPath) as StandardMaterial3D : new StandardMaterial3D();

			if (ResourceLoader.Exists(albedoPath))
			{
				material.AlbedoTexture = ResourceLoader.Load(albedoPath) as Texture2D;
			}

			if (ResourceLoader.Exists(normalsPath))
			{
				material.NormalEnabled = true;
				material.NormalTexture = ResourceLoader.Load(normalsPath) as Texture2D;
			}

			ResourceSaver.Save(material, materialPath);
			material = ResourceLoader.Load(materialPath) as StandardMaterial3D;

			var instance = new MeshInstance3D
			{
				MaterialOverride = material,
				Mesh = mesh,
				Name = cloth,
				Skin = skin
			};

			var scene = new PackedScene();

			scene.Pack(instance);

			ResourceSaver.Save(scene, path + ".tscn");
		}
	}
}
