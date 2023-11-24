using System.Linq;
using Godot;
using Godot.Collections;

namespace GCC
{
	public partial class CharacterEditorGUI : Control
	{
		float cartoonish = 1.0f;

		CharacterBase character;

		public Camera3D camera;

		FileDialog saveFile;

		FileDialog openFile;

		public readonly Array<CharacterMorphSlider> allMorphs = new();

		private void SetupMorphSliders()
		{

			var morphBase = FindChild("morphs");

			camera = FindChild("cam", true) as Camera3D;
			saveFile = FindChild("save_file", true) as FileDialog;
			openFile = FindChild("open_file", true) as FileDialog;

			var body = morphBase.FindChild("Body", true) as TabBar;
			var head = morphBase.FindChild("Head", true) as TabBar;
			var face = morphBase.FindChild("Face", true) as TabBar;
			var eyes = morphBase.FindChild("Eyes", true) as TabBar;
			var expressions = morphBase.FindChild("Expressions", true) as TabBar;

			var headIndex = 0;
			var faceIndex = 0;
			var eyesIndex = 0;
			var expressionsIndex = 0;
			var bodyIndex = 0;

			var morphSlider = (PackedScene)ResourceLoader.Load("res://godot_character_creator/scenes/character_creation_morph_slider.tscn");

			var ageChildButton = morphBase.FindChild("Age_Child", true) as Button;
			var ageTeenButton = morphBase.FindChild("Age_Teen", true) as Button;
			var ageYoungButton = morphBase.FindChild("Age_Young", true) as Button;
			var ageMiddleagedButton = morphBase.FindChild("Age_Middleaged", true) as Button;
			var ageOldButton = morphBase.FindChild("Age_Old", true) as Button;

			ageChildButton.Pressed += () => { character.SetAge(Age.Child); };
			ageTeenButton.Pressed += () => { character.SetAge(Age.Teen); };
			ageYoungButton.Pressed += () => { character.SetAge(Age.Young); };
			ageMiddleagedButton.Pressed += () => { character.SetAge(Age.MiddleAge); };
			ageOldButton.Pressed += () => { character.SetAge(Age.Old); };

			var sexMale = morphBase.FindChild("Sex_Male", true) as Button;
			var sexFemale = morphBase.FindChild("Sex_Female", true) as Button;
			var sexAmbiguous = morphBase.FindChild("Sex_Ambiguous", true) as Button;

			sexMale.Pressed += () => { character.SetSex(Sex.Male); };
			sexFemale.Pressed += () => { character.SetSex(Sex.Female); };
			sexAmbiguous.Pressed += () => { character.SetSex(Sex.Ambiguous); };

			var ethnicityAfrican = morphBase.FindChild("Ethnicity_African", true) as Button;
			var ethnicityAsianButton = morphBase.FindChild("Ethnicity_Asian", true) as Button;
			var ethnicityCaucasianButton = morphBase.FindChild("Ethnicity_Caucasian", true) as Button;

			ethnicityAfrican.Pressed += () => { character.SetEthnicity(Ethnicity.African); };
			ethnicityAsianButton.Pressed += () => { character.SetEthnicity(Ethnicity.Asian); };
			ethnicityCaucasianButton.Pressed += () => { character.SetEthnicity(Ethnicity.Caucasian); };

			TabBar tabParent;
			var index = 0;

			var blockedShapes = new Array<string>() {
				BlendShapes.ETHNICITY_AFRICAN,
				BlendShapes.ETHNICITY_ASIAN ,
				BlendShapes.ETHNICITY_CAUCASIAN,
				BlendShapes.HEAD_YOUNG,
				BlendShapes.HEAD_SCALE_X,
				BlendShapes.HEAD_SCALE_Y,
				BlendShapes.HEAD_SCALE_Z,
				BlendShapes.AGE_OLD,
				BlendShapes.AGE_CHILD,
				BlendShapes.SEX_MAN,
				BlendShapes.SEX_WOMAN_1,
				BlendShapes.SEX_WOMAN_2
			};

			var blendShapesWithNames = typeof(BlendShapes).GetFields().Select(field => (field.Name, field.GetValue(null).ToString())).ToList();


			foreach (var shapeWithName in blendShapesWithNames)
			{
				var shape = shapeWithName.Item2;
				var name = shapeWithName.Item1;

				if (blockedShapes.Contains(shape)) continue;

				var slider = morphSlider.Instantiate() as CharacterMorphSlider;
				slider.LabelText = name.Capitalize();
				if (slider.LabelText.Length != name.Length) slider.LabelText += ".";
				slider.shape = shape;

				if (shape.StartsWith("exp_"))
				{
					tabParent = expressions;
					index = expressionsIndex++;
				}
				else if (shape.StartsWith("eye"))
				{
					tabParent = eyes;
					index = eyesIndex++;
				}
				else if (shape.StartsWith("lip") || shape.StartsWith("nose"))
				{
					tabParent = face;
					index = faceIndex++;
				}
				else if (shape.StartsWith("jaw") || shape.StartsWith("cheek") || shape.StartsWith("head"))
				{
					tabParent = head;
					index = headIndex++;
				}
				else
				{
					tabParent = body;
					index = bodyIndex++;
				}

				tabParent.AddChild(slider);

				var xOffset = index % 3;
				var yOffset = index / 3;

				slider.Position = new Vector2(6 + xOffset * 224, 6 + yOffset * 55);

				slider.Connect("ChangeMorph", Callable.From(() => ChangeMorph(slider.shape, (float)slider.slider.Value)));

				allMorphs.Add(slider);
			}
		}

