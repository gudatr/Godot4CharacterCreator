using Godot;
using System;
using System.Collections.Generic;
namespace GCC
{

	public partial class CharacterExpressionPlayer : Skeleton3D
	{

		/// <summary>
		/// A mapping of the character expressions to their blend shape names
		/// </summary>
		private static Dictionary<CharacterExpression, string> expressionValues = new()
		{
			{
				CharacterExpression.Indifferent,
				"none"
			},
			{
				CharacterExpression.Angry,
				BlendShapes.EXPRESSION_ANGRY
			},
			{
				CharacterExpression.Bored,
				BlendShapes.EXPRESSION_BORED
			},
			{
				CharacterExpression.Crying,
				BlendShapes.EXPRESSION_CRYING
			},
			{
				CharacterExpression.Determined,
				BlendShapes.EXPRESSION_DETERMINED
			},
			{
				CharacterExpression.Disgusted,
				BlendShapes.EXPRESSION_DISGUSTED
			},
			{
				CharacterExpression.Doubting,
				BlendShapes.EXPRESSION_DOUBTING
			},
			{
				CharacterExpression.Strained,
				BlendShapes.EXPRESSION_STRAINED
			},
			{
				CharacterExpression.Suprised,
				BlendShapes.EXPRESSION_SURPRISED
			},
			{
				CharacterExpression.Pained,
				BlendShapes.EXPRESSION_PAINED
			},
			{
				CharacterExpression.Pitying,
				BlendShapes.EXPRESSION_PITYING
			},
			{
				CharacterExpression.Sad,
				BlendShapes.EXPRESSION_SAD
			},
			{
				CharacterExpression.Sleeping,
				BlendShapes.EXPRESSION_SLEEPING
			},
			{
				CharacterExpression.Sick,
				BlendShapes.EXPRESSION_SICK
			},
			{
				CharacterExpression.Stunned,
				BlendShapes.EXPRESSION_STUNNED
			},
			{
				CharacterExpression.Smirking,
				BlendShapes.EXPRESSION_SMIRKING
			},
			{
				CharacterExpression.Smiling1,
				BlendShapes.EXPRESSION_SMILING_1
			},
			{
				CharacterExpression.Smiling2,
				BlendShapes.EXPRESSION_SMILING_2
			},
			{
				CharacterExpression.Smiling3,
				BlendShapes.EXPRESSION_SMILING_3
			},
			{
				CharacterExpression.Kissing,
				BlendShapes.EXPRESSION_KISSING
			},
			{
				CharacterExpression.Laughing,
				BlendShapes.EXPRESSION_LAUGHING
			},
			{
				CharacterExpression.Threaten,
				BlendShapes.EXPRESSION_THREATENING
			},
			{
				CharacterExpression.Concentrating,
				BlendShapes.EXPRESSION_CONCENTRATING
			},
			{
				CharacterExpression.Aiming,
				BlendShapes.EXPRESSION_AIMING
			},
			{
				CharacterExpression.Vocal,
				BlendShapes.EXPRESSION_VOCAL
			},
			{
				CharacterExpression.Devicious,
				BlendShapes.EXPRESSION_DEVICIOUS
			},
			{
				CharacterExpression.Gleeful,
				BlendShapes.EXPRESSION_GLEEFUL
			}
		};


		private CharacterBase character;

		private AudioStreamPlayer3D audioPlayer;

		private float expressionFadeTimeTotal = 0f;
		private float expressionFadeTimeCurrent = 0f;

		private CharacterExpression currentExpressionEnum = CharacterExpression.Indifferent;
		private string currentExpression = expressionValues[CharacterExpression.Indifferent];
		private float currentExpressionIntensity = 1f;

		private CharacterExpression nextExpressionEnum = CharacterExpression.Indifferent;
		private string nextExpression = expressionValues[CharacterExpression.Indifferent];
		private float nextExpressionIntensity = 1f;

		private int jawIndex = 0;
		private float jawBaseAngle = 0;

