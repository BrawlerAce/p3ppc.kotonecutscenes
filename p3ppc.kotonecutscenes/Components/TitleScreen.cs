using p3ppc.kotonecutscenes.Configuration;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using static p3ppc.kotonecutscenes.Utils;

namespace p3ppc.kotonecutscenes.Components;

public class TitleScreen
{
    private static readonly int TITLE_SCREEN_COLOUR = 0x7D0000FF;
    
    private IAsmHook _titleScreenColourHook;
    private Config _config;
    
    public TitleScreen(IReloadedHooks hooks, Config config)
    {
        _config = config;
        SigScan("C7 45 ?? 00 01 25 FF", "Title Screen Colour", address =>
        {
            string[] function =
            {
                "use64",
                $"mov dword [rbp-0x70], {TITLE_SCREEN_COLOUR}"
            };
            _titleScreenColourHook = hooks.CreateAsmHook(function, address, AsmHookBehaviour.ExecuteAfter).Activate();

            if (!config.PinkTitleScreen)
                _titleScreenColourHook.Disable();
        });
    }

    /// <summary>
    /// Call this when the mod's configuration changes so the title screen hook can be enabled/disabled if needed
    /// </summary>
    /// <param name="newConfig">The new configuration</param>
    public void ConfigChanged(Config newConfig)
    {
        if (newConfig.PinkTitleScreen != _config.PinkTitleScreen)
        {
            if(newConfig.PinkTitleScreen)
                _titleScreenColourHook.Enable();
            else
                _titleScreenColourHook.Disable();
        }

        _config = newConfig;
    }
}