		public override void _Ready()
		{
			SetupMorphSliders();

			var path = OS.GetExecutablePath();
			path = path.Substring(0, path.RFind("\\") + 1);
			saveFile.CurrentDir = path;
			openFile.CurrentDir = path;

			var debugCheckbox = FindChild("debug_checkbox", true) as CheckBox;
			debugCheckbox.Pressed += () => { character.debug = debugCheckbox.ButtonPressed; };

			var random = FindChild("random_gen", true) as Button;
			random.Pressed += OnRandomGenPressed;

			var animate = FindChild("animation", true) as Button;
			animate.Pressed += Animate;

			var stopAnimation = FindChild("stop_animation", true) as Button;
			stopAnimation.Pressed += StopAnimation;

			var noHead = FindChild("no_head", true) as CheckBox;
			noHead.Pressed += () => { if (noHead.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Head); else character.ShowBodyParts(BodyPartsHidden.Head); };

			var noArms = FindChild("no_arms", true) as CheckBox;
			noArms.Pressed += () => { if (noArms.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Arms); else character.ShowBodyParts(BodyPartsHidden.Arms); };

			var noHands = FindChild("no_hands", true) as CheckBox;
			noHands.Pressed += () => { if (noHands.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Hands); else character.ShowBodyParts(BodyPartsHidden.Hands); };

			var noTorso = FindChild("no_torso", true) as CheckBox;
			noTorso.Pressed += () => { if (noTorso.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Torso); else character.ShowBodyParts(BodyPartsHidden.Torso); };

			var noLegs = FindChild("no_legs", true) as CheckBox;
			noLegs.Pressed += () => { if (noLegs.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Legs); else character.ShowBodyParts(BodyPartsHidden.Legs); };

			var noFeet = FindChild("no_feet", true) as CheckBox;
			noFeet.Pressed += () => { if (noFeet.ButtonPressed) character.HideBodyParts(BodyPartsHidden.Feet); else character.ShowBodyParts(BodyPartsHidden.Feet); };

			var speak = FindChild("speak", true) as Button;
			var audio = ResourceLoader.Load("res://godot_character_creator/assets/audio/lorem_ipsum.mp3") as AudioStream;
			speak.Pressed += () => character.ExpressionPlayer.Speak(audio, true, () => GD.Print("Speak callback"), 0.15f);

			var stopSpeak = FindChild("stop_speak", true) as Button;
			stopSpeak.Pressed += () => character.ExpressionPlayer.StopSpeaking();

			var resetAppearance = FindChild("reset_appearance", true) as Button;
			resetAppearance.Pressed += () => character.ResetAppearance(); //Lambda is not unnecessary

			var focus = FindChild("focus", true) as Button;
			focus.Pressed += () =>
			{
				character.ExpressionPlayer.LookAt(camera);
				character.ExpressionPlayer.DisableHeadWandering();
			};

			var stopFocus = FindChild("stop_focus", true) as Button;
			stopFocus.Pressed += () =>
			{
				character.ExpressionPlayer.LookAt(null);
				character.ExpressionPlayer.LetHeadWander();
			};

			var browsSlider = FindChild("brows_slider", true) as HSlider;
			browsSlider.Connect("value_changed", Callable.From((Variant _) => BrowSlider((int)browsSlider.Value)));

			var eyesSlider = FindChild("eyes_slider", true) as HSlider;
			eyesSlider.Connect("value_changed", Callable.From((Variant _) => EyeSlider((int)eyesSlider.Value)));

			var cartoonishSlider = random.FindChild("HSlider") as HSlider;
			cartoonishSlider.Connect("value_changed", Callable.From((Variant _) => SetCartoonish((float)cartoonishSlider.Value)));

			var hair = DirAccess.GetFilesAt("res://godot_character_creator/assets/meshes/hair");
			var hairVBox = FindChild("hair_vbox");

			foreach (var hair_cloth in hair)
			{
				if (!hair_cloth.EndsWith(".mesh")) continue;

				var checkbox = new CheckBox();
				var text = hair_cloth[..hair_cloth.RFind(".")];
				checkbox.Text = text.Capitalize();
				checkbox.Pressed += () => { OnToggled("hair/" + text, checkbox.ButtonPressed); };
				hairVBox.AddChild(checkbox);
			}

			var clothes = DirAccess.GetFilesAt("res://godot_character_creator/assets/meshes/clothes");
			var clothesVBox = FindChild("clothes_vbox");

			foreach (var clothes_cloth in clothes)
			{
				if (!clothes_cloth.EndsWith(".mesh")) continue;

				var checkbox = new CheckBox();
				var text = clothes_cloth[..clothes_cloth.RFind(".")];
				checkbox.Text = text.Capitalize();
				checkbox.Pressed += () => { OnToggled("clothes/" + text, checkbox.ButtonPressed); };
				clothesVBox.AddChild(checkbox);
			}
		}