		private SkeletonIK3D headSkeletonIK;
		private Node3D headTarget;
		private Node3D headTargetReset;
		private Vector3 headTargetResetDefaultLocalPosition;
		private float headWanderIntensity = 0.1f;
		private float headWanderScale = 0.1f;
		private float headWanderWait = 0.1f;
		private float headWanderWaitDeviation = 0.3f;
		private float headWanderWaitCurrent = 0.1f;
		private float headWanderFocusChance = 0.1f;
		private bool headWanderActive = false;

		private SkeletonIK3D eyeLSkeletonIK;
		private SkeletonIK3D eyeRSkeletonIK;
		private Node3D eyeTarget;
		private Node3D eyeTargetReset;
		private Vector3 eyeTargetResetDefaultLocalPosition;
		private Vector3 eyeWanderLastPosition;
		private Node3D eyeLTarget;
		private Node3D eyeRTarget;
		private float eyeWanderIntensity = 0.3f;
		private float eyeWanderSpeed = 0.3f;
		private float eyeWanderScale = 0.3f;
		private float eyeWanderWait = 0.3f;
		private float eyeWanderWaitDeviation = 0.3f;
		private float eyeWanderWaitCurrent = 0.3f;
		private float eyeWanderFocusChance = 0.1f;
		private bool eyeWanderActive = false;

		private int rightUpperLidIndex = 0;
		private int leftUpperLidIndex = 0;

		private Node3D lookAtTarget;
		private Vector3 lookAtLastPosition;
		private float lookAtIntensity = 0f;
		private float lookAtSpeed = 2f;

		/// <summary>
		/// This node controls how detailed the expressions are. Usually you want this to be the current Camera.
		/// Set it to null to always enable all features.
		/// </summary>
		public Node3D enabler;

		/// <summary>
		/// The distance to the enabler node at which expressions will be enabled
		/// </summary>
		public float enablerDistanceExpressions = 5;

		/// <summary>
		/// The distance to the enabler node at which blinking will be enabled
		/// </summary>
		public float enablerDistanceBlinking = 10;

		/// <summary>
		/// The distance to the enabler node at which speech will be enabled. This is rather costly as it performs not only bone but facial transformations as well, keep the value low.
		/// </summary>
		public float enablerDistanceSpeech = 30;

		private bool autoBlink = false;
		private bool autoBlinkClosing = false;
		private float autoBlinkSpeed = 5f;
		private float autoBlinkClosed = 0f;
		private float autoBlinkBlinkTime = 0f;
		private float autoBlinkInterval = 0f;
		private float autoBlinkDeviation = 0f;

		private Action speechCallback;
		private float speechIntensity = 0.2f;
		private float speechDuration;
		private float speechLipsSize;
		private bool expressionsEnabled = false;
		private int speechAudioBus;

		private Random randomGenerator;

		public override void _Ready()
		{
			ResetBonePoses();

			randomGenerator = new();

			character = GetParent() as CharacterBase;

			jawIndex = FindBone("Jaw");
			jawBaseAngle = GetBonePoseRotation(jawIndex).Y;

			leftUpperLidIndex = FindBone("uplid.L");
			rightUpperLidIndex = FindBone("uplid.R");

			audioPlayer = character.GetNode("audio") as AudioStreamPlayer3D;
			headTarget = character.GetNode("target_head") as Node3D;
			headTargetReset = character.GetNode("target_head_reset") as Node3D;
			headTargetResetDefaultLocalPosition = headTargetReset.Position;
			lookAtLastPosition = headTargetReset.GlobalPosition;
			eyeTarget = GetNode("head_attachment/target_eye") as Node3D;
			eyeTargetReset = GetNode("head_attachment/target_eye_reset") as Node3D;
			eyeTargetResetDefaultLocalPosition = eyeTargetReset.Position;

			headSkeletonIK = GetNode("ik_head") as SkeletonIK3D;
			headSkeletonIK.TargetNode = headTarget.GetPath();
			headSkeletonIK.Interpolation = 1f;
			headSkeletonIK.Start();

			eyeRTarget = eyeTarget.GetNode("target_ik_eye_r") as Node3D;
			eyeRSkeletonIK = GetNode("ik_eye_r") as SkeletonIK3D;
			eyeRSkeletonIK.TargetNode = eyeRTarget.GetPath();
			eyeRSkeletonIK.Interpolation = 1f;
			eyeRSkeletonIK.Start();

			eyeLTarget = eyeTarget.GetNode("target_ik_eye_l") as Node3D;
			eyeLSkeletonIK = GetNode("ik_eye_l") as SkeletonIK3D;
			eyeLSkeletonIK.TargetNode = eyeLTarget.GetPath();
			eyeLSkeletonIK.Interpolation = 1f;
			eyeLSkeletonIK.Start();
		}

