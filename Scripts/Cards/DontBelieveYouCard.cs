using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace DontBelieveYou.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public class DontBelieveYouCard : ModCardTemplate
{
    private const int energyCost = -1;
    private const CardType type = CardType.Curse;
    private const CardRarity rarity = CardRarity.Curse;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;

    public override int MaxUpgradeLevel => 0;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://dont-believe-you/images/cards/{GetType().Name}.png"
    );

    // 卡牌基础数值 - 能量减少量
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Unplayable,
        // CardKeyword.Ethereal
    ];

    public DontBelieveYouCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 抽到时的效果逻辑 - 使一名随机队友失去能量
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this)
        {
            // 订阅回合开始事件，在能量获取之后再扣除队友能量
            RitsuLibFramework.SubscribeLifecycleOnce<SideTurnStartedEvent>(e =>
            {
                try
                {
                    var teammates = (from c in CombatState.GetTeammatesOf(base.Owner.Creature)
                                     where c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature
                                     select c.Player).ToList();

                    if (teammates.Count > 0)
                    {
                        var random = new Random();
                        var target = teammates[random.Next(teammates.Count)];
                        PlayerCmd.LoseEnergy(DynamicVars.Energy.IntValue, target);
                    }
                }
                catch (Exception ex)
                {
                    Godot.GD.PrintErr($"DontBelieveYou: Error - {ex}");
                }
            }, false);
        }
    }
}