		public void ChangeMorph(string text, float value)
		{
			character.UpdateMorph(text, value);
		}

		public void SetCharacter(CharacterBase character)
		{
			if (this.character != null) camera.RemoveChild(this.character);

			this.character = character;

			camera.FindChild("spt").AddChild(this.character);

			ResetAllSliders();
		}

		public void OnSavePressed()
		{
			saveFile.Popup();
			saveFile.Invalidate();
		}

		public void OnOpenPressed()
		{
			openFile.Popup();
			openFile.Invalidate();
		}

		public async void OnSaveFileSelected(string filePath)
		{
			await character.Save(filePath);
		}

		public async void OnOpenFileSelected(string filePath)
		{
			await character.Load(filePath);
			ResetAllSliders();
		}

		public void ResetAllSliders()
		{
			foreach (var morph in allMorphs)
			{
				if (character.Appearance.TryGetValue(morph.shape, out var value))
				{
					morph.SetSlider(value);
				}
				else
				{
					morph.SetSlider(0);
				}
			}
		}

		public void Animate()
		{
			character.AnimationPlayer.Stop();
		}

		public void StopAnimation()
		{
			character.AnimationPlayer.Play("idle2/mixamo_com");
		}

		public void OnRandomGenPressed()
		{
			character.RandomFaceGen(cartoonish);

			ResetAllSliders();
		}

		public void SetCartoonish(float value)
		{
			cartoonish = value;
		}

		public async void OnToggled(string cloth, bool buttonPressed)
		{
			if (buttonPressed)
			{
				await character.TakeOnClothes(cloth);
			}
			else
			{
				await character.TakeOffClothes(cloth);
			}
		}

		public async void BrowSlider(int value)
		{
			switch (value)
			{
				case 0:
					await character.RemoveOverlay("body/body", "brows");
					break;
				case 1:
					await character.AddOrReplaceOverlay("body/body", "brows", "res://godot_character_creator/assets/materials/overlays/brows01.tres");
					break;
				case 2:
					await character.AddOrReplaceOverlay("body/body", "brows", "res://godot_character_creator/assets/materials/overlays/brows02.tres");
					break;
				default:
					await character.AddOrReplaceOverlay("body/body", "brows", "res://godot_character_creator/assets/materials/overlays/brows03.tres");
					break;
			}
		}

		public async void EyeSlider(int value)
		{
			switch (value)
			{
				case 0:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/brown_eyes.tres");
					break;
				case 1:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/amber_eyes.tres");
					break;
				case 2:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/yellow_eyes.tres");
					break;
				case 3:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/green_eyes.tres");
					break;
				case 4:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/turquoise_eyes.tres");
					break;
				case 5:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/blue_eyes.tres");
					break;
				case 6:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/grey_eyes.tres");
					break;
				case 7:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/red_eyes.tres");
					break;
				default:
					await character.ChangeBaseMaterial("body/eyes", "res://godot_character_creator/assets/materials/eyes/brown_eyes.tres");
					break;
			}
		}
	}
}