		/// <summary>
		/// Returns the current expression and its intensity and the next expression and its intensity
		/// </summary>
		public (CharacterExpression, float, CharacterExpression, float) GetExpressionInfo()
		{
			return (currentExpressionEnum, currentExpressionIntensity,
				nextExpressionEnum, nextExpressionIntensity);
		}

		/// <summary>
		/// Set the <paramref name="expression"/> immediately to the given <paramref name="intensity"/>
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="intensity"></param>
		public void SetExpression(CharacterExpression expression, float intensity)
		{
			nextExpression = currentExpression = expressionValues[expression];
			nextExpressionEnum = currentExpressionEnum = expression;
			nextExpressionIntensity = currentExpressionIntensity = intensity;

			expressionFadeTimeTotal = expressionFadeTimeCurrent = 1f;

			UpdateExpression();
		}

        /// <summary>
        /// Fade the <paramref name="expression"/> over <paramref name="time"/> seconds to the given <paramref name="intensity"/>
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="intensity"></param>
        /// <param name="time"></param>
        public void FadeToExpression(CharacterExpression expression, float intensity, float time = 0.33f)
		{
			currentExpression = nextExpression;
			currentExpressionEnum = nextExpressionEnum;
			currentExpressionIntensity = nextExpressionIntensity;

			nextExpression = expressionValues[expression];
			nextExpressionEnum = expression;
			nextExpressionIntensity = intensity;

			expressionFadeTimeCurrent = 0f;
			expressionFadeTimeTotal = time;
		}

		public override void _Process(double delta)
		{
			var fdelta = (float)delta;

			float enablerDistance = enabler == null ? 0 : (enabler.GlobalPosition - character.GlobalPosition).Length();

			expressionsEnabled = enablerDistance <= enablerDistanceExpressions;

			if (enablerDistance <= enablerDistanceSpeech) ProcessSpeech(fdelta);

			if (enablerDistance <= enablerDistanceBlinking) ProcessAutoBlinking(fdelta);

			ProcessExpression(fdelta);

			ProcessLookAt(fdelta);

			ProcessHeadWandering(fdelta);

			ProcessEyeWandering(fdelta);
		}

		/// <summary>
		/// Moves the headTarget to the lookAtTarget. Takes in the time since the last frame <paramref name="delta"/>
		/// </summary>
		/// <param name="delta"></param>
		private void ProcessLookAt(float delta)
		{
			float _speed = Mathf.Sin(Mathf.Pi * (0.90f * lookAtIntensity + 0.08f)) * lookAtSpeed;

			lookAtIntensity = Mathf.Clamp(lookAtIntensity + _speed * delta, 0f, 1f);

			var lookAtPosition = lookAtTarget == null ? headTargetReset.GlobalPosition : lookAtTarget.GlobalPosition;
			headTarget.GlobalPosition = lookAtLastPosition * (1f - lookAtIntensity) + lookAtPosition * lookAtIntensity;
		}

