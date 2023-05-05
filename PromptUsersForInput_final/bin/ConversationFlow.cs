namespace Microsoft.BotBuilderSamples
{
    public class ConversationFlow
{
        // Identifies the last question asked.
        public enum Question
        {
            Name,
            None, // Our last action did not involve a question.
            TwoPlusTwo, // Our last action did not involve a question.
            OnePlusOne,
            TwoPlusFive,
            FivePlusSix,
            TenPlusOne,
            ElevenPlusTwentyTwo,
            FifteenPlusEighteen,
            TwentyFiveMinusTwentyOne,
            FiftyMinusEleven,
            FiftyFourMinusFortyNine,
            HundredandFourMinusFifteen,
            TwentyPlusSixtyTwoPlusTwentyTwo,
            TenPlusTenPlusTen,

        }

    // The last question asked.
    public Question LastQuestionAsked { get; set; } = Question.None;
}
}
