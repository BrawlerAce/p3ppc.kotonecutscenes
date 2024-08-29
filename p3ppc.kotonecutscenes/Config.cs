using p3ppc.kotonecutscenes.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;
using System.ComponentModel;

namespace p3ppc.kotonecutscenes.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.
    
            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs
    
            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [Category("Title Screen")]
        [DisplayName("FEMC Title Screen")]
        [Description("Makes the title screen FEMC themed with pink assets instead\nof the default colors, as well as adding new Kotone art.")]
        [DefaultValue(true)]
        public bool PinkTitleScreen { get; set; } = true;
        
        [Category("Credits Music")]
        [DisplayName("Good Ending Theme")]
        [Description("Select music to play during the good ending.\n\nOriginal: Memories of You\nReload: Memories of You -Reload-\nReloadInstrumental: Memories of You -Reload Instrumental-\nMovie: Memories of You (Remix) (Persona 3 the Movie)\nOrchestra: Memories of You ~Orchestra ver.~")]
        [DefaultValue(GoodEnding.Original)]
        public GoodEnding GoodEndingMusic { get; set; } = GoodEnding.Original;

        public enum GoodEnding
        {
            Original,
            Reload,
            ReloadInstrumental,
            Movie,
            Orchestral
        }

        [Category("Credits Music")]
        [DisplayName("Bad Ending Theme")]
        [Description("Select music to play during the bad ending.\n\nOriginal: Plays the same Memories of You version as in the good ending.\nReload: Mistic -Reload-\nMistic: Mistic (original)")]
        [DefaultValue(BadEnding.Original)]
        public BadEnding BadEndingMusic { get; set; } = BadEnding.Original;

        public enum BadEnding
        {
            Original,
            Reload,
            Mistic
        }
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
