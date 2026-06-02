using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Khairi.DialogueSystem.Editor
{
    [Serializable]
    public class SetFontNode : LinearDialogueNode
    {
        public const string FontTypeOptionName = "FontType";
        public const string SpeakerFontInputName = "SpeakerFont";
        public const string DialogueFontInputName = "DialogueFont";

        protected override void OnDefineOptions(IOptionDefinitionContext ctx)
        {
            ctx.AddOption<SetFontRuntimeNode.FontAssetType>(FontTypeOptionName).Build();
        }

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            AddInputOutputFlowPorts(ctx);
            GetNodeOptionByName(FontTypeOptionName).TryGetValue(out SetFontRuntimeNode.FontAssetType fontType);

            switch (fontType)
            {
                case SetFontRuntimeNode.FontAssetType.ImportedFont:
                    ctx.AddInputPort<Font>(SpeakerFontInputName).Build();
                    ctx.AddInputPort<Font>(DialogueFontInputName).Build();
                    break;
                case SetFontRuntimeNode.FontAssetType.UnityFontAsset:
                    ctx.AddInputPort<FontAsset>(SpeakerFontInputName).Build();
                    ctx.AddInputPort<FontAsset>(DialogueFontInputName).Build();
                    break;
            }
        }

        public override DialogueRuntimeNode CreateRuntimeNode()
        {
            GetNodeOptionByName(FontTypeOptionName).TryGetValue(out SetFontRuntimeNode.FontAssetType fontType);

            return fontType switch
            {
                SetFontRuntimeNode.FontAssetType.ImportedFont
                    => new SetFontRuntimeNode(GetInputPortByName(SpeakerFontInputName).ToInputPort<Font>(),
                                              GetInputPortByName(DialogueFontInputName).ToInputPort<Font>()),
                
                SetFontRuntimeNode.FontAssetType.UnityFontAsset
                    => new SetFontRuntimeNode(GetInputPortByName(SpeakerFontInputName).ToInputPort<FontAsset>(),
                                              GetInputPortByName(DialogueFontInputName).ToInputPort<FontAsset>()),

                _ => null
            };
        }
    }
}
