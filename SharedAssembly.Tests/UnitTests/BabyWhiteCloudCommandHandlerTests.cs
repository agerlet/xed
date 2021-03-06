using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Logging.Abstractions;
using SharedAssembly.CommandHandlers;
using SharedAssembly.Repositories;
using Xunit;

namespace SharedAssembly.Tests.UnitTests
{
    public class BabyWhiteCloudCommandHandlerTests
    {
        private IDynamoDBContext _dbContext;
        public BabyWhiteCloudCommandHandlerTests()
        {
            var client = new AmazonDynamoDBClient();
            _dbContext = new DynamoDBContext(client);
        }
        [Fact]
        public async Task Should_persist_BabyWhiteCloudCommand_with_arriveAt()
        {
            // Arrange
            var timestamp = DateTime.UtcNow;
            var repository = new Repository(_dbContext);
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, NullLogger<BabyWhiteCloudCommandHandler>.Instance);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "", "", "", "", "" } };
            
            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);
            
            // Assert
            var answer = (await repository.Query("BabyWhiteCloud", "abc")).Single();
            answer.ArriveAt.Should().BeAfter(timestamp);
        }
        [Fact]
        public async Task Should_persist_BabyWhiteCloudCommand()
        {
            // Arrange
            var repository = new Repository(_dbContext);
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, NullLogger<BabyWhiteCloudCommandHandler>.Instance);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "a", "b", "c", "d", "e" } };
            
            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);
            
            // Assert
            (await repository.Query("BabyWhiteCloud", "abc")).Should().HaveCount(1);
        }
        [Fact]
        public async Task Should_persist_another_BabyWhiteCloudCommand()
        {
            // Arrange
            var repository = new Repository(_dbContext);
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, NullLogger<BabyWhiteCloudCommandHandler>.Instance);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "a", "b", "c", "d", "e" } };
            var quizAnswerCommand2 = new QuizAnswerCommand { StudentId = "def", QuizId = "BabyWhiteCloud", Answers = new List<string> { "a", "b", "c", "d", "e" } };
            
            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand2, CancellationToken.None);
            
            // Assert
            (await repository.Query("BabyWhiteCloud")).Should().HaveCountGreaterOrEqualTo(2);
        }
        [Fact]
        public async Task Should_update_the_same_BabyWhiteCloudCommand()
        {
            // Arrange
            var repository = new Repository(_dbContext);
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, NullLogger<BabyWhiteCloudCommandHandler>.Instance);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "a", "b", "c", "d", "e" } };
            var quizAnswerCommand2 = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "e", "b", "c", "d", "a" } };
            
            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand2, CancellationToken.None);
            
            // Assert
            (await repository.Query("BabyWhiteCloud", "abc")).Should().HaveCount(1);
            (await repository.Query("BabyWhiteCloud", "abc")).Single().Answers.Should().IsSameOrEqualTo(new[] {"e", "b", "c", "d", "a"});
        }
        [Fact]
        public async Task Should_not_throw_exception_when_answers_are_missing_or_empty()
        {
            // Arrange
            var repository = new Repository(_dbContext);
            var logger = NullLogger<BabyWhiteCloudCommandHandler>.Instance;
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, logger);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud" };
            Exception exception = null;

            // Act
            try
            {
                await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);
            }
            catch(Exception ex)
            {
                exception = ex;
            }

            // Assert
            exception.Should().BeNull();
        }
        [Fact]
        public async Task Should_complete_test_when_answers_are_correct()
        {
            // Arrange
            var repository = new Repository(_dbContext);
            var logger = NullLogger<BabyWhiteCloudCommandHandler>.Instance;
            var babyWhiteCloudCommandHandler = new BabyWhiteCloudCommandHandler(repository, logger);
            var quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "雪花", "变成", "甜", "尝一尝", "甜", "凉凉" } };

            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);

            // Assert
            (await repository.Query("BabyWhiteCloud", "abc")).Single().CompleteAt.Should().NotBeNull();
            
            // Arrange
            quizAnswerCommand = new QuizAnswerCommand { StudentId = "abc", QuizId = "BabyWhiteCloud", Answers = new List<string> { "雪花", "变成", "甜", "尝一尝", "甜", "" } };

            // Act
            await babyWhiteCloudCommandHandler.Handle(quizAnswerCommand, CancellationToken.None);

            // Assert
            (await repository.Query("BabyWhiteCloud", "abc")).Single().CompleteAt.Should().BeNull();
        }
    }
}