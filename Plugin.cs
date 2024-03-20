using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Dissonance.Config;
using UnityEngine;

namespace DissonanceVoiceSettings
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        //internal static bool defaultAecDelayAgnostic = true;
        //internal static bool defaultAecExtendedFilter = true;
        //internal static bool defaultAecmComfortNoise = true;
        //internal static Dissonance.Audio.Capture.AecmRoutingMode defaultAecmRoutingMode = Dissonance.Audio.Capture.AecmRoutingMode.Disabled;
        //internal static bool defaultAecRefinedAdaptiveFilter = true;
        //internal static Dissonance.Audio.Capture.AecSuppressionLevels defaultAecSuppressionAmount = Dissonance.Audio.Capture.AecSuppressionLevels.Disabled;
        internal static bool defaultBackgroundSoundRemovalEnabled = false;
        internal static int defaultBackgroundSoundRemovalAmount = 65;
        internal static Dissonance.Audio.Capture.NoiseSuppressionLevels defaultDenoiseAmount = Dissonance.Audio.Capture.NoiseSuppressionLevels.Moderate;
        internal static bool defaultForwardErrorCorrection = true;
        internal static Dissonance.FrameSize defaultFrameSize = Dissonance.FrameSize.Small;
        internal static Dissonance.AudioQuality defaultQuality = Dissonance.AudioQuality.Medium;
        internal static int defaultVoiceDuckLevel = 100;
        internal static Dissonance.Audio.Capture.VadSensitivityLevels defaultVadSensitivity = Dissonance.Audio.Capture.VadSensitivityLevels.MediumSensitivity;

        //private ConfigEntry<bool> configAecDelayAgnostic;
        //private ConfigEntry<bool> configAecExtendedFilter;
        //private ConfigEntry<bool> configAecmComfortNoise;
        //private ConfigEntry<Dissonance.Audio.Capture.AecmRoutingMode> configAecmRoutingMode;
        //private ConfigEntry<bool> configAecRefinedAdaptiveFilter;
        //private ConfigEntry<Dissonance.Audio.Capture.AecSuppressionLevels> configAecSuppressionAmount;
        private ConfigEntry<bool> configBackgroundSoundRemovalEnabled;
        private ConfigEntry<int> configBackgroundSoundRemovalAmount;
        private ConfigEntry<Dissonance.Audio.Capture.NoiseSuppressionLevels> configDenoiseAmount;
        private ConfigEntry<bool> configForwardErrorCorrection;
        private ConfigEntry<Dissonance.FrameSize> configFrameSize;
        private ConfigEntry<int> configVoiceDuckLevel;
        private ConfigEntry<Dissonance.Audio.Capture.VadSensitivityLevels> configVadSensitivity;
        private ConfigEntry<Dissonance.AudioQuality> configQuality;

        private void Awake()
        {
            // Plugin startup logic
            Plugin.Log = base.Logger;

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.BackgroundSoundRemovalEnabled = {VoiceSettings.Instance.BackgroundSoundRemovalEnabled}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.BackgroundSoundRemovalAmount = {VoiceSettings.Instance.BackgroundSoundRemovalAmount}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.DenoiseAmount = {VoiceSettings.Instance.DenoiseAmount}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.ForwardErrorCorrection = {VoiceSettings.Instance.ForwardErrorCorrection}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.FrameSize = {VoiceSettings.Instance.FrameSize}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.Quality = {VoiceSettings.Instance.Quality}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.VoiceDuckLevel = {VoiceSettings.Instance.VoiceDuckLevel}");
            Plugin.Log.LogInfo($"Pre-patch VoiceSettings.Instance.VadSensitivity = {VoiceSettings.Instance.VadSensitivity}");

            // Acoustic Echo Cancellation requires some extra setup before it can be used. See this tutorial (https://placeholder-software.co.uk/dissonance/docs/Tutorials/Acoustic-Echo-Cancellation.html).
            //configAecDelayAgnostic = Config.Bind("Acoustic Echo Cancellation", "AecDelayAgnostic", defaultAecDelayAgnostic, "Enable Delay Agnostic when using desktop echo cancellation.");
            //configAecExtendedFilter = Config.Bind("Acoustic Echo Cancellation", "AecExtendedFilter", defaultAecExtendedFilter, "Enable Extended Filter when using desktop echo cancellation.");
            //configAecmComfortNoise = Config.Bind("Acoustic Echo Cancellation", "AecmComfortNoise", defaultAecmComfortNoise, "Enable Comfort Noise when using mobile echo cancellation.");
            //configAecmRoutingMode = Config.Bind("Acoustic Echo Cancellation", "AecmRoutingMode", defaultAecmRoutingMode, "Mobile echo cancellation mode.");
            //configAecRefinedAdaptiveFilter = Config.Bind("Acoustic Echo Cancellation", "AecRefinedAdaptiveFilter", defaultAecRefinedAdaptiveFilter, "Enable Refined Adaptive Filter when using desktop echo cancellation.");
            //configAecSuppressionAmount = Config.Bind("Acoustic Echo Cancellation", "AecSuppressionAmount", defaultAecSuppressionAmount, "Change amount of AEC applied to desktop echo cancellation.");
            configBackgroundSoundRemovalEnabled = Config.Bind("Noise Suppression", "BackgroundSoundRemovalEnabled", defaultBackgroundSoundRemovalEnabled, "Enables RNNoise, an ML based background sound removal system. When there is a lot of background sound (e.g. people talking, dogs barking, keyboard clatter, fan noise, loud breathing) this system will remove it, but will distort speech much more than the basic Noise Suppression system. Dissonance can run both noise removal systems at once, which reduces the amount of distortion present even in very noisy environments. It is recommended to enable this system if you are building an application where there is likely to be a lot of environmental noise (e.g. a mobile app, where the user is expected to be on-the-move while talking) or an intense VR game (where the user may be breathing heavily while talking).");
            configBackgroundSoundRemovalAmount = Config.Bind("Noise Suppression", "BackgroundSoundRemovalAmount", defaultBackgroundSoundRemovalAmount, new ConfigDescription("The intensity slider limits the amount of background sound that can be removed and also limits the maximum amount of distortion even in the worst case. Set it higher to cancel more noise.", new AcceptableValueRange<int>(0, 100)));
            configDenoiseAmount = Config.Bind("Noise Suppression", "DenoiseAmount", defaultDenoiseAmount, "Controls how much the audio pre-processor removes noise from the signal. Higher values will remove more noise but may also make speech quieter. Sounds such as people talking in the background are not noise and will not be removed by the noise suppressor. This system removes non-voice sounds such as fans hum, keyboard clatter, or fuzz from a poor quality microphone.");
            configForwardErrorCorrection = Config.Bind("Codec", "ForwardErrorCorrection", defaultForwardErrorCorrection, "Controls if the codec is using Forward Error Correction which improves audio quality when packets are lost. When network conditions are good this makes no difference to network data used. When network conditions are bad this slightly increases the total data used (by about 10%) and massively improves audio quality (it can almost completely mask ~5% packet loss). WARNING: It is very highly recommended to keep FEC enabled. It is a huge quality increase for a very small increase in network data usage.");
            configFrameSize = Config.Bind("Codec", "FrameSize", defaultFrameSize, "Controls how much audio is packed into a single network packet. Smaller frames reduce recording latency but send more packets over the network per second, which consumes more network data and slightly more CPU power. The exact frame size at each setting is: Tiny = 10ms (100 packets/s), Small = 20ms (50 packets/s), Medium = 40ms (25 packets/s), Large = 60ms (16.6 packets/second). WARNING: The smallest option (Tiny) is not suitable for use over the internet or over a wireless network. This option should only be used in very special cases where all clients will be connected to the same wired local area network.");
            configVoiceDuckLevel = Config.Bind("Codec", "VoiceDuckLevel", defaultVoiceDuckLevel, new ConfigDescription("Controls how much received Dissonance audio will be attenuated by when any VoiceBroadcastTrigger is activated (i.e. speech is being transmitted). This can help prevent feedback of recorded audio into the microphone. The AEC system is not perfect - even if you have AEC setup and working it is still worth using audio ducking. The default value configured in Dissonance is a very mild (almost imperceptible) level of audio ducking. Much smaller values can reasonably be used, particularly on mobile platforms or VR headsets where feedback (due to speakers and microphones in close proximity) is a much more common problem.", new AcceptableValueRange<int>(0, 100)));
            configVadSensitivity = Config.Bind("Codec", "VadSensitivity", defaultVadSensitivity, "The voice detector detects speech and activates Voice Broadcast Trigger components configured with Activation Mode: Voice Activation. This settings controls a tradeoff between accuracy (not activating when no one is speaking) and sensitivity (always activating when someone is speaking). A low sensitivity voice detector will not activate when there is non-speech audio (e.g. keyboard clatter), but it sometimes may not activate when there is speech (e.g. quiet speech). A high sensitivity voice detector will activate when there is speech, but it may also activate when there is non-speech audio.");
            configQuality = Config.Bind("Codec", "Quality", defaultQuality, "Controls how many bits-per-second (on average) the audio codec will use to encode audio. Higher bitrates sound better but use more network data and slightly more CPU power. The data rate used by each quality setting is: Low = 1.25 KB/s, Medium = 2.125 KB/s, High = 3 KB/s");

            Application.quitting += Quit;

            VoiceSettings.Instance.BackgroundSoundRemovalEnabled = configBackgroundSoundRemovalEnabled.Value;
            VoiceSettings.Instance.BackgroundSoundRemovalAmount = configBackgroundSoundRemovalAmount.Value / 100f;
            VoiceSettings.Instance.DenoiseAmount = configDenoiseAmount.Value;
            VoiceSettings.Instance.ForwardErrorCorrection = configForwardErrorCorrection.Value;
            VoiceSettings.Instance.FrameSize = configFrameSize.Value;
            VoiceSettings.Instance.Quality = configQuality.Value;
            VoiceSettings.Instance.VoiceDuckLevel = configVoiceDuckLevel.Value / 100f;
            VoiceSettings.Instance.VadSensitivity = configVadSensitivity.Value;
        }

        static void Quit()
        {
            // Settings are saved into PlayerPrefs and become persistent globally for the player for this game (even when playing unmodded), restore to default settings when quitting in case mod gets uninstalled or when playing unmodded.
            Plugin.Log.LogInfo($"Restoring default voice settings.");
            VoiceSettings.Instance.BackgroundSoundRemovalEnabled = defaultBackgroundSoundRemovalEnabled;
            VoiceSettings.Instance.BackgroundSoundRemovalAmount = defaultBackgroundSoundRemovalAmount / 100f;
            VoiceSettings.Instance.DenoiseAmount = defaultDenoiseAmount;
            VoiceSettings.Instance.ForwardErrorCorrection = defaultForwardErrorCorrection;
            VoiceSettings.Instance.FrameSize = defaultFrameSize;
            VoiceSettings.Instance.Quality = defaultQuality;
            VoiceSettings.Instance.VoiceDuckLevel = defaultVoiceDuckLevel / 100f;
            VoiceSettings.Instance.VadSensitivity = defaultVadSensitivity;
        }
    }
}