        /// <summary>
        /// Adjusts the jaw bone and LIPS_THINNER blend shape if the chracter is currently speaking. Takes in the time since the last frame <paramref name="delta"/> and invokes the callback if the character is done speaking
        /// </summary>
        /// <param name="delta"></param>
        private void ProcessSpeech(float delta)
		{
			if (speechDuration <= 0) return;

			speechDuration -= delta;

			var jawRotation = GetBonePoseRotation(jawIndex);

			if (speechDuration <= 0)
			{
				jawRotation.Y = jawBaseAngle;

				character.UpdateMorph(BlendShapes.LIPS_THINNER, speechLipsSize);

				speechCallback?.Invoke();
			}
			else
			{
				var value = CharacterAudioBusManager.GetSpectrum(speechAudioBus);

				value = Mathf.Clamp(value * 25f * speechIntensity, 0f, speechIntensity);

				if (expressionsEnabled) character.UpdateMorph(BlendShapes.LIPS_THINNER, speechLipsSize + value * 10f);

				jawRotation.Y = jawBaseAngle - value;
			}

			SetBonePoseRotation(jawIndex, jawRotation);

		}

        /// <summary>
        /// Processes eye wandering by moving the eyeTarget to a random location within the field of view. The corresponding SkeletonIK3D will then adjust the bones accordingly.
        /// </summary>
        /// <param name="delta"></param>
        private void ProcessEyeWandering(float delta)
		{
			eyeWanderIntensity = Mathf.Clamp(eyeWanderIntensity + eyeWanderSpeed * delta, 0f, 1f);

			eyeTarget.Position = eyeWanderLastPosition * (1f - eyeWanderIntensity) + eyeTargetReset.Position * eyeWanderIntensity;

			if (eyeWanderIntensity == 1f)
			{
				if (eyeWanderWaitCurrent < 0)
				{
					eyeWanderWaitCurrent = eyeWanderWait + randomGenerator.NextSingle() * eyeWanderWaitDeviation;
					return;
				}

				eyeWanderWaitCurrent -= delta;

				if (eyeWanderWaitCurrent < 0)
				{
					eyeWanderLastPosition = eyeTargetReset.Position;
					eyeTargetReset.Position = eyeTargetResetDefaultLocalPosition;
					eyeWanderIntensity = 0f;

					if (randomGenerator.NextSingle() > headWanderFocusChance)
					{
						var randomVector = new Vector3(randomGenerator.NextSingle(), randomGenerator.NextSingle(), 0f);
						eyeTargetReset.Position += eyeWanderScale * randomVector - new Vector3(0.4f, 0.4f, 0.4f);
					}
				}
			}
		}

        /// <summary>
        /// Processes head wandering by moving the headTarget to a random location within the field of view. The corresponding SkeletonIK3D will then adjust the bones accordingly.
        /// </summary>
        /// <param name="delta"></param>
        private void ProcessHeadWandering(float delta)
		{
			if (lookAtTarget == null && lookAtIntensity == 1f)
			{
				if (headWanderWaitCurrent < 0)
				{
					headWanderWaitCurrent = headWanderWait + randomGenerator.NextSingle() * headWanderWaitDeviation;
					return;
				}

				headWanderWaitCurrent -= delta;

				if (headWanderWaitCurrent < 0)
				{
					lookAtLastPosition = headTarget.GlobalPosition;
					headTargetReset.Position = headTargetResetDefaultLocalPosition;
					lookAtIntensity = 0f;

					if (headWanderActive && randomGenerator.NextSingle() > headWanderFocusChance)
					{
						var randomVector = new Vector3(randomGenerator.NextSingle(), randomGenerator.NextSingle(), 0f);
						headTargetReset.Position += headWanderScale * randomVector - new Vector3(0.5f, 0.5f, 0.5f);
					}
				}
			}
		}

		/// <summary>
		/// Fades the current expression using the given <paramref name="delta"/>
		/// </summary>
		/// <param name="delta"></param>
		private void ProcessExpression(float delta)
		{
			var originalValue = expressionFadeTimeCurrent;

			expressionFadeTimeCurrent = Math.Clamp(expressionFadeTimeCurrent + delta, 0f, expressionFadeTimeTotal);

			if (originalValue < expressionFadeTimeCurrent) UpdateExpression();
		}

