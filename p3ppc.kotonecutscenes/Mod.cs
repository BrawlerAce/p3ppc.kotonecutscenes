using p3ppc.kotonecutscenes.Configuration;
using p3ppc.kotonecutscenes.Template;
using Reloaded.Mod.Interfaces;
using BGME.Framework.Interfaces;
using CriFs.V2.Hook.Interfaces;
using BF.File.Emulator.Interfaces;
using PAK.Stream.Emulator.Interfaces;
using p3ppc.kotonecutscenes.Components;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using static p3ppc.kotonecutscenes.Utils;
using Reloaded.Hooks.Definitions;
using System.Runtime.InteropServices;

namespace p3ppc.kotonecutscenes
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly Reloaded.Hooks.Definitions.IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        /// <summary>
        /// Hooks the title screen, changing its colour
        /// </summary>
        private TitleScreen _titleScreen;

        private IHook<IntroDelegate> _introHook;
        private PlayMoviePlayDelegate _playMovie;
        private nuint _movieThing1;
        private nuint* _movieThing2;

        private int _introCount;


        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;


            // For more information about this template, please see
            // https://reloaded-project.github.io/Reloaded-II/ModTemplate/

            // If you want to implement e.g. unload support in your mod,
            // and some other neat features, override the methods in ModBase.

            // TODO: Implement some mod logic

            Utils.Initialise(_logger, _configuration, _modLoader);
            
            _titleScreen = new TitleScreen(_hooks, _configuration);

            var criFsController = _modLoader.GetController<ICriFsRedirectorApi>();
            if (criFsController == null || !criFsController.TryGetTarget(out var criFsApi))
            {
                _logger.WriteLine($"criFsController returned as null! It's Kotover for p3ppc.kotonecutscenes...", System.Drawing.Color.Red);
                return;
            }

            var BfEmulatorController = _modLoader.GetController<IBfEmulator>();
            if (BfEmulatorController == null || !BfEmulatorController.TryGetTarget(out var _BfEmulator))
            {
                _logger.WriteLine($"BfEmulatorController returned as null! It's Kotover for p3ppc.kotonecutscenes...", System.Drawing.Color.Red);
                return;
            }

            var PakEmulatorController = _modLoader.GetController<IPakEmulator>();
            if (PakEmulatorController == null || !PakEmulatorController.TryGetTarget(out var _PakEmulator))
            {
                _logger.WriteLine($"PakEmulatorController returned as null! It's Kotover for p3ppc.kotonecutscenes...", System.Drawing.Color.Red);
                return;
            }

            var BGMEController = _modLoader.GetController<IBgmeApi>().TryGetTarget(out var bgmeApi);

            var modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);

            Memory memory = Memory.Instance;

            // Opening 1

            if (_configuration.OP1 == true)
            {
                criFsApi.AddProbingPath(Path.Combine(modDir, "Config", "MoonlightDaydream"));
            }

            // Pink Title Screen

            if (_configuration.PinkTitleScreen == true)
            {
                // add pink title screen assets
                _PakEmulator.AddDirectory(Path.Combine(modDir, "Config", "PinkTitleScreen"));

                SigScan("75 ?? F6 83 ?? ?? ?? ?? 02 74 ?? E8 ?? ?? ?? ??", "Fix crashes on title screen", address =>
                {
                    memory.SafeWrite((nuint)address, new byte[] { 0x90, 0x90 });
                });

                Utils.SigScan("0F BA F0 07 ?? ?? ?? ?? ?? ?? ??", "Pink Loading Card + Title Config", 4,
                address =>
                {
                    memory.SafeWrite((nuint)(address + 2), new byte[] { 0xE8 });
                });

                // note that pink background is handled in TitleScreen.cs
            }

            // Good Ending Music

            if (_configuration.GoodEndingMusic == Config.GoodEnding.Original)
            {
                bgmeApi.AddFolder(Path.Combine(modDir, "Config", "MusicConfig", "GoodEnding", "Original"));
            }

            if (_configuration.GoodEndingMusic == Config.GoodEnding.Orchestral)
            {
                bgmeApi.AddFolder(Path.Combine(modDir, "Config", "MusicConfig", "GoodEnding", "Orchestral"));
            }

            if (_configuration.GoodEndingMusic == Config.GoodEnding.Reload)
            {
                bgmeApi.AddFolder(Path.Combine(modDir, "Config", "MusicConfig", "GoodEnding", "Reload"));
            }

            if (_configuration.GoodEndingMusic == Config.GoodEnding.ReloadInstrumental)
            {
                bgmeApi.AddFolder(Path.Combine(modDir, "Config", "MusicConfig", "GoodEnding", "ReloadInstrumental"));
            }

            if (_configuration.GoodEndingMusic == Config.GoodEnding.Movie)
            {
                bgmeApi.AddFolder(Path.Combine(modDir, "Config", "MusicConfig", "GoodEnding", "Movie"));
            }

            // Bad Ending Music

            //if (_configuration.BadEndingMusic == Config.BadEnding.Original)
            //{
            //    criFsApi.AddProbingPath(Path.Combine(modDir, "MusicConfig", "BadEnding", "Original"));
            //}

            //if (_configuration.BadEndingMusic == Config.BadEnding.Reload)
            //{
            //    criFsApi.AddProbingPath(Path.Combine(modDir, "MusicConfig", "BadEnding", "Reload"));
            //}

            //if (_configuration.BadEndingMusic == Config.BadEnding.Mistic)
            //{
            //    criFsApi.AddProbingPath(Path.Combine(modDir, "MusicConfig", "BadEnding", "Mistic"));
            //}

            if (_configuration.BadEndingMusic == Config.BadEnding.Original)
            {
                _BfEmulator.AddDirectory(Path.Combine(modDir, "Config", "MusicConfig", "BadEnding", "Original"));
            }

            if (_configuration.BadEndingMusic == Config.BadEnding.Reload)
            {
                _BfEmulator.AddDirectory(Path.Combine(modDir, "Config", "MusicConfig", "BadEnding", "Reload"));
            }

            if (_configuration.BadEndingMusic == Config.BadEnding.Mistic)
            {
                _BfEmulator.AddDirectory(Path.Combine(modDir, "Config", "MusicConfig", "BadEnding", "Mistic"));
            }

            // FEMC Title Screen, etc assets

            Utils.SigScan("48 89 5C 24 ?? 57 48 83 EC 30 48 8B F9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 5F ?? 48 63 03", "Intro", address =>
            {
                _introHook = _hooks.CreateHook<IntroDelegate>(Intro, address).Activate();
            });

            Utils.SigScan("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 40 49 89 CE 4C 89 CE", "PlayMovie", address =>
            {
                _playMovie = _hooks.CreateWrapper<PlayMoviePlayDelegate>(address, out _);
            });


            Utils.SigScan("48 8B 05 ?? ?? ?? ?? 4C 8D 05 ?? ?? ?? ?? 48 89 44 24 ??", "PlayMovieArgs", address =>
            {
                _movieThing1 = Utils.GetGlobalAddress(address + 10);

                _movieThing2 = (nuint*)Utils.GetGlobalAddress(address + 3);
            });

        }

        private nuint Intro(IntroStruct* introStruct)
        {
            var stateInfo = introStruct->StateInfo;

            if (stateInfo->state == IntroState.MovieStart)
            {
                string currentMovie;
                if (_introCount == 0)
                {
                    currentMovie = "sound/usm/P3OPMV_P3P.usm";
                }
                else if (_introCount == 1)
                {
                    currentMovie = "sound/usm/P3OPMV_P3PB.usm";
                }
                else
                {
                    currentMovie = "sound/usm/P3OPMV_P3PC.usm";
                }

                var taskHandle = _playMovie(introStruct, currentMovie, _movieThing1, 0, 0, *_movieThing2);

                stateInfo->Task = taskHandle;
                _introCount = (_introCount + 1) % 3;
                stateInfo->state = IntroState.MoviePlaying;
                return 0;
            }

            return _introHook.OriginalFunction(introStruct);
        }

        private delegate nuint IntroDelegate(IntroStruct* introStruct);

        private delegate TaskStruct* PlayMoviePlayDelegate(IntroStruct* introStruct, string moviePath, nuint movieThing1, int param4, int param5, nuint movieThing2);

        [StructLayout(LayoutKind.Explicit)]
        private struct IntroStruct
        {
            [FieldOffset(0x48)]
            internal IntroStateStruct* StateInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IntroStateStruct
        {
            [FieldOffset(0)]
            internal IntroState state;

            [FieldOffset(0x10)]
            internal TaskStruct* Task;
        }

        private struct TaskStruct
        {
        }

        private enum IntroState : int
        {
            MovieStart = 4,
            MoviePlaying = 5,
            TitleScreen = 7
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _titleScreen.ConfigChanged(configuration);
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}