using System;
using System.Collections.ObjectModel;

namespace Mislint.Core
{
    public class Theme
    {
        public const string theme = """
               {
                    id: '970ae8ed-e240-487f-916a-7dae62a206b0',
                    base: 'dark',
                    desc: '',
                    name: 'AppleDark@GrapeAppleVer',
                    props: {
            	        X2: ':darken<2<@panel',
            	        X3: 'rgba(255, 255, 255, 0.05)',
            	        X4: 'rgba(255, 255, 255, 0.1)',
            	        X5: 'rgba(255, 255, 255, 0.05)',
            	        X6: 'rgba(255, 255, 255, 0.15)',
            	        X7: 'rgba(255, 255, 255, 0.05)',
            	        X8: ':lighten<5<@accent',
            	        X9: ':darken<5<@accent',
            	        bg: 'rgba(25, 25, 25, 0.75)',
            	        fg: '#dadada',
            	        X10: ':alpha<0.4<@accent',
            	        X11: 'rgba(0, 0, 0, 0.3)',
            	        X12: 'rgba(255, 255, 255, 0.1)',
            	        X13: 'rgba(255, 255, 255, 0.15)',
            	        X14: ':alpha<0.5<@navBg',
            	        X15: ':alpha<0<@panel',
            	        X16: ':alpha<0.7<@panel',
            	        X17: ':alpha<0.8<@bg',
            	        cwBg: '#687390',
            	        cwFg: '#393f4f',
            	        link: '#76118d',
            	        warn: '#ecb637',
            	        badge: '#31b1ce',
            	        error: '#ec4137',
            	        focus: ':alpha<0.3<@accent',
            	        navBg: '@panel',
            	        navFg: '@fg',
            	        panel: ':lighten<3<@bg',
            	        popup: ':lighten<3<@panel',
            	        accent: '#f0587c',
            	        header: ':alpha<0.7<@panel',
            	        infoBg: '#253142',
            	        infoFg: '#fff',
            	        renote: '#229e82',
            	        shadow: 'rgba(0, 0, 0, 0.3)',
            	        divider: 'rgba(255, 255, 255, 0.1)',
            	        hashtag: '#445621',
            	        mention: '@accent',
            	        modalBg: 'rgba(0, 0, 0, 0.5)',
            	        success: '#86b300',
            	        buttonBg: 'rgba(255, 255, 255, 0.05)',
            	        switchBg: 'rgba(255, 255, 255, 0.15)',
            	        acrylicBg: ':alpha<0.5<@bg',
            	        cwHoverBg: '#707b97',
            	        indicator: '@accent',
            	        mentionMe: '@mention',
            	        messageBg: '@bg',
            	        navActive: '@accent',
            	        accentedBg: ':alpha<0.15<@accent',
            	        codeNumber: '#cfff9e',
            	        codeString: '#ffb675',
            	        fgOnAccent: '#fff',
            	        infoWarnBg: '#42321c',
            	        infoWarnFg: '#ffbd3e',
            	        navHoverFg: ':lighten<17<@fg',
            	        swutchOnBg: '@accentedBg',
            	        swutchOnFg: '@accent',
            	        codeBoolean: '#c59eff',
            	        dateLabelFg: '@fg',
            	        deckDivider: '#000',
            	        inputBorder: 'rgba(255, 255, 255, 0.1)',
            	        panelBorder: '" solid 1px var(--divider)',
            	        swutchOffBg: 'rgba(255, 255, 255, 0.1)',
            	        swutchOffFg: '@fg',
            	        accentDarken: ':darken<10<@accent',
            	        acrylicPanel: ':alpha<0.5<@panel',
            	        navIndicator: '@indicator',
            	        windowHeader: ':alpha<0.85<@panel',
            	        accentLighten: ':lighten<10<@accent',
            	        buttonHoverBg: 'rgba(255, 255, 255, 0.1)',
            	        driveFolderBg: ':alpha<0.3<@accent',
            	        fgHighlighted: ':lighten<3<@fg',
            	        fgTransparent: ':alpha<0.5<@fg',
            	        panelHeaderBg: ':lighten<3<@panel',
            	        panelHeaderFg: '@fg',
            	        buttonGradateA: '@accent',
            	        buttonGradateB: ':hue<20<@accent',
            	        htmlThemeColor: '@bg',
            	        panelHighlight: ':lighten<3<@panel',
            	        listItemHoverBg: 'rgba(255, 255, 255, 0.03)',
            	        scrollbarHandle: 'rgba(255, 255, 255, 0.2)',
            	        inputBorderHover: 'rgba(255, 255, 255, 0.2)',
            	        wallpaperOverlay: 'rgba(0, 0, 0, 0.5)',
            	        fgTransparentWeak: ':alpha<0.75<@fg',
            	        panelHeaderDivider: 'rgba(0, 0, 0, 0)',
            	        scrollbarHandleHover: 'rgba(255, 255, 255, 0.4)',
                    },
                    author: '@grapeapple@misskey.04.si',
               }
            """;
    }

    public class GlobalLock
    {
        public static GlobalLock Instance { get; } = new GlobalLock();
        public event EventHandler Unlocked;
        private readonly ObservableCollection<int> lockItems = new();
        public ObservableCollection<int> LockItems
        {
            get => lockItems;
        }
        public bool Lock
        {
            get => lockItems.Count != 0;
        }
        public GlobalLock()
        {
            this.lockItems.CollectionChanged += (sender, e) =>
            {
                if (lockItems.Count == 0)
                {
                    this.Unlocked?.Invoke(this, null);
                }
            };
        }
    }
}
