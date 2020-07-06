using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Model
{
    public class AssociationEntity : AuditableEntity, IHasOuterId
    {
        [Required]
        [StringLength(128)]
        public string AssociationType { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(128)]
        [Required]
        public string StoreId { get; set; }

        public int Priority { get; set; }

        public string ExpressionTreeSerialized { get; set; }

        [StringLength(128)]
        public string OuterId { get; set; }

        public virtual Association ToModel(Association association)
        {
            if (association == null)
            {
                throw new ArgumentNullException(nameof(association));
            }

            association.Id = Id;
            association.CreatedBy = CreatedBy;
            association.CreatedDate = CreatedDate;
            association.ModifiedBy = ModifiedBy;
            association.ModifiedDate = ModifiedDate;

            association.OuterId = OuterId;
            association.AssociationType = AssociationType;
            association.IsActive = IsActive;
            association.Description = Description;
            association.StartDate = StartDate;
            association.EndDate = EndDate;
            association.Name = Name;
            association.Priority = Priority;
            association.StoreId = StoreId;

            association.ExpressionTree = AbstractTypeFactory<AssociationRuleTree>.TryCreateInstance();
            if (ExpressionTreeSerialized != null)
            {
                association.ExpressionTree = JsonConvert.DeserializeObject<AssociationRuleTree>(ExpressionTreeSerialized, new ConditionJsonConverter());
            }

            return association;
        }

        public virtual AssociationEntity FromModel(Association association, PrimaryKeyResolvingMap pkMap)
        {
            if (association == null)
            {
                throw new ArgumentNullException(nameof(association));
            }

            pkMap.AddPair(association, this);

            Id = association.Id;
            CreatedBy = association.CreatedBy;
            CreatedDate = association.CreatedDate;
            ModifiedBy = association.ModifiedBy;
            ModifiedDate = association.ModifiedDate;

            OuterId = association.OuterId;
            AssociationType = association.AssociationType;
            Name = association.Name;
            Description = association.Description;
            StartDate = association.StartDate;
            EndDate = association.EndDate;
            IsActive = association.IsActive;
            StoreId = association.StoreId;
            Priority = association.Priority;

            if (association.ExpressionTree != null)
            {
                ExpressionTreeSerialized = JsonConvert.SerializeObject(association.ExpressionTree, new ConditionJsonConverter(doNotSerializeAvailCondition: true));
            }

            return this;
        }

        public virtual void Patch(AssociationEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Name = Name;
            target.IsActive = IsActive;
            target.Description = Description;
            target.StoreId = StoreId;
            target.StartDate = StartDate;
            target.EndDate = EndDate;
            target.Priority = Priority;
            target.AssociationType = AssociationType;
            target.ExpressionTreeSerialized = ExpressionTreeSerialized;
        }
    }
}