        /// <summary>
        /// Blends between the current and the next expression based on <c>expressionFadeTimeCurrent</c> and <c>expressionFadeTimeTotal</c>
        /// </summary>
        private void UpdateExpression()
		{
			var factor = expressionFadeTimeTotal / expressionFadeTimeCurrent;
			var currentIntensity = currentExpressionIntensity * factor;
			var nextIntensity = nextExpressionIntensity * factor;

			if(expressionsEnabled) character.UpdateMorphs(new[] { currentExpression, nextExpression }, new[] { currentIntensity, nextIntensity });
		}

		/// <summary>
		/// Adjusts the eye lid bones based on the value of <paramref name="closed"/>
		/// </summary>
		/// <param name="closed"></param>
		private void AdjustEyeLids(float closed)
		{
			var positionLeft = GetBonePosePosition(leftUpperLidIndex);
			var positionRight = GetBonePosePosition(rightUpperLidIndex);

			var value = 0.057f - 0.03f * closed;

			positionLeft.Y = value;
			positionRight.Y = value;

			SetBonePosePosition(leftUpperLidIndex, positionLeft);
			SetBonePosePosition(rightUpperLidIndex, positionRight);
		}

		/// <summary>
		/// Calculates the blinking interval for the character and sets the value for AdjustEyeLids
		/// </summary>
		/// <param name="delta"></param>
		private void ProcessAutoBlinking(float delta)
		{
			var deltaSpeed = delta * autoBlinkSpeed;

			if (!autoBlink || currentExpressionEnum == CharacterExpression.Sleeping)
			{
				if (autoBlinkClosed > 0)
				{
					autoBlinkClosed -= deltaSpeed;

					if (autoBlinkClosed < 0) deltaSpeed += autoBlinkClosed;

					AdjustEyeLids(-deltaSpeed);
				}

				return;
			}

			if (autoBlinkClosing)
			{
				autoBlinkClosed += deltaSpeed;

				if (autoBlinkClosed >= 0.3f)
				{
					autoBlinkClosed = 0.3f;
					autoBlinkClosing = false;
				}
			}
			else
			{
				if (autoBlinkClosed <= 0)
				{
					autoBlinkBlinkTime -= delta;

					if (autoBlinkBlinkTime <= 0)
					{
						autoBlinkClosing = true;
					}

					return;
				}

				autoBlinkClosed -= deltaSpeed;

				if (autoBlinkClosed < 0)
				{
					autoBlinkClosed = 0;
					autoBlinkBlinkTime = autoBlinkInterval + autoBlinkDeviation * (randomGenerator.NextSingle() - 0.5f);
				}
			}

			AdjustEyeLids(autoBlinkClosed);
		}

        /// <summary>
        /// Play audio from a <paramref name="stream"/> on a high or low priority bus specified by <paramref name="highPriority"/>. At the end of playing the audio, <paramref name="callback"/> will be invoked. Set <paramref name="speechIntensity"/> to adjust how much the jaw moves. Returns true if there was a free bus and the sound is playing.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="highPriority"></param>
        /// <param name="callback"></param>
        /// <param name="speechIntensity"></param>
        /// <returns></returns>
        public bool Speak(AudioStream stream, bool highPriority, Action callback = null, float speechIntensity = 0.2f)
		{
			var duration = (float)stream.GetLength();

			character.Appearance.TryGetValue(BlendShapes.LIPS_THINNER, out speechLipsSize);

			speechAudioBus = CharacterAudioBusManager.GetBus(duration, highPriority);

			if (speechAudioBus == -1) return false;

			this.speechIntensity = speechIntensity;
			speechDuration = duration;
			audioPlayer.Bus = AudioServer.GetBusName(speechAudioBus);
			audioPlayer.Stream = stream;
			audioPlayer.Play();

			if (speechCallback != null)
			{
				speechCallback();
				speechCallback = callback;
			}

			return true;
		}

		/// <summary>
		/// Stop speaking. Invokes the callback if not stopped by <paramref name="disableCallback"/>
		/// </summary>
		/// <param name="disableCallback"></param>
		public void StopSpeaking(bool disableCallback = false)
		{
			audioPlayer.Stop();
			speechDuration = 0;
			if (!disableCallback) speechCallback?.Invoke();

			var jawRotation = GetBonePoseRotation(jawIndex);
			character.UpdateMorph(BlendShapes.LIPS_THINNER, 0);
			jawRotation.Y = jawBaseAngle;
			SetBonePoseRotation(jawIndex, jawRotation);
		}

