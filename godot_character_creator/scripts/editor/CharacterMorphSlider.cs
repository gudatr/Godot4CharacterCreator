
using Godot;

namespace GCC
{
	///<summary>
	///This class represents a slider on the character creation GUI.
	///It emits the ChangeMorph signal when its value is altered.
	///</summary>
	public partial class CharacterMorphSlider : Label
	{
		[Signal]
		public delegate void ChangeMorphEventHandler(string text, float value);

		public HSlider slider;

		double lastValue = 0f;

		public string shape;

		private string _text;

		public string LabelText
		{
			set
			{
				_text = value;
				Text = value;
			}
			get
			{
				return _text;
			}
		}

		public override void _Ready()
		{
			slider = GetNode("HSlider") as HSlider;

			slider.Value = 0f;

			slider.ValueChanged += (double _) => EmitSignal("ChangeMorph");
		}

		public void SetSlider(float value)
		{
			slider.Value = value;
		}
	}
}
