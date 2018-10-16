using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pubquiz.Domain.Models;
using Pubquiz.Logic.Requests;

namespace Pubquiz.Domain.Tests
{
    [TestCategory("Account actions")]
    [TestClass]
    public class AccountTests : InitializedTestBase
    {
        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_RegisterWithCorrectNewTeam_TeamRegistered()
        {
            // arrange
            var command = new RegisterForGameCommand(UnitOfWork, Bus) {TeamName = "Team 4", Code = "JOINME"};

            // act
            var team = command.Execute().Result;
            UnitOfWork.Commit();

            // assert
            Assert.AreEqual("Team 4", team.Name);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_RegisterWithCorrectNewTeam_TeamInGame()
        {
            // arrange
            var command = new RegisterForGameCommand(UnitOfWork, Bus) {TeamName = "Team 4", Code = "JOINME"};

            // act
            var team = command.Execute().Result;
            UnitOfWork.Commit();

            // assert
            var gameCollection = UnitOfWork.GetCollection<Game>();
            var game = gameCollection.GetAsync(team.GameId).Result;
            CollectionAssert.Contains(game.TeamIds, team.Id);
            Assert.AreEqual("Team 4", team.Name);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_UseTeamRecoveryCode_RecoveryCodeAccepted()
        {
            // arrange 
            var firstTeamId = Game.TeamIds[0];
            var firstTeam = UnitOfWork.GetCollection<Team>().GetAsync(firstTeamId).Result;
            var command = new RegisterForGameCommand(UnitOfWork, Bus) {TeamName = "", Code = firstTeam.RecoveryCode};

            // act
            var team = command.Execute().Result;
            UnitOfWork.Commit();

            // assert
            team.RecoveryCode = firstTeam.RecoveryCode;
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_RegisterWithInvalidCode_ThrowsException()
        {
            // arrange
            var command = new RegisterForGameCommand(UnitOfWork, Bus) {TeamName = "Team 4", Code = "INVALIDCODE"};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual("Invalid code.", exception.Message);
            Assert.IsFalse(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_RegisterWithExistingTeamName_ThrowsException()
        {
            // arrange
            var command = new RegisterForGameCommand(UnitOfWork, Bus) {TeamName = "Team 3", Code = "JOINME"};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual("Team name is taken.", exception.Message);
            Assert.IsTrue(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_ChangeTeamNameForValidTeam_TeamNameChanged()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var command = new ChangeTeamNameCommand(UnitOfWork, Bus) {NewName = "Team 1a", TeamId = teamId};

            // act
            var team = command.Execute().Result;
            UnitOfWork.Commit();

            // assert
            Assert.AreEqual("Team 1a", team.Name);
            Assert.AreEqual("Team%201a", team.UserName);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_ChangeTeamNameForInvalidTeam_ThrowsException()
        {
            // arrange
            var teamId = Guid.Empty;
            var command = new ChangeTeamNameCommand(UnitOfWork, Bus) {NewName = "Team 1a", TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual("Invalid team id.", exception.Message);
            Assert.AreEqual(3, exception.ErrorCode);
            Assert.IsFalse(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_ChangeTeamNameToTakenName_ThrowsException()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var command = new ChangeTeamNameCommand(UnitOfWork, Bus) {NewName = "Team 2", TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual("Team name is taken.", exception.Message);
            Assert.AreEqual(2, exception.ErrorCode);
            Assert.IsTrue(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_ChangeTeamMembersForInvalidTeam_ThrowsException()
        {
            // arrange
            var teamId = Guid.Empty;
            var notification = new ChangeTeamMembersNotification(UnitOfWork, Bus)
                {TeamMembers = "a,b,c", TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => notification.Execute()).Result;
            Assert.AreEqual("Invalid team id.", exception.Message);
            Assert.AreEqual(3, exception.ErrorCode);
            Assert.IsFalse(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_ChangeTeamMembersForValidTeam_TeamMembersChanged()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var notification = new ChangeTeamMembersNotification(UnitOfWork, Bus)
                {TeamMembers = "a,b,c", TeamId = teamId};

            // act
            notification.Execute().Wait();
            UnitOfWork.Commit();

            var team = UnitOfWork.GetCollection<Team>().GetAsync(teamId).Result;
            Assert.AreEqual("a,b,c", team.MemberNames);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_DeleteTeamWithInvalidTeamId_ThrowsException()
        {
            // arrange
            var teamId = Guid.Empty;
            var user = Users.First(u => u.UserName == "Quiz master 1");
            var notification = new DeleteTeamNotification(UnitOfWork, Bus) {ActorId = user.Id, TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => notification.Execute()).Result;
            Assert.AreEqual(ErrorCodes.InvalidTeamId, exception.ErrorCode);
            Assert.AreEqual("Invalid team id.", exception.Message);
            Assert.IsFalse(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_DeleteTeamWithInvalidActorId_ThrowsException()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var actorId = Guid.Empty;
            var notification = new DeleteTeamNotification(UnitOfWork, Bus) {ActorId = actorId, TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => notification.Execute()).Result;
            Assert.AreEqual(ErrorCodes.InvalidUserId, exception.ErrorCode);
            Assert.AreEqual("Invalid actor id.", exception.Message);
            Assert.IsTrue(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_DeleteTeamByUnauthorizedQuizMaster_ThrowsException()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var user = Users.First(u => u.UserName == "Quiz master 2");
            var notification = new DeleteTeamNotification(UnitOfWork, Bus) {ActorId = user.Id, TeamId = teamId};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => notification.Execute()).Result;
            Assert.AreEqual(ErrorCodes.QuizMasterUnauthorizedForGame, exception.ErrorCode);
            Assert.AreEqual($"Actor with id {user.Id} is not authorized for game '{Game.Id}'", exception.Message);
            Assert.IsTrue(exception.IsBadRequest);
        }

        [TestCategory("Team registration")]
        [TestMethod]
        public void TestGame_DeleteValidTeamWithValidActorId_TeamDeleted()
        {
            // arrange
            var teamId = Game.TeamIds[0]; // Team 1
            var user = Users.First(u => u.UserName == "Quiz master 1");
            var notification = new DeleteTeamNotification(UnitOfWork, Bus) {ActorId = user.Id, TeamId = teamId};

            // act
            notification.Execute().Wait();
            UnitOfWork.Commit();

            // assert
            Assert.IsNull(UnitOfWork.GetCollection<Team>().GetAsync(teamId).Result);
            CollectionAssert.DoesNotContain(UnitOfWork.GetCollection<Game>().GetAsync(Game.Id).Result.TeamIds,
                teamId);
        }

        [TestCategory("User login")]
        [TestMethod]
        public void TestGame_LoginAdmin_AdminLoggedIn()
        {
            // arrange
            var command = new LoginCommand(UnitOfWork, Bus) {UserName = "Admin", Password = "secret123"};

            // act
            var user = command.Execute().Result;

            // assert
            Assert.AreEqual(UserRole.Admin, user.UserRole);
        }

        [TestCategory("User login")]
        [TestMethod]
        public void TestGame_LoginQuizMaster_QuizMasterLoggedIn()
        {
            // arrange
            var command = new LoginCommand(UnitOfWork, Bus) {UserName = "Quiz master 1", Password = "qm1"};

            // act
            var user = command.Execute().Result;

            // assert
            Assert.AreEqual(UserRole.QuizMaster, user.UserRole);
        }

        [TestCategory("User login")]
        [TestMethod]
        public void TestGame_LoginInvalidPassword_ThrowsException()
        {
            // arrange
            var command = new LoginCommand(UnitOfWork, Bus) {UserName = "Admin", Password = "wrong password"};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual(ErrorCodes.InvalidUserNameOrPassword, exception.ErrorCode);
            Assert.AreEqual("Invalid username or password.", exception.Message);
            Assert.IsTrue(exception.IsBadRequest);
        }

        [TestCategory("User login")]
        [TestMethod]
        public void TestGame_LoginInvalidUserName_ThrowsException()
        {
            // arrange
            var command = new LoginCommand(UnitOfWork, Bus) {UserName = "NonexistentUser", Password = "secret123"};

            // act & assert
            var exception = Assert.ThrowsExceptionAsync<DomainException>(() => command.Execute()).Result;
            Assert.AreEqual(ErrorCodes.InvalidUserNameOrPassword, exception.ErrorCode);
            Assert.AreEqual("Invalid username or password.", exception.Message);
            Assert.IsTrue(exception.IsBadRequest);
        }
    }
}