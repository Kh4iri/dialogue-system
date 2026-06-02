using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;
using TMPro;

namespace Khairi.DialogueSystem
{
    [Serializable]
    public class SetFontRuntimeNode : LinearDialogueRuntimeNode
    {
        public enum FontAssetType
        {
            ImportedFont,
            UnityFontAsset
        }

        public FontAssetType FontType;
        [SerializeReference] public InputPort<Font> SpeakerFont;
        [SerializeReference] public InputPort<Font> DialogueFont;
        [SerializeReference] public InputPort<FontAsset> SpeakerFontAsset;
        [SerializeReference] public InputPort<FontAsset> DialogueFontAsset;

        public SetFontRuntimeNode(InputPort<Font> speakerFont, InputPort<Font> dialogueFont)
        {
            FontType = FontAssetType.ImportedFont;
            SpeakerFont = speakerFont;
            DialogueFont = dialogueFont;
        }

        public SetFontRuntimeNode(InputPort<FontAsset> speakerFontAsset, InputPort<FontAsset> dialogueFontAsset)
        {
            FontType = FontAssetType.UnityFontAsset;
            SpeakerFontAsset = speakerFontAsset;
            DialogueFontAsset = dialogueFontAsset;
        }

        public override Task ExecuteAsync(DialogueBehaviour ctx, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            var view = ctx.DialogueViewPreset;
            switch (FontType)
            {
                case FontAssetType.ImportedFont:
                    view.SetFont(SpeakerFont.GetValue(ctx), DialogueFont.GetValue(ctx));
                    break;
                case FontAssetType.UnityFontAsset:
                    view.SetFont(SpeakerFontAsset.GetValue(ctx), DialogueFontAsset.GetValue(ctx));
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
