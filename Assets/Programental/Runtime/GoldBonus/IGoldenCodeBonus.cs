namespace Programental
{
    public interface IGoldenCodeBonus
    {
        string BonusId { get; }
        BonusInfo Apply();
        void Revert();
    }
}
