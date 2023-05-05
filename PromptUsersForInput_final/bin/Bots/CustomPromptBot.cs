// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text.Number;

namespace Microsoft.BotBuilderSamples
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    public class CustomPromptBot : ActivityHandler 
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        
        public CustomPromptBot(ConversationState conversationState, UserState userState)
        {
            _conversationState = conversationState;
            _userState = userState;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var conversationStateAccessors = _conversationState.CreateProperty<ConversationFlow>(nameof(ConversationFlow));
            var flow = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationFlow(), cancellationToken);

            var userStateAccessors = _userState.CreateProperty<UserProfile>(nameof(UserProfile));
            var profile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile(), cancellationToken);

            await FillOutUserProfileAsync(flow, profile, turnContext, cancellationToken);

            // Save changes.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private static async Task FillOutUserProfileAsync(ConversationFlow flow, UserProfile profile, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var input = turnContext.Activity.Text?.Trim();
            string message;

            switch (flow.LastQuestionAsked)
            {
                case ConversationFlow.Question.None:
                    await turnContext.SendActivityAsync("Let's get started. What is your name?", null, null, cancellationToken);
                    flow.LastQuestionAsked = ConversationFlow.Question.Name;
                    break;
                case ConversationFlow.Question.Name:
                    if (ValidateName(input, out var name, out message))
                    {
                        profile.Name = name;
                        await turnContext.SendActivityAsync($"Hi {profile.Name}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 1 + 1 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.OnePlusOne;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.OnePlusOne:
                    if (ValidateOnePlusOne(input, out var inputVal, out message))
                    {
                        profile.OnePlusOne = inputVal;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.OnePlusOne}. ", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 2 + 5 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.TwoPlusFive;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.TwoPlusFive:
                    if (ValidateTwoPlusFive(input, out var inputValTwoPlusFive, out message))
                    {
                        profile.TwoPlusFive = inputValTwoPlusFive;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.TwoPlusFive}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 5 + 6 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.FivePlusSix;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.FivePlusSix:
                    if (ValidateFivePlusSix(input, out var inputValFivePlusSix, out message))
                    {
                        profile.FivePlusSix = inputValFivePlusSix;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.FivePlusSix}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 10 + 1 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.TenPlusOne;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.TenPlusOne:
                    if (ValidateTenPlusOne(input, out var inputValTenPlusOne, out message))
                    {
                        profile.TenPlusOne = inputValTenPlusOne;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.TenPlusOne}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 11 + 22 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.ElevenPlusTwentyTwo;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.ElevenPlusTwentyTwo:
                    if (ValidateElevenPlusTwentyTwo(input, out var inputValElevenPlusTwentyTwo, out message))
                    {
                        profile.ElevenPlusTwentyTwo = inputValElevenPlusTwentyTwo;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.ElevenPlusTwentyTwo}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 15 + 18 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.FifteenPlusEighteen;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.FifteenPlusEighteen:
                    if (ValidateFifteenPlusEighteen(input, out var inputValFifteenPlusEighteen, out message))
                    {
                        profile.FifteenPlusEighteen = inputValFifteenPlusEighteen;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.FifteenPlusEighteen}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 25 - 21 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.TwentyFiveMinusTwentyOne;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.TwentyFiveMinusTwentyOne:
                    if (ValidateTwentyFiveMinusTwentyOne(input, out var inputValTwentyFiveMinusTwentyOne, out message))
                    {
                        profile.TwentyFiveMinusTwentyOne = inputValTwentyFiveMinusTwentyOne;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.TwentyFiveMinusTwentyOne}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 50 - 11 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.FiftyMinusEleven;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.FiftyMinusEleven:
                    if (ValidateFiftyMinusEleven(input, out var inputValFiftyMinusEleven, out message))
                    {
                        profile.FiftyMinusEleven = inputValFiftyMinusEleven;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.FiftyMinusEleven}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 54 - 49 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.FiftyFourMinusFortyNine;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.FiftyFourMinusFortyNine:
                    if (ValidateFiftyFourMinusFortyNine(input, out var inputValFiftyFourMinusFortyNine, out message))
                    {
                        profile.FiftyFourMinusFortyNine = inputValFiftyFourMinusFortyNine;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.FiftyFourMinusFortyNine}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 104 - 15 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.HundredandFourMinusFifteen;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.HundredandFourMinusFifteen:
                    if (ValidateHundredandFourMinusFifteen(input, out var inputValHundredandFourMinusFifteen, out message))
                    {
                        profile.HundredandFourMinusFifteen = inputValHundredandFourMinusFifteen;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.HundredandFourMinusFifteen}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 20 + 62 + 22 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.TwentyPlusSixtyTwoPlusTwentyTwo;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.TwentyPlusSixtyTwoPlusTwentyTwo:
                    if (ValidateTwentyPlusSixtyTwoPlusTwentyTwo(input, out var inputValTwentyPlusSixtyTwoPlusTwentyTwo, out message))
                    {
                        profile.TwentyPlusSixtyTwoPlusTwentyTwo = inputValTwentyPlusSixtyTwoPlusTwentyTwo;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.TwentyPlusSixtyTwoPlusTwentyTwo}.", null, null, cancellationToken);
                        await turnContext.SendActivityAsync("What is 10 + 10 + 10 ?", null, null, cancellationToken);
                        flow.LastQuestionAsked = ConversationFlow.Question.TenPlusTenPlusTen;
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
                case ConversationFlow.Question.TenPlusTenPlusTen:
                    if (ValidateTenPlusTenPlusTen(input, out var inputValTenPlusTenPlusTen, out message))
                    {
                        profile.TenPlusTenPlusTen = inputValTenPlusTenPlusTen;
                        await turnContext.SendActivityAsync($"I have the entered values as {profile.TenPlusTenPlusTen}.");
                        await turnContext.SendActivityAsync($"Thanks for completing the game {profile.Name}.");
                        await turnContext.SendActivityAsync($"Type anything to run the bot again.");
                        flow.LastQuestionAsked = ConversationFlow.Question.None;
                        profile = new UserProfile();
                        break;
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(message ?? "I'm sorry, I didn't understand that.", null, null, cancellationToken);
                        break;
                    }
            }
        }

        private static bool ValidateName(string input, out string name, out string message)
        {
            name = null;
            message = null;

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Please enter a name that contains at least one character.";
            }
            else
            {
                name = input.Trim();
            }

            return message is null;
        }
        private static bool ValidateOnePlusOne(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 2)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateTwoPlusFive(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 7)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateTenPlusOne(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 11)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateTenPlusTenPlusTen(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 30)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateTwentyPlusSixtyTwoPlusTwentyTwo(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 104)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateHundredandFourMinusFifteen(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 89)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateFiftyFourMinusFortyNine(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 5)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateFiftyMinusEleven(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 39)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateTwentyFiveMinusTwentyOne(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 4)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateFifteenPlusEighteen(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 33)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateElevenPlusTwentyTwo(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 33)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateFivePlusSix(string input, out int inputVal, out string message)
        {
            inputVal = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        inputVal = Convert.ToInt32(value);
                        if (inputVal == 11)
                        {
                            return true;
                        }
                    }
                }

                message = "Your input answer is wrong, Please enter the corrct answer ";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an correct value. Please enter correct value";
            }

            return message is null;
        }
        private static bool ValidateAge(string input, out int age, out string message)
        {
            age = 0;
            message = null;

            // Try to recognize the input as a number. This works for responses such as "twelve" as well as "12".
            try
            {
                // Attempt to convert the Recognizer result to an integer. This works for "a dozen", "twelve", "12", and so on.
                // The recognizer returns a list of potential recognition results, if any.

                var results = NumberRecognizer.RecognizeNumber(input, Culture.English);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "value" entry contains the processed string.
                    if (result.Resolution.TryGetValue("value", out var value))
                    {
                        age = Convert.ToInt32(value);
                        if (age >= 18 && age <= 120)
                        {
                            return true;
                        }
                    }
                }

                message = "Please enter an age between 18 and 120.";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an age. Please enter an age between 18 and 120.";
            }

            return message is null;
        }

        private static bool ValidateDate(string input, out string date, out string message)
        {
            date = null;
            message = null;

            // Try to recognize the input as a date-time. This works for responses such as "11/14/2018", "9pm", "tomorrow", "Sunday at 5pm", and so on.
            // The recognizer returns a list of potential recognition results, if any.
            try
            {
                var results = DateTimeRecognizer.RecognizeDateTime(input, Culture.English);

                // Check whether any of the recognized date-times are appropriate,
                // and if so, return the first appropriate date-time. We're checking for a value at least an hour in the future.
                var earliest = DateTime.Now.AddHours(1.0);

                foreach (var result in results)
                {
                    // The result resolution is a dictionary, where the "values" entry contains the processed input.
                    var resolutions = result.Resolution["values"] as List<Dictionary<string, string>>;

                    foreach (var resolution in resolutions)
                    {
                        // The processed input contains a "value" entry if it is a date-time value, or "start" and
                        // "end" entries if it is a date-time range.
                        if (resolution.TryGetValue("value", out var dateString)
                            || resolution.TryGetValue("start", out dateString))
                        {
                            if (DateTime.TryParse(dateString, out var candidate)
                                && earliest < candidate)
                            {
                                date = candidate.ToShortDateString();
                                return true;
                            }
                        }
                    }
                }

                message = "I'm sorry, please enter a date at least an hour out.";
            }
            catch
            {
                message = "I'm sorry, I could not interpret that as an appropriate date. Please enter a date at least an hour out.";
            }

            return false;
        }
    }
}
