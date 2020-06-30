namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class BlockOutputTuning : DynamicAssociationTree
    {
        public string Sort { get; set; }
        public int OutputLimit { get; set; } = 10;
    }
}