		/// <summary>
		/// Sets the lookAtTarget to <paramref name="target"/>
		/// </summary>
		/// <param name="target"></param>
		public void LookAt(Node3D target)
		{
			lookAtIntensity = 0.0001f;
			lookAtTarget = target;
			lookAtLastPosition = headTarget.GlobalPosition;
		}

		/// <summary>
		/// Enables eye wandering. The eyes will rotate at a random location within the view that depends on <paramref name="scale"/> with the given <paramref name="speed"/>. There is a <paramref name="focusChance"/> that the eyes will reset their rotation. After finishing rotation wait for <paramref name="wait"/> seconds with a deviation of <paramref name="waitDeviation"/> seconds. 
		/// </summary>
		/// <param name="scale"></param>
		/// <param name="speed"></param>
		/// <param name="wait"></param>
		/// <param name="waitDeviation"></param>
		/// <param name="focusChance"></param>
		public void LetEyesWander(float scale, float speed, float wait, float waitDeviation, float focusChance)
		{
			eyeWanderScale = scale;
			eyeWanderSpeed = speed;
			eyeWanderWait = wait;
			eyeWanderWaitDeviation = waitDeviation;
			eyeWanderFocusChance = focusChance;
			eyeWanderActive = true;

			if (eyeWanderIntensity == 0) eyeWanderIntensity = 0.001f;
		}

        /// <summary>
        /// Enables head wandering. The head will rotate at a random location within the view that depends on <paramref name="scale"/>. After finishing rotation wait for <paramref name="wait"/> seconds with a deviation of <paramref name="waitDeviation"/> seconds. 
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="wait"></param>
        /// <param name="waitDeviation"></param>
        public void LetHeadWander(float scale, float wait = 1f, float waitDeviation = 0.3333f)
		{
			headWanderWaitDeviation = waitDeviation;
			headWanderWait = wait;
			headWanderScale = scale;
			headWanderActive = true;

			if (headWanderIntensity == 0) headWanderIntensity = 0.001f;
		}

		/// <summary>
		/// Enables head wandering
		/// </summary>
		public void LetHeadWander()
		{
			headWanderActive = true;
			if (headWanderIntensity == 0) headWanderIntensity = 0.001f;
		}

		/// <summary>
		/// Disables head wandering
		/// </summary>
		public void DisableHeadWandering()
		{
			headWanderActive = false;
		}

		/// <summary>
		/// Disables eye wandering
		/// </summary>
		public void DisableEyeWandering()
		{
			eyeWanderActive = false;
		}

		/// <summary>
		/// Enables auto blinking in intervals of <paramref name="interval"/> seconds with a deviation of <paramref name="deviation"/> seconds and a given <paramref name="speed"/>
		/// </summary>
		/// <param name="interval"></param>
		/// <param name="deviation"></param>
		/// <param name="speed"></param>
		public void EnableAutoBlink(float interval, float deviation, float speed = 5.0f)
		{
			autoBlinkDeviation = deviation;
			autoBlinkInterval = interval;
			autoBlinkSpeed = speed;
			autoBlink = true;
		}

		/// <summary>
		/// Disables auto blinking
		/// </summary>
		public void DisableAutoBlink()
		{
			autoBlink = false;
		}
	}

	public enum CharacterExpression
	{
		Indifferent,
		Angry,
		Bored,
		Crying,
		Determined,
		Disgusted,
		Doubting,
		Strained,
		Suprised,
		Pained,
		Pitying,
		Sad,
		Sleeping,
		Sick,
		Stunned,
		Smirking,
		Smiling1,
		Smiling2,
		Smiling3,
		Mischievous,
		Kissing,
		Laughing,
		Threaten,
		Concentrating,
		Aiming,
		Devicious,
		Vocal,
		Gleeful
	}
}
