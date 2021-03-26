using Graph.Enum;
using Graph.Interfaces;
using Graph.Nodes;
using Graph.Relationships;
using Graph.Relationships.User;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Graph
{
    public class WriteService
    {
        private readonly IGraphClient graphClient;

        public WriteService()//IGraphClient client)
        {
            var username = "neo4j";
            var password = "GxUG-LGN+!dr3KT4";

            // Singleton in production
            graphClient = new GraphClient(new Uri("http://localhost:7474/"), username, password);
            graphClient.ConnectAsync().Wait();
        }

        public async Task Seed()
        {
            await SeedBasicJourney();
        }

        public async Task StartUserOnJourney(User user, UserJourney journey)
        {
            await RelateTwoNodesIfNotReleated(user, new StartedJourney(), journey);
        }

        private async Task SaveAnswer(UserJourney userJourney, UserAnswer answer, INode node)
        {
            // Type checking so Cypher isn't generated with "INode" as the object name
            if (node is UserPath userPath)
            {
                await RelateTwoNodesIfNotReleated(userJourney, answer, userPath);
            }
            else if (node is SystemEvent systemEvent)
            {
                await RelateTwoNodesIfNotReleated(userJourney, answer, systemEvent);
            }
            else if (node is SystemPath systemPath)
            {
                await RelateTwoNodesIfNotReleated(userJourney, answer, systemPath);
            }
            else if (node is Journey journey)
            {
                await RelateTwoNodesIfNotReleated(userJourney, answer, journey);
            }

            return;
        }

        public async Task SaveUserRangeAnswer(UserJourney userJourney, int answer, INode userPath)
        {
            var rangeAnswer = new UserRangeAnswer(answer);
            await SaveAnswer(userJourney, rangeAnswer, userPath);
        }

        public async Task SaveUserBinaryAnswer(UserJourney userJourney, bool answer, INode userPath)
        {
            await SaveAnswer(userJourney, new UserBinaryAnswer(answer), userPath);
        }

        public async Task SaveUserTextAnswer(UserJourney userJourney, string answer, INode userPath)
        {
            var textAnswer = new UserTextAnswer(answer);
            await SaveAnswer(userJourney, textAnswer, userPath);
        }

        public async Task LinkJourneyParticipants(UserJourney employeeJourney, UserJourney managerJourney)
        {
            var employeeRelationship = new BasicRelationship("IS_EMPLOYEE");
            await RelateTwoNodesIfNotReleated(employeeJourney, employeeRelationship, managerJourney);

            var managerRelationship = new BasicRelationship("IS_MANAGER");
            await RelateTwoNodesIfNotReleated(managerJourney, managerRelationship, employeeJourney);
        }

        #region Seeding
        private async Task SeedBasicJourney()
        {
            // Do this bit manaually so you can give them better names
            //try
            //{
            //    await graphClient.Cypher.CreateUniqueConstraint("c:Choice", "c.id").ExecuteWithoutResultsAsync();
            //    await graphClient.Cypher.CreateUniqueConstraint("c:Journey", "c.id").ExecuteWithoutResultsAsync();
            //    await graphClient.Cypher.CreateUniqueConstraint("c:SystemEvent", "c.id").ExecuteWithoutResultsAsync();
            //    await graphClient.Cypher.CreateUniqueConstraint("c:SystemPath", "c.id").ExecuteWithoutResultsAsync();
            //    await graphClient.Cypher.CreateUniqueConstraint("c:UserPath", "c.id").ExecuteWithoutResultsAsync();
            //}
            //catch (Exception ex)
            //{

            //}

            // Create relationship objects
            var answerIsGreaterThan3 = new AnswerIs("GREATER_THAN_3", ChoiceType.Range, new List<int> { 4, 5 });
            var answerIs1or2 = new AnswerIs("1_OR_2", ChoiceType.Range, new List<int> { 1, 2 });
            var answerIs3 = new AnswerIs("3", ChoiceType.Range, new List<int> { 3 });
            var answerIs4or5 = new AnswerIs("4_OR_5", ChoiceType.Range, new List<int> { 4, 5 });
            var answerIsYes = new AnswerIs("YES", ChoiceType.Binary, new List<int> { 1 });
            var answerIsNo = new AnswerIs("NO", ChoiceType.Binary, new List<int> { 0 });
            var answerIsLessThanOrEqualTo3 = new AnswerIs("LESS_THAN_OR_EQUAL_TO_3", ChoiceType.Range, new List<int> { 1, 2, 3 });
            var leadsTo = new LeadsTo();

            var introToKoreroJourney = await CreateJourney("Intro to Korero");
            var conversationSchedule = await CreateJourney("Conversation Scheduling");
            var catchUpSchedule = await CreateJourney("Induction Catch-up Scheduling");
            var negativeResponseOptionalJourney = await CreateJourney("Optional Conversation");
            var journey = await CreateJourney("Nth day since start");

            // Add Employee 1st Node
            var employee1 = await CreateUserPath("How are you finding life at {company} so far?", ChoiceType.Range);

            // Add Manager 1st Node
            var manager1 = await CreateUserPath("{Employee} has just joined the comapny. Schedule catchups now?", ChoiceType.Binary);

            // Relate first nodes to Journey Start
            await RelateTwoNodesIfNotReleated(journey, new BasicRelationship("IS_EMPLOYEE_START"), employee1);
            await RelateTwoNodesIfNotReleated(journey, new BasicRelationship("IS_MANAGER_START"), manager1);

            // Above 3
            var hasUserCompletedIntro = await CreateSystemPath("Has the user completed the intro Journey already?", ChoiceType.Binary);
            var wouldYouLikeToDiscuss = await CreateUserPath("Would you like to discuss this with manager?", ChoiceType.Binary);
            var tellUsWhy = await CreateUserPath("Please tell us why", ChoiceType.None);

            // Answers to "How are you finding life at..."
            await RelateTwoNodesIfNotReleated(employee1, answerIs1or2, tellUsWhy);
            await RelateTwoNodesIfNotReleated(employee1, answerIs3, wouldYouLikeToDiscuss);
            await RelateTwoNodesIfNotReleated(employee1, answerIsGreaterThan3, hasUserCompletedIntro);

            // Tell us why
            await RelateTwoNodesIfNotReleated(tellUsWhy, leadsTo, conversationSchedule);

            var notifyManager = await CreateSystemEvent("Notify Manager");
            var wait5days = await CreateSystemEvent("Wait 5 Days");

            // Answers to "Would you like to discuss?"
            await RelateTwoNodesIfNotReleated(wouldYouLikeToDiscuss, answerIsNo, notifyManager);
            await RelateTwoNodesIfNotReleated(wouldYouLikeToDiscuss, answerIsYes, conversationSchedule);

            // Notify leads to a wait
            await RelateTwoNodesIfNotReleated(notifyManager, leadsTo, wait5days);

            var introChoicePath = await CreateUserPath("Would you like to know more about Korero now?", ChoiceType.Binary);
            // Answers to "Has user completed intro already"
            await RelateTwoNodesIfNotReleated(hasUserCompletedIntro, answerIsNo, introChoicePath);
            await RelateTwoNodesIfNotReleated(hasUserCompletedIntro, answerIsYes, wait5days);


            // Answers to "Would you like to know about Korero?
            await RelateTwoNodesIfNotReleated(introChoicePath, answerIsYes, introToKoreroJourney);
            await RelateTwoNodesIfNotReleated(introChoicePath, answerIsNo, wait5days);

            await RelateTwoNodesIfNotReleated(introToKoreroJourney, leadsTo, wait5days);

            // Manager steps
            var howIsEmployeeDoing = await CreateUserPath("How is employee doing so far?", ChoiceType.Range);

            // Manager answer to "Schedule catchups now?"
            await RelateTwoNodesIfNotReleated(manager1, answerIsYes, catchUpSchedule);
            await RelateTwoNodesIfNotReleated(manager1, answerIsNo, howIsEmployeeDoing);

            // Catch up journey leads to...
            await RelateTwoNodesIfNotReleated(catchUpSchedule, leadsTo, howIsEmployeeDoing);


            var managerPleaseTellusWhy = await CreateUserPath("Please tell us why", ChoiceType.None);
            var managerWouldYouLikeToDiscuss = await CreateUserPath("Please tell us why. Would you like to discuss?", ChoiceType.Binary);
            var notifyEmployee = await CreateSystemEvent("Notify employee");

            // Answers to "How is employee doing?"
            await RelateTwoNodesIfNotReleated(howIsEmployeeDoing, answerIs1or2, managerPleaseTellusWhy);
            await RelateTwoNodesIfNotReleated(howIsEmployeeDoing, answerIs3, managerWouldYouLikeToDiscuss);
            await RelateTwoNodesIfNotReleated(howIsEmployeeDoing, answerIs4or5, notifyEmployee);

            // Tell us why leads to...
            await RelateTwoNodesIfNotReleated(managerPleaseTellusWhy, leadsTo, conversationSchedule);

            // Answers to "Would you like to discuss"
            await RelateTwoNodesIfNotReleated(managerWouldYouLikeToDiscuss, answerIsYes, conversationSchedule);
            await RelateTwoNodesIfNotReleated(managerWouldYouLikeToDiscuss, answerIsNo, notifyEmployee);

            // Notify leads to a wait...
            await RelateTwoNodesIfNotReleated(notifyEmployee, leadsTo, wait5days);


            var employeeLogsOutcomes = await CreateUserPath("Conversation outcomes logged by employee", ChoiceType.None);
            // Next step after the conversation scheduling journey
            await RelateTwoNodesIfNotReleated(conversationSchedule, leadsTo, employeeLogsOutcomes);

            var didConversationHappen = await CreateSystemPath("Conversation happened before expiry?", ChoiceType.Binary);
            var missedConversation = await CreateSystemEvent("Missed Conversation Notification");
            var happenedWait = await CreateSystemEvent("Wait 3 days after the conversation");

            await RelateTwoNodesIfNotReleated(employeeLogsOutcomes, leadsTo, didConversationHappen);

            // System answer to "Did conversation happen"
            await RelateTwoNodesIfNotReleated(didConversationHappen, answerIsYes, happenedWait);
            await RelateTwoNodesIfNotReleated(didConversationHappen, answerIsNo, missedConversation);

            var expiredWait = await CreateSystemEvent("Wait at least 5 days after bad answer");
            await RelateTwoNodesIfNotReleated(missedConversation, leadsTo, expiredWait);

            // Paths diverge here

            // Info from these steps fed into next catchup
            var employeeHowThingsGoingNow = await CreateUserPath("How are things going now?", ChoiceType.None);
            //  v^ These two steps might need to be connected
            var managerHowThingsGoingNow = await CreateUserPath("How are things going now?", ChoiceType.None);
            await RelateTwoNodesIfNotReleated(wait5days, leadsTo, employeeHowThingsGoingNow);
            await RelateTwoNodesIfNotReleated(happenedWait, leadsTo, managerHowThingsGoingNow);
            await RelateTwoNodesIfNotReleated(expiredWait, leadsTo, managerHowThingsGoingNow);

            var second5dayWait = await CreateSystemEvent("Wait 5 days");
            await RelateTwoNodesIfNotReleated(employeeHowThingsGoingNow, leadsTo, second5dayWait);
            await RelateTwoNodesIfNotReleated(managerHowThingsGoingNow, leadsTo, second5dayWait);

            var employeeCompanyExpectations = await CreateUserPath("Rate expectations", ChoiceType.Range);
            await RelateTwoNodesIfNotReleated(second5dayWait, leadsTo, employeeCompanyExpectations);

            var third5dayWait = await CreateSystemEvent("Wait 5 days");
            // Expectations answers..
            await RelateTwoNodesIfNotReleated(employeeCompanyExpectations, answerIsGreaterThan3, third5dayWait);
            await RelateTwoNodesIfNotReleated(employeeCompanyExpectations, answerIsLessThanOrEqualTo3, negativeResponseOptionalJourney);

            // Negative leads to...
            await RelateTwoNodesIfNotReleated(negativeResponseOptionalJourney, leadsTo, third5dayWait);

            // Employee "Are you clear about your role?" etc...
        }


        #region Create Nodes

        public async Task<User> CreateUser(string name)
        {
            var user = new User(name);
            await AddIfNotExist(user);
            return user;
        }

        public async Task<UserJourney> CreateUserJourney()
        {
            var journey = new UserJourney();
            await AddIfNotExist(journey);
            return journey;
        }

        private async Task<Journey> CreateJourney(string descriptioin)
        {
            var journey = new Journey(descriptioin);
            await AddIfNotExist(journey, PathLabel);
            return journey;
        }

        const string PathLabel = "Path";
        private async Task<SystemPath> CreateSystemPath(string description, ChoiceType choiceType)
        {
            var systemPath = new SystemPath(description, choiceType);
            await AddIfNotExist(systemPath, PathLabel);
            return systemPath;
        }

        private async Task<UserPath> CreateUserPath(string description, ChoiceType choiceType)
        {
            var userPath = new UserPath(description, choiceType);
            await AddIfNotExist(userPath, PathLabel);
            return userPath;
        }

        private async Task<SystemEvent> CreateSystemEvent(string description)
        {
            var systemEvent = new SystemEvent(description);
            await AddIfNotExist(systemEvent, PathLabel);
            return systemEvent;
        }

        #endregion


        #endregion

        #region Add Answers



        #endregion

        #region Perform Actions on DB
        private async Task AddIfNotExist<T>(T node, string additionalLabel = null) where T : INode
        {
            try
            {
                // Because we create a unique Guid with each object this will create duplicates if the Seed function is run multiple times
                // Creat multiple node lables (n:Name1:Name2)
                var mergeStatement = $"(n:{typeof(T).Name} {{ id: $id }})";

                if (!string.IsNullOrEmpty(additionalLabel))
                    mergeStatement = $"(n:{typeof(T).Name}:{additionalLabel} {{ id: $id }})";

                await graphClient.Cypher
                    .Merge(mergeStatement)
                    .OnCreate()
                    .Set("n = $node")
                    .WithParams(new
                    {
                        id = node.Id,
                        node
                    }).ExecuteWithoutResultsAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task RelateTwoNodesIfNotReleated<TSource, TRelationship, TTarget>(TSource sourceNode, TRelationship relationship, TTarget targetNode)
            where TSource : INode
            where TRelationship : IRelationship
            where TTarget : INode
        {
            try
            {
                // Temporary variable is a work around for the Cypher function not doing deep traversal when parsing expression queries
                // I've reported it to the developers to be fixed
                var sourceId = sourceNode.Id;
                var targetId = targetNode.Id;

                await graphClient.Cypher
                .Match($"(source:{typeof(TSource).Name})")
                .Match($"(target:{typeof(TTarget).Name})")
                // Note that "source" "target" alias defined in Match query must match your variables below
                .Where((TSource source) => source.Id == sourceId)
                .AndWhere((TTarget target) => target.Id == targetId)
                .Create($"(source)-[:{relationship.Name.ToUpper()} $relParams]->(target)")
                .WithParam("relParams", relationship)
                .ExecuteWithoutResultsAsync();
            }
            catch (Exception ex2)
            {
                throw;
            }
        }
        #endregion
    }
}