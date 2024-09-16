using Verse;

namespace RainUtils.LocalArmor;

public interface ILocalArmorCallback
{
    public void LocalArmorCallback(float preArmorDamage, float postArmorDamage, float armorPen, DamageDef preArmorDef, 
        DamageDef postArmorDef, Pawn pawn, bool metalArmor, BodyPartRecord part);
}