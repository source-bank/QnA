using System.ComponentModel.DataAnnotations;

namespace QnA.Models.Tables
{
    public class QnABoard
    {
        [Key]
        public int Idx { get; set; }
        public int BRef { get; set; }
        public int BLevel { get; set; }
        public int BStep { get; set; }
        public string BTitle { get; set; }
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public string BContent { get; set; }
        public int HitCount { get; set; }
        public DateTime RegDate { get; set; }
        public Boolean DelDiv { get; set; }
    }
}
