namespace DataGenerator.Test.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Address")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AddressId { get; set; }
        public string? Name { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string? PinCode { get; set; }
        public long? CityId { get; set; }
        public long? AddressTypeId { get; set; }
        public bool IsDeleted { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedByUserId { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }
        [ForeignKey("AddressTypeId")]
        public virtual AddressType? AddressType { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }
    }
}