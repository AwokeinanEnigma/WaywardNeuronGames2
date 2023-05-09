namespace Enigmaware.Weaponry
{
    class EmptyWeapon : BaseWeaponState
    {
        public override int OverheatDrain { get; }
        public override int OverheatPerUse { get; }
        public override int OverheatMaximum { get; }
        public override string Name { get; }

        public override void Primary()
        {
        }

        public override void Secondary()
        {
        }
    }
}