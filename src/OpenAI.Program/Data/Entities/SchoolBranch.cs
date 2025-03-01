namespace OpenAI.Program.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SchoolBranch")]
    public class SchoolBranch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SchoolBranchId { get; set; }
        public string? BranchName { get; set; }
        public long? SchoolId { get; set; }
        public bool IsDeleted { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedByUserId { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("SchoolId")]
        public virtual School? School { get; set; }
        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }
    }
}