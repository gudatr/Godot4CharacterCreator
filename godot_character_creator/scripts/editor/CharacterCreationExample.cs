using Godot;

namespace GCC
{

	public partial class CharacterCreationExample : Node
	{
		public async override void _Ready()
		{
			var creationGUI = (PackedScene)ResourceLoader.Load(CharacterData.resourceBasePath+"/scenes/character_creation_gui.tscn");
			CharacterEditorGUI instance = creationGUI.Instantiate() as CharacterEditorGUI;
			AddChild(instance);

			var characterBase = (PackedScene)ResourceLoader.Load(CharacterData.resourceBasePath+"/scenes/character_base.tscn");
			CharacterBase character = characterBase.Instantiate() as CharacterBase;

			instance.SetCharacter(character);

			character.ExpressionPlayer.EnableAutoBlink(2.5f, 0.4f);
			character.ExpressionPlayer.LetHeadWander(0.66f,1f,1f);
			character.ExpressionPlayer.LetEyesWander(0.2f, 6f, 1.5f, 0.5f, 0.7f);

			character.ExpressionPlayer.enabler = instance.camera;

			CharacterAudioBusManager.Setup(5, 5);
		}
	}

}
