namespace DataGenerator.Test.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? EmailId { get; set; }
        public string? Password { get; set; }
        public bool IsTempPwd { get; set; }
        public int OTP { get; set; }
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
        public int NoOfUnsuccessfullAttempts { get; set; }
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
