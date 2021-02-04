using Graph;
using Graph.Enum;
using Graph.Interfaces;
using Graph.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagerConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World!");

                var writeService = new WriteService();
                //writeService.Seed().Wait();

                var readService = new ReadService();
                
                BeginJourney(readService, writeService);
                Console.WriteLine("Done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }
        }

        public static string GenerateName()
        {
            Random r = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[r.Next(consonants.Length)].ToUpper();
            Name += vowels[r.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.

            var len = r.Next(3, 10);
            while (b < len)
            {
                Name += consonants[r.Next(consonants.Length)];
                b++;
                Name += vowels[r.Next(vowels.Length)];
                b++;
            }

            return Name;
        }

        private class JourneyDTO
        {
            public UserJourney EmployeeJourney { get; set; }
            public UserJourney ManagerJourney { get; set; }
        }
        private static JourneyDTO CreateJourney(WriteService writeService)
        {
            var currentEmployee = writeService.CreateUser(GenerateName()).Result;
            var currentManager = writeService.CreateUser(GenerateName()).Result;

            var employeeJourney = writeService.CreateUserJourney().Result;
            var managerJourney = writeService.CreateUserJourney().Result;

            writeService.LinkJourneyParticipants(employeeJourney, managerJourney).Wait();

            writeService.StartUserOnJourney(currentEmployee, employeeJourney).Wait();
            writeService.StartUserOnJourney(currentManager, managerJourney).Wait();

            return new JourneyDTO
            {
                EmployeeJourney = employeeJourney,
                ManagerJourney = managerJourney
            };
        }

        private static void BeginJourney(ReadService readService, WriteService writeService)
        {
            var journey = CreateJourney(writeService);

            Console.WriteLine("START EMPLOYEE JOURNEY");
            StartUserJourney(writeService, readService, journey.EmployeeJourney, readService.GetFirstNode(false).Result);

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("START MANAGER JOURNEY");
            StartUserJourney(writeService, readService, journey.ManagerJourney, readService.GetFirstNode(true).Result);
        }

        private static void StartUserJourney(WriteService writeService, ReadService readService, UserJourney employeeJourney, UserPath start)
        {
            DisplayNode(start, out bool askForAnswer);

            INode nextNode = start;
            while (true)
            {
                var findNextNodeResult = readService.GetNextNodeResultAsync(nextNode).Result;

                if (findNextNodeResult == null || !findNextNodeResult.NextNodes.Any())
                {
                    Console.WriteLine("No more nodes");
                    break;
                }

                if (askForAnswer)
                {
                    var choiceType = ChoiceType.None;
                    if (nextNode is UserPath userPath)
                    {
                        choiceType = userPath.ChoiceType;
                    }
                    else if (nextNode is SystemPath sysPath)
                    {
                        choiceType = sysPath.ChoiceType;
                    }

                    var userInput = AskForUserInput(findNextNodeResult.NextNodes.SelectMany(n => n.AcceptableAnswers), choiceType);

                    switch (choiceType)
                    {
                        // Text answers always have a single path to next node
                        case ChoiceType.None:
                            {
                                writeService.SaveUserTextAnswer(employeeJourney, userInput, nextNode).Wait();
                                nextNode = findNextNodeResult.NextNodes.Single().Node;
                                break;
                            }
                        // Multiple routes
                        case ChoiceType.Range:
                            {
                                var intAnswer = Convert.ToInt32(userInput);
                                writeService.SaveUserRangeAnswer(employeeJourney, intAnswer, nextNode).Wait();

                                nextNode = findNextNodeResult.NextNodes.Single(n => n.AcceptableAnswers.Contains(intAnswer)).Node;
                                break;
                            }
                        // Two routes
                        case ChoiceType.Binary:
                            {
                                var intAnswer = Convert.ToInt32(userInput);
                                var boolanswer = Convert.ToBoolean(intAnswer);
                                writeService.SaveUserBinaryAnswer(employeeJourney, boolanswer, nextNode).Wait();

                                nextNode = findNextNodeResult.NextNodes.Single(n => n.AcceptableAnswers.Contains(intAnswer)).Node;
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    // No input needed so route has single path (e.g. Wait 5 days)
                    nextNode = findNextNodeResult.NextNodes.Single().Node;
                }

                DisplayNode(nextNode, out askForAnswer);
            }
        }

        private static string AskForUserInput(IEnumerable<int> options, ChoiceType choiceType)
        {
            DisplayAcceptableAnswers(options, choiceType);

            return Console.ReadLine();
        }

        private static void DisplayAcceptableAnswers(IEnumerable<int> options, ChoiceType choiceType)
        {
            if (options != null && options.Any())
            {
                if (choiceType == ChoiceType.Binary)
                {
                    Console.WriteLine("Input answer");
                    Console.WriteLine("1 = true");
                    Console.WriteLine("0 = false");
                }
                else
                {
                    Console.WriteLine($"Input your answer {options.Min()}-{options.Max()}");
                }
            }
            else
            {
                Console.WriteLine("Input your answer");
            }
        }

        private static void DisplayNode(INode node, out bool askForAnswer)
        {
            askForAnswer = false;

            if (node is UserPath userPath)
            {
                Console.WriteLine(userPath.Text);
                askForAnswer = true;
            }
            else if (node is SystemEvent systemEvent)
            {
                Console.WriteLine($" -- {systemEvent.Description} --");
            }
            else if (node is SystemPath systemPath)
            {
                Console.WriteLine(systemPath.Description);
                askForAnswer = true;
            }
            else if (node is Journey journey)
            {
                Console.WriteLine($" -- Other Journey takes place - {journey.Description} --");
            }
        }
    }
}
