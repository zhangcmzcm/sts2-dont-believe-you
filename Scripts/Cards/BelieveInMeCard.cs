using System.Collections.Generic;
using System.Linq;
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
public class BelieveInMeCard : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const bool shouldShowInCardLibrary = true;

    private TargetType _targetType = TargetType.AnyAlly;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://dont-believe-you/images/cards/{GetType().Name}.png"
    );

    // 卡牌基础数值
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(0)
    ];

    public BelieveInMeCard() : base(energyCost, type, rarity, TargetType.AnyAlly, shouldShowInCardLibrary)
    {
    }

    // ✅ Override TargetType 属性
    public override TargetType TargetType => _targetType;

    // 打出时的效果逻辑 - 将队友能量给自己
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.IsUpgraded)
        {
            // 升级后：影响所有队友
            var allies = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
                where c != null && c.IsAlive && c.IsPlayer
                select c;

            foreach (var ally in allies)
            {
                // ✅ 直接传，检查在 StealEnergyFrom 中统一处理
                await StealEnergyFrom(ally.Player);
            }
        }
        else
        {
            // 升级前：影响选中的单个队友
            Player? targetPlayer = cardPlay.Target?.Player;
            // ✅ 直接传，检查在 StealEnergyFrom 中统一处理
            await StealEnergyFrom(targetPlayer);
        }
    }

    // ✅ 统一处理所有 null 检查
    private async Task StealEnergyFrom(Player? targetPlayer)
    {
        if (targetPlayer?.PlayerCombatState == null)
        {
            return;
        }

        int energyLoss = targetPlayer.PlayerCombatState.Energy;
        if (energyLoss > 0)
        {
            await PlayerCmd.SetEnergy(0, targetPlayer);
            await PlayerCmd.GainEnergy(energyLoss, base.Owner);
        }
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        // ✅ 升级时改变 TargetType
        _targetType = TargetType.AllAllies;
    }
}