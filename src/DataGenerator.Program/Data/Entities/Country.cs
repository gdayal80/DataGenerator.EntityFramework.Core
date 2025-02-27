namespace DataGenerator.Program.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Country")]
    public class Country
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CountryId { get; set; }
        public string? Name { get; set; }
        public long? SchoolBranchId { get; set; }
        public bool IsDeleted { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedByUserId { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("SchoolBranchId")]
        public virtual SchoolBranch? SchoolBranch { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }
    }
}