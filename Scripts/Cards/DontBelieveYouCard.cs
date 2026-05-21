using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace DontBelieveYou.Scripts.Cards;

[RegisterCard(typeof(ColorlessCardPool))]
public class DontBelieveYouCard : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://dont-believe-you/images/cards/{GetType().Name}.png"
    );

    // 卡牌基础数值 - 能量减少量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2)
    ];

    public DontBelieveYouCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑 - 使队友失去费用
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获取能量减少量
        int energyLoss = DynamicVars.Energy.IntValue;

        // 获取目标玩家（队友）
        Player? targetPlayer = cardPlay.Target?.Player;

        // 如果目标是玩家，使其失去能量
        if (targetPlayer != null)
        {
            await PlayerCmd.LoseEnergy(energyLoss, targetPlayer);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        // 升级后能量减少量从2变为3
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
