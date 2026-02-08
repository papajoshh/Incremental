namespace Programental
{
    public interface IBonusFeedback
    {
        void ShowBonus(BonusInfo info);
        void ExtendBonus(string bonusId, float addedDuration);
    }
}
