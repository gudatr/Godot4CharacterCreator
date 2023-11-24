using Godot;
using System;

namespace GCC
{

    /// <summary>
    /// The <c>CharacterAudioBusManager</c> manages the buses used for the characters' Speak method.
    /// Use high priority buses for dialogue the player needs to hear and the low priority ones for background characters.
    /// Technically there should be no need for a bus count limit but this class provides one.
    /// </summary>
    public class CharacterAudioBusManager
    {
        private static bool setup = false;

        private static readonly object busLock = new();

        private static ManagedAudioBus[] highPriorityBuses;

        private static ManagedAudioBus[] lowPriorityBuses;

        private static AudioEffectSpectrumAnalyzerInstance[] specturmAnalyers;

        /// <summary>
        /// The initializes a set of buses with <paramref name="highPriorityCount"/> high priority and <paramref name="lowPriorityCount"/> low priority buses. They will be named with their index and the specified <paramref name="busPrefix"/>. This does not affect anything, change it if you already have buses following that naming scheme.
        /// </summary>
        /// <param name="highPriorityCount"></param>
        /// <param name="lowPriorityCount"></param>
        /// <param name="busPrefix"></param>
        /// <exception cref="Exception">Setup has already been run</exception>
        public static void Setup(int highPriorityCount, int lowPriorityCount, string busPrefix = "GCC")
        {
            lock (busLock)
            {
                if (setup) throw new Exception("CharacterAudioBusManager setup has already been run");

                setup = true;

                int busIndex = AudioServer.BusCount;

                specturmAnalyers = new AudioEffectSpectrumAnalyzerInstance[busIndex + highPriorityCount + lowPriorityCount];
                highPriorityBuses = new ManagedAudioBus[highPriorityCount];
                lowPriorityBuses = new ManagedAudioBus[lowPriorityCount];


                foreach (var buses in new[] { highPriorityBuses, lowPriorityBuses })
                {
                    for (int i = 0; i < buses.Length; i++)
                    {
                        var managedBus = buses[i] = new ManagedAudioBus()
                        {
                            index = busIndex++,
                            lockTime = 0,
                        };
                        var effect = new AudioEffectSpectrumAnalyzer
                        {
                            BufferLength = 10,
                            //This has high latency but everything else is too janky. Related issue: https://github.com/godotengine/godot/issues/67650
                            FftSize = AudioEffectSpectrumAnalyzer.FftSizeEnum.Size4096
                        };
                        AudioServer.AddBus(managedBus.index);
                        AudioServer.SetBusName(managedBus.index, busPrefix + managedBus.index);
                        AudioServer.AddBusEffect(managedBus.index, effect, 0);
                        specturmAnalyers[managedBus.index] = AudioServer.GetBusEffectInstance(managedBus.index, 0) as AudioEffectSpectrumAnalyzerInstance;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current volume for the bus with the index <paramref name="bus"/>
        /// </summary>
        /// <param name="bus"></param>
        /// <returns>The current magnitude</returns>
        public static float GetSpectrum(int bus)
        {
            var effect = specturmAnalyers[bus];

            return effect.GetMagnitudeForFrequencyRange(0, 10000).Length();
        }

        public static int GetBus(float lockSeconds, bool highPriority)
        {
            var timestamp = GetTimestamp();

            var lockToTicks = lockSeconds * 10_000_000;

            lock (busLock)
            {
                var busList = highPriority ? highPriorityBuses : lowPriorityBuses;

                for (var i = 0; i < busList.Length; i++)
                {
                    var bus = busList[i];

                    if (bus.lockTime <= timestamp)
                    {
                        bus.lockTime = timestamp + lockToTicks;

                        return bus.index;
                    }
                }

                return -1;
            }
        }

        public static double GetTimestamp()
        {
            return DateTime.Now.Ticks;
        }

        private class ManagedAudioBus
        {
            public int index;
            public double lockTime;
        }
    }


}
