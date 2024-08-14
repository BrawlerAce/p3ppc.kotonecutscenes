using p3ppc.kotonecutscenes.Configuration;
using p3ppc.kotonecutscenes.Template;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using BGME.Framework.Interfaces;
using CriFs.V2.Hook;
using CriFs.V2.Hook.Interfaces;
using BF.File.Emulator;
using BF.File.Emulator.Interfaces;
using PAK.Stream.Emulator;
using PAK.Stream.Emulator.Interfaces;
using p3ppc.kotonecutscenes.Components;
using Reloaded.Memory;
using Reloaded.Memory.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using static p3ppc.kotonecutscenes.Utils;
using System;

namespace p3ppc.kotonecutscenes
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

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

                SigScan("0F BA F0 07 ?? ?? ?? ?? ?? ?? ??", "Pink Loading Card + Icon", address =>
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