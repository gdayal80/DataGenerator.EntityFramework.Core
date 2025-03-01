namespace OpenAI.Program.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("School")]
    public class School
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactNumber { get; set; }
        public string? ContactEmailId { get; set; }
        public bool IsDeleted { get; set; }
        public long? CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? UpdatedByUserId { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User? CreatedBy { get; set; }
        [ForeignKey("UpdatedByUserId")]
        public virtual User? UpdatedBy { get; set; }
    }
}