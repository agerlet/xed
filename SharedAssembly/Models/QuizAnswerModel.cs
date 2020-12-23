using System;

namespace SharedAssembly.Models
{
    public class QuizAnswerModel
    {
        public string QuizId { get; set; }
        public string StudentId { get; set; }
        public string[] Answers { get; set; }
        public DateTime ArriveAt { get; set; }
        public DateTime? CompleteAt { get; set; }
    }
}